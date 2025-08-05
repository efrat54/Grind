import React, { useEffect, useState } from 'react';
import { Box, Typography, Button, Grid, CircularProgress, Alert, Dialog, DialogTitle, DialogContent, DialogActions, TextField, MenuItem,Paper } from '@mui/material';
import { useSelector, useDispatch } from 'react-redux';
import { logout } from '../redux/authSlice';
import { useNavigate } from 'react-router-dom';
import api from '../api/axiosConfig';
import ClassCard from '../components/ClassCard';

function TrainerDashboard() {
    const dispatch = useDispatch();
    const user = useSelector((state) => state.auth.user);
    const navigate = useNavigate();

    const [classes, setClasses] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [openDialog, setOpenDialog] = useState(false);
    const [formData, setFormData] = useState({
        name: '',
        description: '',
        startTime: '',
        endTime: '',
        maxCapacity: 10,
        difficulty: 'Beginner',
        category: 'Cardio',
    });
    const [formError, setFormError] = useState('');
    const [formLoading, setFormLoading] = useState(false);

    useEffect(() => {
        fetchTrainerClasses();
    }, [user.username]);

    const fetchTrainerClasses = async () => {
        try {
            const response = await api.get('/Classes/all');
            const trainerClasses = response.data.filter(c => c.trainerUsername === user.username);
            setClasses(trainerClasses);
        } catch (err) {
            setError("שגיאה בטעינת השיעורים שלך.");
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenDialog = () => {
        setOpenDialog(true);
    };

    const handleCloseDialog = () => {
        setOpenDialog(false);
        setFormError('');
        setFormData({
            name: '',
            description: '',
            startTime: '',
            endTime: '',
            maxCapacity: 10,
            difficulty: 'Beginner',
            category: 'Cardio',
        });
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleCreateClass = async () => {
        setFormLoading(true);
        setFormError('');
        try {
            await api.post('/Classes/add', {
                ...formData,
                trainerUsername: user.username
            });
            handleCloseDialog();
            fetchTrainerClasses();
        } catch (err) {
            setFormError(err.response?.data?.message || 'שגיאה ביצירת השיעור.');
            console.error(err);
        } finally {
            setFormLoading(false);
        }
    };

    return (
        <Box sx={{ p: 4 }}>
    <Typography variant="h4" gutterBottom>
        לוח בקרה למאמן
    </Typography>

    <Button variant="contained" color="primary" onClick={handleOpenDialog} sx={{ mb: 3, ml: 2 }}>
        הוספת שיעור
    </Button>

    <Paper elevation={3} sx={{ p: 3, borderRadius: '12px', mb: 4 }}>
        <Typography variant="h5" component="h2" gutterBottom sx={{ color: 'primary.main' }}>
            השיעורים שיצרת
        </Typography>

        {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
                <CircularProgress />
            </Box>
        ) : error ? (
            <Alert severity="error">{error}</Alert>
        ) : (
            <Grid container spacing={2}>
                {classes.map((classData) => (
                    <Grid item xs={12} sm={6} md={4} key={classData.id}>
                        <ClassCard
                            classData={classData}
                            onViewDetails={() => navigate(`/training/${classData.id}`)}
                            showEnrollButton={false}
                            showCancelEnrollmentButton={false}
                        />
                    </Grid>
                ))}
            </Grid>
        )}
    </Paper>

            {/* דיאלוג הוספת שיעור */}
            <Dialog open={openDialog} onClose={handleCloseDialog}>
                <DialogTitle>הוספת שיעור חדש</DialogTitle>
                <DialogContent>
                    <TextField
                        fullWidth
                        label="שם השיעור"
                        name="name"
                        value={formData.name}
                        onChange={handleInputChange}
                        margin="dense"
                    />
                    <TextField
                        fullWidth
                        label="תיאור"
                        name="description"
                        value={formData.description}
                        onChange={handleInputChange}
                        margin="dense"
                    />
                    <TextField
                        fullWidth
                        label="תאריך התחלה"
                        name="startTime"
                        type="datetime-local"
                        value={formData.startTime}
                        onChange={handleInputChange}
                        margin="dense"
                        InputLabelProps={{ shrink: true }}
                    />
                    <TextField
                        fullWidth
                        label="תאריך סיום"
                        name="endTime"
                        type="datetime-local"
                        value={formData.endTime}
                        onChange={handleInputChange}
                        margin="dense"
                        InputLabelProps={{ shrink: true }}
                    />
                    <TextField
                        fullWidth
                        label="מספר משתתפים מקסימלי"
                        name="maxCapacity"
                        type="number"
                        value={formData.maxCapacity}
                        onChange={handleInputChange}
                        margin="dense"
                    />
                    <TextField
                        fullWidth
                        label="רמת קושי"
                        name="difficulty"
                        value={formData.difficulty}
                        onChange={handleInputChange}
                        select
                        margin="dense"
                    >
                        <MenuItem value="Beginner">מתחיל</MenuItem>
                        <MenuItem value="Intermediate">בינוני</MenuItem>
                        <MenuItem value="Advanced">מתקדם</MenuItem>
                    </TextField>
                    <TextField
                        fullWidth
                        label="קטגוריה"
                        name="category"
                        value={formData.category}
                        onChange={handleInputChange}
                        select
                        margin="dense"
                    >
                        <MenuItem value="Cardio">קרדיו</MenuItem>
                        <MenuItem value="Strength">כוח</MenuItem>
                        <MenuItem value="Yoga">יוגה</MenuItem>
                        <MenuItem value="HIIT">HIIT</MenuItem>
                        <MenuItem value="Other">אחר</MenuItem>
                    </TextField>

                    {formError && <Alert severity="error" sx={{ mt: 2 }}>{formError}</Alert>}
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseDialog} disabled={formLoading}>ביטול</Button>
                    <Button onClick={handleCreateClass} variant="contained" disabled={formLoading}>
                        {formLoading ? <CircularProgress size={24} /> : 'צור שיעור'}
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
}

export default TrainerDashboard;
