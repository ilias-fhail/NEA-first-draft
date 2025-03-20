using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

// this class implements my stack that is used for my undo button for the user to go back to their previous page in the front end

namespace BlazorApp1.Components
{
    public class NavigationService
    {
        private readonly Stack<string> _history = new Stack<string>(); // initialising the stack
        private readonly AuthenticationService _authService;
        private readonly NavigationManager _navigation;

        public NavigationService(AuthenticationService authService, NavigationManager navigation) // constructor initialising the shared instance of navigation and authservice
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }

        public void RecordNavigation(string url) // adding a visited page to the stack
        {
            if (_history.Count == 0 || _history.Peek() != url) // checks if the stack is empty of if the url doesnt equal the last item on the stack
            {
                _history.Push(url);
            }
        }

        public void Undo() // removing a page from the stack and going back to it
        {
            if (_history.Count > 1) // checking the stack is long enough
            {
                _history.Pop();  // Remove the latest item as it is the page it is currently on
                string previousPage = _history.Peek();  // Get the previous page after popping the first

                Console.WriteLine("Undo called");
                Console.WriteLine("Previous page: " + previousPage); // logging it in the console for testing reasons

                if (previousPage == "/") //checking if it goes back to the log in page
                {
                    _authService.LogoutAsync();
                    _authService.NotifyAuthenticationStateChanged(); // logging them out if it does for security
                }

                _navigation.NavigateTo(previousPage); // going back to previous page
            }
        }

        public bool CanUndo() => _history.Count > 1; //checks if the stack can be undone as it must not be empty
    }
}
