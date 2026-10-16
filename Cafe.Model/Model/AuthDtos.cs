using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cafe.Model.Model
{
    public class RegistrationRequestDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "Only alphabetic characters, spaces, and hyphens allowed")]
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email format")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^(?:\+91|91)?[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian mobile number")]
        [JsonPropertyName("mobileNumber")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).{12,}$",
            ErrorMessage = "Password must be at least 12 characters with uppercase, lowercase, number, and special character")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [MinLength(10, ErrorMessage = "Address must be at least 10 characters")]
        [MaxLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must accept the Privacy Policy and Terms of Service")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the Privacy Policy and Terms of Service")]
        [JsonPropertyName("privacyPolicyAccepted")]
        public bool PrivacyPolicyAccepted { get; set; }
    }

    public class RegistrationChallengeResponseDto
    {
        [JsonPropertyName("challengeId")]
        public string ChallengeId { get; set; } = string.Empty;

        [JsonPropertyName("maskedEmail")]
        public string MaskedEmail { get; set; } = string.Empty;

        [JsonPropertyName("expiresInSeconds")]
        public int ExpiresInSeconds { get; set; }

        [JsonPropertyName("resendAvailableInSeconds")]
        public int ResendAvailableInSeconds { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class VerifyOtpRequestDto
    {
        [Required(ErrorMessage = "Challenge ID is required")]
        [JsonPropertyName("challengeId")]
        public string ChallengeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verification code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Please enter a valid 6-digit verification code")]
        [JsonPropertyName("otp")]
        public string Otp { get; set; } = string.Empty;
    }

    public class RegistrationCompletionResponseDto
    {
        [JsonPropertyName("registrationCompleted")]
        public bool RegistrationCompleted { get; set; }

        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("accountStatus")]
        public string AccountStatus { get; set; } = "ACTIVE"; // ACTIVE | PENDING_APPROVAL | SUSPENDED

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("adminName")]
        public string? AdminName { get; set; }
    }

    public class ResendOtpRequestDto
    {
        [Required(ErrorMessage = "Challenge ID is required")]
        [JsonPropertyName("challengeId")]
        public string ChallengeId { get; set; } = string.Empty;
    }

    public class ResendOtpResponseDto
    {
        [JsonPropertyName("maskedEmail")]
        public string MaskedEmail { get; set; } = string.Empty;

        [JsonPropertyName("expiresInSeconds")]
        public int ExpiresInSeconds { get; set; }

        [JsonPropertyName("resendAvailableInSeconds")]
        public int ResendAvailableInSeconds { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class CancelRegistrationRequestDto
    {
        [Required(ErrorMessage = "Challenge ID is required")]
        [JsonPropertyName("challengeId")]
        public string ChallengeId { get; set; } = string.Empty;
    }

    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("rememberMe")]
        public bool RememberMe { get; set; } = false;
    }

    public class LoginResponseDto
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("user")]
        public UserDto User { get; set; } = new UserDto();

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class UserDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("mobileNumber")]
        public string MobileNumber { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("accountStatus")]
        public string AccountStatus { get; set; } = string.Empty;
    }
}
