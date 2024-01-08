using Excel_URL_Checker.DTOs;
using Excel_URL_Checker.Interfaces;
using System.Data.Common;
using System.Data.OleDb;

namespace Excel_URL_Checker.Services
{
    public class DatasourceService : IDatasourceService
    {
        public DatasourceService() { }


        public async Task<List<ExcelDTO>> LoadDataSource(int Similarity, List<string> DBList)
        {

            try
            {
                var Response = new List<ExcelDTO>();
                foreach (var DB in DBList)
                {
                    String DataSource = "Provider=Microsoft.ACE.OLEDB.12.0;" +
                                                "Data Source=" + Path.Combine(Directory.GetCurrentDirectory(), "Imports", DB) + ";" +
                                                                                                     "Extended Properties=Excel 8.0;";
                    // Create connection object by using the preceding connection string.
                    OleDbConnection objConn = new OleDbConnection(DataSource);
                    // Open connection with the database.
                    objConn.Open();

                    // The code to follow uses a SQL SELECT command to display the data from the worksheet.
                    // Create new OleDbCommand to return data from worksheet.
                    OleDbCommand objCmdSelect = new OleDbCommand("SELECT * FROM [Sheet1$]", objConn);

                    // Create new OleDbDataAdapter that is used to build a DataSet
                    // based on the preceding SQL SELECT statement.
                    OleDbDataAdapter objAdapter1 = new OleDbDataAdapter();
                    // Pass the Select command to the adapter.
                    objAdapter1.SelectCommand = objCmdSelect;
                    // Create new DataSet to hold information from the worksheet.

                    using (DbDataReader reader = await objCmdSelect.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            var readerResult = SQLConverter.DataReaderMapToList<ExcelDTO>(reader);
                            if (readerResult != null && readerResult.Count > 0)
                            {
                                Response.AddRange(readerResult);
                            }
                        }
                    }
                    objConn.Close();
                }


                return Response;

            }
            catch (Exception e)
            {

                throw;
            }

        }

    }

}
