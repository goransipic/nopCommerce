using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;
using SaljiDalje.Core.Data;

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
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IRepository<ProductExtended> _productExtendedRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<CostumerPictureAttachmentMapping> _costumerPictureAttachmentMappingRepository;
        private readonly IRepository<SpecificationAttributeGroup> _specificationAttributeGroupRepository;
        private readonly IWorkContext _workContext;

        public MakeNewAdController(ICatalogModelFactory catalogModelFactory,
            ICategoryService categoryService,
            IProductService productService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            INotificationService notificationService,
            ISpecificationAttributeService _specificationAttributeService,
            IRepository<ProductExtended> productExtendedRepository,
            IRepository<Product> productRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<CostumerPictureAttachmentMapping> costumerPictureAttachmentMappingRepository,
            IWorkContext workContext)
        {
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _notificationService = notificationService;
            this._specificationAttributeService = _specificationAttributeService;
            _productExtendedRepository = productExtendedRepository;
            _productRepository = productRepository;
            _specificationAttributeRepository = specificationAttributeRepository;
            _costumerPictureAttachmentMappingRepository = costumerPictureAttachmentMappingRepository;
            _workContext = workContext;
        }

        public virtual async Task<IActionResult> StepOne()
        {
            var model = await _catalogModelFactory.PrepareTopMenuModelAsync();
            return View("~/Plugins/SaljiDalje.Core/Views/StepOneVer2.cshtml", model);
        }

        public async Task<IActionResult> StepTwo(bool isEditMode, int productId, int? categoryId = null)
        {
            Product productToEdit = null;
            StepTwoModel stepTwoModel = null;

            Dictionary<string, IList<SpecificationAttributeOption>> SpecificationAttributeOptions = new();

            var specificationAttributeGroup = (await _specificationAttributeService
                    .GetSpecificationAttributeGroupsAsync())
                .First(item => item.Name == "BasicAdsAttributes");

            var specificationAttributeBasicAds = await _specificationAttributeService
                .GetSpecificationAttributesByGroupIdAsync(specificationAttributeGroup.Id);

            var specificationAttributeState = specificationAttributeBasicAds
                .Single(item => item.Name == "Stanje");

            var specificationAttributeNegotiateForPrice = specificationAttributeBasicAds
                .Single(item => item.Name == "Pregovaranje za cijenu");

            var specificationAttributeZupanija = specificationAttributeBasicAds
                .Single(item => item.Name == "Županija");

            var specificationAttributeDostava = specificationAttributeBasicAds
                .Single(item => item.Name == "Dostava");

            var specificationAttributeMogućnostPlaćanja = specificationAttributeBasicAds
                .Single(item => item.Name == "Mogućnost plaćanja");

            var Dostava = (await _specificationAttributeService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(
                    specificationAttributeDostava.Id));

            SpecificationAttributeOptions["Dostava"] = Dostava;


            var MogućnostPlaćanja = (await _specificationAttributeService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(
                    specificationAttributeMogućnostPlaćanja.Id));

            SpecificationAttributeOptions["MogućnostPlaćanja"] = MogućnostPlaćanja;


            var specificationAttributeNegotiateForPriceId = (await _specificationAttributeService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(
                    specificationAttributeNegotiateForPrice.Id)).First();

            SpecificationAttributeOptions["PregovaranjeZaCijenu"] = new List<SpecificationAttributeOption>
            {
                specificationAttributeNegotiateForPriceId
            };


            var StanjeOption = await _specificationAttributeService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(
                    specificationAttributeState.Id);

            SpecificationAttributeOptions["StanjeOption"] = StanjeOption;

            var item = (StanjeOption)
                .Select(option => new SelectListItem {Text = option.Name, Value = option.Id.ToString()})
                .ToList();

            var itemZupanijeOption = await _specificationAttributeService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(
                    specificationAttributeZupanija.Id);

            SpecificationAttributeOptions["Županije"] = itemZupanijeOption;


            var itemZupanije = (itemZupanijeOption)
                .Select(option => new SelectListItem {Text = option.Name, Value = option.Id.ToString()})
                .ToList();

            if (isEditMode)
            {
                var itemsOfValue = item.Select(itemA => int.Parse(itemA.Value));
                var itemsOfValueZupanije = itemZupanije.Select(itemA => int.Parse(itemA.Value));

                var currentUser = await _workContext.GetCurrentCustomerAsync();

                var productSpecification = await _specificationAttributeService
                    .GetProductSpecificationAttributesAsync(productId);

                var productSpecificationOption = productSpecification.Single(itemSpecification =>
                    itemsOfValue.Any(itemB => itemSpecification.SpecificationAttributeOptionId == itemB));

                var productSpecificationOptionZupanije = productSpecification.FirstOrDefault(itemSpecification =>
                    itemsOfValueZupanije.Any(itemB => itemSpecification.SpecificationAttributeOptionId == itemB));

                var productSpecificationPrice = productSpecification
                    .FirstOrDefault(item => item.SpecificationAttributeOptionId
                                            == specificationAttributeNegotiateForPrice.Id);

                categoryId = (await _categoryService
                    .GetProductCategoriesByProductIdAsync(productId))[0].CategoryId;

                productToEdit =
                    await (from p in _productRepository.Table
                        join pe in _productExtendedRepository.Table on p.Id equals pe.ProductId
                        where pe.UserId == currentUser.Id && p.Id == productId
                        select p).SingleAsync();

                if (productToEdit == null)
                {
                    return RedirectToRoute("Homepage");
                }

                stepTwoModel = new StepTwoModel
                {
                    Title = productToEdit.Name,
                    Cijena = (int)productToEdit.Price,
                    categoryId = (int)categoryId,
                    Dostava = Dostava,
                    MogućnostPlaćanja = MogućnostPlaćanja,
                    OdabaranaZupanija = productSpecificationOptionZupanije?.SpecificationAttributeOptionId,
                    SpecificationAttributeOptionIdStateOptionId =
                        productSpecificationOption.SpecificationAttributeOptionId,
                    NegotatiedPrice = specificationAttributeNegotiateForPriceId != null ? true : false,
                    Stanje = item,
                    Zupanije = itemZupanije,
                    GenericOptionsList = new List<GenericItem> {new() {item = "67"}, new() {item = "67"}},
                    SpecificationAttributeOptions = SpecificationAttributeOptions,
                };
            }
            else
            {
                if (categoryId == null)
                {
                    return RedirectToRoute("Homepage");
                }
                else
                {
                    stepTwoModel = new StepTwoModel
                    {
                        Stanje = item,
                        Dostava = Dostava,
                        MogućnostPlaćanja = MogućnostPlaćanja,
                        Zupanije = itemZupanije,
                        SpecificationAttributeOptions = SpecificationAttributeOptions,
                    };
                }
            }

            return View("~/Plugins/SaljiDalje.Core/Views/StepTwoVer2.cshtml",
                stepTwoModel);
        }

        [HttpPost]
        public async Task<IActionResult> StepTwoFinish(StepTwoModel stepTwoModel)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var product = new Product
                {
                    Name = stepTwoModel.Title,
                    Price = stepTwoModel.Cijena ?? 0,
                    FullDescription = stepTwoModel.Description,
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
                await SaveCategoryMappingsAsync(product, stepTwoModel);

                await Insertpictures(stepTwoModel, product);

                //var psa = model.ToEntity<ProductSpecificationAttribute>();
                //psa.CustomValue = model.ValueRaw;

                //var sawp = await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync();
                //sawp.First(item => item.Id == 1 )
                async Task InsertProductAttribute(string s)
                {
                    int numericValue;
                    if (int.TryParse(s, out numericValue))
                        await _specificationAttributeService.InsertProductSpecificationAttributeAsync(
                            new ProductSpecificationAttribute()
                            {
                                ProductId = product.Id,
                                AttributeType = SpecificationAttributeType.Option,
                                AllowFiltering = true,
                                ShowOnProductPage = true,
                                SpecificationAttributeOptionId = numericValue
                            });
                }

                foreach (var mogućaDostavaItem in stepTwoModel.MogucaDostavaList)
                {
                    await InsertProductAttribute(mogućaDostavaItem);
                }

                foreach (var mogućnostPlaćanjaItem in stepTwoModel.MogućnostPlaćanjaList)
                {
                    await InsertProductAttribute(mogućnostPlaćanjaItem);
                }

                var psa = new ProductSpecificationAttribute
                {
                    ProductId = product.Id,
                    AttributeType = SpecificationAttributeType.Option,
                    AllowFiltering = true,
                    ShowOnProductPage = true,
                    SpecificationAttributeOptionId = stepTwoModel.SpecificationAttributeOptionIdStateOptionId
                };
                var psaZupanija = new ProductSpecificationAttribute
                {
                    ProductId = product.Id,
                    AttributeType = SpecificationAttributeType.Option,
                    AllowFiltering = true,
                    ShowOnProductPage = true,
                    SpecificationAttributeOptionId = (int)stepTwoModel.OdabaranaZupanija
                };

                await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psa);
                await _specificationAttributeService
                    .InsertProductSpecificationAttributeAsync(
                        psaZupanija); //_specificationAttributeService.GetProductSpecificationAttributesAsync(1);

                //await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(attributeId));

                await _productExtendedRepository.InsertAsync(new ProductExtended
                {
                    ProductId = product.Id, UserId = (await _workContext.GetCurrentCustomerAsync()).Id
                });

                _notificationService.SuccessNotification("Uspješno dodan oglas!");

                transaction.Complete();
                transaction.Dispose();

                return RedirectToRoute("Homepage");
            }
            catch (Exception exception)
            {
                transaction.Dispose();
                throw;
            }
        }

        private async Task Insertpictures(StepTwoModel stepTwoModel, Product product)
        {
            foreach (var base64EncodedPictures in stepTwoModel.ImageFile)
            {
                if (base64EncodedPictures == null)
                {
                    return;
                }
            }

            foreach (var base64EncodedPictures in stepTwoModel.ImageFile)
            {
                FilePond filePond =
                    JsonSerializer.Deserialize<FilePond>(base64EncodedPictures);
                //Console.Write(stepThreeModelFinish.StepThreeModel);
                //Console.Write(filePond.name);
                byte[] byteArray = Convert.FromBase64String(filePond.data);
                var stream = new MemoryStream(byteArray);
                IFormFile file = new FormFile(stream, 0, byteArray.Length, filePond.id, filePond.name)
                {
                    Headers = new HeaderDictionary(), ContentType = filePond.type,
                };

                var picture = await _pictureService.InsertPictureAsync(file, file.Name);
                await _productService.InsertProductPictureAsync(new ProductPicture
                {
                    PictureId = picture.Id, ProductId = product.Id, DisplayOrder = 1
                });
            }


            //Console.Write(picture.VirtualPath);
        }

        protected virtual async Task SaveCategoryMappingsAsync(Product product, StepTwoModel model)
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

        [HttpPost]
        public async Task<IActionResult> Process(IFormFile ImageFile, CancellationToken cancellationToken)
        {
            if (ImageFile is null)
            {
                return BadRequest("Process Error: No file submitted");
            }

            // We do some internal application validation here with our caseId

            try
            {
                // get a guid to use as the filename as they're highly unique
                var guid = Guid.NewGuid().ToString();
                //var newimage = string.Format("{0}.{1}", guid, file.FileName.Split('.').LastOrDefault());
                
                using var newMemoryStream = new MemoryStream();
                await ImageFile.CopyToAsync(newMemoryStream, cancellationToken);
                
                await _costumerPictureAttachmentMappingRepository.InsertAsync(new CostumerPictureAttachmentMapping
                {
                    FileName = ImageFile.FileName,
                    FileType = ImageFile.ContentType,
                    FileSize = ImageFile.Length,
                    CreatedOn = DateTime.Now,
                    UserId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    PictureData = newMemoryStream.ToArray(),
                    Guid = guid
                });
                
                return Ok(guid);
            }
            catch (Exception e)
            {
                return BadRequest($"Process Error: {e.Message}"); // Oops!
            }
        }
        [HttpDelete]
        public async Task<ActionResult> Revert()
        {
            // The server id will be send in the delete request body as plain text
            using StreamReader reader = new(Request.Body, Encoding.UTF8);
            string guid = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(guid))
            {
                return BadRequest("Revert Error: Invalid unique file ID");
            }
            var attachment = await _costumerPictureAttachmentMappingRepository.Table.FirstOrDefaultAsync(i => i.Guid == guid);
            // We do some internal application validation here
            try
            {
                await _costumerPictureAttachmentMappingRepository.DeleteAsync(attachment);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(string.Format("Revert Error:'{0}' when writing an object", e.Message));
            }
        }
        
        [HttpDelete]
        public async Task<ActionResult> Remove()
        {
            // The server id will be send in the delete request body as plain text
            using StreamReader reader = new(Request.Body, Encoding.UTF8);
            string guid = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(guid))
            {
                return BadRequest("Revert Error: Invalid unique file ID");
            }
            var attachment = await _costumerPictureAttachmentMappingRepository.Table.FirstOrDefaultAsync(i => i.Guid == guid);
            // We do some internal application validation here
            try
            {
                await _costumerPictureAttachmentMappingRepository.DeleteAsync(attachment);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(string.Format("Revert Error:'{0}' when writing an object", e.Message));
            }
        }
        
        [HttpGet("Load/{id}")]
        public async Task<IActionResult> Load(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Load Error: Invalid parameters");
            }
            var attachment = await _costumerPictureAttachmentMappingRepository.Table
                .SingleOrDefaultAsync(i => i.Guid.Equals(id));
            if (attachment is null)
            {
                return NotFound("Load Error: File not found");
            }

            Response.Headers.Add("Content-Disposition", new ContentDisposition
            {
                FileName = string.Format("{0}.{1}", attachment.FileName, attachment.FileType),
                Inline = true // false = prompt the user for downloading; true = browser to try to show the file inline
            }.ToString());
            return File(attachment.PictureData, attachment.FileType);
        }
    }
    public class SortByCategoryModel
    {
        public int Level { get; set; }
        public bool ResponsiveMobileMenu { get; set; }
        public CategorySimpleModel Category { get; set; }
        public CategorySimpleModel ParentCategory { get; set; }
    }

    public record StepTwoModel
    {
        public Dictionary<string, IList<SpecificationAttributeOption>> SpecificationAttributeOptions = new();
        public int categoryId { get; set; }
        [Required] public string Title { get; set; }
        [Required] public IList<SelectListItem> Stanje { get; set; }

        [DisplayName("Upload File")] public string[] ImageFile { get; set; }

        [Required] public IList<GenericItem> GenericOptionsList { get; set; }


        [Required] public IList<SpecificationAttributeOption> Dostava { get; set; }

        public IList<SpecificationAttributeOption> MogućnostPlaćanja { get; set; }

        [Required] public IList<SelectListItem> Zupanije { get; set; }
        [Required] public int? OdabaranaZupanija { get; set; }

        [Required] public string Description { get; set; }

        public int SpecificationAttributeOptionIdStateOptionId { get; set; }
        public string MogucaDostava { get; set; }
        [Required] public IList<string> MogucaDostavaList { get; set; }
        [Required] public IList<string> MogućnostPlaćanjaList { get; set; }

        public string RazgledavanjePutemPoziva { get; set; }

        [Required] public Valuta Valuta { get; set; }

        public int? Cijena { get; set; }

        [Display(Name = "Pregovaranje za cijenu")]
        public Boolean NegotatiedPrice { get; set; }

        // Mogućnost plaćanja
        [Display(Name = "Gotovina")] public string Gotovina { get; set; }
        [Display(Name = "Kredit")] public string Kredit { get; set; }
        [Display(Name = "Leasing")] public string Leasing { get; set; }

        [Display(Name = "Obročno Bankovnim Karticama")]
        public string ObrocnoBankovnimKarticama { get; set; }

        [Display(Name = "Preuzimanje Leasinga")]
        public string PreuzimanjeLeasinga { get; set; }

        [Display(Name = "Staro za Novo")] public string StaroZaNovo { get; set; }
        [Display(Name = "Zamjena")] public string Zamjena { get; set; }
    }

    public record GenericItem
    {
        [Required] public string item { get; set; }
        public string itemBool { get; set; }
    }

    public record StepThreeModelFinish
    {
        [DisplayName("Upload File")] public string[] ImageFile { get; set; }
        public StepTwoModel StepTwoModel { get; set; }
    }

    public enum Stanje
    {
        [Display(Name = "Novo")] NEW,
        [Display(Name = "Korišteno")] USED,
        [Display(Name = "Oštećeno")] DAMAGED
    }

    public enum Valuta
    {
        // [Display(Name = "$")] USD,
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