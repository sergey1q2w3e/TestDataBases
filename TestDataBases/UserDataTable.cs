using System.Data;

namespace TestDataBases
{
    public class UserDataTable
    {
        public string ParentDBName { get; }

        public DataTable Table { get; set; }
        public UserDataTable(string parentDB, string name)
        {
            ParentDBName = parentDB;
            Table = new DataTable();
            Table.TableName = name;
        }
    }
}
