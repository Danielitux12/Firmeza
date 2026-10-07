#!/usr/bin/env bash
# Script para iniciar el cliente Angular usando la versión compatible de Node.js (v22+)
export PATH="/home/danielillo/snap/antigravity-cli/common/local/bin:$PATH"

cd "$(dirname "$0")/src/Firmeza.Client" || exit 1
echo "==> Usando $(node -v) en $(which node)"
echo "==> Iniciando Angular Frontend en http://localhost:4200 ..."
npm start
