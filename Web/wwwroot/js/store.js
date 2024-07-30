import { apiUrl, webUrl } from './apiConfig.js';

const userStateManager = (function () {
    let state = {
        user: undefined,
    };

    async function fetchUserCredentials() {
        try {
            const { data } = await axios.get(apiUrl + `api/users/credentials`, {
                withCredentials: true
            });
        
            return data;   
        } catch (error) {
            window.location.href = webUrl;
        }
    }

    async function init() {
        if (state.user === undefined) {
            state.user = await fetchUserCredentials();
        }
    };

    function getUser() {
        return state.user;
    }

    return {
        init,
        getUser
    };
})();

const supplierStateManager = (function () {
    let state = {
        supplier: undefined,
    };

    async function fetchSupplierCredentials() {
        const { data } = await axios.get(apiUrl + `api/suppliers/credentials`, {
			withCredentials: true
		});
    
        return { ...data };
    }

    async function init() {
        if (state.supplier === undefined) {
            state.supplier = await fetchSupplierCredentials();
        }
    };

    function getSupplier() {
        return state.supplier;
    }

    return {
        init,
        getSupplier
    };
})();

export default userStateManager;

export {
    supplierStateManager
};
