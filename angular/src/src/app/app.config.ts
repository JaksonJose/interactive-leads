import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';

import { routes } from './app.routes';
import { apiResponseInterceptor, authInterceptor } from '@core/interceptors';

import { providePrimeNG } from 'primeng/config';
import { definePreset } from '@primeuix/themes';
import Lara from '@primeuix/themes/aura';
import { MessageService } from 'primeng/api';

const MyPreset = definePreset(Lara, {
  semantic: {
    colorScheme: {
      light: {
        surface: {
          0: '#ffffff', // White login card
          50: '#f8fafc', // Almost white background with subtle difference
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
          950: '#020617'
        },
        primary: {
          50: '#eff6ff',
          100: '#dbeafe',
          200: '#bfdbfe',
          300: '#93c5fd',
          400: '#60a5fa',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8',
          800: '#1e40af',
          900: '#1e3a8a',
          950: '#172554'
        },
      },
      dark: {
        primary: {
          50: '#0a0e1a',
          100: '#12172b',
          200: '#19203c',
          300: '#1f294d',
          400: '#26325e',
          500: '#2d3b6f',
          600: '#344480',
          700: '#3b4d91',
          800: '#4156a2',
          900: '#4860b3',
          950: '#4e69c4'
        },
        surface: {
          0: '#1e293b', // Dark login card
          50: '#0f172a', // Darker dark background
          100: '#1e293b',
          200: '#334155',
          300: '#475569',
          400: '#64748b',
          500: '#94a3b8',
          600: '#cbd5e1',
          700: '#e2e8f0',
          800: '#f1f5f9',
          900: '#f8fafc',
          950: '#ffffff'
        }
      }
    }
  }
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideAnimationsAsync(),
    provideRouter(routes),
    MessageService,
    provideHttpClient(withFetch(), withInterceptors([authInterceptor, apiResponseInterceptor])),
    provideTranslateService({
      lang: 'pt-BR',
      fallbackLang: 'pt-BR',
      loader: provideTranslateHttpLoader({
        prefix: '/i18n/',
        suffix: '.json'
      })
    }),
    providePrimeNG({
      theme: {
      preset: MyPreset,
        options: {
          darkModeSelector: false,
          prefix: 'p'
        }
      }
    })
  ]
};
