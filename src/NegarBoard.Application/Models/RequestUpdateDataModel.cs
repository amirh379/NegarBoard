namespace NegarBoard.Application.Models;

public record RequestUpdateDataModel
{
    public string TableName { get; set; }
    public int Id { get; set; }
    public Dictionary<string, object> Values { get; set; } = new();
}
