using System.Linq;
using System.Web.Mvc;
          
namespace Agro.Application.Controllers
{
    public class MenuController : BaseController
    { 
        public JsonResult Index(int empresa, int loja, string usuario, int page, int limit, string sort, string dir)
        {
            var listaMenus = dbCooperativa.Menu.ToList();
            return Json(new { data = listaMenus.ToArray(), results = listaMenus.Count, success = true }, JsonRequestBehavior.AllowGet);            
        }
    }
}
