using System.Collections.Generic;

namespace IdentityService.Models
{
    public class AuthenticationResult
    {
        public bool IsSuccessful { get; }
        public IDictionary<string, string> Properties { get; }
        public AuthenticationError? Error { get; }

        public string OriginalUserName { get; }

        private AuthenticationResult(bool isSuccessful, AuthenticationError? error = null, string originalUserName = null, IDictionary<string, string> properties = null)
        {
            IsSuccessful = isSuccessful;
            Error = error;
            OriginalUserName = originalUserName;
            Properties = properties;
        }

        public static AuthenticationResult Successful(string originalUserName, IDictionary<string, string> properties)
        {
            if (originalUserName == null)
            {
                throw new System.ArgumentNullException(nameof(originalUserName));
            }

            if (properties == null)
            {
                throw new System.ArgumentNullException(nameof(properties));
            }

            return new AuthenticationResult(true, null, originalUserName, properties);
        }

        public static AuthenticationResult Failed(AuthenticationError error) => new AuthenticationResult(false, error);
    }
}