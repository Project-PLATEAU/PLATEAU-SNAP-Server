#!/bin/bash

# ===== 設定 =====
PROFILE=${1:-your-sso-profile}  # 引数がなければデフォルトの SSO プロファイル名
REGION="ap-northeast-1"

echo "▶ Logging in with AWS SSO profile: $PROFILE"

# ===== ログイン処理（ブラウザでMFAなどを含むSSOログイン）=====
aws sso login --profile "$PROFILE"
if [ $? -ne 0 ]; then
  echo "❌ SSO login failed. Check your ~/.aws/config settings for profile '$PROFILE'"
  exit 1
fi

# ===== 環境変数を設定 =====
export AWS_PROFILE="$PROFILE"
export AWS_REGION="$REGION"

echo "✅ Login complete. AWS_PROFILE='$AWS_PROFILE' is set."
echo "You can now run Terraform, AWS CLI, etc. using this session."
