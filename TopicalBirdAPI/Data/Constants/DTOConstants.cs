namespace TopicalBirdAPI.Data.Constants
{
    public static class DTOConstants
    {
        public const string PasswordErrorMessage = "Password must be at least 8 characters long, include one uppercase letter, one number, and one special character.";
        public const string PasswordRegex = "^(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9\\s])(?=.{8,}).*$";

    }
}
