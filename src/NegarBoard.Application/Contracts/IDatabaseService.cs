using NegarBoard.Application.Models;

namespace NegarBoard.Application.Contracts;

public interface IDatabaseService
{
    Task CreateDatabaseAsync();
    Task CreateTableAsync(RequestNewTableModel model);
    Task InsertToTableAsync(RequestInsertTableModel insertModel);
    Task<IEnumerable<IDictionary<string, object>>> GetAllFromTableAsync(string tableName);
    Task<IEnumerable<IDictionary<string, object>>> GetFromTableAsync(string tableName, IEnumerable<string> columns);
    Task<List<string>> GetTablesNameAsync();
    Task<List<ColumnMetadata>> GetTableColumnsAsync(string tableName);
    Task UpdateTableAsync(RequestUpdateTableModel model);
    Task UpdateDataAsync(RequestUpdateDataModel model);
    Task DeleteDataAsync(string tableName, int rowId);
    Task DeleteTableAsync(string tableName);
}
