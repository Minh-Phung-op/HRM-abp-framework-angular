using Volo.Abp.Modularity;

namespace Acme.HRM;

/* Inherit from this class for your domain layer tests. */
public abstract class HRMDomainTestBase<TStartupModule> : HRMTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
