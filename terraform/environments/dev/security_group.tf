data "http" "ifconfig" {
  url = "http://ipv4.icanhazip.com/"
}

variable "allowed_cidr" {
  default = null
}

locals {
  current_ip   = chomp(data.http.ifconfig.response_body)
  allowed_cidr = (var.allowed_cidr == null) ? "${local.current_ip}/32" : var.allowed_cidr
}

# ---------------------------------------------
# Security Group
# ---------------------------------------------

resource "aws_security_group" "ec2" {
  name        = "${local.app_name}-ec2-sg"
  description = "Security Group for EC2"
  vpc_id      = aws_vpc.default.id
  tags = {
    Name = "${local.app_name}-ec2-sg"
  }
}

resource "aws_security_group" "rds" {
  name        = "${local.app_name}-rds-sg"
  description = "Security Group for RDS"
  vpc_id      = aws_vpc.default.id
  tags = {
    Name = "${local.app_name}-rds-sg"
  }
}

resource "aws_security_group" "lambda_sg_s3_only" {
  name        = "${local.app_name}-lambda-sg-s3-only"
  description = "Allow outbound traffic to S3 Gateway Endpoint"
  vpc_id      = aws_vpc.default.id

  tags = {
    Name = "${local.app_name}-lambda-sg-s3-only"
  }

  egress {
    description     = "HTTPS to S3 Gateway Endpoint for bucket operations"
    from_port       = 443
    to_port         = 443
    protocol        = "tcp"
    prefix_list_ids = [data.aws_prefix_list.s3.id]
  }
}

