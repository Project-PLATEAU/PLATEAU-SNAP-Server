resource "aws_vpc_endpoint" "s3_gateway" {
  vpc_id            = aws_vpc.default.id
  service_name      = "com.amazonaws.${local.aws.region}.s3"
  vpc_endpoint_type = "Gateway"

  route_table_ids = [
    aws_route_table.private_1a.id,
    aws_route_table.private_1c.id,
  ]

  tags = {
    Name = "${local.app_name}-s3-gateway-endpoint"
  }
}

# Secrets Manager VPC Endpoint for secure access from ECS
resource "aws_vpc_endpoint" "secretsmanager" {
  vpc_id             = aws_vpc.default.id
  service_name       = "com.amazonaws.${local.aws.region}.secretsmanager"
  vpc_endpoint_type  = "Interface"
  subnet_ids         = [aws_subnet.private_1a.id, aws_subnet.private_1c.id]
  security_group_ids = [aws_security_group.vpc_endpoints.id]

  tags = {
    Name = "${local.app_name}-secretsmanager-endpoint"
  }
}

# ECR VPC Endpoints for ECS Fargate
resource "aws_vpc_endpoint" "ecr_dkr" {
  vpc_id             = aws_vpc.default.id
  service_name       = "com.amazonaws.${local.aws.region}.ecr.dkr"
  vpc_endpoint_type  = "Interface"
  subnet_ids         = [aws_subnet.private_1a.id, aws_subnet.private_1c.id]
  security_group_ids = [aws_security_group.vpc_endpoints.id]

  tags = {
    Name = "${local.app_name}-ecr-dkr-endpoint"
  }
}

resource "aws_vpc_endpoint" "ecr_api" {
  vpc_id             = aws_vpc.default.id
  service_name       = "com.amazonaws.${local.aws.region}.ecr.api"
  vpc_endpoint_type  = "Interface"
  subnet_ids         = [aws_subnet.private_1a.id, aws_subnet.private_1c.id]
  security_group_ids = [aws_security_group.vpc_endpoints.id]

  tags = {
    Name = "${local.app_name}-ecr-api-endpoint"
  }
}

# CloudWatch Logs VPC Endpoint
resource "aws_vpc_endpoint" "logs" {
  vpc_id             = aws_vpc.default.id
  service_name       = "com.amazonaws.${local.aws.region}.logs"
  vpc_endpoint_type  = "Interface"
  subnet_ids         = [aws_subnet.private_1a.id, aws_subnet.private_1c.id]
  security_group_ids = [aws_security_group.vpc_endpoints.id]

  tags = {
    Name = "${local.app_name}-logs-endpoint"
  }
}
