// src/pages/NotFoundPage.jsx
import React from 'react';
import { Container, Typography, Box, Button } from '@mui/material';
import { useNavigate } from 'react-router-dom';

function NotFoundPage() {
    const navigate = useNavigate();

    return (
        <Container maxWidth="md" sx={{ mt: 8, textAlign: 'center' }}>
            <Box
                sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    justifyContent: 'center',
                    minHeight: '60vh',
                }}
            >
                <Typography variant="h1" component="h1" color="primary" gutterBottom>
                    404
                </Typography>
                <Typography variant="h5" component="h2" gutterBottom>
                    הדף שחיפשת לא נמצא.
                </Typography>
                <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
                    ייתכן שהכתובת שגויה או שהדף הוסר.
                </Typography>
                <Button variant="contained" onClick={() => navigate('/')}>
                    חזור לדף הבית
                </Button>
            </Box>
        </Container>
    );
}

export default NotFoundPage;