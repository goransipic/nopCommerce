using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
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
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly INotificationService _notificationService;

        public MakeNewAdController(ICatalogModelFactory catalogModelFactory,
            ICategoryService categoryService,
            IProductService productService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            INotificationService notificationService
        )
        {
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _notificationService = notificationService;
        }

        public virtual async Task<IActionResult> StepOne()
        {
            var model = await _catalogModelFactory.PrepareTopMenuModelAsync();
            return View("~/Plugins/SaljiDalje.Core/Views/StepOneVer2.cshtml", model);
        }

        public IActionResult StepTwo(int id)
        {
            return View("~/Plugins/SaljiDalje.Core/Views/StepTwo.cshtml", new StepThreeModel {categoryId = id});
        }

        [HttpPost]
        public IActionResult StepThree(StepThreeModel stepThreeModel)
        {
            var stepThreeModelFinish = new StepThreeModelFinish {StepThreeModel = stepThreeModel};

            return View("~/Plugins/SaljiDalje.Core/Views/StepThree.cshtml", stepThreeModelFinish);
        }

        [HttpPost]
        public async Task<IActionResult> StepThreeFinal(StepThreeModelFinish stepThreeModelFinish)
        {
            var stepThreeModel = stepThreeModelFinish.StepThreeModel;
            var product = new Product()
            {
                Name = stepThreeModel.Title,
                Price = stepThreeModel.Cijena,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                Published = true,
                VisibleIndividually = true
            };
            await _productService.InsertProductAsync(product);

            //search engine name
            var SeName = await _urlRecordService.ValidateSeNameAsync(product, null, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, SeName, 0);

            //locales
            //await UpdateLocalesAsync(product, model);

            //categories
            await SaveCategoryMappingsAsync(product, stepThreeModel);
            
            await Insertpictures(stepThreeModelFinish, product);
            
            _notificationService.SuccessNotification("Uspješno dodan oglas!");

            return RedirectToRoute("Homepage");
        }

        private async Task Insertpictures(StepThreeModelFinish stepThreeModelFinish, Product product)
        {
            foreach (var base64EncodedPictures in stepThreeModelFinish.ImageFile)
            {
                if (base64EncodedPictures == null)
                {
                    return;
                }
            }

            foreach (var base64EncodedPictures in stepThreeModelFinish.ImageFile)
            {
                FilePond filePond =
                    JsonSerializer.Deserialize<FilePond>(base64EncodedPictures);
                //Console.Write(stepThreeModelFinish.StepThreeModel);
                //Console.Write(filePond.name);
                byte[] byteArray = Convert.FromBase64String(filePond.data);
                var stream = new MemoryStream(byteArray);
                IFormFile file = new FormFile(stream, 0, byteArray.Length, filePond.id, filePond.name)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = filePond.type,
                };

                var picture = await _pictureService.InsertPictureAsync(file, file.Name);
                await _productService.InsertProductPictureAsync(new ProductPicture
                {
                    PictureId = picture.Id,
                    ProductId = product.Id,
                    DisplayOrder = 1
                });
            }
            
            
            //Console.Write(picture.VirtualPath);
        }

        protected virtual async Task SaveCategoryMappingsAsync(Product product, StepThreeModel model)
        {
            var existingProductCategories =
                await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);

            //delete categories
            foreach (var existingProductCategory in existingProductCategories)
                if (model.categoryId != existingProductCategory.CategoryId)
                    await _categoryService.DeleteProductCategoryAsync(existingProductCategory);

            //add categories

            if (_categoryService.FindProductCategory(existingProductCategories, product.Id, model.categoryId) == null)
            {
                //find next display order
                var displayOrder = 1;
                var existingCategoryMapping =
                    await _categoryService.GetProductCategoriesByCategoryIdAsync(model.categoryId, showHidden: true);
                if (existingCategoryMapping.Any())
                    displayOrder = existingCategoryMapping.Max(x => x.DisplayOrder) + 1;
                await _categoryService.InsertProductCategoryAsync(new ProductCategory
                {
                    ProductId = product.Id, CategoryId = model.categoryId, DisplayOrder = displayOrder
                });
            }
        }

        public async Task<IActionResult> ChildrenCategories(int id)
        {
            var model = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(id);
            if (model.Count == 0)
            {
                return NoContent();
            }

            return PartialView("~/Plugins/SaljiDalje.Core/Views/Shared/_ChildrenCategories.cshtml",
                (model: model, id: id));
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
        public int categoryId { get; set; }
        [Required] public string Title { get; set; }
        [Required] public Stanje Stanje { get; set; }

        public Boolean MogucaDostava { get; set; }

        public Boolean RazgledavanjePutemPoziva { get; set; }

        [Required] public Valuta Valuta { get; set; }

        [Required] public int Cijena { get; set; }

        [Display(Name = "Pregovaranje za cijenu")]
        public Boolean NegotatiedPrice { get; set; }

        // Mogućnost plaćanja
        [Display(Name = "Gotovina")] public Boolean Gotovina { get; set; }
        [Display(Name = "Kredit")] public Boolean Kredit { get; set; }
        [Display(Name = "Leasing")] public Boolean Leasing { get; set; }

        [Display(Name = "Obročno Bankovnim Karticama")]
        public Boolean ObrocnoBankovnimKarticama { get; set; }

        [Display(Name = "Preuzimanje Leasinga")]
        public Boolean PreuzimanjeLeasinga { get; set; }

        [Display(Name = "Staro za Novo")] public Boolean StaroZaNovo { get; set; }
        [Display(Name = "Zamjena")] public Boolean Zamjena { get; set; }
    }

    public record StepThreeModelFinish
    {
        [DisplayName("Upload File")] public string[] ImageFile { get; set; }
        public StepThreeModel StepThreeModel { get; set; }
    }

    public enum Stanje
    {
        [Display(Name = "Novo")] NEW,
        [Display(Name = "Korišteno")] USED,
        [Display(Name = "Oštećeno")] DAMAGED
    }

    public enum Valuta
    {
        [Display(Name = "$")] USD,
        [Display(Name = "€")] EURO,
    }

    public class Center
    {
        public double x { get; set; }
        public double y { get; set; }
    }

    public class Crop
    {
        public Center center { get; set; }
        public Flip flip { get; set; }
        public int rotation { get; set; }
        public int zoom { get; set; }
        public object aspectRatio { get; set; }
    }

    public class Flip
    {
        public bool horizontal { get; set; }
        public bool vertical { get; set; }
    }

    public class Metadata
    {
        public Crop crop { get; set; }
        public Output output { get; set; }
    }

    public class Output
    {
        public object type { get; set; }
        public object quality { get; set; }
        public List<string> client { get; set; }
    }

    public class FilePond
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int size { get; set; }
        public Metadata metadata { get; set; }
        public string data { get; set; }
    }
}