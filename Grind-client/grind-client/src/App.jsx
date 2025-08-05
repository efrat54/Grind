import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { useSelector } from 'react-redux';
import PrivateRoute from './components/PrivateRoute';

// 💡 חדש: ייבוא של קומפוננטת ה-Navbar
import Navbar from './components/Navbar';
// 💡 חדש: ייבוא של ProfilePage (נשתמש בו מיד)
import ProfilePage from './pages/ProfilePage';

import LoginPage from './pages/LoginPage';
import ClientDashboard from './pages/ClientDashboard';
import TrainerDashboard from './pages/TrainerDashboard';
import AdminDashboard from './pages/AdminDashboard';
import NotFoundPage from './pages/NotFoundPage';
import ClassDetailsPage from './pages/ClassDetailsPage';

// 💡 חדש: ייבוא Material-UI Theme ו-CssBaseline
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { Box } from '@mui/material'; // ייבוא Box כדי להוסיף ריווח

// הגדרת ערכת נושא בסיסית עבור Material-UI
const theme = createTheme({
  palette: {
    primary: {
      main: '#2196f3', // כחול
    },
    secondary: {
      main: '#ff9800', // כתום
    },
  },
  typography: {
    fontFamily: 'Inter, sans-serif', // שימוש בפונט Inter
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: '8px', // כפתורים עם פינות מעוגלות
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: '12px', // כרטיסים עם פינות מעוגלות יותר
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          borderRadius: '12px', // פאנלים עם פינות מעוגלות יותר
        },
      },
    },
    MuiDialog: {
      styleOverrides: {
        paper: {
          borderRadius: '12px', // דיאלוגים עם פינות מעוגלות
        },
      },
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: '8px', // שדות טקסט עם פינות מעוגלות
          },
        },
      },
    },
  },
});


function App() {
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);
  const userRole = useSelector((state) => state.auth.user?.role);

  return (
    <ThemeProvider theme={theme}>
      <Router>
        <Navbar />
        <Box sx={{ pt: { xs: 7, sm: 8 } }}>
          <Routes>
            {/* Public Routes */}
            <Route path="/" element={<LoginPage />} />

            <Route
              element={<PrivateRoute isAuthenticated={isAuthenticated} allowedRoles={['Client', 'Trainer', 'Admin']} userRole={userRole} />}
            >
              <Route path="/training/:id" element={<ClassDetailsPage />} />
            </Route>

            {/* נתיבים ללקוחות */}
            <Route
              element={<PrivateRoute isAuthenticated={isAuthenticated} allowedRoles={['Client']} userRole={userRole} />}
            >
              <Route path="/client-dashboard" element={<ClientDashboard />} />
              <Route path="/client/profile" element={<ProfilePage />} />
            </Route>

            {/* נתיבים למאמנים */}
            <Route
              element={<PrivateRoute isAuthenticated={isAuthenticated} allowedRoles={['Trainer']} userRole={userRole} />}
            >
              <Route path="/trainer-dashboard" element={<TrainerDashboard />} />
              <Route path="/trainer/profile" element={<ProfilePage />} />
            </Route>

            {/* -כרגע לא בשימושנתיבים למנהלים */}
            {/* <Route
              element={<PrivateRoute isAuthenticated={isAuthenticated} allowedRoles={['Admin']} userRole={userRole} />}
            >
              <Route path="/admin-dashboard" element={<AdminDashboard />} />
              <Route path="/admin/profile" element={<ProfilePage />} />
            </Route> */}

            {/* דפים כלליים */}
            <Route path="/unauthorized" element={<NotFoundPage />} />
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </Box>
      </Router>

    </ThemeProvider>
  );
}

export default App;