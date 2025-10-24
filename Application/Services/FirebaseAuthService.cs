using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using VeriWork_Admin.Core.Entities;

namespace VeriWork_Admin.Application.Services
{
    public class FirebaseAuthService
    {
        private readonly FirestoreDb _db;

        public FirebaseAuthService(FirestoreDb db)
        {
            _db = db;
        }

        // ✅ Authenticate user using Firebase
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            try
            {
                // Firebase Admin SDK doesn’t directly “sign in” with password
                // So we verify credentials by using Firebase REST API

                using var client = new HttpClient();
                var apiKey = "AIzaSyASn1CsY7d8U4telwHoax3JMZzAnTp3Y5g"; //  from Firebase Console → Project Settings → Web API Key

                var payload = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                var response = await client.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}",
                    payload
                );

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

                // Fetch user details from Firestore
                var userSnapshot = await _db.Collection("Users")
                    .WhereEqualTo("Email", email)
                    .GetSnapshotAsync();

                var userDoc = userSnapshot.Documents.FirstOrDefault();
                if (userDoc == null) return null;

                var user = userDoc.ConvertTo<User>();

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                return null;
            }
        }
    }
}
