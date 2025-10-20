namespace NegarBoard.WebApp.Models;

public record RequestInsertTableModel
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Values { get; set; } = [];
}