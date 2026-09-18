import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { Login } from './login/login';
import { Register } from './register/register';
import { MemoList } from './memos/memo-list/memo-list';
import { MemoNew } from './memos/memo-new/memo-new';

export const routes: Routes = [
  { path: '', component: Login },
  { path: 'register', component: Register },
  { path: 'memos', component: MemoList, canActivate: [authGuard] },
  { path: 'memos/new', component: MemoNew, canActivate: [authGuard] },
  { path: '**', redirectTo: 'memos' },
];
