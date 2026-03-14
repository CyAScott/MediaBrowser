import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { UsersService } from '../../services/users.service';
import { AddUserModalComponent } from './add-user-modal';

interface AddUserModalTestContext {
  component: AddUserModalComponent;
  mocks: {
    usersService: {
      register: ReturnType<typeof vi.fn>;
    };
  };
}

async function createComponent(): Promise<AddUserModalTestContext> {
  const mocks = {
    usersService: {
      register: vi.fn()
    }
  };

  await TestBed.configureTestingModule({
    imports: [AddUserModalComponent],
    providers: [{ provide: UsersService, useValue: mocks.usersService }]
  }).compileComponents();

  const fixture = TestBed.createComponent(AddUserModalComponent);
  fixture.detectChanges();

  return {
    component: fixture.componentInstance,
    mocks
  };
}

describe('AddUserModalComponent', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('creates the component', async () => {
    const { component } = await createComponent();

    expect(component).toBeTruthy();
  });

  it('validates required fields', async () => {
    const { component, mocks } = await createComponent();

    component.username = '';
    component.password = 'secret123';
    component.confirmPassword = 'secret123';

    await component.onSubmit();

    expect(component.errorMessage).toBe('All fields are required');
    expect(component.isSubmitting).toBe(false);
    expect(mocks.usersService.register).not.toHaveBeenCalled();
  });

  it('validates password confirmation match', async () => {
    const { component, mocks } = await createComponent();

    component.username = 'alice';
    component.password = 'secret123';
    component.confirmPassword = 'different123';

    await component.onSubmit();

    expect(component.errorMessage).toBe('Passwords do not match');
    expect(component.isSubmitting).toBe(false);
    expect(mocks.usersService.register).not.toHaveBeenCalled();
  });

  it('validates minimum password length', async () => {
    const { component, mocks } = await createComponent();

    component.username = 'alice';
    component.password = '12345';
    component.confirmPassword = '12345';

    await component.onSubmit();

    expect(component.errorMessage).toBe('Password must be at least 6 characters long');
    expect(component.isSubmitting).toBe(false);
    expect(mocks.usersService.register).not.toHaveBeenCalled();
  });

  it('validates minimum username length', async () => {
    const { component, mocks } = await createComponent();

    component.username = 'ab';
    component.password = 'secret123';
    component.confirmPassword = 'secret123';

    await component.onSubmit();

    expect(component.errorMessage).toBe('Username must be at least 3 characters long');
    expect(component.isSubmitting).toBe(false);
    expect(mocks.usersService.register).not.toHaveBeenCalled();
  });

  it('registers user successfully and emits progress events', async () => {
    const { component, mocks } = await createComponent();
    const detectChanges = vi.fn();
    const userAddingSpy = vi.spyOn(component.userAdding, 'emit');
    const userAddedSpy = vi.spyOn(component.userAdded, 'emit');
    const userAddingFailedSpy = vi.spyOn(component.userAddingFailed, 'emit');

    (component as any).cdr = { detectChanges };

    component.username = 'alice';
    component.password = 'secret123';
    component.confirmPassword = 'secret123';
    mocks.usersService.register.mockReturnValue(of({ id: '1', username: 'alice' }));

    await component.onSubmit();

    expect(mocks.usersService.register).toHaveBeenCalledWith({
      username: 'alice',
      password: 'secret123'
    });
    expect(userAddingSpy).toHaveBeenCalledTimes(1);
    expect(userAddedSpy).toHaveBeenCalledTimes(1);
    expect(userAddingFailedSpy).not.toHaveBeenCalled();
    expect(component.successMessage).toBe('User added successfully!');
    expect(component.errorMessage).toBe('');
    expect(component.isSubmitting).toBe(false);
    expect(detectChanges).toHaveBeenCalledTimes(1);
  });

  it('shows API error message and emits failure when register fails with message', async () => {
    const { component, mocks } = await createComponent();
    const detectChanges = vi.fn();
    const userAddingSpy = vi.spyOn(component.userAdding, 'emit');
    const userAddedSpy = vi.spyOn(component.userAdded, 'emit');
    const userAddingFailedSpy = vi.spyOn(component.userAddingFailed, 'emit');

    (component as any).cdr = { detectChanges };

    component.username = 'alice';
    component.password = 'secret123';
    component.confirmPassword = 'secret123';
    mocks.usersService.register.mockReturnValue(
      throwError(() => ({ error: { message: 'Username already exists' } }))
    );

    await component.onSubmit();

    expect(userAddingSpy).toHaveBeenCalledTimes(1);
    expect(userAddedSpy).not.toHaveBeenCalled();
    expect(userAddingFailedSpy).toHaveBeenCalledTimes(1);
    expect(component.errorMessage).toBe('Username already exists');
    expect(component.successMessage).toBe('');
    expect(component.isSubmitting).toBe(false);
    expect(detectChanges).toHaveBeenCalledTimes(1);
  });

  it('falls back to default error message when register fails without API message', async () => {
    const { component, mocks } = await createComponent();

    component.username = 'alice';
    component.password = 'secret123';
    component.confirmPassword = 'secret123';
    mocks.usersService.register.mockReturnValue(throwError(() => new Error('no message payload')));

    await component.onSubmit();

    expect(component.errorMessage).toBe('Failed to add user');
    expect(component.isSubmitting).toBe(false);
  });

  it('emits close when onClose is called', async () => {
    const { component } = await createComponent();
    const closeSpy = vi.spyOn(component.close, 'emit');

    component.onClose();

    expect(closeSpy).toHaveBeenCalledTimes(1);
  });

  it('closes on backdrop click only when clicking the backdrop itself', async () => {
    const { component } = await createComponent();
    const closeSpy = vi.spyOn(component, 'onClose');

    const sameTarget = {};
    component.onBackdropClick({ target: sameTarget, currentTarget: sameTarget } as unknown as Event);
    expect(closeSpy).toHaveBeenCalledTimes(1);

    const target = {};
    const currentTarget = {};
    component.onBackdropClick({ target, currentTarget } as unknown as Event);
    expect(closeSpy).toHaveBeenCalledTimes(1);
  });
});
