namespace TopicalBirdAPI.Constants
{
    
    public static class ErrorMessages
    {
        // Auth
        public const string UnauthorizedAction = "You need to be logged in to perform this action.";
        public const string UnauthorizedPostComment = "You need to be logged in to post a comment.";
        public const string UnauthorizedUserNotFound = "User not found.";
        public const string ForbiddenAction = "You do not have permission to perform this action.";

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

        // Users
        public const string UserNotFound = "User not found.";
        public const string UserBanned = "This user is banned.";
        public const string UserNotAuthorized = "You are not authorized to perform this action.";
        public const string UserAlreadyUnbanned = "This user is not banned.";
        public const string UserAlreadyAdmin = "This user is already an admin.";

        // Votes
        public const string VoteAlreadyExists = "You have already voted on this post.";
        public const string VoteNotFound = "Vote not found.";

        // Media
        public const string MediaNotFound = "Media not found.";
        public const string InvalidMediaUrl = "Invalid media URL provided.";

        // General
        public const string InvalidRequest = "Invalid request data.";
        public const string InternalServerError = "An unexpected error occurred. Please try again later.";
    }
}
