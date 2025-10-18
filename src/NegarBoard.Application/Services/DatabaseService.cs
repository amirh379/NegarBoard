using NegarBoard.Application.Contracts;
using System.Data;
using Dapper;
using NegarBoard.Application.Models;
using NegarBoard.Application.Models.Enums;
using System.Diagnostics;

namespace NegarBoard.Application.Services;

public class DatabaseService : IDatabaseService
{
    private readonly IDbConnection _db;

    public DatabaseService(IDbConnection db) => _db = db;

    public Task CreateDatabaseAsync()
    {
        throw new NotImplementedException();
    }
    public async Task CreateTableAsync(RequestNewTableModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            throw new ArgumentException("Table name cannot be empty.");

        if (model.Columns == null || !model.Columns.Any())
            throw new ArgumentException("At least one column must be defined.");

        var columnDefs = new List<string>();

        // Always add Id column as primary key
        columnDefs.Add("Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY");

        foreach (var col in model.Columns)
        {
            string sqlType = col.Type switch
            {
                ColumnType.Int => "INT",
                ColumnType.String => $"NVARCHAR({(col.MaxLength > 0 ? col.MaxLength : 255)})",
                ColumnType.Bool => "BIT",
                ColumnType.Double => "FLOAT",
                ColumnType.DateTime => "DATETIME2",
                _ => throw new NotSupportedException($"Unsupported type: {col.Type}")
            };

            string nullability = col.AllowNull ? "NULL" : "NOT NULL";
            columnDefs.Add($"{col.Name} {sqlType} {nullability}");
        }

        var sql = $@"
        IF OBJECT_ID('{model.Name}', 'U') IS NULL
        BEGIN
            CREATE TABLE {model.Name} (
                {string.Join(", ", columnDefs)}
            );
        END";

        Debug.WriteLine(sql);
        await _db.ExecuteAsync(sql);
    }   
    public async Task InsertToTableAsync(RequestInsertTableModel insertModel)
    {
        await ValidateColumnsAsync(insertModel.Name, insertModel.Values.Keys);

        // Extract column names
        var columns = string.Join(", ", insertModel.Values.Keys);

        // Create parameter names like @p0, @p1, ...
        var paramNames = insertModel.Values.Keys
            .Select((k, i) => $"@p{i}")
            .ToList();

        var valuesClause = string.Join(", ", paramNames);

        // Build SQL
        var sql = $"INSERT INTO {insertModel.Name} ({columns}) VALUES ({valuesClause});";

        // Build parameters object        
        var parameters = new DynamicParameters();
        int index = 0;
        foreach (var value in insertModel.Values.Values)
        {
            parameters.Add($"p{index}", value);
            index++;
        }

        Debug.WriteLine(sql);
        await _db.ExecuteAsync(sql, parameters);
    }
    public async Task ValidateColumnsAsync(string tableName, IEnumerable<string> keys)
    {
        var sql = @"SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @TableName";

        var validColumns = (await _db.QueryAsync<string>(sql, new { TableName = tableName }))
                           .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Check if all keys exist in validColumns
        //return keys.All(k => validColumns.Contains(k));

        var invalidKeys = keys
                .Where(k => !validColumns.Contains(k))
                .ToList();

        if (invalidKeys.Any())
            throw new ArgumentException($"Invalid columns: {string.Join(", ", invalidKeys)}");
    }    
    public async Task<IEnumerable<IDictionary<string, object>>> GetAllFromTableAsync(string tableName)
    {
        // Validate table exists
        var sqlCheck = @"SELECT 1 
                     FROM INFORMATION_SCHEMA.TABLES 
                     WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @TableName";
        var exists = await _db.ExecuteScalarAsync<int?>(sqlCheck, new { TableName = tableName });
        if (exists == null)
            throw new ArgumentException($"Table '{tableName}' does not exist.");

        // Query all rows
        var sql = $"SELECT * FROM {tableName}";
        var rows = await _db.QueryAsync(sql);

        // Each row is a Dapper Row (dynamic object)
        // Convert to dictionary for flexibility
        return rows.Select(r => (IDictionary<string, object>)r);
    }
    public async Task<IEnumerable<IDictionary<string, object>>> GetFromTableAsync(string tableName, IEnumerable<string> columns)
    {
        // Validate columns
        var validColumns = (await _db.QueryAsync<string>(
            @"SELECT COLUMN_NAME 
          FROM INFORMATION_SCHEMA.COLUMNS 
          WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @TableName",
            new { TableName = tableName }))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var invalid = columns.Where(c => !validColumns.Contains(c)).ToList();
        if (invalid.Any())
            throw new ArgumentException($"Invalid columns: {string.Join(", ", invalid)}");

        var columnList = string.Join(", ", columns);
        var sql = $"SELECT {columnList} FROM {tableName}";

        var rows = await _db.QueryAsync(sql);
        return rows.Select(r => (IDictionary<string, object>)r);
    }
    public async Task<IEnumerable<IDictionary<string, object>>> GetFromTableWithFilterAsync(string tableName, Dictionary<string, object> filters)
    {
        // Validate columns
        var validColumns = (await _db.QueryAsync<string>(
            @"SELECT COLUMN_NAME 
          FROM INFORMATION_SCHEMA.COLUMNS 
          WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @TableName",
            new { TableName = tableName }))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var invalid = filters.Keys.Where(c => !validColumns.Contains(c)).ToList();
        if (invalid.Any())
            throw new ArgumentException($"Invalid filter columns: {string.Join(", ", invalid)}");

        // Build WHERE clause
        var conditions = filters.Keys.Select((k, i) => $"{k} = @p{i}");
        var whereClause = string.Join(" AND ", conditions);

        var sql = $"SELECT * FROM {tableName} WHERE {whereClause}";

        var parameters = new DynamicParameters();
        int index = 0;
        foreach (var kv in filters)
            parameters.Add($"p{index++}", kv.Value);

        var rows = await _db.QueryAsync(sql, parameters);
        return rows.Select(r => (IDictionary<string, object>)r);
    }
    public async Task<List<string>> GetTablesNameAsync()
    {
        var sql = @"
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_NAME;";

        var tables = await _db.QueryAsync<string>(sql);
        return tables.ToList();
    }
    public async Task<List<ColumnMetadata>> GetTableColumnsAsync(string tableName)
    {
        var sql = @"
            SELECT 
                c.COLUMN_NAME AS Name,
                c.DATA_TYPE AS DataType,
                c.CHARACTER_MAXIMUM_LENGTH AS MaxLength,
                CASE WHEN c.IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS IsNullable,
                COLUMNPROPERTY(object_id(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IsIdentity,
                CASE WHEN k.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE k
                ON c.TABLE_NAME = k.TABLE_NAME
                AND c.COLUMN_NAME = k.COLUMN_NAME
                AND k.CONSTRAINT_NAME IN (
                    SELECT CONSTRAINT_NAME 
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                    WHERE CONSTRAINT_TYPE = 'PRIMARY KEY' AND TABLE_NAME = @TableName
                )
            WHERE c.TABLE_NAME = @TableName
            ORDER BY c.ORDINAL_POSITION;";

        var columns = await _db.QueryAsync<ColumnMetadata>(sql, new { TableName = tableName });
        return columns.ToList();
    }
    public async Task UpdateTableAsync(RequestUpdateTableModel model)
    {
        if (string.IsNullOrWhiteSpace(model.TableName))
            throw new ArgumentException("Table name cannot be empty.");

        var alterStatements = new List<string>();

        // Add new columns
        foreach (var col in model.AddColumns)
        {
            string sqlType = col.Type switch
            {
                ColumnType.Int => "INT",
                ColumnType.String => $"NVARCHAR({(col.MaxLength > 0 ? col.MaxLength : 255)})",
                ColumnType.Bool => "BIT",
                ColumnType.Double => "FLOAT",
                ColumnType.DateTime => "DATETIME2",
                _ => throw new NotSupportedException($"Unsupported type: {col.Type}")
            };

            string nullability = col.AllowNull ? "NULL" : "NOT NULL";
            alterStatements.Add($"ALTER TABLE {model.TableName} ADD {col.Name} {sqlType} {nullability};");
        }

        // Drop columns
        foreach (var colName in model.DropColumns)
        {
            alterStatements.Add($"ALTER TABLE {model.TableName} DROP COLUMN {colName};");
        }

        // Execute all statements
        foreach (var sql in alterStatements)
        {
            Debug.WriteLine(sql);
            await _db.ExecuteAsync(sql);
        }
    }
    public async Task UpdateDataAsync(RequestUpdateDataModel model)
    {
        if (string.IsNullOrWhiteSpace(model.TableName))
            throw new ArgumentException("Table name cannot be empty.");
        if (!model.Values.Any())
            throw new ArgumentException("No values provided for update.");

        // Build SET clause
        var setClauses = model.Values.Keys.Select((col, i) => $"{col} = @val{i}");
        var setClause = string.Join(", ", setClauses);

        var sql = $"UPDATE {model.TableName} SET {setClause} WHERE Id = @Id;";

        var parameters = new DynamicParameters();
        int index = 0;
        foreach (var val in model.Values.Values)
            parameters.Add($"val{index++}", val);

        parameters.Add("Id", model.Id);

        Debug.WriteLine(sql);
        await _db.ExecuteAsync(sql, parameters);
    }
    public async Task DeleteDataAsync(string tableName, int rowId)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be empty.");

        var sql = $"DELETE FROM {tableName} WHERE Id = @Id;";
        
        Debug.WriteLine(sql);
        await _db.ExecuteAsync(sql, new { rowId });
    }
    public async Task DeleteTableAsync(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be empty.");

        var sql = $@"
        IF OBJECT_ID(@TableName, 'U') IS NOT NULL
        BEGIN
            DROP TABLE [{tableName}];
        END";

        Debug.WriteLine(sql);
        await _db.ExecuteAsync(sql, new { TableName = tableName });
    }
}