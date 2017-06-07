using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Agro.Application.Controllers
{
    public class PedidoController: BaseController
    {
        public JsonResult Index(string filtro, int opcaoBusca)
        {
            int totalRegistros = 0;
            List<AgroApp.Models.Pedido> lista = new List<AgroApp.Models.Pedido>();
            //
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                switch (opcaoBusca)
                {
                    case 1:
                        int icodigo = Convert.ToInt32(filtro);
                        lista = dbCooperativa.Pedido.Where(a => a.Codigo == icodigo && a.Excluido == false).ToList();
                        break;
                    case 2:
                        lista = dbCooperativa.Pedido.Where(a => a.Descricao.Contains(filtro) && a.Excluido == false).ToList();
                        break;
                }
            }
            else
            {
                lista = dbCooperativa.Pedido.Where(a => a.Excluido == false).ToList();
            }
            //
            return Json(
                new
                {
                    data = lista.ToArray(),
                    results = totalRegistros,
                    success = true,
                    errors = String.Empty
                },
                JsonRequestBehavior.AllowGet
            );
        }

    }
}