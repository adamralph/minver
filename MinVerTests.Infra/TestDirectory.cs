using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MinVerTests.Infra
{
    public static class TestDirectory
    {
        public static string Get(string testSuiteName, string testName, object? tag = null) =>
            Path.Combine(
                Path.GetTempPath(),
                testSuiteName,
                TestContext.RunId.ToString(CultureInfo.InvariantCulture),
                $"{testName}{(tag == null ? "" : (tag.GetType().Name.StartsWith("ValueTuple", StringComparison.Ordinal) ? tag : $"({tag})"))}");

        public static string GetTestDirectory(this MethodBase? testMethod, object? tag = null, [CallerMemberName] string testMethodName = "")
        {
            testMethod = testMethod ?? throw new ArgumentNullException(nameof(testMethod));
            var declaringType = testMethod.DeclaringType ?? throw new ArgumentException("The declaring type of the test method is null.", nameof(testMethod));
            var testSuiteName = declaringType.Assembly.GetName().Name ?? throw new ArgumentException("The name of the assembly containing the declaring type of the test method is null.", nameof(testMethod));

            return Get(testSuiteName, $"{testMethod.DeclaringType.GetReflectedType().Name}.{testMethodName}", tag);
        }

        private static Type GetReflectedType(this Type type) => type.ReflectedType?.GetReflectedType() ?? type;
    }
}
