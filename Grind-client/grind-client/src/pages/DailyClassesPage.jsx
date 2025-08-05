// כרגע לא בשימוש
import React from 'react';
import { useParams } from 'react-router-dom';
import { Typography, Container } from '@mui/material';

function DailyClassesPage() {
    const { date } = useParams(); // קבלת התאריך מה-URL

    return (
        <Container>
            <Typography variant="h4" component="h1" gutterBottom>
                שיעורים לתאריך: {date ? new Date(date).toLocaleDateString() : 'לא נבחר תאריך'}
            </Typography>
            <Typography>
                רשימת השיעורים לאותו יום תופיע כאן.
            </Typography>
        </Container>
    );
}

export default DailyClassesPage;