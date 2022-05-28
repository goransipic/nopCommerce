using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Controllers;

namespace SaljiDalje.Core.Controllers;

public class CustomerInfoController : BasePluginController
{
    public virtual IActionResult Notification()
    {
        return View("~/Plugins/SaljiDalje.Core/Views/Notification.cshtml");
    }
    
    public virtual IActionResult CustomerAds()
    {
        return View("~/Plugins/SaljiDalje.Core/Views/CustomerAds.cshtml");
    }
    
}