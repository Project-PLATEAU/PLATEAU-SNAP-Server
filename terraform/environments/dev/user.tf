
data "aws_iam_group" "default" {
  group_name = local.iam.group_name
}

data "aws_iam_user" "default" {
  user_name = local.iam.user_name
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
  group      = data.aws_iam_group.default.group_name
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
  group      = data.aws_iam_group.default.group_name
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
  group      = data.aws_iam_group.default.group_name
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
      "arn:aws:ecr:${local.aws.region}:${data.aws_caller_identity.current.account_id}:repository/${local.app_name}"
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
  group      = data.aws_iam_group.default.group_name
}
