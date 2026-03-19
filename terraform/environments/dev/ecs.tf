resource "aws_ecs_cluster" "default" {
  name = "${local.app_name}-ecs-cluster"
}

resource "aws_cloudwatch_log_group" "ecs_logs_api" {
  name              = "/ecs/${local.app_name}-api"
  retention_in_days = 30
}

resource "aws_cloudwatch_log_group" "ecs_logs_cms" {
  name              = "/ecs/${local.app_name}-cms"
  retention_in_days = 30
}

data "aws_iam_policy_document" "ecs_task_execution" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "ecs_task_execution" {
  name               = "${local.app_name}-ecs-task-execution-role"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_execution.json
}

data "aws_iam_policy_document" "ecs_task_role" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "ecs_task_role" {
  name               = "${local.app_name}-ecs-task-role"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_role.json
}

data "aws_iam_policy_document" "ecs_task_execution_policy" {
  version = "2012-10-17"

  statement {
    effect = "Allow"
    actions = [
      "ecr:GetAuthorizationToken",
      "ecr:BatchCheckLayerAvailability",
      "ecr:GetDownloadUrlForLayer",
      "ecr:BatchGetImage"
    ]
    resources = ["*"]
  }

  statement {
    effect = "Allow"
    actions = [
      "logs:CreateLogGroup",
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]
    resources = [
      "arn:aws:logs:${local.aws.region}:${data.aws_caller_identity.current.account_id}:log-group:/ecs/*"
    ]
  }

  statement {
    effect = "Allow"
    actions = [
      "secretsmanager:GetSecretValue"
    ]
    resources = [
      aws_secretsmanager_secret.default.arn,
      aws_secretsmanager_secret.cms.arn
    ]
  }
}

resource "aws_iam_policy" "ecs_task_execution_policy" {
  name        = "${local.app_name}-ecs-task-execution-policy"
  description = "Policy for ECS task execution role"
  policy      = data.aws_iam_policy_document.ecs_task_execution_policy.json
}

# Policy for ECS Task Role (application permissions)
data "aws_iam_policy_document" "ecs_task_policy" {
  version = "2012-10-17"

  # S3 permissions
  statement {
    effect = "Allow"
    actions = [
      "s3:GetObject",
      "s3:PutObject",
      "s3:ListBucket"
    ]
    resources = [
      "arn:aws:s3:::${aws_s3_bucket.default.bucket}",
      "arn:aws:s3:::${aws_s3_bucket.default.bucket}/*"
    ]
  }

  # Secrets Manager permissions for application
  statement {
    effect = "Allow"
    actions = [
      "secretsmanager:GetSecretValue",
      "secretsmanager:DescribeSecret"
    ]
    resources = [
      "arn:aws:secretsmanager:${local.aws.region}:${data.aws_caller_identity.current.account_id}:secret:${local.app_name}-*"
    ]
  }

  # Cognito permissions (if needed)
  statement {
    effect = "Allow"
    actions = [
      "cognito-idp:AdminGetUser",
      "cognito-idp:AdminCreateUser",
      "cognito-idp:AdminSetUserPassword"
    ]
    resources = ["*"]
  }

  # Lambda permissions
  statement {
    effect = "Allow"
    actions = [
      "lambda:InvokeFunction"
    ]
    resources = [
      "arn:aws:lambda:${local.aws.region}:${data.aws_caller_identity.current.account_id}:function:${local.app_name}-*"
    ]
  }
}

resource "aws_iam_policy" "ecs_task_policy" {
  name        = "${local.app_name}-ecs-task-policy"
  description = "Policy for ECS task role"
  policy      = data.aws_iam_policy_document.ecs_task_policy.json
}

# Attach policies to Task Execution Role
resource "aws_iam_role_policy_attachment" "ecs_task_execution_role_policy" {
  role       = aws_iam_role.ecs_task_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution_custom_policy" {
  role       = aws_iam_role.ecs_task_execution.name
  policy_arn = aws_iam_policy.ecs_task_execution_policy.arn
}

# Attach policies to Task Role
resource "aws_iam_role_policy_attachment" "ecs_task_role_policy" {
  role       = aws_iam_role.ecs_task_role.name
  policy_arn = aws_iam_policy.ecs_task_policy.arn
}

# SSM permissions for ECS Exec (optional, for debugging)
resource "aws_iam_role_policy_attachment" "ecs_task_role_ssm_policy" {
  role       = aws_iam_role.ecs_task_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}

