#!/usr/bin/env bash
# Script para iniciar la aplicación Angular Firmeza.Web usando Node.js (v22+)
export PATH="/home/danielillo/snap/antigravity-cli/common/local/bin:$PATH"

cd "$(dirname "$0")/src/Firmeza.Web" || exit 1
echo "==> Usando $(node -v) en $(which node)"
echo "==> Iniciando Angular Frontend (Firmeza.Web) en http://localhost:4200 ..."
npm start -- --host 0.0.0.0 --port 4200
