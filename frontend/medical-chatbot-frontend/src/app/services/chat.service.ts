import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { Observable, catchError, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private apiUrl = 'http://localhost:5179/api/chat';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getUsername(): string {
    return this.authService.getUsername() || 'Guest';
  }

  askBot(message: string, sessionId?: string): Observable<any> {
    const username = this.getUsername();
    const body = { username, userMessage: message, sessionId };
    return this.http.post(`${this.apiUrl}/ask`, body, { withCredentials: true }).pipe(
      catchError(this.handleError)
    );
  }

  uploadDocument(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('username', this.getUsername());
    return this.http.post(`${this.apiUrl}/upload`, formData, { withCredentials: true }).pipe(
      catchError(this.handleError)
    );
  }

  getChatHistory(sessionId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/history/${sessionId}`, { withCredentials: true }).pipe(
      catchError(this.handleError)
    );
  }

  getSessions(username: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/sessions/${username}`, { withCredentials: true }).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any): Observable<never> {
    const errorMessage = error.error?.message || 'Something went wrong!';
    console.error('ChatService Error:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
