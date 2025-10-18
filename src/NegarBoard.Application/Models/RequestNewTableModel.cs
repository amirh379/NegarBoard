namespace NegarBoard.Application.Models;

public record RequestNewTableModel
{
    public string Name { get; set; }
    public List<ColumnDefinition> Columns { get; set; } = new();
}