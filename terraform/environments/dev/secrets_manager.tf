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
    "Database:Host" = "${aws_db_instance.app_db.address}"
    "Database:Port" = "${aws_db_instance.app_db.port}"
    "S3:Bucket"     = "${aws_s3_bucket.default.bucket}"
  })
  depends_on = [aws_db_instance.app_db]
  lifecycle {
    ignore_changes = [secret_string]
  }
}
