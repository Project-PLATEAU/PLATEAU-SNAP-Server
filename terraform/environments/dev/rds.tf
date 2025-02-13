data "aws_caller_identity" "current" {}

# ---------------------------------------------
# IAM
# ---------------------------------------------
data "aws_iam_policy_document" "rds_monitoring" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["monitoring.rds.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "rds_monitoring" {
  name               = "${local.app_name}-allow-rds-monitoring"
  assume_role_policy = data.aws_iam_policy_document.rds_monitoring.json
}

resource "aws_iam_role_policy_attachment" "rds_monitoring" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonRDSEnhancedMonitoringRole"
  role       = aws_iam_role.rds_monitoring.name
}

# ---------------------------------------------
# RDS instance
# ---------------------------------------------
resource "aws_db_subnet_group" "rds" {
  name = "${local.app_name}-db-sng"
  subnet_ids = [
    aws_subnet.private_1a.id,
    aws_subnet.private_1c.id,
  ]

  tags = {
    Name = "${local.app_name}-db-sng"
  }
}

# ---------------------------------------------
# RDS KMS
# ---------------------------------------------
resource "aws_kms_key" "rds_master_user" {
  description             = "KMS Key for RDS encryption"
  deletion_window_in_days = 7
  enable_key_rotation     = false
  tags = {
    Name = "${local.app_name}-kms-key-rds-master-user"
  }
}

resource "aws_kms_alias" "rds_master_user" {
  name          = "alias/${local.app_name}_rds_master_user"
  target_key_id = aws_kms_key.rds_master_user.key_id
}

resource "aws_kms_key" "rds_storage" {
  description             = "KMS Key for RDS storage"
  key_usage               = "ENCRYPT_DECRYPT"
  deletion_window_in_days = 7
  enable_key_rotation     = false
  tags = {
    Name = "${local.app_name}-kms-key-rds-storage"
  }
}

resource "aws_kms_alias" "rds_storage" {
  name          = "alias/${local.app_name}_rds_storage"
  target_key_id = aws_kms_key.rds_storage.key_id
}

# ---------------------------------------------
# RDS instance
# ---------------------------------------------
resource "aws_db_instance" "app_db" {
  identifier        = "${local.app_name}-db"
  engine            = "postgres"
  engine_version    = local.rds.engine_version
  instance_class    = local.rds.instance_class
  allocated_storage = local.rds.allocated_storage
  storage_type      = local.rds.storage_type
  username          = local.rds.master_username

  manage_master_user_password   = true
  master_user_secret_kms_key_id = aws_kms_key.rds_master_user.key_id

  vpc_security_group_ids = ["${aws_security_group.rds.id}"]
  db_subnet_group_name   = aws_db_subnet_group.rds.name
  publicly_accessible    = false

  multi_az                     = false
  backup_retention_period      = 7
  backup_window                = "03:00-04:00"
  skip_final_snapshot          = true
  performance_insights_enabled = true
  monitoring_role_arn          = aws_iam_role.rds_monitoring.arn
  monitoring_interval          = 60
  storage_encrypted            = true
  deletion_protection          = true
  # kms_key_id という名前なのに、何故か arn を指定する必要がある
  # ref: https://github.com/hashicorp/terraform-provider-aws/issues/25978
  kms_key_id = aws_kms_key.rds_storage.arn
  lifecycle {
    prevent_destroy = true
    ignore_changes  = [instance_class, deletion_protection]
  }
  tags = {
    Name = "${local.app_name}-db"
  }
}
