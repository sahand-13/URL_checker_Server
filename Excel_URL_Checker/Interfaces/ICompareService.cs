using Excel_URL_Checker.DTOs;

namespace Excel_URL_Checker.Interfaces
{
    public interface ICompareService
    {
        Task<List<ChildrenDTO>> CompareData(List<ExcelDTO> Data, int Similarity);
    }
}
