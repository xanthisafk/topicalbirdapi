namespace TopicalBirdAPI.Data.API
{
    /// <summary>
    /// Represents a successful API response, optionally containing data of a specified type.
    /// This structure is commonly used for consistent response formatting.
    /// </summary>
    /// <typeparam name="T">The type of the data/content contained in the response.</typeparam>
    public class SuccessResponse<T>
    {
        /// <summary>
        /// A descriptive message about the success of the operation.
        /// </summary>
        /// <example>Operation completed successfully.</example>
        public string? Message { get; set; }

        /// <summary>
        /// The payload or data content returned by the successful operation. This can be null.
        /// </summary>
        public T? Content { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="SuccessResponse{T}"/> class with a message and content.
        /// </summary>
        /// <param name="message">The message to include in the response.</param>
        /// <param name="Content">The content (data payload) to include in the response.</param>
        /// <returns>A new <see cref="SuccessResponse{T}"/> instance.</returns>
        public static SuccessResponse<T> Create(string? message, T? Content)
        {
            return new SuccessResponse<T> { Message = message, Content = Content };
        }
    }
}