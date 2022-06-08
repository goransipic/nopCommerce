using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Transactions;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using LinqToDB;
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
    [NopMigration("2022-06-06 00:00:01", "AddLocationMigration", MigrationProcessType.Update)]
    public class AddLocationsMigration : MigrationBase
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly INopDataProvider _dataProvider;

        #endregion

        #region Ctor

        public AddLocationsMigration(
            ILanguageService languageService,
            ILocalizationService localizationService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            INopDataProvider dataProvider)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _settingService = settingService;
            _specificationAttributeService = specificationAttributeService;
            _dataProvider = dataProvider;
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

            using TransactionScope transactionScope = new TransactionScope();
            try
            {
                var SpecificationAttributeGroupId = _dataProvider.InsertEntity(
                    new SpecificationAttributeGroup {Name = "BasicAdsAttributes"}
                ).Id;

                // Stanje
                var stanje = insertSpecificationAttribute("Stanje", SpecificationAttributeGroupId);
                insertSpecificationAttributeOption("Novo", stanje);
                insertSpecificationAttributeOption("Korišteno", stanje);
                insertSpecificationAttributeOption("Oštećeno", stanje);

                // Pregovaranje za cijenu
                var pregovaranjeZaCijenu =
                    insertSpecificationAttribute("Pregovaranje za cijenu", SpecificationAttributeGroupId);
                insertSpecificationAttributeOption("True", pregovaranjeZaCijenu);

                // Boja
                var boja = insertSpecificationAttribute("Boja", SpecificationAttributeGroupId);
                insertSpecificationAttributeOptionColor("Siva", "#8a97a8", boja);
                insertSpecificationAttributeOptionColor("Crvena", "#8a374a", boja);
                insertSpecificationAttributeOptionColor("Plava", "#47476f", boja);
                // Locations
                var zupanija = insertSpecificationAttribute("Županija", SpecificationAttributeGroupId);
                //var gradoviBjelovarskoBilogorska = insertSpecificationAttribute("Bjelovarsko_bilogorska_grad",SpecificationAttributeGroupId);
                //var naselje = insertSpecificationAttribute("Naselje",SpecificationAttributeGroupId);
                insertSpecificationAttributeOption("Bjelovarsko-bilogorska", zupanija);
                insertSpecificationAttributeOption("Brodsko-posavska", zupanija);
                insertSpecificationAttributeOption("Dubrovačko-neretvanska", zupanija);
                insertSpecificationAttributeOption("Istarska", zupanija);
                insertSpecificationAttributeOption("Karlovačka", zupanija);
                insertSpecificationAttributeOption("Koprivničko-križevačka", zupanija);
                insertSpecificationAttributeOption("Krapinsko-zagorska", zupanija);
                insertSpecificationAttributeOption("Ličko-senjska", zupanija);
                insertSpecificationAttributeOption("Međimurska", zupanija);
                insertSpecificationAttributeOption("Osječko-baranjska", zupanija);
                insertSpecificationAttributeOption("Požeško-slavonska", zupanija);
                insertSpecificationAttributeOption("Primorsko-goranska", zupanija);
                insertSpecificationAttributeOption("Sisačko-moslavačka", zupanija);
                insertSpecificationAttributeOption("Splitsko-dalmatinska", zupanija);
                insertSpecificationAttributeOption("Šibensko-kninska", zupanija);
                insertSpecificationAttributeOption("Varaždinska", zupanija);
                insertSpecificationAttributeOption("Virovitičko-podravska", zupanija);
                insertSpecificationAttributeOption("Vukovarsko-srijemska", zupanija);
                insertSpecificationAttributeOption("Zadarska", zupanija);
                insertSpecificationAttributeOption("Grad Zagreb", zupanija);
                insertSpecificationAttributeOption("Zagrebačka", zupanija);
                insertSpecificationAttributeOption("Izvan Hrvatske", zupanija);

                // Dostava
                var dostavaID = insertSpecificationAttribute("Dostava", SpecificationAttributeGroupId);
                insertSpecificationAttributeOption("Moguća dostava", dostavaID);
                insertSpecificationAttributeOption("Razgledavanje putem video poziva", dostavaID);

                // Mogućnost plaćanja
                var mogućnostPlaćanja = insertSpecificationAttribute("Mogućnost plaćanja", SpecificationAttributeGroupId);
                insertSpecificationAttributeOption("Gotovina", mogućnostPlaćanja);
                insertSpecificationAttributeOption("Kredit", mogućnostPlaćanja);
                insertSpecificationAttributeOption("Leasing", mogućnostPlaćanja);
                insertSpecificationAttributeOption("Obročno Bankovnim Karticama", mogućnostPlaćanja);
                insertSpecificationAttributeOption("Preuzimanje Leasinga", mogućnostPlaćanja);
                insertSpecificationAttributeOption("Staro za Novo", mogućnostPlaćanja);
                insertSpecificationAttributeOption("Zamjena", mogućnostPlaćanja);


                transactionScope.Complete();
                transactionScope.Dispose();
            }
            catch (TransactionException ex)
            {
                transactionScope.Dispose();
                //MessageBox.Show("Transaction Exception Occured");
            }
        }

        /// <summary>
        /// Collects the DOWN migration expressions
        /// </summary>
        public override void Down()
        {
            //nothing
        }

        int insertSpecificationAttribute(string s, int specificationAttributeGroupId)
        {
            return _dataProvider.InsertEntity(new SpecificationAttribute
            {
                Name = s, SpecificationAttributeGroupId = specificationAttributeGroupId
            }).Id;
        }

        int insertSpecificationAttributeOption(string s, int specificationAttributeId)
        {
            return _dataProvider.InsertEntity(new SpecificationAttributeOption
            {
                Name = s, SpecificationAttributeId = specificationAttributeId
            }).Id;
        }

        int insertSpecificationAttributeOptionColor(string name, string colorSquareRgb, int specificationAttributeId)
        {
            return _dataProvider.InsertEntity(new SpecificationAttributeOption
            {
                Name = name, ColorSquaresRgb = colorSquareRgb, SpecificationAttributeId = specificationAttributeId
            }).Id;
        }

        #endregion
    }
}