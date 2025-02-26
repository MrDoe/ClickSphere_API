using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;
using ClickSphere_API.Models;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using NodaTime.Text;
namespace ClickSphere_API.Controllers;
/// <summary>
/// Controller class for managing views in the ClickSphere database system
/// </summary>
[ApiController]
public class ViewController(IApiViewService viewServices, IDbService dbService) : ControllerBase
{
    private readonly IApiViewService ViewServices = viewServices;
    private readonly IDbService DbService = dbService;

    /// <summary>
    /// Create a new view in the specified database
    /// </summary>
    /// <param name="view">The view to create</param>
    /// <returns>The result of the view creation</returns>
    [Authorize]
    [HttpPost]
    [Route("/createView")]
    public async Task<IResult> CreateView(View view)
    {
        HttpResponseMessage result = await ViewServices.CreateView(view);
        if (result.IsSuccessStatusCode)
        {
            return Results.Ok("View created successfully");
        }
        else
        {
            return Results.BadRequest(result.ReasonPhrase);
        }
    }

    /// <summary>
    /// Create materialized view in the specified database
    /// </summary>
    /// <param name="view">The materialized view to create</param>
    /// <returns>The result of the materialized view creation</returns>
    [Authorize]
    [HttpPost]
    [Route("/createMaterializedView")]
    public async Task<IResult> CreateMaterializedView(MaterializedView view)
    {
        return await ViewServices.CreateMaterializedView(view);
    }

    /// <summary>
    /// Delete a view from a database
    /// </summary>
    /// <param name="database">The database where the view is located</param>
    /// <param name="viewId">The view to delete</param>
    /// <returns>The result of the view deletion</returns>
    [Authorize]
    [HttpDelete]
    [Route("/deleteView")]
    public async Task<IResult> DeleteView(string database, string viewId)
    {
        return await ViewServices.DeleteView(database, viewId);
    }

    /// <summary>
    /// Get all views of a database which are registered in ClickSphere
    /// </summary>
    /// <param name="database">The database to get the views from</param>
    /// <returns>The views of the database</returns>
    [Authorize]
    [HttpGet]
    [Route("/getAllViews")]
    public async Task<IList<View>> GetAllViews(string database)
    {
        return await ViewServices.GetAllViews(database);
    }

    /// <summary>
    /// Get configuration of a view in a database
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the configuration from</param>
    /// <returns>The configuration of the view</returns>
    [Authorize]
    [HttpGet]
    [Route("/getViewConfig")]
    public async Task<View> GetViewConfig(string database, string viewId)
    {
        return await ViewServices.GetViewConfig(database, viewId);
    }

    /// <summary>
    /// Update view configuration
    /// </summary>
    /// <param name="view">The view to update</param>
    /// <returns>The result of the view update</returns>
    [Authorize]
    [HttpPost]
    [Route("/updateView")]
    public async Task<IResult> UpdateView(View view)
    {
        return await ViewServices.UpdateView(view);
    }

    /// <summary>
    /// Get view columns and type for QBE search
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the columns from</param>
    /// <param name="forceUpdate">Force update of the columns</param>
    /// <returns>The columns of the view</returns>
    [Authorize]
    [HttpGet]
    [Route("/getViewColumns")]
    public async Task<IList<ViewColumns>> GetViewColumns(string database, string viewId, bool forceUpdate)
    {
        return await ViewServices.GetViewColumns(database, viewId, forceUpdate);
    }

    /// <summary>
    /// Get view definition for AI search
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the definition from</param>
    /// <returns>The view definition</returns>
    [Authorize]
    [HttpGet]
    [Route("/getViewDefinition")]
    public async Task<string> GetViewDefinition(string database, string viewId)
    {
        return await ViewServices.GetViewDefinition(database, viewId);
    }


    /// <summary>
    /// Update configuration of a view column 
    /// </summary>
    /// <param name="columns">The columns to update</param>
    /// <returns>The result of the column update</returns>
    [Authorize]
    [HttpPost]
    [Route("/updateViewColumn")]
    public async Task<IResult> UpdateViewColumn(ViewColumns columns)
    {
        return await ViewServices.UpdateViewColumn(columns);
    }

    /// <summary>
    /// Get the distinct values of a column
    /// </summary>
    /// <param name="database">The database to get the data from</param>
    /// <param name="viewId">The viewId to get the data from</param>
    /// <param name="columnName">The column to get the distinct values from</param>
    /// <returns>The distinct values of the column</returns>
    [Authorize]
    [HttpGet]
    [Route("/getDistinctValues")]
    public async Task<IList<string>> GetDistinctValues(string database, string viewId, string columnName)
    {
        if (database == null || viewId == null || columnName == null)
        {
            throw new ArgumentNullException("database, viewId or columnName is null");
        }

        return await ViewServices.GetDistinctValues(database, viewId, columnName);
    }

