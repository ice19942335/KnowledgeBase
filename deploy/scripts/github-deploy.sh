#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

if [[ ! -f .env ]]; then
  echo "Missing deploy/.env — GitHub Actions must create it from Environment secrets."
  exit 1
fi

compose() {
  docker compose -f docker-compose.yml -f docker-compose.traefik.yml --env-file .env "$@"
}

echo "Cleaning up orphan containers from interrupted deploys..."
docker ps -a --format '{{.Names}}' | grep -E '^[0-9a-f]+_deploy-' | xargs -r docker rm -f || true

echo "Stopping previous stack..."
compose down --remove-orphans

echo "Building and starting stack..."
compose up -d --build --remove-orphans

echo "Waiting for API (GET /api/documents via gateway)..."
for attempt in $(seq 1 30); do
  if compose exec -T reverse-proxy wget -q -S -O /dev/null http://gateway:8080/api/documents 2>&1 | grep -q ' 200 OK'; then
    echo "Smoke test passed."
    exit 0
  fi

  echo "Attempt ${attempt}/30 — API not ready yet, retrying in 3s..."
  sleep 3
done

echo "Smoke test failed — gateway /api/documents did not return 200."
compose ps
compose logs gateway --tail 40
exit 1
