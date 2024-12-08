using System;
using System.IO;
using System.Text;
using System.Threading;

public static class TestLogger
{
    private static readonly object LockObject = new object();
    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out", "tests_results.md");

    public static void InitializeLog()
    {
        lock (LockObject)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);
            File.WriteAllText(LogFilePath, "# Test Results\n\n| Endpoint | Test Case | Result |\n|----------|-----------|--------|\n");
        }
    }

    public static void LogTestResult(string endpoint, string testCase, string result)
    {
        lock (LockObject)
        {
            File.AppendAllText(LogFilePath, $"| {endpoint} | {testCase} | {result} |\n");
        }
    }
}