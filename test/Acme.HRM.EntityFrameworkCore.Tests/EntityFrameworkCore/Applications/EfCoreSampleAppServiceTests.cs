using Acme.HRM.Samples;
using Xunit;

namespace Acme.HRM.EntityFrameworkCore.Applications;

[Collection(HRMTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<HRMEntityFrameworkCoreTestModule>
{

}
