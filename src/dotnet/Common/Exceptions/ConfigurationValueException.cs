namespace FoundationaLLM.Common.Exceptions
{
    /// <summary>
    /// Represents an error with a configuration value.
    /// </summary>
    public class ConfigurationValueException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationValueException"/> class with a default message.
        /// </summary>
        public ConfigurationValueException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationValueException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        public ConfigurationValueException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationValueException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ConfigurationValueException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
