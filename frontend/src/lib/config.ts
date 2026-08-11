import { browser } from '$app/environment';

export const API_URL = browser
    ? 'http://localhost:5172'
    : 'http://api:8080';