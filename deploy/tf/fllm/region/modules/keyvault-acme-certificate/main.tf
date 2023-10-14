resource "acme_registration" "reg" {
  account_key_pem = tls_private_key.private_key.private_key_pem
  email_address   = var.administrator_email
}

resource "acme_certificate" "main" {
  account_key_pem           = acme_registration.reg.account_key_pem
  common_name               = var.domain
  subject_alternative_names = [var.domain]

  dns_challenge {
    provider = "azuredns"
    config = {
      AZURE_RESOURCE_GROUP = var.public_dns_zone.resource_group_name
      AZURE_TTL            = 300
      AZURE_ZONE_NAME      = var.public_dns_zone.name
    }
  }
}

resource "azurerm_key_vault_certificate" "certificate" {
  name         = replace(acme_certificate.main.common_name, ".", "-")
  key_vault_id = var.key_vault_id

  certificate {
    contents = acme_certificate.main.certificate_p12
    password = ""
  }

  certificate_policy {
    issuer_parameters {
      name = "Unknown"
    }

    key_properties {
      exportable = true
      key_size   = 2048
      key_type   = "RSA"
      reuse_key  = false
    }

    secret_properties {
      content_type = "application/x-pkcs12"
    }
  }
}

resource "azurerm_key_vault_secret" "ca" {
  name         = "${replace(acme_certificate.main.common_name, ".", "-")}-ca"
  key_vault_id = var.key_vault_id
  value        = acme_certificate.main.issuer_pem
}

resource "azurerm_key_vault_secret" "certificate" {
  name         = "${replace(acme_certificate.main.common_name, ".", "-")}-cert"
  key_vault_id = var.key_vault_id
  value        = "${acme_certificate.main.certificate_pem}${acme_certificate.main.issuer_pem}"
}

resource "azurerm_key_vault_secret" "key" {
  name         = "${replace(acme_certificate.main.common_name, ".", "-")}-key"
  key_vault_id = var.key_vault_id
  value        = acme_certificate.main.private_key_pem
}

resource "tls_private_key" "private_key" {
  algorithm = "RSA"
}