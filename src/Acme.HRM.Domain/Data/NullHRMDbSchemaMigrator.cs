using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Acme.HRM.Data;

/* This is used if database provider does't define
 * IHRMDbSchemaMigrator implementation.
 */
public class NullHRMDbSchemaMigrator : IHRMDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
