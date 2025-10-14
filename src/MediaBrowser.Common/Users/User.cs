namespace MediaBrowser.Users;

[Table("users")]
public class UserEntity
{
    [Column("id"), Key]
    public Guid Id { get; init; }

    [Column("user_name"), MaxLength(50)]
    public required string Username { get; set; }

    [Column("password_hash"), MaxLength(50)]
    public required string PasswordHash { get; set; }
    
    public UserReadModel ToReadModel() => new()
    {
        Id = Id,
        Username = Username
    };
}

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}

public class UserReadModel
{
    public Guid Id { get; init; }
    public required string Username { get; init; }
}