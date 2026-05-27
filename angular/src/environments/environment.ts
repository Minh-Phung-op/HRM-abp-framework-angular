import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44340/',
  redirectUri: baseUrl,
  clientId: 'HRM_App',
  responseType: 'code',
  scope: 'offline_access HRM',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'HRM',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44340',
      rootNamespace: 'Acme.HRM',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;

export const firebaseConfig = {
  apiKey: "AIzaSyCiUnmqKrkVFhPbeK4YG0Jjl1FaIuBQiO0",
  authDomain: "findeducation-6c082.firebaseapp.com",
  projectId: "findeducation-6c082",
  databaseURL: "https://findeducation-6c082-default-rtdb.asia-southeast1.firebasedatabase.app",
  storageBucket: "findeducation-6c082.firebasestorage.app",
  messagingSenderId: "1024919506183",
  appId: "1:1024919506183:web:a74c364d009af0e7a7c9a2"
};