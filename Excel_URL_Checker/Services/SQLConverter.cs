using System.Data;
using System.Reflection;

namespace Excel_URL_Checker.Services
{
    public class SQLConverter
    {
        public static List<T> DataReaderMapToList<T>(IDataReader dr)
        {
            List<T> list = new List<T>();
            T obj = default(T);
            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (prop.PropertyType == typeof(int?) && !object.Equals(dr[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, Convert.ToInt32(dr[prop.Name]), null);
                    }
                     else if (!object.Equals(dr[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, dr[prop.Name], null);
                    }
                }
                list.Add(obj);
            }
            return list;
        }
    }
}
