using Acme.HRM.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Acme.HRM.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(HRMEntityFrameworkCoreModule),
    typeof(HRMApplicationContractsModule)
)]
public class HRMDbMigratorModule : AbpModule
{
}
