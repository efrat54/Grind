// src/main.jsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App.jsx';
import './index.css';
import { Provider } from 'react-redux'; // ייבוא Provider
import { store } from './redux/store'; // ייבוא ה-store

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <Provider store={store}> {/* עוטפים את App ב-Redux Provider */}
      <App />
    </Provider>
  </React.StrictMode>,
);