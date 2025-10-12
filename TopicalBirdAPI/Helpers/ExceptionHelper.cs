using Npgsql;

namespace TopicalBirdAPI.Helpers
{
    public static class ExceptionHelper
    {
        public static string GetFriendlyErrorMessage(Exception ex)
        {
            // Check if it's a PostgresException (database error)
            if (ex is PostgresException pgEx)
            {
                // Check by SQLSTATE error code
                switch (pgEx.SqlState)
                {
                    case "23505":
                        return pgEx.ConstraintName switch
                        {
                            "users_email_key" => "A user with this email already exists.",
                            "users_username_key" => "A user with this username already exists.",
                            _ => "A unique constraint was violated."
                        };

                    case "23503": // foreign_key_violation
                        return "A related record is missing (foreign key constraint violated).";

                    case "23514": // check_violation
                        return "A data check constraint was violated.";

                    case "22P02": // invalid_text_representation
                        return "Invalid input format.";

                    default:
                        // fallback for other SQLSTATE codes
                        return $"Database error ({pgEx.SqlState}): {pgEx.MessageText}";
                }
            }

            // Not a Postgres exception — just return its message
            return ex.Message;
        }
    }
}
