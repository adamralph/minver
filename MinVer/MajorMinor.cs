namespace MinVer
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

        public static bool TryParse(string text, out MajorMinor range)
        {
            range = default;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var numbers = text.Split('.');

            var minor = 0;

            if (numbers.Length > 2 ||
                !int.TryParse(numbers[0], out var major) ||
                (numbers.Length > 1 && !int.TryParse(numbers[1], out minor)))
            {
                return false;
            }

            range = new MajorMinor(major, minor);

            return true;
        }
    }
}
