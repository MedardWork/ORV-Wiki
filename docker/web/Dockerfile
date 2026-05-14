# syntax=docker/dockerfile:1.6
# Build context is the solution root (see docker-compose.yml).

FROM nginx:1.27-alpine

# Static frontend assets.
COPY index.html  /usr/share/nginx/html/index.html
COPY styles.css  /usr/share/nginx/html/styles.css
COPY js/         /usr/share/nginx/html/js/

# nginx's official entrypoint will envsubst this template at container start
# and write the result to /etc/nginx/conf.d/default.conf.
COPY docker/web/nginx.conf.template     /etc/nginx/templates/default.conf.template
# Not a template — loads at http{} scope, defines $connection_upgrade for WS.
COPY docker/web/connection_upgrade.conf /etc/nginx/conf.d/connection_upgrade.conf

# Defaults work for local docker-compose (web → api:8080). Railway overrides
# all three: PORT (auto-injected by platform), API_HOST=api.railway.internal,
# API_PORT=${{api.PORT}}.
ENV PORT=80 \
    API_HOST=api \
    API_PORT=8080 \
    # Restrict envsubst to OUR variables only — without this, nginx's own
    # $host / $remote_addr / etc. in the template would be replaced with
    # empty strings.
    NGINX_ENVSUBST_FILTER="^(PORT|API_HOST|API_PORT)$"

EXPOSE 80
