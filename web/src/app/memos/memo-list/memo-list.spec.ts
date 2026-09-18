import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { MemoList } from './memo-list';
import { Memo } from '../memo';
import { MemoService } from '../memo.service';

const memos: Memo[] = [
  { id: 1, title: 'Groceries', body: 'Milk, bread, eggs', createdAt: '2026-09-18T14:01:00Z' },
  { id: 2, title: 'Ideas', body: 'Rename the project.', createdAt: '2026-09-18T14:05:00Z' },
];

class MemoServiceStub {
  memos = [...memos];
  listAnswer: Observable<Memo[]> | null = null;
  calls: string[] = [];

  list(): Observable<Memo[]> {
    this.calls.push('list');
    return this.listAnswer ?? of(this.memos);
  }

  delete(id: number): Observable<void> {
    this.calls.push(`delete ${id}`);
    this.memos = this.memos.filter((m) => m.id !== id);
    return of(undefined);
  }
}

describe('MemoList', () => {
  let fixture: ComponentFixture<MemoList>;
  let stub: MemoServiceStub;
  let toastr: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  const root = () => fixture.nativeElement as HTMLElement;
  const cards = () => root().querySelectorAll('article');

  const create = async () => {
    fixture = TestBed.createComponent(MemoList);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    stub = new MemoServiceStub();
    toastr = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [MemoList],
      providers: [
        provideRouter([]),
        { provide: MemoService, useValue: stub },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
  });

  it('lists my memos with their title, date and text', async () => {
    await create();

    expect(cards().length).toBe(2);
    expect(cards()[0].querySelector('h2')?.textContent?.trim()).toBe('Groceries');
    expect(cards()[0].querySelector('.card-text')?.textContent?.trim()).toBe('Milk, bread, eggs');
    expect(cards()[0].textContent).toContain('Sep 18, 2026');
  });

  it('says when there is nothing yet', async () => {
    stub.memos = [];
    await create();

    expect(cards().length).toBe(0);
    expect(root().querySelector('[data-testid="empty"]')?.textContent).toContain('No memos yet');
  });

  it('deletes a memo, says so and reloads', async () => {
    await create();

    cards()[1].querySelector<HTMLButtonElement>('button.btn-danger')?.click();
    await fixture.whenStable();

    expect(stub.calls).toEqual(['list', 'delete 2', 'list']);
    expect(toastr.success).toHaveBeenCalledWith('Memo deleted');
    expect(cards().length).toBe(1);
  });

  it('shows the message when the list cannot be loaded', async () => {
    stub.listAnswer = throwError(() => new HttpErrorResponse({ status: 0 }));
    await create();

    expect(toastr.error).toHaveBeenCalledWith(expect.stringContaining('did not answer'));
    expect(root().querySelector('[data-testid="empty"]')).not.toBeNull();
  });
});
