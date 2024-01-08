using Excel_URL_Checker.DTOs;

namespace Excel_URL_Checker.Interfaces
{
    public interface IDatasourceService
    {
        Task<List<ExcelDTO>> LoadDataSource(int Similarity, List<string> DBList);
    }
}
