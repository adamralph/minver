using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace MSBuild.Caching
{
    public class CacheGet : Task
    {
        [Required]
        public string Key { get; set; }

        [Output]
        public string Value { get; set; }

        public override bool Execute()
        {
            Value = (string)BuildEngine4.GetRegisteredTaskObject(Key, RegisteredTaskObjectLifetime.Build);
            return true;
        }
    }
}
