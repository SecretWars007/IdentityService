#!/bin/bash
set -e

echo "Applying database migrations..."
dotnet ef database update --startup-project IdentityService.API
echo "Migrations applied"

exec "$@"
