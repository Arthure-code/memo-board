import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Memo, NewMemo } from './memo';

export const MEMOS_URL = 'http://localhost:5047/api/memos';

// No account name travels with these calls: the API reads it from the
// token the interceptor attaches.
@Injectable({ providedIn: 'root' })
export class MemoService {
  private readonly http = inject(HttpClient);

  list(): Observable<Memo[]> {
    return this.http.get<Memo[]>(MEMOS_URL);
  }

  create(memo: NewMemo): Observable<Memo> {
    return this.http.post<Memo>(MEMOS_URL, memo);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${MEMOS_URL}/${id}`);
  }
}
