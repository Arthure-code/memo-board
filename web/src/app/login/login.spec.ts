import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { Login } from './login';
import { Session } from '../auth/session';
import { SessionService } from '../auth/session.service';

describe('Login', () => {
  let fixture: ComponentFixture<Login>;
  let signIn: ReturnType<typeof vi.fn>;
  let toastr: { error: ReturnType<typeof vi.fn> };
  let navigate: ReturnType<typeof vi.spyOn>;

  const root = () => fixture.nativeElement as HTMLElement;
  const fill = async (userName: string, password: string) => {
    const name = root().querySelector<HTMLInputElement>('#userName')!;
    const pass = root().querySelector<HTMLInputElement>('#password')!;
    name.value = userName;
    name.dispatchEvent(new Event('input'));
    pass.value = password;
    pass.dispatchEvent(new Event('input'));
    await fixture.whenStable();
  };
  const submit = async () => {
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  };

  beforeEach(async () => {
    signIn = vi.fn((): Observable<Session> =>
      of({ userName: 'nadia', token: 't', expiresAt: '', lastLoginAt: null }),
    );
    toastr = { error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideRouter([]),
        { provide: SessionService, useValue: { signIn } },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture = TestBed.createComponent(Login);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  });

  it('refuses to submit an empty form', async () => {
    await submit();

    expect(signIn).not.toHaveBeenCalled();
    expect(toastr.error).toHaveBeenCalledWith('Please enter your user name and password');
  });

  it('signs in with what was typed and goes to the memos', async () => {
    await fill('nadia', 'correct horse battery');
    await submit();

    expect(signIn).toHaveBeenCalledWith({ userName: 'nadia', password: 'correct horse battery' });
    expect(navigate).toHaveBeenCalledWith(['/memos']);
  });

  it('shows the message of a refused login', async () => {
    signIn.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 401,
            error: { message: 'Wrong user name or password.' },
          }),
      ),
    );
    await fill('nadia', 'wrong');
    await submit();

    expect(toastr.error).toHaveBeenCalledWith('Wrong user name or password.');
    expect(navigate).not.toHaveBeenCalled();
  });
});
