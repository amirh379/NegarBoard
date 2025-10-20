using Microsoft.AspNetCore.Mvc;
using NegarBoard.Application.Contracts;
using NegarBoard.Application.Models;

namespace NegarBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DynamicDataController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;

        public DynamicDataController(IDatabaseService databaseService) => _databaseService = databaseService;

        [HttpPost("create-table")]
        public async Task<IActionResult> CreateTable(RequestNewTableModel tableModel)
        {
            await _databaseService.CreateTableAsync(tableModel);
            return Ok();
        }

        [HttpPost("insert")]
        public async Task<IActionResult> InsertAsync(RequestInsertTableModel insertModel)
        {
            await _databaseService.InsertToTableAsync(insertModel);
            return Ok();
        }

        [HttpGet("query/{tableName}")]
        public async Task<IActionResult> GetAllFromTable(string tableName)
        {
            var result = await _databaseService.GetAllFromTableAsync(tableName);
            return Ok(result);
        }

        [HttpGet("tables")]
        public async Task<IActionResult> GetTablesName()
        {
            var result = await _databaseService.GetTablesNameAsync();
            return Ok(result);
        }

        [HttpGet("table-columns/{tableName}")]
        public async Task<IActionResult> GetTableColumns(string tableName)
        {
            var result = await _databaseService.GetTableColumnsAsync(tableName);
            return Ok(result);
        }

        [HttpPut("table-columns")]
        public async Task<IActionResult> UpdateTable(RequestUpdateTableModel updateTableModel)
        {
            await _databaseService.UpdateTableAsync(updateTableModel);
            return Accepted();
        }

        [HttpPut("{tableName}/{id}")]
        public async Task<IActionResult> UpdateData(string tableName, int id, [FromBody] Dictionary<string, object> Values)
        {
            var updateDataModel = new RequestUpdateDataModel { Id = id, TableName = tableName, Values = Values };
            await _databaseService.UpdateDataAsync(updateDataModel);
            return Accepted();
        }

        [HttpDelete("{tableName}/{id}")]
        public async Task<IActionResult> DeleteData(string tableName, int id)
        {
            await _databaseService.DeleteDataAsync(tableName, id);
            return Accepted();
        }

        [HttpDelete("{tableName}")]
        public async Task<IActionResult> DeleteTable(string tableName)
        {
            await _databaseService.DeleteTableAsync(tableName);
            return Accepted();
        }
    }
}
