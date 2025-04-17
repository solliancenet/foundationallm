using FoundationaLLM.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FoundationaLLM.DataPipelineEngine.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs when a service error occurs in the Context API.
    /// </summary>
    public class DataPipelineServiceException : HttpStatusCodeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPipelineServiceException"/> class with a default message.
        /// </summary>
        public DataPipelineServiceException() : this(null, StatusCodes.Status500InternalServerError)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPipelineServiceException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="statusCode">The HTTP status code associated with the exception.</param>
        public DataPipelineServiceException(string? message, int statusCode = StatusCodes.Status500InternalServerError) :
            base(message, statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPipelineServiceException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="statusCode">The HTTP status code associated with the exception.</param>
        public DataPipelineServiceException(string? message, Exception? innerException, int statusCode = StatusCodes.Status500InternalServerError) :
            base(message, innerException, statusCode)
        {
        }

        /// <summary>Throws an exception if <paramref name="argument"/> is null, empty, or consists only of white-space characters.</summary>
        /// <param name="argument">The string argument to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="DataPipelineServiceException"><paramref name="argument"/> is empty or consists only of white-space characters.</exception>
        public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new DataPipelineServiceException(
                    $"The {paramName} argument cannot be null, empty, or consist only of white-space characters.",
                    StatusCodes.Status400BadRequest);
            }
        }
    }
}
