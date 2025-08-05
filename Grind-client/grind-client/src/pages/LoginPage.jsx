import React, { useState, useEffect } from 'react';
import { Box, Typography, Container, TextField, Button, Link, Dialog, DialogTitle, DialogContent, DialogActions, CircularProgress, Alert, IconButton, FormControl, InputLabel, MenuItem, Select, Checkbox, ListItemText, } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { useNavigate } from 'react-router-dom';
import api from '../api/axiosConfig';
import { useDispatch, useSelector } from 'react-redux';
import { loginSuccess, logout, setLoading } from '../redux/authSlice';
import { selectDaysOfWeek, selectCategories } from '../redux/preferencesSlice';

function LoginPage() {
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);
    const userRole = useSelector((state) => state.auth.user?.role);
    const isLoadingAuth = useSelector((state) => state.auth.isLoading);
    const daysOfWeek = useSelector(selectDaysOfWeek);
    const categories = useSelector(selectCategories);

    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoadingState] = useState(false);
    const [error, setError] = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    const [openRegisterDialog, setOpenRegisterDialog] = useState(false);
    const [registerForm, setRegisterForm] = useState({
        username: '',
        password: '',
        confirmPassword: '',
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        dateOfBirth: '',
        street: '',
        city: '',
        apartmentNumber: '',
        preferredDifficulty: '',
        preferredDay: '',
        startTime: '',
        endTime: '',
        preferredClassCategory: [],
    });
    const [registerLoading, setRegisterLoading] = useState(false);
    const [registerError, setRegisterError] = useState('');
    const [registerSuccess, setRegisterSuccess] = useState('');

    useEffect(() => {
        if (!isLoadingAuth && isAuthenticated) {
            if (userRole === 'Client') {
                navigate('/client-dashboard', { replace: true });
            } else if (userRole === 'Trainer') {
                navigate('/trainer-dashboard', { replace: true });
            } else if (userRole === 'Admin') {
                navigate('/admin-dashboard', { replace: true });
            } else {
                setError(`תפקיד משתמש לא ידוע: "${userRole}". אנא פנה למנהל המערכת.`);
                dispatch(logout());
            }
        }
        if (isLoadingAuth && !isAuthenticated) {
            dispatch(setLoading(false));
        }
    }, [isAuthenticated, userRole, navigate, isLoadingAuth, dispatch]);

    const handleLogin = async (e) => {
        e.preventDefault();
        setLoadingState(true);
        setError('');
        setSuccessMessage('');

        try {
            const response = await api.post('/Auth/login', { username, password });
            const { token, refreshToken, role, userId } = response.data;

            dispatch(loginSuccess({ token, refreshToken, role, userId, username }));

            setSuccessMessage('התחברת בהצלחה!');

            if (role === 'Client') {
                navigate('/client-dashboard', { replace: true });
            } else if (role === 'Trainer') {
                navigate('/trainer-dashboard', { replace: true });
            } else if (role === 'Admin') {
                navigate('/admin-dashboard', { replace: true });
            } else {
                setError(`תפקיד משתמש לא ידוע: "${role}". אנא פנה למנהל המערכת.`);
                dispatch(logout());
            }
        } catch (err) {
            setError(err.response?.data?.message || err.response?.data || 'שם משתמש או סיסמה שגויים');
            dispatch(logout());
        } finally {
            setLoadingState(false);
        }
    };


    const handleRegisterChange = (e) => {
        const { name, value } = e.target;
        setRegisterForm({ ...registerForm, [name]: value });
    };

    const handleRegisterSubmit = async (e) => {
        e.preventDefault();
        setRegisterLoading(true);
        setRegisterError('');
        setRegisterSuccess('');

        try {
            const response = await api.post('/Auth/register', {
                username: registerForm.username,
                password: registerForm.password,
                confirmPassword: registerForm.confirmPassword,
                firstName: registerForm.firstName,
                lastName: registerForm.lastName,
                email: registerForm.email,
                phoneNumber: registerForm.phone,
                dateOfBirth: registerForm.dateOfBirth,
                address: {
                    street: registerForm.street,
                    city: registerForm.city,
                    apartmentNumber: registerForm.apartmentNumber,
                },
                preferredDifficulty: registerForm.preferredDifficulty,
                preferredTimes: [
                    {
                        day: registerForm.preferredDay,
                        startTime: registerForm.startTime,
                        endTime: registerForm.endTime,
                    },
                ],
                preferredClasses: registerForm.preferredClassCategory.map((category) => ({
                    classCategoryName: category,
                })),

            });

            setRegisterSuccess('ההרשמה הושלמה בהצלחה!');
            setRegisterForm({
                username: '',
                password: '',
                confirmPassword: '',
                firstName: '',
                lastName: '',
                email: '',
                phone: '',
                dateOfBirth: '',
                street: '',
                city: '',
                apartmentNumber: '',
                preferredDifficulty: '',
                preferredDay: '',
                startTime: '',
                endTime: '',
                preferredClassCategory: [],
            });

            navigate('/');
            setOpenRegisterDialog(false);
        } catch (err) {
            const errorData = err.response?.data;
            let errorMessage = 'שגיאה ברישום המשתמש.';

            if (typeof errorData === 'string') {
                errorMessage = errorData;
            } else if (errorData?.message) {
                errorMessage = errorData.message;
            } else if (errorData?.errors) {
                errorMessage = Object.values(errorData.errors).flat().join('\n');
            }

            setRegisterError(errorMessage);
        } finally {
            setRegisterLoading(false);
        }
    };

    const handleOpenRegisterDialog = () => setOpenRegisterDialog(true);
    const handleCloseRegisterDialog = () => {
        setOpenRegisterDialog(false);
        setRegisterError('');
        setRegisterSuccess('');
    };

    return (
        <Container component="main" maxWidth="xs" sx={{ mt: 8 }}>
            <Box
                sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    p: 3,
                    border: '1px solid #ccc',
                    borderRadius: '8px',
                    boxShadow: '0px 4px 10px rgba(0, 0, 0, 0.1)',
                    backgroundColor: 'white',
                }}
            >
                <Typography component="h1" variant="h5">
                    התחברות
                </Typography>
                <Box component="form" onSubmit={handleLogin} noValidate sx={{ mt: 1 }}>
                    <TextField
                        margin="normal"
                        required
                        fullWidth
                        id="username"
                        label="שם משתמש"
                        name="username"
                        autoComplete="username"
                        autoFocus
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                    />
                    <TextField
                        margin="normal"
                        required
                        fullWidth
                        name="password"
                        label="סיסמה"
                        type="password"
                        id="password"
                        autoComplete="current-password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        sx={{ mt: 3, mb: 2 }}
                        disabled={loading}
                    >
                        {loading ? <CircularProgress size={24} /> : 'התחבר'}
                    </Button>
                    {error && (
                        <Alert severity="error" sx={{ mb: 2 }}>
                            {error}
                        </Alert>
                    )}
                    {successMessage && (
                        <Alert severity="success" sx={{ mb: 2 }}>
                            {successMessage}
                        </Alert>
                    )}
                    <Link
                        href="#"
                        variant="body2"
                        onClick={(e) => {
                            e.preventDefault();
                            handleOpenRegisterDialog();
                        }}
                        sx={{ textAlign: 'center', display: 'block' }}
                    >
                        אין לך חשבון? הירשם כאן
                    </Link>
                </Box>
            </Box>

            {/* Register Dialog */}
            <Dialog open={openRegisterDialog} onClose={handleCloseRegisterDialog}>
                <DialogTitle>
                    <Box display="flex" justifyContent="space-between" alignItems="center">
                        רישום לקוח חדש
                        <IconButton aria-label="close" onClick={handleCloseRegisterDialog}>
                            <CloseIcon />
                        </IconButton>
                    </Box>
                </DialogTitle>
                <DialogContent dividers>
                    <form onSubmit={handleRegisterSubmit}>
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="שם משתמש"
                            name="username"
                            value={registerForm.username}
                            onChange={handleRegisterChange}
                        />
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="סיסמה"
                            name="password"
                            type="password"
                            value={registerForm.password}
                            onChange={handleRegisterChange}
                        />
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="אימות סיסמה"
                            name="confirmPassword"
                            type="password"
                            value={registerForm.confirmPassword || ''}
                            onChange={handleRegisterChange}
                            error={
                                registerForm.confirmPassword &&
                                registerForm.password !== registerForm.confirmPassword
                            }
                            helperText={
                                registerForm.confirmPassword &&
                                    registerForm.password !== registerForm.confirmPassword
                                    ? 'הסיסמה ואימות הסיסמה אינן תואמות'
                                    : ''
                            }
                        />
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="שם פרטי"
                            name="firstName"
                            value={registerForm.firstName}
                            onChange={handleRegisterChange}
                        />
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="שם משפחה"
                            name="lastName"
                            value={registerForm.lastName}
                            onChange={handleRegisterChange}
                        />
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="אימייל"
                            name="email"
                            type="email"
                            value={registerForm.email}
                            onChange={handleRegisterChange}
                        />
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="מספר טלפון"
                            name="phone"
                            value={registerForm.phone}
                            onChange={handleRegisterChange}
                        />
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="תאריך לידה"
                            name="dateOfBirth"
                            type="date"
                            InputLabelProps={{ shrink: true }}
                            value={registerForm.dateOfBirth}
                            onChange={handleRegisterChange}
                        />

                        <Typography sx={{ mt: 3, mb: 1 }} variant="subtitle1">
                            כתובת
                        </Typography>
                        <TextField
                            margin="dense"
                            fullWidth
                            label="רחוב"
                            name="street"
                            value={registerForm.street || ''}
                            onChange={handleRegisterChange}
                        />
                        <TextField
                            margin="dense"
                            fullWidth
                            label="עיר"
                            name="city"
                            value={registerForm.city || ''}
                            onChange={handleRegisterChange}
                        />
                        <TextField
                            margin="dense"
                            fullWidth
                            label="מספר דירה"
                            name="apartmentNumber"
                            value={registerForm.apartmentNumber || ''}
                            onChange={handleRegisterChange}
                        />

                        <TextField
                            select
                            margin="normal"
                            fullWidth
                            label="מהי רמת הקושי המועדפת עליך?"
                            name="preferredDifficulty"
                            value={registerForm.preferredDifficulty || ''}
                            onChange={handleRegisterChange}
                            SelectProps={{ native: true }}
                        >
                            <option value="">בחר</option>
                            <option value="Beginner">מתחיל</option>
                            <option value="Intermediate">בינוני</option>
                            <option value="Advanced">מתקדם</option>
                        </TextField>

                        <Typography sx={{ mt: 3, mb: 1 }} variant="subtitle1">
                            זמנים מועדפים
                        </Typography>
                        <TextField
                            select
                            fullWidth
                            margin="dense"
                            label="יום בשבוע"
                            name="preferredDay"
                            value={registerForm.preferredDay || ''}
                            onChange={handleRegisterChange}
                            SelectProps={{ native: true }}
                        >
                            <option value="">בחר יום</option>
                            {daysOfWeek.map((day) => (
                                <option key={day} value={day}>{day}</option>
                            ))}
                        </TextField>

                        <TextField
                            select
                            fullWidth
                            margin="dense"
                            label="שעת התחלה"
                            name="startTime"
                            value={registerForm.startTime || ''}
                            onChange={handleRegisterChange}
                            SelectProps={{ native: true }}
                        >
                            <option value="">בחר</option>
                            {Array.from({ length: 17 }, (_, i) => {
                                const hour = i + 8;
                                const time = `${hour.toString().padStart(2, '0')}:00:00`;
                                return (
                                    <option key={time} value={time}>
                                        {hour}:00
                                    </option>
                                );
                            })}
                        </TextField>
                        <TextField
                            select
                            fullWidth
                            margin="dense"
                            label="שעת סיום"
                            name="endTime"
                            value={registerForm.endTime || ''}
                            onChange={handleRegisterChange}
                            SelectProps={{ native: true }}
                        >
                            <option value="">בחר</option>
                            {Array.from({ length: 17 }, (_, i) => {
                                const hour = i + 8;
                                const time = `${hour.toString().padStart(2, '0')}:00:00`;
                                return (
                                    <option key={time} value={time}>
                                        {hour}:00
                                    </option>
                                );
                            })}
                        </TextField>

                        <FormControl fullWidth margin="normal">
                            <InputLabel id="preferredClassCategory-label">קטגוריות מועדפות</InputLabel>
                            <Select
                                labelId="preferredClassCategory-label"
                                id="preferredClassCategory"
                                name="preferredClassCategory"
                                multiple
                                value={registerForm.preferredClassCategory}
                                onChange={(e) =>
                                    setRegisterForm({
                                        ...registerForm,
                                        preferredClassCategory: typeof e.target.value === 'string'
                                            ? e.target.value.split(',')
                                            : e.target.value,
                                    })
                                }
                                renderValue={(selected) => selected.join(', ')}
                            >
                                {categories.map((category) => (
                                    <MenuItem key={category} value={category}>
                                        <Checkbox checked={registerForm.preferredClassCategory.indexOf(category) > -1} />
                                        <ListItemText primary={category} />
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>



                        {registerError && (
                            <Alert severity="error" sx={{ mt: 2 }}>
                                {registerError}
                            </Alert>
                        )}
                        {registerSuccess && (
                            <Alert severity="success" sx={{ mt: 2 }}>
                                {registerSuccess}
                            </Alert>
                        )}
                        <DialogActions>
                            <Button onClick={handleCloseRegisterDialog} color="primary">
                                ביטול
                            </Button>
                            <Button type="submit" variant="contained" color="primary" disabled={registerLoading}>
                                {registerLoading ? <CircularProgress size={24} /> : 'הרשמה'}
                            </Button>
                        </DialogActions>
                    </form>
                </DialogContent>
            </Dialog>
        </Container>
    );
}

export default LoginPage;