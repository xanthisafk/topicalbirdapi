using System; // Added 'using System;' for Guid and DateTime
using TopicalBirdAPI.Data.DTO.UsersDTO;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Data.DTO.CommentDTO
{
    /// <summary>
    /// Data Transfer Object (DTO) representing a single comment returned in an API response.
    /// </summary>
    public class CommentResponse
    {
        /// <summary>
        /// The unique identifier for the comment.
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid Id { get; set; }

        /// <summary>
        /// The text content of the comment.
        /// </summary>
        /// <example>This is a fantastic point! I completely agree.</example>
        public string Content { get; set; }

        /// <summary>
        /// The date and time when the comment was created.
        /// </summary>
        /// <example>2025-10-15T14:30:00Z</example>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The author of the comment, represented by a <see cref="UserResponse"/> DTO.
        /// </summary>
        public UserResponse Author { get; set; }

        /// <summary>
        /// Creates a <see cref="CommentResponse"/> DTO from a <see cref="Comment"/> model.
        /// </summary>
        /// <param name="cmt">The Comment model object to convert.</param>
        /// <returns>A fully populated <see cref="CommentResponse"/> instance.</returns>
        public static CommentResponse FromComment(Comment cmt)
        {
            return new CommentResponse
            {
                Id = cmt.Id,
                Content = cmt.Content,
                CreatedAt = cmt.CreatedAt,
                // Assuming UserResponse.FromUser exists and correctly maps the Author property
                Author = UserResponse.FromUser(cmt.Author)
            };
        }
    }
}