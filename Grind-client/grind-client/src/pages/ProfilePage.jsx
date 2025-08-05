import React, { useState, useEffect, useCallback } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { Container, Typography, Box, Paper, Grid, CircularProgress, Alert, Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Avatar, MenuItem, Select, InputLabel, FormControl, Checkbox, ListItemText } from '@mui/material';
import { AccountCircle, Email, Phone, Person, AttachMoney, TrendingUp, CalendarToday, LocationOn } from '@mui/icons-material';
import api from '../api/axiosConfig';
import { loginSuccess, logout } from '../redux/authSlice';
import { selectDaysOfWeek, selectCategories } from '../redux/preferencesSlice';

function ProfilePage() {
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const daysOfWeek = useSelector(selectDaysOfWeek);
    const categories = useSelector(selectCategories);
    const user = useSelector((state) => state.auth.user);

    const [profileData, setProfileData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [editMode, setEditMode] = useState(false);
    const [formData, setFormData] = useState({});
    const [actionLoading, setActionLoading] = useState(false);
    const [actionError, setActionError] = useState('');
    const [actionSuccess, setActionSuccess] = useState('');

    useEffect(() => {
        if (user) {
            console.log("User object from Redux state:", user);
            if (user.id && !user.userId) {
                console.warn("Redux user object contains 'id' but not 'userId'. Please update authSlice to use 'userId'.");
            }
        }
    }, [user]);
    const fetchProfileData = useCallback(async () => {
        if (!user || !user.userId) {
            setError('משתמש לא מזוהה. אנא התחבר מחדש.');
            setLoading(false);
            navigate('/login');
            return;
        }

        setLoading(true);
        setError('');
        try {
            let endpoint = '';
            if (user.role === 'Client') {
                endpoint = `/Clients/${user.userId}`;
            } else if (user.role === 'Trainer') {
                endpoint = `/Trainers/${user.userId}`;
            } else if (user.role === 'Admin') {
                endpoint = `/Admins/${user.userId}`;
            } else {
                setError('תפקיד משתמש לא נתמך עבור פרופיל.');
                setLoading(false);
                return;
            }

            const response = await api.get(endpoint);
            setProfileData(response.data);

            const initialFormData = {
                ...response.data,
                address: response.data.address || {},
            };

            // 💡 טיפול בשדות ייחודיים למאמן
            if (user.role === 'Trainer') {
                initialFormData.hireDate = response.data.hireDate
                    ? new Date(response.data.hireDate).toISOString().slice(0, 10)
                    : '';

                initialFormData.dateOfBirth = response.data.dateOfBirth
                    ? new Date(response.data.dateOfBirth).toISOString().slice(0, 10)
                    : '';

                initialFormData.hourlyRate = response.data.hourlyRate ?? '';
                initialFormData.specializations = Array.isArray(response.data.specializations)
                    ? response.data.specializations : [];
            }

            if (user.role === 'Client' && typeof initialFormData.preferredDifficulty === 'string') {
                switch (initialFormData.preferredDifficulty.toLowerCase()) {
                    case 'beginner':
                    case 'קל':
                        initialFormData.preferredDifficulty = 0;
                        break;
                    case 'intermediate':
                    case 'בינוני':
                        initialFormData.preferredDifficulty = 1;
                        break;
                    case 'advanced':
                    case 'קשה':
                        initialFormData.preferredDifficulty = 2;
                        break;
                    default:
                        initialFormData.preferredDifficulty = '';
                }
            }

            if (user.role === 'Client') {
                initialFormData.preferredDays = Array.isArray(response.data.preferredDays)
                    ? response.data.preferredDays
                    : [];
                initialFormData.preferredCategories = Array.isArray(response.data.preferredCategories)
                    ? response.data.preferredCategories
                    : [];
            }

            setFormData(initialFormData);

        } catch (err) {
            console.error("Failed to fetch profile data:", err.response?.data || err.message);
            if (err.response && err.response.status === 404) {
                setError("הפרופיל לא נמצא. ודא שהנתונים קיימים בשרת.");
            } else if (err.response && err.response.status === 401) {
                setError("אין לך הרשאה לגשת לפרופיל זה. אנא התחבר מחדש.");
                dispatch(logout());
                navigate('/login');
            } else {
                setError(err.response?.data?.message || "אירעה שגיאה בטעינת נתוני הפרופיל.");
            }
        } finally {
            setLoading(false);
        }
    }, [user, navigate, dispatch]);

    useEffect(() => {
        if (user) {
            fetchProfileData();
        } else {
            navigate('/login');
        }
    }, [user, fetchProfileData, navigate]);

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        if (name.startsWith('address.')) {
            const addressField = name.split('.')[1];
            setFormData(prev => ({
                ...prev,
                address: {
                    ...prev.address,
                    [addressField]: value
                }
            }));
        } else {
            setFormData(prev => ({ ...prev, [name]: value }));
        }
    };

    const handleMultiSelectChange = (event) => {
        const { name, value } = event.target;
        setFormData(prev => ({
            ...prev,
            [name]: typeof value === 'string' ? value.split(',') : value,
        }));
    };

    const handleOpenEdit = () => {
        setFormData({
            ...profileData,
            address: profileData.address || {},
            // ודא שהם מערכים עבור ה-Select Multiple
            preferredDays: Array.isArray(profileData.preferredDays)
                ? profileData.preferredDays: [],
            preferredCategories: Array.isArray(profileData.preferredCategories)
                ? profileData.preferredCategories: []
        });
        setEditMode(true);
        setActionError('');
        setActionSuccess('');
    };

    const handleCloseEdit = () => {
        setEditMode(false);
    };

    const handleSaveProfile = async () => {


        setActionLoading(true);
        setActionError('');
        setActionSuccess('');

        try {
            let endpoint = '';
            let updatedData = {};

            if (user.role === 'Client') {
                endpoint = `/Clients/${user.userId}/profile`;
                updatedData = {
                    username: formData.username,
                    email: formData.email,
                    firstName: formData.firstName,
                    lastName: formData.lastName,
                    phoneNumber: formData.phoneNumber,
                    dateOfBirth: formData.dateOfBirth ? new Date(formData.dateOfBirth).toISOString() : null,
                    address: formData.address ? {
                        id: formData.address.id || 0,
                        street: formData.address.street,
                        city: formData.address.city,
                        apartmentNumber: formData.address.apartmentNumber
                    } : null,
                    preferredDifficulty: formData.preferredDifficulty,
                    preferredDays: formData.preferredDays,
                    preferredCategories: formData.preferredCategories
                };
                console.log("🔼 Sending trainer data to server:", updatedData);
            }

            else if (user.role === 'Trainer') {
                endpoint = `/Trainers/${user.userId}/profile`;
                updatedData = {
                    firstName: formData.firstName,
                    lastName: formData.lastName,
                    email: formData.email,
                    phoneNumber: formData.phoneNumber,
                    dateOfBirth: formData.dateOfBirth ? new Date(formData.dateOfBirth).toISOString() : null,
                    specializations: formData.specializations || [],
                    bio: formData.bio,
                    hireDate: formData.hireDate ? new Date(formData.hireDate).toISOString() : null,
                    hourlyRate: parseFloat(formData.hourlyRate) || 0,
                    address: formData.address ? {
                        id: formData.address.id || 0,
                        street: formData.address.street,
                        city: formData.address.city,
                        apartmentNumber: formData.address.apartmentNumber
                    } : null
                };
            } else if (user.role === 'Admin') {
                endpoint = `/Admins/${user.userId}/profile`;
                updatedData = {
                    username: formData.username,
                    email: formData.email,
                    firstName: formData.firstName,
                    lastName: formData.lastName,
                    phoneNumber: formData.phoneNumber
                };
            } else {
                setActionError('תפקיד משתמש לא נתמך עבור עדכון פרופיל.');
                setActionLoading(false);
                return;
            }

            const response = await api.put(endpoint, updatedData);

            setProfileData(response.data);

            const updatedProfileData = { ...response.data };
            if (user.role === 'Client' && typeof updatedProfileData.preferredDifficulty === 'string') {
                switch (updatedProfileData.preferredDifficulty.toLowerCase()) {
                    case 'beginner': case 'קל': updatedProfileData.preferredDifficulty = 0; break;
                    case 'intermediate': case 'בינוני': updatedProfileData.preferredDifficulty = 1; break;
                    case 'advanced': case 'קשה': updatedProfileData.preferredDifficulty = 2; break;
                    default: updatedProfileData.preferredDifficulty = '';
                }
            }
            // 💡 טיפול ב-preferredDays ו-preferredCategories: ודא שהם מערכים
            if (user.role === 'Client') {
                updatedProfileData.preferredDays = Array.isArray(response.data.preferredDays)
                    ? response.data.preferredDays
                    : [];
                updatedProfileData.preferredCategories = Array.isArray(response.data.preferredCategories)
                    ? response.data.preferredCategories
                    : [];
            }
            setFormData(updatedProfileData);

            dispatch(loginSuccess({
                token: user.token,
                refreshToken: user.refreshToken,
                role: user.role,
                userId: user.userId,
                username: response.data.username
            }));

            setActionSuccess("הפרופיל עודכן בהצלחה!");
            setEditMode(false);
        } catch (err) {
            console.error("Failed to save profile data:", err.response?.data || err.message);
            if (err.response?.data?.errors) {
                console.log("Validation Errors:", err.response.data.errors);
            }
            if (err.response) {
                if (err.response.status === 405) {
                    setActionError("שגיאה: פעולה לא מאושרת על השרת. ודא שה-Backend תומך בעדכון פרופיל (PUT) בנתיב הנכון.");
                } else if (err.response.status === 400) {
                    let errorMessage = "אירעה שגיאה בשמירת הפרופיל. פרטי הלידציה אינם תקינים.";
                    if (err.response.data) {
                        if (typeof err.response.data === 'string') {
                            errorMessage = err.response.data;
                        } else if (err.response.data.errors) {
                            const validationErrors = Object.values(err.response.data.errors).flat().join('; ');
                            errorMessage = `שגיאות ולידציה: ${validationErrors}`;
                        } else if (typeof err.response.data === 'object') {
                            const errors = [];
                            for (const key in err.response.data) {
                                if (Array.isArray(err.response.data[key])) {
                                    errors.push(...err.response.data[key]);
                                } else if (typeof err.response.data[key] === 'string') {
                                    errors.push(err.response.data[key]);
                                }
                            }
                            if (errors.length > 0) {
                                errorMessage = `שגיאות ולידציה: ${errors.join('; ')}`;
                            } else {
                                errorMessage = "שגיאה: " + JSON.stringify(err.response.data);
                            }
                        }
                    }
                    setActionError(errorMessage);
                } else {
                    setActionError(err.response.data?.message || err.response.data || "אירעה שגיאה בשמירת הפרופיל.");
                }
            } else {
                setActionError("אירעה שגיאה בשמירת הפרופיל. ייתכן שיש בעיה בתקשורת עם השרת.");
            }
        } finally {
            setActionLoading(false);
            setTimeout(() => {
                setActionSuccess('');
                setActionError('');
            }, 3000);
        }
    };

    if (loading) {
        return (
            <Container maxWidth="md" sx={{ mt: 4, display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
                <CircularProgress />
                <Typography sx={{ ml: 2 }}>טוען את נתוני הפרופיל...</Typography>
            </Container>
        );
    }

    if (error) {
        return (
            <Container maxWidth="md" sx={{ mt: 4 }}>
                <Alert severity="error">{error}</Alert>
            </Container>
        );
    }

    if (!profileData) {
        return (
            <Container maxWidth="md" sx={{ mt: 4 }}>
                <Alert severity="info">לא נמצאו נתוני פרופיל. אנא ודא שהתחברת.</Alert>
            </Container>
        );
    }

    const DetailRow = ({ icon, label, value }) => (
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1.5, direction: 'rtl', textAlign: 'right' }}>
            {icon && <Box sx={{ ml: 1, display: 'flex', alignItems: 'center' }}>{icon}</Box>}
            <Typography variant="body1" sx={{ direction: 'rtl', textAlign: 'right' }}>
                <strong>{label}:</strong> {value || 'לא הוזן'}
            </Typography>
        </Box>
    );

    const getDifficultyText = (level) => {
        if (typeof level === 'string') {
            switch (level.toLowerCase()) {
                case 'beginner': return 'קל';
                case 'intermediate': return 'בינוני';
                case 'advanced': return 'קשה';
                default: return 'לא ידוע';
            }
        } else {
            switch (level) {
                case 0: return 'קל';
                case 1: return 'בינוני';
                case 2: return 'קשה';
                default: return 'לא ידוע';
            }
        }
    };
    const commonFields = [
        { label: 'שם משתמש', value: profileData.username, icon: <Person color="primary" /> },
        { label: 'שם פרטי', value: profileData.firstName, icon: <Person color="primary" /> },
        { label: 'שם משפחה', value: profileData.lastName, icon: <Person color="primary" /> },
        { label: 'אימייל', value: profileData.email, icon: <Email color="primary" /> },
        { label: 'טלפון', value: profileData.phoneNumber, icon: <Phone color="primary" /> },
        { label: 'תאריך לידה', value: profileData.dateOfBirth && !isNaN(new Date(profileData.dateOfBirth)) ? new Date(profileData.dateOfBirth).toLocaleDateString('he-IL') : 'לא הוזן', icon: <CalendarToday color="primary" /> },
        { label: 'מנוי פעיל', value: profileData.isActive ? 'כן' : 'לא' }
    ];

    const addressFields = [
        { label: 'רחוב', value: profileData.address?.street },
        { label: 'עיר', value: profileData.address?.city },
        { label: 'מספר דירה', value: profileData.address?.apartmentNumber }
    ].filter(field => field.value);

    const clientSpecificFields = [
        // { label: 'יתרה לתשלום', value: profileData.balanceDue ? `${profileData.balanceDue} ₪` : '0 ₪', icon: <TrendingUp color="primary" /> },
        { label: 'רמת קושי מועדפת', value: getDifficultyText(profileData.preferredDifficulty) },
        { label: 'ימים מועדפים', value: Array.isArray(profileData.preferredDays) ? profileData.preferredDays.join(', ') : 'לא הוזן' },
        { label: 'קטגוריות מועדפות', value: Array.isArray(profileData.preferredCategories) ? profileData.preferredCategories.join(', ') : 'לא הוזן' }
    ];
    const trainerSpecificFields = [];

    // טיפול בשדה התמחות (specializations הוא מערך)
    if (profileData.specializations && Array.isArray(profileData.specializations) && profileData.specializations.length > 0) {
        trainerSpecificFields.push({
            label: 'התמחות',
            value: profileData.specializations.join(', '),
        });
    }

    // תאריך הצטרפות
    if (profileData.hireDate) {
        trainerSpecificFields.push({
            label: 'תאריך הצטרפות',
            value: new Date(profileData.hireDate).toLocaleDateString('he-IL'),
            icon: <CalendarToday color="primary" />,
        });
    }

    // לא מציגים כרגע את שכר לשעה (מוסרים את השורה של hourlyRate)


    return (
        <Container maxWidth="md" sx={{ mt: 4, mb: 4, direction: 'rtl' }}>
            <Paper elevation={3} sx={{ p: 4, borderRadius: '12px', textAlign: 'center' }}>
                <Avatar sx={{ width: 80, height: 80, bgcolor: 'primary.main', mx: 'auto', mb: 2 }}>
                    <AccountCircle sx={{ fontSize: 50 }} />
                </Avatar>
                <Typography variant="h4" component="h1" gutterBottom sx={{ color: 'primary.dark' }}>
                    {profileData.username}
                </Typography>
                <Typography variant="subtitle1" color="text.secondary" sx={{ mb: 3 }}>
                    {user?.role === 'Client' ? 'לקוח' : user?.role === 'Trainer' ? 'מאמן' : user?.role === 'Admin' ? 'מנהל' : 'משתמש'}
                </Typography>

                {actionError && <Alert severity="error" sx={{ mb: 2 }}>{actionError}</Alert>}
                {actionSuccess && <Alert severity="success" sx={{ mb: 2 }}>{actionSuccess}</Alert>}

                <Box sx={{ textAlign: 'right', mt: 3 }}>
                    {commonFields.map((field, index) => (
                        <DetailRow key={index} icon={field.icon} label={field.label} value={field.value} />
                    ))}

                    {addressFields.length > 0 && (profileData.address?.street || profileData.address?.city) && (
                        <>
                            <Typography variant="h6" sx={{ mt: 3, mb: 1, display: 'flex', alignItems: 'center', direction: 'rtl', textAlign: 'right' }}>
                                <LocationOn sx={{ ml: 1, mr: 0 }} color="primary" /> פרטי כתובת:
                            </Typography>
                            {addressFields.map((field, index) => (
                                <DetailRow key={`address-${index}`} label={field.label} value={field.value} />
                            ))}
                        </>
                    )}

                    {user?.role === 'Client' && (
                        <>
                            <Typography variant="h6" sx={{ mt: 3, mb: 1, direction: 'rtl', textAlign: 'right' }}>פרטי לקוח:</Typography>
                            {clientSpecificFields.map((field, index) => (
                                <DetailRow key={`client-${index}`} icon={field.icon} label={field.label} value={field.value} />
                            ))}
                        </>
                    )}
                    {user?.role === 'Trainer' && (
                        <>
                            <Typography variant="h6" sx={{ mt: 3, mb: 1, direction: 'rtl', textAlign: 'right' }}>פרטי מאמן:</Typography>
                            {trainerSpecificFields.map((field, index) => (
                                <DetailRow key={`trainer-${index}`} icon={field.icon} label={field.label} value={field.value} />
                            ))}
                        </>
                    )}
                </Box>

                <Button
                    variant="contained"
                    color="primary"
                    startIcon={<AccountCircle />}
                    onClick={handleOpenEdit}
                    sx={{ mt: 3 }}
                >
                    ערוך פרופיל
                </Button>
            </Paper>

            <Dialog open={editMode} onClose={handleCloseEdit} fullWidth maxWidth="sm">
                <DialogTitle sx={{ textAlign: 'center', pb: 2 }}>
                    ערוך את הפרופיל שלך
                </DialogTitle>
                <DialogContent dividers sx={{ direction: 'rtl' }}>
                    <Grid container spacing={2}>

                        {/* שדות בסיסיים */}
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                margin="dense"
                                name="username"
                                label="שם משתמש"
                                value={formData.username || ''}
                                onChange={handleInputChange}
                                InputProps={{ startAdornment: <Person sx={{ ml: 1, mr: 0 }} />, readOnly: true, style: { pointerEvents: 'none' } }}
                                InputLabelProps={{ shrink: true }}
                            />
                        </Grid>

                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                margin="dense"
                                name="firstName"
                                label="שם פרטי"
                                value={formData.firstName || ''}
                                onChange={handleInputChange}
                                InputLabelProps={{ shrink: true }}
                            />
                        </Grid>

                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                margin="dense"
                                name="lastName"
                                label="שם משפחה"
                                value={formData.lastName || ''}
                                onChange={handleInputChange}
                                InputLabelProps={{ shrink: true }}
                            />
                        </Grid>

                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                margin="dense"
                                name="email"
                                label="אימייל"
                                type="email"
                                value={formData.email || ''}
                                onChange={handleInputChange}
                                InputProps={{ startAdornment: <Email sx={{ ml: 1, mr: 0 }} /> }}
                                InputLabelProps={{ shrink: true }}
                            />
                        </Grid>

                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                margin="dense"
                                name="phoneNumber"
                                label="טלפון"
                                value={formData.phoneNumber || ''}
                                onChange={handleInputChange}
                                InputProps={{ startAdornment: <Phone sx={{ ml: 1, mr: 0 }} /> }}
                                InputLabelProps={{ shrink: true }}
                            />
                        </Grid>

                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                margin="dense"
                                name="dateOfBirth"
                                label="תאריך לידה"
                                type="date"
                                value={formData.dateOfBirth ? formData.dateOfBirth.split('T')[0] : ''}
                                onChange={handleInputChange}
                                InputLabelProps={{ shrink: true }}
                                InputProps={{ readOnly: true, style: { pointerEvents: 'none' } }}
                            />
                        </Grid>

                        {/* כתובת */}
                        {(user.role === 'Client' || user.role === 'Trainer') && (
                            <>
                                <Grid item xs={12}>
                                    <Typography variant="h6" sx={{ mt: 2, mb: 1, direction: 'rtl', textAlign: 'right', width: '100%' }}>
                                        פרטי כתובת:
                                    </Typography>
                                </Grid>

                                <Grid item xs={12}>
                                    <TextField
                                        fullWidth
                                        margin="dense"
                                        name="address.street"
                                        label="רחוב"
                                        value={formData.address?.street || ''}
                                        onChange={handleInputChange}
                                        InputLabelProps={{ shrink: true }}
                                    />
                                </Grid>

                                <Grid item xs={12} sm={6}>
                                    <TextField
                                        fullWidth
                                        margin="dense"
                                        name="address.city"
                                        label="עיר"
                                        value={formData.address?.city || ''}
                                        onChange={handleInputChange}
                                        InputLabelProps={{ shrink: true }}
                                    />
                                </Grid>

                                <Grid item xs={12} sm={6}>
                                    <TextField
                                        fullWidth
                                        margin="dense"
                                        name="address.apartmentNumber"
                                        label="מספר בית"
                                        value={formData.address?.apartmentNumber || ''}
                                        onChange={handleInputChange}
                                        InputLabelProps={{ shrink: true }}
                                    />
                                </Grid>
                            </>
                        )}

                        {user?.role === 'Client' && (
                            <>
                                <Grid item xs={12}>
                                    <Typography variant="h6" sx={{ mt: 2, mb: 1, direction: 'rtl', textAlign: 'right' }}>העדפות אימון:</Typography>
                                </Grid>

                                <Grid item xs={12}>
                                    <FormControl fullWidth margin="dense">
                                        <InputLabel id="preferredDifficulty-label" sx={{ transformOrigin: 'top right !important', right: 0, left: 'unset !important' }}>רמת קושי מועדפת</InputLabel>
                                        <Select
                                            labelId="preferredDifficulty-label"
                                            id="preferredDifficulty"
                                            name="preferredDifficulty"
                                            value={formData.preferredDifficulty !== undefined && formData.preferredDifficulty !== null ? formData.preferredDifficulty : ''}
                                            label="רמת קושי מועדפת"
                                            onChange={handleInputChange}
                                            sx={{ textAlign: 'right' }}
                                        >
                                            <MenuItem value={0} sx={{ direction: 'rtl' }}>קל</MenuItem>
                                            <MenuItem value={1} sx={{ direction: 'rtl' }}>בינוני</MenuItem>
                                            <MenuItem value={2} sx={{ direction: 'rtl' }}>קשה</MenuItem>
                                        </Select>
                                    </FormControl>
                                </Grid>
                                <Grid item xs={12}>
                                    <FormControl fullWidth margin="dense">
                                        <InputLabel id="preferredDays-label" sx={{ transformOrigin: 'top right !important', right: 0, left: 'unset !important' }}>
                                            ימים מועדפים
                                        </InputLabel>
                                        <Select
                                            labelId="preferredDays-label"
                                            id="preferredDays"
                                            name="preferredDays"
                                            multiple
                                            value={formData.preferredDays || []}
                                            onChange={handleMultiSelectChange}
                                            renderValue={(selected) => selected.join(', ')}
                                            label="ימים מועדפים"
                                            sx={{ textAlign: 'right' }}
                                        >
                                            {daysOfWeek.map((day) => (
                                                <MenuItem key={day} value={day} sx={{ direction: 'rtl' }}>
                                                    <Checkbox checked={formData.preferredDays.indexOf(day) > -1} />
                                                    <ListItemText primary={day} />
                                                </MenuItem>
                                            ))}
                                        </Select>
                                    </FormControl>

                                </Grid>
                                <Grid item xs={12}>
                                    <FormControl fullWidth margin="dense">
                                        <InputLabel id="preferredCategories-label" sx={{ transformOrigin: 'top right !important', right: 0, left: 'unset !important' }}>
                                            קטגוריות מועדפות
                                        </InputLabel>
                                        <Select
                                            labelId="preferredCategories-label"
                                            id="preferredCategories"
                                            name="preferredCategories"
                                            multiple
                                            value={formData.preferredCategories || []}
                                            onChange={handleMultiSelectChange}
                                            renderValue={(selected) => selected.join(', ')}
                                            label="קטגוריות מועדפות"
                                            sx={{ textAlign: 'right' }}
                                        >
                                            {categories.map((category) => (
                                                <MenuItem key={category} value={category} sx={{ direction: 'rtl' }}>
                                                    <Checkbox checked={formData.preferredCategories.indexOf(category) > -1} />
                                                    <ListItemText primary={category} />
                                                </MenuItem>
                                            ))}
                                        </Select>
                                    </FormControl>

                                </Grid>
                            </>
                        )}

                        {user?.role === 'Trainer' && (
                            <>
                                <Grid item xs={12}>
                                    <Typography variant="h6" sx={{ mt: 2, mb: 1, direction: 'rtl', textAlign: 'right', width: '100%' }}>
                                        פרטי העסקה והתמחות:
                                    </Typography>
                                </Grid>

                                {/* בחירה מרובה של התמחות (specializations) - אם את רוצה להשתמש ב-MUI Select */}
                                <Grid item xs={12}>
                                    <FormControl fullWidth margin="dense">
                                        <InputLabel shrink id="specializations-label">
                                            התמחות
                                        </InputLabel>
                                        <Select
                                            labelId="specializations-label"
                                            id="specializations"
                                            multiple
                                            name="specializations"
                                            value={formData.specializations || []}
                                            onChange={handleInputChange}
                                            renderValue={(selected) => selected.join(', ')}
                                        >
                                            {/* לדוגמה, רשימת אפשרויות קשיחות (את יכולה להחליף לפי מה שמוגדר ב-Redux) */}
                                            {categories.map((spec) => (
                                                <MenuItem key={spec} value={spec}>
                                                    <Checkbox checked={formData.specializations?.includes(spec) || false} />
                                                    <ListItemText primary={spec} />
                                                </MenuItem>
                                            ))}
                                        </Select>
                                    </FormControl>
                                </Grid>

                                <Grid item xs={12}>
                                    <TextField
                                        fullWidth
                                        margin="dense"
                                        name="hireDate"
                                        label="תאריך הצטרפות"
                                        type="date"
                                        value={formData.hireDate ? formData.hireDate.split('T')[0] : ''}
                                        onChange={handleInputChange}
                                        InputLabelProps={{ shrink: true }}
                                    />
                                </Grid>
                            </>
                        )}
                    </Grid>
                </DialogContent>
                <DialogActions sx={{ p: 2, justifyContent: 'center' }}>
                    <Button onClick={handleCloseEdit} color="secondary" disabled={actionLoading}>
                        ביטול
                    </Button>
                    <Button
                        onClick={handleSaveProfile}
                        color="primary"
                        variant="contained"
                        disabled={actionLoading}
                        startIcon={actionLoading ? <CircularProgress size={20} /> : null}
                    >
                        שמור שינויים
                    </Button>
                </DialogActions>
            </Dialog>
        </Container>
    );
}

export default ProfilePage;