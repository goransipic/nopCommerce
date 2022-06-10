using Nop.Core;
using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Data.Mapping;

namespace SaljiDalje.Core.Data
{
    
    public partial class TablesOrColumnsNames : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(CostumerPictureAttachmentMapping), "Costumer_PictureAttachment_Mapping" },
        };

        public Dictionary<(Type, string), string> ColumnName => new();
    }
}
