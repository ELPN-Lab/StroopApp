namespace StroopApp.Services.Summary
{
    public enum BlockSummaryCellType
    {
        Text,
        BooleanGlyph
    }
    public class BlockSummaryColumn
    {
        public string Header { get; }
        public string BindingPath { get; }
        public string? StringFormat { get; set; }
        public BlockSummaryCellType Type { get; set; }
        public BlockSummaryColumn(string header, string bindingPath)
        {
            Header = header;
            BindingPath = bindingPath;
        }
    }
}
