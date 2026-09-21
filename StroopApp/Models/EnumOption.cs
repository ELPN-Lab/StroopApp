namespace StroopApp.Models
{
    public class EnumOption<T> where T : Enum
    {
        public required T Value { get; set; }
        public required string DisplayName { get; set; }
        public override string ToString() => DisplayName;
    }
}
