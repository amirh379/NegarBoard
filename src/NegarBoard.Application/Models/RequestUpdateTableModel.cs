namespace NegarBoard.Application.Models;

public record RequestUpdateTableModel
{
    public string TableName { get; set; }
    public List<ColumnDefinition> AddColumns { get; set; } = new();
    public List<string> DropColumns { get; set; } = new();
}
