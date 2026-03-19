terraform {
  required_version = ">= 1.7"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.17"
    }
    random = {
      source  = "hashicorp/random"
      version = ">= 3.0"
    }
  }
  backend "s3" {
    region  = "ap-northeast-1"
    bucket  = "plateausnap-terraform"
    key     = "dev/terraform.tfstate"
    encrypt = true
    # profile = "<AWS CLI profile name>"
  }
}

provider "aws" {
  region  = local.aws.region
  # profile = "<AWS CLI profile name>"
}
