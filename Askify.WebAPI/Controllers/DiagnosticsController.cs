using Askify.DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Askify.DataAccessLayer.Data;
using System.Threading.Tasks;
using Askify.DataAccessLayer;
using System.Collections.Generic;

namespace Askify.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public DiagnosticsController(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        [HttpGet("table-status")]
        public async Task<IActionResult> GetTableStatus()
        {
            // Fix the async counting issue
            var consultations = await _unitOfWork.Consultations.GetAllAsync();
            var consultationsCount = consultations.Count();
            
            return Ok(new {
                consultations = new {
                    count = consultationsCount,
                    isEmpty = consultationsCount == 0
                }
            });
        }

        // Fix async method without await
        [HttpGet("database-info")]
        public IActionResult GetDatabaseInfo() // Remove async if not using await
        {
            var connection = _context.Database.GetDbConnection();
            
            return Ok(new
            {
                ConnectionString = connection.ConnectionString
                    .Replace(connection.Database, "[REDACTED]")
                    .Replace("Password=", "Password=[REDACTED];"),
                DatabaseName = connection.Database,
                State = connection.State.ToString()
            });
        }

        [HttpGet("schema-check")]
        public async Task<IActionResult> CheckSchema()
        {
            try
            {
                // Force EF Core to refresh its model cache
                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                
                // Check if the columns actually exist in the database
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        TABLE_NAME, 
                        COLUMN_NAME 
                    FROM 
                        INFORMATION_SCHEMA.COLUMNS 
                    WHERE 
                        TABLE_NAME = 'Consultations' AND 
                        (COLUMN_NAME = 'Title' OR COLUMN_NAME = 'Description')";
                
                var columns = new List<string>();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        columns.Add($"{reader.GetString(0)}.{reader.GetString(1)}");
                    }
                }
                
                // Check if we're using the right database
                command = connection.CreateCommand();
                command.CommandText = "SELECT DB_NAME()";
                var currentDatabase = (string?)await command.ExecuteScalarAsync() ?? string.Empty;
                
                await connection.CloseAsync();
                
                return Ok(new
                {
                    DatabaseName = currentDatabase,
                    ExpectedColumns = new[] { "Title", "Description" },
                    FoundColumns = columns,
                    ConnectionString = connection.ConnectionString
                        .Replace(connection.Database, "[REDACTED]")
                        .Replace("Password=", "Password=[REDACTED];")
                        .Replace("User ID=", "User ID=[REDACTED];"),
                    Message = columns.Count == 2 
                        ? "All expected columns found. This might be an EF Core caching issue." 
                        : "Missing columns in the database. Run the SQL to add them."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        [HttpPost("add-columns")]
        public async Task<IActionResult> AddColumns()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                var command = connection.CreateCommand();
                command.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_NAME = 'Consultations' AND COLUMN_NAME = 'Title')
                    BEGIN
                        ALTER TABLE Consultations ADD Title nvarchar(max) NOT NULL DEFAULT 'Untitled Consultation'
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_NAME = 'Consultations' AND COLUMN_NAME = 'Description')
                    BEGIN
                        ALTER TABLE Consultations ADD Description nvarchar(max) NOT NULL DEFAULT 'No description provided.'
                    END";
                
                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
                
                // Force EF Core to refresh its model cache
                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                
                return Ok(new { Message = "Columns added successfully. App restart may be required." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }
    }
}
