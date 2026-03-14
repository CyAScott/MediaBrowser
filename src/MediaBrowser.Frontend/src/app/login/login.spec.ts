import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SearchComponent } from '../search/search';
import { UsersService } from '../services/users.service';
import { LoginComponent } from './login';

interface LoginTestContext {
  component: LoginComponent;
  fixture: ReturnType<typeof TestBed.createComponent<LoginComponent>>;
  mocks: {
    router: {
      navigate: ReturnType<typeof vi.fn>;
    };
    usersService: {
      login: ReturnType<typeof vi.fn>;
    };
  };
}

async function createComponent(): Promise<LoginTestContext> {
  const mocks = {
    router: {
      navigate: vi.fn().mockResolvedValue(true)
    },
    usersService: {
      login: vi.fn()
    }
  };

  await TestBed.configureTestingModule({
    imports: [LoginComponent],
    providers: [
      { provide: Router, useValue: mocks.router },
      { provide: UsersService, useValue: mocks.usersService }
    ]
  }).compileComponents();

  const fixture = TestBed.createComponent(LoginComponent);
  fixture.detectChanges();

  return {
    component: fixture.componentInstance,
    fixture,
    mocks
  };
}

describe('LoginComponent', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('creates the component', async () => {
    const { component } = await createComponent();

    expect(component).toBeTruthy();
  });

  it('shows validation message when username or password is blank', async () => {
    const { component, mocks } = await createComponent();

    component.username = '   ';
    component.password = 'secret';
    await component.onSubmit();

    expect(component.errorMessage).toBe('Please enter both username and password');
    expect(component.isLoading).toBe(false);
    expect(mocks.usersService.login).not.toHaveBeenCalled();

    component.username = 'alice';
    component.password = '   ';
    await component.onSubmit();

    expect(component.errorMessage).toBe('Please enter both username and password');
    expect(component.isLoading).toBe(false);
    expect(mocks.usersService.login).not.toHaveBeenCalled();
  });

  it('logs in successfully, clears page state, and navigates to search', async () => {
    const { component, mocks } = await createComponent();
    const clearPagePositionSpy = vi
      .spyOn(SearchComponent, 'clearPagePositionState')
      .mockImplementation(() => {});
    const detectChanges = vi.fn();

    // Replace the injected CDR with a spy so we can assert finalization behavior.
    (component as any).cdr = { detectChanges };

    component.username = '  alice  ';
    component.password = 'password123';
    mocks.usersService.login.mockReturnValue(of({ id: '1', username: 'alice' }));

    await component.onSubmit();

    expect(mocks.usersService.login).toHaveBeenCalledWith({
      username: 'alice',
      password: 'password123'
    });
    expect(clearPagePositionSpy).toHaveBeenCalledTimes(1);
    expect(mocks.router.navigate).toHaveBeenCalledWith(['/search'], {
      queryParams: {
        sort: SearchComponent.DEFAULT_SORT
      },
      queryParamsHandling: 'replace'
    });
    expect(component.errorMessage).toBe('');
    expect(component.isLoading).toBe(false);
    expect(detectChanges).toHaveBeenCalledTimes(1);
  });

  it('shows a generic error message when login fails', async () => {
    const { component, mocks } = await createComponent();
    const detectChanges = vi.fn();

    (component as any).cdr = { detectChanges };

    component.username = 'alice';
    component.password = 'wrong-password';
    mocks.usersService.login.mockReturnValue(throwError(() => new Error('Login failed')));

    await component.onSubmit();

    expect(mocks.usersService.login).toHaveBeenCalledWith({
      username: 'alice',
      password: 'wrong-password'
    });
    expect(mocks.router.navigate).not.toHaveBeenCalled();
    expect(component.errorMessage).toBe('An error occurred during login. Please try again.');
    expect(component.isLoading).toBe(false);
    expect(detectChanges).toHaveBeenCalledTimes(1);
  });

  it('submits only on Enter for username keypress', async () => {
    const { component } = await createComponent();
    const submitSpy = vi.spyOn(component, 'onSubmit').mockResolvedValue();

    component.onUsernameKeyPress({ key: 'a' } as KeyboardEvent);
    expect(submitSpy).not.toHaveBeenCalled();

    component.onUsernameKeyPress({ key: 'Enter' } as KeyboardEvent);
    expect(submitSpy).toHaveBeenCalledTimes(1);
  });

  it('submits only on Enter for password keypress', async () => {
    const { component } = await createComponent();
    const submitSpy = vi.spyOn(component, 'onSubmit').mockResolvedValue();

    component.onPasswordKeyPress({ key: 'Tab' } as KeyboardEvent);
    expect(submitSpy).not.toHaveBeenCalled();

    component.onPasswordKeyPress({ key: 'Enter' } as KeyboardEvent);
    expect(submitSpy).toHaveBeenCalledTimes(1);
  });
});
