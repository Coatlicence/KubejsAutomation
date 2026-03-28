// Tests/TestResult.cs
namespace KubeAutomation.Tests
{
    public class TestResult
    {
        public string TestName { get; set; }
        public bool Passed { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }

        public static TestResult Pass(string name, TimeSpan duration)
            => new()
            { TestName = name, Passed = true, Duration = duration };

        public static TestResult Fail(string name, string error, TimeSpan duration)
            => new()
            { TestName = name, Passed = false, ErrorMessage = error, Duration = duration };
    }
}