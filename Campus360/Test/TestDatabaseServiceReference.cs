using Campus360.Services;

namespace Campus360.Test
{
    public class TestClass
    {
        public void TestDatabaseServiceReference()
        {
            // Just test that DatabaseService can be referenced
            System.Type dbType = typeof(DatabaseService);
            System.Console.WriteLine($"DatabaseService type: {dbType.FullName}");
        }
    }
}
