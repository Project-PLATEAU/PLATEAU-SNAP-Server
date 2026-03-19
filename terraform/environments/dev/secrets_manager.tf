# ---------------------------------------------
# Secrets Manager
# ---------------------------------------------
resource "aws_secretsmanager_secret" "default" {
  name = "${local.app_name}-secret"
}

# DB名, ユーザ名, パスワードは後で手動で設定する
resource "aws_secretsmanager_secret_version" "default" {
  secret_id = aws_secretsmanager_secret.default.id
  secret_string = jsonencode({
    "Database__Host" = "${aws_db_instance.app_db.address}"
    "Database__Port" = "${aws_db_instance.app_db.port}"
    "S3__Bucket"     = "${aws_s3_bucket.default.bucket}"
    "App__ApiKey"    = "${random_bytes.api_key.base64}"
  })
  depends_on = [aws_db_instance.app_db]
  lifecycle {
    ignore_changes = [secret_string]
  }
}

resource "aws_secretsmanager_secret" "cms" {
  name = "${local.app_name}-cms-secret"
}

resource "aws_secretsmanager_secret_version" "cms" {
  secret_id = aws_secretsmanager_secret.cms.id
  secret_string = jsonencode({
    "API_KEY" = "${random_bytes.api_key.base64}"
  })
  lifecycle {
    ignore_changes = [secret_string]
  }
}

resource "random_bytes" "api_key" {
  length = 64
}
