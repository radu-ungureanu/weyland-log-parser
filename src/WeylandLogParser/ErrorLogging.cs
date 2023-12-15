
namespace WeylandLogParser
{
    public class ErrorLogging(string[] lines)
    {
        public void Log()
        {
            var errors = ParseErrors(lines);
            var groupedErrors = errors
                .GroupBy(error => error.Code)
                .Select(group => new ErrorInfo
                {
                    Code = group.Key,
                    Reason = group.First().Reason,
                    Count = group.Count()
                })
                .ToList();

            foreach (var entry in groupedErrors)
            {
                Console.WriteLine($"Found error code \"{entry.Code}\": {entry.Reason} - {entry.Count} times");
            }

            Console.WriteLine();
            Console.WriteLine($"Found a total of {groupedErrors.Sum(err => err.Count)} errors");

            var warnings = ParseWarnings(lines);
            Console.WriteLine($"Found a total of {warnings.Length} warnings during minification");
        }

        private string[] ParseWarnings(string[] lines)
        {
            return lines
                .Where(line => line.StartsWith("WARN: "))
                .ToArray();
        }

        private IEnumerable<Error> ParseErrors(string[] lines)
        {
            var code = string.Empty;
            var rawReason = string.Empty;
            var reasonIsOnNextLine = false;

            foreach (var line in lines)
            {
                if (reasonIsOnNextLine)
                {
                    rawReason = GetPropertyValue(line);
                    reasonIsOnNextLine = false;
                }
                if (line.StartsWith("ERR! jshint   raw:"))
                {
                    reasonIsOnNextLine = !line.Contains('\'');
                    if (!reasonIsOnNextLine)
                    {
                        rawReason = GetPropertyValue(line);
                    }
                }

                if (line.StartsWith("ERR! jshint   code: "))
                {
                    code = GetPropertyValue(line);
                }

                if (line.StartsWith("ERR! jshint   ") && line.EndsWith(" }"))
                {
                    var error = new Error
                    {
                        Code = code,
                        Reason = rawReason
                    };

                    code = string.Empty;
                    rawReason = string.Empty;

                    yield return error;
                }
            }
        }

        private string GetPropertyValue(string line)
        {
            var start = line.IndexOf('\'') + 1;
            var end = line.LastIndexOf('\'');

            return line[start..end];
        }

        private class Error
        {
            public string Code { get; init; } = default!;
            public string Reason { get; init; } = default!;
        }

        private class ErrorInfo : Error
        {
            public int Count { get; init; }
        }
    }
}
