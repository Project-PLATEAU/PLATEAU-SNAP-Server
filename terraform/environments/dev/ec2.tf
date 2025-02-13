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

data "template_file" "user_data" {
  template = file("../../script/init.sh")
}

resource "aws_instance" "app_server" {
  # ami                         = data.aws_ami.app.id
  ami           = local.ec2.ami_id
  instance_type = local.ec2.instance_type
  subnet_id     = aws_subnet.public_1a.id
  vpc_security_group_ids = [
    aws_security_group.ec2.id
  ]
  key_name  = "${local.app_name}-ec2-key-pair"
  user_data = base64encode(data.template_file.user_data.rendered)
  lifecycle {
    ignore_changes = [
      user_data
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
