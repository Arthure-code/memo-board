import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Credentials } from '../auth/session';
import { SessionService } from '../auth/session.service';
import { apiMessage } from '../auth/api-message';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly session = inject(SessionService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly credentials: Credentials = { userName: '', password: '' };

  signIn(): void {
    if (!this.credentials.userName || !this.credentials.password) {
      this.toastr.error('Please enter your user name and password');
      return;
    }
    this.session.signIn(this.credentials).subscribe({
      next: () => this.router.navigate(['/memos']),
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }
}
