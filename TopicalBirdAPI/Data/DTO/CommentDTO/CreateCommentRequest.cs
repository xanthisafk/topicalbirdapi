using System.ComponentModel.DataAnnotations;
using TopicalBirdAPI.Data.Constants;

namespace TopicalBirdAPI.Data.DTO.CommentDTO
{
    /// <summary>
    /// Helper class used to submit a comment
    /// </summary>
    public class CreateCommentRequest
    {
        /// <summary>
        /// Content of comment. Min length: 1, Max len: 10,000.
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.FieldRequired + "Content")]
        [MinLength(1, ErrorMessage = ErrorMessages.CommentEmpty)]
        [MaxLength(10000, ErrorMessage = ErrorMessages.CommentTooLong)]
        public string Content { get; set; } = string.Empty;
    }
}
