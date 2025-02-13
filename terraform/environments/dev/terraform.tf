locals {
  aws = {
    region                = "<AWS Region>"
    aws_access_key_id     = "<AWS Access Key>"
    aws_secret_access_key = "<AWS Secret Access Key>"
    aws_session_token     = "<AWS Session Token>"
  }

  app_name_prefix = "plateausnap"
  stage           = "<Stage>"
  cidr_prefix     = "<CIDR Prefix e.g. 10.0>"

  iam = {
    group_name = "<IAM Group of user executing the application>"
    user_name  = "<IAM User of executing the application>"
  }

  ec2 = {
    ami_id           = "ami-06c6f3fa7959e5fdd"
    instance_type    = "t3.large"
    public_key_file  = "<Full path to public key file (automatically generated) e.g. C:\\Users\\user\\.ssh\\plateausnap\\${local.stage}.id_rsa.pub>"
    private_key_file = "<Full path to public key file (will be deleted automatically) e.g. C:\\Users\\user\\.ssh\\plateausnap\\${local.stage}.id_rsa>"
  }

  rds = {
    engine_version    = "16.6"
    instance_class    = "db.t3.large"
    storage_type      = "gp2"
    allocated_storage = 20
    master_username   = "plateausnap_dev"
  }
}

locals {
  app_name = "${local.app_name_prefix}-${local.stage}"
}
