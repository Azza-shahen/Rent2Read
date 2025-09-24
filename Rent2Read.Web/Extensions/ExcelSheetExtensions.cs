using ClosedXML.Excel;

namespace Rent2Read.Web.Extensions
{
    public static class ExcelSheetExtensions
    {
        private static int _startRow = 4;//Private field to define the starting row for headers

        public static void AddHeader(this IXLWorksheet sheet, string[] cells)//Method to add header (column titles) to the sheet
        {
            for (int i = 0; i < cells.Length; i++)
                sheet.Cell(_startRow, i + 1).SetValue(cells[i]);

            //var header = sheet.Range(startRow, 1, startRow, cells.Length);
            //header.Style.Fill.BackgroundColor = XLColor.Black;
            //header.Style.Font.FontColor = XLColor.White;
            //header.Style.Font.SetBold();
        }

        public static void Format(this IXLWorksheet sheet)//Method to format the sheet
        {
            sheet.ColumnsUsed().AdjustToContents(); // Auto-adjust column widths based on content
            sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Apply thin black borders around all used cells
            sheet.CellsUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.CellsUsed().Style.Border.OutsideBorderColor = XLColor.Black;
        }
        public static void AddTable(this IXLWorksheet sheet, int numberOfRows, int numberOfColumns)//Method to add a formatted table
        {
            var range = sheet.Range(_startRow, 1, numberOfRows + _startRow, numberOfColumns);
            var table = range.CreateTable();// Create a table from the defined range
            table.Theme = XLTableTheme.TableStyleMedium16;// Apply a built-in table style (theme)

            table.ShowAutoFilter = false;// Disable auto-filters above the table headers
        }


        public static void AddLocalImage(this IXLWorksheet sheet, string imagePath)//Method to add a local image (logo) into the sheet
        {
            sheet.AddPicture(imagePath)
                 .MoveTo(sheet.Cell("A1"))//position image at cell A1
                 .Scale(.4);//resize image to 40% of its original size

        }

    }

}
