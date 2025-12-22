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
            this.BuildEngine4.RegisterTaskObject(this.Key, this.Value, RegisteredTaskObjectLifetime.Build, false);
            return true;
        }
    }
}
