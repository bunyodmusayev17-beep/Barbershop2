#!/bin/sh
# Kunlik zaxira (cron): baza + yuklangan rasmlar, oxirgi 7 kun saqlanadi.
set -eu
cd /opt/knyazbarber
mkdir -p backups
stamp=$(date +%Y-%m-%d)
docker compose exec -T db pg_dump -U knyazbarber -d knyazbarber | gzip > "backups/db-$stamp.sql.gz"
tar -czf "backups/uploads-$stamp.tar.gz" uploads
find backups -type f -mtime +7 -delete
