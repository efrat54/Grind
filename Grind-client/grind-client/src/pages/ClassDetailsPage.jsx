import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import {Container, Typography, CircularProgress, Alert, Box, Paper, Button, Dialog, DialogTitle, DialogContent, DialogActions} from '@mui/material';
import { AccessTime, CalendarToday, LocationOn, Person, Group, FitnessCenter, Numbers, Warning, Category } from '@mui/icons-material';
import api from '../api/axiosConfig';

function ClassDetailsPage() {
    const { id } = useParams();
    const navigate = useNavigate();

    const userRole = useSelector((state) => state.auth.user?.role);

    const [training, setTraining] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [actionLoading, setActionLoading] = useState(false);
    const [actionError, setActionError] = useState('');
    const [actionSuccess, setActionSuccess] = useState('');
    const [openConfirmDialog, setOpenConfirmDialog] = useState(false);
    const [dialogActionType, setDialogActionType] = useState('');

    useEffect(() => {
        const fetchTrainingDetails = async () => {
            setLoading(true);
            setError('');
            try {
                const response = await api.get(`/Classes/${id}`);
                setTraining(response.data);
            } catch (err) {
                console.error("Failed to fetch training details:", err.response?.data || err.message);
                setError("אירעה שגיאה בטעינת פרטי האימון. אנא נסה שוב.");
                if (err.response && err.response.status === 404) {
                    setError("האימון המבוקש לא נמצא.");
                }
            } finally {
                setLoading(false);
            }
        };

        if (id) {
            fetchTrainingDetails();
        } else {
            setError("לא סופק מזהה אימון.");
            setLoading(false);
        }
    }, [id]);

    const formatDate = (dateString) => {
        const options = { year: 'numeric', month: 'long', day: 'numeric' };
        return new Date(dateString).toLocaleDateString('he-IL', options);
    };

    const formatTime = (dateString) => {
        const options = { hour: '2-digit', minute: '2-digit', hour12: false };
        return new Date(dateString).toLocaleTimeString('he-IL', options);
    };

    const handleOpenConfirmDialog = (actionType) => {
        setDialogActionType(actionType);
        setOpenConfirmDialog(true);
    };

    const handleConfirmAction = async () => {
        setOpenConfirmDialog(false);
        setActionLoading(true);
        setActionError('');
        setActionSuccess('');
        try {
            if (dialogActionType === 'cancelEnrollment') {
                await api.delete(`/Classes/${id}/unregister`);
                setActionSuccess("הרישום לשיעור בוטל בהצלחה.");
                setTimeout(() => navigate('/client-dashboard'), 2000);
            } else if (dialogActionType === 'cancelClass') {
                await api.delete(`/Classes/${id}`);
                setActionSuccess("השיעור בוטל בהצלחה מהמערכת.");
                setTimeout(() => {
                    if (userRole === 'Trainer') navigate('/trainer-dashboard');
                    else if (userRole === 'Admin') navigate('/admin-dashboard');
                }, 2000);
            }
        } catch (err) {
            console.error(`Failed to ${dialogActionType}:`, err.response?.data || err.message);
            setActionError(err.response?.data?.message || `אירעה שגיאה בביצוע הפעולה.`);
        } finally {
            setActionLoading(false);
        }
    };

    return (
        <Container maxWidth="md" sx={{ mt: 4, mb: 4, textAlign: 'right' }}>
           
            <Typography variant="h4" component="h1" gutterBottom sx={{ mb: 3 }}>
                פרטי שיעור
            </Typography>

            {loading ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
                    <CircularProgress />
                    <Typography sx={{ ml: 2 }}>טוען פרטי שיעור...</Typography>
                </Box>
            ) : error ? (
                <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>
            ) : training ? (
                <Paper elevation={3} sx={{ p: 4, borderRadius: '12px' }}>
                    <Typography variant="h5" component="h2" gutterBottom sx={{ mb: 2, color: 'primary.dark' }}>
                        {training.name}
                    </Typography>

                    <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', flexDirection: 'row-reverse' }}>
                        <CalendarToday color="action" sx={{ ml: 1 }} />
                        <Typography variant="body1">
                            תאריך: {formatDate(training.startTime)}
                        </Typography>
                    </Box>

                    <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', flexDirection: 'row-reverse' }}>
                        <AccessTime color="action" sx={{ ml: 1 }} />
                        <Typography variant="body1">
                            שעה: {formatTime(training.startTime)} - {formatTime(training.endTime)}
                        </Typography>
                    </Box>

                    {training.location && (
                        <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', flexDirection: 'row-reverse' }}>
                            <LocationOn color="action" sx={{ ml: 1 }} />
                            <Typography variant="body1">
                                מיקום: {training.location}
                            </Typography>
                        </Box>
                    )}

                    {training.trainerUsername && (
                        <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', flexDirection: 'row-reverse' }}>
                            <Person color="action" sx={{ ml: 1 }} />
                            <Typography variant="body1">
                                מאמן: {training.trainerUsername}
                            </Typography>
                        </Box>
                    )}

                    <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', flexDirection: 'row-reverse' }}>
                        <Group color="action" sx={{ ml: 1 }} />
                        <Typography variant="body1">
                            משתתפים רשומים: {training.currentParticipants} / {training.maxCapacity}
                        </Typography>
                    </Box>

                    <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', flexDirection: 'row-reverse' }}>
                        <FitnessCenter color="action" sx={{ ml: 1 }} />
                        <Typography variant="body1">
                            רמת קושי: {
                                training.difficulty === 'Beginner' ? 'מתחיל' :
                                    training.difficulty === 'Intermediate' ? 'בינוני' :
                                        training.difficulty === 'Advanced' ? 'מתקדם' :
                                            training.difficulty
                            }
                        </Typography>
                    </Box>

                    <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', flexDirection: 'row-reverse' }}>
                        <Category color="action" sx={{ ml: 1 }} />
                        <Typography variant="body1">
                            קטגוריה: {training.category}
                        </Typography>
                    </Box>

                    {training.isCancelled && (
                        <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', color: 'error.main', flexDirection: 'row-reverse' }}>
                            <Warning color="error" sx={{ ml: 1 }} />
                            <Typography variant="body1">
                                שיעור זה בוטל. {training.cancellationReason && `סיבה: ${training.cancellationReason}`}
                            </Typography>
                        </Box>
                    )}

                    {training.description && (
                        <Box sx={{ mt: 3 }}>
                            <Typography variant="h6" gutterBottom sx={{ textAlign: 'right' }}>
                                תיאור השיעור:
                            </Typography>
                            <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'right' }}>
                                {training.description}
                            </Typography>
                        </Box>
                    )}

                    {actionError && (
                        <Alert severity="error" sx={{ mt: 2 }}>
                            {actionError}
                        </Alert>
                    )}
                    {actionSuccess && (
                        <Alert severity="success" sx={{ mt: 2 }}>
                            {actionSuccess}
                        </Alert>
                    )}

                    <Box sx={{ mt: 4, display: 'flex', gap: 2 }}>
                        {userRole === 'Client' && (
                            <Button
                                variant="contained"
                                color="error"
                                onClick={() => handleOpenConfirmDialog('cancelEnrollment')}
                                disabled={actionLoading}
                            >
                                {actionLoading ? <CircularProgress size={24} /> : 'בטל הרשמה'}
                            </Button>
                        )}

                        {(userRole === 'Trainer' || userRole === 'Admin') && (
                            <Button
                                variant="contained"
                                color="error"
                                onClick={() => handleOpenConfirmDialog('cancelClass')}
                                disabled={actionLoading}
                            >
                                {actionLoading ? <CircularProgress size={24} /> : 'בטל שיעור מהמערכת'}
                            </Button>
                        )}
                    </Box>
                </Paper>
            ) : null}

            <Dialog
                open={openConfirmDialog}
                onClose={() => setOpenConfirmDialog(false)}
                aria-labelledby="alert-dialog-title"
                aria-describedby="alert-dialog-description"
            >
                <DialogTitle id="alert-dialog-title">
                    {dialogActionType === 'cancelEnrollment' ? "אישור ביטול הרשמה" : "אישור ביטול שיעור"}
                </DialogTitle>
                <DialogContent>
                    <Typography id="alert-dialog-description">
                        {dialogActionType === 'cancelEnrollment'
                            ? "האם אתה בטוח שברצונך לבטל את הרשמתך לשיעור זה?"
                            : "האם אתה בטוח שברצונך לבטל שיעור זה לחלוטין מהמערכת?"}
                    </Typography>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setOpenConfirmDialog(false)} disabled={actionLoading}>
                        ביטול
                    </Button>
                    <Button
                        onClick={handleConfirmAction}
                        color="error"
                        variant="contained"
                        disabled={actionLoading}
                        autoFocus
                    >
                        {actionLoading ? <CircularProgress size={24} /> : 'אשר'}
                    </Button>
                </DialogActions>
            </Dialog>
        </Container>
    );
}

export default ClassDetailsPage;
