import React from 'react';
import { Card, CardContent, Typography, Button, Box, CircularProgress } from '@mui/material';
import { AccessTime, CalendarToday, LocationOn, Category } from '@mui/icons-material';

function ClassCard({
    classData,
    onViewDetails,
    showEnrollButton, 
    onEnroll,         
    showCancelEnrollmentButton,
    onCancelEnrollment, 
    actionLoading ,extraButton,
}) {
    const formatDate = (dateString) => {
        const options = { year: 'numeric', month: 'long', day: 'numeric' };
        return new Date(dateString).toLocaleDateString('he-IL', options);
    };

    const formatTime = (dateString) => {
        const options = { hour: '2-digit', minute: '2-digit', hour12: false };
        return new Date(dateString).toLocaleTimeString('he-IL', options);
    };

    return (
        <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column', borderRadius: '12px', boxShadow: 3, textAlign: 'right' }}>
            <CardContent sx={{ flexGrow: 1 }}>
                <Typography variant="h6" component="h3" gutterBottom sx={{ color: 'primary.dark', textAlign: 'right' }}>
                    {classData.name}
                </Typography>

                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1, justifyContent: 'flex-end' }}>
                    <Typography variant="body2" color="text.secondary" sx={{ ml: 1 }}>
                        {formatDate(classData.startTime)}
                    </Typography>
                    <CalendarToday fontSize="small" sx={{ ml: 1, color: 'text.secondary' }} />
                </Box>

                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1, justifyContent: 'flex-end' }}>
                    <Typography variant="body2" color="text.secondary" sx={{ ml: 1 }}>
                        {classData.category}
                    </Typography>
                    <Category fontSize="small" sx={{ ml: 1, color: 'text.secondary' }} />
                </Box>

                {classData.location && (
                    <Box sx={{ display: 'flex', alignItems: 'center', mb: 1, justifyContent: 'flex-end' }}>
                        <Typography variant="body2" color="text.secondary" sx={{ ml: 1 }}>
                            {classData.location}
                        </Typography>
                        <LocationOn fontSize="small" sx={{ ml: 1, color: 'text.secondary' }} />
                    </Box>
                )}

                {classData.trainerUsername && (
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 1, textAlign: 'right' }}>
                        מאמן: {classData.trainerUsername}
                    </Typography>
                )}
            </CardContent>

            <Box sx={{ p: 2, display: 'flex', justifyContent: 'space-between', gap: 1 }}>
                <Button size="small" variant="outlined" onClick={onViewDetails}>
                    צפה בפרטים
                </Button>
                {showEnrollButton && (
                    <Button
                        size="small"
                        variant="contained"
                        color="primary"
                        onClick={onEnroll}
                        disabled={actionLoading}
                    >
                        {actionLoading ? <CircularProgress size={20} /> : 'הירשם'}
                    </Button>
                )}
                {showCancelEnrollmentButton && (
                    <Button
                        size="small"
                        variant="contained"
                        color="error"
                        onClick={onCancelEnrollment}
                        disabled={actionLoading}
                    >
                        {actionLoading ? <CircularProgress size={20} /> : 'בטל הרשמה'}
                    </Button>
                )}

                {/* כאן נציג את הכפתור הנוסף אם קיים */}
                {extraButton}
            </Box>
        </Card>
    );
}

export default ClassCard;