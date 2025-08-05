// src/api/axiosConfig.js
import axios from 'axios';
import { store } from '../redux/store'; // ייבוא ה-Redux store
import { logout } from '../redux/authSlice'; // ייבוא פעולת התנתקות

const api = axios.create({
    // 💡 ודא שזהו ה-URL הנכון של השרת שלך!
    // בדוק את קובץ 'launchSettings.json' בפרויקט ה-Backend שלך
    // כדי לוודא שהפורט (לדוגמה 7251) תואם לפורט שבו ה-Backend רץ.
    baseURL: 'https://localhost:7251/api', 
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request Interceptor: הוספת טוקן לכל בקשה
api.interceptors.request.use(
    (config) => {
        const state = store.getState();
        const token = state.auth.user?.token; // קבל את הטוקן מ-Redux State

        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Response Interceptor: טיפול ב-401 (Unauthorized)
api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;
        // אם זו שגיאה 401 ולא ניסינו כבר לרענן את הטוקן/התנתקנו
        if (error.response.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true; // סמן שניסינו לטפל ב-401 פעם אחת

            // לוגיקת ריפרש טוקן תהיה כאן אם תיישמי אותה בעתיד.
            // נכון לעכשיו, פשוט נתנתק את המשתמש.
            console.warn("401 Unauthorized: Token might be expired or invalid. Logging out.");
            store.dispatch(logout()); // נתק את המשתמש מ-Redux
            window.location.href = '/'; // נווט לדף ההתחברות
            return Promise.reject(error); // זרוק את השגיאה הלאה
        }
        return Promise.reject(error);
    }
);

export default api;
