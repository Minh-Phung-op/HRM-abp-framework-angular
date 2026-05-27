using System.Threading.Tasks;

namespace Acme.HRM.Data;

public interface IHRMDbSchemaMigrator
{
    Task MigrateAsync();
}
