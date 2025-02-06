using Microsoft.JSInterop;
using System.Threading.Tasks;

public class AuthenticationService
{
    public int UserKey = 0;
    public event Action? OnAuthenticationStateChanged;

    public AuthenticationService()
    {
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        if (UserKey == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public async Task<int> GetUserIdAsync()
    {
        return UserKey;
    }

    public async Task LoginAsync(int userId)
    {
        UserKey = userId;
        NotifyAuthenticationStateChanged();
    }
    public async Task LogoutAsync()
    {
        UserKey = 0;
    }
    private void NotifyAuthenticationStateChanged()
    {
        OnAuthenticationStateChanged?.Invoke();
    }
}



