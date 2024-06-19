FROM node:22 as build
WORKDIR /node
COPY EmotectMemo.UI/package.json ./
RUN npm install
#RUN npx update-browserslist-db@latest
COPY "EmotectMemo.UI/src" ./src
COPY EmotectMemo.UI/tailwind.config.js ./
RUN npx tailwindcss -i ./src/css/tailwindstyles.css -o ./src/css/styles.css
ARG EMOTECTAPI=http://localhost:62000
RUN cd ./src/js && echo "const EMOTECTMEMO_BASE_URL = '${EMOTECTAPI}'" > env.js

COPY . .

FROM nginx:alpine as final
COPY "EmotectMemo.UI/nginx.conf" /etc/nginx/nginx.conf
COPY --from=build "/node" /code
EXPOSE 80 

ENTRYPOINT ["nginx", "-g", "daemon off;"]
