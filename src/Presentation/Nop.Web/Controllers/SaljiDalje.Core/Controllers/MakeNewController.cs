using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;

namespace SaljiDalje.Core.Controllers
{
    public partial class MakeNewAdController : BasePluginController
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        public MakeNewAdController(ICatalogModelFactory catalogModelFactory)
        {
            _catalogModelFactory = catalogModelFactory;
        }

        public virtual async Task<IActionResult> Index()
        {
            var model = await _catalogModelFactory.PrepareTopMenuModelAsync();
            return View("~/Plugins/SaljiDalje.Core/Views/Index.cshtml", model);
        }
    }
}