output "vpc_id" {
  value = aws_vpc.tunnlr_vpc.id
}

output "subnet_id" {
  value = aws_subnet.tunnlr_subnet.id
}

output "ec2_public_ip" {
  value = aws_instance.tunnlr_ec2.public_ip
}

output "ec2_public_dns" {
  value = aws_instance.tunnlr_ec2.public_dns
}
