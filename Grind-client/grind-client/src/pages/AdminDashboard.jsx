// src/pages/AdminDashboard.jsx
import React from 'react';
import { Box, Typography, Button } from '@mui/material';
import { useSelector, useDispatch } from 'react-redux';
import { logout } from '../redux/authSlice';
import { useNavigate } from 'react-router-dom';

function AdminDashboard() {
    const dispatch = useDispatch();
    const user = useSelector((state) => state.auth.user);
    const navigate = useNavigate();

    const handleLogout = () => {
        dispatch(logout());
        navigate('/', { replace: true });
    };

    return (
        <Box sx={{ p: 4 }}>
            <Typography variant="h4" component="h1" gutterBottom>
                ברוך הבא, {user?.username || 'מנהל יקר'}!
            </Typography>
            <Typography variant="body1">
                זוהי עמוד הדאשבורד של המנהל. התפקיד שלך: {user?.role}
            </Typography>
            <Button variant="contained" color="secondary" onClick={handleLogout} sx={{ mt: 3 }}>
                התנתק
            </Button>
        </Box>
    );
}

export default AdminDashboard;