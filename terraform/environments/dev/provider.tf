terraform {
  required_version = ">= 1.7"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
  backend "s3" {
    access_key = "<Terraform Backend Access Key>"
    secret_key = "<Terraform Backend Secret Access Key>"
    token = "<Terraform Backend Session Token>"
    region  = "<Terraform Backend Region>"
    bucket  = "<Terraform Backend Bucket>"
    key     = "<Terraform Backend Path To the State File>"
    encrypt = true
  }
}

provider "aws" {
  region     = local.aws.region
  access_key = local.aws.aws_access_key_id
  secret_key = local.aws.aws_secret_access_key
  token      = local.aws.aws_session_token
}
