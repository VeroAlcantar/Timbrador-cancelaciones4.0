using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;

namespace TimbradorCFDI2020
{
    public class IntLibrary
    {
        private SqlDataAdapter sqlDA;
        private SqlCommand sqlCmd;
        private string _sErrorNumber;
        private string _sErrorDescription;

        public string BaseDatos;
        public string Servidor;

        private string GetConnectionString(string _usuario, string _password) {
            string result = "";
            try {
                if (BaseDatos.Trim() == "" || Servidor.Trim() == "")
                {
                    //incluir MsgBox("No se ha configurado la conexion al servidor", MsgBoxStyle.Critical, "CFDi");
                }
                else {
                    string _servidor = Servidor;
                    string _DB = BaseDatos;

                    //'Return String.Format("Server={0};Database={1};User Id={2};Password={3};Connection Timeout=300", _servidor, _DB, _usuario, _password)
                    result = string.Format("Server={0};Database={1};User Id={2};Password={3}", _servidor, _DB, _usuario, _password);
                }
            }
            catch (Exception ex)
            {
                RegistraErrorSQL("ERROR DE CONEXIÓN Proceso (GetConnectionString), sever: " + Servidor + "BD: " + BaseDatos + "--" + ex.Message);
                result = "";
                //'MsgBox("ERROR DE CONEXIÓN Proceso (GetConnectionString), sever: " & Servidor & "BD: " & BaseDatos & "--" & ex.Message)
                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();
            }
            return result;
        }
        private string GetConnectionString() //'' para uso de autenticació con la informacion de la cuenta de windows del usuario conectado
        {
            string _servidor = Servidor;
            string _DB = BaseDatos;
            string result = "";
            string _ISecurity = "SSPI";

            if (BaseDatos.Trim() == "" || Servidor.Trim() == "")
            { 
                //incluir MsgBox("No se ha configurado la conexion al servidor", MsgBoxStyle.Critical, "CFDi");
            }
            else
            {
                result = string.Format("Server={0};Database={1};Integrated Security={2}", _servidor, _DB, _ISecurity);
            }
            return result;
        }

        private string GetConnectionStringAccess() //'' para uso de autenticació con la informacion de la cuenta de windows del usuario conectado
        {
            //string _servidor = "DESARROLLO1";
            string _DB = "INTEGRA";
            //string _ISecurity = "SSPI";

            return string.Format("Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0}", _DB);
        }

        public string exeQry(string strQry, string LBL)//ByVal LBL As Label
        {
            SqlConnection myCnn = new SqlConnection(GetConnectionString(ConfigurationManager.AppSettings["Usuario"].ToString(), ConfigurationManager.AppSettings["Pass"].ToString()));
            SqlCommand cmd = new SqlCommand(strQry, myCnn); ;
            string result = "";
            //'  Dim Update As String
            try {
                myCnn.Open();
                //' cmd.CommandTimeout = 600
                cmd.ExecuteNonQuery();
                //LBL.Text = "OPERACIÓN REALIZADA SATISFACTORIAMENTE" 'strQry.ToString 'Update.ToString
                result = "OPERACIÓN REALIZADA SATISFACTORIAMENTE"; //'strQry.ToString 'Update.ToString;
                myCnn.Close();
            }
            catch (Exception ex)
            {
                //LBL.Text = ex.Message
                result = ex.Message;
                //'MsgBox("ERROR DE SQL, Proceso(exeQry) sever: " & Servidor & "BD: " & BaseDatos & "--" & ex.Message & " :" & strQry)
                RegistraErrorSQL("ERROR DE SQL, Proceso(exeQry) sever: " + Servidor + "BD: " + BaseDatos + "--" + ex.Message + " :" + strQry);
                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();
            }
            finally
            {
                myCnn.Close();
            }
            return result;
        }

        // public string exeQry(string strQry, string LBL)//ByVal LBL As Label
        //{
        //    SqlConnection myCnn = new SqlConnection(GetConnectionString("parnet", "pubusradmin"));
        //    SqlCommand cmd = new SqlCommand(strQry, myCnn); ;
        //    string result = "";
        //    //'  Dim Update As String
        //    try {
        //        myCnn.Open();
        //        //' cmd.CommandTimeout = 600
        //        cmd.ExecuteNonQuery();
        //        //LBL.Text = "OPERACIÓN REALIZADA SATISFACTORIAMENTE" 'strQry.ToString 'Update.ToString
        //        result = "OPERACIÓN REALIZADA SATISFACTORIAMENTE"; //'strQry.ToString 'Update.ToString;
        //        myCnn.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        //LBL.Text = ex.Message
        //        result = ex.Message;
        //        //'MsgBox("ERROR DE SQL, Proceso(exeQry) sever: " & Servidor & "BD: " & BaseDatos & "--" & ex.Message & " :" & strQry)
        //        RegistraErrorSQL("ERROR DE SQL, Proceso(exeQry) sever: " + Servidor + "BD: " + BaseDatos + "--" + ex.Message + " :" + strQry);
        //    }
        //    finally
        //    {
        //        myCnn.Close();
        //    }
        //    return result;
        //}

