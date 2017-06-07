using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.ServiceModel;

namespace Agro.Application
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        void Application_AcquireRequestState(object sender, EventArgs e)
        {
            try
            {
                HttpContext context = HttpContext.Current;
                string culture = System.Globalization.CultureInfo.CurrentCulture.Name;

                if (context.Session != null && context.Session["Culture"] != null)
                {
                    culture = Convert.ToString(context.Session["Culture"]);
                }
                else
                {
                    culture = "pt-BR";
                }

                context.Session["Culture"] = culture.ToUpper();
                System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo(culture);
                System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
            }
            catch { }
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute(
                "Home",  
                "Home",  
                new { controller = "Home", action = "Index" } 
            );

            routes.MapRoute(
                "Login",
                "Login",
                new { controller = "Home", action = "Login" }
            );

            routes.MapRoute(
                "ChangeCulture",
                "ChangeCulture/{culture}",
                new { controller = "Home", action = "ChangeCulture", culture = "pt-BR" }
            );

            routes.MapRoute(
                "ChangeCultureLogado",
                "ChangeCultureLogado/{culture}/{empresa}/{usuario}/{loja}",  
                new { controller = "Home", action = "ChangeCultureLogado", culture = "pt-BR", empresa = "", usuario = "", loja = ""}
            );

            routes.MapRoute(
                "Default",  
                "{controller}/{action}/{id}",  
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }, 
                new string[] { "Agro.Application.Controllers" }
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}