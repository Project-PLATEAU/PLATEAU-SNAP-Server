resource "aws_route53_zone" "default" {
  name = local.domain
}

resource "aws_lb" "default" {
  name               = "${local.app_name}-alb"
  load_balancer_type = "application"
  security_groups = [
    aws_security_group.alb.id
  ]
  subnets = [
    aws_subnet.public_1a.id,
    aws_subnet.public_1c.id,
  ]

  idle_timeout = 300
}

# Target Group for ECS API
resource "aws_lb_target_group" "api" {
  name        = "${local.app_name}-api-tg"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = aws_vpc.default.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = "/health"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 2
  }

  tags = {
    Name = "${local.app_name}-api-tg"
  }
}

# HTTPS Listener with host-based routing
resource "aws_lb_listener" "https" {
  load_balancer_arn = aws_lb.default.arn
  port              = 443
  protocol          = "HTTPS"
  certificate_arn   = aws_acm_certificate_validation.default.certificate_arn

  default_action {
    type = "fixed-response"
    fixed_response {
      content_type = "text/plain"
      message_body = "404 Not Found"
      status_code  = "404"
    }
  }
}

# HTTPS Listener Rule for API subdomain
resource "aws_lb_listener_rule" "api_https" {
  listener_arn = aws_lb_listener.https.arn
  priority     = 100

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api.arn
  }

  condition {
    host_header {
      values = ["api.${local.domain}"]
    }
  }
}

# HTTP Listener - Redirect to HTTPS
resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.default.arn
  port              = "80"
  protocol          = "HTTP"

  default_action {
    type = "redirect"
    redirect {
      port        = "443"
      protocol    = "HTTPS"
      status_code = "HTTP_301"
    }
  }
}

resource "aws_acm_certificate" "default" {
  domain_name       = local.domain
  validation_method = "DNS"

  subject_alternative_names = [
    "api.${local.domain}",
    "cms.${local.domain}",
  ]

  lifecycle {
    create_before_destroy = true
  }
}

# ACM Certificate Validation
resource "aws_acm_certificate_validation" "default" {
  certificate_arn         = aws_acm_certificate.default.arn
  validation_record_fqdns = [for record in aws_route53_record.cert_validation : record.fqdn]
}

resource "aws_route53_record" "cert_validation" {
  for_each = {
    for dvo in aws_acm_certificate.default.domain_validation_options : dvo.domain_name => {
      name   = dvo.resource_record_name
      record = dvo.resource_record_value
      type   = dvo.resource_record_type
    }
  }

  allow_overwrite = true
  name            = each.value.name
  records         = [each.value.record]
  ttl             = 60
  type            = each.value.type
  zone_id         = aws_route53_zone.default.zone_id
}

# Route53 A record for API subdomain pointing to ALB
resource "aws_route53_record" "api" {
  zone_id = aws_route53_zone.default.zone_id
  name    = "api.${local.domain}"
  type    = "A"

  alias {
    name                   = aws_lb.default.dns_name
    zone_id                = aws_lb.default.zone_id
    evaluate_target_health = true
  }
}

resource "aws_lb_target_group" "cms" {
  name        = "${local.app_name}-cms-tg"
  port        = 3000
  protocol    = "HTTP"
  vpc_id      = aws_vpc.default.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = "/"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 2
  }

  tags = {
    Name = "${local.app_name}-cms-tg"
  }
}

resource "aws_lb_listener_rule" "cms_https" {
  listener_arn = aws_lb_listener.https.arn
  priority     = 101

  # Cognito認証アクション
  action {
    type = "authenticate-cognito"
    authenticate_cognito {
      user_pool_arn       = aws_cognito_user_pool.cms.arn
      user_pool_client_id = aws_cognito_user_pool_client.cms.id
      user_pool_domain    = aws_cognito_user_pool_domain.cms.domain
      authentication_request_extra_params = {
        "lang" = "ja"
      }

      on_unauthenticated_request = "authenticate"

      session_cookie_name = "AWSELBAuthSessionCookie"
      session_timeout     = 3600

      scope = "openid email profile"
    }
  }

  # 認証成功後の転送アクション
  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.cms.arn
  }

  condition {
    host_header {
      values = ["cms.${local.domain}"]
    }
  }
}

resource "aws_route53_record" "cms" {
  zone_id = aws_route53_zone.default.zone_id
  name    = "cms.${local.domain}"
  type    = "A"

  alias {
    name                   = aws_lb.default.dns_name
    zone_id                = aws_lb.default.zone_id
    evaluate_target_health = true
  }
}
