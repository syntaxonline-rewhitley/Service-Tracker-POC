# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM node:22-alpine AS build

WORKDIR /app

# Install dependencies first (layer cached unless package files change)
COPY package.json package-lock.json* ./
RUN npm ci

# Copy source and build
COPY . .
# VITE_API_URL is baked into the JS bundle at build time by Vite.
# Pass it as a build arg: docker build --build-arg VITE_API_URL=https://api.example.com/api
ARG VITE_API_URL
ENV VITE_API_URL=${VITE_API_URL}
RUN npm run build

# ── Stage 2: Serve ────────────────────────────────────────────────────────────
FROM nginx:1.27-alpine AS final

# Copy nginx config as a template — nginx entrypoint runs envsubst on it at startup
COPY nginx.conf /etc/nginx/templates/default.conf.template

# Copy Vite build output
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
