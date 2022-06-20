using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace SaljiDalje.Core.Data
{
    [NopMigration("2022-04-29 00:00:00", "SaljiDaljeMigration", MigrationProcessType.Update)]
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

        #endregion

        #region Ctor

        public SaljiDaljeMigration(
            INopDataProvider nopDataProvider,
            IRepository<CategoryTemplate> categoryTemplateRepository,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            CustomerSettings customerSettings)
        {
            _nopDataProvider = nopDataProvider;
            _categoryTemplateRepository = categoryTemplateRepository;
            _languageService = languageService;
            _localizationService = localizationService;
            _settingService = settingService;
            _specificationAttributeService = specificationAttributeService;
            _customerSettings = customerSettings;
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
            var rootCategories = new List<string>
            {
                "Auto Moto",
                "Nekretnine",
                "Nautika",
                "Hrana i piće",
                "Popusti i katalozi",
                "Turizam",
                "Usluge",
                "Sve za dom",
                "Kućni ljubimci",
                "Informatika",
                "Mobiteli",
                "Audio Video Foto",
                "Glazbala",
                "Literatura",
                "Sport i oprema",
                "Pronađeno blago",
                "Dječji svijet",
                "Strojevi i alati",
                "Od glave do pete",
                "Posao",
                "Ostalo"
            };

            for (var i = 0; i < rootCategories.Count; i++)
            {
                var item = rootCategories[i];
                var category = AddCategories(item, i);
                
                
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