#!/usr/bin/env bash
# Cria o .env com senhas aleatórias na primeira vez que o Codespace é criado.
# A chave do Gemini não fica no .env: o Docker Compose a lê do secret GEMINI_API_KEY do Codespaces.
set -euo pipefail
cd "$(dirname "$0")/.."

if [ -f .env ]; then
    echo ".env já existe, mantendo as configurações atuais."
    exit 0
fi

aleatorio() { od -An -N"$1" -tx1 /dev/urandom | tr -d ' \n'; }

# A senha do SQL Server exige maiúscula, minúscula, número e símbolo.
cat > .env <<EOF
SA_PASSWORD=Hg@$(aleatorio 12)Aa1
JWT_KEY=$(aleatorio 48)
EOF

echo ".env criado com senhas aleatórias."
