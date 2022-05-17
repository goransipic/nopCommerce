using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Nop.Services.Catalog;

using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Catalog;

namespace SaljiDalje.Core.Controllers
{
    public partial class MakeNewAdController : BasePluginController
    {
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ILogger _logger;

        public MakeNewAdController(ICatalogModelFactory catalogModelFactory, 
            ICategoryService categoryService
            )
        {
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            
        }

        public virtual async Task<IActionResult> StepOne()
        {
            var model = await _catalogModelFactory.PrepareTopMenuModelAsync();
            return View("~/Plugins/SaljiDalje.Core/Views/StepOneVer2.cshtml", model);
        }

        public IActionResult StepTwo(int id)
        {
            return View("~/Plugins/SaljiDalje.Core/Views/StepTwo.cshtml");
        }
        
        [HttpPost]
        public async Task<IActionResult> StepThree(StepThreeModel stepThreeModel)
        {
            //if (Model)
            Console.Write(stepThreeModel);
            var model = await _catalogModelFactory.PrepareTopMenuModelAsync();
            return View("~/Plugins/SaljiDalje.Core/Views/StepThree.cshtml", model);
        }
        
        public async Task<IActionResult> ChildrenCategories(int id)
        {
            var model = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(id);
            if (model.Count == 0)
            {
                return NoContent();
            }

            return PartialView("~/Plugins/SaljiDalje.Core/Views/Shared/_ChildrenCategories.cshtml", (model: model, id: id));
        }
    }
    
    public class SortByCategoryModel
    {
        public int Level { get; set; }
        public bool ResponsiveMobileMenu { get; set; }
        public CategorySimpleModel Category { get; set; }
        public CategorySimpleModel ParentCategory { get; set; }
    }
    
    public record StepThreeModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public Stanje Stanje { get; set; }
        
        public Boolean MogucaDostava { get; set; }
        
        public Boolean RazgledavanjePutemPoziva { get; set; }
        
        [Required]
        public Valuta Valuta { get; set; }
        
        [Required]
        public int Cijena { get; set; }
        [Display(Name = "Pregovaranje za cijenu")]
        public Boolean NegotatiedPrice { get; set; }
        // Mogućnost plaćanja
        [Display(Name = "Gotovina")]
        public Boolean Gotovina { get; set; }
        [Display(Name = "Kredit")]
        public Boolean Kredit { get; set; }
        [Display(Name = "Leasing")]
        public Boolean Leasing { get; set; }
        [Display(Name = "Obročno Bankovnim Karticama")]
        public Boolean ObrocnoBankovnimKarticama { get; set; }
        [Display(Name = "Preuzimanje Leasinga")]
        public Boolean PreuzimanjeLeasinga { get; set; }
        [Display(Name = "Staro za Novo")]
        public Boolean StaroZaNovo { get; set; }
        [Display(Name = "Zamjena")]
        public Boolean Zamjena { get; set; }
        
    }

    public enum Stanje
    {
        [Display(Name = "Novo")]
        NEW,
        [Display(Name = "Korišteno")]
        USED,
        [Display(Name = "Oštećeno")]
        DAMAGED
    }
    
    public enum Valuta
    {
        [Display(Name = "$")]
        USD,
        [Display(Name = "€")]
        EURO,
    }
}