#!/bin/bash

set -e

echo "[entrypoint] MidIAProjeto container starting..."

# Require an explicit connection string. In Docker this is wired by compose.yaml.
: "${ConnectionStrings__DefaultConnection:?ConnectionStrings__DefaultConnection must be set (see compose.yaml)}"

echo "[entrypoint] Applying EF Core migrations via ./efbundle..."
# ./efbundle is idempotent: it exits 0 quickly when nothing is pending.
./efbundle --connection "$ConnectionStrings__DefaultConnection"

echo "[entrypoint] Launching MidIAProjeto..."
exec dotnet MidIAProjeto.dll "$@"
