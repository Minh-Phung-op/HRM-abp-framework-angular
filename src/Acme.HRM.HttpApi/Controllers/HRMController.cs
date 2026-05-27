using Acme.HRM.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Acme.HRM.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class HRMController : AbpControllerBase
{
    protected HRMController()
    { 
        LocalizationResource = typeof(HRMResource);
    }
}
