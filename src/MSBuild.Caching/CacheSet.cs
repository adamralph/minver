using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace MSBuild.Caching
{
    public class CacheSet : Task
    {
        [Required]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }

        public override bool Execute()
        {
            BuildEngine4.RegisterTaskObject(Key, Value, RegisteredTaskObjectLifetime.Build, false);
            return true;
        }
    }
}
