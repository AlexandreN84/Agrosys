using System.Linq;
using System.Web.Mvc;

namespace Agro.Application.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            if (Session["culture"] != null)
            {
                if (Session["TrocaCultura"] != null)
                {
                    ViewBag.TrocaCultura = Session["TrocaCultura"].ToString();
                }
                else
                {
                    ViewBag.TrocaCultura = null;
                }
                //
                CarregaCultura();
            }
            return View();
        }

        public JsonResult ChangeCulture(string culture)
        {
            Session["culture"] = culture;
            return Json(new { data = "".ToArray(), results = 1, success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult teste(string parametros)
        {
            if (!string.IsNullOrEmpty(parametros))
            {
                string[] lista = parametros.Split('@');
                //
                ViewBag.empresa = lista[0].Split('=')[1];
                ViewBag.loja = lista[1].Split('=')[1];
                ViewBag.cliente = lista[2].Split('=')[1];
                ViewBag.opcao = lista[3].Split('=')[1];
                ViewBag.programa = lista[4].Split('=')[1];
            }
            //
            return View();
        }
    }
}
