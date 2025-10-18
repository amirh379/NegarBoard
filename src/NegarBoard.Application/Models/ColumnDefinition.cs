using NegarBoard.Application.Models.Enums;

namespace NegarBoard.Application.Models;

public record ColumnDefinition
{
    public string Name { get; set; }
    public ColumnType Type { get; set; }
    public int MaxLength { get; set; }
    public bool AllowNull { get; set; } = true;
}