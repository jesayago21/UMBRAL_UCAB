<#
.SYNOPSIS
  Configura SMTP del realm umbral hacia Mailpit (dev) sin reimportar el realm.
#>
[CmdletBinding()]
param(
    [string]$Container = "umbral_keycloak",
    [string]$AdminUser = "admin",
    [string]$AdminPassword = "admin",
    [string]$Realm = "umbral",
    [string]$SmtpHost = "mailpit",
    [string]$SmtpPort = "1025",
    [string]$KeycloakInternalUrl = "http://localhost:8080"
)

$ErrorActionPreference = "Stop"

function Invoke-Kcadm {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$KcadmArgs)
    & docker exec $Container /opt/keycloak/bin/kcadm.sh @KcadmArgs
    if ($LASTEXITCODE -ne 0) {
        throw "kcadm falló (exit $LASTEXITCODE)"
    }
}

Write-Host "Conectando kcadm al realm master..."
Invoke-Kcadm config credentials `
    --server $KeycloakInternalUrl `
    --realm master `
    --user $AdminUser `
    --password $AdminPassword | Out-Null

Write-Host "Aplicando SMTP (${SmtpHost}:${SmtpPort}) en realm '$Realm'..."
Invoke-Kcadm update "realms/$Realm" `
    -s "smtpServer.host=$SmtpHost" `
    -s "smtpServer.port=$SmtpPort" `
    -s "smtpServer.from=noreply@umbral.local" `
    -s "smtpServer.fromDisplayName=UMBRAL" `
    -s "smtpServer.ssl=false" `
    -s "smtpServer.starttls=false" `
    -s "smtpServer.auth=false" `
    -s "smtpServer.replyTo=noreply@umbral.local" | Out-Null

Write-Host "SMTP configurado. UI de Mailpit: http://localhost:8025"
