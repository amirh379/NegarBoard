namespace NegarBoard.WebApp.Models;

public record ColumnMetadata
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public bool IsNullable { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsPrimaryKey { get; set; }
    public string InputStringValue { get; set; } = string.Empty;
    public int InputNumberValue { get; set; }
}
