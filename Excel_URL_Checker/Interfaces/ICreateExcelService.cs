using Excel_URL_Checker.Wrappers;

namespace Excel_URL_Checker.Interfaces
{
    public interface ICreateExcelService
    {
        Task<Response<string>> createExcel(List<ChildrenDTO> Data, List<string> CreatFileName , int Similarity);
    }
}
