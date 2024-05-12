FROM node:22 as build
WORKDIR /node
COPY EmotectMemo.UI/package.json ./
RUN npm install
COPY "EmotectMemo.UI/src" ./src
RUN npx tailwindcss -i ./src/css/tailwindstyles.css -o ./src/css/styles.css

COPY ./EmotectMemo.UI .

FROM nginx:alpine as final
COPY "EmotectMemo.UI/nginx.conf" /etc/nginx/nginx.conf
COPY --from=build "/node" /usr/share/nginx/code
EXPOSE 80 

ENTRYPOINT ["nginx", "-g", "daemon off;"]
