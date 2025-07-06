using SIPOTEK.Data;
using SIPOTEK.Models;
using Microsoft.EntityFrameworkCore;

public class LoginService
{
    private readonly SipotekDbContext _context;

    public LoginService(SipotekDbContext context)
    {
        _context = context;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
    }
}
