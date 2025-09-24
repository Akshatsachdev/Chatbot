import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { ChatService } from '../services/chat.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {
  messageText = '';
  chatMessages: any[] = [];
  isLoading = false;
  username = '';
  previousChats: any[] = [];
  activeSessionId: string | undefined;
  isDarkMode = false;

  selectedFile: File | null = null;
  uploadStatus: string = '';

  @ViewChild('chatContainer') chatContainer!: ElementRef;

  constructor(
    private chatService: ChatService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.username = this.authService.getUsername() || 'Guest';
    this.loadPreviousChats();
    this.applyTheme(); // 👈 apply light/dark theme
  }

  loadPreviousChats(): void {
    this.chatService.getSessions(this.username).subscribe({
      next: (sessions) => {
        this.previousChats = sessions.map((session: any) => ({
          sessionId: session.sessionId,
          title: session.title || `Session ${session.sessionId}`
        }));
      },
      error: (err) => console.error('Failed to load sessions', err)
    });
  }

  startNewChat(): void {
    this.chatMessages = [];
    this.activeSessionId = undefined;
  }

  loadChat(sessionId: string): void {
    this.activeSessionId = sessionId;
    this.chatService.getChatHistory(sessionId).subscribe({
      next: (messages) => {
        this.chatMessages = [];
        for (const m of messages) {
          this.chatMessages.push({ sender: 'user', message: m.message });
          this.chatMessages.push({ sender: 'bot', message: m.botReply });
        }
        this.scrollToBottom();
      },
      error: (err) => console.error('Failed to load chat history', err)
    });
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    this.uploadStatus = '';
  }

  sendMessage(): void {
    if (this.selectedFile) {
      this.uploadDocument(this.selectedFile);
      this.selectedFile = null;
      return;
    }

    if (!this.messageText.trim()) return;

    const userMessage = this.messageText.trim();
    this.chatMessages.push({ sender: 'user', message: userMessage });
    this.isLoading = true;
    this.messageText = '';
    this.scrollToBottom();

    this.chatService.askBot(userMessage, this.activeSessionId).subscribe({
      next: (res) => {
        this.simulateTyping(res.response);
        if (!this.activeSessionId && res.sessionId) {
          this.activeSessionId = res.sessionId;
          this.loadPreviousChats();
        }
      },
      error: (err) => {
        console.error('Chat error', err);
        this.isLoading = false;
      }
    });
  }

  uploadDocument(file: File): void {
    this.isLoading = true;
    this.uploadStatus = 'Uploading... ⏳';

    this.chatService.uploadDocument(file).subscribe({
      next: (res) => {
        this.simulateTyping(res.response);
        this.isLoading = false;
        this.uploadStatus = 'Uploaded Successfully! ✅';
      },
      error: (err) => {
        console.error('File upload error:', err);
        this.isLoading = false;
        this.uploadStatus = 'Upload failed ❌';
      }
    });
  }

  simulateTyping(fullText: string): void {
    const formattedText = fullText.replace(/\n/g, '<br>');
    const botMessage = { sender: 'bot', message: formattedText };
    this.chatMessages.push(botMessage);
    this.isLoading = false;
    this.scrollToBottom();
  }

  scrollToBottom(): void {
    setTimeout(() => {
      if (this.chatContainer) {
        this.chatContainer.nativeElement.scrollTop = this.chatContainer.nativeElement.scrollHeight;
      }
    }, 100);
  }

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    this.applyTheme();
  }

  applyTheme(): void {
    const body = document.body;
    if (this.isDarkMode) {
      body.classList.add('dark-mode');
      body.classList.remove('light-mode');
    } else {
      body.classList.add('light-mode');
      body.classList.remove('dark-mode');
    }
  }
}
