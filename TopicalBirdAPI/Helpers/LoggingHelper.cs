using Microsoft.AspNetCore.Identity;
using System.Text;

namespace TopicalBirdAPI.Helpers
{
    public class LoggingHelper
    {
        private readonly ILogger<LoggingHelper> _logger;
        private readonly string _logPath = "logs";
        private readonly string _basePath;

        public LoggingHelper(ILogger<LoggingHelper> logger, IHostEnvironment env)
        {
            _logger = logger;
            _basePath = env.ContentRootPath;
        }

        protected string CreateRefCode(Guid traceId, string level)
        {
            long ticks = DateTime.Now.Ticks / 2;
            string tracePrefix = traceId.ToString("N").Substring(0, 6).Replace("-", "").ToUpperInvariant();
            return $"{level}-{ticks}-{tracePrefix}";
        }

        protected async Task<bool> WriteToFile(Guid traceId, string refId, string content)
        {
            try
            {
                string fullLogDirectoryPath = Path.Combine(_basePath, _logPath);
                Directory.CreateDirectory(fullLogDirectoryPath);

                string filename = $"{refId}_{traceId:N}.log";
                string fullPath = Path.Combine(fullLogDirectoryPath, filename);

                if (File.Exists(fullPath))
                {
                    _logger.LogWarning("Log file already exists: {FullPath}", fullPath);
                    return false;
                }

                await File.WriteAllTextAsync(fullPath, content);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to write log file for Ref: {RefId}\n{@content}", refId, content);
                return false;
            }
            return true;
        }

        public bool Info(string message)
        {
            _logger.LogInformation("[INFO] | {@Date} | {@Message}", DateTime.Now, message);
            return true;
        }


        public string Warn(string message)
        {
            Guid traceCode = Guid.NewGuid();
            string refCode = CreateRefCode(traceCode, "WARN");
            _logger.LogWarning("[WARN] | {@Date} | {@Ref} | {@Message}", DateTime.Now, refCode, message);
            return refCode;
        }

        public async Task<string> Error(string message, Exception? exception)
        {
            string logMessage = exception != null ? exception.Message : message;

            Guid traceCode = Guid.NewGuid();
            string refCode = CreateRefCode(traceCode, "ERRO");

            _logger.LogError(exception, "[ERRO] | {Date:o} | {Ref} | {Message}", DateTime.Now, refCode, logMessage);

            var str = new StringBuilder();

            str.AppendLine("---- DETAILED ERROR LOG ----");
            str.AppendLine($"TraceID: {traceCode:D}");
            str.AppendLine($"RefID: {refCode}");
            str.AppendLine($"Date: {DateTime.Now:o}");
            str.AppendLine($"User Message: {message}");
            str.AppendLine("----------------------------");

            if (exception != null)
            {
                str.AppendLine("--- EXCEPTION DETAILS ---");
                str.AppendLine($"Type: {exception.GetType().FullName}");
                str.AppendLine($"Message: {exception.Message}");
                str.AppendLine($"Source: {exception.Source ?? "N/A"}");
                str.AppendLine($"HelpLink: {exception.HelpLink ?? "N/A"}");

                // Stack Trace
                str.AppendLine("-- STACK TRACE --");
                str.AppendLine(exception.StackTrace ?? "No stack trace available.");

                // Inner Exception
                AppendInnerExceptionDetails(str, exception.InnerException);
            }
            else
            {
                str.AppendLine("No exception object provided. Logging only the user message.");
            }

            await WriteToFile(traceCode, refCode, str.ToString());
            return refCode;
        }

        public async Task<string> IdError(string message, IEnumerable<IdentityError> errors)
        {
            var errorDetails = string.Join(
                Environment.NewLine,
                errors.Select(e => $"- Code: {e.Code}, Description: {e.Description}")
            );

            string fullLogMessage = $"{message}{Environment.NewLine}--- Identity Errors ---{Environment.NewLine}{errorDetails}";

            return await Error(fullLogMessage, null);
        }

        public async Task<string> Crit(string message, Exception ex)
        {
            Guid traceCode = Guid.NewGuid();
            string refCode = CreateRefCode(traceCode, "CRIT");

            _logger.LogCritical(ex, "[CRIT] | {Date:o} | {Ref} | {Message}", DateTime.Now, refCode, message);

            var str = new StringBuilder();

            str.AppendLine("---- CRITICAL ERROR LOG ----");
            str.AppendLine($"TraceID: {traceCode:D}");
            str.AppendLine($"RefID: {refCode}");
            str.AppendLine($"Date: {DateTime.Now:o}");
            str.AppendLine($"User Message: {message}");
            str.AppendLine("----------------------------");

            str.AppendLine("--- EXCEPTION DETAILS ---");
            str.AppendLine($"Type: {ex.GetType().FullName}");
            str.AppendLine($"Message: {ex.Message}");
            str.AppendLine($"Source: {ex.Source ?? "N/A"}");
            str.AppendLine($"HelpLink: {ex.HelpLink ?? "N/A"}");

            str.AppendLine("-- STACK TRACE --");
            str.AppendLine(ex.StackTrace ?? "No stack trace available.");

            AppendInnerExceptionDetails(str, ex.InnerException);

            await WriteToFile(traceCode, refCode, str.ToString());
            return refCode;
        }

        protected void AppendInnerExceptionDetails(StringBuilder sb, Exception? inner, int depth = 0)
        {
            Exception? current = inner;
            while (current != null)
            {
                sb.AppendLine($"-- INNER EXCEPTION (Depth {depth}) --");
                sb.AppendLine($"Inner Type: {current.GetType().FullName}");
                sb.AppendLine($"Inner Message: {current.Message}");
                sb.AppendLine($"Inner Source: {current.Source ?? "N/A"}");
                sb.AppendLine($"Inner Stack Trace:\n{current.StackTrace ?? "N/A"}");

                current = current.InnerException;
                depth++;
                if (depth >= 10)
                {
                    break;
                }
            }
        }
    }
}
