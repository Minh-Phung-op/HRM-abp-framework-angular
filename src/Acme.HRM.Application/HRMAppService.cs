using Acme.HRM.Localization;
using Volo.Abp.Application.Services;

namespace Acme.HRM;

/* Inherit your application services from this class.
 */
public abstract class HRMAppService : ApplicationService
{
    protected HRMAppService()
    {
        LocalizationResource = typeof(HRMResource);
    }
}
