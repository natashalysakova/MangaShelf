git pull
docker compose pull
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env up -d