using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Web.Controllers.SaljiDalje.Core;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Customer;
using SaljiDalje.Core.Data;

namespace SaljiDalje.Core.Controllers;

public class CustomerInfoController : BasePluginController
{
    public IRepository<ProductPicture> ProductPictureRepository { get; }
    private readonly IWorkContext _workContext;
    private readonly ICustomerService _customerService;
    private readonly CustomerSettings _customerSettings;
    private readonly IPictureService _pictureService;
    private readonly ILocalizationService _localizationService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IDownloadService _downloadService;
    private readonly MediaSettings _mediaSettings;
    private readonly ICustomerModelFactory _customerModelFactory;
    private readonly IRepository<ProductExtended> _productExtendedRepository;
    private readonly IRepository<Product> _productRepository;

    public CustomerInfoController(
        IWorkContext workContext,
        ICustomerService customerService,
        CustomerSettings customerSettings,
        IPictureService pictureService,
        ILocalizationService localizationService,
        IGenericAttributeService genericAttributeService,
        IDownloadService downloadService,
        MediaSettings mediaSettings,
        ICustomerModelFactory customerModelFactory,
        IRepository<ProductExtended> productExtendedRepository,
        IRepository<Product> productRepository,
        IRepository<ProductPicture> productPictureRepository)
    {
        ProductPictureRepository = productPictureRepository;
        _workContext = workContext;
        _customerService = customerService;
        _customerSettings = customerSettings;
        _pictureService = pictureService;
        _localizationService = localizationService;
        _genericAttributeService = genericAttributeService;
        _downloadService = downloadService;
        _mediaSettings = mediaSettings;
        _customerModelFactory = customerModelFactory;
        _productExtendedRepository = productExtendedRepository;
        _productRepository = productRepository;
    }

    public virtual IActionResult Notification()
    {
        return View("~/Plugins/SaljiDalje.Core/Views/Notification.cshtml");
    }

    [HttpGet]
    public virtual async Task<IActionResult> CustomerAds(CurrentActiveTab currentActiveTab)
    {
        var currentuserID = await _workContext.GetCurrentCustomerAsync();
        var costumerProducts = new CostumerAdModel {CurrentActiveTab = currentActiveTab};
        
        var numberofAll = await (from c in _productRepository.Table
            join p in _productExtendedRepository.Table on c.Id equals p.ProductId
            join pc in ProductPictureRepository.Table on c.Id equals pc.ProductId
            where p.UserId == currentuserID.Id && !c.Deleted
            select p).CountAsync();

        costumerProducts.TotalNumber = numberofAll;
        
        switch (currentActiveTab)
        {
            case CurrentActiveTab.ACTIVE:
                var query = await (from c in _productRepository.Table
                    join p in _productExtendedRepository.Table on c.Id equals p.ProductId
                    join pc in ProductPictureRepository.Table on c.Id equals pc.ProductId
                    where p.UserId == currentuserID.Id && !c.Deleted && c.Published
                    select new {Product = c, ProductPicture = pc}).ToListAsync();

                foreach (var adModel in query)
                {
                    var pictureUrl = await _pictureService.GetPictureUrlAsync(adModel.ProductPicture.PictureId);
                    costumerProducts.ProductInformation.Add(new ProductInformation
                    {
                        Product = adModel.Product, ProductPicture = pictureUrl
                    });
                }

                break;
            case CurrentActiveTab.PASSIVE:
                var queryPassive = await (from c in _productRepository.Table
                    join p in _productExtendedRepository.Table on c.Id equals p.ProductId
                    join pc in ProductPictureRepository.Table on c.Id equals pc.ProductId
                    where p.UserId == currentuserID.Id && !c.Deleted && !c.Published
                    select new {Product = c, ProductPicture = pc}).ToListAsync();
                foreach (var adModel in queryPassive)
                {
                    var pictureUrl = await _pictureService.GetPictureUrlAsync(adModel.ProductPicture.PictureId);
                    costumerProducts.ProductInformation.Add(new ProductInformation
                    {
                        Product = adModel.Product, ProductPicture = pictureUrl
                    });
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentActiveTab), currentActiveTab, null);
        }


