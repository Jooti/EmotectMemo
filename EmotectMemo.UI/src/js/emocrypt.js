const Crypto = class {
    #password;
    #encryptionKey;
    #secretKey;
    constructor(){
    }

    static async Create(password)
    {
        const crypto = new Crypto();
        await crypto.setPassword(password);
        return crypto;
    }

    async setPassword(password){
        if (password != this.#password)
        {
            this.#password = password;
            this.#encryptionKey = await this.createEncryptionKey(this.#password);
            this.#secretKey = await this.createSecretKey(this.#encryptionKey);
        }
    }

    get secretKey(){
        return this.#secretKey;
    }

    encode(text){
        const encoder = new TextEncoder();
        return encoder.encode(text);
    }
    async digestSHA256(message){
        const data = this.encode(message);
        return await window.crypto.subtle.digest("SHA-256", data);
    }

    async createEncryptionKey(password){
        return await this.digestSHA256(password);
    }

    async createSecretKey(encryptionKey){
        //We need a unique key to set as password in the server 
        //  to make sure the user has correct Emoword and to 
        //  be sure that the Emoword is not retrievable having
        //  this key.
        //Therefor SHA of Half of SHA string of the Emoword is used as the key.
        const keybase = this.arrayBufferToString(encryptionKey);
        const halfOfKey = keybase.substring(0, Math.floor(keybase.length/2)+1);
        const secretKeyBuff = await this.digestSHA256(halfOfKey);
        return btoa(this.arrayBufferToString(secretKeyBuff));
    }

    arrayBufferToString(arraybuffer){
        const hashArray = Array.from(new Uint8Array(arraybuffer));
        return hashArray.map(byte => String.fromCharCode(byte)).join('');
    }
    stringToUint8Array(text){
        return new Uint8Array(Array.from(text).map(ch => ch.charCodeAt(0))); 
    }

    async encrypt(plainText){
        const encoded = this.encode(plainText);

        const iv = crypto.getRandomValues(new Uint8Array(12));  
        const algorithm = { name: "AES-GCM", iv: iv };
        const key = await crypto.subtle.importKey('raw', 
            this.#encryptionKey, algorithm, false, ['encrypt']);

            const encrypted = await window.crypto.subtle.encrypt(
            algorithm,
            key,
            encoded,
        );

        const encryptedMessage =btoa(
            this.arrayBufferToString(iv) + 
            this.arrayBufferToString(encrypted));
        return encryptedMessage;
    }

    async decrypt(encryptedMessage){
        const encrypted = atob(encryptedMessage);
        const iv = this.stringToUint8Array(encrypted.slice(0,12));
        const message = this.stringToUint8Array(encrypted.slice(12));

        const algorithm = { name: "AES-GCM", iv: iv };
        const key = await crypto.subtle.importKey('raw', 
            this.#encryptionKey, algorithm, false, ['decrypt']);

        try {
            const plainBuff = await crypto.subtle.decrypt(
                algorithm, 
                key, 
                message);  
            return this.arrayBufferToString(plainBuff);
        } catch (error) {
            return "Failed to decrypt message."
        }
    }
}
