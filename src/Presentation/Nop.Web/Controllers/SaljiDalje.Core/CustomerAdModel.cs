using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Catalog;

namespace SaljiDalje.Core
{
   public class CustomerAdModel
    {
        public CurrentActiveTab CurrentActiveTab { get; set; }
        public CommmandToExecute CommmandToExecute { get; set; }
        public int ProductId { get; set; }
        
    }

   public enum CurrentActiveTab
   {
       ACTIVE,
       PASSIVE,
   }
   
   public enum CommmandToExecute
   {
       MAKE_ACTIVE,
       MAKE_PASSIVE,
       MAKE_EDIT,
       MAKE_DELETE,
       DELETE_ALL,
   }
}