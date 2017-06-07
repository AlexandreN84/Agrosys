using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Agro.Application.Controllers
{
    public class ProdutoController : BaseController
    {
        public JsonResult Index(string filtro, int opcaoBusca)
        {
            int totalRegistros = 0;
            List<AgroApp.Models.Produto> lista = new List<AgroApp.Models.Produto>();
            //
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                switch (opcaoBusca)
                {
                    case 1:
                        int icodigo = Convert.ToInt32(filtro);
                        lista = dbCooperativa.Produto.Where(a => a.Cd_Produto == icodigo).ToList();
                        break;
                    case 2:
                        lista = dbCooperativa.Produto.Where(a => a.Ds_Descricao.Contains(filtro)).ToList();
                        break;
                }
            }
            else
            {
                lista = dbCooperativa.Produto.Where(a => a.Cd_Produto > 0).ToList();
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

        public JsonResult Save(int codigo, string descricao, string codigobarras, int qtde, int qtdemin, int quilos, int quilosmin, string datavalidade, string preco)
        {
            string ret = string.Empty;
            try
            {
                AgroApp.Models.Produto produto = new AgroApp.Models.Produto();
                produto.Ds_Descricao = descricao;
                produto.Cd_Barras = codigobarras;
                produto.Qt_Quantidade = qtde;
                produto.Qt_Minima = qtdemin;
                produto.Qt_Quilos = quilos;
                produto.Qt_QuilosMinimo = quilosmin;
                produto.Vl_ValorUnitario = Convert.ToDecimal(preco.Replace(".", ","));
                produto.Dt_DataInclusao = DateTime.Now;
                //
                if (string.IsNullOrWhiteSpace(datavalidade))
                {
                    produto.Dt_DataValidade = null;
                }
                else
                {
                    produto.Dt_DataValidade = Convert.ToDateTime(datavalidade);
                }
                //
                if (codigo == 0)
                {
                    dbCooperativa.Produto.Add(produto);
                    dbCooperativa.SaveChanges();
                }
                else
                {
                    produto.Cd_Produto = codigo;
                    dbCooperativa.Entry(produto).State = System.Data.Entity.EntityState.Modified;
                    dbCooperativa.SaveChanges();
                }
                ret = "ok";
            }
            catch (Exception exc)
            {
                ret = exc.Message;
            }
            //
            return Json(new
            {
                data = ret,
                results = 1,
                success = true,
                errors = String.Empty
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete(int cdcodigo)
        {
            AgroApp.Models.Produto produto = dbCooperativa.Produto.Where(a => a.Cd_Produto == cdcodigo).FirstOrDefault();
            //
            if (produto != null)
            {
                dbCooperativa.Produto.Remove(produto);
                dbCooperativa.SaveChanges();
            }
            //
            return Json(new
            {
                data = "OK".ToArray(),
                results = 1,
                success = true,
                errors = String.Empty
            }, JsonRequestBehavior.AllowGet);
        }
    }
}