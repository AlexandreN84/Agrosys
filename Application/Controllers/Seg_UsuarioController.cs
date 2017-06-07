using Agro.Application.Models;
using Agro.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Agro.Application.Controllers
{
    public class UsuarioController : BaseController
    {
        List<AgroApp.Models.Cargo> lista_cargo;

        public JsonResult UsuarioCargoCombo(int empresa)
        {
            //List<Cargo_Usuario> usuariosCargo = dbElist.Cargo_Usuario.Where(a => a.Cd_Empresa == empresa).ToList();

            //List<Agro.Application.AgroApp.Models.Usuario> usuarios_tmp = dbElist.Usuario
            //                                                                    .Where(a => a.Cd_Empresa == empresa)
            //                                                                    .OrderBy(a => a.Ds_Usuario)
            //                                                                    .ToList();                    
            ////
            List<AgroApp.Models.Usuario> usuarios = new List<AgroApp.Models.Usuario>();
            ////
            //foreach (Agro.Application.AgroApp.Models.Usuario Usuario in usuarios_tmp)
            //{
            //    if (!usuariosCargo.Any(c => c.Cd_Usuario.ToLower().TrimEnd().Equals(Usuario.Cd_Usuario.ToLower().TrimEnd())))
            //    {
            //        Usuario.Ds_Usuario = Agro.Utils.Extensions.StringExtensions.Camelize("pt-BR", Usuario.Ds_Usuario);
            //        usuarios.Add(Usuario);
            //    }
            //}
            //        
            return Json(new { data = usuarios.ToArray(), results = usuarios.Count(), success = true }, JsonRequestBehavior.AllowGet);
        }

        private List<UsuarioModel> OrdenaValidandoPaginacao(List<UsuarioModel> UsuarioModelList, string sort, string dir)
        {
            // --- ORDENAÇÃO DOS REGISTROS EXIBIDOS NA TELA            
            switch (sort)
            {
                case "Cd_Representante":
                    UsuarioModelList = Utils.Utils.Sort.SortList(UsuarioModelList, o => o.Cd_Representante, dir);
                    break;
                case "Ds_Usuario":
                    UsuarioModelList = Utils.Utils.Sort.SortList(UsuarioModelList, o => o.Ds_Usuario, dir);
                    break;
                case "Ds_Email":
                    UsuarioModelList = Utils.Utils.Sort.SortList(UsuarioModelList, o => o.Ds_Email, dir);
                    break;
                case "Ds_Cargo":
                    UsuarioModelList = Utils.Utils.Sort.SortList(UsuarioModelList, o => o.Ds_Cargo, dir);
                    break;
                case "Cd_Usuario":
                    UsuarioModelList = Utils.Utils.Sort.SortList(UsuarioModelList, o => o.Cd_Usuario, dir);
                    break;
                case "Dt_Cadastro":
                    UsuarioModelList = Utils.Utils.Sort.SortList(UsuarioModelList, o => o.Dt_Cadastro, dir);
                    break;
                case "Dt_Bloqueio":
                    UsuarioModelList = Utils.Utils.Sort.SortList(UsuarioModelList, o => o.Dt_Bloqueio, dir);
                    break;
                case "Dt_Exclusao":
                    UsuarioModelList = Utils.Utils.Sort.SortList(UsuarioModelList, o => o.Dt_Exclusao, dir);
                    break;
                default:
                    UsuarioModelList = Utils.Utils.Sort.SortList(UsuarioModelList, o => o.Cd_Representante, dir);
                    break;
            }
            //
            return UsuarioModelList;
        }

        public JsonResult Login(string email, string senha, string browser)
        {
            // Valida se existe um usuário com o email informado
            AgroApp.Models.Usuario usuario = null;
            try
            {
                usuario = dbCooperativa.Usuario.Where(a => a.Ds_Email.ToLower().Trim().Equals(email.ToLower().Trim())).FirstOrDefault();
            }
            catch (Exception ex)
            {
                //Utils.Utils.Log.GravaLogExc(ex);
                return Json(new { data = ex.Message, results = 0, success = true, lojaunica = true }, JsonRequestBehavior.AllowGet);
            }
            //
            if (usuario != null)
            {
                // Valida se está logado em outra tela
                //string sret = GravaBloqueioLogin(usuario.Cd_Empresa, Convert.ToInt32(usuario.Cd_Loja), usuario.Cd_Usuario, usuario.Cd_Empresa + "ß" + usuario.Cd_Loja + "ß" + usuario.Ds_Email, browser);
                //
                //if (!string.IsNullOrEmpty(sret) && !sret.Equals("nok"))
                //{
                    AgroApp.Models.Cargo cargo = dbCooperativa.Cargo.Where(a => a.Cd_Empresa == usuario.Cd_Empresa && a.Cd_Cargo == usuario.Cd_Cargo).FirstOrDefault();
                    //
                    if (cargo != null)
                    {
                        if (cargo.Bt_Ativo != null && cargo.Bt_Ativo == true)
                        {
                            //senha = Utils.Security.Encryption.DecryptStringAES(senha);
                            //
                            if (usuario.Cd_Senha.TrimEnd().Equals(senha.TrimEnd()))
                            {
                                if (senha.Equals("1234"))
                                {
                                    return Json(new { data = "senha", results = 0, success = true, lojaunica = true }, JsonRequestBehavior.AllowGet);
                                }
                                else if (usuario.Dt_Exclusao != null || usuario.Dt_Bloqueio != null) // nos dois vale o inativo
                                {
                                    if (usuario.Dt_Exclusao != null && usuario.Dt_Bloqueio != null)
                                    {
                                        return Json(new { data = "inativo_bloqueado", results = 0, success = true, lojaunica = true }, JsonRequestBehavior.AllowGet);
                                    }
                                    else if (usuario.Dt_Exclusao != null)
                                    {
                                        return Json(new { data = "inativo", results = 0, success = true, lojaunica = true }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        return Json(new { data = "bloqueado", results = 0, success = true, lojaunica = true }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    bool multiloja = true;
                                    //
                                    return CarregaUsuario(usuario.Cd_Empresa, Convert.ToInt32(usuario.Cd_Loja), usuario.Cd_Usuario, usuario.Ds_Email, Convert.ToInt32(usuario.Cd_Loja), multiloja);
                                }
                            }
                            else
                            {
                                return Json(new { data = "usuario", results = 0, success = true, lojaunica = true }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json(new { data = "cargoinativo", results = 0, success = true, lojaunica = true }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else // usuário sem cargo
                    {
                        return Json(new { data = "semcargo", results = 0, success = true, lojaunica = true }, JsonRequestBehavior.AllowGet);
                    }
                //}
                //else
                //{
                //    return Json(new { data = "bloqueado_sessao", results = 0, success = true, lojaunica = true }, JsonRequestBehavior.AllowGet);
                //}
            }
            else
            {
                // email não encontrado
                return Json(new { data = "email", results = 0, success = true, lojaunica = true }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CarregaUsuarioRelogin()
        {
            if (Session["empresa"] != null)
            {
                int empresa = Convert.ToInt32(Session["empresa"]);
                string usuario = Session["usuario"].ToString();
                int loja = Convert.ToInt32(Session["loja"]);
                //
                // return CarregaUsuario(empresa, usuario, loja);
            }
            return Json(new { data = "erro", results = 1, success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CarregaUsuario(int empresa_usuario, int loja_usuario, string usuario, string email, int loja_logado, bool multiloja)
        {
            AgroApp.Models.Usuario Usuario = new AgroApp.Models.Usuario();
            AgroApp.Models.Cargo cargo;

            Usuario = dbCooperativa
                         .Usuario
                         .Where(a => a.Cd_Empresa == empresa_usuario
                                  && a.Cd_Loja == loja_usuario
                                  && a.Ds_Email.Equals(email)
                                  && a.Cd_Usuario.TrimEnd().Equals(usuario.TrimEnd()))
                         .FirstOrDefault();
            //
            if (Usuario != null)
            {
                cargo = dbCooperativa.Cargo.Where(a => a.Cd_Empresa == Usuario.Cd_Empresa && a.Cd_Cargo == Usuario.Cd_Cargo).FirstOrDefault();
                //
                AgroApp.Models.Loja oloja = null;
                List<Menu_AcaoModels> lista_menu_acao = null;
                List<Menu_AcaoModels> lista_menu_acao_usuario = null;
                //
                if (Usuario.Cd_Usuario.ToLower().Trim().Equals("master"))
                {
                    oloja = dbCooperativa.Loja.Where(l => l.Cd_Loja == loja_logado).FirstOrDefault();
                    //
                    string sql = string.Empty;
                    sql = @"SELECT * FROM Menu_Acao";
                    //
                    DataTable oDataTable = this.ExecutaSQL(sql);
                    if (oDataTable.Rows != null && oDataTable.Rows.Count > 0)
                    {
                        lista_menu_acao = new List<Menu_AcaoModels>();
                        foreach (DataRow item in oDataTable.Rows)
                        {
                            Menu_AcaoModels tmp = new Menu_AcaoModels();
                            tmp.Cd_Acao = Convert.ToInt32(item["Cd_Acao"]);
                            tmp.Cd_Menu = Convert.ToInt32(item["Cd_Menu"]);
                            tmp.Cd_MenuAcao = Convert.ToInt32(item["Cd_MenuAcao"]);
                            lista_menu_acao.Add(tmp);
                        }
                    }
                }
                else
                {
                    oloja = dbCooperativa.Loja.Where(l => l.Cd_Loja == loja_logado).FirstOrDefault();
                    //
                    if (loja_usuario == 0)
                    {
                        lista_menu_acao = (from mac in dbCooperativa.Menu_AcaoCargo
                                           join ma in dbCooperativa.Menu_Acao on mac.Cd_MenuAcao equals ma.Cd_MenuAcao
                                           where mac.Cd_Empresa == empresa_usuario && mac.Cd_Cargo == Usuario.Cd_Cargo
                                           select new Menu_AcaoModels { Cd_MenuAcao = mac.Cd_MenuAcao, Cd_Acao = ma.Cd_Acao, Cd_Menu = ma.Cd_Menu }).ToList();
                    }
                    else // adicionado a validação fl_loja
                    {
                        lista_menu_acao = (from mac in dbCooperativa.Menu_AcaoCargo
                                           join ma in dbCooperativa.Menu_Acao on mac.Cd_MenuAcao equals ma.Cd_MenuAcao
                                           where mac.Cd_Empresa == empresa_usuario && mac.Cd_Cargo == Usuario.Cd_Cargo
                                           select new Menu_AcaoModels { Cd_MenuAcao = mac.Cd_MenuAcao, Cd_Acao = ma.Cd_Acao, Cd_Menu = ma.Cd_Menu }).ToList();
                    }
                    //
                    //lista_menu_acao_usuario = (from mau in dbCooperativa.Menu_AcaoUsuario
                    //                           join ma in dbCooperativa.Menu_Acao on mau.Cd_MenuAcao equals ma.Cd_MenuAcao
                    //                           where mau.Cd_Empresa == empresa_usuario && mau.Cd_Usuario.Equals(usuario.TrimEnd()) && mau.Fl_Ativo
                    //                           select new Menu_AcaoModels { Cd_MenuAcao = mau.Cd_MenuAcao, Cd_Acao = ma.Cd_Acao, Cd_Menu = ma.Cd_Menu }).ToList();
                    //
                    //foreach (Menu_AcaoModels item in lista_menu_acao_usuario)
                    //{
                    //    lista_menu_acao.Add(item);
                    //}
                }
                //
                var lista_menu = (from me in dbCooperativa.Menu
                                  join mo in dbCooperativa.Modulo on me.Cd_Modulo equals mo.Cd_Modulo
                                  where me.Fl_Mostra.Value
                                  select new MenuModels
                                  {
                                      Cd_Menu = me.Cd_Menu,
                                      Cd_Menu_Pai = me.Cd_Menu_Pai,
                                      Cd_Modulo = me.Cd_Modulo,
                                      Ds_Funcao = me.Ds_Funcao,
                                      Ds_Menu = me.Ds_Menu,
                                      Ds_Menu_En = me.Ds_Menu_En,
                                      Ds_Menu_Es = me.Ds_Menu_Es,
                                      Ds_Modulo = mo.Ds_Modulo,
                                      Ds_Imagem = me.Ds_Imagem

                                  }).ToList();
                //
                List<AgroApp.Models.Acao> lista_acoes = dbCooperativa.Acao.ToList();
                AgroApp.Models.Empresa oempresa = dbCooperativa.Empresa.Where(e => e.EMP_Codigo == oloja.Cd_Empresa).FirstOrDefault();
                language = Convert.ToInt32(oloja.Cd_Idioma);
                switch (language)
                {
                    case 2: Session["Culture"] = "EN-US"; break;
                    case 3: Session["Culture"] = "ES-ES"; break;
                    default: Session["Culture"] = "PT-BR"; break;
                }
                //
                SessaoModels sessaoModels = new SessaoModels();
                //
                sessaoModels.SES_Cd_Idioma = language;
                sessaoModels.SES_Cd_Cargo = Usuario.Cd_Cargo.HasValue ? Usuario.Cd_Cargo.Value : -1;
                sessaoModels.SES_Cd_Empresa = oloja.Cd_Empresa;
                sessaoModels.SES_Cd_Loja = oloja.Cd_Loja;
                sessaoModels.SES_Qt_Sessao = oloja.Qt_Sessao.HasValue ? oloja.Qt_Sessao.Value : 15;
                sessaoModels.SES_Cd_Usuario = usuario;
                sessaoModels.SES_Corrente = true;
                sessaoModels.SES_Cd_Mercado = oloja.Cd_Mercado;
                sessaoModels.SES_Empresa_Fantasia = StringExtensions.Camelize("pt-BR", oempresa != null ? oempresa.EMP_Nome : "");
                sessaoModels.SES_Loja_Fantasia = StringExtensions.Camelize("pt-BR", oloja.Ds_Fantasia);
                sessaoModels.SES_Representante = "";
                sessaoModels.SES_Usuario_Nome = StringExtensions.Camelize("pt-BR", Usuario.Ds_Usuario);
                sessaoModels.SES_Menu_Acao = lista_menu_acao;
                sessaoModels.SES_Menu = lista_menu;
                sessaoModels.SES_Lista_Acoes = lista_acoes;
                sessaoModels.SES_Email = Usuario.Ds_Email;
                sessaoModels.SES_Cd_Representante = Usuario.Cd_Representante.HasValue ? Usuario.Cd_Representante.Value : 0;
                //
                if (Session["Data_Login"] != null)
                {
                    sessaoModels.SES_Data_Login = Session["Data_Login"].ToString();
                }
                //
                Session["usuario"] = usuario;
                //
                return Json(new { data = sessaoModels, results = 1, success = true, lojaunica = multiloja }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = "NOK", results = 0, success = false, lojaunica = multiloja }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult Logout()
        {
            Session["TrocaCultura"] = null;
            Session["empresa"] = null;
            Session["usuario"] = null;
            Session["loja"] = null;
            //
            return Json(new
            {
                data = "",
                results = 1,
                success = true
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ResetarSenhaLogin(string email, string senha)
        {
            AgroApp.Models.Usuario Usuario = dbCooperativa.Usuario
                        .Where(a => a.Cd_Senha.TrimEnd().Equals("1234")
                                 && a.Ds_Email.TrimEnd().Equals(email))
                        .FirstOrDefault();

            if (Usuario != null)
            {
                Usuario.Cd_Senha = senha;
                dbCooperativa.SaveChanges();
                return Json(new { data = "ok", results = 0, success = true }, JsonRequestBehavior.AllowGet);
            }
            //
            return Json(new { data = "nok", results = 0, success = true }, JsonRequestBehavior.AllowGet);
        }

        private string PegaDescricaoCargo(int? cargo, int empresa)
        {
            string sRet = string.Empty;
            //
            if (cargo == null)
            {
                return "";
            }
            //
            if (lista_cargo == null)
            {

                lista_cargo = dbCooperativa.Cargo.ToList();
                AgroApp.Models.Cargo ocargo = lista_cargo.Where(a => a.Cd_Cargo == cargo && a.Cd_Empresa == empresa).FirstOrDefault();
                if (ocargo != null)
                {
                    sRet = ocargo.Ds_Cargo;
                }
            }
            else
            {
                AgroApp.Models.Cargo ocargo = lista_cargo.Where(a => a.Cd_Cargo == cargo && a.Cd_Empresa == empresa).FirstOrDefault();
                if (ocargo != null)
                {
                    sRet = ocargo.Ds_Cargo;
                }
            }
            //
            return sRet;
        }

        public JsonResult IndexForCombobox(int empresa, int loja, int empresa_acao, int loja_acao, string usuario)
        {
            //UsuarioMethods UsuarioMethods = new UsuarioMethods(empresa, usuario);
            //UsuarioLojaPerfilMethods UsuarioLojaPerfilMethods = new UsuarioLojaPerfilMethods(empresa, usuario);

            List<UsuarioModel> UsuarioListModel = new List<UsuarioModel>();
            //List<UsuarioLojaPerfil> UsuarioLojaPerfilList = UsuarioLojaPerfilMethods.FindAll(q => q.Id.Cd_Empresa == empresa
            //                                                                                               && q.Id.Cd_Loja == loja);

            //foreach (UsuarioLojaPerfil perfil in UsuarioLojaPerfilList)
            //{
            //    List<Entities.Usuario> UsuarioList = UsuarioMethods.FindAll(q => q.Id.Cd_Empresa == perfil.Id.Cd_Empresa
            //                                                                     && q.Id.Cd_Usuario == perfil.Id.Cd_Usuario
            //                                                                     && q.Dt_Bloqueio == null
            //                                                                     && q.Dt_Exclusao == null);
            //    foreach (Entities.Usuario Usuario in UsuarioList)
            //    {
            //        UsuarioListModel.Add(new UsuarioModel()
            //        {
            //            Id = Usuario.GetExtJSId(),
            //            Cd_Empresa = Usuario.Id.Cd_Empresa,
            //            Cd_Usuario = Usuario.Id.Cd_Usuario.Trim(),
            //            Ds_Usuario = StringExtensions.Camelize("pt-BR", Usuario.Ds_Usuario.ToLower())
            //        });
            //    }
            //}

            //UsuarioListModel = UsuarioListModel.OrderBy(o => o.Ds_Usuario).ToList();

            List<AgroApp.Models.Usuario> lista_usuarios = dbCooperativa.Usuario.Where(a => a.Cd_Empresa == empresa && a.Cd_Loja == loja && a.Dt_Bloqueio == null && a.Dt_Exclusao == null).ToList();
            //
            foreach (AgroApp.Models.Usuario Usuario in lista_usuarios)
            {
                UsuarioListModel.Add(new UsuarioModel()
                {
                    Cd_Empresa = Usuario.Cd_Empresa,
                    Cd_Usuario = Usuario.Cd_Usuario.Trim(),
                    Ds_Usuario = StringExtensions.Camelize("pt-BR", Usuario.Ds_Usuario.ToLower())
                });
            }
            //
            return Json(new { data = UsuarioListModel.ToArray(), results = UsuarioListModel.Count, success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}