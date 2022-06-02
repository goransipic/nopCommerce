using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentMigrator;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace SaljiDalje.Core.Data
{
    
    public class ProductExtended : BaseEntity
    {
       public int ProductId { get; set; }
       public int UserId { get; set; }
    }
}
