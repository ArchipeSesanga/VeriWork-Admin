using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;

namespace VeriWork_Admin.Infrastructure.Config;

public  class FirebaseInitializer
{
    public static FirestoreDb Initialize(string projectId, string credentialsPath)
    {
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
        return FirestoreDb.Create(projectId);
    }
}