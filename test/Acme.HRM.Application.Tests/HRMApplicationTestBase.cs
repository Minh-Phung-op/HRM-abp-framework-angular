using Volo.Abp.Modularity;

namespace Acme.HRM;

public abstract class HRMApplicationTestBase<TStartupModule> : HRMTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
