import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css']
})
export class SignupComponent {
  signupForm: FormGroup;
  errorMessage: string = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.signupForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      confirmPassword: ['', Validators.required]
    });
  }

  onSignup(): void {
    if (this.signupForm.valid) {
      const { password, confirmPassword } = this.signupForm.value;
      if (password !== confirmPassword) {
        this.errorMessage = "Passwords do not match.";
        return;
      }

      this.authService.signup(this.signupForm.value).subscribe({
        next: (response) => {
          console.log('Signup successful', response);
          alert('Signup successful! You can now login.');
          this.signupForm.reset();
          this.errorMessage = '';

          // 🚀 WAIT 2 SECONDS THEN REDIRECT
          setTimeout(() => {
            this.router.navigate(['/login']); // After signup, go to login
          }, 2000);
        },
        error: (error) => {
          console.error('Signup failed', error);
          this.errorMessage = error.message || "Signup failed. Please try again.";
        }
      });
    } else {
      this.errorMessage = "Please fill all required fields.";
    }
  }
}
