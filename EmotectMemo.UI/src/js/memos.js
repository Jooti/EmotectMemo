const Memos = ()=>{
    const emoCrypt = new Crypto();
    return {
        key: "",
        content: [],
        hasContent: false,
        isLoading: true,
        emoword: "",
        decrypted: false,
        emowordIsIncorrect: false,
        HEADER:{
            title: 'Emotect Memo',
            icon: 'img/emotect.svg',
            darkIcon: 'img/darkemotect.svg'
        },
        PAGES: {
            LOADING: {key: 'loading', icon: '', darkIcon: ''}, 
            HOME: {key: 'home', icon: '', darkIcon: ''}, 
            NEW: {key: 'new', icon: 'img/new.svg', darkIcon: 'img/darknew.svg'}, 
            VERIFY_EMOCODE: {key: 'emocode', icon: 'img/emocode.svg', darkIcon: 'img/darkemocode.svg'},  
            SHOW_CONTENT: {key: 'content', icon: '', darkIcon: ''}
        },
        init() {
            if (window.location.pathname == "/"){
                this.isLoading = false;
                return;
            }
            fetch(`${baseURL}/exists`)
                .then(response => response.json())
                .then(response => {
                    this.isLoading = false;
                    this.hasContent = response.exists;
                    this.key = window.location.pathname;
                })
                .catch((exception) => {
                    console.log(exception);
                });
        },
        get currentPage(){
            if (this.isLoading){
                return this.PAGES.LOADING;
            }
            if (this.key == ''){
                return this.PAGES.HOME;
            }
            //else
            if (!this.hasContent){
                return this.PAGES.NEW;
            }
            //else
            if (!this.decrypted){
                return this.PAGES.VERIFY_EMOCODE;
            }
            //else
            return this.PAGES.SHOW_CONTENT;
        },
        async post(memo) {
            if (!memo)
            {
                memo = welcomeMessage;
            }
            await emoCrypt.setPassword(this.emoword);
            (async () => {
                const rawResponse = await fetch(baseURL, {
                    method: 'POST',
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json',
                        'Secret-Key' : emoCrypt.secretKey
                    },
                    body: JSON.stringify({ body: await emoCrypt.encrypt(memo) })
                });
                if (rawResponse.status == 401){
                    this.emoword = "";
                    this.content = [];
                    this.decrypted = false;
                    this.emowordIsIncorrect = true;
                    return false;
                }
                if (rawResponse.status == 400)
                {
                    return false;
                }
                if (rawResponse.status == 200){
                    const response = await rawResponse.json();
                    this.content.unshift({
                        id: response.id,
                        body: memo
                    });
                    this.hasContent = true;
                    this.decrypted = true;
                    this.$dispatch('memochange');
                }
                return true;
            })();
        },
        async deleteMemo(memoId) {
            if (!memoId)
            {
                return false
            }
            await emoCrypt.setPassword(this.emoword);
            (async () => {
                const rawResponse = await fetch(baseURL + `/${memoId}`, {
                    method: 'DELETE',
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json',
                        'Secret-Key' : emoCrypt.secretKey
                    }
                });
                if (rawResponse.status == 401){
                    this.emoword = "";
                    this.content = [];
                    this.decrypted = false;
                    this.emowordIsIncorrect = true;
                    return false;
                }
                if (rawResponse.status == 200){
                    this.content.splice(this.content.findIndex(x=>x.id == memoId), 1);
                    this.$dispatch('memochange');
                }
                return true;
            })();
        },
        async getPage(){
            this.emowordIsIncorrect = false;
            await emoCrypt.setPassword(this.emoword);
            (async () => {
                const rawResponse = await fetch(baseURL, {
                    method: 'GET',
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json',
                        'Secret-Key' : emoCrypt.secretKey
                    }});
                if (rawResponse.status == 401){
                    this.emowordIsIncorrect = true;
                    return;
                }
                const response = await rawResponse.json();
                this.content = [];
                for (let i = 0; i < response.content.length; i++) {
                    const c = response.content[i]
                    this.content.unshift({
                        id: c.id,
                        body: await emoCrypt.decrypt(c.body)
                    });
                }
                this.hasContent = true;
                this.decrypted = true;
            })();
        }
    };
}
