using SIPOTEK.Models;

public class AppState
{
    public bool IsLoggedIn { get; set; } = false;
    public User? CurrentUser { get; set; }
}
