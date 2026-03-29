using ClosedXML.Excel;

namespace HycoatApi.Services.Reports;

public class ExcelExportService
{
    public byte[] ExportToExcel<T>(List<T> data, string sheetName)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);
        worksheet.Cell(1, 1).InsertTable(data);
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
