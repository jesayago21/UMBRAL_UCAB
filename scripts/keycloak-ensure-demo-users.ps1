# Crea usuarios demo en Keycloak si faltan (p. ej. realm importado antes de añadir participante).
# Uso: .\scripts\keycloak-ensure-demo-users.ps1
# Requiere: docker compose up keycloak -d

$ErrorActionPreference = "Stop"
$Container = "umbral_keycloak"
$Realm = "umbral"
$Password = "Umbral123!"

function Test-KeycloakContainer {
    $running = docker ps --filter "name=$Container" --filter "status=running" -q
    if (-not $running) {
        Write-Error "El contenedor '$Container' no está en ejecución. Ejecuta: docker compose up keycloak -d"
    }
}

function Invoke-Kcadm {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Args)
    docker exec $Container /opt/keycloak/bin/kcadm.sh @Args 2>&1 | Out-String
}

function Ensure-RealmRole {
    param([string]$Role)
    $roles = Invoke-Kcadm get roles -r $Realm
    if ($roles -notmatch "`"name`"\s*:\s*`"$Role`"") {
        Write-Host "Creando rol de realm $Role..."
        Invoke-Kcadm create roles -r $Realm -s "name=$Role" | Out-Null
    }
}

function Ensure-DemoUser {
    param(
        [string]$Username,
        [string]$Email,
        [string]$FirstName,
        [string]$LastName,
        [string]$Role
    )

    Ensure-RealmRole -Role $Role

    $existing = Invoke-Kcadm get users -r $Realm -q "username=$Username"
    if ($existing -match '"id"\s*:') {
        Invoke-Kcadm add-roles -r $Realm --uusername $Username --rolename $Role 2>$null | Out-Null
        Write-Host "OK  $Username ya existe (rol $Role verificado)"
        return
    }

    Write-Host "Creando usuario $Username en realm $Realm..."
    Invoke-Kcadm create users -r $Realm `
        -s "username=$Username" `
        -s enabled=true `
        -s "email=$Email" `
        -s emailVerified=true `
        -s "firstName=$FirstName" `
        -s "lastName=$LastName" | Out-Null

    Invoke-Kcadm set-password -r $Realm --username $Username --new-password $Password | Out-Null
    Invoke-Kcadm add-roles -r $Realm --uusername $Username --rolename $Role | Out-Null
    Write-Host "OK  $Username creado (contraseña: $Password, rol: $Role)"
}

Test-KeycloakContainer
Write-Host "Conectando kcadm al realm master..."
Invoke-Kcadm config credentials --server http://localhost:8080 --realm master --user admin --password admin | Out-Null

Ensure-DemoUser -Username "admin" -Email "admin@umbral.local" -FirstName "Admin" -LastName "UMBRAL" -Role "Administrador"
Ensure-DemoUser -Username "operador" -Email "operador@umbral.local" -FirstName "Operador" -LastName "UMBRAL" -Role "Operador"
Ensure-DemoUser -Username "participante" -Email "participante@umbral.local" -FirstName "Participante" -LastName "UMBRAL" -Role "Participante"

Write-Host ""
Write-Host "Listo. Prueba login en http://localhost:5173/login"
Write-Host "  participante / $Password  -> /participante"
