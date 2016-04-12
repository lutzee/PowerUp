using Id.DatabaseMigration.SqlServer;

namespace Id.DatabaseMigration
{
  public static class AmbientSettings
  {
    public static ISqlServerSettings Settings { get; set; }
  }
}