    /// <summary>
    /// Export view to Excel file
    /// </summary>
    /// <param name="b64query">The view query to export</param>
    /// <param name="viewId">The viewId to get the data from</param>
    /// <param name="fileName">The name of the file</param>
    /// <returns>The Excel file</returns>
    [HttpGet]
    [Route("/exportToExcel")]
    public async Task ExportToExcel(string b64query, string viewId, string fileName)
    {
        if (string.IsNullOrEmpty(b64query) || string.IsNullOrEmpty(viewId) || string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException("b64query, viewId or fileName is null or empty");
        }

        string query = Encoding.UTF8.GetString(Convert.FromBase64String(b64query));

        // Get column names before processing data
        var columns = (await ViewServices.GetViewColumns("ClickSphere", viewId, false))
            .Select(c => c.ColumnName)
            .Where(name => name != null)
            .Select(name => name!)
            .ToList();

        // Setup response headers for Excel file download
        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");

        // Stream data in batches from database
        const int batchSize = 10000;

        // EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Use EPPlus with streaming mode for large files
        using var package = new ExcelPackage();
        package.Workbook.CalcMode = ExcelCalcMode.Manual;
        package.Workbook.Properties.Manager = string.Empty;
        package.Workbook.Properties.Company = string.Empty;
        package.Compression = CompressionLevel.BestSpeed;
        var worksheet = package.Workbook.Worksheets.Add(fileName);

        // Apply preset column widths instead of using autofit
        const int defaultColumnWidth = 15; // Set a reasonable default width
        for (int i = 0; i < columns.Count; ++i)
        {
            worksheet.Column(i + 1).Width = defaultColumnWidth;
        }

        // Create and style header row as a single operation
        var headerRange = worksheet.Cells[1, 1, 1, columns.Count];
        for (int i = 0; i < columns.Count; ++i)
        {
            worksheet.Cells[1, i + 1].Value = columns[i];
        }

        var headerStyle = headerRange.Style;
        headerStyle.Font.Bold = true;
        headerStyle.Fill.PatternType = ExcelFillStyle.Solid;
        headerStyle.Fill.BackgroundColor.SetColor(Color.LightBlue);

        // Add pagination to the query if it doesn't already have it
        string paginatedQuery = query.TrimEnd(';');
        if (!paginatedQuery.Contains(" LIMIT "))
        {
            paginatedQuery += " LIMIT {0}, {1}";
        }

        // Use pagination to fetch and process data in batches
        int rowIndex = 2; // Start after header
        int offset = 0;
        bool hasMoreData = true;

        while (hasMoreData)
        {
            // Create query with current pagination parameters
            string batchQuery = string.Format(paginatedQuery, offset, batchSize);
            var rowsData = new List<object[]>(batchSize);
            int rowsInBatch = 0;
            var dateCache = new Dictionary<string, string>(StringComparer.Ordinal);

            // Collect batch data as arrays for bulk insertion
            await foreach (var rowDict in DbService.ExecuteQueryAsStream(batchQuery))
            {
                var rowData = new object[columns.Count];
                for (int c = 0; c < columns.Count; ++c)
                {
                    // get value from rowDict
                    if (rowDict.TryGetValue(columns[c], out var value) && value != null)
                    {
                        string strValue = value.ToString() ?? string.Empty;
                        // Handle dates with caching for repeating values
                        if (value is DateTime || TryIdentifyDateString(strValue))
                        {
                            if (dateCache.TryGetValue(strValue, out var formattedDate))
                            {
                                rowData[c] = formattedDate;
                            }
                            else
                            {
                                var date = ParseDateTimeString(strValue);
                                if (date.HasValue)
                                {
                                    var formatted = date.Value.ToString("dd.MM.yyyy HH:mm");
                                    dateCache[strValue] = formatted;
                                    rowData[c] = formatted;
                                }
                                else
                                {
                                    rowData[c] = strValue;
                                }
                            }
                        }
                        else
                        {
                            // Escape special characters if needed
                            if (strValue.Contains(';') || strValue.Contains('\n') || strValue.Contains('"'))
                            {
                                rowData[c] = strValue.Replace("\"", "\"\"");
                            }
                            else
                            {
                                rowData[c] = strValue;
                            }
                        }
                    }
                }
                rowsData.Add(rowData);
                ++rowsInBatch;
            }

            // Write batch data in one operation if we have rows
            if (rowsData.Count > 0)
            {
                // Bulk set values
                var dataRange = worksheet.Cells[rowIndex, 1, rowIndex + rowsData.Count - 1, columns.Count];
                dataRange.LoadFromArrays([.. rowsData]);

                rowIndex += rowsData.Count;
            }

            // If we got fewer rows than the batch size, we've reached the end
            if (rowsInBatch < batchSize)
            {
                hasMoreData = false;
            }
            else
            {
                // Move to next batch
                offset += batchSize;

                // Force garbage collection to prevent memory pressure
                GC.Collect(0, GCCollectionMode.Optimized);
            }
        }

        // Write directly to the response stream
        await package.SaveAsAsync(Response.Body, CancellationToken.None);
    }


