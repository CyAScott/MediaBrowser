namespace MediaBrowser.Users;

public class ChangePasswordRequest
{
    [Required, MinLength(6), MaxLength(50)]
    public required string OldPassword { get; init; }
    [Required, Password]
    public required string NewPassword { get; init; }
}

public class UserRegisterRequest
{
    [Required, RegularExpression(@"^[a-z][\w\-\._]{0,48}[a-z\d]$")]
    public required string Username { get; init; }

    [Required, Password]
    public required string Password { get; init; }
}

public class UserLoginRequest
{
    [Required, RegularExpression(@"^[a-z][\w\-\._]{0,48}[a-z\d]$")]
    public required string Username { get; init; }

    [Required, MinLength(6), MaxLength(50)]
    public required string Password { get; init; }
}

public class PasswordAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string password || password.Length < 6 || password.Length > 50 ||
            !password.Any(char.IsUpper) || !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit) || password.All(char.IsLetterOrDigit))
        {
            return new ValidationResult("Password must be at least 6 characters long and include uppercase, lowercase, digit, and special character.");
        }

        return ValidationResult.Success;
    }
}