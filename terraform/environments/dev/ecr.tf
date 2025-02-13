# ---------------------------------------------
# ECR
# ---------------------------------------------
resource "aws_ecr_repository" "default" {
  name = "${local.app_name}"
  tags = {
    Name = "${local.app_name}-ecr"
  }
}
