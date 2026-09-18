import { Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SessionService } from './auth/session.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
})
export class App {
  private readonly session = inject(SessionService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly signedIn = this.session.signedIn;
  readonly userName = this.session.userName;

  signOut(): void {
    this.session.signOut();
    this.toastr.info('Signed out');
    this.router.navigate(['/']);
  }
}
