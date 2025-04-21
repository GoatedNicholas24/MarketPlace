using Supabase;
using Supabase.Gotrue;
using System.Threading.Tasks;
using System;
using Xamarin.Essentials;

namespace MarketPlace.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _supabaseClient;
        private User _currentUser;

        public AuthService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<Session> GetCurrentSession()
        {
            return _supabaseClient.Auth.CurrentSession;
        }

        public async Task<User> GetCurrentUser()
        {
            try
            {
                if (_currentUser != null)
                    return _currentUser;

                var session = await GetCurrentSession();
                if (session?.User == null)
                    return null;

                var result = await _supabaseClient
                    .From<User>()
                    .Where(x => x.email == session.User.Email)
                    .Single();

                _currentUser = result;
                return _currentUser;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current user: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SignIn(string email, string password)
        {
            try
            {
                var response = await _supabaseClient.Auth.SignIn(email, password);
                if (response?.User != null)
                {
                    _currentUser = null; // Reset cached user
                    Xamarin.Essentials.Preferences.Set("UserEmail", email);
                    Xamarin.Essentials.Preferences.Set("isLoggedIn", true);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error signing in: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SignUp(string email, string password, string username)
        {
            try
            {
                var response = await _supabaseClient.Auth.SignUp(email, password);
                if (response?.User != null)
                {
                    // Create user profile
                    var user = new User
                    {
                        id = response.User.Id,
                        email = email,
                        username = username,
                        CreatedAt = DateTime.UtcNow 
                    };

                    await _supabaseClient.From<User>().Insert(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error signing up: {ex.Message}");
                return false;
            }
        }

        public async Task SignOut()
        {
            try
            {
                await _supabaseClient.Auth.SignOut();
                _currentUser = null;
                Xamarin.Essentials.Preferences.Remove("UserEmail");
                Xamarin.Essentials.Preferences.Set("isLoggedIn", false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error signing out: {ex.Message}");
            }
        }

        public async Task<bool> UpdateProfile(string username, string avatarUrl = null)
        {
            try
            {
                var user = await GetCurrentUser();
                if (user == null)
                    return false;

                user.username = username;
                if (!string.IsNullOrEmpty(avatarUrl))
                    user.avatar = avatarUrl;

                await _supabaseClient.From<User>().Update(user);
                _currentUser = user;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ChangePassword(string currentPassword, string newPassword)
        {
            try
            {
                var user = await GetCurrentUser();
                if (user == null)
                    return false;

                await _supabaseClient.Auth.Update(new Supabase.Gotrue.UserAttributes
                {
                    Password = newPassword
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing password: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetPassword(string email)
        {
            try
            {
                await _supabaseClient.Auth.ResetPasswordForEmail(email);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return false;
            }
        }
    }
} 