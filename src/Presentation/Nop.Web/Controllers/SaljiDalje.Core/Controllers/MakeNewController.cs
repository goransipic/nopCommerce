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

        public virtual async Task<IActionResult> StepOne()
        {
            var model = await _catalogModelFactory.PrepareTopMenuModelAsync();
            return View("~/Plugins/SaljiDalje.Core/Views/StepOneVer2.cshtml", model);
        }

        public IActionResult StepTwo()
        {
            return View("~/Plugins/SaljiDalje.Core/Views/StepTwo.cshtml");
        }
        
        public async Task<IActionResult> StepThree()
        {
            var model = await _catalogModelFactory.PrepareTopMenuModelAsync();
            return View("~/Plugins/SaljiDalje.Core/Views/StepThree.cshtml", model);
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