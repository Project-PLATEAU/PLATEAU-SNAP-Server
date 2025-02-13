# ---------------------------------------------
# S3 bucket
# ---------------------------------------------

resource "aws_s3_bucket" "default" {
  bucket = "${local.app_name}-bucket"

  tags = {
    Name = "${local.app_name}-bucket"
  }
}
