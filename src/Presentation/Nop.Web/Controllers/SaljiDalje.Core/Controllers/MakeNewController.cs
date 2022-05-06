using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Catalog;

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
        
        public async Task<IActionResult> PlaceAd()
        {
            var model = await _catalogModelFactory.PrepareTopMenuModelAsync();
            return View("~/Plugins/SaljiDalje.Core/Views/PlaceAd.cshtml", model);
        }
    }
    
    public class SortByCategoryModel
    {
        public int Level { get; set; }
        public bool ResponsiveMobileMenu { get; set; }
        public CategorySimpleModel Category { get; set; }
        public CategorySimpleModel ParentCategory { get; set; }
    }
}