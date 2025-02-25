using System.Text;
using ClickSphere_API.Models;
using System.Data.Odbc;
using System.Net;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Globalization;
using NodaTime;
using NodaTime.Text;
namespace ClickSphere_API.Services;

/// <summary>
/// Service class for handling view operations.
/// </summary>
/// <param name="dbService">Database service</param>
/// <param name="configuration">Configuration settings</param>
public class ApiViewServices(IDbService dbService, IConfiguration configuration) : IApiViewService
{
    private readonly IDbService _dbService = dbService;
    private readonly string ODBC_DSN = configuration["ODBC:DSN"] ?? "";
    private readonly string ODBC_User = configuration["ODBC:User"] ?? "";
    private readonly string ODBC_Password = configuration["ODBC:Password"] ?? "";
    private readonly string ODBC_Database = configuration["ODBC:Database"] ?? "";

    /// <summary>
    /// Create a new view in the specified database
    /// </summary>
    /// <param name="view">The view to create</param>
    /// <returns>The result of the view creation</returns>
    public async Task<HttpResponseMessage> CreateView(View view)
    {
        string query = $"CREATE VIEW {view.Database}.{view.Id} AS {view.Query};";
        int result;
        try
        {
            result = await _dbService.ExecuteNonQuery(query);
        }
        catch (Exception e)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = e.Message };
        }

        // insert view into CV_Views table
        if (result == 0)
        {
            string insertQuery = $"INSERT INTO ClickSphere.Views (Id, Name, Description, Type) VALUES ('{view.Id}','{view.Name}','{view.Description}','V');";
            int insertResult = await _dbService.ExecuteNonQuery(insertQuery);
            if (insertResult < 0)
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "Could not insert view into ClickSphere.Views table" };
            else
                return new HttpResponseMessage(HttpStatusCode.OK);
        }
        else
            return new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "Could not create view" };
    }

    /// <summary>
    /// Create materialized view in the specified database
    /// </summary>
    /// <param name="view">The materialized view to create</param>
    /// <returns>The result of the materialized view creation</returns>
    public async Task<IResult> CreateMaterializedView(MaterializedView view)
    {
        // create materialized view
        string query = $"CREATE MATERIALIZED VIEW {view.Database}.{view.Id} " +
                       $"ENGINE = {view.Engine} ";

        if (view.PartitionBy != null)
            query += $"PARTITION BY {view.PartitionBy} ";

        if (view.OrderBy != null)
            query += $"ORDER BY {view.OrderBy} ";

        if (view.Populate == true)
            query += "POPULATE ";

        query += $"AS {view.Query};";

        int result = await _dbService.ExecuteNonQuery(query);

        // insert view into ClickSphere.Views metadata table
        if (result == 0)
        {
            string insertQuery = $"INSERT INTO ClickSphere.Views (Id, Name, Description, Type) VALUES ('{view.Id}','{view.Name}','{view.Description}','M');";
            int insertResult = await _dbService.ExecuteNonQuery(insertQuery);
            if (insertResult < 0)
                return Results.BadRequest("Could not insert view into ClickSphere.Views table");
            else
                return Results.Ok();
        }
        else
            return Results.BadRequest("Could not create materialized view");
    }

    /// <summary>
    /// Delete a view from a database
    /// </summary>
    /// <param name="database">The database where the view is located</param>
    /// <param name="view">The view to delete</param>
    /// <returns>The result of the view deletion</returns>
    public async Task<IResult> DeleteView(string database, string view)
    {
        int result = await _dbService.ExecuteNonQuery($"DROP VIEW {database}.{view}");
        if (result == 0)
        {
            result = await _dbService.ExecuteNonQuery($"DELETE FROM ClickSphere.Views WHERE Id = '{view}'");
            if (result == 0)
                return Results.Ok();
            else
                return Results.BadRequest("Could not delete view from ClickSphere.Views table");
        }
        else
            return Results.BadRequest("Could not drop view");
    }

    /// <summary>
    /// Get the views of a database which are registered in ClickSphere
    /// </summary>
    /// <param name="database">The database to get the views from</param>
    /// <returns>A list of views of the database</returns>
    public async Task<List<View>> GetAllViews(string database)
    {
        return await _dbService.ExecuteQueryList<View>("SELECT c.*, s.database as Database, s.as_select as Query " +
                                                       "FROM system.tables s JOIN ClickSphere.Views c ON c.Id = s.name " +
                                                       $"WHERE s.database = '{database}' and (s.engine = 'View' or s.engine = 'Table') " +
                                                       "ORDER BY c.Name");
    }

    /// <summary>
    /// Get configuration of specific view from database
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the configuration from</param>
    /// <returns>The configuration of the view</returns>
    public async Task<View> GetViewConfig(string database, string viewId)
    {
        return await _dbService.ExecuteQueryObject<View>("SELECT c.Id, c.Name, c.Description, s.database as Database, s.as_select as Query, c.Questions " +
                                                         "FROM system.tables s JOIN ClickSphere.Views c ON c.Id = s.name " +
                                                         $"WHERE s.database = '{database}' and s.engine = 'View' and c.Id = '{viewId}'");
    }

    /// <summary>
    /// Update a view in the specified database
    /// </summary>
    /// <param name="view">The view to update</param>
    /// <returns>The result of the view update</returns>
    public async Task<IResult> UpdateView(View view)
    {
        string query = $"CREATE OR REPLACE VIEW {view.Database}.{view.Id} AS {view.Query};";
        int result = await _dbService.ExecuteNonQuery(query);

        // update view in CV_Views table
        if (result == 0)
        {
            // escape single quotes
            view.Description = view.Description!.Replace("'", "''");
            view.Questions = view.Questions!.Replace("'", "''");

            string updateQuery = $"ALTER TABLE ClickSphere.Views " +
                                 $"UPDATE Name = '{view.Name}', " +
                                 $"Description = '{view.Description}', " +
                                 $"Type = '{view.Type}', " +
                                 $"Questions = '{view.Questions}' " +
                                 $"WHERE Id = '{view.Id}';";
            int updateResult = await _dbService.ExecuteNonQuery(updateQuery);
            if (updateResult < 0)
                return Results.BadRequest("Could not update view in ClickSphere.Views table");
            else
                return Results.Ok();
        }
        else
            return Results.BadRequest("Could not update view");
    }

    /// <summary>
    /// Get view columns and type for QBE search
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the columns from</param>
    /// <param name="forceUpdate">Force update of the view columns</param>
    /// <returns>The columns of the view</returns>
    public async Task<IList<ViewColumns>> GetViewColumns(string database, string viewId, bool forceUpdate = false)
    {
        List<ViewColumns> columns;

        if (forceUpdate)
        {
            // delete data from ViewColumns table
            await _dbService.ExecuteNonQuery($"DELETE FROM ClickSphere.ViewColumns WHERE Database = '{database}' AND ViewId = '{viewId}'");

            // update data from ViewColumns table
            bool success = await UpdateViewColumns(database, viewId);

            if (!success)
                return [];

            // load data from ViewColumns table
            return await LoadViewColumns(database, viewId);
        }
        else
        {
            // try to get data from ViewColumns table
            columns = await LoadViewColumns(database, viewId);

            if (columns.Count == 0)
            {
                // regenerate columns if not found
                bool success = await UpdateViewColumns(database, viewId);

                if (!success)
                    return [];

                // load data from ViewColumns table again
                columns = await LoadViewColumns(database, viewId);
            }
            return columns;
        }
    }

    private async Task<List<ViewColumns>> LoadViewColumns(string database, string viewId)
    {
        return await _dbService.ExecuteQueryList<ViewColumns>($"SELECT * FROM ClickSphere.ViewColumns WHERE Database = '{database}' AND ViewId = '{viewId}' order by Sorter");
    }

    private async Task<bool> UpdateViewColumns(string database, string viewId)
    {
        // insert data into viewColumns table
        var viewCols = await _dbService.ExecuteQueryDictionary($"SELECT name as `Column Name`, type as `Data Type` FROM system.columns WHERE table = '{viewId}' and database = '{database}'");

        if (viewCols == null)
            return false;

        int sorter = 0;
        foreach (var col in viewCols)
        {
            string? controlType = col["Data Type"] switch
            {
                "String" or "Nullable(String)" => "TextBox",
                "UInt8" or "UInt16" or "UInt32" or "UInt64" or "Int8" or "Int16" or "Int32" or "Int64" => "Numeric",
                "Nullable(UInt8)" or "Nullable(UInt16)" or "Nullable(UInt32)" or "Nullable(UInt64)" or "Nullable(Int8)" or "Nullable(Int16)" or "Nullable(Int32)" or "Nullable(Int64)" => "Numeric",
                "Float32" or "Float64" => "Numeric",
                "Nullable(Float32)" or "Nullable(Float64)" => "Numeric",
                "DateTime" or "DateTime64(3)" => "DateTime",
                "Nullable(DateTime)" or "Nullable(DateTime64(3))" => "DateTime",
                _ => "TextBox",
            };

            string? columnWidth = col["Data Type"] switch
            {
                "String" or "Nullable(String)" => "200",
                "UInt8" or "UInt16" or "UInt32" or "UInt64" or "Int8" or "Int16" or "Int32" or "Int64" => "100",
                "Nullable(UInt8)" or "Nullable(UInt16)" or "Nullable(UInt32)" or "Nullable(UInt64)" or "Nullable(Int8)" or "Nullable(Int16)" or "Nullable(Int32)" or "Nullable(Int64)" => "100",
                "Float32" or "Float64" => "100",
                "Nullable(Float32)" or "Nullable(Float64)" => "100",
                "DateTime" or "DateTime64(3)" => "100",
                "Nullable(DateTime)" or "Nullable(DateTime64(3))" => "100",
                _ => "200",
            };

            // escape single quotes from data type
            string dataType = col["Data Type"].ToString()?.Replace("'", "''") ?? "";

            string insertQuery = "INSERT INTO ClickSphere.ViewColumns (Id, Database, ViewId, ColumnName, DataType, ControlType, Sorter, Width) " +
                                 $"VALUES ('{Guid.NewGuid()}','{database}','{viewId}','{col["Column Name"]}','{dataType}','{controlType}',{sorter}, {columnWidth});";

            await _dbService.ExecuteNonQuery(insertQuery);
            ++sorter;
        }
        return true;
    }

    /// <summary>
    /// Get columns, data types and descriptions of a view
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the columns from</param>
    /// <returns>View definition as string</returns>
    public async Task<string> GetViewDefinition(string database, string viewId)
    {
        string query = $"SELECT ColumnName, DataType, Description " +
                       $"FROM ClickSphere.ViewColumns " +
                       $"WHERE Database = '{database}' AND ViewId = '{viewId}' order by Sorter";

        var output = await _dbService.ExecuteQueryDictionary(query);
        if (output == null)
            return "";

        // create view definition as CSV
        StringBuilder csv = new();
        csv.AppendLine("ColumnName, ColumnDataType, ColumnDescription");
        foreach (var row in output)
        {
            csv.AppendLine($"{row["ColumnName"]}, {row["DataType"]}, {row["Description"]}");
        }

        return csv.ToString();
    }

    /// <summary>
    /// Update view column configuration
    /// </summary>
    /// <param name="column">The column to update</param>
    /// <returns>The result of the column update</returns>
    public async Task<IResult> UpdateViewColumn(ViewColumns column)
    {
        if (column == null)
            return Results.BadRequest("Column is null");

        // escape single quotes
        string placeholder = column.Placeholder!.Replace("'", "''");
        string description = column.Description!.Replace("'", "''");

        string query = $"ALTER TABLE ClickSphere.ViewColumns " +
                       $"UPDATE ControlType = '{column.ControlType}', " +
                       $"Placeholder = '{placeholder}', " +
                       $"Sorter = {column.Sorter}, " +
                       $"Description = '{description}', " +
                       $"Width = {column.Width} " +
                       $"WHERE Id = '{column.Id}';";

        int result = await _dbService.ExecuteNonQuery(query);
        if (result == 0)
            return Results.Ok();
        else
            return Results.BadRequest("Could not update column");
    }

    /// <summary>
    /// Get the distinct values of a column
    /// </summary>
    /// <param name="database">The database to get the data from</param>
    /// <param name="viewId">The viewId to get the data from</param>
    /// <param name="column">The column to get the distinct values from</param>
    /// <returns>The distinct values of the column</returns>
    public async Task<IList<string>> GetDistinctValues(string database, string viewId, string column)
    {
        if (string.IsNullOrEmpty(database) || string.IsNullOrEmpty(viewId) || string.IsNullOrEmpty(column))
            return [];

        return await _dbService.ExecuteQuery(
            $"SELECT DISTINCT `{column}` FROM `{database}`.`{viewId}` WHERE `{column}` is not null AND `{column}` != '' ORDER BY 1 LIMIT 100") ?? [];
    }

    /// <summary>
    /// Import view from ODBC table into ClickHouse.
    /// </summary>
    /// <param name="view">The view name</param>
    /// <param name="dropExisting">Whether to drop the existing view</param>
    /// <returns>ok if the view was imported successfully, error message otherwise</returns>
    public async Task<IResult> ImportViewFromODBC(string view, bool dropExisting = false)
    {
        if (string.IsNullOrEmpty(view))
        {
            return Results.BadRequest("Invalid request");
        }

        // check if system table
        if (view == "Views" || view == "ViewColumns" || view == "Embeddings" || view == "Config" || view == "Users")
        {
            return Results.BadRequest("Cannot import system tables");
        }

        // We need this view on our server:
        // --------------------------------
        // CREATE VIEW V_VIEW_COLUMNS AS
        // SELECT 
        //     TABLE_NAME,
        //     COLUMN_NAME,
        //     DATA_TYPE,
        //     CHARACTER_MAXIMUM_LENGTH,
        //     IS_NULLABLE
        // FROM 
        //     INFORMATION_SCHEMA.COLUMNS

        string odbcConnectionString = $"DSN={ODBC_DSN};Uid={ODBC_User};Pwd={ODBC_Password};Database={ODBC_Database};";

        // get columns from ODBC view
        IList<Dictionary<string, object>> columns;
        string query =
         "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE " +
        $"FROM odbc('{odbcConnectionString}', '', 'V_VIEW_COLUMNS') " +
        $"WHERE TABLE_NAME = '{view}';";

        columns = await _dbService.ExecuteQueryDictionary(query);

        if (columns.Count == 0)
        {
            return Results.BadRequest("No columns found in ODBC view");
        }

        // convert from MS SQL to ClickHouse data types
        foreach (var column in columns)
        {
            string dataType = column["DATA_TYPE"].ToString() ?? "String";
            string chDataType = dataType switch
            {
                "tinyint" => "UInt8",
                "smallint" => "Int16",
                "int" => "Int32",
                "bigint" => "UInt64",
                "float" => "Float32",
                "double" => "Float64",
                "varchar" => "String",
                "datetime" => "DateTime64",
                "date" => "DateTime64",
                "time" => "DateTime64",
                "bit" => "UInt8",
                "decimal" => "Decimal",
                "numeric" => "Decimal",
                "char" => "String",
                "nvarchar" => "String",
                "nchar" => "String",
                "ntext" => "String",
                "text" => "String",
                "uniqueidentifier" => "UUID",
                "real" => "Float32",
                "money" => "Decimal",
                "smallmoney" => "Decimal",
                "binary" => "String",
                "varbinary" => "String",
                "image" => "String",
                "timestamp" => "String",
                "xml" => "String",
                "geography" => "String",
                "geometry" => "String",
                "hierarchyid" => "String",
                "sql_variant" => "String",
                "sysname" => "String",
                "datetime2" => "DateTime64",
                "datetimeoffset" => "DateTime64",
                "smalldatetime" => "DateTime64",
                _ => "String"
            };

            // add length to data type
            if (!string.IsNullOrEmpty(column["CHARACTER_MAXIMUM_LENGTH"].ToString()) &&
                chDataType != "String" && chDataType != "UUID" && chDataType != "DateTime64")
            {
                chDataType += $"({column["CHARACTER_MAXIMUM_LENGTH"]})";
            }

            column["DATA_TYPE"] = chDataType;
        }

        if (dropExisting)
        {
            query = $"DROP VIEW IF EXISTS ClickSphere.{view};";
            await _dbService.ExecuteNonQuery(query);

            query = $"DROP TABLE IF EXISTS ClickSphere.{view}_TBL;";
            await _dbService.ExecuteNonQuery(query);

            query = $"DELETE FROM ClickSphere.Views WHERE Name = '{view}';";
            await _dbService.ExecuteNonQuery(query);
        }

        query = $"CREATE TABLE IF NOT EXISTS ClickSphere.{view}_TBL (";

        // build create table statement
        foreach (var column in columns)
        {
            query += $"`{column["COLUMN_NAME"]}` {column["DATA_TYPE"]} ";

            if (column["IS_NULLABLE"].ToString() == "YES" && column["COLUMN_NAME"].ToString()?.ToUpper() != "ID")
                query += "NULL DEFAULT NULL";
            else
                query += "NOT NULL";

            // add comma if not last column
            if (columns.IndexOf(column) < columns.Count - 1)
                query += ", ";
        }

        // each view has an ID column
        query += ") ENGINE = ReplacingMergeTree(ID) ORDER BY ID SETTINGS allow_experimental_replacing_merge_with_cleanup = 1;";
        await _dbService.ExecuteNonQuery(query);

        // call ReplaceUmlautsInView procedure on ODBC server
        await ExecuteOdbc($"EXEC ReplaceUmlautsInView '{view}'");

        // fill table with data
        query = $"INSERT INTO ClickSphere.{view}_TBL " +
                $"SELECT * FROM odbc('{odbcConnectionString}', '', '{view}_CH');";
        await _dbService.ExecuteNonQuery(query);

        // add view configuration to ClickSphere.Views
        var result = await CreateView(new View
        {
            Id = view,
            Name = view,
            Description = "Imported via ODBC",
            Database = "ClickSphere",
            Query = $"SELECT * FROM ClickSphere.{view}_TBL",
            Type = "V",
            Questions = ""
        });

        if (result != null && result.IsSuccessStatusCode)
            return Results.Ok();
        else
            return Results.BadRequest(result?.ReasonPhrase);
    }

    /// <summary>
    /// Synchronize view table with ODBC table
    /// </summary>
    /// <param name="view">The view to synchronize</param>
    /// <returns>The result of the synchronization</returns>
    public async Task<IResult> SynchronizeView(string view)
    {
        if (string.IsNullOrEmpty(view))
        {
            return Results.BadRequest("Invalid request");
        }

        // check if system table
        if (view == "Views" || view == "ViewColumns" || view == "Embeddings" || view == "Config" || view == "Users")
        {
            return Results.BadRequest("Cannot synchronize system tables");
        }

        string odbcConnectionString = $"DSN={ODBC_DSN};Uid={ODBC_User};Pwd={ODBC_Password};Database={ODBC_Database};";

        // synchronize deleted rows
        string query = $"SELECT ID FROM ClickSphere.{view}_TBL";
        IList<string> ids = await _dbService.ExecuteQuery(query) ?? [];

        query = $"SELECT ID FROM odbc('{odbcConnectionString}', '', '{view}_CH')";
        IList<string> odbcIds = await _dbService.ExecuteQuery(query) ?? [];

        var idsToDelete = ids.Except(odbcIds).ToList();
        if (idsToDelete.Count > 0)
        {
            query = $"ALTER TABLE ClickSphere.{view}_TBL DELETE WHERE ID IN ({string.Join(",", idsToDelete.Select(id => $"'{id}'"))});";
            await _dbService.ExecuteNonQuery(query);
        }

        // execute insert query
        query = $"INSERT INTO ClickSphere.{view}_TBL " +
                $"SELECT * FROM odbc('{odbcConnectionString}', '', '{view}_CH');";

        try
        {
            await _dbService.ExecuteNonQuery(query);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }

        // call OPTIMIZE TABLE to delete outdated rows
        query = $"OPTIMIZE TABLE ClickSphere.{view}_TBL FINAL CLEANUP;";

        try
        {
            await _dbService.ExecuteNonQuery(query);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }

        return Results.Ok();
    }

    /// <summary>
    /// Get column from ODBC view
    /// </summary>
    /// <param name="view">The view name</param>
    /// <param name="columnName">The column name</param>
    /// <returns>List of strings</returns>
    public async Task<IList<string>> GetColumnsFromODBC(string view, string columnName)
    {
        string query =
            $"select {columnName} " +
            $"from odbc('DSN={ODBC_DSN};Uid={ODBC_User};Pwd={ODBC_Password};Database={ODBC_Database};', '', '{view}');";

        return await _dbService.ExecuteQuery(query);
    }

    /// <summary>
    /// Get views from ODBC
    /// </summary>
    /// <returns>List of views available for import as strings</returns>
    public async Task<IList<string>> GetViewsFromODBC()
    {
        string query = $"SELECT VIEW_NAME FROM odbc('DSN={ODBC_DSN};Uid={ODBC_User};Pwd={ODBC_Password};Database={ODBC_Database};', '', 'VIEW_SETTINGS');";
        return await _dbService.ExecuteQuery(query);
    }

    private async Task<int> ExecuteOdbc(string sql)
    {
        string connectionString = $"DSN={ODBC_DSN};UID={ODBC_User};PWD={ODBC_Password};";
        using OdbcConnection connection = new(connectionString);
        connection.Open();
        using OdbcCommand command = new(sql, connection);
        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Export view to Excel
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="viewId">The ID of the view</param>
    /// <param name="fileName">The name of the file</param>
    /// <returns>The result of the export</returns>
    public async Task<FileResult?> ExportToExcel(string query, string viewId, string fileName)
    {
        try
        {
            // Get all rows from the query
            var rowData = await _dbService.ExecuteQueryDictionary(query);
            if (rowData == null)
                return null;

            // Get column names
            var columns = (await GetViewColumns("ClickSphere", viewId, false))
                .Select(c => c.ColumnName)
                .Where(name => name != null)
                .Select(name => name!)
                .ToList();

            // Use DocumentFormat.OpenXml to create Excel file
            using var stream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                // Add parts
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

                // Create worksheet
                var worksheet = new Worksheet();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = worksheet;

                // Create columns with widths
                var columnsList = new Columns();
                for (int i = 0; i < columns.Count; ++i)
                {
                    columnsList.Append(new Column
                    {
                        Min = (uint)(i + 1),
                        Max = (uint)(i + 1),
                        Width = 15,
                        CustomWidth = true
                    });
                }
                worksheet.Append(columnsList);

                // Sanitize sheet name
                var invalidCharsRegex = new Regex(@"[\:\/\\\?*\[\]]");
                string sanitizedSheetName = invalidCharsRegex.Replace(fileName, "");
                sanitizedSheetName = sanitizedSheetName.Length > 31
                    ? sanitizedSheetName.Substring(0, 31)
                    : sanitizedSheetName;

                // Add worksheet to workbook
                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = sanitizedSheetName
                };
                sheets.Append(sheet);

                // Create styles
                var workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                workbookStylesPart.Stylesheet = CreateExcelStyleSheet(workbookPart);

                // Create header row with formatting
                var headerRow = new Row { Height = 20, CustomHeight = true };
                foreach (var column in columns)
                {
                    var cell = new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(column),
                        StyleIndex = 2 // Header style
                    };
                    headerRow.Append(cell);
                }
                sheetData.Append(headerRow);

                // Check for valid data
                if (rowData.Count == 0 || columns.Count == 0)
                {
                    worksheet.Append(sheetData);
                    workbookPart.Workbook.Save();

                    stream.Seek(0, SeekOrigin.Begin);
                    return CreateExcelResult(stream, fileName);
                }

                // Create data rows
                foreach (var row in rowData)
                {
                    var dataRow = new Row();
                    foreach (var column in columns)
                    {
                        Cell cell = new();

                        if (!row.TryGetValue(column, out var value))
                        {
                            cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(""),
                                StyleIndex = 1
                            };
                            dataRow.Append(cell);
                            continue;
                        }

                        if (value == null)
                        {
                            cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(""),
                                StyleIndex = 1
                            };
                        }
                        else if (value is object objectValue)
                        {
                            var stringValue = objectValue.ToString();

                            if(string.IsNullOrEmpty(stringValue))
                            {
                                cell = new Cell
                                {
                                    DataType = CellValues.String,
                                    CellValue = new CellValue(""),
                                    StyleIndex = 1
                                };
                            }
                            // if string is datetime like "04/16/2020 00:00:00 +02:00"
                            else if (Regex.IsMatch(stringValue, @"^\d{2}/\d{2}/\d{4} \d{2}:\d{2}:\d{2} \+\d{2}:\d{2}$"))
                            {
                                var date = ParseDateTimeString(stringValue);
                                if (date != null)
                                {
                                    double oaDate = date.Value.ToOADate();
                                    cell = new Cell
                                    {
                                        DataType = CellValues.Number,
                                        CellValue = new CellValue(oaDate.ToString(CultureInfo.InvariantCulture)),
                                        StyleIndex = 3 // Date format
                                    };
                                }
                            }
                            else
                            {
                                cell = new Cell
                                {
                                    DataType = CellValues.String,
                                    CellValue = new CellValue(stringValue),
                                    StyleIndex = 1
                                };
                            }
                        }
                        else if (value is DateTime dateTimeValue)
                        {
                            double oaDate = dateTimeValue.ToOADate();
                            cell = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(oaDate.ToString(CultureInfo.InvariantCulture)),
                                StyleIndex = 3 // Date format
                            };
                        }
                        else if (value is int || value is long || value is double || value is float || value is decimal)
                        {
                            string number = string.Format(CultureInfo.InvariantCulture, "{0}", value);
                            cell = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(number),
                                StyleIndex = 1
                            };
                        }
                        else
                        {
                            cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(value.ToString() ?? ""),
                                StyleIndex = 1
                            };
                        }
                        dataRow.Append(cell);
                    }
                    sheetData.Append(dataRow);
                }

                worksheet.Append(sheetData);
                workbookPart.Workbook.Save();
            }
            stream.Seek(0, SeekOrigin.Begin);
            return CreateExcelResult(stream, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting to Excel: {ex.Message}");
            return null;
        }
    }

    private FileContentResult CreateExcelResult(MemoryStream stream, string fileName)
    {
        string extension = ".xlsx";
        string downloadName = fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? fileName
            : fileName + extension;

        return new FileContentResult(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = downloadName
        };
    }

    private Stylesheet CreateExcelStyleSheet(WorkbookPart workbookPart)
    {
        var stylesheet = new Stylesheet();

        // Create fonts
        var fonts = new Fonts { Count = 2 };
        fonts.Append(new Font()); // Default font
        fonts.Append(
            new Font(
                new Bold(),
                new FontSize { Val = 11 },
                new Color { Rgb = new HexBinaryValue { Value = "000000" } }
            )
        ); // Bold font for headers
        stylesheet.Fonts = fonts;

        // Create fills
        var fills = new Fills { Count = 3 };
        fills.Append(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // Index 0
        fills.Append(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // Index 1
        fills.Append(
            new Fill(
                new PatternFill
                {
                    PatternType = PatternValues.Solid,
                    ForegroundColor = new ForegroundColor { Rgb = "DDEBF7" }, // Light blue header
                    BackgroundColor = new BackgroundColor { Indexed = 64 }
                }
            )
        ); // Index 2 - Header fill
        stylesheet.Fills = fills;

        // Create borders
        var borders = new Borders { Count = 2 };
        borders.Append(new Border()); // Default border
        borders.Append(
            new Border(
                new LeftBorder { Style = BorderStyleValues.Thin },
                new RightBorder { Style = BorderStyleValues.Thin },
                new TopBorder { Style = BorderStyleValues.Thin },
                new BottomBorder { Style = BorderStyleValues.Thin }
            )
        ); // Border for cells
        stylesheet.Borders = borders;

        // Create cell formats
        var cellFormats = new CellFormats { Count = 4 };
        cellFormats.Append(new CellFormat()); // Default format (0)

        // Regular cell with border (1)
        cellFormats.Append(
            new CellFormat
            {
                FontId = 0,
                FillId = 0,
                BorderId = 1,
                ApplyBorder = true
            }
        );

        // Header cell with border and fill (2)
        cellFormats.Append(
            new CellFormat
            {
                FontId = 1,
                FillId = 2,
                BorderId = 1,
                ApplyFill = true,
                ApplyFont = true,
                ApplyBorder = true,
                Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center }
            }
        );

        // Date cell format (3)
        cellFormats.Append(
            new CellFormat
            {
                FontId = 0,
                FillId = 0,
                BorderId = 1,
                ApplyBorder = true,
                NumberFormatId = 14, // Date format
                ApplyNumberFormat = true
            }
        );

        stylesheet.CellFormats = cellFormats;
        return stylesheet;
    }

    // parse date from extended iso to localdatetime
    public DateTime? ParseDateTimeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return null;

        try
        {
            // 
            if (input.Contains("+") || input.Contains("Z"))
            {
                // convert to DateTime with NodaTime
                var pattern = OffsetDateTimePattern.CreateWithInvariantCulture("MM/dd/yyyy HH:mm:ss +o<HH:mm>");
                var result = pattern.Parse(input);
                return result.Value.LocalDateTime.ToDateTimeUnspecified();
            }
        }
        catch
        {
            return null;
        }
        return null;
    }

}