<# 
.SYNOPSIS
  AWS SSO にログインし、指定プロファイルの一時クレデンシャルを使えるようにする
#>

param(
    [Parameter(Position=0)]
    [string]$Profile = "default"
)

Write-Host "▶ Logging in with AWS SSO profile: $Profile"

# ログイン（ブラウザで確認コード入力）
aws sso login --profile $Profile

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ SSO login failed. Check your profile configuration in ~/.aws/config"
    exit 1
}

# 環境変数を設定
$env:AWS_PROFILE = $Profile

Write-Host "✅ SSO login complete. AWS_PROFILE set to '$Profile'"
Write-Host "Now you can run Terraform / AWS CLI using this session."
