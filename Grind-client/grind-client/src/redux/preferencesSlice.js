import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  daysOfWeek: [
    'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'
  ],
  categories: [
    'Cardio','Strength','Yoga','Pilates','Zumba','Spin','CrossFit',
  ],
};

const preferencesSlice = createSlice({
  name: 'preferences',
  initialState,
  reducers: {
    // אם תרצי בעתיד לעדכן את הרשימות - אפשר להוסיף reducers כאן
  },
});

export const selectDaysOfWeek = (state) => state.preferences.daysOfWeek;
export const selectCategories = (state) => state.preferences.categories;

export default preferencesSlice.reducer;
