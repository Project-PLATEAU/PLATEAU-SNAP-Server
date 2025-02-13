# ---------------------------------------------
# VPC
# ---------------------------------------------

resource "aws_vpc" "default" {
  enable_dns_hostnames = true
  enable_dns_support   = true
  cidr_block           = "${local.cidr_prefix}.0.0/16"
  tags = {
    Name = local.app_name
  }
}

# ---------------------------------------------
# Public Subnet
# ---------------------------------------------

resource "aws_internet_gateway" "default" {
  vpc_id = aws_vpc.default.id
  tags = {
    Name = "${local.app_name}-igw"
  }
}

resource "aws_subnet" "public_1a" {
  vpc_id            = aws_vpc.default.id
  cidr_block        = "${local.cidr_prefix}.1.0/24"
  availability_zone = "ap-northeast-1a"
  tags = {
    Name = "${local.app_name}-public-1a"
  }
}

# ---------------------------------------------
# Private Subnet
# ---------------------------------------------

resource "aws_subnet" "private_1a" {
  vpc_id            = aws_vpc.default.id
  cidr_block        = "${local.cidr_prefix}.10.0/24"
  availability_zone = "ap-northeast-1a"
  tags = {
    Name = "${local.app_name}-private-1a"
  }
}

resource "aws_subnet" "private_1c" {
  vpc_id            = aws_vpc.default.id
  cidr_block        = "${local.cidr_prefix}.20.0/24"
  availability_zone = "ap-northeast-1c"
  tags = {
    Name = "${local.app_name}-private-1c"
  }
}

# ---------------------------------------------
# Elastic IP for nat
# ---------------------------------------------

resource "aws_eip" "nat_1a" {
  associate_with_private_ip = true
  tags = {
    Name = "${local.app_name}-eip-ngw-1a"
  }
}

# ---------------------------------------------
# NAT Gateway
# ---------------------------------------------
resource "aws_nat_gateway" "nat_1a" {
  subnet_id     = aws_subnet.public_1a.id
  allocation_id = aws_eip.nat_1a.id
  tags = {
    Name = "${local.app_name}-ngw-1a"
  }
}

# ---------------------------------------------
# Public Subnet Route Table
# ---------------------------------------------

resource "aws_route_table" "public" {
  vpc_id = aws_vpc.default.id
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.default.id
  }
  tags = {
    Name = "${local.app_name}-public-rt"
  }
}

# ---------------------------------------------
# Public Route Table Association
# ---------------------------------------------

resource "aws_route_table_association" "public_1a_to_ig" {
  subnet_id      = aws_subnet.public_1a.id
  route_table_id = aws_route_table.public.id
}

# ---------------------------------------------
# Private Subnet Route Table
# ---------------------------------------------

resource "aws_route_table" "private_1a" {
  vpc_id = aws_vpc.default.id
  route {
    cidr_block     = "0.0.0.0/0"
    nat_gateway_id = aws_nat_gateway.nat_1a.id
  }
  tags = {
    Name = "${local.app_name}-private-1a-rt"
  }
}

# ---------------------------------------------
# Private Route Table Association
# ---------------------------------------------

resource "aws_route_table_association" "private_1a" {
  route_table_id = aws_route_table.private_1a.id
  subnet_id      = aws_subnet.private_1a.id
}
