import { createSlice } from '@reduxjs/toolkit';

// פונקציית עזר לטעינת מצב ראשוני מ-localStorage
const loadStateFromLocalStorage = () => {
    try {
        const serializedState = localStorage.getItem('auth');
        if (serializedState === null) {
            return undefined; // אם אין נתונים, החזר undefined כדי שה-reducer ישתמש ב-initialState
        }
        return JSON.parse(serializedState);
    } catch (e) {
        console.error("Failed to load state from localStorage:", e);
        return undefined;
    }
};

// פונקציית עזר לשמירת מצב ל-localStorage
const saveStateToLocalStorage = (state) => {
    try {
        const serializedState = JSON.stringify(state);
        localStorage.setItem('auth', serializedState);
    } catch (e) {
        console.error("Failed to save state to localStorage:", e);
    }
};

const authSlice = createSlice({
    name: 'auth',
    // טעינה ראשונית מ-localStorage או ברירת מחדל
    initialState: loadStateFromLocalStorage() || {
        user: null, // יכיל: { userId, username, role, token, refreshToken }
        isAuthenticated: false,
        isLoading: true, // לטעינה ראשונית מ-localStorage
    },
    reducers: {
        // פעולה להתחברות
        loginSuccess: (state, action) => {
            const { token, refreshToken, role, userId, username } = action.payload; // הוספתי username
            state.user = {
                userId: userId, // 💡 תיקון: שמור את המזהה במפתח 'userId' כדי שיתאים ל-ProfilePage
                role: role,
                token: token,
                refreshToken: refreshToken,
                username: username // שמירת username
            };
            state.isAuthenticated = true;
            saveStateToLocalStorage(state); // שמור מצב מעודכן ל-localStorage
        },
        // פעולה להתנתקות
        logout: (state) => {
            state.user = null;
            state.isAuthenticated = false;
            localStorage.removeItem('auth'); // נקה מ-localStorage
        },
        // פעולה לציון סיום טעינה ראשונית (מ-localStorage)
        setLoading: (state, action) => {
            state.isLoading = action.payload;
        }
    },
});

export const { loginSuccess, logout, setLoading } = authSlice.actions;

export default authSlice.reducer;
