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

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Ctor

        public SaljiDaljeMigration(
            ILanguageService languageService,
            ILocalizationService localizationService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            CustomerSettings customerSettings)
        {
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
            _customerSettings.AllowCustomersToUploadAvatars = true;
            _customerSettings.AvatarMaximumSizeBytes = 2000000;
            
            _settingService.SaveSettingAsync(_customerSettings).Wait();
            
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