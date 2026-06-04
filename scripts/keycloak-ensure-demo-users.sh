#!/usr/bin/env bash
# Crea usuarios demo en Keycloak si faltan. Requiere: docker compose up keycloak -d
set -euo pipefail

CONTAINER="${KEYCLOAK_CONTAINER:-umbral_keycloak}"
REALM="${KEYCLOAK_REALM:-umbral}"
PASSWORD="${KEYCLOAK_DEMO_PASSWORD:-Umbral123!}"

kcadm() {
  docker exec "$CONTAINER" /opt/keycloak/bin/kcadm.sh "$@"
}

if ! docker ps --filter "name=$CONTAINER" --filter "status=running" -q | grep -q .; then
  echo "Error: contenedor $CONTAINER no está en ejecución. Ejecuta: docker compose up keycloak -d" >&2
  exit 1
fi

echo "Conectando kcadm..."
kcadm config credentials --server http://localhost:8080 --realm master --user admin --password admin >/dev/null

ensure_role() {
  local role="$1"
  if ! kcadm get roles -r "$REALM" 2>/dev/null | grep -q "\"name\" : \"$role\""; then
    echo "Creando rol $role..."
    kcadm create roles -r "$REALM" -s "name=$role" >/dev/null
  fi
}

ensure_user() {
  local username="$1" email="$2" first="$3" last="$4" role="$5"
  ensure_role "$role"
  if kcadm get users -r "$REALM" -q "username=$username" 2>/dev/null | grep -q '"id"'; then
    kcadm add-roles -r "$REALM" --uusername "$username" --rolename "$role" >/dev/null 2>&1 || true
    echo "OK  $username ya existe (rol $role verificado)"
    return
  fi
  echo "Creando $username..."
  kcadm create users -r "$REALM" \
    -s "username=$username" -s enabled=true -s "email=$email" \
    -s emailVerified=true -s "firstName=$first" -s "lastName=$last" >/dev/null
  kcadm set-password -r "$REALM" --username "$username" --new-password "$PASSWORD" >/dev/null
  kcadm add-roles -r "$REALM" --uusername "$username" --rolename "$role" >/dev/null
  echo "OK  $username ($role)"
}

ensure_user admin admin@umbral.local Admin UMBRAL Administrador
ensure_user operador operador@umbral.local Operador UMBRAL Operador
ensure_user participante participante@umbral.local Participante UMBRAL Participante

echo ""
echo "Listo. Login: participante / $PASSWORD -> http://localhost:5173/participante"
