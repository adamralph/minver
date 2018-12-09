namespace MinVer.Lib
{
    public class MajorMinor
    {
        public MajorMinor(int major, int minor)
        {
            this.Major = major;
            this.Minor = minor;
        }

        public int Major { get; }

        public int Minor { get; }

        public override string ToString() => $"{this.Major}.{this.Minor}";

        public static string ValidValues => "1.0, 1.1, 2.0, etc.";

        public static bool TryParse(string value, out MajorMinor majorMinor)
        {
            majorMinor = default;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var numbers = value.Split('.');

            var minor = 0;

            if (numbers.Length > 2 ||
                !int.TryParse(numbers[0], out var major) ||
                (numbers.Length > 1 && !int.TryParse(numbers[1], out minor)))
            {
                return false;
            }

            majorMinor = new MajorMinor(major, minor);

            return true;
        }
    }
}
