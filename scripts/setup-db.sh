#!/usr/bin/env bash
set -euo pipefail

: "${MSSQL_SA_PASSWORD:?Configure the MSSQL_SA_PASSWORD Codespaces secret}"

export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=ModernBackendLabDb;User Id=sa;Password=${MSSQL_SA_PASSWORD};Encrypt=True;TrustServerCertificate=True"

docker compose up -d sqlserver

echo "Waiting for SQL Server..."
for i in {1..60}; do
  if docker exec efcore-sqlserver \
    /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "SELECT 1" >/dev/null 2>&1; then
    break
  fi

  if [ "$i" -eq 60 ]; then
    echo "SQL Server did not become ready."
    docker compose logs sqlserver
    exit 1
  fi

  sleep 2
done

export PATH="$PATH:$HOME/.dotnet/tools"

dotnet tool install --global dotnet-ef --version 10.0.12 \
  || dotnet tool update --global dotnet-ef --version 10.0.12

dotnet ef database update \
  --project ModernBackendLab \
  --startup-project ModernBackendLab

echo "SQL Server and EF Core database are ready."
