import React from 'react';
import { AppBar, Toolbar, Typography, Button, Box } from '@mui/material';
import { Link, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { logout } from '../redux/authSlice';

function Navbar() {
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);
    const user = useSelector((state) => state.auth.user);

    const handleLogout = () => {
        dispatch(logout());
        navigate('/login');
    };

    return (
        <AppBar position="static" sx={{ backgroundColor: '#2196f3' }}>
            <Toolbar>
                <Typography
                    variant="h6"
                    component="div"
                    sx={{ flexGrow: 1, cursor: 'pointer' }}
                    onClick={() => {
                        if (isAuthenticated && user) {
                            if (user.role === 'Client') navigate('/client-dashboard');
                            else if (user.role === 'Trainer') navigate('/trainer-dashboard');
                            else if (user.role === 'Admin') navigate('/admin-dashboard');
                        } else {
                            navigate('/');
                        }
                    }}
                >
                    Grind 💪
                </Typography>

                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    {isAuthenticated ? (
                        <>
                            <Button
                                color="inherit"
                                onClick={() => {
                                    if (user.role === 'Client') navigate('/client/profile');
                                    else if (user.role === 'Trainer') navigate('/trainer/profile');
                                    else if (user.role === 'Admin') navigate('/admin/profile');
                                }}
                                sx={{ mr: 1 }}
                            >
                                פרופיל
                            </Button>

                            <Button color="inherit" onClick={handleLogout}>
                                התנתק
                            </Button>
                        </>
                    ) : (
                        <>
                            {/* אם עושים מסך בית יפה- משם לפתוח את המסך הראשון כרגע ע"י הכפתורים האלו */}
                            {/* <Button color="inherit" component={Link} to="/login" sx={{ mr: 1 }}>
                כניסה
              </Button>
              <Button color="inherit" component={Link} to="/register">
                הרשמה
              </Button> */}
                        </>
                    )}
                </Box>
            </Toolbar>
        </AppBar>
    );
}

export default Navbar;
