namespace MinVer.Tasks
{
    using Microsoft.Build.Utilities;

    public class GetVersion : Task
    {
        public string BuildMetadata { get; set; }

        public string Path { get; set; }

        public string MajorMinor { get; set; }

        public string TagPrefix { get; set; }

        public bool Verbose { get; set; }

        public string Version { get; set; }

        public override bool Execute()
        {
            var major = 0;
            var minor = 0;

            var majorMinorValue = MajorMinor;

            if (!string.IsNullOrEmpty(majorMinorValue))
            {
                var numbers = majorMinorValue.Split('.');

                if (numbers.Length > 2)
                {
                    Log.LogError("Bad property value", $"More than one dot in MAJOR.MINOR range '{majorMinorValue}'.", "MINVER0004");
                    return false;
                }

                if (!int.TryParse(numbers[0], out major))
                {
                    Log.LogError("Bad property value", $"Invalid MAJOR '{numbers[0]}' in MAJOR.MINOR range '{majorMinorValue}'.", "MINVER0005");
                    return false;
                }

                if (numbers.Length > 1 && !int.TryParse(numbers[1], out minor))
                {
                    Log.LogError("Bad property value", $"Invalid MINOR '{numbers[1]}' in MAJOR.MINOR range '{majorMinorValue}'.", "MINVER0006");
                    return false;
                }
            }

            Version = Versioner.GetVersion(this.Path ?? ".", Verbose, TagPrefix, major, minor, BuildMetadata).ToString();

            return !Log.HasLoggedErrors;
        }
    }
}
