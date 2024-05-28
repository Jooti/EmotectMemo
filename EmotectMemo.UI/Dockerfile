FROM node:22 as build
WORKDIR /node
COPY EmotectMemo.UI/package.json ./
RUN npm install
#RUN npx update-browserslist-db@latest
COPY "EmotectMemo.UI/src" ./src
COPY EmotectMemo.UI/tailwind.config.js ./
RUN npx tailwindcss -i ./src/css/tailwindstyles.css -o ./src/css/styles.css

COPY . .

FROM nginx:alpine as final
COPY "EmotectMemo.UI/nginx.conf" /etc/nginx/nginx.conf
COPY --from=build "/node" /usr/share/nginx/code
EXPOSE 80 

ENTRYPOINT ["nginx", "-g", "daemon off;"]
