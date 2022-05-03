using Microsoft.AspNetCore.Mvc;
using Nop.Services.Directory;

using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace SaljiDalje.Core.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class MakeNewAd : BasePluginController
    {
        #region Fields

        private readonly ICountryService _countryService;

        #endregion

        #region Ctor

        public MakeNewAd(ICountryService countryService)
        {
            _countryService = countryService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> makeNewAd()
        {
            return View("~/Plugins/SaljiDalje.Core/Views/Index.cshtml");
        }

        #endregion
    }
}