resource "aws_ecs_task_definition" "api" {
  family                   = "${local.app_name}-task"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "2048"
  memory                   = "4096"
  execution_role_arn       = aws_iam_role.ecs_task_execution.arn
  task_role_arn            = aws_iam_role.ecs_task_role.arn

  ephemeral_storage {
    size_in_gib = 200
  }

  container_definitions = jsonencode([
    {
      name      = "${local.app_name}-api"
      image     = "${aws_ecr_repository.default.repository_url}:latest"
      essential = true

      secrets = [
        {
          name      = "Database__Host"
          valueFrom = "${aws_secretsmanager_secret.default.arn}:Database__Host::"
        },
        {
          name      = "Database__Port"
          valueFrom = "${aws_secretsmanager_secret.default.arn}:Database__Port::"
        },
        {
          name      = "Database__Username"
          valueFrom = "${aws_secretsmanager_secret.default.arn}:Database__Username::"
        },
        {
          name      = "Database__Password"
          valueFrom = "${aws_secretsmanager_secret.default.arn}:Database__Password::"
        },
        {
          name      = "Database__Database"
          valueFrom = "${aws_secretsmanager_secret.default.arn}:Database__Database::"
        },
        {
          name      = "S3__Bucket"
          valueFrom = "${aws_secretsmanager_secret.default.arn}:S3__Bucket::"
        },
        {
          name      = "App__ApiKey"
          valueFrom = "${aws_secretsmanager_secret.default.arn}:App__ApiKey::"
        }
      ]

      environment = [
        {
          name  = "TransformFunctionName"
          value = "${local.app_name}-ortho_transform"
        },
        {
          name  = "RoofExtractionFunctionName"
          value = "${local.app_name}-roof_extraction"
        },
        {
          name  = "ApplyTextureFunctionName"
          value = "${local.app_name}-texture_building"
        },
        {
          name  = "ExportBuildingFunctionName"
          value = "${local.app_name}-export_building"
        },
        {
          name  = "ExportMeshFunctionName"
          value = "${local.app_name}-export_mesh"
        },
        {
          name  = "UseSwagger"
          value = "true"
        },
        {
          name  = "EnableRequestResponseLogging"
          value = "true"
        },
        {
          name  = "AWS_REGION"
          value = local.aws.region
        }
      ]

      portMappings = [
        {
          containerPort = 8080
          protocol      = "tcp"
        }
      ]
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.ecs_logs_api.name
          "awslogs-region"        = local.aws.region
          "awslogs-stream-prefix" = "${local.app_name}-api"
        }
      }
    }
  ])
}

resource "aws_ecs_task_definition" "cms" {
  family                   = "${local.app_name}-cms-task"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "512"
  memory                   = "2048"
  execution_role_arn       = aws_iam_role.ecs_task_execution.arn
  task_role_arn            = aws_iam_role.ecs_task_role.arn

  container_definitions = jsonencode([
    {
      name      = "${local.app_name}-cms"
      image     = "${aws_ecr_repository.cms.repository_url}:latest"
      essential = true

      secrets = [
        {
          name      = "API_KEY"
          valueFrom = "${aws_secretsmanager_secret.cms.arn}:API_KEY::"
        }
      ]

      environment = [
        {
          name  = "API_URL"
          value = "https://api.${local.domain}"
        }
      ]

      portMappings = [
        {
          containerPort = 3000
          protocol      = "tcp"
        }
      ]
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.ecs_logs_cms.name
          "awslogs-region"        = local.aws.region
          "awslogs-stream-prefix" = "${local.app_name}-cms"
        }
      }
    }
  ])
}

resource "aws_ecs_service" "api" {
  name                   = "${local.app_name}-ecs-service-api"
  cluster                = aws_ecs_cluster.default.id
  task_definition        = aws_ecs_task_definition.api.arn
  desired_count          = 2
  launch_type            = "FARGATE"
  enable_execute_command = true

  network_configuration {
    subnets          = [aws_subnet.private_1a.id, aws_subnet.private_1c.id]
    security_groups  = [aws_security_group.ecs.id]
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.api.arn
    container_name   = "${local.app_name}-api"
    container_port   = 8080
  }

  depends_on = [aws_lb_listener.http]
}

resource "aws_ecs_service" "cms" {
  name                   = "${local.app_name}-ecs-service-cms"
  cluster                = aws_ecs_cluster.default.id
  task_definition        = aws_ecs_task_definition.cms.arn
  desired_count          = 1
  launch_type            = "FARGATE"
  enable_execute_command = true

  network_configuration {
    subnets          = [aws_subnet.private_1a.id, aws_subnet.private_1c.id]
    security_groups  = [aws_security_group.ecs.id]
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.cms.arn
    container_name   = "${local.app_name}-cms"
    container_port   = 3000
  }

  depends_on = [aws_lb_listener.http]
}