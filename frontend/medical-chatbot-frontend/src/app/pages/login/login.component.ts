import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  loginForm: FormGroup;
  errorMessage: string = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  onLogin(): void {
    if (this.loginForm.valid) {
      this.authService.login(this.loginForm.value).subscribe({
        next: (response) => {
          console.log('Login successful', response);
          this.authService.setUsername(this.loginForm.value.username); // Save username after login
          alert('Login successful! 🎉');
          this.loginForm.reset();
          this.errorMessage = '';

          setTimeout(() => {
            this.router.navigate(['/chat']);
          }, 2000);
        },
        error: (error) => {
          console.error('Login failed', error);
          this.errorMessage = error.message || "Login failed. Please try again.";
        }
      });
    } else {
      this.errorMessage = "Please fill all required fields.";
    }
  }
}
