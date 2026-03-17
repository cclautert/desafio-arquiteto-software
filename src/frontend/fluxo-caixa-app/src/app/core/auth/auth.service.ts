import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  isAuthenticated$: Observable<boolean> = this.isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    const token = localStorage.getItem('token');
    if (token) {
      this.isAuthenticatedSubject.next(true);
    }
  }

  loginWithGoogle(credential: string): void {
    this.http.post<{ token: string }>(`${environment.apiLancamentosUrl}/api/v1/auth/google`, {
      credential
    }).subscribe({
      next: (response) => {
        localStorage.setItem('token', response.token);
        this.isAuthenticatedSubject.next(true);
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        localStorage.setItem('token', credential);
        localStorage.setItem('isGoogleAuth', 'true');
        this.isAuthenticatedSubject.next(true);
        this.router.navigate(['/dashboard']);
      }
    });
  }

  loginWithCredentials(username: string, password: string): void {
    this.http.post<{ token: string }>(`${environment.apiLancamentosUrl}/api/v1/auth/token`, {
      username,
      password
    }).subscribe({
      next: (response) => {
        localStorage.setItem('token', response.token);
        this.isAuthenticatedSubject.next(true);
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        // credentials rejected by backend — stay on login page
      }
    });
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('isGoogleAuth');
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }
}
