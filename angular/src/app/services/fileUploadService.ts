import { Injectable } from '@angular/core';
import { initializeApp } from 'firebase/app';
import { getStorage, ref, uploadBytes, getDownloadURL } from 'firebase/storage';
import { getAuth, signInAnonymously } from 'firebase/auth'; // Thêm Auth
import { firebaseConfig } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FileUploadService {
  private storage;
  private auth;

  constructor() {
    const app = initializeApp(firebaseConfig);
    this.storage = getStorage(app);
    this.auth = getAuth(app);
    
    // Tự động đăng nhập ẩn danh khi service khởi tạo
    this.ensureAuthenticated();
  }

  private async ensureAuthenticated() {
    if (!this.auth.currentUser) {
      await signInAnonymously(this.auth);
    }
  }

  async uploadAvatar(file: File, employeeCode: string): Promise<string> {
    await this.ensureAuthenticated(); // Đảm bảo đã có token auth trước khi upload

    const filePath = `avatars/${employeeCode}_${new Date().getTime()}`;
    const storageRef = ref(this.storage, filePath);
    
    const snapshot = await uploadBytes(storageRef, file);
    return await getDownloadURL(snapshot.ref);
  }
}