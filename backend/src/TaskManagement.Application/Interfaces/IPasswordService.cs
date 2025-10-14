namespace TaskManagement.Application.Interfaces;

public interface IPasswordService
{
    string HashPassword(string? userPassword = null);
    bool VerifyPassword(string password, string passwordHashed);
}
