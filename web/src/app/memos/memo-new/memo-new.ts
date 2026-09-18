import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../auth/api-message';
import { NewMemo } from '../memo';
import { MemoService } from '../memo.service';

@Component({
  selector: 'app-memo-new',
  imports: [FormsModule, RouterLink],
  templateUrl: './memo-new.html',
  styleUrl: './memo-new.css',
})
export class MemoNew {
  private readonly memoService = inject(MemoService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly memo: NewMemo = { title: '', body: '' };

  add(): void {
    this.memoService.create(this.memo).subscribe({
      next: () => {
        this.toastr.success('Memo created');
        this.router.navigate(['/memos']);
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }
}
