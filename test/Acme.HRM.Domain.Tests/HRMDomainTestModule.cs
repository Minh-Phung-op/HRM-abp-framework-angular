using Volo.Abp.Modularity;

namespace Acme.HRM;

[DependsOn(
    typeof(HRMDomainModule),
    typeof(HRMTestBaseModule)
)]
public class HRMDomainTestModule : AbpModule
{

}