        return View("~/Plugins/SaljiDalje.Core/Views/CustomerAds.cshtml", costumerProducts);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CustomerAdsActions(CustomerAdModel customerAdCommand)
    {
        var currentuserID = await _workContext.GetCurrentCustomerAsync();

        switch (customerAdCommand.CommmandToExecute)
        {
            case CommmandToExecute.MAKE_ACTIVE:
                var queryMakeActive =
                    await (from c in _productRepository.Table
                        join p in _productExtendedRepository.Table on c.Id equals p.ProductId
                        where p.UserId == currentuserID.Id && c.Id == customerAdCommand.ProductId
                        select c).FirstOrDefaultAsync();

                queryMakeActive.Published = true;
                await _productRepository.UpdateAsync(queryMakeActive);
                break;
            case CommmandToExecute.MAKE_PASSIVE:
                var queryMakePassive =
                    await (from c in _productRepository.Table
                        join p in _productExtendedRepository.Table on c.Id equals p.ProductId
                        where p.UserId == currentuserID.Id && c.Id == customerAdCommand.ProductId
                        select c).FirstOrDefaultAsync();

                queryMakePassive.Published = false;
                await _productRepository.UpdateAsync(queryMakePassive);

                break;
            case CommmandToExecute.MAKE_EDIT:
                break;
            case CommmandToExecute.MAKE_DELETE:
                var queryMakeDelete =
                    await (from c in _productRepository.Table
                        join p in _productExtendedRepository.Table on c.Id equals p.ProductId
                        where p.UserId == currentuserID.Id && c.Id == customerAdCommand.ProductId
                        select c).FirstOrDefaultAsync();
                queryMakeDelete.Deleted = true;
                await _productRepository.UpdateAsync(queryMakeDelete);
                break;
            case CommmandToExecute.DELETE_ALL:
                var query =
                    await (from c in _productRepository.Table
                        join p in _productExtendedRepository.Table on c.Id equals p.ProductId
                        where p.UserId == currentuserID.Id
                        select c).ToListAsync();
                await _productRepository.DeleteAsync(query, true);
                break;
            default:
                return RedirectToAction("CustomerAds");
        }

        return Ok();
    }

    [HttpPost, ActionName("Avatar")]
    [FormValueRequired("upload-avatar")]
    public virtual async Task<IActionResult> UploadAvatar(CustomerAvatarModel model, IFormCollection form)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        if (!_customerSettings.AllowCustomersToUploadAvatars)
            return RedirectToRoute("CustomerInfo");

        IFormFile uploadedFile = null;

        if (!StringValues.IsNullOrEmpty(form["uploadedFile"]))
        {
            FilePond filePond = JsonSerializer.Deserialize<FilePond>(form["uploadedFile"]);
            //Console.Write(stepThreeModelFinish.StepThreeModel);
            //Console.Write(filePond.name);
            byte[] byteArray = Convert.FromBase64String(filePond.data);
            var stream = new MemoryStream(byteArray);
            uploadedFile = new FormFile(stream, 0, byteArray.Length, filePond.id, filePond.name)
            {
                Headers = new HeaderDictionary(), ContentType = filePond.type,
            };
        }

        var contentType = uploadedFile.ContentType.ToLowerInvariant();

        if (!contentType.Equals("image/jpeg") && !contentType.Equals("image/gif"))
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Avatar.UploadRules"));

        if (ModelState.IsValid)
        {
            try
            {
                var customerAvatar = await _pictureService.GetPictureByIdAsync(
                    await _genericAttributeService.GetAttributeAsync<int>(customer,
                        NopCustomerDefaults.AvatarPictureIdAttribute));
                if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
                {
                    var avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                    if (uploadedFile.Length > avatarMaxSize)
                        throw new NopException(string.Format(
                            await _localizationService.GetResourceAsync("Account.Avatar.MaximumUploadedFileSize"),
                            avatarMaxSize));

                    var customerPictureBinary = await _downloadService.GetDownloadBitsAsync(uploadedFile);
                    if (customerAvatar != null)
                        customerAvatar = await _pictureService.UpdatePictureAsync(customerAvatar.Id,
                            customerPictureBinary, contentType, null);
                    else
                        customerAvatar =
                            await _pictureService.InsertPictureAsync(customerPictureBinary, contentType, null);
                }

                var customerAvatarId = 0;
                if (customerAvatar != null)
                    customerAvatarId = customerAvatar.Id;

                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.AvatarPictureIdAttribute, customerAvatarId);

                model.AvatarUrl = await _pictureService.GetPictureUrlAsync(
                    await _genericAttributeService.GetAttributeAsync<int>(customer,
                        NopCustomerDefaults.AvatarPictureIdAttribute),
                    _mediaSettings.AvatarPictureSize,
                    false);

                return View("~/Themes/SaljiDalje/Views/Customer/Avatar.cshtml", model);
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }
        }

        //If we got this far, something failed, redisplay form
        model = await _customerModelFactory.PrepareCustomerAvatarModelAsync(model);
        return View("~/Themes/SaljiDalje/Views/Customer/Avatar.cshtml", model);
    }
}

public record CostumerAdModel
{
    public List<ProductInformation> ProductInformation { get; set; } = new();
    public CurrentActiveTab CurrentActiveTab { get; set; }
    
    public int TotalNumber { get; set; }
}

public record ProductInformation
{
    public Product Product { get; set; }
    public string ProductPicture { get; set; }
}