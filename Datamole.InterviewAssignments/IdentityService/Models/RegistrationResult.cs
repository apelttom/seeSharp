namespace IdentityService.Models
{
    public class RegistrationResult
    {
        public bool IsSuccessful { get; }

        public RegistrationError? Error { get; }

        private RegistrationResult(bool isSuccessful, RegistrationError? error = null)
        {
            IsSuccessful = isSuccessful;
            Error = error;
        }

        public static RegistrationResult Successful() => new RegistrationResult(true);

        public static RegistrationResult Failed(RegistrationError error) => new RegistrationResult(false, error);
    }
}