        public string exeQry(string strQry)
        {
            string result = "";
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    bool request = false;
                    request = exeQryB(strQry);
                    if (request)
                    {
                        result = "OPERACIÓN REALIZADA SATISFACTORIAMENTE";// 'strQry.ToString 'Update.ToString
                        break;
                    }
                    else
                    {
                        result = "NO SE PUDO ESTABLECER LA CONEXION CON EL SERVIDOR SQL,  REVISAR LAS CONEXIONES";// 'strQry.ToString 'Update.ToString
                    }
                }

            }
            catch (Exception ex)
            {
                result = "NO SE PUDO ESTABLECER LA CONEXION CON EL SERVIDOR SQL,  REVISAR LAS CONEXIONES";
                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();
            }

            return result;
        }

        public bool exeQryB(string strQry)
        {
            //SqlConnection myCnn = new SqlConnection(GetConnectionString("parnet", "pubusradmin"));
            SqlConnection myCnn = new SqlConnection(GetConnectionString(ConfigurationManager.AppSettings["Usuario"].ToString(), ConfigurationManager.AppSettings["Pass"].ToString()));

            SqlCommand cmd = new SqlCommand(strQry, myCnn);
            bool result = false;
            try
            {
                myCnn.Open();
                // 'cmd.CommandTimeout = 600
                cmd.ExecuteNonQuery();
                result = true; //"OPERACIÓN REALIZADA SATISFACTORIAMENTE";// 'strQry.ToString 'Update.ToString

                result = false;
                //System.Threading.Thread.Sleep(2000);                

            }
            catch (Exception ex)
            {
                result = false;
                //System.Threading.Thread.Sleep(2000);
                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();
            }
            finally
            {
                myCnn.Close();
            }

            return result;
        }


        public DataTable getDataTable(string strQry)
        {
            SqlConnection myCnn = new SqlConnection(GetConnectionString());
            DataTable dt = new DataTable();
            SqlCommand qry = new SqlCommand(strQry, myCnn);
            try
            {
                myCnn.Open();

                SqlDataAdapter da = new SqlDataAdapter(qry);
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();
            }
            finally {
                myCnn.Close();
            }
            return dt;
        }

        public string GetP5sistema(string pParametro, string pModulo, string pEmpresa)
        {
            DataView dvResp = new DataView();
            System.DBNull res;
            string result = "";
            dvResp = GetDataView("Select isnull(Valor,'') as Valor from p5sistema where Empresa = '" + pEmpresa + "' and Modulo ='" + pModulo + "' and Parametro='" + pParametro + "'");
            //if (dvResp == res)            
            if (dvResp.Count > 0)
            {
                result = dvResp[0]["Valor"].ToString();
            }
            else
            {
                result = "";
            }
            return result;
        }

        public DataView GetDataView(string strQry)
        {
            DataView dvResult = new DataView();
            DataSet dsResult = new DataSet();
            string ErrorDescription = "";
            //DataView dv = new DataView();
            //System.DBNull res;
            SqlConnection myCnn = new SqlConnection(GetConnectionString(ConfigurationManager.AppSettings["Usuario"].ToString(), ConfigurationManager.AppSettings["Pass"].ToString()));
            try
            {
                bool error = false;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        dvResult = new DataView();
                        dsResult = new DataSet();
                        //incluir  frmInicia.EtiquetaSQL.Text = "|" & strQry.PadLeft(100);//localizar label
                        myCnn.Open();
                        sqlDA = new SqlDataAdapter(strQry, myCnn);
                        sqlDA.SelectCommand.CommandTimeout = 400;
                        sqlDA.Fill(dsResult);
                        if (dsResult.Tables.Count > 0)
                        {
                            dvResult = dsResult.Tables[0].DefaultView;
                        }
                        error = false;
                        if (!error)
                        {
                            myCnn.Close();
                            break;//sale del ciclo
                        }

                    }
                    catch (Exception ex)
                    {
                        ErrorDescription = ex.Message.ToString();
                        error = true;
                        //System.Threading.Thread.Sleep(2000);
                        //si hay error pregunta de nuevo 
                        string path = "c:\\xml33\\";
                        StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                        //w.Write("\r\nLog Entry : ");
                        w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                            DateTime.Now.ToLongDateString());
                        w.WriteLine("  {0}", ex.ToString());
                        w.WriteLine("-------------------------------");
                        w.Close();
                    }
                    finally
                    {
                        myCnn.Close();
                    }

                }
                if (error)
                {
                    //incluir MsgBox("NO SE PUDO ESTABLECER LA CONEXION CON EL SERVIDOR SQL,  REVISAR LAS CONEXIONES");
                    RegistraErrorSQL("ERROR DE SQL, Proceso(GetDataView) sever: " + Servidor + "BD: " + BaseDatos + "--" + ErrorDescription + " :" + strQry);
                }

            }
            catch (Exception ex)
            {
                //result = "NO SE PUDO ESTABLECER LA CONEXION CON EL SERVIDOR SQL,  REVISAR LAS CONEXIONES";      
                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();
            }
            finally
            {
                myCnn.Close();
            }
            return dvResult;
        }

        private void RegistraErrorSQL(string aDescripcion) 
        {
            try
            {
                exeQry("CFDRegErrorGeneral  '" + aDescripcion.Replace("'", "-") + "'");
            }
            catch (Exception ex){
                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();
            }           

        }
    }
}