resource "aws_security_group" "lambda_sg_internet_access" {
  name        = "${local.app_name}-lambda-sg-internet-access"
  description = "Allow all outbound traffic"
  vpc_id      = aws_vpc.default.id

  tags = {
    Name = "${local.app_name}-lambda-sg-internet-access"
  }

  egress {
    description = "HTTPS for external API calls"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_security_group" "lambda_rds" {
  name        = "${local.app_name}-lambda-rds-sg"
  description = "Security Group for RDS"
  vpc_id      = aws_vpc.default.id
  tags = {
    Name = "${local.app_name}-lambda-rds-sg"
  }
}

resource "aws_security_group" "alb" {
  name        = "${local.app_name}-alb-sg"
  description = "${local.app_name} alb rule based routing"
  vpc_id      = aws_vpc.default.id
  tags = {
    Name = "${local.app_name}-alb-sg"
  }
}

resource "aws_security_group" "ecs" {
  name        = "${local.app_name}-ecs-sg"
  description = "Security Group for ECS"
  vpc_id      = aws_vpc.default.id
  tags = {
    Name = "${local.app_name}-ecs-sg"
  }
}

resource "aws_security_group" "vpc_endpoints" {
  name        = "${local.app_name}-vpc-endpoints-sg"
  description = "Security Group for VPC Endpoints"
  vpc_id      = aws_vpc.default.id
  tags = {
    Name = "${local.app_name}-vpc-endpoints-sg"
  }
}

data "aws_prefix_list" "s3" {
  name = "com.amazonaws.${local.aws.region}.s3"
}

resource "aws_security_group_rule" "ec2_in_ssh" {
  security_group_id = aws_security_group.ec2.id
  type              = "ingress"
  protocol          = "tcp"
  from_port         = 22
  to_port           = 22
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_security_group_rule" "ec2_in_http" {
  security_group_id = aws_security_group.ec2.id
  type              = "ingress"
  protocol          = "tcp"
  from_port         = 80
  to_port           = 80
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_security_group_rule" "ec2_in_https" {
  security_group_id = aws_security_group.ec2.id
  type              = "ingress"
  protocol          = "tcp"
  from_port         = 443
  to_port           = 443
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_security_group_rule" "ec2_out_any" {
  security_group_id = aws_security_group.ec2.id
  type              = "egress"
  protocol          = "-1"
  from_port         = 0
  to_port           = 0
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_security_group_rule" "rds_in_rds" {
  security_group_id = aws_security_group.rds.id
  type              = "ingress"
  protocol          = "tcp"
  from_port         = 5432
  to_port           = 5432
  cidr_blocks       = [aws_subnet.public_1a.cidr_block]
}

resource "aws_security_group_rule" "rds_out_any" {
  security_group_id = aws_security_group.rds.id
  type              = "egress"
  protocol          = "-1"
  from_port         = 0
  to_port           = 0
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_security_group_rule" "rds_in_lambda" {
  security_group_id        = aws_security_group.rds.id
  type                     = "ingress"
  protocol                 = "tcp"
  from_port                = 5432
  to_port                  = 5432
  source_security_group_id = aws_security_group.lambda_rds.id
}

resource "aws_security_group_rule" "lambda_rds_out_any" {
  security_group_id = aws_security_group.lambda_rds.id
  type              = "egress"
  protocol          = "-1"
  from_port         = 0
  to_port           = 0
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_security_group_rule" "alb_http" {
  from_port         = 80
  protocol          = "tcp"
  security_group_id = aws_security_group.alb.id
  to_port           = 80
  type              = "ingress"
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_security_group_rule" "alb_https" {
  from_port         = 443
  protocol          = "tcp"
  security_group_id = aws_security_group.alb.id
  to_port           = 443
  type              = "ingress"
  cidr_blocks       = ["0.0.0.0/0"]
}

# ALB to ECS communication (API on port 8080)
resource "aws_security_group_rule" "alb_to_ecs_api" {
  from_port                = 8080
  protocol                 = "tcp"
  security_group_id        = aws_security_group.alb.id
  to_port                  = 8080
  type                     = "egress"
  source_security_group_id = aws_security_group.ecs.id
}

# ALB to ECS communication (CMS on port 3000)
resource "aws_security_group_rule" "alb_to_ecs_cms" {
  from_port                = 3000
  protocol                 = "tcp"
  security_group_id        = aws_security_group.alb.id
  to_port                  = 3000
  type                     = "egress"
  source_security_group_id = aws_security_group.ecs.id
}

# ECS from ALB (API on port 8080)
resource "aws_security_group_rule" "ecs_from_alb_api" {
  from_port                = 8080
  protocol                 = "tcp"
  security_group_id        = aws_security_group.ecs.id
  to_port                  = 8080
  type                     = "ingress"
  source_security_group_id = aws_security_group.alb.id
}

# ECS from ALB (CMS on port 3000)
resource "aws_security_group_rule" "ecs_from_alb_cms" {
  from_port                = 3000
  protocol                 = "tcp"
  security_group_id        = aws_security_group.ecs.id
  to_port                  = 3000
  type                     = "ingress"
  source_security_group_id = aws_security_group.alb.id
}

# ECS outbound for internet access (ECR, CloudWatch, etc.)
resource "aws_security_group_rule" "ecs_egress" {
  from_port         = 0
  protocol          = "-1"
  security_group_id = aws_security_group.ecs.id
  to_port           = 0
  type              = "egress"
  cidr_blocks       = ["0.0.0.0/0"]
}

# ECS to RDS
resource "aws_security_group_rule" "ecs_to_rds" {
  from_port                = 5432
  protocol                 = "tcp"
  security_group_id        = aws_security_group.ecs.id
  to_port                  = 5432
  type                     = "egress"
  source_security_group_id = aws_security_group.rds.id
}

# RDS from ECS
resource "aws_security_group_rule" "rds_from_ecs" {
  from_port                = 5432
  protocol                 = "tcp"
  security_group_id        = aws_security_group.rds.id
  to_port                  = 5432
  type                     = "ingress"
  source_security_group_id = aws_security_group.ecs.id
}

# VPC Endpoints - ingress from ECS for HTTPS
resource "aws_security_group_rule" "vpc_endpoints_from_ecs" {
  from_port                = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.vpc_endpoints.id
  to_port                  = 443
  type                     = "ingress"
  source_security_group_id = aws_security_group.ecs.id
}

# ECS to VPC Endpoints for HTTPS
resource "aws_security_group_rule" "ecs_to_vpc_endpoints" {
  from_port                = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.ecs.id
  to_port                  = 443
  type                     = "egress"
  source_security_group_id = aws_security_group.vpc_endpoints.id
}

# Lambda RDS to VPC Endpoints for HTTPS (Secrets Manager access)
resource "aws_security_group_rule" "lambda_rds_to_vpc_endpoints" {
  from_port                = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.lambda_rds.id
  to_port                  = 443
  type                     = "egress"
  source_security_group_id = aws_security_group.vpc_endpoints.id
}

# VPC Endpoints - ingress from Lambda RDS
resource "aws_security_group_rule" "vpc_endpoints_from_lambda_rds" {
  from_port                = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.vpc_endpoints.id
  to_port                  = 443
  type                     = "ingress"
  source_security_group_id = aws_security_group.lambda_rds.id
}

# VPC Endpoints - ingress from Lambda S3-only
resource "aws_security_group_rule" "vpc_endpoints_from_lambda_s3" {
  from_port                = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.vpc_endpoints.id
  to_port                  = 443
  type                     = "ingress"
  source_security_group_id = aws_security_group.lambda_sg_s3_only.id
}
