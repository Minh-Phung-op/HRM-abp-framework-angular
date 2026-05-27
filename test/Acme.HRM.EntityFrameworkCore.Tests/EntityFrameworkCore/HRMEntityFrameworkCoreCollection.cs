using Xunit;

namespace Acme.HRM.EntityFrameworkCore;

[CollectionDefinition(HRMTestConsts.CollectionDefinitionName)]
public class HRMEntityFrameworkCoreCollection : ICollectionFixture<HRMEntityFrameworkCoreFixture>
{

}
