using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Agro.Application.Models;
using Agro.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Agro.Application.Controllers
{
    public class ClienteController : BaseController
    {
        public JsonResult Index(string _dc, int page, int start, int limit, string sort, string dir, int opcaoBusca, string textoBusca)
        {
            //
            int totalRegistros = 0;
            List<AgroApp.Models.Cliente> clienteList = Search(opcaoBusca, textoBusca, start, limit, page, sort, dir, out totalRegistros);
            //
            return Json(
                new
                {
                    data = clienteList.ToArray(),
                    results = 0,
                    success = true,
                    errors = String.Empty
                },
                JsonRequestBehavior.AllowGet
            );
        }

        public List<AgroApp.Models.Cliente> Search(int opcaoBusca, string textoBusca, int start, int limit, int page, string sort, string dir, out int totalRegistros)
        {
            string where = String.Empty;
            var values = new object[12];
            //
            List<AgroApp.Models.Cliente> clienteList = new List<AgroApp.Models.Cliente>();
            //
            if (!string.IsNullOrEmpty(textoBusca))
            {
                switch (opcaoBusca)
                {
                    case 1: // Código cliente
                        int icodigo = Convert.ToInt32(textoBusca);
                        clienteList = dbCooperativa.Cliente.Where(a => a.Codigo == icodigo).ToList();
                        break;

                    case 2: // Nome / Razão Social
                        clienteList = dbCooperativa.Cliente.Where(a => a.Nome.Contains(textoBusca)).ToList();
                        break;

                    case 3: // Endereço
                        clienteList = dbCooperativa.Cliente.Where(a => a.Endereco.Contains(textoBusca)).ToList();
                        break;

                    case 4: // Bairro
                        clienteList = dbCooperativa.Cliente.Where(a => a.Bairro.Contains(textoBusca)).ToList();
                        break;

                    case 5: // Cidade
                        clienteList = dbCooperativa.Cliente.Where(a => a.Cidade.Contains(textoBusca)).ToList();
                        break;

                    case 6: // Esrado
                        clienteList = dbCooperativa.Cliente.Where(a => a.Estado.Contains(textoBusca)).ToList();
                        break;

                    case 7: // Todos os Telefones
                        clienteList = dbCooperativa.Cliente.Where(a => a.Telefone1.Contains(textoBusca) || a.Telefone2.Contains(textoBusca)).ToList();
                        break;

                    case 8: // Email
                        clienteList = dbCooperativa.Cliente.Where(a => a.Email.Contains(textoBusca)).ToList();
                        break;
                }
            }
            else
            {
                clienteList = dbCooperativa.Cliente.Where(a => a.Codigo > 0).ToList();
            }
            //
            totalRegistros = clienteList.Count;
            //            
            return clienteList;
        }

        public JsonResult Save(int codigo,
                               string txtCliNome, string txtCliCPFCNPJ, string txtCliRGIE, string txtCliEndResTelefone1, string txtCliEndResTelefone2, string txtCliEndResEndereco,
                               string txtCliEndResNumero, string txtCliEndResComplemento, string txtCliEndResBairro, string txtCliEndResCEP, string txtCliEstado,
                               string txtCliCidade, string txtCliEmail)
        {
            string ret = string.Empty;
            try
            {
                AgroApp.Models.Cliente cliente = new AgroApp.Models.Cliente();
                //
                if (codigo == 0)
                {
                    cliente.DataCadastro = DateTime.Now;
                }
                //
                cliente.Nome = txtCliNome;
                cliente.Bairro = txtCliEndResBairro;
                cliente.Cep = txtCliEndResCEP;
                cliente.CgcCpf = txtCliCPFCNPJ;
                cliente.Cidade = txtCliCidade;
                cliente.CNPJTrabalho = txtCliCPFCNPJ;
                cliente.Email = txtCliEmail;
                cliente.Endereco = txtCliEndResEndereco;
                cliente.EnderecoCompl = txtCliEndResComplemento;
                cliente.EnderecoNum = txtCliEndResNumero;
                cliente.Estado = txtCliEstado;
                cliente.InscRG = txtCliRGIE;
                cliente.Telefone1 = txtCliEndResTelefone1;
                cliente.Telefone2 = txtCliEndResTelefone2;
                //
                if (codigo == 0)
                {
                    dbCooperativa.Cliente.Add(cliente);
                    dbCooperativa.SaveChanges();
                }
                else
                {
                    cliente.Codigo = codigo;
                    dbCooperativa.Entry(cliente).State = System.Data.Entity.EntityState.Modified;
                    dbCooperativa.SaveChanges();
                }
                ret = "ok";
            }
            catch (Exception exc)
            {
                ret = exc.Message;
            }
            //
            return Json(new { success = true, data = "ok", results = 1, errors = "" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete(int cdcodigo)
        {
            AgroApp.Models.Cliente cliente = dbCooperativa.Cliente.Where(a => a.Codigo == cdcodigo).FirstOrDefault();
            //
            if (cliente != null)
            {
                dbCooperativa.Cliente.Remove(cliente);
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

        ///// <param name="empresa">Código da Empresa loagada.</param>
        ///// <param name="loja">Código da Loja logada.</param>        
        ///// <param name="usuario">Identificação do Usuário logado.</param>   
        ///// <param name="cultura">Código da Cultura do Usuário logado. Possíveis valores: pt-BR, en-US, es-ES.</param>
        ///// <param name="ftCdEmpresa">Código da Empresa para filtro.</param>
        ///// <param name="ftCdLoja">Código da Loja para filtro.</param>        
        //public JsonResult Representante(int empresa, int loja, string usuario, string cultura, int ftCdEmpresa, int ftCdLoja, bool fllistarepresentantes, int cdrepresentante)
        //{
        //    List<RepresentanteModel> lista = new List<RepresentanteModel>();
        //    //
        //    string sql = @"
        //        SELECT Ds_Nome 
        //             , Ds_Usuario
        //          , Cd_Representante
        //          FROM Usuario
        //         WHERE (Cd_Empresa = {0} OR {0} = -1)                   
        //           AND Dt_Exclusao IS NULL
        //           AND Dt_Bloqueio IS NULL            
        //    ";

        //    sql = String.Format(
        //        sql,
        //        ftCdEmpresa, // 0
        //        ftCdLoja // 1
        //    );

        //    if (!fllistarepresentantes)
        //    {
        //        sql += String.Format(" AND Cd_Representante = {0}", cdrepresentante);
        //    }

        //    sql += " ORDER BY Ds_Usuario ";
        //    //
        //    DataTable oDataTable = ExecutaSQL(sql);
        //    //
        //    if (oDataTable.Rows != null && oDataTable.Rows.Count > 0)
        //    {
        //        int i = 1;
        //        //
        //        if (oDataTable.Rows.Count > 1)
        //        {
        //            RepresentanteModel representanteModel = new RepresentanteModel();
        //            //
        //            representanteModel.Cd_Codigo = -1;
        //            representanteModel.Ds_Representante = Resources.Resources.ComboboxTodos;
        //            representanteModel.Cd_Representante = "-1";
        //            //
        //            lista.Add(representanteModel);
        //        }
        //        //
        //        foreach (DataRow item in oDataTable.Rows)
        //        {
        //            RepresentanteModel representanteModel = new RepresentanteModel();
        //            //
        //            representanteModel.Cd_Codigo = Convert.ToInt32(item["cd_representante"].ToString());
        //            representanteModel.Ds_Representante = StringExtensions.Camelize("pt-BR", Convert.ToString(item["ds_usuario"]));
        //            representanteModel.Cd_Representante = item["cd_representante"].ToString();
        //            //
        //            lista.Add(representanteModel);
        //            i++;
        //        }
        //    }
        //    //
        //    return Json(
        //        new
        //        {
        //            data = lista.ToArray(),
        //            results = lista.Count,
        //            success = true
        //        },
        //        JsonRequestBehavior.AllowGet
        //    );
        //}

        //#region Buscas - PegaUsuarios - PegaNomeEquipe ...
        ////
        ///// <summary>
        /////     Método para retornar uma consulta de clientes.
        ///// </summary>
        ///// <param name="empresa">Código da empresa loagada.</param>
        ///// <param name="loja">Código da loja logada.</param>        
        ///// <param name="usuario">Identificação do usuário logado.</param>        
        ///// <param name="query">Texto que se deseja procurar.</param>        
        ///// <param name="limit">Quantidade limite de registros por página.</param>        
        ///// <param name="page">Número da página que se deseja visualizar.</param>        
        //public JsonResult FindByName(int empresa, int loja, int empresa_acao, int loja_acao, string usuario, string query, int limit, int page)
        //{
        //    ClienteMethods clienteMethods = new ClienteMethods(empresa_acao, usuario);
        //    List<Entities.Cliente> clienteList = new List<Entities.Cliente>();
        //    int numRegs = 0;

        //    if (!String.IsNullOrEmpty(query))
        //    {
        //        if (query.Length > 3)
        //        {
        //            clienteList = clienteMethods.FindAll(q => q.Id.CLI_Emp == empresa_acao
        //                                                   && q.Id.CLI_Loja == loja_acao
        //                                                   && q.CLI_Nome.Trim().Contains(query),
        //                                                   out numRegs,
        //                                                   (limit * (page - 1)),
        //                                                   limit,
        //                                                   "CLI_NOME");
        //        }
        //    }

        //    return Json(new { data = clienteList.ToArray(), results = numRegs, success = true }, JsonRequestBehavior.AllowGet);
        //}
        ////
        ///// <param name="empresa">Código da Empresa loagada.</param>
        ///// <param name="loja">Código da Loja logada.</param>        
        ///// <param name="usuario">Identificação do Usuário logado.</param>   
        ///// <param name="cultura">Código da Cultura do Usuário logado. Possíveis valores: pt-BR, en-US, es-ES.</param>	
        ///// <param name="ftCdEmpresa">Código da Empresa para filtro.</param>
        ///// <param name="ftCdLoja">Código da Loja para filtro.</param>        
        //public JsonResult FindByNameNovo(int empresa, int loja, string usuario, string cultura, int ftCdEmpresa, int ftCdLoja, string query, int limit, int page)
        //{
        //    ClienteMethods clienteMethods = new ClienteMethods(empresa, usuario);
        //    List<Entities.Cliente> clienteList = new List<Entities.Cliente>();
        //    int numRegs = 0;

        //    if (!String.IsNullOrEmpty(query))
        //    {
        //        if (query.Length > 3)
        //        {
        //            int n;
        //            bool isNumeric = int.TryParse(query, out n);
        //            //
        //            if (isNumeric)
        //            {
        //                clienteList = clienteMethods.FindAll(q => q.Id.CLI_Emp == ftCdEmpresa
        //                               && q.Id.CLI_Loja == ftCdLoja
        //                               && q.Id.CLI_Codigo.ToString().Contains(query),
        //                               out numRegs,
        //                               (limit * (page - 1)),
        //                               limit,
        //                               "CLI_NOME");
        //            }
        //            else
        //            {
        //                clienteList = clienteMethods.FindAll(q => q.Id.CLI_Emp == ftCdEmpresa
        //                               && q.Id.CLI_Loja == ftCdLoja
        //                               && q.CLI_Nome.Trim().Contains(query),
        //                               out numRegs,
        //                               (limit * (page - 1)),
        //                               limit,
        //                               "CLI_NOME");
        //            }
        //            //

        //        }
        //    }

        //    return Json(
        //        new
        //        {
        //            data = clienteList.ToArray(),
        //            results = numRegs,
        //            success = true,
        //            errors = String.Empty
        //        },
        //        JsonRequestBehavior.AllowGet
        //    );
        //}
        ////
        //public JsonResult PegaUsuarios(int empresa, int loja, int empresa_acao, int loja_acao, string usuario, string cultura, int empresaselecionada, int lojaselecionada, int equipe)
        //{
        //    UsuarioMethods UsuarioMethods = new UsuarioMethods(Convert.ToInt32(empresa), usuario);
        //    List<Entities.Usuario> Usuario_list = UsuarioMethods.FindAll(a => a.Id.Cd_Empresa == empresaselecionada).ToList();

        //    ClienteMethods clienteMethods = new ClienteMethods(Convert.ToInt32(empresa), usuario);
        //    var values = new object[5];

        //    List<Entities.Cliente> clienteList;

        //    if (equipe != -1)
        //    {
        //        clienteList = clienteMethods.
        //            FindAll(a => a.Id.CLI_Emp == empresaselecionada &&
        //                a.CLI_UserEquipe == equipe &&
        //                (a.Id.CLI_Loja == lojaselecionada || lojaselecionada == 0))
        //                .ToList();
        //    }
        //    else
        //    {
        //        clienteList = clienteMethods.
        //            FindAll(a => a.Id.CLI_Emp == empresaselecionada &&
        //                (a.Id.CLI_Loja == lojaselecionada || lojaselecionada == 0))
        //                .ToList();
        //    }

        //    List<UsuarioModel> UsuarioModel_List = new List<UsuarioModel>();
        //    UsuarioModel Usuario = new UsuarioModel();
        //    Usuario.Cd_Empresa = 0;
        //    Usuario.Ds_Usuario = "--Selecione--";
        //    Usuario.Cd_Usuario = "";
        //    UsuarioModel_List.Add(Usuario);

        //    foreach (Entities.Usuario user in Usuario_list)
        //    {
        //        foreach (Entities.Cliente cliente in clienteList)
        //        {
        //            if (cliente.CLI_UserCadastro == user.Cd_Representante)
        //            {
        //                if (!UsuarioModel_List.Any(a => a.Cd_Usuario.TrimEnd().Equals(user.Cd_Usuario.TrimEnd())))
        //                {
        //                    UsuarioModel UsuarioModel = new UsuarioModel();
        //                    UsuarioModel.Cd_Empresa = user.Id.Cd_Empresa;
        //                    UsuarioModel.Cd_Usuario = user.Id.Cd_Usuario.Trim();
        //                    UsuarioModel.Cd_Representante = (int)user.Cd_Representante;
        //                    UsuarioModel.Ds_Usuario = StringExtensions.Camelize("pt-BR", user.Ds_Usuario.ToLower());
        //                    UsuarioModel_List.Add(UsuarioModel);
        //                }
        //            }
        //        }
        //    }

        //    return Json(new
        //    {
        //        data = UsuarioModel_List.ToArray(),
        //        results = UsuarioModel_List.Count,
        //        success = true
        //    }, JsonRequestBehavior.AllowGet);
        //}
        ////
        //private string PegaNomeResponsavel(int empresa, string usuario)
        //{
        //    string sRet = string.Empty;
        //    //            
        //    Models.Usuario Usuario = dbElist.Usuario.Where(a => a.Cd_Empresa == empresa && a.Cd_Usuario.Equals(usuario.TrimEnd())).FirstOrDefault();
        //    if (Usuario != null)
        //    {
        //        sRet = Usuario.Ds_Usuario;
        //    }
        //    //
        //    return sRet;
        //}
        ////
        //private string PegaNomeProcedencia(int empresa, int? codigo)
        //{
        //    string sRet = string.Empty;
        //    //
        //    Models.ClienteProcedencia clienteProcedencia = dbElist.ClienteProcedencia.Where(q => q.CPR_Emp == empresa && q.CPR_Codigo == codigo).FirstOrDefault();
        //    if (clienteProcedencia != null)
        //    {
        //        sRet = clienteProcedencia.CPR_Descricao.ToUpper();
        //    }
        //    //
        //    return sRet;
        //}
        ////
        //private string PegaNomeEquipe(int empresa, string usuario, int loja, int codigo)
        //{
        //    string sRet = string.Empty;
        //    Equipe equipe = null;
        //    //
        //    if (this.Equipes_list == null)
        //    {
        //        this.Equipes_list = dbElist.Equipe.Where(a => a.EQP_Emp == empresa && a.EQP_Loja == loja).ToList();
        //        equipe = this.Equipes_list.FindAll(a => a.EQP_Emp == empresa && a.EQP_Loja == loja && a.EQP_Codigo == codigo).FirstOrDefault();
        //        if (equipe != null)
        //        {
        //            sRet = equipe.EQP_Descricao;
        //        }
        //        else
        //        {
        //            sRet = "";
        //        }
        //    }
        //    else
        //    {
        //        equipe = this.Equipes_list.FindAll(a => a.EQP_Emp == empresa && a.EQP_Loja == loja && a.EQP_Codigo == codigo).FirstOrDefault();
        //        if (equipe != null)
        //        {
        //            sRet = equipe.EQP_Descricao;
        //        }
        //        else
        //        {
        //            sRet = "";
        //        }
        //    }
        //    //
        //    sRet = sRet.ToLower().TrimEnd().TrimStart().Replace("equipe", "");
        //    return sRet.ToUpper();
        //}
        ////
        //private string PegaNomeusuarioCadastro(int empresa, int? representante, UsuarioMethods UsuarioMethods)
        //{
        //    string sRet = Agro.Resources.Resources.ClienteSemRepresentanteDefinido;
        //    //
        //    List<Models.Usuario> Usuario_list = dbElist.Usuario.Where(a => a.Cd_Empresa == empresa && a.Cd_Representante == representante).ToList();
        //    if (Usuario_list.Any())
        //    {
        //        sRet = Usuario_list.FirstOrDefault().Ds_Nome;
        //    }
        //    //
        //    return sRet;
        //}
        ////
        //private string PegaNomeRepresentante(int empresa, string usuario, int loja, int representante)
        //{
        //    string sRet = string.Empty;
        //    //
        //    UsuarioMethods UsuarioMethods = new UsuarioMethods(empresa, usuario);
        //    Entities.Usuario Usuario = UsuarioMethods.FindAll(a => a.Id.Cd_Empresa == empresa && a.Cd_Representante == representante).FirstOrDefault();
        //    if (Usuario == null)
        //    {
        //        return "";
        //    }
        //    sRet = Usuario.Ds_Usuario.Capitalize();
        //    //
        //    sRet = sRet.ToLower().TrimEnd().TrimStart();

        //    TextInfo textInfo = new CultureInfo(slanguage, false).TextInfo;
        //    sRet = textInfo.ToTitleCase(sRet);

        //    return sRet;
        //}
        ////
        //public int PegaCodigoCliente(int loja)
        //{
        //    int ultimoCodigo = 0;

        //    try
        //    {
        //        var consulta = dbElist.Cliente.Where(q => q.CLI_Loja == loja).Max(a => a.CLI_Codigo).ToString();

        //        if (consulta != null)
        //        {
        //            ultimoCodigo = Convert.ToInt32(consulta);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ultimoCodigo = 0;
        //    }

        //    ++ultimoCodigo;

        //    return ultimoCodigo;
        //}
        ////
        //// Retorna o próximo código de cliente que será utilizado na criação de novo cliente
        //public JsonResult Top(int loja, int loja_acao)
        //{
        //    long ultimoCodigo = 0;

        //    try
        //    {
        //        var consulta = dbElist.Cliente.Where(q => q.CLI_Loja == loja_acao).Max(a => a.CLI_Codigo).ToString();

        //        if (consulta != null)
        //        {
        //            ultimoCodigo = Convert.ToInt32(consulta);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ultimoCodigo = 0;
        //    }

        //    ++ultimoCodigo;

        //    return Json(new { data = ultimoCodigo, results = 1, success = true }, JsonRequestBehavior.AllowGet);
        //}
        //// 
        //public JsonResult Get(int empresa, int loja, string usuario, string cultura, int clienteEmpresa, int clienteLoja, int clienteCodigo)
        //{
        //    ClienteMethods clienteMethods = new ClienteMethods(empresa, usuario);

        //    List<Entities.Cliente> clienteList = clienteMethods.FindAll(q => q.Id.CLI_Emp == clienteEmpresa
        //                                             && q.Id.CLI_Loja == clienteLoja
        //                                             && q.Id.CLI_Codigo == clienteCodigo);

        //    List<ClienteModels> clienteModelsList = ToModel(empresa, usuario, clienteList);

        //    return Json(new
        //    {
        //        data = clienteModelsList.ToArray(),
        //        results = clienteModelsList.Count,
        //        success = true,
        //        errors = String.Empty
        //    }, JsonRequestBehavior.AllowGet);
        //}
        ////
        //public JsonResult CamposObrigatorios(int empresa, int loja, int empresa_acao, int loja_acao, string cultura)
        //{
        //    CamposObrigatoriosMethods camposObrigatoriosMethods = new CamposObrigatoriosMethods();
        //    //
        //    List<CamposObrigatoriosModels> camposObrigatoriosModels_list = new List<CamposObrigatoriosModels>();
        //    //
        //    List<CamposObrigatoriosModels> camposObrigatoriosModels_list_geral = camposObrigatoriosMethods
        //                                            .FindAll(a => a.CD_EMPRESA == 0 && a.CD_LOJA == 0 && a.ORDEM != null && a.OBRIGATORIO_CADASTRO && a.CADASTRO.Equals("CLIENTE"))
        //                                            .Select(s => new CamposObrigatoriosModels()
        //                                            {
        //                                                ID = s.ID,
        //                                                CD_LOJA = s.CD_LOJA,
        //                                                CD_EMPRESA = s.CD_EMPRESA,
        //                                                GRUPO = s.GRUPO,
        //                                                NOME = s.NOME,
        //                                                OBRIGATORIO_CADASTRO = s.OBRIGATORIO_CADASTRO,
        //                                                NOME_CAMPO = s.NOME_CAMPO,
        //                                                ORDEM = s.ORDEM,
        //                                                CADASTRO = s.CADASTRO
        //                                            })
        //                                            .OrderBy(a => a.ORDEM)
        //                                            .ToList();
        //    //
        //    List<CamposObrigatoriosModels> camposObrigatoriosModels_list_especifica = camposObrigatoriosMethods
        //                                .FindAll(a => a.CD_EMPRESA == empresa_acao && a.CD_LOJA == loja_acao && a.ORDEM != null && a.CADASTRO.Equals("CLIENTE"))
        //                                .Select(s => new CamposObrigatoriosModels()
        //                                {
        //                                    ID = s.ID,
        //                                    CD_LOJA = s.CD_LOJA,
        //                                    CD_EMPRESA = s.CD_EMPRESA,
        //                                    GRUPO = s.GRUPO,
        //                                    NOME = s.NOME,
        //                                    OBRIGATORIO_CADASTRO = s.OBRIGATORIO_CADASTRO,
        //                                    NOME_CAMPO = s.NOME_CAMPO,
        //                                    ORDEM = s.ORDEM,
        //                                    CADASTRO = s.CADASTRO
        //                                })
        //                                .OrderBy(a => a.ORDEM)
        //                                .ToList();
        //    //
        //    if (camposObrigatoriosModels_list_especifica != null && camposObrigatoriosModels_list_especifica.Count > 0)
        //    {
        //        foreach (CamposObrigatoriosModels geral in camposObrigatoriosModels_list_geral)
        //        {
        //            foreach (CamposObrigatoriosModels especifico in camposObrigatoriosModels_list_especifica)
        //            {
        //                if (geral.NOME_CAMPO.Equals(especifico.NOME_CAMPO))
        //                {
        //                    camposObrigatoriosModels_list.Add(especifico);
        //                }
        //                else
        //                {
        //                    camposObrigatoriosModels_list.Add(geral);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return Json(new
        //        {
        //            success = true,
        //            data = camposObrigatoriosModels_list_geral.ToArray(),
        //            results = camposObrigatoriosModels_list_geral.Count,
        //            errors = ""
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    //
        //    return Json(new
        //    {
        //        success = true,
        //        data = camposObrigatoriosModels_list.ToArray(),
        //        results = camposObrigatoriosModels_list.Count,
        //        errors = ""
        //    }, JsonRequestBehavior.AllowGet);
        //}
        ////
        //private string PegaTipoResidencia(long? tipo)
        //{
        //    if (tipo == 0)
        //    {
        //        return Agro.Resources.Resources.TipoResidenciaPropria.ToUpper();
        //    }
        //    return Agro.Resources.Resources.TipoResidenciaAlugada.ToUpper();
        //}
        ////
        //private string PegaEstadoCivil(long? tipo)
        //{
        //    switch (tipo)
        //    {
        //        case 0: return Agro.Resources.Resources.EstadoCivilSolteiro.ToUpper();
        //        case 1: return Agro.Resources.Resources.EstadoCivilCasado.ToUpper();
        //        case 2: return Agro.Resources.Resources.EstadoCivilDivorciado.ToUpper();
        //        case 3: return Agro.Resources.Resources.EstadoCivilViuvo.ToUpper();
        //        case 4: return Agro.Resources.Resources.EstadoCivilUniaoEstavel.ToUpper();
        //    }
        //    return "";
        //}
        ////
        //private string PegaObservacoesCliente(int empresa, int loja, int empresa_acao, int loja_acao, int cliente_id)
        //{
        //    string sRet = string.Empty;
        //    //
        //    ClienteObservacoesMethods clienteObservacoesMethods = new ClienteObservacoesMethods();
        //    //
        //    List<ClienteObservacoesModels> ClienteObservacoes_list = clienteObservacoesMethods
        //        .FindAll(a => a.Cd_Emppresa == empresa_acao && a.Cd_Loja == loja_acao && a.Cd_Cliente == cliente_id)
        //        .OrderByDescending(a => a.Criado_Em)
        //        .Select(a => new ClienteObservacoesModels()
        //        {
        //            Id = a.Id,
        //            Criado_Em = a.Criado_Em,
        //            Observacoes = a.Observacoes,
        //            Responsavel = PegaNomeResponsavel(a.Cd_Emppresa, a.Cd_Usuario)
        //        })
        //        .ToList();
        //    //
        //    foreach (ClienteObservacoesModels item in ClienteObservacoes_list)
        //    {
        //        sRet += String.Format("<p align='justify'><font color='blue'>{0} - {1} - </font>{2}</p>"
        //                    , item.Criado_Em.ToString()
        //                    , item.Responsavel
        //                    , item.Observacoes);
        //    }
        //    //
        //    return sRet;
        //}
        ////
        //public JsonResult PegaTipoPessoa(int empresa, int loja, int empresa_acao, int loja_acao, int codigo)
        //{
        //    Models.Cliente cliente = dbElist.Cliente.Where(q => q.CLI_Emp == empresa_acao && q.CLI_Loja == loja_acao && q.CLI_Codigo == codigo).FirstOrDefault();
        //    string CLI_FisJur = string.Empty;
        //    //
        //    if (cliente != null)
        //    {
        //        CLI_FisJur = cliente.CLI_FisJur;
        //    }
        //    //
        //    return Json(new { data = CLI_FisJur, results = 1, success = true }, JsonRequestBehavior.AllowGet);
        //}
        ////
        //#endregion

        //#region validações - ValidaPedidoFabrica - ValidaCPFDuplicado ...
        ////
        //public JsonResult ValidaPedidoFabrica(int empresa, int loja, int empresa_acao, int loja_acao, string usuario, int filtroempresa, int filtroloja, int cliente)
        //{
        //    List<Models.Projeto> lista_projetos = dbElist.Projeto.Where(a => a.PRO_Emp == filtroempresa && a.PRO_Loja == filtroloja && a.PRO_Cliente == cliente && (a.PRO_DataRomaneio != null || a.PRO_RomaneioI > 0)).ToList();

        //    return Json(new { data = "", results = 1, success = lista_projetos.Count > 0 }, JsonRequestBehavior.AllowGet);
        //}
        ////
        //public bool ValidaPedidoFabricaLocal(int empresa, int loja, int empresa_acao, int loja_acao, string usuario, int cliente)
        //{
        //    ProjetoMethods projetoMethods = new ProjetoMethods(empresa_acao, usuario);
        //    //
        //    List<Entities.Projeto> lista_projetos = projetoMethods.FindAll(a => a.Id.PRO_Emp == empresa_acao && a.Id.PRO_Loja == loja_acao && a.Id.PRO_Cliente == cliente && (a.PRO_DataRomaneio != null || a.PRO_RomaneioI > 0));
        //    //
        //    return lista_projetos.Count > 0;
        //}
        ////
        //public JsonResult ValidaCPFDuplicado(int empresa, string usuario, int loja, string cpf, string codigo)
        //{
        //    if (cpf == null || cpf.Length == 0)
        //    {
        //        return Json(new { data = false, results = 1, success = true, dados = "" }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        ClienteMethods clientesMethods = new ClienteMethods(empresa, usuario);
        //        bool bRet = false;
        //        string dados = "";
        //        //
        //        List<Entities.Cliente> cliente_list = null;
        //        //
        //        if (string.IsNullOrEmpty(codigo))
        //        {
        //            cliente_list = clientesMethods.FindAll(a => a.Id.CLI_Emp == empresa && a.Id.CLI_Loja == loja && a.CLI_CgcCpf.Equals(cpf));
        //        }
        //        else
        //        {
        //            cliente_list = clientesMethods.FindAll(a => a.Id.CLI_Emp == empresa && a.Id.CLI_Loja == loja && a.CLI_CgcCpf.Trim() == cpf.Trim() && a.Id.CLI_Codigo != Convert.ToInt32(codigo)).ToList();
        //        }
        //        //
        //        if (cliente_list.Any())
        //        {
        //            bRet = true;
        //            foreach (Entities.Cliente item in cliente_list)
        //            {
        //                dados += item.CLI_Codigo + " - " + item.CLI_Nome + "<br />";
        //            }
        //        }
        //        return Json(new { data = bRet, results = 1, success = true, dados = dados }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        ////
        //public string ValidaIE(string ie, string uf)
        //{
        //    try
        //    {
        //        if (ConsisteInscricaoEstadual(ie, uf) == 1)
        //        {
        //            return Resources.Resources.InscricaoEstadualIncorreta;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Resources.Resources.ErroValidacaoIE;
        //    }
        //    return "";
        //}
        ////
        //private bool ValidaCampo(object objeto)
        //{
        //    bool bRet = true;
        //    //
        //    try
        //    {
        //        if (objeto.ToString().Length > 0)
        //        {
        //            if (objeto.ToString().Equals("DDD") || objeto.ToString().Equals("0"))
        //            {
        //                return true;
        //            }
        //            return false;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    //
        //    return bRet;
        //}
        ////
        //private bool ValidaCampoIE(object objeto)
        //{
        //    bool bRet = true;
        //    //
        //    try
        //    {
        //        if (objeto.ToString().Length > 0)
        //        {
        //            if (objeto.ToString().ToLower().Equals("isento"))
        //            {
        //                return true;
        //            }
        //            return false;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    //
        //    return bRet;
        //}
        ////
        //private bool ValidaClienteUsaEfinance(int empresa, int loja, int empresa_acao, int loja_acao, Rom_LojaMethods rom_LojaMethods)
        //{
        //    Entities.Rom_Loja rom_Loja = rom_LojaMethods.FindAll(a => a.Id.Cd_Empresa == empresa_acao && a.Id.Cd_Loja == loja_acao).FirstOrDefault();
        //    if (rom_Loja != null)
        //    {
        //        if (rom_Loja.Fl_UsaEFinance.HasValue)
        //        {
        //            if (rom_Loja.Fl_UsaEFinance.Value)
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    //
        //    return false;
        //}
        ////
        //#endregion

        //#region Genéricas - Export - ToDataSet - ToModel ...
        ////
        //private string AtualizaClienteNoElist(Entities.Cliente entity, string empresa, string usuario, string loja)
        //{
        //    string sRet = string.Empty;
        //    DataTable dtCliente = new DataTable("Table");
        //    //
        //    #region Adiciona Colunas
        //    //
        //    dtCliente.Columns.Add("CLI_EmpFab");            // 01
        //    dtCliente.Columns.Add("CLI_Emp");               // 02
        //    dtCliente.Columns.Add("CLI_Loja");              // 03
        //    dtCliente.Columns.Add("CLI_Codigo");            // 04
        //    dtCliente.Columns.Add("CLI_Nome");              // 05
        //    dtCliente.Columns.Add("CLI_SeqProjeto");        // 06
        //    dtCliente.Columns.Add("CLI_Endereco");          // 07
        //    dtCliente.Columns.Add("CLI_Bairro");            // 08
        //    dtCliente.Columns.Add("CLI_Cidade");            // 09
        //    dtCliente.Columns.Add("CLI_Cep");               // 10
        //    dtCliente.Columns.Add("CLI_Estado");            // 11
        //    dtCliente.Columns.Add("CLI_Procedencia");       // 12
        //    dtCliente.Columns.Add("CLI_CgcCpf");            // 13
        //    dtCliente.Columns.Add("CLI_InscRG");            // 14
        //    dtCliente.Columns.Add("CLI_FisJur");            // 15
        //    dtCliente.Columns.Add("CLI_EntEndereco");       // 16
        //    dtCliente.Columns.Add("CLI_EntBairro");         // 17
        //    dtCliente.Columns.Add("CLI_EntCidade");         // 18
        //    dtCliente.Columns.Add("CLI_EntCep");            // 19
        //    dtCliente.Columns.Add("CLI_EntEstado");         // 20
        //    dtCliente.Columns.Add("CLI_EntCgcCpf");         // 21
        //    dtCliente.Columns.Add("CLI_EntInscRG");         // 22
        //    dtCliente.Columns.Add("CLI_CobEndereco");       // 23
        //    dtCliente.Columns.Add("CLI_CobBairro");         // 24
        //    dtCliente.Columns.Add("CLI_CobCidade");         // 25
        //    dtCliente.Columns.Add("CLI_CobCep");            // 26
        //    dtCliente.Columns.Add("CLI_CobEstado");         // 27
        //    dtCliente.Columns.Add("CLI_CobCgcCpf");         // 28
        //    dtCliente.Columns.Add("CLI_CobInscRG");         // 29
        //    dtCliente.Columns.Add("CLI_DataCadastro");      // 30
        //    dtCliente.Columns.Add("CLI_HoraCadastro");      // 31
        //    dtCliente.Columns.Add("CLI_UserCadastro");      // 32
        //    dtCliente.Columns.Add("CLI_DataAltera");        // 33
        //    dtCliente.Columns.Add("CLI_HoraAltera");        // 34
        //    dtCliente.Columns.Add("CLI_UserAltera");        // 35
        //    dtCliente.Columns.Add("CLI_UserEquipe");        // 36
        //    dtCliente.Columns.Add("CLI_Obs");               // 37
        //    dtCliente.Columns.Add("CLI_DataObra");          // 38
        //    dtCliente.Columns.Add("CLI_EnderecoNum");       // 39
        //    dtCliente.Columns.Add("CLI_EnderecoCompl");     // 40
        //    dtCliente.Columns.Add("CLI_EntEnderecoNum");    // 41
        //    dtCliente.Columns.Add("CLI_EntEnderecoCompl");  // 42
        //    dtCliente.Columns.Add("CLI_CobEnderecoNum");    // 43
        //    dtCliente.Columns.Add("CLI_CobEnderecoCompl");  // 44
        //    dtCliente.Columns.Add("CLI_CodCidadeIBGE");     // 45
        //    dtCliente.Columns.Add("CLI_CobCodCidadeIBGE");  // 46
        //    dtCliente.Columns.Add("CLI_EntCodCidadeIBGE");  // 47
        //    dtCliente.Columns.Add("CLI_Contribuinte");      // 48
        //    dtCliente.Columns.Add("CLI_PrevisaoFechamento");// 49
        //    dtCliente.Columns.Add("CLI_Suframa");           // 50
        //    dtCliente.Columns.Add("CLI_Email");             // 51
        //    dtCliente.Columns.Add("CLI_EntEdificio");       // 52
        //    dtCliente.Columns.Add("CLI_EntReferencia");     // 53  
        //    dtCliente.Columns.Add("CLI_CorFone1");          // 54
        //    dtCliente.Columns.Add("CLI_CorFone2");          // 55
        //    dtCliente.Columns.Add("CLI_DataNascimento");    // 56
        //    dtCliente.Columns.Add("CLI_CobFone1");          // 57
        //    dtCliente.Columns.Add("CLI_CobFone2");          // 58
        //    dtCliente.Columns.Add("CLI_EntFone1");          // 59
        //    dtCliente.Columns.Add("CLI_EntFone2");          // 60          
        //    dtCliente.Columns.Add("CLI_Profissao");         // 61
        //    dtCliente.Columns.Add("CLI_DDD_CorFone1");      // 62
        //    dtCliente.Columns.Add("CLI_DDD_CorFone2");      // 63
        //    dtCliente.Columns.Add("CLI_DDD_CobFone1");      // 64
        //    dtCliente.Columns.Add("CLI_DDD_CobFone2");      // 65			
        //    dtCliente.Columns.Add("CLI_Agencia");           // 66
        //    //dtCliente.Columns.Add("CLI_AgenciaDigito");     // 67
        //    dtCliente.Columns.Add("CLI_Banco");             // 68
        //    // dtCliente.Columns.Add("CLI_CadastroCompleto");  // 69
        //    dtCliente.Columns.Add("CLI_CNPJTrabalho");      // 70
        //    dtCliente.Columns.Add("CLI_CobDataAdmissao");   // 71
        //    dtCliente.Columns.Add("CLI_CobEstadoCivil");    // 72
        //    dtCliente.Columns.Add("CLI_CobNacionalidade");  // 73
        //    dtCliente.Columns.Add("CLI_CobNaturalidade");   // 74
        //    dtCliente.Columns.Add("CLI_CobNomeEmpresa");    // 75
        //    dtCliente.Columns.Add("CLI_CobNomeMae");        // 76
        //    dtCliente.Columns.Add("CLI_CobNomePai");        // 77
        //    dtCliente.Columns.Add("CLI_CobOcupacao");       // 78
        //    dtCliente.Columns.Add("CLI_CobPais");           // 79
        //    dtCliente.Columns.Add("CLI_CobTelefoneEmpresa");// 80
        //    dtCliente.Columns.Add("CLI_CobTempoResidencia");// 81
        //    dtCliente.Columns.Add("CLI_CobTipoResidencia"); // 82
        //    dtCliente.Columns.Add("CLI_CodigoFabrica");     // 83
        //    dtCliente.Columns.Add("CLI_ComBairro");         // 84
        //    dtCliente.Columns.Add("CLI_ComCEP");            // 85
        //    dtCliente.Columns.Add("CLI_ComCidade");         // 86
        //    dtCliente.Columns.Add("CLI_ComCodCidadeIBGE");  // 87
        //    dtCliente.Columns.Add("CLI_ComEndereco");       // 88
        //    dtCliente.Columns.Add("CLI_ComEnderecoCompl");  // 89
        //    dtCliente.Columns.Add("CLI_ComEnderecoNum");    // 90
        //    dtCliente.Columns.Add("CLI_ComEstado");         // 91
        //    dtCliente.Columns.Add("CLI_ComPais");           // 92
        //    dtCliente.Columns.Add("CLI_ConjugeCNPJTrabalho");   // 93
        //    dtCliente.Columns.Add("CLI_ConjugeCPF");        // 94
        //    dtCliente.Columns.Add("CLI_ConjugeDataNascimento"); // 95
        //    dtCliente.Columns.Add("CLI_ConjugeFone");       // 96
        //    dtCliente.Columns.Add("CLI_ConjugeNome");       // 97
        //    dtCliente.Columns.Add("CLI_ConjugeNomeEmpresa");// 98
        //    dtCliente.Columns.Add("CLI_ConjugeRendaMensal");// 99
        //    dtCliente.Columns.Add("CLI_ConjugeRG");         // 100
        //    dtCliente.Columns.Add("CLI_CorBairro");         // 101
        //    dtCliente.Columns.Add("CLI_CorCep");            // 102
        //    dtCliente.Columns.Add("CLI_CorCidade");         // 103
        //    dtCliente.Columns.Add("CLI_CorEndereco");       // 104
        //    dtCliente.Columns.Add("CLI_CorEstado");         // 105
        //    dtCliente.Columns.Add("CLI_CorFax");            // 106
        //    //dtCliente.Columns.Add("CLI_CorPais");           // 107
        //    dtCliente.Columns.Add("CLI_CTABanco");          // 108
        //    //dtCliente.Columns.Add("CLI_CTABancoDigito");    // 109
        //    dtCliente.Columns.Add("CLI_CTADesde");          // 110
        //    dtCliente.Columns.Add("CLI_DataEmissaoRG");     // 111
        //    dtCliente.Columns.Add("CLI_DataEnvio");         // 112
        //    dtCliente.Columns.Add("CLI_DataRomaneio");      // 113
        //    //dtCliente.Columns.Add("CLI_DDD_CobFax");        // 114
        //    dtCliente.Columns.Add("CLI_DDD_CobTelefoneEmpresa");    // 115
        //    dtCliente.Columns.Add("CLI_DDD_ConjugeFone");   // 116		
        //    dtCliente.Columns.Add("CLI_DDD_EntFax");        // 117
        //    dtCliente.Columns.Add("CLI_DDD_EntFone1");      // 118
        //    dtCliente.Columns.Add("CLI_DDD_EntFone2");      // 119
        //    dtCliente.Columns.Add("CLI_DDD_FoneAgencia");   // 120
        //    dtCliente.Columns.Add("CLI_DDD_TelReferencia1");// 121
        //    dtCliente.Columns.Add("CLI_DDD_TelReferencia2");// 122
        //    dtCliente.Columns.Add("CLI_EntPais");           // 123
        //    dtCliente.Columns.Add("CLI_FoneAgencia");       // 124
        //    dtCliente.Columns.Add("CLI_NomeReferencia1");   // 125
        //    dtCliente.Columns.Add("CLI_NomeReferencia2");   // 126
        //    dtCliente.Columns.Add("CLI_OrgEmissorRG");      // 127
        //    dtCliente.Columns.Add("CLI_Pais");              // 128
        //    dtCliente.Columns.Add("CLI_RendaMensal");       // 129
        //    dtCliente.Columns.Add("CLI_Status");            // 130
        //    dtCliente.Columns.Add("CLI_TelReferencia1");    // 131
        //    dtCliente.Columns.Add("CLI_TelReferencia2");    // 132
        //    dtCliente.Columns.Add("CLI_Tipo");              // 133
        //    //
        //    #endregion
        //    //
        //    #region Adiciona dados
        //    //
        //    dtCliente.Rows.Add(
        //          entity.Id.CLI_Emp         // 01
        //        , entity.Id.CLI_Emp         // 02
        //        , entity.Id.CLI_Loja        // 03
        //        , entity.CLI_Codigo      // 04
        //        , entity.CLI_Nome.Length > 40 ? entity.CLI_Nome.Substring(0, 40) : entity.CLI_Nome // 05
        //        , entity.CLI_SeqProjeto     // 06
        //        , entity.CLI_Endereco       // 07
        //        , entity.CLI_Bairro         // 08
        //        , entity.CLI_Cidade         // 09
        //        , entity.CLI_Cep            // 10 
        //        , entity.CLI_Estado         // 11
        //        , entity.CLI_Procedencia    // 12
        //        , entity.CLI_CgcCpf         // 13
        //        , entity.CLI_InscRG         // 14
        //        , entity.CLI_FisJur         // 15 
        //        , entity.CLI_EntEndereco    // 16
        //        , entity.CLI_EntBairro      // 17
        //        , entity.CLI_EntCidade      // 18
        //        , entity.CLI_EntCep         // 19
        //        , entity.CLI_EntEstado      // 20
        //        , entity.CLI_EntCgcCpf      // 21 
        //        , entity.CLI_EntInscRG      // 22
        //        , entity.CLI_CobEndereco    // 23
        //        , entity.CLI_CobBairro      // 24
        //        , entity.CLI_CobCidade      // 25
        //        , entity.CLI_CobCep         // 26
        //        , entity.CLI_CobEstado      // 27
        //        , entity.CLI_CobCgcCpf      // 28
        //        , entity.CLI_CobInscRG      // 29
        //        , entity.CLI_DataCadastro == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_DataCadastro.Value.Date) //!= null ? String.Format(FormatoData, entity.CLI_DataCadastro) : null  // 30
        //        , entity.CLI_HoraCadastro == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_HoraCadastro.Value) // != null ? String.Format(FormatoData, entity.CLI_HoraCadastro) : null  // 31
        //        , entity.CLI_UserCadastro   // 32
        //        , entity.CLI_DataAltera == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_DataAltera.Value.Date)  //!= null ? String.Format(FormatoData, entity.CLI_DataAltera) : null      // 33
        //        , entity.CLI_HoraAltera == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_HoraAltera.Value)  //!= null ? String.Format(FormatoData, entity.CLI_HoraAltera) : null      // 34
        //        , entity.CLI_UserAltera     // 35
        //        , entity.CLI_UserEquipe     // 36
        //        , entity.CLI_Obs            // 37 
        //        , entity.CLI_DataObra == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_DataObra.Value.Date)  //!= null ? String.Format(FormatoData, entity.CLI_DataObra) : null          // 38
        //        , entity.CLI_EnderecoNum    // 39
        //        , entity.CLI_EnderecoCompl  // 40
        //        , entity.CLI_EntEnderecoNum // 41
        //        , entity.CLI_EntEnderecoCompl   // 42
        //        , entity.CLI_CobEnderecoNum     // 43
        //        , entity.CLI_CobEnderecoCompl   // 44                              
        //        , entity.CLI_CodCidadeIBGE      // 45
        //        , entity.CLI_CobCodCidadeIBGE   // 46 
        //        , entity.CLI_EntCodCidadeIBGE   // 47
        //        , entity.CLI_Contribuinte       // 48
        //        , entity.CLI_PrevisaoFechamento == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_PrevisaoFechamento.Value.Date)  //!= null ? String.Format(FormatoData, entity.CLI_PrevisaoFechamento) : null // 49
        //        , entity.CLI_Suframa            // 50
        //        , entity.CLI_Email.Length > 40 ? entity.CLI_Email.Substring(0, 40) : entity.CLI_Email // 51
        //        , entity.CLI_EntEdificio        // 52
        //        , entity.CLI_EntReferencia      // 53
        //        , entity.CLI_CorFone1           // 54
        //        , entity.CLI_CorFone2           // 55
        //        , entity.CLI_DataNascimento == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_DataNascimento.Value.Date)  //!= null ? String.Format(FormatoData, entity.CLI_DataNascimento) : null      // 56
        //        , entity.CLI_CobFone1           // 57
        //        , entity.CLI_CobFone2           // 58
        //        , entity.CLI_EntFone1           // 59
        //        , entity.CLI_EntFone2           // 60
        //        , entity.CLI_Profissao          // 61
        //        , entity.CLI_DDD_CorFone1       // 62
        //        , entity.CLI_DDD_CorFone2       // 63
        //        , entity.CLI_DDD_CobFone1       // 64
        //        , entity.CLI_DDD_CobFone2       // 65
        //        , entity.CLI_Agencia            // 66
        //                                        //, entity.CLI_AgenciaDigito      // 67
        //        , entity.CLI_Banco              // 68
        //                                        //, entity.CLI_CadastroCompleto   // 69
        //        , entity.CLI_CNPJTrabalho       // 70
        //        , entity.CLI_CobDataAdmissao == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_CobDataAdmissao.Value.Date)  //!= null ? String.Format(FormatoData, entity.CLI_CobDataAdmissao) : null      // 71
        //        , entity.CLI_CobEstadoCivil != null ? entity.CLI_CobEstadoCivil : 0   // 72
        //        , entity.CLI_CobNacionalidade   // 73
        //        , entity.CLI_CobNaturalidade    // 74
        //        , entity.CLI_CobNomeEmpresa     // 75
        //        , entity.CLI_CobNomeMae         // 76
        //        , entity.CLI_CobNomePai         // 77
        //        , entity.CLI_CobOcupacao        // 78
        //        , entity.CLI_CobPais            // 79
        //        , entity.CLI_CobTelefoneEmpresa // 80
        //        , entity.CLI_CobTempoResidencia // 81
        //        , entity.CLI_CobTipoResidencia  // 82
        //        , entity.CLI_CodigoFabrica      // 83
        //        , entity.CLI_ComBairro          // 84
        //        , entity.CLI_ComCEP             // 85
        //        , entity.CLI_ComCidade          // 86
        //        , entity.CLI_ComCodCidadeIBGE   // 87
        //        , entity.CLI_ComEndereco        // 88
        //        , entity.CLI_ComEnderecoCompl   // 89
        //        , entity.CLI_ComEnderecoNum     // 90
        //        , entity.CLI_ComEstado          // 91
        //        , entity.CLI_ComPais            // 92
        //        , entity.CLI_ConjugeCNPJTrabalho// 93
        //        , entity.CLI_ConjugeCPF         // 94
        //        , entity.CLI_ConjugeDataNascimento == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_ConjugeDataNascimento.Value.Date)  //!= null ? String.Format(FormatoData, entity.CLI_ConjugeDataNascimento) : null                // 95
        //        , entity.CLI_ConjugeFone        // 96
        //        , entity.CLI_ConjugeNome        // 97
        //        , entity.CLI_ConjugeNomeEmpresa // 98
        //        , entity.CLI_ConjugeRendaMensal // 99
        //        , entity.CLI_ConjugeRG          // 100
        //        , entity.CLI_CorBairro          // 101
        //        , entity.CLI_CorCep             // 102
        //        , entity.CLI_CorCidade          // 103
        //        , entity.CLI_CorEndereco        // 104
        //        , entity.CLI_CorEstado          // 105
        //        , entity.CLI_CorFax             // 106
        //                                        //, entity.CLI_CorPais            // 107
        //        , entity.CLI_CTABanco           // 108
        //                                        //, entity.CLI_CTABancoDigito     // 109
        //        , entity.CLI_CTADesde != null ? String.Format(FormatoData, entity.CLI_CTADesde) : null                // 110
        //        , entity.CLI_DataEmissaoRG == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_DataEmissaoRG.Value.Date)  //!= null ? String.Format(FormatoData, entity.CLI_DataEmissaoRG) : null      // 111
        //        , entity.CLI_DataEnvio == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_DataEnvio.Value.Date)  //!= null ? String.Format(FormatoData, entity.CLI_DataEnvio) : null              // 112
        //        , entity.CLI_DataRomaneio == null ? null : String.Format("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss}", entity.CLI_DataRomaneio.Value.Date)  //!= null ? String.Format(FormatoData, entity.CLI_DataRomaneio) : null        // 113
        //                                                                                                                                           //, entity.CLI_DDD_CobFax         // 114
        //        , entity.CLI_DDD_CobTelefoneEmpresa // 115
        //        , entity.CLI_DDD_ConjugeFone    // 116
        //        , entity.CLI_DDD_EntFax         // 117
        //        , entity.CLI_DDD_EntFone1       // 118
        //        , entity.CLI_DDD_EntFone2       // 119
        //        , entity.CLI_DDD_FoneAgencia    // 120
        //        , entity.CLI_DDD_TelReferencia1 // 121
        //        , entity.CLI_DDD_TelReferencia2 // 122
        //        , entity.CLI_EntPais            // 123
        //        , entity.CLI_FoneAgencia        // 124
        //        , entity.CLI_NomeReferencia1    // 125
        //        , entity.CLI_NomeReferencia2    // 126
        //        , entity.CLI_OrgEmissorRG       // 127
        //        , entity.CLI_Pais               // 128
        //        , entity.CLI_RendaMensal        // 129
        //        , entity.CLI_Status             // 130
        //        , entity.CLI_TelReferencia1     // 131
        //        , entity.CLI_TelReferencia2     // 132
        //        , entity.CLI_Tipo               // 133
        //    );

        //    #endregion
        //    //
        //    DataSet dsCliente = new DataSet("Cliente");
        //    dsCliente.Tables.Add(dtCliente);
        //    //
        //    int iempresa = Convert.ToInt32(empresa);
        //    int iloja = Convert.ToInt32(loja);
        //    //
        //    Rom_LojaMethods rom_LojaMethods = new Rom_LojaMethods(iempresa, usuario);
        //    string Ds_URLWSSF = rom_LojaMethods.FindAll(a => a.Id.Cd_Empresa == iempresa && a.Id.Cd_Loja == iloja).FirstOrDefault().Ds_URLWSSF;
        //    //
        //    WebServiceEfinance.ServiceSoapClient webservice1 = new WebServiceEfinance.ServiceSoapClient();
        //    webservice1.Endpoint.Address = new System.ServiceModel.EndpointAddress(Ds_URLWSSF);
        //    sRet = webservice1.AtualizaCliente(dsCliente, 0);
        //    //
        //    if (sRet.ToLower().Equals("ok"))
        //    {
        //        base.GravaSolicitacaoWSCliente(dsCliente.GetXml(), "Cliente no eFinance atualizado com sucesso", "0");
        //    }
        //    else
        //    {
        //        base.GravaSolicitacaoWSCliente(dsCliente.GetXml(), "ERRO Atualizando Cliente no eFinance : " + sRet, "1");
        //    }
        //    //
        //    return sRet;
        //}
        ////
        //public ActionResult Export(int empresa, int loja, int empresa_acao, int loja_acao, string usuario, string cultura, int equipe, int procedencia, string _dc, int page,
        //                           int start, int limit, string sort, string dir, int opcaoBusca, string textoBusca, int periodo, int filtroempresa,
        //                           int filtroloja, int aniversario, string dataInicial, string dataFinal)
        //{
        //    ClienteMethods clienteMethods = new ClienteMethods(Convert.ToInt32(empresa_acao), usuario);
        //    string where = String.Empty;
        //    var values = new object[5];
        //    int paramNo = 0, numResults = 0;

        //    switch (periodo)
        //    {
        //        case 0:

        //            break;
        //        case 3:
        //            if (where != String.Empty) { where += " AND "; }
        //            where += "CLI_DataCadastro >= @" + paramNo;
        //            values[paramNo] = DateTime.Now.Date.AddDays(-90);
        //            paramNo++;
        //            if (where != String.Empty) { where += " AND "; }
        //            where += "CLI_DataCadastro <= @" + paramNo;
        //            values[paramNo] = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
        //            paramNo++;
        //            break;
        //        case 6:
        //            if (where != String.Empty) { where += " AND "; }
        //            where += "CLI_DataCadastro >= @" + paramNo;
        //            values[paramNo] = DateTime.Now.Date.AddDays(-180);
        //            paramNo++;
        //            if (where != String.Empty) { where += " AND "; }
        //            where += "CLI_DataCadastro <= @" + paramNo;
        //            values[paramNo] = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
        //            paramNo++;
        //            break;
        //        case 12:
        //            if (where != String.Empty) { where += " AND "; }
        //            where += "CLI_DataCadastro >= @" + paramNo;
        //            values[paramNo] = DateTime.Now.Date.AddDays(-365);
        //            paramNo++;
        //            if (where != String.Empty) { where += " AND "; }
        //            where += "CLI_DataCadastro <= @" + paramNo;
        //            values[paramNo] = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
        //            paramNo++;
        //            break;
        //    }

        //    if (!string.IsNullOrEmpty(textoBusca))
        //    {
        //        switch (opcaoBusca)
        //        {
        //            case 1: // Código cliente
        //                if (where != String.Empty) { where += " AND "; }
        //                where += "Id.CLI_Codigo = @" + paramNo;
        //                values[paramNo] = Convert.ToInt32(textoBusca);
        //                paramNo++;
        //                break;

        //            case 2: // Nome / Razão Social
        //                if (where != String.Empty) { where += " AND "; }
        //                where += "CLI_Nome.Trim().Contains(@" + paramNo + ")";
        //                values[paramNo] = HttpUtility.UrlDecode(textoBusca);
        //                paramNo++;
        //                break;

        //            case 3: // CPF/CNPJ
        //                if (where != String.Empty) { where += " AND "; }
        //                where += "CLI_CgcCpf.Trim().Contains(@" + paramNo + ")";
        //                values[paramNo] = HttpUtility.UrlDecode(textoBusca);
        //                paramNo++;
        //                break;

        //            case 4: // RG/IE
        //                if (where != String.Empty) { where += " AND "; }
        //                where += "CLI_InscRG.Trim().Contains(@" + paramNo + ")";
        //                values[paramNo] = HttpUtility.UrlDecode(textoBusca);
        //                paramNo++;
        //                break;

        //            case 5: // Endereço
        //                if (where != String.Empty) { where += " AND "; }
        //                where += "CLI_Endereco.Trim().Contains(@" + paramNo + ")";
        //                values[paramNo] = HttpUtility.UrlDecode(textoBusca);
        //                paramNo++;
        //                break;

        //            case 6: // Bairro
        //                if (where != String.Empty) { where += " AND "; }
        //                where += "CLI_Bairro.Trim().Contains(@" + paramNo + ")";
        //                values[paramNo] = HttpUtility.UrlDecode(textoBusca);
        //                paramNo++;
        //                break;

        //            case 7: // Cidade
        //                if (where != String.Empty) { where += " AND "; }
        //                where += "CLI_Cidade.Trim().Contains(@" + paramNo + ")";
        //                values[paramNo] = HttpUtility.UrlDecode(textoBusca);
        //                paramNo++;
        //                break;

        //            case 8: // Estado
        //                if (where != String.Empty) { where += " AND "; }
        //                where += "CLI_Estado.Trim().Contains(@" + paramNo + ")";
        //                values[paramNo] = HttpUtility.UrlDecode(textoBusca);
        //                paramNo++;
        //                break;

        //            case 9: // Todos os Telefones
        //                if (where != String.Empty) { where += " AND "; }
        //                where += "(" +
        //                            "CLI_CorFone1.Trim().Contains(@" + paramNo + ") OR CLI_CorFone2.Trim().Contains(@" + paramNo + ")" +
        //                            " OR CLI_CobFone1.Trim().Contains(@" + paramNo + ") OR CLI_CobFone2.Trim().Contains(@" + paramNo + ")" +
        //                            " OR CLI_TelReferencia1.Trim().Contains(@" + paramNo + ") OR CLI_TelReferencia2.Trim().Contains(@" + paramNo + ")" +
        //                            " OR CLI_CobTelefoneEmpresa.Trim().Contains(@" + paramNo + ") OR CLI_FoneAgencia.Trim().Contains(@" + paramNo + ")" +
        //                            " OR CLI_ConjugeFone.Trim().Contains(@" + paramNo + ") OR CLI_EntFone1.Trim().Contains(@" + paramNo + ")" +
        //                            " OR CLI_EntFone2.Trim().Contains(@" + paramNo + ")" +
        //                         ")";
        //                values[paramNo] = HttpUtility.UrlDecode(textoBusca);
        //                paramNo++;
        //                break;

        //            case 10: // Email
        //                if (where != String.Empty) { where += " AND "; }
        //                where += "CLI_Email.Trim().Contains(@" + paramNo + ")";
        //                values[paramNo] = HttpUtility.UrlDecode(textoBusca);
        //                paramNo++;
        //                break;
        //        }
        //    }
        //    //
        //    List<Entities.Cliente> clienteList;
        //    //            
        //    if (equipe > 0)
        //    {
        //        if (where != String.Empty) { where += " AND "; }
        //        where += "CLI_UserEquipe = @" + paramNo;
        //        values[paramNo] = Convert.ToInt32(equipe);
        //        paramNo++;
        //    }
        //    //
        //    if (procedencia > 0)
        //    {
        //        if (where != String.Empty) { where += " AND "; }
        //        where += "CLI_Procedencia = @" + paramNo;
        //        values[paramNo] = Convert.ToInt32(procedencia);
        //        paramNo++;
        //    }

        //    // Aniversário 
        //    if (aniversario > 0)
        //    {
        //        if (where != String.Empty) { where += " AND "; }
        //        where += "CLI_DataNascimento != null && CLI_DataNascimento.Value.Month = @" + paramNo;
        //        values[paramNo] = aniversario;
        //        paramNo++;
        //    }

        //    //
        //    int limit_max = Int32.MaxValue;
        //    if (!String.IsNullOrEmpty(where))
        //    {
        //        try
        //        {
        //            if (filtroempresa == 0 && filtroloja == 0)
        //            {
        //                clienteList = clienteMethods.FindAll(where, values, q => q.Id.CLI_Emp >= 0, out numResults, (limit_max * (1 - 1)), limit_max);
        //            }
        //            else
        //            {
        //                clienteList = clienteMethods.FindAll(where, values, q => q.Id.CLI_Loja == filtroloja, out numResults, (limit_max * (1 - 1)), limit_max);
        //            }
        //        }
        //        catch (Exception nn)
        //        {
        //            clienteList = new List<Entities.Cliente>();
        //        }
        //    }
        //    else
        //    {
        //        if (filtroempresa == 0 && filtroloja == 0)
        //        {
        //            clienteList = clienteMethods.FindAll(q => q.Id.CLI_Emp >= 0, out numResults, (limit_max * (1 - 1)), limit_max, "Id.CLI_Codigo");
        //        }
        //        else if (filtroempresa != 0 && filtroloja == 0)
        //        {
        //            clienteList = clienteMethods.FindAll(q => q.Id.CLI_Emp == filtroempresa, out numResults, (limit_max * (1 - 1)), limit_max);
        //        }
        //        else if (filtroempresa == 0 && filtroloja != 0)
        //        {
        //            clienteList = clienteMethods.FindAll(q => q.Id.CLI_Loja == filtroloja, out numResults, (limit_max * (1 - 1)), limit_max);
        //        }
        //        else
        //        {
        //            clienteList = clienteMethods.FindAll(q => q.Id.CLI_Emp == filtroempresa && q.Id.CLI_Loja == filtroloja, out numResults, (limit_max * (1 - 1)), limit_max);
        //        }
        //    }
        //    //
        //    numResults = clienteList.Count;
        //    clienteList = this.OrdenaValidandoPaginacao(clienteList, sort, dir);
        //    //
        //    #region Entity to Model
        //    //
        //    while (clienteList.Count < 20)
        //    {
        //        Entities.Cliente tmp = new Entities.Cliente();
        //        clienteList.Add(tmp);
        //    }
        //    //
        //    List<ClienteExportModels> clienteModelsList = clienteList.Select(s => new ClienteExportModels()
        //    {
        //        CLI_Codigo = s.Id != null ? s.Id.CLI_Codigo.ToString() : "",
        //        CLI_Loja = s.Id != null ? s.Id.CLI_Loja.ToString() : "",
        //        CLI_Nome = s.CLI_Nome != null ? StringExtensions.Camelize("pt-BR", s.CLI_Nome) : "",
        //        CLI_Endereco = s.CLI_Endereco,
        //        CLI_Bairro = s.CLI_Bairro,
        //        CLI_Cidade = s.CLI_Cidade,
        //        CLI_Cep = s.CLI_Cep,
        //        CLI_Estado = s.CLI_Estado,
        //        CLI_UserEquipeDescricao = s.Id != null ? this.PegaNomeEquipe(s.Id.CLI_Emp, s.CLI_Nome, s.Id.CLI_Loja, s.CLI_UserEquipe) : "",
        //        CLI_CorFone1 = s.CLI_CorFone1,
        //        CLI_Email = s.CLI_Email != null ? s.CLI_Email.ToLower() : ""

        //    }).ToList();
        //    //
        //    #endregion
        //    //
        //    DataSet ds = ToDataSet(clienteModelsList);
        //    string file_name = this.GeraNomeArquivo();
        //    ExcelLibrary.DataSetHelper.CreateWorkbook(AppDomain.CurrentDomain.BaseDirectory + "Temp\\" + file_name, ds);
        //    //
        //    Workbook book = Workbook.Load(AppDomain.CurrentDomain.BaseDirectory + "Temp\\" + file_name);
        //    Worksheet sheet = book.Worksheets[0];
        //    sheet.Cells.ColumnWidth[0, 0] = 40;
        //    sheet.Cells.ColumnWidth[0, 1] = 60;
        //    sheet.Cells.ColumnWidth[0, 2] = 3000;
        //    sheet.Cells.ColumnWidth[0, 3] = 4000;
        //    sheet.Cells.ColumnWidth[0, 4] = 5000;
        //    sheet.Cells.ColumnWidth[0, 5] = 6000;
        //    sheet.Cells.ColumnWidth[0, 6] = 7000;
        //    book.Save(AppDomain.CurrentDomain.BaseDirectory + "Temp\\" + file_name);
        //    //
        //    return Json(new { data = file_name, results = 1, success = true }, JsonRequestBehavior.AllowGet);
        //}

        ///// <summary>
        ///// Função que retorna o nome do arquivo a ser gerado para exportação do Excel
        ///// </summary>
        ///// <returns></returns>
        //private string GeraNomeArquivo()
        //{
        //    string sRet = string.Empty;
        //    //
        //    sRet = "Lista_Clientes "
        //            + DateTime.Now.Day + "-"
        //            + DateTime.Now.Month + "-"
        //            + DateTime.Now.Year + "_"
        //            + DateTime.Now.Hour + "-"
        //            + DateTime.Now.Minute + "-"
        //            + DateTime.Now.Second + ".xls";
        //    //
        //    return sRet;
        //}

        ///// <summary>
        ///// Gera um DataSet a partir de uma lista de clientes, que será utilizado na exportação do Excel
        ///// </summary>
        ///// <param name="rom_ClienteList"></param>
        ///// <returns></returns>
        //public DataSet ToDataSet(List<ClienteExportModels> rom_ClienteList)
        //{
        //    DataSet oDataSet = new DataSet();
        //    DataTable oDataTable = new DataTable();
        //    oDataSet.Tables.Add(oDataTable);
        //    //
        //    oDataTable.Columns.Add("Loja");
        //    oDataTable.Columns.Add("Código");
        //    oDataTable.Columns.Add("Razão");
        //    oDataTable.Columns.Add("Equipe");
        //    oDataTable.Columns.Add("Endereço");
        //    oDataTable.Columns.Add("Bairro");
        //    oDataTable.Columns.Add("Cidade");
        //    oDataTable.Columns.Add("Estado");
        //    oDataTable.Columns.Add("Telefone");
        //    oDataTable.Columns.Add("Email");
        //    //
        //    foreach (var item in rom_ClienteList)
        //    {
        //        DataRow row = oDataTable.NewRow();
        //        row["Loja"] = item.CLI_Loja.ToString();
        //        row["Código"] = item.CLI_Codigo.ToString();
        //        row["Razão"] = !string.IsNullOrEmpty(item.CLI_Nome) ? item.CLI_Nome : "";
        //        row["Equipe"] = !string.IsNullOrEmpty(item.CLI_UserEquipeDescricao) ? item.CLI_UserEquipeDescricao : "";
        //        row["Endereço"] = !string.IsNullOrEmpty(item.CLI_Endereco) ? item.CLI_Endereco : "";
        //        row["Bairro"] = !string.IsNullOrEmpty(item.CLI_Bairro) ? item.CLI_Bairro : "";
        //        row["Cidade"] = !string.IsNullOrEmpty(item.CLI_Cidade) ? item.CLI_Cidade : "";
        //        row["Estado"] = !string.IsNullOrEmpty(item.CLI_Estado) ? item.CLI_Estado : "";
        //        row["Telefone"] = !string.IsNullOrEmpty(item.CLI_CorFone1) ? item.CLI_CorFone1 : "";
        //        row["Email"] = !string.IsNullOrEmpty(item.CLI_Email) ? item.CLI_Email : "";
        //        oDataTable.Rows.Add(row);
        //    }
        //    //
        //    return oDataSet;
        //}

        //private List<Entities.Cliente> OrdenaValidandoPaginacao(List<Entities.Cliente> UsuarioModelList, string sort, string dir)
        //{
        //    // --- ORDENAÇÃO DOS REGISTROS EXIBIDOS NA TELA            
        //    switch (sort)
        //    {
        //        case "CLI_Codigo":
        //            UsuarioModelList = Agro.Utils.Utils.Sort.SortList(UsuarioModelList, o => o.CLI_Codigo, dir);
        //            break;
        //        case "CLI_Nome":
        //            UsuarioModelList = Agro.Utils.Utils.Sort.SortList(UsuarioModelList, o => o.CLI_Nome, dir);
        //            break;
        //        case "CLI_Endereco":
        //            UsuarioModelList = Agro.Utils.Utils.Sort.SortList(UsuarioModelList, o => o.CLI_Endereco, dir);
        //            break;
        //        case "CLI_Bairro":
        //            UsuarioModelList = Agro.Utils.Utils.Sort.SortList(UsuarioModelList, o => o.CLI_Bairro, dir);
        //            break;
        //        case "CLI_Cidade":
        //            UsuarioModelList = Agro.Utils.Utils.Sort.SortList(UsuarioModelList, o => o.CLI_Cidade, dir);
        //            break;
        //        case "CLI_Estado":
        //            UsuarioModelList = Agro.Utils.Utils.Sort.SortList(UsuarioModelList, o => o.CLI_Estado, dir);
        //            break;
        //        case "CLI_CorFone1":
        //            UsuarioModelList = Agro.Utils.Utils.Sort.SortList(UsuarioModelList, o => o.CLI_CorFone1, dir);
        //            break;
        //        case "CLI_Email":
        //            UsuarioModelList = Agro.Utils.Utils.Sort.SortList(UsuarioModelList, o => o.CLI_Email, dir);
        //            break;
        //        case "CLI_UserEquipeDescricao":
        //            UsuarioModelList = Agro.Utils.Utils.Sort.SortList(UsuarioModelList, o => o.CLI_UserEquipeDescricao, dir);
        //            break;
        //        default:
        //            UsuarioModelList = Agro.Utils.Utils.Sort.SortList(UsuarioModelList, o => o.CLI_Codigo, "DESC");
        //            break;
        //    }
        //    //
        //    return UsuarioModelList;
        //}


        //public List<ClienteModels> ToModel(int empresa, string usuario, List<Entities.Cliente> clienteList)
        //{
        //    UsuarioMethods UsuarioMethods = new UsuarioMethods(empresa, usuario);
        //    Rom_LojaMethods rom_LojaMethods = new Rom_LojaMethods(empresa, usuario);
        //    //
        //    return clienteList.Select(s => new ClienteModels()
        //    {
        //        CLI_Codigo = s.CLI_Codigo,
        //        CLI_Emp = s.Id.CLI_Emp,
        //        CLI_Loja = s.Id.CLI_Loja,
        //        CLI_Nome = string.IsNullOrEmpty(s.CLI_Nome) ? s.CLI_Nome : s.CLI_Nome.ToUpper(),
        //        CLI_CodigoFabrica = s.CLI_CodigoFabrica,
        //        CLI_SeqProjeto = s.CLI_SeqProjeto,
        //        CLI_Endereco = string.IsNullOrEmpty(s.CLI_Endereco) ? s.CLI_Endereco : s.CLI_Endereco.ToUpper(),
        //        CLI_Bairro = string.IsNullOrEmpty(s.CLI_Bairro) ? s.CLI_Bairro : s.CLI_Bairro.ToUpper(),
        //        CLI_Cidade = string.IsNullOrEmpty(s.CLI_Cidade) ? s.CLI_Cidade : s.CLI_Cidade.ToUpper(),
        //        CLI_Cep = s.CLI_Cep,
        //        CLI_Estado = s.CLI_Estado,
        //        CLI_DataNascimento = s.CLI_DataNascimento,
        //        CLI_Tipo = s.CLI_Tipo,
        //        CLI_Procedencia = s.CLI_Procedencia,
        //        CLI_ProcedenciaNome = s.CLI_Procedencia == null ? null : this.PegaNomeProcedencia(s.Id.CLI_Emp, s.CLI_Procedencia),
        //        CLI_CgcCpf = s.CLI_CgcCpf,
        //        CLI_InscRG = s.CLI_InscRG,
        //        CLI_FisJur = s.CLI_FisJur,
        //        CLI_Status = s.CLI_Status,
        //        CLI_Email = string.IsNullOrEmpty(s.CLI_Email) ? s.CLI_Email : s.CLI_Email.ToLower(),
        //        CLI_EntEdificio = s.CLI_EntEdificio,
        //        CLI_EntReferencia = s.CLI_EntReferencia,
        //        CLI_EntEndereco = string.IsNullOrEmpty(s.CLI_EntEndereco) ? s.CLI_EntEndereco : s.CLI_EntEndereco.ToUpper(),
        //        CLI_EntBairro = string.IsNullOrEmpty(s.CLI_EntBairro) ? s.CLI_EntBairro : s.CLI_EntBairro.ToUpper(),
        //        CLI_EntCidade = string.IsNullOrEmpty(s.CLI_EntCidade) ? s.CLI_EntCidade : s.CLI_EntCidade.ToUpper(),
        //        CLI_EntCep = s.CLI_EntCep,
        //        CLI_EntEstado = s.CLI_EntEstado,
        //        CLI_EntFax = s.CLI_EntFax,
        //        CLI_EntFone1 = s.CLI_EntFone1,
        //        CLI_EntFone2 = s.CLI_EntFone2,
        //        CLI_EntPais = s.CLI_EntPais,
        //        CLI_EntCgcCpf = s.CLI_EntCgcCpf,
        //        CLI_EntInscRG = s.CLI_EntInscRG,
        //        CLI_CobEndereco = s.CLI_CobEndereco,
        //        CLI_CobBairro = s.CLI_CobBairro,
        //        CLI_CobCidade = s.CLI_CobCidade,
        //        CLI_CobCep = s.CLI_CobCep,
        //        CLI_CobEstado = s.CLI_CobEstado,
        //        CLI_CobFax = s.CLI_CobFax,
        //        CLI_CobFone1 = s.CLI_CobFone1,
        //        CLI_CobFone2 = s.CLI_CobFone2,
        //        CLI_CobPais = s.CLI_CobPais,
        //        CLI_CobCgcCpf = s.CLI_CobCgcCpf,
        //        CLI_CobInscRG = s.CLI_CobInscRG,
        //        CLI_CorEndereco = s.CLI_CorEndereco,
        //        CLI_CorBairro = s.CLI_CorBairro,
        //        CLI_CorCidade = s.CLI_CorCidade,
        //        CLI_CorCep = s.CLI_CorCep,
        //        CLI_CorEstado = s.CLI_CorEstado,
        //        CLI_CorFax = s.CLI_CorFax,
        //        CLI_CorFone1 = s.CLI_CorFone1,
        //        CLI_CorFone2 = s.CLI_CorFone2,
        //        CLI_DataEnvio = s.CLI_DataEnvio,
        //        CLI_DataRomaneio = s.CLI_DataRomaneio,
        //        CLI_DataCadastro = s.CLI_DataCadastro,
        //        CLI_HoraCadastro = s.CLI_HoraCadastro,
        //        CLI_UserCadastro = s.CLI_UserCadastro,
        //        CLI_UserCadastroDescricao = this.PegaNomeusuarioCadastro(s.Id.CLI_Emp, s.CLI_UserCadastro, UsuarioMethods),
        //        CLI_DataAltera = s.CLI_DataAltera,
        //        CLI_HoraAltera = s.CLI_HoraAltera,
        //        CLI_UserAltera = s.CLI_UserAltera,
        //        CLI_UserEquipe = s.CLI_UserEquipe,
        //        CLI_UserEquipeDescricao = this.PegaNomeEquipe(s.Id.CLI_Emp, s.CLI_Nome, s.Id.CLI_Loja, s.CLI_UserEquipe),
        //        CLI_Obs = s.CLI_Obs,
        //        CLI_DataObra = s.CLI_DataObra,
        //        CLI_Contribuinte = s.CLI_Contribuinte,
        //        CLI_Suframa = s.CLI_Suframa,
        //        CLI_Profissao = s.CLI_Profissao,
        //        CLI_EnderecoNum = s.CLI_EnderecoNum,
        //        CLI_EnderecoCompl = s.CLI_EnderecoCompl,
        //        CLI_EntEnderecoNum = s.CLI_EntEnderecoNum,
        //        CLI_EntEnderecoCompl = s.CLI_EntEnderecoCompl,
        //        CLI_CobEnderecoNum = s.CLI_CobEnderecoNum,
        //        CLI_CobEnderecoCompl = s.CLI_CobEnderecoCompl,
        //        CLI_PrevisaoFechamento = s.CLI_PrevisaoFechamento,
        //        CLI_CodCidadeIBGE = s.CLI_CodCidadeIBGE,
        //        CLI_EntCodCidadeIBGE = s.CLI_EntCodCidadeIBGE,
        //        CLI_CobCodCidadeIBGE = s.CLI_CobCodCidadeIBGE,
        //        CLI_Representante = this.PegaNomeRepresentante(s.Id.CLI_Emp, s.CLI_Nome, s.Id.CLI_Loja, s.CLI_UserCadastro),
        //        CLI_Selecionado = s.CLI_Selecionado,
        //        CLI_DDD_CorFone1 = s.CLI_DDD_CorFone1,
        //        CLI_DDD_CorFone2 = s.CLI_DDD_CorFone2,
        //        CLI_DDD_CobFone1 = s.CLI_DDD_CobFone1,
        //        CLI_DDD_CobFone2 = s.CLI_DDD_CobFone2,
        //        CLI_DDD_TelReferencia1 = s.CLI_DDD_TelReferencia1,
        //        CLI_TelReferencia1 = s.CLI_TelReferencia1,
        //        CLI_DDD_TelReferencia2 = s.CLI_DDD_TelReferencia2,
        //        CLI_TelReferencia2 = s.CLI_TelReferencia2,
        //        CLI_DDD_CobFax = s.CLI_DDD_CobFax,
        //        CLI_DDD_CobTelefoneEmpresa = s.CLI_DDD_CobTelefoneEmpresa,
        //        CLI_DDD_ConjugeFone = s.CLI_DDD_ConjugeFone,
        //        CLI_DDD_EntFax = s.CLI_DDD_EntFax,
        //        CLI_DDD_EntFone1 = s.CLI_DDD_EntFone1,
        //        CLI_DDD_EntFone2 = s.CLI_DDD_EntFone2,
        //        CLI_DDD_FoneAgencia = s.CLI_DDD_FoneAgencia,
        //        CLI_NomeReferencia1 = s.CLI_NomeReferencia1,
        //        CLI_NomeReferencia2 = s.CLI_NomeReferencia2,
        //        CLI_DataEmissaoRG = s.CLI_DataEmissaoRG,
        //        CLI_OrgEmissorRG = s.CLI_OrgEmissorRG,
        //        CLI_CobTipoResidencia = s.CLI_CobTipoResidencia,
        //        CLI_CobTempoResidencia = s.CLI_CobTempoResidencia,
        //        CLI_CobNomeMae = s.CLI_CobNomeMae,
        //        CLI_CobNomePai = s.CLI_CobNomePai,
        //        CLI_CobNaturalidade = s.CLI_CobNaturalidade,
        //        CLI_CobNacionalidade = s.CLI_CobNacionalidade,
        //        CLI_CobEstadoCivil = s.CLI_CobEstadoCivil,
        //        CLI_CobNomeEmpresa = s.CLI_CobNomeEmpresa,
        //        CLI_CNPJTrabalho = s.CLI_CNPJTrabalho,
        //        CLI_ComEndereco = s.CLI_ComEndereco,
        //        CLI_ComEnderecoNum = s.CLI_ComEnderecoNum,
        //        CLI_ComEnderecoCompl = s.CLI_ComEnderecoCompl,
        //        CLI_ComBairro = s.CLI_ComBairro,
        //        CLI_ComCidade = s.CLI_ComCidade,
        //        CLI_ComEstado = s.CLI_ComEstado,
        //        CLI_ComCEP = s.CLI_ComCEP,
        //        CLI_ComCodCidadeIBGE = s.CLI_ComCodCidadeIBGE,
        //        CLI_ComPais = s.CLI_ComPais,
        //        CLI_CobTelefoneEmpresa = s.CLI_CobTelefoneEmpresa,
        //        CLI_CobDataAdmissao = s.CLI_CobDataAdmissao,
        //        CLI_CobOcupacao = s.CLI_CobOcupacao,
        //        CLI_Banco = s.CLI_Banco,
        //        CLI_Agencia = s.CLI_Agencia,
        //        CLI_CTABanco = s.CLI_CTABanco,
        //        CLI_CTADesde = s.CLI_CTADesde,
        //        CLI_FoneAgencia = s.CLI_FoneAgencia,
        //        CLI_RendaMensal = s.CLI_RendaMensal,
        //        CLI_ConjugeNome = string.IsNullOrEmpty(s.CLI_ConjugeNome) ? s.CLI_ConjugeNome : s.CLI_ConjugeNome.ToUpper(),
        //        CLI_ConjugeCPF = s.CLI_ConjugeCPF,
        //        CLI_ConjugeRG = s.CLI_ConjugeRG,
        //        CLI_ConjugeDataNascimento = s.CLI_ConjugeDataNascimento,
        //        CLI_ConjugeFone = s.CLI_ConjugeFone,
        //        CLI_ConjugeNomeEmpresa = s.CLI_ConjugeNomeEmpresa,
        //        CLI_ConjugeCNPJTrabalho = s.CLI_ConjugeCNPJTrabalho,
        //        CLI_ConjugeRendaMensal = s.CLI_ConjugeRendaMensal,
        //        CLI_Pais = s.CLI_Pais,
        //        CLI_AtualizadoeFinance = this.ValidaClienteUsaEfinance(s.Id.CLI_Emp, s.Id.CLI_Loja, s.Id.CLI_Emp, s.Id.CLI_Loja, rom_LojaMethods) ? s.CLI_AtualizadoeFinance : true,
        //        CLI_ObservacoesTooltip = this.PegaObservacoesCliente(s.Id.CLI_Emp, s.Id.CLI_Loja, s.Id.CLI_Emp, s.Id.CLI_Loja, s.Id.CLI_Codigo)
        //    }).ToList();
        //}

        ///// <param name="empresa">Código da Empresa loagada.</param>
        ///// <param name="loja">Código da Loja logada.</param>        
        ///// <param name="usuario">Identificação do Usuário logado.</param>   
        ///// <param name="cultura">Código da Cultura do Usuário logado. Possíveis valores: pt-BR, en-US, es-ES.</param>
        //public JsonResult Project(int empresa, int loja, string usuario, string cultura, int ftCdEmpresa, int ftCdLoja, int ftCdEquipe, int ftCdRepresentante, int ftOpcaoBusca, string ftTextoBusca)
        //{
        //    List<ClienteTransferenciaModel> modelList = new List<ClienteTransferenciaModel>();
        //    ClienteTransferenciaModel model;

        //    string sqlConsulta = @"
        //        SELECT CLI_Emp
        //             , CLI_Loja 
        //          , CLI_Codigo
        //          , CLI_Nome
        //          , CLI_SeqProjeto
        //          , CLI_DataCadastro
        //          , CLI_Selecionado	 
        //             , CLI_UserEquipe
        //             , CLI_UserCadastro
        //             , EQP_Sigla
        //       , Ds_Usuario
        //          FROM Cliente
        //          LEFT OUTER JOIN Equipe ON (Equipe.EQP_Emp = Cliente.CLI_Emp AND Equipe.EQP_Loja = Cliente.CLI_Loja AND Equipe.EQP_Codigo = Cliente.CLI_UserEquipe)
        //       LEFT OUTER JOIN Usuario ON (Usuario.Cd_Empresa = Cliente.CLI_Emp AND Usuario.Cd_Representante = Cliente.CLI_UserCadastro)	
        //         WHERE (CLI_Emp = {0} OR {0} = -1)
        //           AND (CLI_Loja = {1} OR {1} = -1)
        //           AND (CLI_UserEquipe = {2} OR {2} = -1)
        //           AND (CLI_UserCadastro = {3} OR {3} = -1)
        //    ";

        //    sqlConsulta = String.Format(
        //        sqlConsulta,
        //        ftCdEmpresa, // 0
        //        ftCdLoja, // 1
        //        ftCdEquipe, // 2
        //        ftCdRepresentante // 3
        //    );

        //    // Tipo de Projeto
        //    //if (ftTipoProj != 0)
        //    //{
        //    //    sqlConsulta += @"
        //    //        AND EXISTS (SELECT * 
        //    //                      FROM Projeto 
        //    //         WHERE Projeto.PRO_Emp = Cliente.CLI_Emp 
        //    //           AND Projeto.PRO_Loja = Cliente.CLI_Loja 
        //    //        AND Projeto.PRO_Cliente = Cliente.CLI_Codigo
        //    //        AND Projeto.PRO_CdSituacao = 0)
        //    //    ";
        //    //}

        //    // Opções de busca
        //    if (!string.IsNullOrEmpty(ftTextoBusca))
        //    {
        //        switch (ftOpcaoBusca)
        //        {
        //            case 1: // Código cliente
        //                sqlConsulta += String.Format(
        //                    " AND CLI_Codigo = {0} ",
        //                    Convert.ToInt32(ftTextoBusca) // 0
        //                );
        //                break;
        //            case 2: // Nome/Razão Social
        //                sqlConsulta += String.Format(
        //                    " AND CLI_Nome LIKE '%{0}%' ",
        //                    HttpUtility.UrlDecode(ftTextoBusca) // 0
        //                );
        //                break;
        //        }
        //    }

        //    sqlConsulta += " ORDER BY CLI_Nome ";

        //    DataSet dsConsulta = ExecutaSQLToDataSet(sqlConsulta);

        //    foreach (DataRow row in dsConsulta.Tables[0].Rows)
        //    {
        //        model = new ClienteTransferenciaModel();

        //        model.CLI_Emp = Convert.ToInt32(row["CLI_Emp"]);
        //        model.CLI_Loja = Convert.ToInt32(row["CLI_Loja"]);
        //        model.CLI_Codigo = Convert.ToInt32(row["CLI_Codigo"]);
        //        model.CLI_Nome = StringExtensions.Camelize("pt-BR", Convert.ToString(row["CLI_Nome"]));
        //        model.CLI_SeqProjeto = Convert.ToInt32(row["CLI_SeqProjeto"]);
        //        model.CLI_DataCadastro = Convert.ToDateTime(row["CLI_DataCadastro"]);
        //        model.CLI_Selecionado = String.IsNullOrEmpty(Convert.ToString(row["CLI_Selecionado"]))
        //                              ? false
        //                              : Convert.ToBoolean(row["CLI_Selecionado"]);
        //        model.CLI_UserEquipe = Convert.ToInt32(row["CLI_UserEquipe"]);
        //        model.CLI_UserEquipeDescricao = Convert.ToString(row["EQP_Sigla"]);
        //        model.CLI_UserCadastro = Convert.ToInt32(row["CLI_UserCadastro"]);
        //        model.CLI_Representante = StringExtensions.Camelize("pt-BR", Convert.ToString(row["Ds_Usuario"]));

        //        modelList.Add(model);
        //    }

        //    return Json(
        //        new
        //        {
        //            data = modelList.ToArray(),
        //            results = modelList.Count,
        //            success = true,
        //            errors = String.Empty
        //        },
        //        JsonRequestBehavior.AllowGet
        //    );
        //}
        ////
        //public JsonResult Seleciona(int empresa, int loja, int empresa_acao, int loja_acao, string usuario, string cultura, int cliempresa, int cliLoja, int cliCodigo, int marcado)
        //{
        //    ClienteMethods clienteMethods = new ClienteMethods(Convert.ToInt32(empresa_acao), usuario);
        //    string sql = @"UPDATE Cliente
        //                      SET CLI_Selecionado = " + marcado.ToString()
        //            + " WHERE CLI_Emp = " + cliempresa
        //            + "   AND CLI_Loja = " + cliLoja
        //            + "   AND CLI_Codigo = " + cliCodigo;

        //    sql = sql.Replace("\r\n", " ");
        //    clienteMethods.ExecuteSQL(sql);
        //    //
        //    sql = @"UPDATE Projeto 
        //                      SET PRO_Selecionado = " + marcado.ToString()
        //            + " WHERE PRO_Emp = " + cliempresa
        //            + "   AND PRO_Loja = " + cliLoja
        //            + "   AND PRO_Cliente = " + cliCodigo;

        //    sql = sql.Replace("\r\n", " ");
        //    clienteMethods.ExecuteSQL(sql);
        //    //
        //    return Json(new { data = "OK".ToArray(), results = 1, success = true, errors = String.Empty }, JsonRequestBehavior.AllowGet);
        //}
        ////

        ///// <param name="empresa">Código da Empresa loagada.</param>
        ///// <param name="loja">Código da Loja logada.</param>        
        ///// <param name="usuario">Identificação do Usuário logado.</param>   
        ///// <param name="cultura">Código da Cultura do Usuário logado. Possíveis valores: pt-BR, en-US, es-ES.</param>
        //public JsonResult TransfereCliente(int empresa, int loja, string usuario, string cultura, int cdEquipe, int cdRepresentante, int cdTipoProjeto, string jsonGrid)
        //{
        //    List<ClienteTransferenciaModel> modelList = new JavaScriptSerializer().Deserialize<List<ClienteTransferenciaModel>>(jsonGrid);

        //    string sqlCliente = @"
        //        UPDATE Cliente 
        //           SET CLI_UserCadastro = {3}
        //             , CLI_UserEquipe = {4}
        //         WHERE CLI_Emp = {0}
        //           AND CLI_Loja = {1} 
        //           AND CLI_Codigo = {2}
        //    ";

        //    string sqlProjeto = @"
        //        UPDATE Projeto 
        //           SET PRO_UserCadastro = {3}
        //             , PRO_UserEquipe = {4}
        //         WHERE PRO_Emp = {0}                
        //           AND PRO_Loja = {1}
        //           AND PRO_Cliente = {2}
        //    ";

        //    if (cdTipoProjeto == 1)
        //    {
        //        sqlProjeto += " AND PRO_CdSituacao = 0";
        //    }

        //    foreach (ClienteTransferenciaModel model in modelList)
        //    {
        //        // Cliente
        //        ExecutaSQLNonQuery(
        //            String.Format(
        //                sqlCliente,
        //                model.CLI_Emp,
        //                model.CLI_Loja,
        //                model.CLI_Codigo,
        //                cdRepresentante,
        //                cdEquipe
        //            )
        //        );

        //        // Projeto
        //        ExecutaSQLNonQuery(
        //            String.Format(
        //                sqlProjeto,
        //                model.CLI_Emp,
        //                model.CLI_Loja,
        //                model.CLI_Codigo,
        //                cdRepresentante,
        //                cdEquipe
        //            )
        //        );
        //    }

        //    return Json(new
        //    {
        //        success = true,
        //        data = "OK".ToArray(),
        //        results = 1,
        //        errors = ""
        //    }, JsonRequestBehavior.AllowGet);
        //}
        ////
        //public JsonResult FichaCliente(int empresa, int loja, int empresa_acao, int loja_acao, string usuario, string cultura, int filtroempresa, int filtroloja, int codigo_cliente)
        //{
        //    ClienteMethods clienteMethods = new ClienteMethods(empresa_acao, usuario);
        //    //
        //    List<ClienteFichaModels> cliente_model_list = clienteMethods
        //        .FindAll(a => a.Id.CLI_Emp == filtroempresa && a.Id.CLI_Loja == filtroloja && a.Id.CLI_Codigo == codigo_cliente)
        //        .Select(s => new ClienteFichaModels()
        //        {
        //            CLI_Codigo = s.Id.CLI_Codigo.ToString(),
        //            CLI_Emp = s.Id.CLI_Emp.ToString(),
        //            CLI_Loja = s.Id.CLI_Loja.ToString(),
        //            CLI_Nome = string.IsNullOrEmpty(s.CLI_Nome) ? s.CLI_Nome : s.CLI_Nome.ToUpper(),
        //            CLI_Endereco = string.IsNullOrEmpty(s.CLI_Endereco) ? "" : s.CLI_Endereco.ToUpper() + (string.IsNullOrEmpty(s.CLI_EnderecoNum) ? "" : ", " + s.CLI_EnderecoNum),
        //            CLI_EnderecoCompl = string.IsNullOrEmpty(s.CLI_EnderecoCompl) ? "" : s.CLI_EnderecoCompl.ToUpper(),
        //            CLI_Bairro = string.IsNullOrEmpty(s.CLI_Bairro) ? "" : s.CLI_Bairro.ToUpper(),
        //            CLI_Cidade = string.IsNullOrEmpty(s.CLI_Cidade) ? "" : s.CLI_Cidade.ToUpper(),
        //            CLI_Cep = string.IsNullOrEmpty(s.CLI_Cep) ? "" : s.CLI_Cep.ToUpper(),
        //            CLI_Estado = string.IsNullOrEmpty(s.CLI_Estado) ? "" : s.CLI_Estado.ToUpper(),
        //            CLI_DataNascimento = s.CLI_DataNascimento != null ? s.CLI_DataNascimento.Value.ToShortDateString() : "",
        //            CLI_CgcCpf = this.FormataCPFCNPJ(s.CLI_CgcCpf),
        //            CLI_InscRG = string.IsNullOrEmpty(s.CLI_InscRG) ? "" : s.CLI_InscRG.ToUpper(),
        //            CLI_Profissao = string.IsNullOrEmpty(s.CLI_Profissao) ? "" : s.CLI_Profissao.ToUpper(),
        //            CLI_Email = string.IsNullOrEmpty(s.CLI_Email) ? "" : s.CLI_Email.ToUpper(),
        //            CLI_CorFone1 = s.CLI_DDD_CorFone1 == null || s.CLI_DDD_CorFone1 == "0" ? s.CLI_CorFone1 : "(" + s.CLI_DDD_CorFone1 + ") " + s.CLI_CorFone1,
        //            CLI_CorFone2 = s.CLI_DDD_CorFone2 == null || s.CLI_DDD_CorFone2 == "0" ? s.CLI_CorFone2 : "(" + s.CLI_DDD_CorFone2 + ") " + s.CLI_CorFone2,
        //            CLI_Suframa = string.IsNullOrEmpty(s.CLI_Suframa) ? "" : s.CLI_Suframa.ToUpper(),
        //            CLI_Contribuinte = s.CLI_Contribuinte ? Agro.Resources.Resources.Sim.ToUpper() : Agro.Resources.Resources.Nao.ToUpper(),
        //            CLI_CobEndereco = string.IsNullOrEmpty(s.CLI_CobEndereco) ? "" : s.CLI_CobEndereco.ToUpper() + (string.IsNullOrEmpty(s.CLI_CobEnderecoNum) ? "" : ", " + s.CLI_CobEnderecoNum),
        //            CLI_CobBairro = string.IsNullOrEmpty(s.CLI_CobBairro) ? "" : s.CLI_CobBairro.ToUpper(),
        //            CLI_CobCidade = string.IsNullOrEmpty(s.CLI_CobCidade) ? "" : s.CLI_CobCidade.ToUpper(),
        //            CLI_CobCep = string.IsNullOrEmpty(s.CLI_CobCep) ? "" : s.CLI_CobCep.ToUpper(),
        //            CLI_CobEstado = string.IsNullOrEmpty(s.CLI_CobEstado) ? "" : s.CLI_CobEstado.ToUpper(),
        //            CLI_CobCgcCpf = this.FormataCPFCNPJ(s.CLI_CobCgcCpf),
        //            CLI_CobInscRG = string.IsNullOrEmpty(s.CLI_CobInscRG) ? "" : s.CLI_CobInscRG.ToUpper(),
        //            CLI_CorEndereco = string.IsNullOrEmpty(s.CLI_CorEndereco) ? "" : s.CLI_CorEndereco.ToUpper(),
        //            CLI_CorBairro = string.IsNullOrEmpty(s.CLI_CorBairro) ? "" : s.CLI_CorBairro.ToUpper(),
        //            CLI_CorCidade = string.IsNullOrEmpty(s.CLI_CorCidade) ? "" : s.CLI_CorCidade.ToUpper(),
        //            CLI_CorCep = string.IsNullOrEmpty(s.CLI_CorCep) ? "" : s.CLI_CorCep.ToUpper(),
        //            CLI_CorEstado = string.IsNullOrEmpty(s.CLI_CorEstado) ? "" : s.CLI_CorEstado.ToUpper(),
        //            CLI_CobFone1 = s.CLI_DDD_CobFone1 == null || s.CLI_DDD_CobFone1 == "0" ? s.CLI_CobFone1 : "(" + s.CLI_DDD_CobFone1 + ") " + s.CLI_CobFone1,
        //            CLI_CobFone2 = s.CLI_DDD_CobFone2 == null || s.CLI_DDD_CobFone2 == "0" ? s.CLI_CobFone2 : "(" + s.CLI_DDD_CobFone2 + ") " + s.CLI_CobFone2,
        //            CLI_OrgEmissorRG = string.IsNullOrEmpty(s.CLI_OrgEmissorRG) ? "" : s.CLI_OrgEmissorRG.ToUpper(),
        //            CLI_DataEmissaoRG = s.CLI_DataEmissaoRG != null ? s.CLI_DataEmissaoRG.Value.ToShortDateString() : "",
        //            CLI_NomeReferencia1 = string.IsNullOrEmpty(s.CLI_NomeReferencia1) ? "" : s.CLI_NomeReferencia1.ToUpper(),
        //            CLI_NomeReferencia2 = string.IsNullOrEmpty(s.CLI_NomeReferencia2) ? "" : s.CLI_NomeReferencia2.ToUpper(),
        //            CLI_TelReferencia1 = s.CLI_DDD_TelReferencia1 == null || s.CLI_DDD_TelReferencia1 == "0" ? s.CLI_TelReferencia1 : "(" + s.CLI_DDD_TelReferencia1 + ") " + s.CLI_TelReferencia1,
        //            CLI_TelReferencia2 = s.CLI_DDD_TelReferencia2 == null || s.CLI_DDD_TelReferencia2 == "0" ? s.CLI_TelReferencia2 : "(" + s.CLI_DDD_TelReferencia1 + ") " + s.CLI_TelReferencia2,
        //            CLI_CobEnderecoCompl = string.IsNullOrEmpty(s.CLI_CobEnderecoCompl) ? "" : s.CLI_CobEnderecoCompl.ToUpper(),
        //            CLI_CobTipoResidencia = s.CLI_CobTipoResidencia != null ? this.PegaTipoResidencia(s.CLI_CobTipoResidencia) : "",
        //            CLI_CobTempoResidencia = s.CLI_CobTempoResidencia != null ? s.CLI_CobTempoResidencia.ToUpper() : "",
        //            CLI_CobNomeMae = string.IsNullOrEmpty(s.CLI_CobNomeMae) ? "" : s.CLI_CobNomeMae.ToUpper(),
        //            CLI_CobNomePai = string.IsNullOrEmpty(s.CLI_CobNomePai) ? "" : s.CLI_CobNomePai.ToUpper(),
        //            CLI_CobNaturalidade = string.IsNullOrEmpty(s.CLI_CobNaturalidade) ? "" : s.CLI_CobNaturalidade.ToUpper(),
        //            CLI_CobNacionalidade = string.IsNullOrEmpty(s.CLI_CobNacionalidade) ? "" : s.CLI_CobNacionalidade.ToUpper(),
        //            CLI_CobEstadoCivil = s.CLI_CobEstadoCivil != null ? this.PegaEstadoCivil(s.CLI_CobEstadoCivil) : "",
        //            CLI_CobNomeEmpresa = string.IsNullOrEmpty(s.CLI_CobNomeEmpresa) ? "" : s.CLI_CobNomeEmpresa.ToUpper(),
        //            CLI_CNPJTrabalho = this.FormataCPFCNPJ(s.CLI_CNPJTrabalho),
        //            CLI_ComEndereco = string.IsNullOrEmpty(s.CLI_ComEndereco) ? "" : s.CLI_ComEndereco.ToUpper() + (string.IsNullOrEmpty(s.CLI_ComEnderecoNum) ? "" : ", " + s.CLI_ComEnderecoNum),
        //            CLI_ComEnderecoCompl = string.IsNullOrEmpty(s.CLI_ComEnderecoCompl) ? "" : s.CLI_ComEnderecoCompl.ToUpper(),
        //            CLI_ComBairro = string.IsNullOrEmpty(s.CLI_ComBairro) ? "" : s.CLI_ComBairro.ToUpper(),
        //            CLI_ComCidade = string.IsNullOrEmpty(s.CLI_ComCidade) ? "" : s.CLI_ComCidade.ToUpper(),
        //            CLI_ComEstado = string.IsNullOrEmpty(s.CLI_ComEstado) ? "" : s.CLI_ComEstado.ToUpper(),
        //            CLI_ComCEP = string.IsNullOrEmpty(s.CLI_ComCEP) ? "" : s.CLI_ComCEP.ToUpper(),
        //            CLI_CobTelefoneEmpresa = s.CLI_DDD_CobTelefoneEmpresa != null ? s.CLI_DDD_CobTelefoneEmpresa == "0" ? s.CLI_CobTelefoneEmpresa : "(" + s.CLI_DDD_CobTelefoneEmpresa + ") " + s.CLI_CobTelefoneEmpresa : "",
        //            CLI_CobDataAdmissao = s.CLI_CobDataAdmissao != null ? s.CLI_CobDataAdmissao.Value.ToShortDateString() : "",
        //            CLI_CobOcupacao = string.IsNullOrEmpty(s.CLI_CobOcupacao) ? "" : s.CLI_CobOcupacao.ToUpper(),
        //            CLI_Banco = string.IsNullOrEmpty(s.CLI_Banco) ? "" : s.CLI_Banco == "0" ? "" : s.CLI_Banco,
        //            CLI_Agencia = string.IsNullOrEmpty(s.CLI_Agencia) ? "" : s.CLI_Agencia.ToUpper(),
        //            CLI_CTABanco = string.IsNullOrEmpty(s.CLI_CTABanco) ? "" : s.CLI_CTABanco == "0" ? "" : s.CLI_CTABanco,
        //            CLI_CTADesde = string.IsNullOrEmpty(s.CLI_CTADesde) ? "" : s.CLI_CTADesde.ToUpper(),
        //            CLI_FoneAgencia = s.CLI_DDD_FoneAgencia == "0" ? s.CLI_FoneAgencia : "(" + s.CLI_DDD_FoneAgencia + ") " + s.CLI_FoneAgencia,
        //            CLI_RendaMensal = s.CLI_RendaMensal != null ? s.CLI_RendaMensal > 0 ? s.CLI_RendaMensal.Value.ToString("c") : "" : "",
        //            CLI_EntEndereco = string.IsNullOrEmpty(s.CLI_EntEndereco) ? "" : s.CLI_EntEndereco.ToUpper() + (string.IsNullOrEmpty(s.CLI_EntEnderecoNum) ? "" : ", " + s.CLI_EntEnderecoNum),
        //            CLI_EntEnderecoCompl = string.IsNullOrEmpty(s.CLI_EntEnderecoCompl) ? "" : s.CLI_EntEnderecoCompl.ToUpper(),
        //            CLI_EntBairro = string.IsNullOrEmpty(s.CLI_EntBairro) ? "" : s.CLI_EntBairro.ToUpper(),
        //            CLI_EntCidade = string.IsNullOrEmpty(s.CLI_EntCidade) ? "" : s.CLI_EntCidade.ToUpper(),
        //            CLI_EntEstado = string.IsNullOrEmpty(s.CLI_EntEstado) ? "" : s.CLI_EntEstado.ToUpper(),
        //            CLI_EntCep = string.IsNullOrEmpty(s.CLI_EntCep) ? "" : s.CLI_EntCep.ToUpper(),
        //            CLI_EntEdificio = string.IsNullOrEmpty(s.CLI_EntEdificio) ? "" : s.CLI_EntEdificio.ToUpper(),
        //            CLI_EntReferencia = string.IsNullOrEmpty(s.CLI_EntReferencia) ? "" : s.CLI_EntReferencia.ToUpper(),
        //            CLI_EntCgcCpf = this.FormataCPFCNPJ(s.CLI_EntCgcCpf),
        //            CLI_EntInscRG = string.IsNullOrEmpty(s.CLI_EntInscRG) ? "" : s.CLI_EntInscRG.ToUpper(),
        //            CLI_EntFone1 = s.CLI_DDD_EntFone1 == null || s.CLI_DDD_EntFone1 == "0" ? s.CLI_EntFone1 : "(" + s.CLI_DDD_EntFone1 + ") " + s.CLI_EntFone1,
        //            CLI_EntFone2 = s.CLI_DDD_EntFone2 == null || s.CLI_DDD_EntFone2 == "0" ? s.CLI_EntFone2 : "(" + s.CLI_DDD_EntFone2 + ") " + s.CLI_EntFone2,
        //            CLI_ConjugeNome = string.IsNullOrEmpty(s.CLI_ConjugeNome) ? "" : s.CLI_ConjugeNome.ToUpper(),
        //            CLI_ConjugeCPF = this.FormataCPFCNPJ(s.CLI_ConjugeCPF),
        //            CLI_ConjugeRG = string.IsNullOrEmpty(s.CLI_ConjugeRG) ? "" : s.CLI_ConjugeRG.ToUpper(),
        //            CLI_ConjugeDataNascimento = s.CLI_ConjugeDataNascimento != null ? s.CLI_ConjugeDataNascimento.Value.ToShortDateString() : "",
        //            CLI_ConjugeFone = s.CLI_DDD_ConjugeFone == null || s.CLI_DDD_ConjugeFone == "0" ? s.CLI_ConjugeFone : "(" + s.CLI_DDD_ConjugeFone + ") " + s.CLI_ConjugeFone,
        //            CLI_ConjugeNomeEmpresa = string.IsNullOrEmpty(s.CLI_ConjugeNomeEmpresa) ? "" : s.CLI_ConjugeNomeEmpresa.ToUpper(),
        //            CLI_ConjugeCNPJTrabalho = string.IsNullOrEmpty(s.CLI_ConjugeCNPJTrabalho) ? "" : s.CLI_ConjugeCNPJTrabalho.ToUpper(),
        //            CLI_ConjugeRendaMensal = s.CLI_ConjugeRendaMensal != null ? s.CLI_ConjugeRendaMensal > 0 ? s.CLI_ConjugeRendaMensal.Value.ToString("c") : "" : ""
        //        }
        //        )
        //        .ToList();
        //    //
        //    ReportDocument clienteFichaReport = new ReportDocument();
        //    clienteFichaReport.Load(System.Web.HttpContext.Current.Server.MapPath("~/Reports/") + "ClienteFicha.rpt");
        //    clienteFichaReport.SetDataSource(cliente_model_list);
        //    clienteFichaReport.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, "ClienteFicha");
        //    clienteFichaReport.Close();
        //    //
        //    return Json(new
        //    {
        //        data = "OK",
        //        results = cliente_model_list.Count(),
        //        success = true,
        //        errors = String.Empty
        //    }, JsonRequestBehavior.AllowGet);
        //}
        ////
        //private string FormataCPFCNPJ(string cpfcnpj)
        //{
        //    string sRet = string.Empty;
        //    //
        //    if (!string.IsNullOrEmpty(cpfcnpj))
        //    {
        //        if (cpfcnpj.Length == 11)
        //        {
        //            sRet = String.Format(@"{0:000\.000\.000\-00}", Convert.ToInt64(cpfcnpj));
        //        }
        //        else if (cpfcnpj.Length == 14)
        //        {
        //            sRet = String.Format(@"{0:00\.000\.000\/0000\-00}", Convert.ToInt64(cpfcnpj));
        //        }
        //        else
        //        {
        //            sRet = cpfcnpj;
        //        }
        //    }
        //    //
        //    return sRet;
        //}
        ////
        //#endregion

        //#region Observações
        ////
        //public JsonResult SaveObservacoes(int empresa, int loja, int empresa_acao, int loja_acao, string usuario, int tipo, string observacoes, bool insert, int cliente_id, int column_id)
        //{
        //    ClienteMethods clienteMethods = new ClienteMethods();
        //    //
        //    int ano = DateTime.Now.Year;
        //    int mes = DateTime.Now.Month;
        //    int dia = DateTime.Now.Day;
        //    int hora = DateTime.Now.Hour;
        //    int minuto = DateTime.Now.Minute;
        //    int segundos = DateTime.Now.Second;
        //    //
        //    string data = ano + "-" + mes + "-" + dia + " " + hora + ":" + minuto + ":" + segundos;
        //    //
        //    if (insert)
        //    {
        //        clienteMethods.ExecuteSQL("INSERT INTO ClienteObservacoes (Cd_Emppresa, Cd_Loja, Cd_Usuario, Cd_Cliente, Criado_Por, Criado_Em, Observacoes) " +
        //                                  " VALUES (" + empresa + "," + loja + ", '" + usuario + "' , " + cliente_id + " , '" + usuario + "', '"
        //                                              + data + "', '" + observacoes + "')");
        //    }
        //    else
        //    {

        //        clienteMethods.ExecuteSQL("UPDATE ClienteObservacoes SET Observacoes = '" + observacoes + "', Alterado_Em = '" + data + "', Alterado_Por = '" + usuario + "' "
        //                                             + " WHERE Cd_Emppresa = " + empresa + " AND Cd_Loja = " + loja + " AND Cd_Cliente = " + cliente_id + " AND Id = " + column_id);
        //    }

        //    return Json(new { data = "", results = 0, success = true }, JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult CarregaObservacoes(int empresa, int loja, int empresa_acao, int loja_acao, int cliente_id)
        //{
        //    List<ClienteObservacoesModels> ClienteObservacoes_list = dbElist.ClienteObservacoes
        //        .Where(a => a.Cd_Emppresa == empresa_acao && a.Cd_Loja == loja_acao && a.Cd_Cliente == cliente_id)
        //        .Select(a => new ClienteObservacoesModels()
        //        {
        //            Id = a.Id,
        //            Cd_Emppresa = a.Cd_Emppresa,
        //            Cd_Loja = a.Cd_Loja,
        //            Cd_Cliente = a.Cd_Cliente,
        //            Cd_Usuario = a.Cd_Usuario,
        //            Alterado_Em = a.Alterado_Em,
        //            Alterado_Por = a.Alterado_Por,
        //            Criado_Em = a.Criado_Em,
        //            Criado_Por = a.Criado_Por,
        //            Observacoes = a.Observacoes
        //        })
        //        .ToList();
        //    //
        //    foreach (ClienteObservacoesModels item in ClienteObservacoes_list)
        //    {
        //        item.Responsavel = PegaNomeResponsavel(item.Cd_Emppresa, item.Cd_Usuario);
        //    }
        //    //
        //    return Json(new { data = ClienteObservacoes_list.ToArray(), results = ClienteObservacoes_list.Count, success = true }, JsonRequestBehavior.AllowGet);
        //}
        ////
        //#endregion

        //public JsonResult EtiquetaReport(int empresa, int loja, string usuario, string cultura, int equipe, int procedencia, int opcaoBusca, string textoBusca, int periodo, int filtroempresa,
        //                                 int filtroloja, int aniversario, string dataInicial, string dataFinal, string representante)
        //{
        //    string fllistaRepresentantes = string.Empty;
        //    if (Session["_fllistarepresentantes"] != null)
        //    {
        //        fllistaRepresentantes = Session["_fllistarepresentantes"].ToString();
        //    }
        //    int totalRegistros = 0;
        //    List<Entities.Cliente> clienteList = Search(empresa, loja, usuario, cultura, equipe, procedencia, opcaoBusca, textoBusca, periodo,
        //                                                filtroempresa, filtroloja, aniversario, 1, Int32.MaxValue, 1, "CLI_Nome", "ASC", dataInicial, dataFinal,
        //                                                representante, fllistaRepresentantes, out totalRegistros);

        //    // Filtra somente os clientes com endereço completo
        //    clienteList = clienteList.Where(w => !String.IsNullOrEmpty(w.CLI_Endereco)
        //                                      && !String.IsNullOrEmpty(w.CLI_EnderecoNum)
        //                                      && !String.IsNullOrEmpty(w.CLI_Bairro)
        //                                      && !String.IsNullOrEmpty(w.CLI_Cidade)
        //                                      && !String.IsNullOrEmpty(w.CLI_Estado)
        //                                      && !String.IsNullOrEmpty(w.CLI_Cep)).ToList();

        //    List<ClienteEtiquetaModel> clienteEtiquetaModelList = ToEtiquetaModel(clienteList);
        //    ReportDocument clienteEtiquetaReport = new ReportDocument();
        //    clienteEtiquetaReport.Load(System.Web.HttpContext.Current.Server.MapPath("~/Reports/") + "ClienteEtiqueta.rpt");
        //    clienteEtiquetaReport.SetDataSource(clienteEtiquetaModelList);
        //    clienteEtiquetaReport.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, "ClienteEtiqueta");
        //    clienteEtiquetaReport.Close();
        //    //
        //    return Json(new
        //    {
        //        data = "OK",
        //        results = clienteEtiquetaModelList.Count(),
        //        success = true,
        //        errors = String.Empty
        //    }, JsonRequestBehavior.AllowGet);
        //}

        ////public List<ClienteEtiquetaModel> ToEtiquetaModel(List<Entities.Cliente> clienteList)
        ////{
        ////    List<ClienteEtiquetaModel> clienteEtiquetaModelList = new List<ClienteEtiquetaModel>();

        ////    foreach (Entities.Cliente item in clienteList)
        ////    {
        ////        ClienteEtiquetaModel model = new ClienteEtiquetaModel();
        ////        model.Id = item.GetHashCode().ToString();
        ////        model.CLI_Codigo = item.Id.CLI_Codigo.ToString();
        ////        model.CLI_Emp = item.Id.CLI_Emp.ToString();
        ////        model.CLI_Loja = item.Id.CLI_Loja.ToString();
        ////        model.Linha1 = String.IsNullOrEmpty(item.CLI_Nome) ? String.Empty : item.CLI_Nome.ToUpper();
        ////        model.Linha2 = String.IsNullOrEmpty(item.CLI_Endereco) ? String.Empty : item.CLI_Endereco.ToUpper();

        ////        if (!String.IsNullOrEmpty(item.CLI_EnderecoNum))
        ////        {
        ////            model.Linha2 += ", " + item.CLI_EnderecoNum.ToUpper();
        ////        }

        ////        model.Linha3 = String.IsNullOrEmpty(item.CLI_EnderecoCompl) ? String.Empty : item.CLI_EnderecoCompl.ToUpper();

        ////        model.Linha4 = String.Empty;

        ////        if (!String.IsNullOrEmpty(item.CLI_Bairro))
        ////        {
        ////            model.Linha4 = item.CLI_Bairro.ToUpper();
        ////        }

        ////        if (!String.IsNullOrEmpty(item.CLI_Cidade))
        ////        {
        ////            if (model.Linha4 != String.Empty)
        ////            {
        ////                model.Linha4 += " - ";
        ////            }

        ////            model.Linha4 += item.CLI_Cidade.ToUpper();
        ////        }

        ////        if (!String.IsNullOrEmpty(item.CLI_Estado))
        ////        {
        ////            if (model.Linha4 != String.Empty)
        ////            {
        ////                model.Linha4 += " - ";
        ////            }

        ////            model.Linha4 += item.CLI_Estado.ToUpper();
        ////        }

        ////        model.Linha5 = String.IsNullOrEmpty(item.CLI_Cep) ? String.Empty : item.CLI_Cep.Length == 9 ? "CEP " + String.Format("{0:00000-000}", Convert.ToInt32(item.CLI_Cep)) : "CEP " + item.CLI_Cep;

        ////        clienteEtiquetaModelList.Add(model);
        ////    }

        ////    return clienteEtiquetaModelList;
        ////}

        ////private List<Entities.Cliente> PegaClientesRepresentante(int pCdEmpresa, int pCdLoja, int representante)
        ////{
        ////    //
        ////    List<Entities.Cliente> lista = new List<Entities.Cliente>();
        ////    //
        ////    string sql = string.Empty;
        ////    //
        ////    #region SQL
        ////    sql = @"
        ////            SELECT cli_nome, 
        ////                   Count(pro_cliente)    AS Projetos,
        ////                   Isnull(U.ds_nome, '') AS Ds_Nome,
        ////                 CLI_Emp  AS CLI_Emp ,
        ////              CLI_Loja  AS CLI_Loja ,
        ////              CLI_Codigo  AS CLI_Codigo ,
        ////              CLI_CodigoFabrica  AS CLI_CodigoFabrica ,
        ////              CLI_SeqProjeto  AS CLI_SeqProjeto ,
        ////              CLI_Endereco  AS CLI_Endereco ,
        ////              CLI_Bairro  AS CLI_Bairro ,
        ////              CLI_Cidade  AS CLI_Cidade ,
        ////              CLI_Cep  AS CLI_Cep ,
        ////              CLI_Estado  AS CLI_Estado ,
        ////              CLI_DataNascimento  AS CLI_DataNascimento ,
        ////              CLI_Tipo  AS CLI_Tipo ,
        ////              CLI_Procedencia  AS CLI_Procedencia ,
        ////              CLI_CgcCpf  AS CLI_CgcCpf ,
        ////              CLI_InscRG  AS CLI_InscRG ,
        ////              CLI_FisJur  AS CLI_FisJur ,
        ////              CLI_Status  AS CLI_Status ,
        ////              CLI_Email  AS CLI_Email ,
        ////              CLI_EntEdificio  AS CLI_EntEdificio ,
        ////              CLI_EntReferencia  AS CLI_EntReferencia ,
        ////              CLI_EntEndereco  AS CLI_EntEndereco ,
        ////              CLI_EntBairro  AS CLI_EntBairro ,
        ////              CLI_EntCidade  AS CLI_EntCidade ,
        ////              CLI_EntCep  AS CLI_EntCep ,
        ////              CLI_EntEstado  AS CLI_EntEstado ,
        ////              CLI_EntFax  AS CLI_EntFax ,
        ////              CLI_EntFone1  AS CLI_EntFone1 ,
        ////              CLI_EntFone2  AS CLI_EntFone2 ,
        ////              CLI_EntPais  AS CLI_EntPais ,
        ////              CLI_EntCgcCpf  AS CLI_EntCgcCpf ,
        ////              CLI_EntInscRG  AS CLI_EntInscRG ,
        ////              CLI_CobEndereco  AS CLI_CobEndereco ,
        ////              CLI_CobBairro  AS CLI_CobBairro ,
        ////              CLI_CobCidade  AS CLI_CobCidade ,
        ////              CLI_CobCep  AS CLI_CobCep ,
        ////              CLI_CobEstado  AS CLI_CobEstado ,
        ////              CLI_CobFax  AS CLI_CobFax ,
        ////              CLI_CobFone1  AS CLI_CobFone1 ,
        ////              CLI_CobFone2  AS CLI_CobFone2 ,
        ////              CLI_CobPais  AS CLI_CobPais ,
        ////              CLI_CobCgcCpf  AS CLI_CobCgcCpf ,
        ////              CLI_CobInscRG  AS CLI_CobInscRG ,
        ////              CLI_CorEndereco  AS CLI_CorEndereco ,
        ////              CLI_CorBairro  AS CLI_CorBairro ,
        ////              CLI_CorCidade  AS CLI_CorCidade ,
        ////              CLI_CorCep  AS CLI_CorCep ,
        ////              CLI_CorEstado  AS CLI_CorEstado ,
        ////              CLI_CorFax  AS CLI_CorFax ,
        ////              CLI_CorFone1  AS CLI_CorFone1 ,
        ////              CLI_CorFone2  AS CLI_CorFone2 ,
        ////              CLI_CorPais  AS CLI_CorPais ,
        ////              CLI_DataEnvio  AS CLI_DataEnvio ,
        ////              CLI_DataRomaneio  AS CLI_DataRomaneio ,
        ////              CLI_DataCadastro  AS CLI_DataCadastro ,
        ////              CLI_HoraCadastro  AS CLI_HoraCadastro ,
        ////              CLI_UserCadastro  AS CLI_UserCadastro ,
        ////              CLI_DataAltera  AS CLI_DataAltera ,
        ////              CLI_HoraAltera  AS CLI_HoraAltera ,
        ////              CLI_UserAltera  AS CLI_UserAltera ,
        ////              CLI_UserEquipe  AS CLI_UserEquipe ,
        ////              CLI_Obs  AS CLI_Obs ,
        ////              CLI_DataObra  AS CLI_DataObra ,
        ////              CLI_Contribuinte  AS CLI_Contribuinte ,
        ////              CLI_Suframa  AS CLI_Suframa ,
        ////              CLI_Profissao  AS CLI_Profissao ,
        ////              CLI_EnderecoNum  AS CLI_EnderecoNum ,
        ////              CLI_EnderecoCompl  AS CLI_EnderecoCompl ,
        ////              CLI_EntEnderecoNum  AS CLI_EntEnderecoNum ,
        ////              CLI_EntEnderecoCompl  AS CLI_EntEnderecoCompl ,
        ////              CLI_CobEnderecoNum  AS CLI_CobEnderecoNum ,
        ////              CLI_CobEnderecoCompl  AS CLI_CobEnderecoCompl ,
        ////              CLI_PrevisaoFechamento  AS CLI_PrevisaoFechamento ,
        ////              CLI_CodCidadeIBGE  AS CLI_CodCidadeIBGE ,
        ////              CLI_EntCodCidadeIBGE  AS CLI_EntCodCidadeIBGE ,
        ////              CLI_CobCodCidadeIBGE  AS CLI_CobCodCidadeIBGE ,
        ////              cli_Fone1  AS cli_Fone1 ,
        ////              cli_Fone2  AS cli_Fone2 ,
        ////              CLI_TelReferencia2  AS CLI_TelReferencia2 ,
        ////              CLI_DDD_TelReferencia2  AS CLI_DDD_TelReferencia2 ,
        ////              CLI_DDD_CorFone1  AS CLI_DDD_CorFone1 ,
        ////              CLI_DDD_CorFone2  AS CLI_DDD_CorFone2 ,
        ////              CLI_DDD_CobFone1  AS CLI_DDD_CobFone1 ,
        ////              CLI_DDD_CobFone2  AS CLI_DDD_CobFone2 ,
        ////              CLI_DDD_TelReferencia1  AS CLI_DDD_TelReferencia1 ,
        ////              CLI_TelReferencia1  AS CLI_TelReferencia1 ,
        ////              CLI_DDD_CobFax  AS CLI_DDD_CobFax ,
        ////              CLI_DDD_CobTelefoneEmpresa  AS CLI_DDD_CobTelefoneEmpresa ,
        ////              CLI_DDD_EntFax  AS CLI_DDD_EntFax ,
        ////              CLI_DDD_EntFone1  AS CLI_DDD_EntFone1 ,
        ////              CLI_DDD_EntFone2  AS CLI_DDD_EntFone2 ,
        ////              CLI_DataEmissaoRG  AS CLI_DataEmissaoRG ,
        ////              CLI_OrgEmissorRG  AS CLI_OrgEmissorRG ,
        ////              CLI_NomeReferencia1  AS CLI_NomeReferencia1 ,
        ////              CLI_NomeReferencia2  AS CLI_NomeReferencia2 ,
        ////              CLI_DDD_FoneAgencia  AS CLI_DDD_FoneAgencia ,
        ////              CLI_CobTipoResidencia  AS CLI_CobTipoResidencia ,
        ////              CLI_CobTempoResidencia  AS CLI_CobTempoResidencia ,
        ////              CLI_CobNomeMae  AS CLI_CobNomeMae ,
        ////              CLI_CobNomePai  AS CLI_CobNomePai ,
        ////              CLI_CobNaturalidade  AS CLI_CobNaturalidade ,
        ////              CLI_CobEstadoCivil  AS CLI_CobEstadoCivil ,
        ////              CLI_CobNomeEmpresa  AS CLI_CobNomeEmpresa ,
        ////              CLI_CNPJTrabalho  AS CLI_CNPJTrabalho ,
        ////              CLI_ComEndereco  AS CLI_ComEndereco ,
        ////              CLI_ComEnderecoNum  AS CLI_ComEnderecoNum ,
        ////              CLI_ComEnderecoCompl  AS CLI_ComEnderecoCompl ,
        ////              CLI_ComBairro  AS CLI_ComBairro ,
        ////              CLI_ComCEP  AS CLI_ComCEP ,
        ////              CLI_ComEstado  AS CLI_ComEstado ,
        ////              CLI_CobOcupacao  AS CLI_CobOcupacao ,
        ////              CLI_CobTelefoneEmpresa  AS CLI_CobTelefoneEmpresa ,
        ////              CLI_CobDataAdmissao  AS CLI_CobDataAdmissao ,
        ////              CLI_RendaMensal  AS CLI_RendaMensal ,
        ////              CLI_Banco  AS CLI_Banco ,
        ////              CLI_Agencia  AS CLI_Agencia ,
        ////              CLI_FoneAgencia  AS CLI_FoneAgencia ,
        ////              CLI_CTABanco  AS CLI_CTABanco ,
        ////              CLI_CTADesde  AS CLI_CTADesde ,
        ////              CLI_ComCodCidadeIBGE  AS CLI_ComCodCidadeIBGE ,
        ////              CLI_ConjugeNome  AS CLI_ConjugeNome ,
        ////              CLI_ConjugeCPF  AS CLI_ConjugeCPF ,
        ////              CLI_ConjugeRG  AS CLI_ConjugeRG ,
        ////              CLI_ConjugeDataNascimento  AS CLI_ConjugeDataNascimento ,
        ////              CLI_DDD_ConjugeFone  AS CLI_DDD_ConjugeFone ,
        ////              CLI_ConjugeFone  AS CLI_ConjugeFone ,
        ////              CLI_ConjugeNomeEmpresa  AS CLI_ConjugeNomeEmpresa ,
        ////              CLI_ConjugeCNPJTrabalho  AS CLI_ConjugeCNPJTrabalho ,
        ////              CLI_ConjugeRendaMensal  AS CLI_ConjugeRendaMensal ,
        ////              CLI_Pais  AS CLI_Pais ,
        ////              CLI_ComPais  AS CLI_ComPais ,
        ////              CLI_AtualizadoeFinance  AS CLI_AtualizadoeFinance

        ////            FROM   cliente 
        ////                   LEFT JOIN projeto 
        ////                          ON ( cli_emp = pro_emp 
        ////                               AND cli_loja = pro_loja 
        ////                               AND cli_codigo = pro_cliente ) 
        ////                   LEFT JOIN Usuario U 
        ////                          ON ( pro_emp = U.cd_empresa 
        ////                               AND cli_usercadastro = U.cd_representante ) 

        ////            WHERE  ( cli_emp = " + pCdEmpresa + @" ) 
        ////                    AND ( cli_loja = " + pCdLoja + @" ) 
        ////                    AND ( cli_usercadastro = " + representante + @" ) 
        ////                    AND ( cli_seqprojeto > 0 ) 
        ////                    AND ( pro_datavenda IS NULL ) 
        ////                    AND ( pro_dataperdido IS NULL ) 
        ////            GROUP  BY cli_codigo, 
        ////                      cli_nome, 
        ////                      cli_datacadastro, 
        ////                      cli_horacadastro, 
        ////                      cli_loja, 
        ////                      cli_usercadastro, 
        ////                      U.ds_nome,
        ////               CLI_Emp ,
        ////               CLI_Loja ,
        ////               CLI_Codigo ,
        ////               CLI_Nome ,
        ////               CLI_CodigoFabrica ,
        ////               CLI_SeqProjeto ,
        ////               CLI_Endereco ,
        ////               CLI_Bairro ,
        ////               CLI_Cidade ,
        ////               CLI_Cep ,
        ////               CLI_Estado ,
        ////               CLI_DataNascimento ,
        ////               CLI_Tipo ,
        ////               CLI_Procedencia ,
        ////               CLI_CgcCpf ,
        ////               CLI_InscRG ,
        ////               CLI_FisJur ,
        ////               CLI_Status ,
        ////               CLI_Email ,
        ////               CLI_EntEdificio ,
        ////               CLI_EntReferencia ,
        ////               CLI_EntEndereco ,
        ////               CLI_EntBairro ,
        ////               CLI_EntCidade ,
        ////               CLI_EntCep ,
        ////               CLI_EntEstado ,
        ////               CLI_EntFax ,
        ////               CLI_EntFone1 ,
        ////               CLI_EntFone2 ,
        ////               CLI_EntPais ,
        ////               CLI_EntCgcCpf ,
        ////               CLI_EntInscRG ,
        ////               CLI_CobEndereco ,
        ////               CLI_CobBairro ,
        ////               CLI_CobCidade ,
        ////               CLI_CobCep ,
        ////               CLI_CobEstado ,
        ////               CLI_CobFax ,
        ////               CLI_CobFone1 ,
        ////               CLI_CobFone2 ,
        ////               CLI_CobPais ,
        ////               CLI_CobCgcCpf ,
        ////               CLI_CobInscRG ,
        ////               CLI_CorEndereco ,
        ////               CLI_CorBairro ,
        ////               CLI_CorCidade ,
        ////               CLI_CorCep ,
        ////               CLI_CorEstado ,
        ////               CLI_CorFax ,
        ////               CLI_CorFone1 ,
        ////               CLI_CorFone2 ,
        ////               CLI_CorPais ,
        ////               CLI_DataEnvio ,
        ////               CLI_DataRomaneio ,
        ////               CLI_DataCadastro ,
        ////               CLI_HoraCadastro ,
        ////               CLI_UserCadastro ,
        ////               CLI_DataAltera ,
        ////               CLI_HoraAltera ,
        ////               CLI_UserAltera ,
        ////               CLI_UserEquipe ,
        ////               CLI_Obs ,
        ////               CLI_DataObra ,
        ////               CLI_Contribuinte ,
        ////               CLI_Suframa ,
        ////               CLI_Profissao ,
        ////               CLI_EnderecoNum ,
        ////               CLI_EnderecoCompl ,
        ////               CLI_EntEnderecoNum ,
        ////               CLI_EntEnderecoCompl ,
        ////               CLI_CobEnderecoNum ,
        ////               CLI_CobEnderecoCompl ,
        ////               CLI_PrevisaoFechamento ,
        ////               CLI_CodCidadeIBGE ,
        ////               CLI_EntCodCidadeIBGE ,
        ////               CLI_CobCodCidadeIBGE ,
        ////               cli_Fone1 ,
        ////               cli_Fone2 ,
        ////               CLI_TelReferencia2 ,
        ////               CLI_DDD_TelReferencia2 ,
        ////               CLI_DDD_CorFone1 ,
        ////               CLI_DDD_CorFone2 ,
        ////               CLI_DDD_CobFone1 ,
        ////               CLI_DDD_CobFone2 ,
        ////               CLI_DDD_TelReferencia1 ,
        ////               CLI_TelReferencia1 ,
        ////               CLI_DDD_CobFax ,
        ////               CLI_DDD_CobTelefoneEmpresa ,
        ////               CLI_DDD_EntFax ,
        ////               CLI_DDD_EntFone1 ,
        ////               CLI_DDD_EntFone2 ,
        ////               CLI_DataEmissaoRG ,
        ////               CLI_OrgEmissorRG ,
        ////               CLI_NomeReferencia1 ,
        ////               CLI_NomeReferencia2 ,
        ////               CLI_DDD_FoneAgencia ,
        ////               CLI_CobTipoResidencia ,
        ////               CLI_CobTempoResidencia ,
        ////               CLI_CobNomeMae ,
        ////               CLI_CobNomePai ,
        ////               CLI_CobNaturalidade ,
        ////               CLI_CobEstadoCivil ,
        ////               CLI_CobNomeEmpresa ,
        ////               CLI_CNPJTrabalho ,
        ////               CLI_ComEndereco ,
        ////               CLI_ComEnderecoNum ,
        ////               CLI_ComEnderecoCompl ,
        ////               CLI_ComBairro ,
        ////               CLI_ComCEP ,
        ////               CLI_ComEstado ,
        ////               CLI_DDD_CobTelefoneEmpresa ,
        ////               CLI_CobOcupacao ,
        ////               CLI_CobTelefoneEmpresa ,
        ////               CLI_CobDataAdmissao ,
        ////               CLI_RendaMensal ,
        ////               CLI_Banco ,
        ////               CLI_Agencia ,
        ////               CLI_FoneAgencia ,
        ////               CLI_CTABanco ,
        ////               CLI_CTADesde ,
        ////               CLI_ComCodCidadeIBGE ,
        ////               CLI_ConjugeNome ,
        ////               CLI_ConjugeCPF ,
        ////               CLI_ConjugeRG ,
        ////               CLI_ConjugeDataNascimento ,
        ////               CLI_DDD_ConjugeFone ,
        ////               CLI_ConjugeFone ,
        ////               CLI_ConjugeNomeEmpresa ,
        ////               CLI_ConjugeCNPJTrabalho ,
        ////               CLI_ConjugeRendaMensal ,
        ////               CLI_Pais ,
        ////               CLI_ComPais ,
        ////               CLI_AtualizadoeFinance

        ////            HAVING ( Count(pro_cliente) > 0 ) 
        ////            ORDER  BY cli_nome ";
        ////    #endregion
        ////    //
        ////    DataTable oDataTable = ExecutaSQL(sql);
        ////    if (oDataTable.Rows != null && oDataTable.Rows.Count > 0)
        ////    {
        ////        foreach (DataRow item in oDataTable.Rows)
        ////        {
        ////            Entities.Cliente clientetmp = new Entities.Cliente();
        ////            //
        ////            try
        ////            {
        ////                clientetmp.Id = new ClienteIdentifier();
        ////                clientetmp.Id.CLI_Codigo = Convert.ToInt32(item["CLI_Codigo"]);
        ////                clientetmp.Id.CLI_Emp = Convert.ToInt32(item["CLI_Emp"]);
        ////                clientetmp.Id.CLI_Loja = Convert.ToInt32(item["CLI_Loja"]);
        ////                //
        ////                clientetmp.CLI_Nome = item["CLI_Nome"].ToString();
        ////                clientetmp.CLI_CodigoFabrica = Convert.ToInt32(item["CLI_CodigoFabrica"]);
        ////                clientetmp.CLI_SeqProjeto = Convert.ToInt32(item["CLI_SeqProjeto"]);
        ////                clientetmp.CLI_Endereco = item["CLI_Endereco"].ToString();
        ////                clientetmp.CLI_Bairro = item["CLI_Bairro"].ToString();
        ////                clientetmp.CLI_Cidade = item["CLI_Cidade"].ToString();
        ////                clientetmp.CLI_Cep = item["CLI_Cep"].ToString();
        ////                clientetmp.CLI_Estado = item["CLI_Estado"].ToString();
        ////                clientetmp.CLI_Procedencia = PegaCodigoInt(item, "CLI_Procedencia");
        ////                clientetmp.CLI_CgcCpf = item["CLI_CgcCpf"].ToString();
        ////                clientetmp.CLI_InscRG = item["CLI_InscRG"].ToString();
        ////                clientetmp.CLI_FisJur = item["CLI_FisJur"].ToString();
        ////                clientetmp.CLI_Status = item["CLI_Status"].ToString();
        ////                clientetmp.CLI_Email = item["CLI_Email"].ToString();
        ////                clientetmp.CLI_EntEndereco = item["CLI_EntEndereco"].ToString();
        ////                clientetmp.CLI_UserCadastro = PegaCodigoInt(item, "CLI_UserCadastro");
        ////                clientetmp.CLI_UserEquipe = PegaCodigoInt(item, "CLI_UserEquipe");
        ////                clientetmp.CLI_UserEquipeDescricao = PegaNomeEquipe(pCdEmpresa, clientetmp.CLI_Nome, pCdLoja, clientetmp.CLI_UserEquipe);
        ////                //
        ////                clientetmp.CLI_EntEdificio = item["CLI_EntEdificio"].ToString();
        ////                clientetmp.CLI_EntReferencia = item["CLI_EntReferencia"].ToString();
        ////                clientetmp.CLI_EntBairro = item["CLI_EntBairro"].ToString();
        ////                clientetmp.CLI_EntCidade = item["CLI_EntCidade"].ToString();
        ////                clientetmp.CLI_EntCep = item["CLI_EntCep"].ToString();
        ////                clientetmp.CLI_EntEstado = item["CLI_EntEstado"].ToString();
        ////                clientetmp.CLI_EntFax = item["CLI_EntFax"].ToString();
        ////                clientetmp.CLI_EntFone1 = item["CLI_EntFone1"].ToString();
        ////                clientetmp.CLI_EntFone2 = item["CLI_EntFone2"].ToString();
        ////                clientetmp.CLI_EntCgcCpf = item["CLI_EntCgcCpf"].ToString();
        ////                clientetmp.CLI_EntInscRG = item["CLI_EntInscRG"].ToString();
        ////                //
        ////                clientetmp.CLI_CobEndereco = item["CLI_CobEndereco"].ToString();
        ////                clientetmp.CLI_CobBairro = item["CLI_CobBairro"].ToString();
        ////                clientetmp.CLI_CobCidade = item["CLI_CobCidade"].ToString();
        ////                clientetmp.CLI_CobCep = item["CLI_CobCep"].ToString();
        ////                clientetmp.CLI_CobEstado = item["CLI_CobEstado"].ToString();
        ////                clientetmp.CLI_CobFax = item["CLI_CobFax"].ToString();
        ////                clientetmp.CLI_CobFone1 = item["CLI_CobFone1"].ToString();
        ////                clientetmp.CLI_CobFone2 = item["CLI_CobFone2"].ToString();
        ////                clientetmp.CLI_CobCgcCpf = item["CLI_CobCgcCpf"].ToString();
        ////                clientetmp.CLI_CobInscRG = item["CLI_CobInscRG"].ToString();
        ////                clientetmp.CLI_CorEndereco = item["CLI_CorEndereco"].ToString();
        ////                clientetmp.CLI_CorBairro = item["CLI_CorBairro"].ToString();
        ////                clientetmp.CLI_CorCidade = item["CLI_CorCidade"].ToString();
        ////                clientetmp.CLI_CorCep = item["CLI_CorCep"].ToString();
        ////                clientetmp.CLI_CorEstado = item["CLI_CorEstado"].ToString();
        ////                clientetmp.CLI_CorFax = item["CLI_CorFax"].ToString();
        ////                clientetmp.CLI_CorFone1 = item["CLI_CorFone1"].ToString();
        ////                clientetmp.CLI_CorFone2 = item["CLI_CorFone2"].ToString();
        ////                //
        ////                clientetmp.CLI_ConjugeNome = item["CLI_ConjugeNome"].ToString();
        ////                clientetmp.CLI_ConjugeCPF = item["CLI_ConjugeCPF"].ToString();
        ////                clientetmp.CLI_ConjugeRG = item["CLI_ConjugeRG"].ToString();
        ////                clientetmp.CLI_ConjugeFone = item["CLI_ConjugeFone"].ToString();
        ////                clientetmp.CLI_ConjugeNomeEmpresa = item["CLI_ConjugeNomeEmpresa"].ToString();
        ////                clientetmp.CLI_ConjugeCNPJTrabalho = item["CLI_ConjugeCNPJTrabalho"].ToString();
        ////                //
        ////                clientetmp.CLI_Suframa = item["CLI_Suframa"].ToString();
        ////                clientetmp.CLI_Profissao = item["CLI_Profissao"].ToString();
        ////                clientetmp.CLI_EnderecoNum = item["CLI_EnderecoNum"].ToString();
        ////                clientetmp.CLI_EnderecoCompl = item["CLI_EnderecoCompl"].ToString();
        ////                clientetmp.CLI_EntEnderecoNum = item["CLI_EntEnderecoNum"].ToString();
        ////                clientetmp.CLI_EntEnderecoCompl = item["CLI_EntEnderecoCompl"].ToString();
        ////                clientetmp.CLI_CobEnderecoNum = item["CLI_CobEnderecoNum"].ToString();
        ////                clientetmp.CLI_CobEnderecoCompl = item["CLI_CobEnderecoCompl"].ToString();
        ////                //                    
        ////                clientetmp.CLI_CobTempoResidencia = item["CLI_CobTempoResidencia"].ToString();
        ////                clientetmp.CLI_CobNomeMae = item["CLI_CobNomeMae"].ToString();
        ////                clientetmp.CLI_CobNomePai = item["CLI_CobNomePai"].ToString();
        ////                clientetmp.CLI_CobNaturalidade = item["CLI_CobNaturalidade"].ToString();
        ////                clientetmp.CLI_CobNomeEmpresa = item["CLI_CobNomeEmpresa"].ToString();
        ////                clientetmp.CLI_CNPJTrabalho = item["CLI_CNPJTrabalho"].ToString();
        ////                clientetmp.CLI_ComEndereco = item["CLI_ComEndereco"].ToString();
        ////                clientetmp.CLI_ComEnderecoNum = item["CLI_ComEnderecoNum"].ToString();
        ////                clientetmp.CLI_ComEnderecoCompl = item["CLI_ComEnderecoCompl"].ToString();
        ////                clientetmp.CLI_ComBairro = item["CLI_ComBairro"].ToString();
        ////                clientetmp.CLI_ComCEP = item["CLI_ComCEP"].ToString();
        ////                clientetmp.CLI_ComEstado = item["CLI_ComEstado"].ToString();
        ////                clientetmp.CLI_CobOcupacao = item["CLI_CobOcupacao"].ToString();
        ////                clientetmp.CLI_CobTelefoneEmpresa = item["CLI_CobTelefoneEmpresa"].ToString();
        ////                //
        ////                if (!string.IsNullOrEmpty(item["CLI_CobEstadoCivil"].ToString()))
        ////                {
        ////                    clientetmp.CLI_CobEstadoCivil = Convert.ToInt64(item["CLI_CobEstadoCivil"].ToString());
        ////                }
        ////                //
        ////                if (!string.IsNullOrEmpty(item["CLI_CobTipoResidencia"].ToString()))
        ////                {
        ////                    clientetmp.CLI_CobTipoResidencia = Convert.ToInt64(item["CLI_CobTipoResidencia"].ToString());
        ////                }
        ////                //
        ////                clientetmp.CLI_DataAltera = PegaData(item, "CLI_DataAltera");
        ////                clientetmp.CLI_DataCadastro = PegaData(item, "CLI_DataCadastro");
        ////                clientetmp.CLI_DataEmissaoRG = PegaData(item, "CLI_DataEmissaoRG");
        ////                clientetmp.CLI_DataEnvio = PegaData(item, "CLI_DataEnvio");
        ////                clientetmp.CLI_DataNascimento = PegaData(item, "CLI_DataNascimento");
        ////                clientetmp.CLI_DataObra = PegaData(item, "CLI_DataObra");
        ////                clientetmp.CLI_DataRomaneio = PegaData(item, "CLI_DataRomaneio");
        ////                clientetmp.CLI_HoraCadastro = PegaData(item, "CLI_HoraCadastro");
        ////                clientetmp.CLI_HoraAltera = PegaData(item, "CLI_HoraAltera");
        ////                clientetmp.CLI_PrevisaoFechamento = PegaData(item, "CLI_PrevisaoFechamento");
        ////                clientetmp.CLI_CobDataAdmissao = PegaData(item, "CLI_CobDataAdmissao");
        ////                clientetmp.CLI_ConjugeDataNascimento = PegaData(item, "CLI_ConjugeDataNascimento");
        ////                //
        ////                clientetmp.CLI_CobPais = PegaCodigoInt(item, "CLI_CobPais");
        ////                clientetmp.CLI_EntPais = PegaCodigoInt(item, "CLI_EntPais");
        ////                clientetmp.CLI_UserAltera = PegaCodigoInt(item, "CLI_UserAltera");
        ////                clientetmp.CLI_Tipo = PegaBoolean(item, "CLI_Tipo");
        ////                clientetmp.CLI_Contribuinte = PegaBoolean(item, "CLI_Contribuinte");
        ////                clientetmp.CLI_AtualizadoeFinance = PegaBoolean(item, "CLI_AtualizadoeFinance");
        ////                clientetmp.CLI_Obs = item["CLI_Obs"].ToString();
        ////                clientetmp.CLI_CodCidadeIBGE = PegaCodigoInt(item, "CLI_CodCidadeIBGE");
        ////                clientetmp.CLI_EntCodCidadeIBGE = PegaCodigoInt(item, "CLI_EntCodCidadeIBGE");
        ////                clientetmp.CLI_CobCodCidadeIBGE = PegaCodigoInt(item, "CLI_CobCodCidadeIBGE");
        ////                clientetmp.CLI_TelReferencia2 = item["CLI_TelReferencia2"].ToString();
        ////                clientetmp.CLI_DDD_TelReferencia2 = item["CLI_DDD_TelReferencia2"].ToString();
        ////                //
        ////                clientetmp.CLI_DDD_CorFone1 = item["CLI_DDD_CorFone1"].ToString();
        ////                clientetmp.CLI_DDD_CorFone2 = item["CLI_DDD_CorFone2"].ToString();
        ////                clientetmp.CLI_DDD_CobFone1 = item["CLI_DDD_CobFone1"].ToString();
        ////                clientetmp.CLI_DDD_CobFone2 = item["CLI_DDD_CobFone2"].ToString();
        ////                clientetmp.CLI_DDD_TelReferencia1 = item["CLI_DDD_TelReferencia1"].ToString();
        ////                clientetmp.CLI_TelReferencia1 = item["CLI_TelReferencia1"].ToString();
        ////                clientetmp.CLI_DDD_CobFax = item["CLI_DDD_CobFax"].ToString();
        ////                clientetmp.CLI_DDD_CobTelefoneEmpresa = item["CLI_DDD_CobTelefoneEmpresa"].ToString();
        ////                clientetmp.CLI_DDD_EntFax = item["CLI_DDD_EntFax"].ToString();
        ////                clientetmp.CLI_DDD_EntFone1 = item["CLI_DDD_EntFone1"].ToString();
        ////                clientetmp.CLI_DDD_EntFone2 = item["CLI_DDD_EntFone2"].ToString();
        ////                clientetmp.CLI_OrgEmissorRG = item["CLI_OrgEmissorRG"].ToString();
        ////                clientetmp.CLI_NomeReferencia1 = item["CLI_NomeReferencia1"].ToString();
        ////                clientetmp.CLI_NomeReferencia2 = item["CLI_NomeReferencia2"].ToString();
        ////                clientetmp.CLI_DDD_FoneAgencia = item["CLI_DDD_FoneAgencia"].ToString();
        ////                clientetmp.CLI_Agencia = item["CLI_Agencia"].ToString();
        ////                clientetmp.CLI_FoneAgencia = item["CLI_FoneAgencia"].ToString();
        ////                clientetmp.CLI_CTABanco = item["CLI_CTABanco"].ToString();
        ////                clientetmp.CLI_CTADesde = item["CLI_CTADesde"].ToString();
        ////                clientetmp.CLI_Pais = PegaCodigoInt(item, "CLI_Pais");
        ////                clientetmp.CLI_ComPais = PegaCodigoInt(item, "CLI_ComPais");
        ////                clientetmp.CLI_DDD_ConjugeFone = item["CLI_DDD_ConjugeFone"].ToString();
        ////                clientetmp.CLI_ConjugeRendaMensal = PegaCodigoDoublet(item, "CLI_ConjugeRendaMensal");
        ////            }
        ////            catch (Exception exc)
        ////            {
        ////            }
        ////            //
        ////            lista.Add(clientetmp);
        ////        }
        ////    }
        ////    //            
        ////    return lista;
        ////}

        //private int PegaCodigoInt(DataRow row, string campo)
        //{
        //    //
        //    int i = 0;
        //    //
        //    if (row != null)
        //    {
        //        Int32.TryParse(row[campo].ToString(), out i);
        //    }
        //    //
        //    return i;
        //}

        //private double PegaCodigoDoublet(DataRow row, string campo)
        //{
        //    //
        //    double i = 0;
        //    //
        //    if (row != null)
        //    {
        //        Double.TryParse(row[campo].ToString(), out i);
        //    }
        //    //
        //    return i;
        //}

        //private bool PegaBoolean(DataRow row, string campo)
        //{
        //    //
        //    bool bret = false;
        //    //
        //    if (row != null)
        //    {
        //        if (row[campo].ToString().Equals("1") || row[campo].ToString().ToLower().Equals("true"))
        //        {
        //            bret = true;
        //        }
        //    }
        //    //
        //    return bret;
        //}

        //private DateTime? PegaData(DataRow row, string campo)
        //{
        //    DateTime dateValue = new DateTime();
        //    //
        //    if (row != null)
        //    {
        //        if (DateTime.TryParse(row[campo].ToString(), out dateValue))
        //        {
        //            return dateValue;
        //        }
        //    }
        //    //
        //    return null;
        //}

        ////private List<Entities.Cliente> FiltroTextBuscaRepresentante(List<Entities.Cliente> lista, string texto, int opcao, string dataInicial, string dataFinal, int periodo)
        ////{
        ////    //
        ////    List<Entities.Cliente> lista_retorno = new List<Entities.Cliente>();
        ////    //
        ////    if (!string.IsNullOrEmpty(texto))
        ////    {
        ////        switch (opcao)
        ////        {
        ////            case 1: // Código cliente
        ////                //
        ////                int icodigocliente = Convert.ToInt32(texto);
        ////                lista_retorno = lista.FindAll(a => a.CLI_Codigo == icodigocliente).ToList();
        ////                break;

        ////            case 2: // Nome / Razão Social
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_Nome.ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 3: // CPF/CNPJ
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_CgcCpf.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 4: // RG/IE
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_InscRG.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 6: // Endereço
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_Endereco.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 7: // Endereço de Entrega
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_EntEndereco.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 8: // Bairro
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_Bairro.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 9: // Bairro de Entrega
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_EntBairro.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 10: // Cidade
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_Cidade.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 11: // Estado
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_Estado.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 12: // Cidade de Entrega
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_EntCidade.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 13: // Todos os Telefones
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_CorFone1.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_CorFone2.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_CobFone1.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_CobFone2.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_TelReferencia1.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_TelReferencia2.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_CobTelefoneEmpresa.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_FoneAgencia.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_ConjugeFone.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_EntFone1.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_EntFone2.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 14: // Email
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_Email.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 15: // Edifíio
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_EntEdificio.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 16: // Referência
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_EntReferencia.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_NomeReferencia1.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_TelReferencia2.Trim().ToLower().Contains(texto.ToLower()) ||
        ////                                                   a.CLI_NomeReferencia2.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;

        ////            case 18: // Nome do Conjuge
        ////                //
        ////                lista_retorno = lista.FindAll(a => a.CLI_ConjugeNome.Trim().ToLower().Contains(texto.ToLower())).ToList();
        ////                break;
        ////        }
        ////    }
        ////    else
        ////    {
        ////        if (!string.IsNullOrEmpty(dataInicial) && !string.IsNullOrEmpty(dataFinal))
        ////        {
        ////            DateTime dtdataInicial = Convert.ToDateTime(dataInicial, culture);
        ////            DateTime dtdataFinal = Convert.ToDateTime(dataFinal, culture);
        ////            lista_retorno = lista.FindAll(a => a.CLI_DataCadastro >= dtdataInicial.Date && a.CLI_DataCadastro <= dtdataFinal.Date).ToList();
        ////        }
        ////        else
        ////        {
        ////            DateTime dtdataInicial = new DateTime();
        ////            DateTime dtdataFinal = new DateTime();
        ////            //
        ////            switch (periodo)
        ////            {
        ////                case 0:
        ////                    lista_retorno = lista;
        ////                    break;
        ////                case 3:
        ////                    //
        ////                    dtdataInicial = DateTime.Now.Date.AddDays(-90);
        ////                    dtdataFinal = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
        ////                    lista_retorno = lista.FindAll(a => a.CLI_DataCadastro >= dtdataInicial && a.CLI_DataCadastro <= dtdataFinal).ToList();
        ////                    break;
        ////                case 6:
        ////                    //
        ////                    dtdataInicial = DateTime.Now.Date.AddDays(-180);
        ////                    dtdataFinal = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
        ////                    lista_retorno = lista.FindAll(a => a.CLI_DataCadastro >= dtdataInicial.Date && a.CLI_DataCadastro <= dtdataFinal.Date).ToList();
        ////                    break;
        ////                case 12:
        ////                    //
        ////                    dtdataInicial = DateTime.Now.Date.AddDays(-365);
        ////                    dtdataFinal = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
        ////                    lista_retorno = lista.FindAll(a => a.CLI_DataCadastro >= dtdataInicial.Date && a.CLI_DataCadastro <= dtdataFinal.Date).ToList();
        ////                    break;
        ////            }
        ////        }
        ////    }
        ////    //
        ////    return lista_retorno;
        ////}

        //public JsonResult PegaCliente(int empresa, int loja, string usuario, string cultura, int ftCdEmpresa, int ftCdLoja, int clicodigo)
        //{
        //    ClienteMethods cliente_methods = new ClienteMethods(empresa, usuario);

        //    List<Entities.Cliente> clienteList = cliente_methods
        //                                        .FindAll(a => a.Id.CLI_Emp == ftCdEmpresa
        //                                                  && a.Id.CLI_Loja == ftCdLoja
        //                                                  && a.Id.CLI_Codigo == clicodigo)
        //                                        .ToList();

        //    List<ClienteModels> clienteModelsList = ToModel(empresa, usuario, clienteList);
        //    //
        //    return Json(new { data = clienteModelsList[0], results = 1, success = true }, JsonRequestBehavior.AllowGet);
        //}

        ///// <summary>
        /////     Lista as Transportadoras (Clientes) para seleção.
        ///// </summary>          
        ///// <param name="empresa">Código da Empresa logada.</param>
        ///// <param name="loja">Código da Loja logada.</param>
        ///// <param name="usuario">Código do Usuário logado.</param>
        ///// <param name="ftCdEmpresa">Código da Empresa das Transportadora.</param>
        ///// <param name="ftCdLoja">Código da Loja das Transportadora.</param>
        ///// <param name="query">Filtro desejado para o nome do cliente.</param>
        ///// <param name="page">Número da página que se deseja visualizar.</param>        
        ///// <param name="limit">Quantidade limite de registros por página.</param>       
        //public JsonResult Transportadoras(int empresa, int loja, string usuario, int empresa_acao, int ftCdEmpresa, int ftCdLoja, string query, int page, int limit)
        //{
        //    ClienteMethods clienteMethods = new ClienteMethods(empresa, usuario);

        //    int numRegs;

        //    if (!String.IsNullOrEmpty(query))
        //    {
        //        query = query.Trim();
        //    }

        //    List<Entities.Cliente> clientesList = clienteMethods
        //                                         .FindAll(q => q.Id.CLI_Emp == ftCdEmpresa
        //                                                    && q.Id.CLI_Loja == ftCdLoja
        //                                                    && (q.CLI_Nome.Trim().Contains(query) || String.IsNullOrEmpty(query))
        //                                                    && q.CLI_CgcCpf != null
        //                                                  , out numRegs
        //                                                  , (limit * (page - 1))
        //                                                  , limit
        //                                                  , "CLI_Nome"
        //                                                  , "ASC");

        //    List<ClienteModels> clienteModelsList = ToModel(empresa, usuario, clientesList);

        //    return Json(new
        //    {
        //        data = clienteModelsList.ToArray(),
        //        results = numRegs,
        //        success = true,
        //        errors = String.Empty
        //    }, JsonRequestBehavior.AllowGet);
        //}
    }
}