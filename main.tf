#####################################
# Provider
#####################################

provider "aws" {
  region = var.aws_region
}

#####################################
# VPC 
#####################################
resource "aws_vpc" "tunnlr_vpc" {
  cidr_block = var.vpc_cidr

  tags = {
    Name = "TunnlR-VPC"
  }
}

# Internet Gateway
resource "aws_internet_gateway" "tunnlr_igw" {
  vpc_id = aws_vpc.tunnlr_vpc.id

  tags = {
    Name = "TunnlR-IGW"
  }
}

# Route Table
resource "aws_route_table" "tunnlr_route" {
  vpc_id = aws_vpc.tunnlr_vpc.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.tunnlr_igw.id
  }

  tags = {
    Name = "TunnlR-RouteTable"
  }
}

resource "aws_route_table_association" "tunnlr_subnet_assoc" {
  subnet_id      = aws_subnet.tunnlr_subnet.id
  route_table_id = aws_route_table.tunnlr_route.id
}


#####################################
# Subnet
#####################################

resource "aws_subnet" "tunnlr_subnet" {
  vpc_id            = aws_vpc.tunnlr_vpc.id
  cidr_block        = var.subnet_cidr
  availability_zone = var.subnet_az

  tags = {
    Name = "TunnlR-Subnet"
  }
}


#####################################
# SSH Key Pair
#####################################

resource "aws_key_pair" "tunnlr_key" {
  key_name   = var.key_pair_name
  public_key = file(var.ssh_public_key_path)
}

#####################################
# Security Group
#####################################

resource "aws_security_group" "tunnlr_sg" {
  name        = "tunnlr-sg"
  description = "Security group for TunnlR EC2"
  vpc_id      = aws_vpc.tunnlr_vpc.id  

  # SSH – only from your laptop IP
  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["${var.ssh_allowed_ip}/32"]
  }

  # HTTP
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # HTTPS
  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Outbound – allow all
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "tunnlr-sg"
  }
}

#####################################
# Ubuntu AMI (eu-west-1 safe)
#####################################

data "aws_ami" "ubuntu" {
  most_recent = true

  filter {
    name   = "name"
    values = ["ubuntu/images/hvm-ssd/ubuntu-jammy-22.04-amd64-server-*"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }

  owners = ["099720109477"] # Canonical
}

#####################################
# EC2 Instance
#####################################

resource "aws_instance" "tunnlr_ec2" {
  ami           = data.aws_ami.ubuntu.id
  instance_type = var.instance_type
  subnet_id     = aws_subnet.tunnlr_subnet.id
  key_name      = aws_key_pair.tunnlr_key.key_name
  associate_public_ip_address = true  
  
  vpc_security_group_ids = [
    aws_security_group.tunnlr_sg.id
  ]

  tags = {
    Name = "TunnlR-Server"
  }
}
