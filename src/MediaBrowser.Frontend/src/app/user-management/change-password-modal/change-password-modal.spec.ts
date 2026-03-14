import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { UsersService } from '../../services/users.service';
import { ChangePasswordModalComponent } from './change-password-modal';

interface ChangePasswordModalTestContext {
  component: ChangePasswordModalComponent;
  mocks: {
    usersService: {
      changePassword: ReturnType<typeof vi.fn>;
    };
  };
}

async function createComponent(): Promise<ChangePasswordModalTestContext> {
  const mocks = {
    usersService: {
      changePassword: vi.fn()
    }
  };

  await TestBed.configureTestingModule({
    imports: [ChangePasswordModalComponent],
    providers: [{ provide: UsersService, useValue: mocks.usersService }]
  }).compileComponents();

  const fixture = TestBed.createComponent(ChangePasswordModalComponent);
  fixture.detectChanges();

  return {
    component: fixture.componentInstance,
    mocks
  };
}

describe('ChangePasswordModalComponent', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('creates the component', async () => {
    const { component } = await createComponent();

    expect(component).toBeTruthy();
  });

  it('validates required fields', async () => {
    const { component, mocks } = await createComponent();

    component.oldPassword = '';
    component.newPassword = 'new-secret';
    component.confirmPassword = 'new-secret';

    await component.onSubmit();

    expect(component.errorMessage).toBe('All fields are required');
    expect(component.isSubmitting).toBe(false);
    expect(mocks.usersService.changePassword).not.toHaveBeenCalled();
  });

  it('validates password confirmation match', async () => {
    const { component, mocks } = await createComponent();

    component.oldPassword = 'old-secret';
    component.newPassword = 'new-secret';
    component.confirmPassword = 'different-secret';

    await component.onSubmit();

    expect(component.errorMessage).toBe('New passwords do not match');
    expect(component.isSubmitting).toBe(false);
    expect(mocks.usersService.changePassword).not.toHaveBeenCalled();
  });

  it('validates minimum new password length', async () => {
    const { component, mocks } = await createComponent();

    component.oldPassword = 'old-secret';
    component.newPassword = '12345';
    component.confirmPassword = '12345';

    await component.onSubmit();

    expect(component.errorMessage).toBe('New password must be at least 6 characters long');
    expect(component.isSubmitting).toBe(false);
    expect(mocks.usersService.changePassword).not.toHaveBeenCalled();
  });

  it('changes password successfully and closes modal', async () => {
    const { component, mocks } = await createComponent();
    const detectChanges = vi.fn();
    const userUpdatingSpy = vi.spyOn(component.userUpdating, 'emit');
    const userUpdatingFailedSpy = vi.spyOn(component.userUpdatingFailed, 'emit');
    const closeSpy = vi.spyOn(component.close, 'emit');

    (component as any).cdr = { detectChanges };

    component.oldPassword = 'old-secret';
    component.newPassword = 'new-secret';
    component.confirmPassword = 'new-secret';
    mocks.usersService.changePassword.mockReturnValue(of(void 0));

    await component.onSubmit();

    expect(mocks.usersService.changePassword).toHaveBeenCalledWith({
      oldPassword: 'old-secret',
      newPassword: 'new-secret'
    });
    expect(userUpdatingSpy).toHaveBeenCalledTimes(1);
    expect(userUpdatingFailedSpy).not.toHaveBeenCalled();
    expect(closeSpy).toHaveBeenCalledTimes(1);
    expect(component.successMessage).toBe('Password changed successfully!');
    expect(component.errorMessage).toBe('');
    expect(component.isSubmitting).toBe(false);
    expect(detectChanges).toHaveBeenCalledTimes(1);
  });

  it('shows API error message and emits failure when changing password fails with message', async () => {
    const { component, mocks } = await createComponent();
    const detectChanges = vi.fn();
    const userUpdatingSpy = vi.spyOn(component.userUpdating, 'emit');
    const userUpdatingFailedSpy = vi.spyOn(component.userUpdatingFailed, 'emit');
    const closeSpy = vi.spyOn(component.close, 'emit');

    (component as any).cdr = { detectChanges };

    component.oldPassword = 'old-secret';
    component.newPassword = 'new-secret';
    component.confirmPassword = 'new-secret';
    mocks.usersService.changePassword.mockReturnValue(
      throwError(() => ({ error: { message: 'Current password is incorrect' } }))
    );

    await component.onSubmit();

    expect(userUpdatingSpy).toHaveBeenCalledTimes(1);
    expect(userUpdatingFailedSpy).toHaveBeenCalledTimes(1);
    expect(closeSpy).not.toHaveBeenCalled();
    expect(component.errorMessage).toBe('Current password is incorrect');
    expect(component.successMessage).toBe('');
    expect(component.isSubmitting).toBe(false);
    expect(detectChanges).toHaveBeenCalledTimes(1);
  });

  it('falls back to default error message when changing password fails without API message', async () => {
    const { component, mocks } = await createComponent();

    component.oldPassword = 'old-secret';
    component.newPassword = 'new-secret';
    component.confirmPassword = 'new-secret';
    mocks.usersService.changePassword.mockReturnValue(throwError(() => new Error('failure')));

    await component.onSubmit();

    expect(component.errorMessage).toBe('Failed to change password');
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
