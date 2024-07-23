Emotect Memo
=============
EmotectMemo is a minimal end-to-end encrypted text/note sharing app, protected by Emoji passwords. We strongly recommend using Emoji passwords, because there are a lot of Emojis, way more than letters, and it would make your passwords stronger; but for the sake of liberty, you are free to choose any password you like. It's your data, your choice.

EmotectMemo is password protected but it has no username. The data is stored both encrypted and anonymous. Each page has its unique URL and Emoword. Anyone knowing both, is the owner of the page.

No kind of Emoword or URL recovery is available. EmotectMemo stores no data to verify your ownership.
The demo of the application is available on emotect.me.


The demo of the application is avaiable on [emotect.me]. 

Self Host
------
The following steps to host Emotect Memo on your server:
1. Clone this repository:
    ```
    git clone https://github.com/Jooti/EmotectMemo.git
    ```
2. Edit .yml file: there are two .yml files, the configurations you most likely are about to change are in docker-compose.sample.dev, you need to rename it to something like docker-compose.production.dev and edit it to your needs.

    ```
    cd EmotectMemo/
    cp docker-compose.sample.yml docker-compose.production.yml
    vim docker-compose.production.yml
    ```

3. Configuration:

    - &lt;MongoAdmin&gt;: Replace this value with the admin username you want to set for the mongodb
    - &lt;MongoPassword&gt;: Replace this value with the admin password you want to set for the mongodb
    - FRONTEND_URL: Let the backend know to whom it should respond (CORS).
    - EMOTECTAPI: The URL address of backend.

4. Build:

    Use the following command to build docker images and run them:
    ```
    docker compose -f "./docker-compose.yml" -f "./docker-compose.production.yml"  up -d  --build
    ```
5. Reverce proxy and https (optional):
    Reverce proxt and https using haproxy. If you don't have haproxy installed, install haproxy using following code:
    ````
    sudo apt install haproxy
    ````
    And then the edit config file:
    ```
    sudo vim /etc/haproxy/haproxy.cfg
    ```
    And add following code to the config
    ```
    frontend web
        bind :80
        bind :443 ssl crt **yourdomain-cert.pem**
        http-request redirect scheme https unless { ssl_fc }

        acl emotapi hdr(host) -i **backend-url eg. api.example.com**
        use_backend emotectmemo-container if emotapi

        acl emotect hdr(host) -i **frontend-url eg. example.com**
        use_backend emotectmemosite-container if emotect

    backend emotectmemo-container
        server static 127.0.0.1:62000 check


    backend emotectmemosite-container
        server static 127.0.0.1:9090 check

    ```
    Reload haproxy config and you should be ready to go
    ```
    sudo service haproxy reload
    ```

About Encryption
------
The end-to-end encryption is completely done in the front end of the Emotect memo and the back end has no way to decrypt the memos. The encryption algorithm is [AES-GCM] and the encryption key is SHA256 of the Emoword.
We didn't want to send the encryption key to the server but then there was a problem with authenticating the client and knowing if they have the correct Emoword before sending the encrypted content to them. So half of the encryption key is digested using SHA256 and sent to the server as a secret/verification key.



[Emotect.me]: https://emotect.me
[AES-GCM]: https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/encrypt#aes-gcm