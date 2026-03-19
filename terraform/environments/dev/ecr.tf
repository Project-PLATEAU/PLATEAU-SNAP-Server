# ---------------------------------------------
# ECR
# ---------------------------------------------
resource "aws_ecr_repository" "default" {
  name = local.app_name

  image_scanning_configuration {
    scan_on_push = true
  }
  tags = {
    Name = "${local.app_name}-ecr"
  }
}

resource "aws_ecr_repository" "geo_lambda_image" {
  name                 = "geo-lambda"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }
  tags = {
    Name = "${local.app_name}-ecr"
  }
}

resource "aws_ecr_repository" "export_lambda_image" {
  name                 = "export-lambda"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name = "${local.app_name}-ecr"
  }
}

resource "aws_ecr_repository" "cms" {
  name = "${local.app_name}-cms"

  image_scanning_configuration {
    scan_on_push = true
  }
  tags = {
    Name = "${local.app_name}-ecr"
  }
}
