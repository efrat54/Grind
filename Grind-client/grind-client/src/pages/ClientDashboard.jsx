import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { Container, Typography, Box, Paper, Grid, CircularProgress, Alert, Button, List, ListItem, ListItemText, Dialog, DialogTitle, DialogContent, DialogActions } from '@mui/material';
import ClassCard from '../components/ClassCard';
import api from '../api/axiosConfig';

function ClientDashboard() {
    const navigate = useNavigate();
    const user = useSelector((state) => state.auth.user);

    const [clientSchedule, setClientSchedule] = useState([]);
    const [allAvailableClasses, setAllAvailableClasses] = useState([]);
    const [loadingSchedule, setLoadingSchedule] = useState(true);
    const [loadingAllClasses, setLoadingAllClasses] = useState(true);
    const [errorSchedule, setErrorSchedule] = useState('');
    const [errorAllClasses, setErrorAllClasses] = useState('');
    const [actionLoading, setActionLoading] = useState(false);
    const [actionError, setActionError] = useState('');
    const [actionSuccess, setActionSuccess] = useState('');
    const [openConfirmDialog, setOpenConfirmDialog] = useState(false);
    const [selectedClassId, setSelectedClassId] = useState(null);
    const [dialogActionType, setDialogActionType] = useState(''); 

   const fetchClientSchedule = useCallback(async () => {
    setLoadingSchedule(true);
    setErrorSchedule('');
    try {
        const response = await api.get('/Classes/client-schedule');
        setClientSchedule(response.data);
    } catch (err) {
        const errorMessage = err.response?.data || err.message;

        if (err.response && err.response.status === 404 && errorMessage?.includes("No schedule found")) {
            setClientSchedule([]);
            setErrorSchedule(''); 
        } else {
            // console.error("שגיאה אמיתית בטעינת לוח השיעורים:", errorMessage);
            setErrorSchedule(err.response?.data?.message || "אירעה שגיאה בטעינת לוח השיעורים שלך.");
            setClientSchedule([]);
        }
    } finally {
        setLoadingSchedule(false);
    }
}, []);


    const fetchAllAvailableClasses = useCallback(async (currentClientClassIds) => {
        setLoadingAllClasses(true);
        setErrorAllClasses('');
        try {
            const response = await api.get('/Classes/all');
            const filteredClasses = response.data.filter(cls => !currentClientClassIds.has(cls.id));
            setAllAvailableClasses(filteredClasses);
        } catch (err) {
            console.error("Failed to fetch all available classes:", err.response?.data || err.message);
            setErrorAllClasses(err.response?.data?.message || "אירעה שגיאה בטעינת כלל השיעורים במכון.");
            setAllAvailableClasses([]);
        } finally {
            setLoadingAllClasses(false);
        }
    }, []);

    useEffect(() => {
        if (user?.role === 'Client') {
            fetchClientSchedule();
        }
    }, [user, fetchClientSchedule]);

    useEffect(() => {
        if (user?.role === 'Client' && !loadingSchedule) {
            const currentClientClassIds = new Set(clientSchedule.map(cls => cls.id));
            fetchAllAvailableClasses(currentClientClassIds);
        }
    }, [user, loadingSchedule, clientSchedule, fetchAllAvailableClasses]);

    const handleViewDetails = (classId) => {
        navigate(`/training/${classId}`);
    };

    const handleOpenConfirmDialog = (actionType, classId) => {
        setDialogActionType(actionType);
        setSelectedClassId(classId);
        setOpenConfirmDialog(true);
    };

    const handleConfirmAction = async () => {
        setOpenConfirmDialog(false);
        setActionLoading(true);
        setActionError('');
        setActionSuccess('');
        try {
            if (dialogActionType === 'enroll') {
                await api.post(`/Classes/${selectedClassId}/register`);
                setActionSuccess("נרשמת לשיעור בהצלחה!");
            } else if (dialogActionType === 'cancelEnrollment') {
                await api.delete(`/Classes/${selectedClassId}/unregister`);
                setActionSuccess("הרישום לשיעור בוטל בהצלחה.");
            }

            await fetchClientSchedule();

        } catch (err) {
            console.error(`Failed to ${dialogActionType}:`, err.response?.data || err.message);
            setActionError(err.response?.data?.message || `אירעה שגיאה בביצוע הפעולה: ${dialogActionType === 'enroll' ? 'הרשמה' : 'ביטול הרשמה'}.`);
        } finally {
            setActionLoading(false);
            setSelectedClassId(null);
            setTimeout(() => {
                setActionSuccess('');
                setActionError('');
            }, 3000);
        }
    };

    if (!user || user.role !== 'Client') {
        navigate('/login');
        return null;
    }

    return (
        <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
            <Typography variant="h4" component="h1" gutterBottom align="center" sx={{ mb: 4 }}>
                איזור אישי-ראשי- {user.username}
            </Typography>

            {actionError && <Alert severity="error" sx={{ mb: 2 }}>{actionError}</Alert>}
            {actionSuccess && <Alert severity="success" sx={{ mb: 2 }}>{actionSuccess}</Alert>}

            {/* My Classes */}
            <Paper elevation={3} sx={{ p: 3, mb: 4, borderRadius: '12px' }}>
                <Typography variant="h5" component="h2" gutterBottom sx={{ color: 'primary.main' }}>
                    השיעורים שלי
                </Typography>
                {loadingSchedule ? (
                    <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
                        <CircularProgress />
                        <Typography sx={{ ml: 2 }}>טוען את השיעורים שלך...</Typography>
                    </Box>
                ) : errorSchedule ? (
                    <Alert severity="error" sx={{ mt: 2 }}>{errorSchedule}</Alert>
                ) : clientSchedule.length > 0 ? (
                    <Grid container spacing={3}>
                        {clientSchedule.map((cls) => (
                            <Grid item xs={12} sm={6} md={4} key={cls.id}>
                                <ClassCard
                                    classData={cls}
                                    onViewDetails={() => handleViewDetails(cls.id)}
                                    showCancelEnrollmentButton={true}
                                    onCancelEnrollment={() => handleOpenConfirmDialog('cancelEnrollment', cls.id)}
                                    actionLoading={actionLoading && selectedClassId === cls.id}
                                />
                            </Grid>
                        ))}
                    </Grid>
                ) : (
                    <Alert severity="info">
                        👇עדיין לא נרשמת לשיעורים כלשהם. ניתן להירשם לשיעורים בחלק "כלל השיעורים במכון" למטה.
                    </Alert>
                )}
            </Paper>

            {/* All Available Classes */}
            <Paper elevation={3} sx={{ p: 3, borderRadius: '12px' }}>
                <Typography variant="h5" component="h2" gutterBottom sx={{ color: 'secondary.main' }}>
                    כלל השיעורים במכון
                </Typography>
                {loadingAllClasses ? (
                    <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
                        <CircularProgress />
                        <Typography sx={{ ml: 2 }}>טוען את כלל השיעורים...</Typography>
                    </Box>
                ) : errorAllClasses ? (
                    <Alert severity="error" sx={{ mt: 2 }}>{errorAllClasses}</Alert>
                ) : allAvailableClasses.length > 0 ? (
                    <Grid container spacing={3}>
                        {allAvailableClasses.map((cls) => (
                            <Grid item xs={12} sm={6} md={4} key={cls.id}>
                                <ClassCard
                                    classData={cls}
                                    onViewDetails={() => handleViewDetails(cls.id)}
                                    showEnrollButton={true}
                                    onEnroll={() => handleOpenConfirmDialog('enroll', cls.id)}
                                    actionLoading={actionLoading && selectedClassId === cls.id}
                                />
                            </Grid>
                        ))}
                    </Grid>
                ) : (
                    <Alert severity="info">
                        אין שיעורים זמינים להרשמה כרגע.
                    </Alert>
                )}
            </Paper>

            {/* Confirm Dialog */}
            <Dialog
                open={openConfirmDialog}
                onClose={() => setOpenConfirmDialog(false)}
                aria-labelledby="alert-dialog-title"
                aria-describedby="alert-dialog-description"
            >
                <DialogTitle id="alert-dialog-title">
                    {dialogActionType === 'enroll' ? "אישור הרשמה לשיעור" : "אישור ביטול הרשמה"}
                </DialogTitle>
                <DialogContent>
                    <Typography id="alert-dialog-description">
                        {dialogActionType === 'enroll'
                            ? "האם אתה בטוח שברצונך להירשם לשיעור זה?"
                            : "האם אתה בטוח שברצונך לבטל את הרשמתך לשיעור זה?"}
                    </Typography>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setOpenConfirmDialog(false)} disabled={actionLoading}>
                        ביטול
                    </Button>
                    <Button
                        onClick={handleConfirmAction}
                        color={dialogActionType === 'enroll' ? "primary" : "error"}
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

export default ClientDashboard;