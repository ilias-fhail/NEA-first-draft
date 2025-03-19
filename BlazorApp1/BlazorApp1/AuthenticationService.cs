using Microsoft.JSInterop;
using System.Threading.Tasks;


// authentication class which deals with verifiying for the front end whether a user is logged in or not and then assigning a userKey and a login status.

public class AuthenticationService
{
    public int UserKey = 0;
    public event Action? OnAuthenticationStateChanged;

    public AuthenticationService(){}

    public async Task<bool> IsAuthenticatedAsync() // checks if the user is authenticated by having a valid userKey
    {
        if (UserKey == 0) // default key for being logged out
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public async Task<int> GetUserIdAsync() // can be called whenever the front end needs to determine who is logged in if anyone
    {
        return UserKey;
    }

    public async Task LoginAsync(int userId) // setting the user key to match the userId
    {
        UserKey = userId;
        NotifyAuthenticationStateChanged();
    }
    public async Task LogoutAsync() // resetting the user key upon logging out
    {
        UserKey = 0;
    }
    private void NotifyAuthenticationStateChanged() // letting the front end know a state change has been made to reloud the UI with the updated information
    {
        OnAuthenticationStateChanged?.Invoke();
    }
}



