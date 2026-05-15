#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION="$ROOT_DIR/SistemaCotizaciones.sln"
WEB_PROJECT="$ROOT_DIR/SistemaCotizaciones.Web/SistemaCotizaciones.Web.csproj"
PUBLISH_DIR="/tmp/sistema-cotizaciones-publish"
PROD_DIR="/opt/sistema-cotizaciones"
SERVICE_NAME="sistema-cotizaciones"
LOCAL_URL="http://127.0.0.1:5082/login"
HOST_HEADER="cotizaciones.jahmantencion.cl"

echo "==> Compilando solucion"
dotnet build "$SOLUTION"

echo "==> Publicando proyecto web en $PUBLISH_DIR"
rm -rf "$PUBLISH_DIR"
dotnet publish "$WEB_PROJECT" -c Release -o "$PUBLISH_DIR"

echo "==> Validando permisos sudo"
sudo -v

echo "==> Deteniendo servicio $SERVICE_NAME"
sudo systemctl stop "$SERVICE_NAME"

echo "==> Copiando archivos a $PROD_DIR"
sudo rsync -av --delete \
  --exclude 'App_Data/' \
  --exclude 'wwwroot/pdfs/' \
  "$PUBLISH_DIR/" "$PROD_DIR/"

echo "==> Ajustando permisos"
sudo chown -R www-data:www-data "$PROD_DIR"
sudo chmod -R u+rwX,g+rX "$PROD_DIR"

echo "==> Iniciando servicio $SERVICE_NAME"
sudo systemctl start "$SERVICE_NAME"

echo "==> Estado del servicio"
sudo systemctl --no-pager --full status "$SERVICE_NAME"

echo "==> Validando respuesta local"
curl -i -H "Host: $HOST_HEADER" "$LOCAL_URL"

echo
echo "Deploy terminado. Revisa en: https://$HOST_HEADER/login"
