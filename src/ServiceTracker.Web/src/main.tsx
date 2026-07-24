import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'
import './index.css'

declare global {
  interface Window {
    _env_: Record<string, string> | undefined;
  }
}

if (window._env_) {
  ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
      <App />
    </React.StrictMode>,
  );
}
 else {
      console.error("Failed to load runtime configuration");
    }