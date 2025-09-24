import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5179/api/Auth'; // ✅ Backend Auth API

  constructor(private http: HttpClient) {}

  signup(user: { username: string; password: string; confirmPassword: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/signup`, user, {
      withCredentials: true
    }).pipe(
      catchError(this.handleError)
    );
  }

  login(user: { username: string; password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, user, {
      withCredentials: true
    }).pipe(
      catchError(this.handleError)
    );
  }

  getUsername(): string | null {
    return localStorage.getItem('username');  // ✅ From local storage
  }

  setUsername(username: string): void {
    localStorage.setItem('username', username); // ✅ Save when login success
  }

  logout(): void {
    localStorage.removeItem('username'); // ✅ On logout clear username
  }

  private handleError(error: any): Observable<never> {
    const errorMessage = error.error?.message || 'Unknown error occurred!';
    console.error('AuthService Error:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
