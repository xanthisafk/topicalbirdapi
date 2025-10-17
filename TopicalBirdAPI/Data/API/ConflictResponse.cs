namespace TopicalBirdAPI.Data.API
{
    /// <summary>
    /// Represents an API response structure used to indicate a 409 Conflict error.
    /// It provides details about the resource or variable that caused the conflict.
    /// </summary>
    public class ConflictResponse
    {
        /// <summary>
        /// The fully qualified type name of the variable or resource that is in conflict.
        /// </summary>
        /// <example>System.String</example>
        public string Type { get; set; }

        /// <summary>
        /// The name of the variable, parameter, or property that caused the conflict.
        /// </summary>
        /// <example>Username</example>
        public string Variable { get; set; }

        /// <summary>
        /// A descriptive message detailing what may have caused the conflict.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="ConflictResponse"/> class.
        /// </summary>
        /// <typeparam name="T">The type of the variable that is in conflict.</typeparam>
        /// <param name="variable">The variable that is in conflict (used to determine its type and name).</param>
        /// <param name="message">An optional message detailing the cause of the conflict.</param>
        /// <returns>A new <see cref="ConflictResponse"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided variable is null.</exception>
        public static ConflictResponse Create<T>(T variable, string? message)
        {
            if (variable == null) throw new ArgumentNullException($"{variable} is null");
            return new ConflictResponse
            {
                Type = variable.GetType().ToString(),
                Variable = nameof(variable),
                Message = message
            };
        }
    }
}