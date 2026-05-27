using Volo.Abp.Modularity;

namespace Acme.HRM;

[DependsOn(
    typeof(HRMApplicationModule),
    typeof(HRMDomainTestModule)
)]
public class HRMApplicationTestModule : AbpModule
{

}
