using Excel_URL_Checker.Interfaces;
using Excel_URL_Checker.Wrappers;
using OfficeOpenXml;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Formula.Functions;
using OfficeOpenXml.Drawing.Controls;
using System.Collections.Concurrent;

namespace Excel_URL_Checker.Services
{
    public class CreateExcelService : ICreateExcelService
    {
        public CreateExcelService() { }

        public async Task<Response<string>> createExcel(List<ChildrenDTO> Data, List<string> CreatFileName, int Similarity)
        {
            // Create workbook and worksheet
            IWorkbook workbook = new XSSFWorkbook();
            ISheet worksheet = workbook.CreateSheet("MasterDetail");

            // Set headers
            IRow headerRow = worksheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Group");
            headerRow.CreateCell(1).SetCellValue("Child");
            headerRow.CreateCell(2).SetCellValue("Group Search Rate");
            headerRow.CreateCell(3).SetCellValue("Group Difficulty");

            // parallel

            //int currentRow = 1;
            //object lockObject = new object();

            //// Create style for parent rows
            //ICellStyle parentRowStyle = workbook.CreateCellStyle();
            //parentRowStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
            //parentRowStyle.FillPattern = FillPattern.SolidForeground;

            //ConcurrentDictionary<int, ConcurrentBag<ChildrenDTO>> childItemsDict = new ConcurrentDictionary<int, ConcurrentBag<ChildrenDTO>>();

            //Parallel.ForEach(Data, (item) =>
            //{
            //    // Populate data
            //    IRow dataRow;
            //    lock (lockObject)
            //    {
            //        dataRow = worksheet.CreateRow(currentRow);
            //        dataRow.RowStyle = parentRowStyle;
            //        currentRow++;
            //    }

            //    dataRow.CreateCell(0).SetCellValue(item.Key);
            //    dataRow.CreateCell(1).SetCellValue(item?.SimilarityChildrens.Count ?? 0);
            //    dataRow.CreateCell(2).SetCellValue(item?.SearchRate ?? 0);
            //    dataRow.CreateCell(3).SetCellValue(item?.Difficulty ?? 0);

            //    if (item.SimilarityChildrens != null && item.SimilarityChildrens.Count > 0)
            //    {
            //        ConcurrentBag<ChildrenDTO> childItems = new ConcurrentBag<ChildrenDTO>();

            //        foreach (var child in item.SimilarityChildrens)
            //        {
            //            childItems.Add(child);

            //            lock (lockObject)
            //            {
            //                dataRow = worksheet.CreateRow(currentRow);
            //                currentRow++;
            //            }

            //            dataRow.CreateCell(1).SetCellValue(child.Key);
            //            dataRow.CreateCell(2).SetCellValue(child?.SearchRate ?? 0);
            //            dataRow.CreateCell(3).SetCellValue(child?.Difficulty ?? 0);
            //        }

            //        childItemsDict.TryAdd(currentRow - item.SimilarityChildrens.Count - 1, childItems);
            //    }
            //});

            //foreach (var kvp in childItemsDict)
            //{
            //    int startRow = kvp.Key;
            //    int endRow = startRow + kvp.Value.Count;

            //    worksheet.GroupRow(startRow, endRow);
            //    worksheet.SetRowGroupCollapsed(startRow, false);
            //}

            // 




            int currentRow = 1;
            // Create style for parent rows
            ICellStyle parentRowStyle = workbook.CreateCellStyle();
            parentRowStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
            parentRowStyle.FillPattern = FillPattern.SolidForeground;

            foreach (var item in Data)
            {
                // Populate data

                IRow dataRow = worksheet.CreateRow(currentRow);

                dataRow.RowStyle = parentRowStyle;

                dataRow.CreateCell(0).SetCellValue(item.Key);
                dataRow.CreateCell(1).SetCellValue(item?.SimilarityChildrens.Count ?? 0);
                dataRow.CreateCell(2).SetCellValue(item?.SearchRate ?? 0);
                dataRow.CreateCell(3).SetCellValue(item?.Difficulty ?? 0);




                if (item.SimilarityChildrens != null && item.SimilarityChildrens.Count > 0)
                {
                    foreach (var child in item.SimilarityChildrens)
                    {
                        currentRow++;
                        IRow childRow = worksheet.CreateRow(currentRow);
                        childRow.CreateCell(1).SetCellValue(child.Key);
                        childRow.CreateCell(2).SetCellValue(child?.SearchRate ?? 0);
                        childRow.CreateCell(3).SetCellValue(child?.Difficulty ?? 0);
                    }
                }

                // Group and collapse rows
                if (item.SimilarityChildrens != null && item.SimilarityChildrens.Count > 0)
                {
                    int startRow = (currentRow + 1 - item.SimilarityChildrens.Count);
                    int endRow = currentRow + 1;

                    worksheet.GroupRow(startRow, endRow);
                    worksheet.SetRowGroupCollapsed(startRow, false);
                }

                currentRow++;
            }

            // Auto-size columns
            for (int i = 0; i < 4; i++)
            {
                worksheet.AutoSizeColumn(i);
            }

            // Save the Excel file
            using (FileStream fileStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Exports", $"{string.Join("-", CreatFileName) + " - " + Similarity + "%"}.xlsx"), FileMode.Create))
            {
                workbook.Write(fileStream);
            }
            return new Response<string>()
            {
                Data = $"{string.Join("-", CreatFileName) + " - " + Similarity + "%"}.xlsx",
                Succeeded = true,
            };
        }
    }
}
