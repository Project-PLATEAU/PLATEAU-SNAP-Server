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
