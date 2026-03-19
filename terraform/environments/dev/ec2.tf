# data "aws_ami" "app" {
#   most_recent = true
#   owners      = ["amazon"]

#   filter {
#     name   = "name"
#     values = ["${local.ec2.ami_name}"]
#   }
#   filter {
#     name   = "architecture"
#     values = ["x86_64"]
#   }
#   filter {
#     name   = "root-device-type"
#     values = ["ebs"]
#   }
#   filter {
#     name   = "virtualization-type"
#     values = ["hvm"]
#   }
# }

# ---------------------------------------------
# EC2 instance
# ---------------------------------------------
resource "tls_private_key" "private_key" {
  algorithm = "RSA"
  rsa_bits  = 2048
}

resource "local_file" "private_key_pem" {
  filename = local.ec2.private_key_file
  content  = tls_private_key.private_key.private_key_pem
}

resource "aws_key_pair" "ec2" {
  key_name   = "${local.app_name}-ec2-key-pair"
  public_key = tls_private_key.private_key.public_key_openssh
}



resource "aws_instance" "app_server" {
  # ami                         = data.aws_ami.app.id
  ami                  = local.ec2.ami_id
  instance_type        = local.ec2.instance_type
  subnet_id            = aws_subnet.public_1a.id
  iam_instance_profile = aws_iam_instance_profile.ec2_profile.name
  vpc_security_group_ids = [
    aws_security_group.ec2.id
  ]
  key_name = "${local.app_name}-ec2-key-pair"

  user_data_base64 = filebase64("../../script/init.sh")
  lifecycle {
    ignore_changes = [
      user_data,
      user_data_base64
    ]
  }
  tags = {
    Name = "${local.app_name}-ec2"
  }
}

resource "aws_eip" "ec2" {
  instance                  = aws_instance.app_server.id
  associate_with_private_ip = aws_instance.app_server.private_ip
  tags = {
    Name = "${local.app_name}-eip-ec2"
  }
}

resource "aws_ebs_volume" "app_server_sdf" {
  availability_zone = aws_instance.app_server.availability_zone
  size              = 30
  type              = "gp3"
  iops              = 3000
  throughput        = 125
  encrypted         = "true"
  tags = {
    Name = "${local.app_name}-ec2"
  }
}

resource "aws_volume_attachment" "sdf" {
  device_name = "/dev/sdf"
  volume_id   = aws_ebs_volume.app_server_sdf.id
  instance_id = aws_instance.app_server.id
}

resource "aws_iam_role" "ec2_role" {
  name = "${local.app_name}-ec2-role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "ec2.amazonaws.com"
      }
    }]
  })
}

resource "aws_iam_instance_profile" "ec2_profile" {
  name = "${local.app_name}-ec2-profile"
  role = aws_iam_role.ec2_role.name
}

resource "aws_iam_role_policy" "ec2_lambda_invoke" {
  name = "${local.app_name}-ec2-lambda-invoke"
  role = aws_iam_role.ec2_role.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action   = "lambda:InvokeFunction"
      Effect   = "Allow"
      Resource = "arn:aws:lambda:${local.aws.region}:${data.aws_caller_identity.current.account_id}:function:${local.app_name}-*"
    }]
  })
}

# S3 access policy for EC2
resource "aws_iam_role_policy" "ec2_s3_access" {
  name = "${local.app_name}-ec2-s3-access"
  role = aws_iam_role.ec2_role.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = [
        "s3:GetObject",
        "s3:PutObject",
        "s3:DeleteObject",
        "s3:ListBucket"
      ]
      Effect = "Allow"
      Resource = [
        aws_s3_bucket.default.arn,
        "${aws_s3_bucket.default.arn}/*"
      ]
    }]
  })
}

# Secrets Manager access policy for EC2
resource "aws_iam_role_policy" "ec2_secrets_access" {
  name = "${local.app_name}-ec2-secrets-access"
  role = aws_iam_role.ec2_role.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = [
        "secretsmanager:GetSecretValue",
        "secretsmanager:DescribeSecret"
      ]
      Effect   = "Allow"
      Resource = aws_secretsmanager_secret.default.arn
    }]
  })
}

# Attach SSM Managed Instance Core policy for Session Manager
resource "aws_iam_role_policy_attachment" "ec2_ssm_policy" {
  role       = aws_iam_role.ec2_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}
