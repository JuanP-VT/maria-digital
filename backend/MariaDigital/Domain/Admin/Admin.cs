namespace MariaDigital.Domain.Admin;

public class Admin(string username, string hashedPassword, string email)
{
	public int Id { get; private set; }
	public string Username { get; private set; } = username;
	public string HashedPassword { get; private set; } = hashedPassword;
	public string Email { get; private set; } = email;
	public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

	public void SetHashedPassword(string hashedPassword)
	{
		HashedPassword = hashedPassword;
	}
}
