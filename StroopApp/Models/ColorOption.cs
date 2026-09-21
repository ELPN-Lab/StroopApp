namespace StroopApp.Models
{
    public class ColorOption
    {
        public required string Hex { get; set; }
        public required string DisplayName { get; set; }
        public override string ToString() => DisplayName;
    }
}
