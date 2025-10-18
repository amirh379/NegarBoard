namespace NegarBoard.Application.Models;

public record ColumnMetadata
{
    public string Name { get; set; }
    public string DataType { get; set; }
    public int? MaxLength { get; set; }
    public bool IsNullable { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsPrimaryKey { get; set; }
}
