using Microsoft.AspNetCore.Mvc;
using VeriWork_Admin.Application.Services;
using FirebaseAdmin.Auth;

namespace VeriWork_Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly FaceService _faceService;
        private readonly AdminService _adminService;
        private readonly FirebaseStorageService _storageService;
        private readonly AuditLogService _auditLogService;

        public VerificationController(
            FaceService faceService,
            AdminService adminService,
            FirebaseStorageService storageService,
            AuditLogService auditLogService)
        {
            _faceService = faceService;
            _adminService = adminService;
            _storageService = storageService;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Compares the uploaded selfie with the employee's registered photo.
        /// If the face matches, the employee is automatically approved.
        /// </summary>
        [HttpPost("verify-selfie")]
        public async Task<IActionResult> VerifySelfie(
            [FromQuery] string uid,
            IFormFile selfie,
            [FromHeader(Name = "Authorization")] string? authHeader = null)
        {
            if (string.IsNullOrEmpty(uid))
                return BadRequest("UID is required.");

            if (selfie == null || selfie.Length == 0)
                return BadRequest("Selfie file is required.");

            // ✅ Step 1: (Optional) Validate Firebase token if provided
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader["Bearer ".Length..];
                var verifiedUid = await ValidateFirebaseTokenAsync(token);
                if (verifiedUid == null || verifiedUid != uid)
                    return Unauthorized("Invalid Firebase token or UID mismatch.");
            }

            // ✅ Step 2: Fetch employee data
            var user = await _adminService.GetProfileAsync(uid);
            if (user == null)
                return NotFound("User not found.");

            if (string.IsNullOrEmpty(user.PhotoUrl))
                return BadRequest("No registered photo found for this user.");

            // ✅ Step 3: Upload selfie to Firebase Storage
            string selfieUrl;
            try
            {
                var selfieFileName = $"selfies/{uid}/{Guid.NewGuid()}_{selfie.FileName}";
                selfieUrl = await _storageService.UploadFileAsync(selfie, selfieFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to upload selfie: {ex.Message}");
            }

            // ✅ Step 4: Call Azure Face API to verify both images
            var (isMatch, confidence, error) =
                await _faceService.VerifyByImageUrlsAsync(user.PhotoUrl, selfieUrl);

            // ✅ Step 5: Update Firestore based on result
            user.VerificationNotes = error ?? $"Confidence: {confidence:F3}";
            user.VerificationStatus = isMatch ? "Approved" : "Rejected";
            await _adminService.UpdateProfileAsync(user);

            // ✅ Step 6: Add audit log
            await _auditLogService.AddLogAsync(
                User.Identity?.Name ?? "MobileUser",
                "AutoVerification",
                $"{user.Email} verification {(isMatch ? "Approved" : "Rejected")} (confidence {confidence:F3})");

            // ✅ Step 7: Return a clean JSON response
            return Ok(new
            {
                success = isMatch,
                confidence,
                selfieUrl,
                registeredPhoto = user.PhotoUrl,
                status = user.VerificationStatus,
                message = isMatch
                    ? $"✅ Face matched with confidence {confidence:P2}. Verification Approved."
                    : error ?? $"❌ Face mismatch (confidence {confidence:P2}). Verification Rejected."
            });
        }

        /// <summary>
        /// Validates a Firebase ID token and returns the UID if valid.
        /// </summary>
        private async Task<string?> ValidateFirebaseTokenAsync(string idToken)
        {
            try
            {
                var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                return decoded.Uid;
            }
            catch
            {
                return null;
            }
        }
    }
}
