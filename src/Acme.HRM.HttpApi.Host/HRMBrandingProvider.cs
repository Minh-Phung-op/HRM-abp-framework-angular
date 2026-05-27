using Microsoft.Extensions.Localization;
using Acme.HRM.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Acme.HRM;

[Dependency(ReplaceServices = true)]
public class HRMBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<HRMResource> _localizer;

    public HRMBrandingProvider(IStringLocalizer<HRMResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
