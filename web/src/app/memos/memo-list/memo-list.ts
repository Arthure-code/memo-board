import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../auth/api-message';
import { Memo } from '../memo';
import { MemoService } from '../memo.service';

// My memos, oldest first as the API sends them, each with its Delete.
@Component({
  selector: 'app-memo-list',
  imports: [DatePipe, RouterLink],
  templateUrl: './memo-list.html',
  styleUrl: './memo-list.css',
})
export class MemoList {
  private readonly memoService = inject(MemoService);
  private readonly toastr = inject(ToastrService);

  readonly memos = signal<Memo[]>([]);
  readonly loading = signal(true);

  constructor() {
    this.load();
  }

  remove(memo: Memo): void {
    this.memoService.delete(memo.id).subscribe({
      next: () => {
        this.toastr.success('Memo deleted');
        this.load();
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  private load(): void {
    this.memoService.list().subscribe({
      next: (memos) => {
        this.memos.set(memos);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.memos.set([]);
        this.loading.set(false);
        this.toastr.error(apiMessage(error));
      },
    });
  }
}
