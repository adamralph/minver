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
            this.Value =
                (string)this.BuildEngine4.GetRegisteredTaskObject(this.Key, RegisteredTaskObjectLifetime.Build);

            return true;
        }
    }
}