    /// <summary>
    /// Export to CSV file
    /// </summary>
    /// <param name="b64query">The view query to export</param>
    /// <param name="viewId">The viewId to get the data from</param>
    /// <param name="fileName">The name of the file</param>
    /// <returns>The CSV file as stream</returns>
    [HttpGet]
    [Route("/exportToCsv")]
    public async Task ExportToCsv(string b64query, string viewId, string fileName)
    {
        if (string.IsNullOrEmpty(b64query) || string.IsNullOrEmpty(viewId) || string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException("b64query, viewId or fileName is null or empty");
        }

        string query = Encoding.UTF8.GetString(Convert.FromBase64String(b64query));

        // Get column names before processing data
        var columns = (await ViewServices.GetViewColumns("ClickSphere", viewId, false))
            .Select(c => c.ColumnName)
            .Where(name => name != null)
            .Select(name => name!)
            .ToList();

        // Setup response headers for CSV file download
        Response.ContentType = "text/csv";
        Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");

        // Use a buffered approach with StringBuilder for better performance
        const int batchSize = 1000;
        var buffer = new StringBuilder(batchSize * 100); // Pre-allocate reasonable size

        // Use a stream writer with appropriate buffer size
        await using var writer = new StreamWriter(Response.Body, Encoding.Latin1, bufferSize: 100000, leaveOpen: true);

        // Write header row
        buffer.AppendLine("sep=;"); // Excel-specific separator hint
        buffer.AppendLine(string.Join(";", columns));

        // Track dates to avoid repeated parsing of identical values
        var dateCache = new Dictionary<string, string>(StringComparer.Ordinal);
        int rowCount = 0;

        // Process data
        await foreach (var rowDict in DbService.ExecuteQueryAsStream(query))
        {
            // Build row as we go rather than using string.Join
            bool first = true;
            foreach (var column in columns)
            {
                if (!first) buffer.Append(';');
                first = false;

                if (rowDict.TryGetValue(column, out var value) && value != null)
                {
                    string strValue = value.ToString() ?? string.Empty;
                    // Handle dates with caching for repeating values
                    if (value is DateTime || TryIdentifyDateString(strValue))
                    {
                        if (dateCache.TryGetValue(strValue, out var formattedDate))
                        {
                            buffer.Append(formattedDate);
                        }
                        else
                        {
                            var date = ParseDateTimeString(strValue);
                            if (date.HasValue)
                            {
                                var formatted = date.Value.ToString("dd.MM.yyyy HH:mm");
                                dateCache[strValue] = formatted;
                                buffer.Append(formatted);
                            }
                            else
                            {
                                buffer.Append(strValue);
                            }
                        }
                    }
                    else
                    {
                        // Escape special characters if needed
                        if (strValue.Contains(';') || strValue.Contains('\n') || strValue.Contains('"'))
                        {
                            buffer.Append('"').Append(strValue.Replace("\"", "\"\"")).Append('"');
                        }
                        else
                        {
                            buffer.Append(strValue);
                        }
                    }
                }
            }
            buffer.AppendLine();

            ++rowCount;

            // Write in batches to avoid excessive memory usage
            if (rowCount % batchSize == 0)
            {
                await writer.WriteAsync(buffer);
                buffer.Clear();

                // Periodically clean date cache if it grows too large
                if (dateCache.Count > 10000)
                {
                    dateCache.Clear();
                }
            }
        }

        // Write any remaining data
        if (buffer.Length > 0)
        {
            await writer.WriteAsync(buffer);
        }

        // Explicitly flush the writer
        await writer.FlushAsync();
    }

    private bool TryIdentifyDateString(string value)
    {
        // Quick check to avoid unnecessary date parsing attempts
        return value.Length >= 10 &&
               value.Length <= 30 &&
               char.IsDigit(value[0]) &&
               (value.Contains('/') || value.Contains('-'));
    }

    private static readonly OffsetDateTimePattern Pattern =
    OffsetDateTimePattern.CreateWithInvariantCulture("MM/dd/yyyy HH:mm:ss +o<HH:mm>");

    // parse date from extended iso to localdatetime
    private DateTime? ParseDateTimeString(string input)
    {
        try
        {
            // convert to DateTime with NodaTime
            var result = Pattern.Parse(input);
            return result.Value.LocalDateTime.ToDateTimeUnspecified();
        }
        catch
        {
            return null;
        }
    }
}
