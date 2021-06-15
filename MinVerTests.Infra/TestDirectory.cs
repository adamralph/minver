using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MinVerTests.Infra
{
    public static class TestDirectory
    {
        public static string Get(string testSuiteName, string testName, object tag = null) =>
            Path.Combine(
                Path.GetTempPath(),
                testSuiteName,
                TestContext.RunId.ToString(CultureInfo.InvariantCulture),
                $"{testName}{(tag == null ? "" : (tag.GetType().Name.StartsWith("ValueTuple", StringComparison.Ordinal) ? tag : $"({tag})"))}");

        public static string GetTestDirectory(this MethodBase testMethod, object tag = null, [CallerMemberName] string testMethodName = "") =>
            Get(testMethod?.DeclaringType?.Assembly.GetName().Name, $"{testMethod.DeclaringType.GetReflectedType().Name}.{testMethodName}", tag);

        private static Type GetReflectedType(this Type type) => type.ReflectedType?.GetReflectedType() ?? type;
    }
}
