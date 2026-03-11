# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY fluxo-caixa-app/package*.json ./
RUN npm ci
COPY fluxo-caixa-app/ ./
RUN npm run build -- --configuration production

# Production stage
FROM nginx:alpine
COPY --from=build /app/dist/fluxo-caixa-app/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
