// src/components/PrivateRoute.jsx
import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';

const PrivateRoute = ({ isAuthenticated, allowedRoles, userRole }) => {
    if (!isAuthenticated) {
        // אם המשתמש אינו מאומת, הפנה אותו לדף ההתחברות.
        return <Navigate to="/" replace />;
    }

    if (allowedRoles && userRole && !allowedRoles.includes(userRole)) {
        // אם המשתמש מאומת אך אין לו את התפקיד המתאים, הפנה אותו לדף 'אין הרשאה'.
        console.warn(`User with role ${userRole} tried to access a route for roles: ${allowedRoles.join(', ')}`);
        return <Navigate to="/unauthorized" replace />; // ניתן לשנות ל-/client-dashboard או כל דף אחר
    }

    // אם המשתמש מאומת ובעל התפקיד המתאים, הצג את הקומפוננטה המוגנת.
    return <Outlet />;
};

export default PrivateRoute;