# ---------------------------------------------
# IAM Group
# ---------------------------------------------
resource "aws_iam_group" "default" {
  name = local.iam.group_name
}

# ---------------------------------------------
# IAM User
# ---------------------------------------------
resource "aws_iam_user" "default" {
  name = local.iam.user_name
}

resource "aws_iam_user_group_membership" "default" {
  user = aws_iam_user.default.name
  groups = [
    aws_iam_group.default.name
  ]
}

# ---------------------------------------------
# App IAM user
# ---------------------------------------------
data "aws_iam_policy_document" "allow_s3_rw" {
  version = "2012-10-17"
  statement {
    effect = "Allow"
    actions = [
      "s3:PutObject",
      "s3:GetObject",
      "s3:ListBucket",
    ]
    resources = [
      "arn:aws:s3:::${aws_s3_bucket.default.bucket}/*",
      "arn:aws:s3:::${aws_s3_bucket.default.bucket}"
    ]
  }
}

resource "aws_iam_policy" "allow_s3_rw" {
  name        = "${local.app_name}-allow-s3-read-write"
  description = "Allow S3 read write"
  policy      = data.aws_iam_policy_document.allow_s3_rw.json
}

resource "aws_iam_group_policy_attachment" "allow_s3_rw" {
  policy_arn = aws_iam_policy.allow_s3_rw.arn
  group      = aws_iam_group.default.name
}

data "aws_iam_policy_document" "allow_secret_manager_r" {
  version = "2012-10-17"
  statement {
    effect = "Allow"
    actions = [
      "secretsmanager:GetResourcePolicy",
      "secretsmanager:GetSecretValue",
      "secretsmanager:DescribeSecret",
      "secretsmanager:ListSecretVersionIds"
    ]
    resources = [
      "arn:aws:secretsmanager:${local.aws.region}:${data.aws_caller_identity.current.account_id}:secret:${local.app_name}-*"
    ]
  }
}

resource "aws_iam_policy" "allow_secret_manager_r" {
  name        = "${local.app_name}-allow-secret-manager-read"
  description = "Allow secret manager read"
  policy      = data.aws_iam_policy_document.allow_secret_manager_r.json
}

resource "aws_iam_group_policy_attachment" "allow_rds_secret_manager_r" {
  policy_arn = aws_iam_policy.allow_secret_manager_r.arn
  group      = aws_iam_group.default.name
}

data "aws_iam_policy_document" "allow_ecr_pull" {
  version = "2012-10-17"
  statement {
    effect = "Allow"
    actions = [
      "ecr:GetDownloadUrlForLayer",
      "ecr:BatchGetImage",
      "ecr:BatchCheckLayerAvailability",
      "ecr:GetAuthorizationToken"
    ]
    resources = [
      "arn:aws:ecr:${local.aws.region}:${data.aws_caller_identity.current.account_id}:repository/${local.app_name}"
    ]
  }
}

resource "aws_iam_policy" "allow_ecr_pull" {
  name        = "${local.app_name}-allow-ecr-pull"
  description = "Allow ECR pull"
  policy      = data.aws_iam_policy_document.allow_ecr_pull.json
}

resource "aws_iam_group_policy_attachment" "allow_ecr_pull" {
  policy_arn = aws_iam_policy.allow_ecr_pull.arn
  group      = aws_iam_group.default.name
}

data "aws_iam_policy_document" "allow_ecr_push" {
  version = "2012-10-17"
  statement {
    effect = "Allow"
    actions = [
      "ecr:CompleteLayerUpload",
      "ecr:UploadLayerPart",
      "ecr:InitiateLayerUpload",
      "ecr:BatchCheckLayerAvailability",
      "ecr:PutImage",
      "ecr:BatchGetImage"
    ]
    resources = [
      "arn:aws:ecr:${local.aws.region}:${data.aws_caller_identity.current.account_id}:repository/${local.app_name}",
      "arn:aws:ecr:${local.aws.region}:${data.aws_caller_identity.current.account_id}:repository/${local.app_name}-cms",
      "arn:aws:ecr:${local.aws.region}:${data.aws_caller_identity.current.account_id}:repository/export-lambda",
    ]
  }
  statement {
    effect = "Allow"
    actions = [
      "ecr:GetAuthorizationToken"
    ]
    resources = [
      "*"
    ]
  }
}

resource "aws_iam_policy" "allow_ecr_push" {
  name        = "${local.app_name}-allow-ecr-push"
  description = "Allow ECR push"
  policy      = data.aws_iam_policy_document.allow_ecr_push.json
}

resource "aws_iam_group_policy_attachment" "allow_ecr_push" {
  policy_arn = aws_iam_policy.allow_ecr_push.arn
  group      = aws_iam_group.default.name
}
resource "aws_iam_group_policy" "lambda_invoke" {
  name  = "${local.app_name}-lambda-invoke"
  group = local.iam.group_name

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action   = "lambda:InvokeFunction"
      Effect   = "Allow"
      Resource = "arn:aws:lambda:${local.aws.region}:${data.aws_caller_identity.current.account_id}:function:${local.app_name_prefix}-${local.stage}-*"
    }]
  })
}