using Acme.HRM.Samples;
using Xunit;

namespace Acme.HRM.EntityFrameworkCore.Domains;

[Collection(HRMTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<HRMEntityFrameworkCoreTestModule>
{

}
