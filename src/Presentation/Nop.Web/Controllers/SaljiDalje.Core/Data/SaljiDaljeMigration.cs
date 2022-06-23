using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FluentMigrator;
using Newtonsoft.Json;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Themes;

namespace SaljiDalje.Core.Data
{
    [NopMigration("2022-04-30 10:00:00", "SaljiDaljeMigration", MigrationProcessType.Update)]
    public class SaljiDaljeMigration : MigrationBase
    {
        #region Fields

        private readonly INopDataProvider _nopDataProvider;
        private readonly IRepository<CategoryTemplate> _categoryTemplateRepository;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly CustomerSettings _customerSettings;
        private readonly INopFileProvider _fileProvider;

        #endregion

        #region Ctor

        public SaljiDaljeMigration(
            INopDataProvider nopDataProvider,
            IRepository<CategoryTemplate> categoryTemplateRepository,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            CustomerSettings customerSettings,
            INopFileProvider nopFileProvider)
        {
            _nopDataProvider = nopDataProvider;
            _categoryTemplateRepository = categoryTemplateRepository;
            _languageService = languageService;
            _localizationService = localizationService;
            _settingService = settingService;
            _specificationAttributeService = specificationAttributeService;
            _customerSettings = customerSettings;
            _fileProvider = nopFileProvider;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            var storeInformationSettings = EngineContext.Current.Resolve<StoreInformationSettings>();
            storeInformationSettings.AllowCustomerToSelectTheme = true;
            
            _settingService.SaveSettingAsync(storeInformationSettings).Wait();
            
            //EngineContext.Current.Resolve<IThemeContext>().SetWorkingThemeNameAsync("SaljiDalje");
            
            _customerSettings.UsernamesEnabled = true;
            _customerSettings.FirstNameRequired = false;
            _customerSettings.LastNameRequired = false;
            _customerSettings.AllowCustomersToUploadAvatars = true;
            _customerSettings.AvatarMaximumSizeBytes = 2000000;

            _settingService.SaveSettingAsync(_customerSettings).Wait();

            InstallCategories();

            Create.TableFor<CostumerPictureAttachmentMapping>();

            Create.TableFor<ProductExtended>();

            Create.ForeignKey()
                .FromTable(nameof(ProductExtended))
                .ForeignColumn(nameof(ProductExtended.ProductId))
                .ToTable(nameof(Product)).PrimaryColumn(nameof(Product.Id)).OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(nameof(ProductExtended))
                .ForeignColumn(nameof(ProductExtended.UserId))
                .ToTable(nameof(Customer)).PrimaryColumn(nameof(Customer.Id)).OnDelete(Rule.Cascade);
        }

        private void InstallCategories()
        {
            var filePath = _fileProvider.MapPath("~/Plugins/SaljiDalje.Core/categorydata.json");
            var jsonString = _fileProvider.ReadAllText(filePath, Encoding.UTF8);
            var adsCategory = JsonConvert.DeserializeObject<AdsCategory>(jsonString);

            var prop = adsCategory.GetType().GetProperties();
            for (int i = 0; i < prop.Length; i++)
            {
                var value = prop[i].GetValue(adsCategory, null);
                var category = AddCategories((string) value
                    .GetType()
                    .GetProperty("CategoryName")
                    .GetValue(value), i);
                
                List<ChildCategory> type = (List<ChildCategory>) value.GetType().GetProperty("ChildCategory").GetValue(value);
                AddChildCategory(type,category.Id);
            }
            
            void AddChildCategory(List<ChildCategory> categories, int parentCategoryId)
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    var category = AddCategories(categories[i].CategoryName, i, parentCategoryId);
                    if (categories[i].NestedChildCategory != null)
                    {
                        AddChildCategory(categories[i].NestedChildCategory, category.Id);
                    }
                }
            }

            Category AddCategories(string name, int displayOrder, int parentCategoryId = default)
            {
                var categoryTemplateInGridAndLines = _categoryTemplateRepository
                    .Table.FirstOrDefault(pt => pt.Name == "Products in Grid or Lines");
                if (categoryTemplateInGridAndLines == null)
                    throw new Exception("Category template cannot be loaded");

                var category = new Category
                {
                    Name = name,
                    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                    ParentCategoryId = parentCategoryId,
                    PageSize = 25,
                    AllowCustomersToSelectPageSize = true,
                    PageSizeOptions = "25,50,100",
                    IncludeInTopMenu = true,
                    ShowOnHomepage = true,
                    PriceRangeFiltering = true,
                    Published = true,
                    DisplayOrder = displayOrder,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                return _nopDataProvider.InsertEntityAsync(category).Result;
            }
        }


        /// <summary>
        /// Collects the DOWN migration expressions
        /// </summary>
        public override void Down()
        {
            //nothing
        }

        #endregion
    }
}