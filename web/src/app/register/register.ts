import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Credentials } from '../auth/session';
import { SessionService } from '../auth/session.service';
import { apiMessage } from '../auth/api-message';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly session = inject(SessionService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly credentials: Credentials = { userName: '', password: '' };

  register(): void {
    this.session.register(this.credentials).subscribe({
      next: () => {
        this.toastr.success('Account created, you can sign in');
        this.router.navigate(['/']);
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }
}
