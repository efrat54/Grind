// src/redux/store.js
import { configureStore } from '@reduxjs/toolkit';
import authReducer from './authSlice';
import preferencesReducer from './preferencesSlice';

export const store = configureStore({
    reducer: {
        auth: authReducer,
        preferences: preferencesReducer,
        // ... כאן אפשר להוסיף reducers נוספים בהמשך
    },
    // Redux Toolkit כבר מגדיר DevTools אוטומטית בסביבת פיתוח
    // וגם את middleware כמו redux-thunk.
});