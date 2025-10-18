using System.ComponentModel.DataAnnotations;

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
        [Required, MinLength(1), MaxLength(10000)]
        public string Content { get; set; }
    }
}
