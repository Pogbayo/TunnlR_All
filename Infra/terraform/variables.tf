variable "aws_region" {
  description = "AWS region to deploy resources into"
  type        = string
  default     = "eu-west-1"
}

variable "key_pair_name" {
  description = "Name of the EC2 key pair"
  type        = string
  default     = "tunnlr-key"
}

variable "ssh_public_key_path" {
  description = "Path to the SSH public key"
  type        = string
}

variable "instance_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t2.micro"
}

variable "ssh_allowed_ip" {
  description = "Public IP allowed to SSH into EC2"
  type        = string
}

variable "vpc_cidr" {
  description = "CIDR block for the VPC"
  type        = string
  default     = "10.0.0.0/16"
}
variable "subnet_cidr" {
  description = "CIDR block for the subnet"
  type        = string
  default     = "10.0.1.0/24"
}

# Subnet availability zone
variable "subnet_az" {
  description = "Availability zone for the subnet"
  type        = string
  default     = "eu-west-1a"
}