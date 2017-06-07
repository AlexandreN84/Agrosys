using AgroApp.Models;
using Agro.Application.Models;
using Agro.Utils;
using Agro.Utils.Extensions;
using Mvc.Mailer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Agro.Application.Controllers
{
    public class BaseController : Controller
    {
        public CooperativaEntities dbCooperativa;
        public int language = (int)LanguageTypeEnum.portugues;
        public string slanguage = "pt-BR";
        public LanguageTypeEnum currentLanguage = LanguageTypeEnum.portugues;
        public int pageRows = 10;
        public int webconfig_empresa = Convert.ToInt32(WebConfigurationManager.AppSettings["empresa"]);
        public string webconfig_usuario = WebConfigurationManager.AppSettings["usuario"];
        private string stringConexao = string.Empty;
        SqlConnection oSqlConnection;
        public CultureInfo culture = null;
        public Thread oThread = null;

        public BaseController()
        {
            dbCooperativa = new CooperativaEntities();
            CultureInfo ci;
            //
            switch (language)
            {
                case 1:
                    ci = new CultureInfo("pt-BR", true);
                    slanguage = "pt-BR";
                    break;
                case 2:
                    ci = new CultureInfo("en-US", true);
                    slanguage = "en-US";
                    break;
                case 3:
                    ci = new CultureInfo("es-ES", true);
                    slanguage = "es-ES";
                    break;
                default:
                    ci = new CultureInfo("pt-BR", true);
                    slanguage = "pt-BR";
                    break;
            }
            culture = new CultureInfo(slanguage);
            Thread.CurrentThread.CurrentCulture = ci;
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            if (requestContext.HttpContext.Session["Culture"] == null)
            {
                requestContext.HttpContext.Session["Culture"] = "pt-BR";
            }
            //
            string browser = ((HttpBrowserCapabilitiesWrapper)(((HttpContextWrapper)requestContext.HttpContext).Request.Browser)).Browser;
            if (((HttpContextWrapper)requestContext.HttpContext).Response != null)
            {
                if (requestContext.HttpContext.Session["Data_Login"] != null)
                {
                    ((HttpContextWrapper)requestContext.HttpContext).Response.AppendHeader("sessao_data_login", requestContext.HttpContext.Session["Data_Login"].ToString());
                }
                //
                if (requestContext.HttpContext.Session["usuario"] != null)
                {
                    ((HttpContextWrapper)requestContext.HttpContext).Response.AppendHeader("sessao_usuario", requestContext.HttpContext.Session["usuario"].ToString());
                }
            }
            //
            switch (requestContext.HttpContext.Session["Culture"].ToString().ToUpper())
            {
                case "EN-US":
                    currentLanguage = LanguageTypeEnum.english;
                    break;
                case "ES-ES":
                    currentLanguage = LanguageTypeEnum.espanol;
                    break;
                case "PT-BR":
                    currentLanguage = LanguageTypeEnum.portugues;
                    break;
                default: // se for qualquer outro valor assume Português
                    currentLanguage = LanguageTypeEnum.portugues;
                    break;
            }
            //
            language = (int)currentLanguage;
            base.Initialize(requestContext);
        }

        public JsonResult SessaoFinaliza(int empresa, int loja, string[] lista_blouqeio)
        {
            if (lista_blouqeio != null)
            {
                Session["finaliza"] = lista_blouqeio;
            }
            //
            return Json(new { data = "".ToArray(), results = 0, success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Desistiu()
        {
            if (Session["finaliza"] != null)
            {
                string sql = "";
                string sql_delete = "";
                string[] lista = (string[])Session["finaliza"];
                //
                for (int i = 0; i < lista.Length; i++)
                {
                    sql = "SELECT * FROM Bloqueio WHERE Chave = '" + lista[i] + "'";
                    //
                    if (string.IsNullOrEmpty(sql_delete))
                    {
                        sql_delete = "'" + lista[i] + "'";
                    }
                    else
                    {
                        sql_delete += ", '" + lista[i] + "'";
                    }
                    //
                    DataTable tb = new DataTable();
                    //
                    if (ExecutaSQL(sql, out tb))
                    {
                        string sessao = "";
                        string chave = tb.Rows[0]["Chave"].ToString();
                        string empresa = tb.Rows[0]["Empresa"].ToString();
                        string loja = tb.Rows[0]["Loja"].ToString();
                        string usuario = tb.Rows[0]["Usuario"].ToString();
                        string tipo = tb.Rows[0]["Tipo"].ToString();
                        string data = tb.Rows[0]["Data"].ToString();
                        //
                        if (string.IsNullOrEmpty(sessao))
                        {
                            sessao = chave + "§" + empresa + "§" + loja + "§" + usuario + "§" + tipo + "§" + data;
                        }
                        else
                        {
                            sessao += "¥" + chave + "§" + empresa + "§" + loja + "§" + usuario + "§" + tipo + "§" + data;
                        }
                        Session["volta"] = sessao;
                    }
                }
                //
                sql = "DELETE Bloqueio WHERE Chave IN (" + sql_delete + ")";
                if (ExecutaSQLNonQuery(sql) < 1)
                {
                    Session["volta"] = "";
                }
            }
            //
            return Json(new { data = "".ToArray(), results = 0, success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Volto()
        {
            if (Session["volta"] != null)
            {
                string[] dados = Session["volta"].ToString().Split('¥');
                //
                for (int i = 0; i < dados.Length; i++)
                {
                    string[] registro = dados[i].Split('§');
                    string values = "'" + registro[0] + "', " + registro[1] + ", " + registro[2] + ", '" + registro[3] + "', " + registro[4] + ", '" + PegaDataBloqueio() + "', 1";
                    string sql = "INSERT INTO Bloqueio (Chave, Empresa, Loja, Usuario, Tipo, Data, Liberado) VALUES (" + values + ")";
                    ExecutaSQLNonQuery(sql);
                }
                //
                Session["volta"] = "";
            }
            //
            return Json(new { data = "".ToArray(), results = 0, success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GravaBloqueio(int empresa, int loja, string usuario, string chave, int tipo)
        {
            string sRet = string.Empty;
            string sql = string.Empty;
            int iRet = 0;
            //
            sql = @"SELECT * FROM  Bloqueio  WHERE Tipo = " + tipo + " AND Chave = '" + chave + "'";
            //
            DataTable oDataTable = new DataTable();
            if (!ExecutaSQL(sql, out oDataTable))
            {
                sql = @"INSERT INTO Bloqueio (Empresa, Loja, usuario, Chave, Data, Tipo) values (" + empresa + ", " +
                                                                                                     loja + ", '" +
                                                                                                     usuario + "', '" +
                                                                                                     chave + "', '" +
                                                                                                     PegaDataBloqueio() + "', " +
                                                                                                     tipo + ")";
                iRet = ExecutaSQLNonQuery(sql);
                //
                if (iRet > 0)
                {
                    sRet = "ok";
                }
                else
                {
                    sRet = "nok";
                }
            }
            else
            {
                sRet = "bloq";
            }
            //
            return Json(new { data = sRet, results = 1, success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GravaDesbloqueio(string chave)
        {
            string sRet = string.Empty;
            string sql = string.Empty;
            int iRet = 0;
            //
            sql = @"DELETE  Bloqueio WHERE Chave = '" + chave + "'";
            iRet = ExecutaSQLNonQuery(sql);
            //
            return Json(new { data = sRet, results = 1, success = true }, JsonRequestBehavior.AllowGet);
        }

        public string GravaBloqueioLogin(int empresa, int loja, string usuario, string chave, string browser)
        {
            string sRet = string.Empty;
            string sql = string.Empty;
            string sData = string.Empty;
            string sql_comando = string.Empty;
            //
            sql = @"SELECT * FROM  Bloqueio  WHERE Tipo = 0 AND Chave = '" + chave + "'";
            //
            DataTable oDataTable = ExecutaSQL(sql);
            sData = PegaDataBloqueio();
            //
            if (oDataTable.Rows.Count > 0)
            {
                sql = @"UPDATE Bloqueio SET Data = '" + sData + "', Navegador = '" + browser + "' WHERE Chave = '" + oDataTable.Rows[0]["Chave"].ToString() + "'";
            }
            else
            {
                sql = @"INSERT INTO Bloqueio (Empresa, Loja, usuario, Chave, Data, Tipo, Navegador) values (" + empresa + ", " +
                                                                                                                loja + ", '" +
                                                                                                                usuario + "', '" +
                                                                                                                chave + "', '" +
                                                                                                                PegaDataBloqueio() + "', 0, '" +
                                                                                                                browser + "')";
            }
            //
            int iRet = ExecutaSQLNonQuery(sql);
            //
            if (iRet > 0)
            {
                Session["Data_Login"] = sData;
                sRet = "ok";
            }
            else
            {
                sRet = "nok";
            }
            //
            return sRet;
        }

        public JsonResult PegaSessaoLogTrocaLista(int empresa, int loja, int cliente, string opcao, string indice)
        {
            string sesssao1 = string.Empty;
            string sesssao2 = string.Empty;
            string sesssao3 = string.Empty;
            string sesssao4 = string.Empty;
            string sesssao5 = string.Empty;
            //
            if (Session[empresa + "" + loja + "" + cliente + "" + opcao + "1"] != null)
            {
                sesssao1 = Session[empresa + "" + loja + "" + cliente + "" + opcao + "1"].ToString();
            }
            //
            if (Session[empresa + "" + loja + "" + cliente + "" + opcao + "2"] != null)
            {
                sesssao2 = Session[empresa + "" + loja + "" + cliente + "" + opcao + "2"].ToString();
            }
            //
            if (Session[empresa + "" + loja + "" + cliente + "" + opcao + "3"] != null)
            {
                sesssao3 = Session[empresa + "" + loja + "" + cliente + "" + opcao + "3"].ToString();
            }
            //
            if (Session[empresa + "" + loja + "" + cliente + "" + opcao + "4"] != null)
            {
                sesssao4 = Session[empresa + "" + loja + "" + cliente + "" + opcao + "4"].ToString();
            }
            //
            if (Session[empresa + "" + loja + "" + cliente + "" + opcao + "5"] != null)
            {
                sesssao5 = Session[empresa + "" + loja + "" + cliente + "" + opcao + "5"].ToString();
            }
            //
            //if (Session[empresa + "" + loja + "" + cliente + "" + opcao + "6"] != null)
            //{
            //    sesssao5 = Session[empresa + "" + loja + "" + cliente + "" + opcao + "6"].ToString();
            //}
            //
            return Json(new { data = "", results = 1, success = true, sesssao1 = sesssao1, sesssao2 = sesssao2, sesssao3 = sesssao3, sesssao4 = sesssao4, sesssao5 = sesssao5 }, JsonRequestBehavior.AllowGet);
        }

        public void GravaSessaoProjeto(int empresa, int loja, int cliente, string opcao, string status)
        {
            //
            //Utils.Utils.Log.GravaLogMesmo("Status : " + status);
            Session[empresa + "" + loja + "" + cliente + "" + opcao + status] = status;
        }

        public string PegaDataBloqueio()
        {
            string sRet = string.Empty;
            //
            sRet = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
            //
            return sRet;
        }

        public JsonResult PegaDadosLogin(string chave)
        {
            string sData_Login = string.Empty;
            string sData_Banco = string.Empty;
            string sNavegador = string.Empty;
            bool bLoginDiferente = false;
            Stack<EventLogEntry> stack = new Stack<EventLogEntry>();
            //
            if (Session["Data_Login"] != null)
            {
                sData_Login = Session["Data_Login"].ToString();
            }
            //
            if (!sData_Login.Equals(sData_Banco))
            {
                bLoginDiferente = true;
            }
            //
            if (Session["Data_Login"] == null)
            {
                EventLog aLog = new EventLog("system", ".");
                EventLogEntry entry;
                EventLogEntryCollection entries = aLog.Entries;
                for (int i = 0; i < entries.Count; i++)
                {
                    entry = entries[i];
                    //
                    if (entry.Source.Equals("WAS") && entry.TimeWritten >= DateTime.Today)
                    {
                        stack.Push(entry);
                    }
                }
            }
            //
            return Json(new
            {
                data = "",
                bLoginDiferente = bLoginDiferente,
                navegador = sNavegador,
                sData_Login = sData_Login,
                sData_Banco = sData_Banco,
                results = 1,
                sessao_nula = Session["Data_Login"] == null,
                stack = stack,
                success = true
            },
                              JsonRequestBehavior.AllowGet);
        }

        #region SQL
        //
        private string StringConexao
        {
            get
            {
                string sessao = string.Empty;
                if (Session == null)
                {
                    sessao = SetaConexao();
                }
                else
                {
                    if (Session["stringConexao"] == null)
                    {
                        Session["stringConexao"] = SetaConexao();
                    }
                    sessao = Session["stringConexao"].ToString();
                }
                return sessao;
            }

            set
            {
                if (Session != null)
                {
                    Session["stringConexao"] = value;
                }
            }
        }

        private string SetaConexao()
        {
            return WebConfigurationManager.AppSettings["dbstring"];
        }

        public bool StringSqlValida(string textoBusca)
        {
            if (textoBusca.Contains(";")) { return false; }
            if (textoBusca.Contains("'")) { return false; }
            if (textoBusca.Contains("-")) { return false; }
            if (textoBusca.Contains("/")) { return false; }
            if (textoBusca.Contains("*")) { return false; }
            if (textoBusca.Contains("\"")) { return false; }
            //
            return true;
        }

        public DataTable ExecutaSQL(string sql)
        {
            oSqlConnection = new SqlConnection(StringConexao);
            SqlCommand oSqlCommand = new SqlCommand();
            oSqlCommand.CommandText = sql;
            oSqlCommand.CommandType = CommandType.Text;
            oSqlCommand.Connection = oSqlConnection;
            oSqlCommand.CommandTimeout = 300;
            oSqlConnection.Open();
            System.Data.Common.DbDataReader dbDataReader = oSqlCommand.ExecuteReader();
            DataTable oDataTable = ObterTabela(dbDataReader);
            oSqlConnection.Close();
            //
            return oDataTable;
        }

        public bool ExecutaSQL(string sql, out DataTable datatable)
        {
            oSqlConnection = new SqlConnection(StringConexao);
            SqlCommand oSqlCommand = new SqlCommand();
            oSqlCommand.CommandText = sql;
            oSqlCommand.CommandType = CommandType.Text;
            oSqlCommand.Connection = oSqlConnection;
            oSqlCommand.CommandTimeout = 300;
            oSqlConnection.Open();
            System.Data.Common.DbDataReader dbDataReader = oSqlCommand.ExecuteReader();
            datatable = ObterTabela(dbDataReader);
            oSqlConnection.Close();
            //
            if (datatable != null && datatable.Rows.Count > 0)
            {
                return true;
            }
            //
            return false;
        }

        public bool ExecutaSQL(string sql, out DataTable datatable, string data)
        {
            oSqlConnection = new SqlConnection(data);
            SqlCommand oSqlCommand = new SqlCommand();
            oSqlCommand.CommandText = sql;
            oSqlCommand.CommandType = CommandType.Text;
            oSqlCommand.Connection = oSqlConnection;
            oSqlCommand.CommandTimeout = 300;
            oSqlConnection.Open();
            System.Data.Common.DbDataReader dbDataReader = oSqlCommand.ExecuteReader();
            datatable = ObterTabela(dbDataReader);
            oSqlConnection.Close();
            //
            if (datatable != null && datatable.Rows.Count > 0)
            {
                return true;
            }
            //
            return false;
        }

        public int ExecutaSQLNonQuery(string sql)
        {
            int iRet = 0;
            oSqlConnection = new SqlConnection(StringConexao);
            SqlCommand oSqlCommand = new SqlCommand();
            oSqlConnection.Open();
            SqlTransaction oSqlTransaction = oSqlConnection.BeginTransaction();
            oSqlCommand.Transaction = oSqlTransaction;
            oSqlCommand.CommandText = sql;
            oSqlCommand.CommandType = CommandType.Text;
            oSqlCommand.Connection = oSqlConnection;
            oSqlCommand.CommandTimeout = 300;
            iRet = oSqlCommand.ExecuteNonQuery();
            oSqlCommand.Transaction.Commit();
            oSqlConnection.Close();
            //
            return iRet;
        }

        public DataSet ExecutaSQLToDataSet(string sql)
        {
            oSqlConnection = new SqlConnection(StringConexao);
            SqlDataAdapter da = new SqlDataAdapter(sql, oSqlConnection);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds;
        }

        private DataTable ObterTabela(System.Data.Common.DbDataReader reader)
        {
            DataTable tbEsquema = reader.GetSchemaTable();
            DataTable tbRetorno = new DataTable();

            foreach (DataRow r in tbEsquema.Rows)
            {
                if (!tbRetorno.Columns.Contains(r["ColumnName"].ToString()))
                {
                    DataColumn col = new DataColumn()
                    {
                        ColumnName = r["ColumnName"].ToString(),
                        Unique = Convert.ToBoolean(r["IsUnique"]),
                        AllowDBNull = Convert.ToBoolean(r["AllowDBNull"])
                        //ReadOnly = false //Convert.ToBoolean(r["IsReadOnly"])
                    };
                    tbRetorno.Columns.Add(col);
                }
                else
                {
                    /* break point aqui quando der louca na montagem da estrutura da tabela dinâmica */
                }
            }

            while (reader.Read())
            {
                DataRow novaLinha = tbRetorno.NewRow();
                for (int i = 0; i < tbRetorno.Columns.Count; i++)
                {
                    novaLinha[i] = reader.GetValue(i);
                }
                tbRetorno.Rows.Add(novaLinha);
            }

            return tbRetorno;
        }
        //
        #endregion

        public void CarregaCultura()
        {
            string culture = Session["culture"].ToString();
            string global = string.Empty;
            string caminho = string.Empty;
            //
            caminho = ((HttpRequestWrapper)Request).Url.AbsoluteUri;
            ViewBag.caminho = caminho;
            switch (culture.ToUpper())
            {
                case "EN-US": global = caminho + "js/en-US/global.js"; break;
                case "ES-ES": global = caminho + "js/es-ES/global.js"; break;
                default: global = caminho + "js/pt-BR/global.js"; break;
            }
            //
            CultureInfo cultureInfo = new CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            //
            ViewBag.Culture = global;
        }

        //public string BuscaMensagem(int cdmensagem, int cdidioma, ArrayList argumento = null)
        //{
        //    string sRet = string.Empty;
        //    //
        //    Rom_MensagemIdioma rom_MensagemIdioma = dbCooperativa.Rom_MensagemIdioma.Where(a => a.Cd_Idioma == cdidioma && a.Cd_Mensagem == cdmensagem).FirstOrDefault();
        //    //
        //    if (rom_MensagemIdioma != null)
        //    {
        //        sRet = rom_MensagemIdioma.MM_Mensagem;
        //    }
        //    //
        //    if (argumento != null)
        //    {
        //        foreach (string item in argumento)
        //        {
        //            sRet = Microsoft.VisualBasic.Strings.Replace(sRet, "%s", item, 1, 1);
        //        }
        //    }
        //    //
        //    return sRet;
        //}

        #region CARGO - MÓDULO - PERMISSÕES
        //
        public JsonResult ListaModulos()
        {

            List<AgroApp.Models.Menu> menuList = dbCooperativa.Menu.Where(a => a.Cd_Menu_Pai == null).ToList();
            //            
            foreach (AgroApp.Models.Menu item in menuList)
            {
                //item.Ds_Modulo = moduloMethods.FindAll(a => a.Cd_Modulo == item.Cd_Modulo).FirstOrDefault().Ds_Modulo +
                //    "$" + item.Ds_Menu;
            }
            //
            return Json(new { data = menuList.ToArray(), results = menuList.Count, success = true }, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult SetaPermissao(int empresa, int loja, int empresa_acao, int loja_acao, string usuario, int menu, int acao, bool ativa)
        //{
        //    int cd_menu_acao = PegaMenuAcaoId(menu, acao);
        //    if (cd_menu_acao > 0)
        //    {
        //        AgroApp.Models.Usuario Usuario = PegaUsuario(empresa_acao, loja_acao, empresa_acao, loja_acao, usuario);

        //        if (Usuario != null)
        //        {
        //            AgroApp.Models.Menu_AcaoUsuario menu_AcaoUsuario = dbCooperativa.Menu_AcaoUsuario
        //                .Where(a => a.Cd_Empresa == empresa_acao &&
        //                              a.Cd_Loja == loja_acao &&
        //                              a.Cd_MenuAcao == cd_menu_acao &&
        //                              a.Cd_Usuario.Trim().TrimEnd().Equals(usuario))
        //                .FirstOrDefault();
        //            //
        //            if (menu_AcaoUsuario == null) // Insert
        //            {
        //                menu_AcaoUsuario = new AgroApp.Models.Menu_AcaoUsuario();
        //                if (ativa)
        //                {
        //                    menu_AcaoUsuario.Fl_Ativo = true;
        //                }
        //                else
        //                {
        //                    menu_AcaoUsuario.Fl_Ativo = false;
        //                }
        //                //menu_AcaoUsuario.Id = new Menu_AcaoUsuarioIdentifier();
        //                //menu_AcaoUsuario.Id.Cd_Empresa = empresa_acao;
        //                //menu_AcaoUsuario.Id.Cd_Loja = loja_acao;
        //                //menu_AcaoUsuario.Id.Cd_MenuAcao = cd_menu_acao;
        //                //menu_AcaoUsuario.Id.Cd_Usuario = usuario;
        //            }
        //            else // Update
        //            {
        //                if (ativa)
        //                {
        //                    menu_AcaoUsuario.Fl_Ativo = true;
        //                }
        //                else
        //                {
        //                    menu_AcaoUsuario.Fl_Ativo = false;
        //                }
        //            }

        //            // Salva
        //            try
        //            {
        //                //dbElist.Menu_AcaoUsuario.Save(menu_AcaoUsuario);
        //            }
        //            catch (Exception exc)
        //            {
        //            }
        //            //         
        //            return Json(new { data = "", results = 0, success = true }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { data = "perfil", results = 0, success = true }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    else
        //    {
        //        return Json(new { data = "cd_menu_acao", results = 0, success = true }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public JsonResult SetaPermissaoCargo(int empresa, int loja, string usuario, string cultura, int ftCdEmpresa, int ftCdCargo, int cd_menu_acao, bool ativa, int acao, int menu)
        {
            if (cd_menu_acao > 0)
            {
                AgroApp.Models.Menu_AcaoCargo menu_AcaoCargo = dbCooperativa.Menu_AcaoCargo.Where(a => a.Cd_Empresa == ftCdEmpresa &&
                                                                                         a.Cd_Cargo == ftCdCargo &&
                                                                                         a.Cd_MenuAcao == cd_menu_acao)
                                                                             .FirstOrDefault();
                //
                if (menu_AcaoCargo != null)
                {
                    dbCooperativa.Menu_AcaoCargo.Remove(menu_AcaoCargo);
                    dbCooperativa.SaveChanges();
                    return Json(new { data = "ok", results = 0, success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (ativa)
                    {
                        AgroApp.Models.Menu_AcaoCargo menu_AcaoCargo_temp = new AgroApp.Models.Menu_AcaoCargo();
                        menu_AcaoCargo_temp.Cd_Cargo = ftCdCargo;
                        menu_AcaoCargo_temp.Cd_Empresa = ftCdEmpresa;
                        // menu_AcaoCargo_temp.Cd_Loja = loja_acao;
                        menu_AcaoCargo_temp.Cd_MenuAcao = cd_menu_acao;
                        dbCooperativa.Menu_AcaoCargo.Add(menu_AcaoCargo_temp);
                        //
                        dbCooperativa.SaveChanges();
                        return Json(new { data = "ok", results = 0, success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { data = "perfil", results = 0, success = true }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                if (acao > 0 && menu > 0)
                {
                    AgroApp.Models.Menu_Acao menu_Acao = new AgroApp.Models.Menu_Acao();
                    menu_Acao.Cd_Acao = acao;
                    menu_Acao.Cd_Menu = menu;
                    dbCooperativa.Menu_Acao.Add(menu_Acao);
                    dbCooperativa.SaveChanges();
                    //
                    menu_Acao = dbCooperativa.Menu_Acao.Where(a => a.Cd_Menu == menu && a.Cd_Acao == acao).FirstOrDefault();
                    if (menu_Acao != null)
                    {
                        AgroApp.Models.Menu_AcaoCargo menu_AcaoCargo_temp = new AgroApp.Models.Menu_AcaoCargo();
                        menu_AcaoCargo_temp.Cd_Cargo = ftCdCargo;
                        menu_AcaoCargo_temp.Cd_Empresa = ftCdEmpresa;
                        menu_AcaoCargo_temp.Cd_MenuAcao = menu_Acao.Cd_MenuAcao;
                        dbCooperativa.Menu_AcaoCargo.Add(menu_AcaoCargo_temp);
                        //
                        dbCooperativa.SaveChanges();
                        return Json(new { data = "ok", results = 0, success = true }, JsonRequestBehavior.AllowGet);
                    }
                    //
                    return Json(new { data = "cd_menu_acao_novo", results = 0, success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = "cd_menu_acao", results = 0, success = true }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult SetaPermissaoMenuAcao(int cdmenuacao, bool ativa)
        {
            string sql = "UPDATE Menu_Acao SET Fl_Loja = " + (ativa ? "1" : "0") + " WHERE Cd_MenuAcao = " + cdmenuacao;
            //
            try
            {
                ExecutaSQLNonQuery(sql);
            }
            catch (Exception ex)
            {
                return Json(new { data = "erro", results = 1, success = true }, JsonRequestBehavior.AllowGet);
            }
            //
            return Json(new { data = "ok", results = 1, success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PermissoesCargo(int empresa, int loja, int empresa_acao, int loja_acao, string usuario, int menu)
        {
            AgroApp.Models.Usuario Usuario = PegaUsuario(empresa_acao, loja_acao, empresa_acao, loja_acao, usuario);
            //
            if (Usuario != null)
            {
                //List<AcaoModel> acao_model_list = PegaListaPermissoes(menu, empresa_acao);
                //List<Agro.Entities.Menu_Acao> menuAcao_list = PegaListaMenuAcao(menu, empresa_acao);
                //List<Agro.Entities.Menu_AcaoCargo> menu_AcaoCargo_list = PegaListaMenuAcaoCargo(empresa_acao, Usuario.Cd_Cargo, menuAcao_list);
                //List<Agro.Entities.Menu_AcaoUsuario> menu_AcaoUsuario_list = PegaListaMenuAcaoUsuario(empresa_acao, loja_acao, empresa_acao, loja_acao, usuario);
                ////               
                //foreach (AcaoModel acaoModel in acao_model_list)
                //{
                //    foreach (Agro.Entities.Menu_AcaoCargo menu_AcaoCargo in menu_AcaoCargo_list)
                //    {
                //        if (acaoModel.Cd_MenuAcao == menu_AcaoCargo.Id.Cd_MenuAcao)
                //        {
                //            acaoModel.Fl_Concedida = true;
                //            acaoModel.Fl_Grupo = true;
                //        }
                //    }
                //}
                ////
                //foreach (AcaoModel acaoModel in acao_model_list)
                //{
                //    foreach (Agro.Entities.Menu_AcaoUsuario menu_AcaoUsuario in menu_AcaoUsuario_list)
                //    {
                //        if (acaoModel.Cd_MenuAcao == menu_AcaoUsuario.Id.Cd_MenuAcao && menu_AcaoUsuario.Fl_Ativo && !acaoModel.Fl_Grupo)
                //        {
                //            acaoModel.Fl_Concedida = true;
                //            acaoModel.Fl_Especial = true;
                //        }
                //    }
                //}
                //         
                //return Json(new { data = acao_model_list.ToArray(), results = acao_model_list.Count, success = true }, JsonRequestBehavior.AllowGet);
                return Json(new { data = "cargo", results = 0, success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = "cargo", results = 0, success = true }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult PermissoesCargoModulo(int empresa, int ftCdEmpresa, int cargo, int menu)
        {
            List<AcaoModel> acao_model_list = PegaListaPermissoes(menu, ftCdEmpresa);
            List<AgroApp.Models.Menu_Acao> menuAcao_list = PegaListaMenuAcao(menu, ftCdEmpresa);
            List<AgroApp.Models.Menu_AcaoCargo> menu_AcaoCargo_list = PegaListaMenuAcaoCargo(ftCdEmpresa, cargo, menuAcao_list);
            //               
            //foreach (AcaoModel acaoModel in acao_model_list)
            //{
            //    foreach (AgroApp.Models.Menu_AcaoCargo menu_AcaoCargo in menu_AcaoCargo_list)
            //    {
            //        if (acaoModel.Cd_MenuAcao == menu_AcaoCargo.Id.Cd_MenuAcao)
            //        {
            //            acaoModel.Fl_Concedida = true;
            //            acaoModel.Fl_Grupo = true;
            //        }
            //    }
            //}
            return Json(new { data = acao_model_list.ToArray(), results = acao_model_list.Count, success = true }, JsonRequestBehavior.AllowGet);
        }

        private int PegaMenuAcaoId(int menu, int acao)
        {
            int iRet = 0;
            //
            //MenuAcaoMethods menuAcaoMethods = new MenuAcaoMethods();
            //Agro.Entities.Menu_Acao menu_Acao = menuAcaoMethods.FindAll(a => a.Cd_Acao == acao && a.Cd_Menu == menu).FirstOrDefault();
            ////
            //if (menu_Acao == null)
            //{
            //    Agro.Entities.Menu_Acao tmp = new Agro.Entities.Menu_Acao();
            //    tmp.Cd_Acao = acao;
            //    tmp.Cd_Menu = menu;
            //    menuAcaoMethods.Save(tmp);
            //    menu_Acao = menuAcaoMethods.FindAll(a => a.Cd_Acao == acao && a.Cd_Menu == menu).FirstOrDefault();
            //    if (menu_Acao != null)
            //    {
            //        iRet = menu_Acao.Cd_MenuAcao;
            //    }
            //}
            //else
            //{
            //    iRet = menu_Acao.Cd_MenuAcao;
            //}
            //
            return iRet;
        }

        private AgroApp.Models.Usuario PegaUsuario(int empresa, int loja, int empresa_acao, int loja_acao, string usuario)
        {
            AgroApp.Models.Usuario Usuario = dbCooperativa.Usuario.Where(a => a.Cd_Empresa == empresa_acao && a.Cd_Loja == loja_acao && a.Cd_Usuario.Equals(usuario.TrimEnd())).FirstOrDefault();
            if (Usuario != null)
            {
                return Usuario;
            }
            else
            {
                return null;
            }
        }

        private List<AcaoModel> PegaListaPermissoes(int cd_menu_pai, int empresa)
        {
            List<AgroApp.Models.Menu> menu_list = PegaListaMenus(cd_menu_pai);
            //
            List<AcaoModel> acao_list = dbCooperativa.Acao.Where(a => a.Fl_Ativo.HasValue && a.Fl_Ativo.Value)
                .Select(s => new AcaoModel()
                {
                    Cd_Acao = s.Cd_Acao,
                    Ds_Acao = s.Ds_Acao,
                    Ds_Acao_En = s.Ds_Acao_En,
                    Ds_Acao_Es = s.Ds_Acao_Es
                    //Fl_Ativo = s.Fl_Ativo
                })
                .ToList();
            //
            List<AcaoModel> acao_list_ret = new List<AcaoModel>();
            int i = 1;
            //
            foreach (AgroApp.Models.Menu menu in menu_list)
            {
                foreach (AcaoModel acaomodel in acao_list)
                {
                    AgroApp.Models.Menu_Acao menu_Acao = new AgroApp.Models.Menu_Acao();
                    //

                    if (empresa != 0)
                    {
                        menu_Acao = dbCooperativa.Menu_Acao.Where(a => a.Cd_Acao == acaomodel.Cd_Acao && a.Cd_Menu == menu.Cd_Menu).FirstOrDefault();
                    }
                    else
                    {
                        menu_Acao = dbCooperativa.Menu_Acao.Where(a => a.Cd_Acao == acaomodel.Cd_Acao && a.Cd_Menu == menu.Cd_Menu).FirstOrDefault();
                    }

                    //
                    if (menu_Acao != null)
                    {
                        if (menu_Acao.Cd_Menu == 27 && menu_Acao.Cd_Acao == 2)
                        {

                        }
                        AcaoModel tmp = new AcaoModel();
                        tmp.Cd_ID = i;
                        tmp.Cd_Acao = acaomodel.Cd_Acao;
                        tmp.Ds_Acao_En = acaomodel.Ds_Acao_En;
                        tmp.Ds_Acao_Es = acaomodel.Ds_Acao_Es;
                        tmp.Fl_Ativo = acaomodel.Fl_Ativo;
                        tmp.Fl_Concedida = acaomodel.Fl_Concedida;
                        tmp.Fl_Especial = acaomodel.Fl_Especial;
                        //
                        tmp.Cd_Menu = menu.Cd_Menu;
                        tmp.Ds_Acao = menu.Ds_Menu + " - " + acaomodel.Ds_Acao;
                        tmp.Ds_Acao_En = menu.Ds_Menu_En + " - " + acaomodel.Ds_Acao_En;
                        tmp.Ds_Acao_Es = menu.Ds_Menu_Es + " - " + acaomodel.Ds_Acao_Es;
                        //
                        tmp.Cd_MenuAcao = menu_Acao != null ? menu_Acao.Cd_MenuAcao : 0;
                        acao_list_ret.Add(tmp);
                        i++;
                    }

                }
            }
            //
            return acao_list_ret;
        }

        private List<AgroApp.Models.Menu_Acao> PegaListaMenuAcao(int menu, int empresa)
        {
            List<AgroApp.Models.Menu_Acao> menuAcao_list = new List<AgroApp.Models.Menu_Acao>();
            //List<Entities.Menu> menu_list = PegaListaMenus(menu);
            ////
            //string sql = string.Empty;
            ////
            //if (empresa != 0)
            //{
            //    int i = 0;
            //    sql = "SELECT * FROM Menu_Acao WHERE Fl_Loja = 1 AND ";
            //    //
            //    if (menu_list.Count > 0)
            //    {
            //        foreach (Entities.Menu menu_item in menu_list)
            //        {
            //            if (i == 0)
            //            {
            //                sql += " (Cd_Menu = " + menu_item.Cd_Menu;
            //            }
            //            else
            //            {
            //                sql += " OR Cd_Menu = " + menu_item.Cd_Menu;
            //            }
            //            //
            //            i++;
            //        }
            //        //
            //        sql += ")";
            //    }
            //}
            //else
            //{
            //    int i = 0;
            //    sql = "SELECT * FROM Menu_Acao WHERE ";
            //    //
            //    if (menu_list.Count > 0)
            //    {
            //        foreach (Entities.Menu menu_item in menu_list)
            //        {
            //            if (i == 0)
            //            {
            //                sql += " (Cd_Menu = " + menu_item.Cd_Menu;
            //            }
            //            else
            //            {
            //                sql += " OR Cd_Menu = " + menu_item.Cd_Menu;
            //            }
            //            //
            //            i++;
            //        }
            //        //
            //        sql += ")";
            //    }
            //}
            ////
            //DataTable oDataTable = ExecutaSQL(sql);
            ////
            //if (oDataTable.Rows != null && oDataTable.Rows.Count > 0)
            //{
            //    for (int i = 0; i < oDataTable.Rows.Count; i++)
            //    {
            //        Entities.Menu_Acao menu_acao_item = new Entities.Menu_Acao();
            //        //
            //        menu_acao_item.Cd_MenuAcao = Convert.ToInt32(oDataTable.Rows[i]["Cd_MenuAcao"]);
            //        menu_acao_item.Cd_Menu = Convert.ToInt32(oDataTable.Rows[i]["Cd_Menu"]);
            //        menu_acao_item.Cd_Acao = Convert.ToInt32(oDataTable.Rows[i]["Cd_Acao"]);
            //        menu_acao_item.Fl_Loja = string.IsNullOrEmpty(oDataTable.Rows[i]["Fl_Loja"].ToString()) ? 0 : 1;
            //        //
            //        menuAcao_list.Add(menu_acao_item);
            //    }
            //}
            //
            return menuAcao_list;
        }

        private List<AgroApp.Models.Menu> PegaListaMenus(int menu)
        {
            List<AgroApp.Models.Menu> menu_list = dbCooperativa.Menu.Where(a => a.Cd_Menu_Pai == menu).ToList();
            List<AgroApp.Models.Menu> menu_list_retorno = new List<AgroApp.Models.Menu>();
            //
            foreach (AgroApp.Models.Menu item in menu_list)
            {
                menu_list_retorno.Add(item);
                List<AgroApp.Models.Menu> menu_list_temp = dbCooperativa.Menu.Where(a => a.Cd_Menu_Pai == item.Cd_Menu).ToList();
                //
                foreach (AgroApp.Models.Menu item_filho in menu_list_temp)
                {
                    menu_list_retorno.Add(item_filho);
                }
            }
            //
            return menu_list_retorno;
        }

        private List<AgroApp.Models.Menu_AcaoCargo> PegaListaMenuAcaoCargo(int empresa, int? cargo, List<AgroApp.Models.Menu_Acao> menuAcao_list)
        {
            string where, where_menu = String.Empty;
            var values = new object[0];
            int paramNo = 0, numResults = 0;
            //
            where = "";
            values = new object[menuAcao_list.Count + 3];
            paramNo = 0;

            where += " Id.Cd_Empresa = @" + paramNo + " ";
            values[paramNo] = Convert.ToInt32(empresa);
            paramNo++;
            //
            //where += " AND Id.Cd_Loja = @" + paramNo + " ";
            //values[paramNo] = Convert.ToInt32(loja);
            //paramNo++;
            //
            where += " AND Id.Cd_Cargo = @" + paramNo + " ";
            values[paramNo] = Convert.ToInt32(cargo);
            paramNo++;
            //
            foreach (AgroApp.Models.Menu_Acao menu_acao in menuAcao_list)
            {
                if (where_menu != String.Empty) { where_menu += " OR "; }
                where_menu += " Id.Cd_MenuAcao = @" + paramNo + " ";
                values[paramNo] = Convert.ToInt32(menu_acao.Cd_MenuAcao);
                paramNo++;
            }
            where += " AND ( " + where_menu + " ) ";
            //
            List<AgroApp.Models.Menu_AcaoCargo> menu_AcaoCargo_list = new List<AgroApp.Models.Menu_AcaoCargo>();
            //
            //if (!string.IsNullOrEmpty(where_menu))
            //{
            //    MenuAcaoCargoMethods menuAcaoCargoMethods = new MenuAcaoCargoMethods();
            //    menu_AcaoCargo_list = menuAcaoCargoMethods.FindAll(where, values, out numResults).ToList();
            //}
            //
            return menu_AcaoCargo_list;
        }

        //private List<AgroApp.Models.Menu_AcaoUsuario> PegaListaMenuAcaoUsuario(int empresa, int loja, int empresa_acao, int loja_acao, string usuario)
        //{
        //    //MenuAcaoUsuarioMethods menuAcaoUsuarioMethods = new MenuAcaoUsuarioMethods();
        //    //List<Agro.Entities.Menu_AcaoUsuario> menu_AcaoUsuario_list = menuAcaoUsuarioMethods
        //    //    .FindAll(a => a.Id.Cd_Empresa == empresa_acao && a.Id.Cd_Loja == loja_acao && a.Id.Cd_Usuario.Equals(usuario)).ToList();
        //    //return menu_AcaoUsuario_list;
        //    return null;
        //}

        public JsonResult BuscaPermissoes(int empresa, int loja, int empresa_acao, int loja_acao, string usuario, int codigo_menu)
        {
            string sRet = string.Empty;
            //
            AgroApp.Models.Usuario Usuario = dbCooperativa.Usuario.Where(a => a.Cd_Empresa == empresa && a.Cd_Loja == loja && a.Cd_Usuario.TrimEnd().Equals(usuario.TrimEnd())).FirstOrDefault();
            //
            if (Usuario != null)
            {
                var lista_menu_acao = (from mac in dbCooperativa.Menu_AcaoCargo
                                       join ma in dbCooperativa.Menu_Acao on mac.Cd_MenuAcao equals ma.Cd_MenuAcao
                                       where mac.Cd_Empresa == empresa_acao && mac.Cd_Cargo == Usuario.Cd_Cargo && ma.Cd_Menu == codigo_menu
                                       select new Menu_AcaoModels { Cd_MenuAcao = mac.Cd_MenuAcao, Cd_Acao = ma.Cd_Acao, Cd_Menu = ma.Cd_Menu }).ToList();
                //
                if (usuario.ToLower().Equals("master") && (codigo_menu == 36 || codigo_menu == 46))
                {
                    lista_menu_acao = (from ma in dbCooperativa.Menu_Acao
                                       where ma.Cd_Menu == codigo_menu
                                       select new Menu_AcaoModels { Cd_MenuAcao = ma.Cd_MenuAcao, Cd_Acao = ma.Cd_Acao, Cd_Menu = ma.Cd_Menu }).ToList();
                }
                else
                {
                    lista_menu_acao = (from mac in dbCooperativa.Menu_AcaoCargo
                                       join ma in dbCooperativa.Menu_Acao on mac.Cd_MenuAcao equals ma.Cd_MenuAcao
                                       where mac.Cd_Empresa == empresa_acao && mac.Cd_Cargo == Usuario.Cd_Cargo && ma.Cd_Menu == codigo_menu
                                       select new Menu_AcaoModels { Cd_MenuAcao = mac.Cd_MenuAcao, Cd_Acao = ma.Cd_Acao, Cd_Menu = ma.Cd_Menu }).ToList();
                }

                //
                //var lista_menu_acao_usuario = (from mau in dbCooperativa.Menu_AcaoUsuario
                //                               join ma in dbCooperativa.Menu_Acao on mau.Cd_MenuAcao equals ma.Cd_MenuAcao
                //                               where mau.Cd_Empresa == empresa_acao && mau.Cd_Usuario.Equals(usuario.TrimEnd()) && mau.Fl_Ativo && ma.Cd_Menu == codigo_menu
                //                               select new Menu_AcaoModels { Cd_MenuAcao = mau.Cd_MenuAcao, Cd_Acao = ma.Cd_Acao, Cd_Menu = ma.Cd_Menu }).ToList();
                //
                //foreach (Agro.Application.Models.Menu_AcaoModels item in lista_menu_acao_usuario)
                //{
                //    lista_menu_acao.Add(item);
                //}
                ////
                //foreach (var item in lista_menu_acao)
                //{
                //    if (string.IsNullOrEmpty(sRet))
                //    {
                //        sRet = item.Cd_Acao.ToString();
                //    }
                //    else
                //    {
                //        sRet += "," + item.Cd_Acao.ToString();
                //    }
                //}
            }
            else
            {
                return Json(new { data = "usuario", results = 0, success = false }, JsonRequestBehavior.AllowGet);
            }
            //
            return Json(new { data = sRet, results = 1, success = true }, JsonRequestBehavior.AllowGet); ;
        }
        //
        #endregion

    }
}
