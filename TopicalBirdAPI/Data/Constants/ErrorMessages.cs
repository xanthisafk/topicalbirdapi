namespace TopicalBirdAPI.Data.Constants
{

    public static class ErrorMessages
    {
        // Auth
        public const string UnauthorizedAction = "You need to be logged in to perform this action.";
        public const string UnauthorizedUserNotFound = "User not found.";
        public const string ForbiddenAction = "You do not have permission to perform this action.";
        public const string InvalidCredentials = "Email or password was incorrect.";
        public const string InvalidPassword = "Given password is invalid.";
        public const string UserMalformedPassword = "Password must be at least 8 characters long, contain a mix of uppercase and lowercase letters, at least one number, and at least one special symbol.";

        // Comments
        public const string CommentNotFound = "Comment not found.";
        public const string CommentEmpty = "Comment cannot be empty.";
        public const string CommentPostMismatch = "Post ID in route does not match Post ID in body.";

        // Posts
        public const string PostNotFound = "Post with given id not found.";
        public const string PostDeleted = "Post has been deleted.";

        // Nests
        public const string NestNotFound = "Nest not found.";
        public const string NestTitleRequired = "Nest title is required.";
        public const string NestTitleConflict = "A nest with this title already exists.";

        // Users
        public const string UserNotFound = "User not found.";
        public const string UserBanned = "This user is banned.";
        public const string UserAlreadyUnbanned = "This user is not banned.";
        public const string UserAlreadyAdmin = "This user is already an admin.";
        public const string UserEmailConflict = "This email is already in use.";
        public const string UserHandleConflict = "This handle is already taken.";
        public const string UserCantCreate = "Unable to create a new user.";
        public const string UserHandleChange = "Cannot change username.";

        // Votes
        public const string VoteAlreadyExists = "You have already voted on this post.";
        public const string VoteNotFound = "Vote not found.";
        public const string VoteNotCast = "Your vote was not cast.";

        // Media
        public const string MediaNotFound = "Media not found.";
        public const string InvalidMediaUrl = "Invalid media URL provided.";
        public const string UnsupportedFileType = "The uploaded file type is not supported.";
        public const string FileTooLarge = "The uploaded file is too large. Maximum allowed size is 05MB.";
        public const string FailedToProcessMedia = "Failed to process uploaded media file.";

        // Search
        public const string SearchNoQuery = "No string provided to search.";
        public const string QueryTooSmall = "Search string must be at least 3 characters long.";
        public const string SearchNoResults = "Your search returned no matching results.";

        // Pagination
        public const string InvalidPageNumber = "The requested page number is invalid.";
        public const string InvalidLimit = "The requested limit (items per page) is invalid or exceeds the maximum.";


        // General
        public const string InvalidRequest = "Invalid request data.";
        public const string InternalServerError = "An unexpected error occurred. Please try again later.";
        public const string ResourceConflict = "The request conflicts with existing resource.";
        public const string NotImplemented = "The feature is not yet implemented.";
        public const string DeprecationWarning = "This feature is scheduled for deprecation in a future release. Please consult the API documentation for migration paths.";
        public const string DeprecatedError = "Error: This feature has been removed and is no longer supported. Please check the API documentation for the replacement feature.";
    }
}
