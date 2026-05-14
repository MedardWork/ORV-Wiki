# syntax=docker/dockerfile:1.6
# Build context is the solution root (see docker-compose.yml).

FROM nginx:1.27-alpine

# Static frontend assets.
COPY orv-wiki-frontend.html /usr/share/nginx/html/index.html
COPY styles.css              /usr/share/nginx/html/styles.css
COPY js/                     /usr/share/nginx/html/js/

# nginx config — serves static files, proxies /api and /hubs to the API.
COPY docker/web/nginx.conf              /etc/nginx/conf.d/default.conf
COPY docker/web/connection_upgrade.conf /etc/nginx/conf.d/connection_upgrade.conf

EXPOSE 80
