
# Etapa 1 — Build do Angular
FROM node:22-alpine AS build

WORKDIR /app

# Instala as dependências
COPY package*.json ./
RUN npm ci

# Copia o projeto
COPY . .

# Gera o build de produção
RUN npm run build

# Etapa 2 — Nginx
FROM nginx:stable-alpine AS final

# Remove a configuração padrão
RUN rm /etc/nginx/conf.d/default.conf

# Copia a configuração personalizada
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copia os arquivos compilados do Angular
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
