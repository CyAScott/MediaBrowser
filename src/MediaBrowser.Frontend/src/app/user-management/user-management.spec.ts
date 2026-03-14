import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { UsersService } from '../services/users.service';
import { UserManagementComponent } from './user-management';

interface UserManagementTestContext {
  component: UserManagementComponent;
  mocks: {
    usersService: {
      getUsers: ReturnType<typeof vi.fn>;
      deleteUser: ReturnType<typeof vi.fn>;
    };
  };
}

async function createComponent(overrides?: {
  users?: Array<{ id: string; username: string }>;
}): Promise<UserManagementTestContext> {
  const users = overrides?.users ?? [];

  const mocks = {
    usersService: {
      getUsers: vi.fn().mockReturnValue(of(users)),
      deleteUser: vi.fn().mockReturnValue(of(void 0))
    }
  };

  await TestBed.configureTestingModule({
    imports: [UserManagementComponent],
    providers: [{ provide: UsersService, useValue: mocks.usersService }]
  }).compileComponents();

  const fixture = TestBed.createComponent(UserManagementComponent);
  fixture.detectChanges();

  // Clear lifecycle side effects so each test starts from a clean call history.
  mocks.usersService.getUsers.mockClear();
  mocks.usersService.deleteUser.mockClear();

  return {
    component: fixture.componentInstance,
    mocks
  };
}

describe('UserManagementComponent', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('creates the component', async () => {
    const { component } = await createComponent();

    expect(component).toBeTruthy();
  });

  it('calls loadUsers in ngOnInit', async () => {
    const { component } = await createComponent();
    const loadUsersSpy = vi.spyOn(component, 'loadUsers').mockResolvedValue();

    await component.ngOnInit();

    expect(loadUsersSpy).toHaveBeenCalledTimes(1);
  });

  it('loads users successfully and resets loading state', async () => {
    const { component, mocks } = await createComponent();
    const detectChanges = vi.fn();

    (component as any).cdr = { detectChanges };

    mocks.usersService.getUsers.mockReturnValue(
      of([
        { id: '1', username: 'alice' },
        { id: '2', username: 'bob' }
      ])
    );

    await component.loadUsers();

    expect(mocks.usersService.getUsers).toHaveBeenCalledTimes(1);
    expect(component.users).toEqual([
      { id: '1', username: 'alice' },
      { id: '2', username: 'bob' }
    ]);
    expect(component.isLoading).toBe(false);
    expect(detectChanges).toHaveBeenCalledTimes(1);
  });

  it('handles loadUsers errors and still resets loading state', async () => {
    const { component, mocks } = await createComponent();
    const detectChanges = vi.fn();
    const errorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

    (component as any).cdr = { detectChanges };

    mocks.usersService.getUsers.mockReturnValue(throwError(() => new Error('load failed')));

    await component.loadUsers();

    expect(errorSpy).toHaveBeenCalledWith('Error loading users:', expect.any(Error));
    expect(component.isLoading).toBe(false);
    expect(detectChanges).toHaveBeenCalledTimes(1);
  });

  it('does nothing when delete confirmation is canceled', async () => {
    const { component, mocks } = await createComponent();
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
    const loadUsersSpy = vi.spyOn(component, 'loadUsers').mockResolvedValue();

    await component.deleteUser('1');

    expect(confirmSpy).toHaveBeenCalledWith('Are you sure you want to delete this user?');
    expect(mocks.usersService.deleteUser).not.toHaveBeenCalled();
    expect(loadUsersSpy).not.toHaveBeenCalled();
  });

  it('deletes a user and reloads users when confirmation is accepted', async () => {
    const { component, mocks } = await createComponent();
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    const loadUsersSpy = vi.spyOn(component, 'loadUsers').mockResolvedValue();

    await component.deleteUser('2');

    expect(confirmSpy).toHaveBeenCalledTimes(1);
    expect(mocks.usersService.deleteUser).toHaveBeenCalledWith('2');
    expect(loadUsersSpy).toHaveBeenCalledTimes(1);
  });

  it('logs deletion errors and still reloads users', async () => {
    const { component, mocks } = await createComponent();
    const errorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    const loadUsersSpy = vi.spyOn(component, 'loadUsers').mockResolvedValue();

    mocks.usersService.deleteUser.mockReturnValue(throwError(() => new Error('delete failed')));

    await component.deleteUser('3');

    expect(errorSpy).toHaveBeenCalledWith('Error deleting user:', expect.any(Error));
    expect(loadUsersSpy).toHaveBeenCalledTimes(1);
  });

  it('toggles change-password modal visibility', async () => {
    const { component } = await createComponent();

    component.openChangePasswordModal();
    expect(component.isChangePasswordModalOpen).toBe(true);

    component.closeChangePasswordModal();
    expect(component.isChangePasswordModalOpen).toBe(false);
  });

  it('toggles add-user modal visibility', async () => {
    const { component } = await createComponent();

    component.openAddUserModal();
    expect(component.isAddUserModalOpen).toBe(true);

    component.closeAddUserModal();
    expect(component.isAddUserModalOpen).toBe(false);
  });

  it('closes add-user modal and reloads users when user is added', async () => {
    const { component } = await createComponent();
    const loadUsersSpy = vi.spyOn(component, 'loadUsers').mockResolvedValue();

    component.isAddUserModalOpen = true;
    await component.onUserAdded();

    expect(component.isAddUserModalOpen).toBe(false);
    expect(loadUsersSpy).toHaveBeenCalledTimes(1);
  });
});
