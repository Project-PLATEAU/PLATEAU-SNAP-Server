locals {
  aws = {
    region  = "ap-northeast-1"
    # profile = "<AWS CLI profile name>"
  }

  app_name_prefix = "plateausnap"
  stage           = "dev"
  cidr_prefix     = "10.0"
  domain          = "<Your domain name (e.g. example.com)>"

  iam = {
    group_name = "snap-dev-group"
    user_name  = "snap-dev-user"
  }

  ec2 = {
    ami_id           = "ami-06c6f3fa7959e5fdd"
    instance_type    = "t3.large"
    public_key_file  = "<Full path to public key file (automatically generated) e.g. C:\\Users\\user\\.ssh\\plateausnap\\${local.stage}.id_rsa.pub>"
    private_key_file = "<Full path to public key file (will be deleted automatically) e.g. C:\\Users\\user\\.ssh\\plateausnap\\${local.stage}.id_rsa>"
  }

  rds = {
    engine_version    = "16.8"
    instance_class    = "db.t3.large"
    storage_type      = "gp2"
    allocated_storage = 20
    master_username   = "plateausnap_dev"
  }
}

locals {
  app_name = "${local.app_name_prefix}-${local.stage}"
}
