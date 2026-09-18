import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { MemoNew } from './memo-new';
import { Memo } from '../memo';
import { MemoService } from '../memo.service';

describe('MemoNew', () => {
  let fixture: ComponentFixture<MemoNew>;
  let create: ReturnType<typeof vi.fn>;
  let toastr: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };
  let navigate: ReturnType<typeof vi.spyOn>;

  const root = () => fixture.nativeElement as HTMLElement;
  const fill = async (title: string, body: string) => {
    const titleInput = root().querySelector<HTMLInputElement>('#title')!;
    const bodyInput = root().querySelector<HTMLTextAreaElement>('#body')!;
    titleInput.value = title;
    titleInput.dispatchEvent(new Event('input'));
    bodyInput.value = body;
    bodyInput.dispatchEvent(new Event('input'));
    await fixture.whenStable();
  };
  const submit = async () => {
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  };

  beforeEach(async () => {
    create = vi.fn((): Observable<Memo> =>
      of({ id: 3, title: 'Groceries', body: 'Milk', createdAt: '' }),
    );
    toastr = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [MemoNew],
      providers: [
        provideRouter([]),
        { provide: MemoService, useValue: { create } },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture = TestBed.createComponent(MemoNew);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  });

  it('sends the title and text, then goes back to the list', async () => {
    await fill('Groceries', 'Milk');
    await submit();

    expect(create).toHaveBeenCalledWith({ title: 'Groceries', body: 'Milk' });
    expect(toastr.success).toHaveBeenCalledWith('Memo created');
    expect(navigate).toHaveBeenCalledWith(['/memos']);
  });

  it('shows the message of a refused memo and stays on the form', async () => {
    create.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: { message: 'You already have a memo with that title.' },
          }),
      ),
    );
    await fill('Groceries', 'Milk');
    await submit();

    expect(toastr.error).toHaveBeenCalledWith('You already have a memo with that title.');
    expect(navigate).not.toHaveBeenCalled();
  });
});
