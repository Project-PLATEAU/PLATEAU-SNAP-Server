resource "aws_cognito_user_pool" "cms" {
  name = "${local.app_name}-cms-user-pool"

  password_policy {
    minimum_length    = 8
    require_lowercase = true
    require_numbers   = true
    require_uppercase = true
    require_symbols   = false
  }

  account_recovery_setting {
    recovery_mechanism {
      name     = "verified_email"
      priority = 1
    }
  }

  auto_verified_attributes = ["email"]

  username_attributes = ["email"]

  mfa_configuration = "OFF"

  admin_create_user_config {
    allow_admin_create_user_only = true
    invite_message_template {
      email_message = <<-EOT
PLATEAU SNAP CMSへようこそ！<br><br>
アカウントが作成されました。<br><br>
ユーザー名: {username}<br>
仮パスワード: {####}<br><br>
以下のURLからログインし、パスワードを変更してください:<br>
https://cms.${local.domain}<br><br>
EOT
      email_subject = "PLATEAU SNAP CMS 仮パスワードのお知らせ"
      sms_message   = "ユーザー名: {username} 仮パスワード: {####}"
    }
  }

  deletion_protection = "ACTIVE"

  tags = {
    Name = "${local.app_name}-cms-user-pool"
  }
}

resource "aws_cognito_user_pool_client" "cms" {
  name         = "${local.app_name}-cms-client"
  user_pool_id = aws_cognito_user_pool.cms.id

  generate_secret = true

  supported_identity_providers = ["COGNITO"]

  explicit_auth_flows = [
    "ALLOW_REFRESH_TOKEN_AUTH",
    "ALLOW_USER_SRP_AUTH"
  ]

  # マネージドログインを使用
  enable_token_revocation                       = true
  enable_propagate_additional_user_context_data = false

  allowed_oauth_flows                  = ["code"]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes                 = ["email", "openid", "profile"]

  callback_urls = [
    "https://cms.${local.domain}/oauth2/idpresponse"
  ]

  logout_urls = [
    "https://cms.${local.domain}/logout"
  ]

  access_token_validity  = 1
  id_token_validity      = 1
  refresh_token_validity = 30

  token_validity_units {
    access_token  = "days"
    id_token      = "days"
    refresh_token = "days"
  }

  prevent_user_existence_errors = "ENABLED"

  read_attributes = [
    "email",
    "email_verified"
  ]

  write_attributes = [
    "email"
  ]
}

resource "aws_cognito_user_pool_domain" "cms" {
  domain                = "${local.app_name}-cms-${local.stage}"
  user_pool_id          = aws_cognito_user_pool.cms.id
  managed_login_version = 2
}

resource "aws_cognito_managed_login_branding" "cms" {
  user_pool_id = aws_cognito_user_pool.cms.id
  client_id    = aws_cognito_user_pool_client.cms.id

  use_cognito_provided_values = true
}
