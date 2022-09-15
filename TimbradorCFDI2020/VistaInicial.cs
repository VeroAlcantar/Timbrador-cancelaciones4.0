//Modificado por Carlos Loera 10 de Enero 2021
//Se agregó la cancelación

using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Globalization;
//using System.Web;

namespace TimbradorCFDI2020
{
    public partial class VistaInicial : Form
    {
        private IntLibrary r = new IntLibrary();
        public long Segundos;
        public bool UsaLog;
        public string CFDIncluirRefBancaria;
        public string CreaRequestXML;
        public int TiempoEspera;
        public string STRSolicitaPDF;
        public string RutaXMLCFD;
        public string RutaPDFCFD;
        public string RutaCertificado;
        public string FormatoRutaCFD;
        public string PswCertificado;
        public string CFDiURLSoriana;
        public string CFDiURLpdf;
        public string CFDiRutaRequest;
        public string ParamArticuloesp25;
        public string ClaveImpRetencion;
        public string Empresa;
        public string VersionINE;
        public string listaDeCliente;
        public bool UsaReferencia;
        public string CFDiURL;

        static public bool Procesando = false;
        public string pFOLIO;
        public string Operacion;
        public string Documento;
        public string Plaza;
        public string FolioFActura;
        public string IDunico;

        public string idtoken;
        public string token;
        public string CFDI33certificado;
        public string CFDiURLCancela;
        public string RFCemisor;
        public string RFCreceptor;

        public string ErrorResponse;

        public int Ldia;
        public int Lanio;
        public int Lmes;

        public string RutaPDFGraba;
        public string RutaXMLGraba;
        public string Minutos;
        public VistaInicial()
        {
            InitializeComponent();
            //Double val = VBVal("-16.0000");
            //EtiquetaSQL.Text = val.ToString();
            //Double val = VBVal("-16.9999");
            //EtiquetaSQL.Text = val.ToString("N2");
            Minutos = ConfigurationManager.AppSettings["Minutos"].ToString().Trim();
            label11.Text = "Milisegundos Configurados para Reiniciar la petición: " + Minutos;
            Timer1.Interval = Convert.ToInt32(Minutos);
            Parametros();
            //ParametrosTest();
            //TimbraDiverza33test();
            //CreaDirectoriostest();
        }

        private bool ProcesaRespuestatest(string Respuesta, Int64 iddocto)
        {
            try
            {

                //Err.Clear()
                DataView dvProc = new DataView();
                DataView dvGraba = new DataView();
                DataView dvafecta = new DataView();

                DateTime lFecha = DateTime.Now;
                string lArchivo;
                string CadenaOriginal;
                string lCertificadoSat;
                string lnoCertificado;
                string luuid;

                if (Respuesta == "")
                    RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "El documento de respuesta esta vacio: Revisar en Buzon el estado de factura " + DateTime.Now.ToString());
                //Err.Clear()


                CadenaOriginal = "";

                lCertificadoSat = "";
                lnoCertificado = "";
                Registralog("ProcesaRespuesta", pFOLIO, Operacion, "ARCHIVO ENVIADO CON EXITO");

                string SelloSAt;

                lArchivo = pFOLIO;

                //' 1) Archivo Original de respuesta  (serializamos toda la clase -- Respuesta )
                System.Xml.Serialization.XmlSerializer Z = new System.Xml.Serialization.XmlSerializer(Respuesta.GetType());
                StreamWriter writer = new StreamWriter(CFDiRutaRequest + "\\R" + lArchivo + ".XML");
                Z.Serialize(writer, Respuesta);

                //If Err.Number <> 0 Then
                //    RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Generar (Serializar):" & RutaXMLGraba + "\R" + lArchivo + ".XML " & " :" & Err.Description)
                //End If

                //Err.Clear()

                string sPDF = "";
                Byte[] aFile;
                FileStream fs;
                FileStream fs2;
                //byte[] GraficoCB;
                aFile = Convert.FromBase64String(Respuesta);

                try
                {
                    //'2. El archivo que se le entrega al Cliente viene en el attributo "archivo" y se graba en binario, se trae a ruta local para extraer datos
                    if (aFile != null && aFile.Length > 0)
                    {
                        sPDF = CFDiRutaRequest + "\\" + lArchivo + ".XML";
                        fs = new FileStream(sPDF, FileMode.Create);
                        fs.Write(aFile, 0, aFile.Length);
                        fs.Close();
                    }
                    else
                    {
                        RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Generar versión 3.3:  ");
                    }
                }
                catch (Exception ex)
                {
                    RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Genera:" + sPDF + " :" + ex.ToString());

                    string path = "c:\\xml33\\";
                    StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                    //w.Write("\r\nLog Entry : ");
                    w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        DateTime.Now.ToLongDateString());
                    w.WriteLine("  {0}", ex.ToString());
                    w.WriteLine("-------------------------------");
                    w.Close();
                }
                //'EXTRACCION DEL SELLO SAT
                SelloSAt = "";
                luuid = "";
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                try
                {
                    using (XmlReader reader = XmlReader.Create(sPDF, settings))
                    {

                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element && "Comprobante" == reader.LocalName)
                            {
                                lCertificadoSat = reader.GetAttribute("Certificado").ToString();
                            }
                        }

                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element && "TimbreFiscalDigital" == reader.LocalName)
                            {
                                luuid = reader.GetAttribute("UUID").ToString();
                                SelloSAt = reader.GetAttribute("SelloSAT").ToString();
                                lFecha = Convert.ToDateTime(reader.GetAttribute("FechaTimbrado").ToString());
                            }
                        }
                    }
                    Ldia = lFecha.Day;
                    Lmes = lFecha.Month;
                    Lanio = lFecha.Year;
                    CreaDirectorios();

                    if (aFile != null && aFile.Length > 0)
                    {
                        sPDF = RutaXMLGraba + "\\" + lArchivo + ".XML";
                        Registralog("GeneraXML", pFOLIO, Operacion, "SE GENERARA XML:" + sPDF);
                        fs2 = new FileStream(sPDF, FileMode.Create);
                        fs2.Write(aFile, 0, aFile.Length);
                        fs2.Close();
                    }
                    else
                        RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Genera: Nodo (Respuesta.InfoCFDi.archivo) esta vacio ");
                }
                catch (Exception ex)
                {
                    RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Genera:" + sPDF + " :" + ex.ToString());

                    string path = "c:\\xml33\\";
                    StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                    //w.Write("\r\nLog Entry : ");
                    w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        DateTime.Now.ToLongDateString());
                    w.WriteLine("  {0}", ex.ToString());
                    w.WriteLine("-------------------------------");
                    w.Close();
                }
                //'SE ACTULIZAN LOS REGISTROS DEL DOCUMENTO
                dvGraba = r.GetDataView("exec  CDFI33ActualizaCFDFactura '" + IDunico + "','" + Lmes + "','" + Ldia + "','" + Lanio + "','" + CadenaOriginal + "','" + lCertificadoSat + "','" + lFecha.ToString("yyyyMMdd hh:mm:ss") + "','" + lnoCertificado + "','" + Lanio + "','comprobante ','" + luuid + "', '" + lCertificadoSat + "'");
                dvafecta = r.GetDataView("P5CFDAFECTACXCCLIENTE " + IDunico + " ,'" + lFecha.ToString("yyyyMMdd") + "' ");
                //'dvGraba = r.GetDataView("exec  CDFActaulizaCFDFactura '" & iddocto.ToString & "','" & Lmes & "','" & Ldia & "','" & Lanio & "','" & CadenaOriginal & "','" & Respuesta.InfoCFDi.noCertificadoSAT & "','" & Respuesta.InfoCFDi.fechaTimbrado.ToString("yyyymmdd") & "','" & Respuesta.InfoCFDi.noCertificado & "','2011','comprobantebuscarlo'")
                //' dvafecta = r.GetDataView("P5CFDAFECTACXCCLIENTECARGO " & iddocto.ToString & " '" & Format(Respuesta.InfoCFDi.fechaTimbrado, "yyyymmdd") & "' ")

                //'lArchivo = Respuesta.InfoCFDi.serie.ToString.Trim & Respuesta.InfoCFDi.folio.ToString.Trim
                Registralog("ProcesaRespuesta", pFOLIO, Operacion, "ACTUALIZACION DE REGISTROS");
                r.exeQry(" update CFDFactura set SelloDigital = '" + SelloSAt + "' where    id = " + IDunico);

                //Err.Clear()

                dvProc = r.GetDataView(" update CFDFactura set ArchivoXML = '" + RutaXMLGraba + "\"" + lArchivo + ".XML" + "'  where  id = " + IDunico);
                //'YA NO SE SOLICITA EL PDF EN LINEA, SOLO SE SOLICITA EN LOTES
                r.exeQry(" update CFDFactura set ArchivoPDF = '', IntentoPDF = isnull(IntentoPDF,0)+1 , SolicitaPDF = -1  where    id = " + IDunico);
                Etiqueta.Text = "Genera XML: " + pFOLIO;
                this.Refresh();
                Registralog("ProcesaRespuesta", pFOLIO, Operacion, "SE CREO PDF");

            }
            catch (Exception ex)
            {
                RegistralogX("ProcesaRespuesta ", pFOLIO, Operacion, "ERROR AL procesar respuesta" + ex.ToString());

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

            }
            return true;
        }


        public void CreaDirectoriostest()
        {
            string Directorio;
            DateTime lFecha = DateTime.Now;
            Operacion = "FACTURA";
            Ldia = lFecha.Day;
            Lmes = lFecha.Month;
            Lanio = lFecha.Year;
            Empresa = "MMY";
            Documento = "FACTLIB";
            Plaza = "1";

            Directorio = "plaza\\empresa\\anio\\mes\\dia\\operacion";

            Directorio = Directorio.Replace("operacion", Operacion.ToUpper().Trim());
            Directorio = Directorio.Replace("plaza", Plaza.ToUpper().Trim());
            Directorio = Directorio.Replace("documento", Documento.Trim());
            Directorio = Directorio.Replace("anio", Lanio.ToString().Trim());
            Directorio = Directorio.Replace("mes", Lmes.ToString().Trim());
            Directorio = Directorio.Replace("dia", Ldia.ToString().Trim());
            Directorio = Directorio.Replace("empresa", Empresa.Trim());

            //On Error Resume Next
            //'RutaPDFCFD = "C:\PDF\"
            //'RutaXMLCFD = RutaPDFCFD
            try
            {
               // RutaPDFGraba = "\"Tau2k3FacElec\"FacElec\"" + Directorio;
                //RutaXMLGraba = RutaXMLCFD + Directorio;
                RutaXMLGraba = "c:\\FacElec\\" + Directorio;


                if (!ExisteArchivo(RutaXMLGraba))
                {
                    //Shell("cmd.exe /c md " + RutaXMLGraba);
                    //Process.Start("cmd.exe /c md " + RutaXMLGraba);
                    DirectoryInfo di = Directory.CreateDirectory(RutaXMLGraba);
                    //System.Threading.Thread.Sleep(2000);
                }
                //'MkDir (RutaPDFGraba)

                //if Err.Number <> 75 And Err.Number <> 0 Then     'error 75 = ya existe el directorio--- cualquier otro error sera reportado
                //MsgBox("Error a crear el directorio: " & Err.Description & ":" & RutaPDFGraba)
                //End If

                //RutaPDFGraba = RutaPDFCFD + Directorio;
                if (!ExisteArchivo(RutaPDFGraba))
                {
                    DirectoryInfo di = Directory.CreateDirectory(RutaPDFGraba);
                    //Shell("cmd.exe /c md " + RutaPDFGraba);
                    //Process.Start("cmd.exe /c md " + RutaPDFGraba);
                    //System.Threading.Thread.Sleep(1000);
                }

                //'  MkDir (RutaXMLGraba)
                //If Err.Number <> 75 And Err.Number <> 0 Then     'error 75 = ya existe el directorio--- cualquier otro error sera reportado
                //RegistralogX("CreaDirectorio", pFOLIO, Operacion, "Error a crear el directorio: " + Err.Description + ":" + RutaXMLGraba);
                //End If

                //Err.Clear()
            }
            catch (Exception ex)
            {
                //MsgBox("Error a crear el directorio: " + Err.Description + ":" + RutaPDFGraba);
                //RegistralogX("CreaDirectorios", pFOLIO, Operacion, "Error a crear el directorio");
                //RegistraError(IDunico, "Error a crear el directorio: " + ex.ToString(), "CreaDirectorios");

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

        public bool TimbraDiverza33test()
        {
            CFDiRutaRequest = "c:" + "\\" + "XML33" + "\\";
            pFOLIO = "TEST000006";

            //'Adaptacion código web service para cfdi 3.3
            //MemoryStream MemStream = FileToMemory(CFDiRutaRequest + pFOLIO + ".XML");
            //Byte[] myByteArray = MemStream.ToArray();
            byte[] myByteArray = FileToMemory(CFDiRutaRequest + pFOLIO + ".XML");
            string Content64;
            Content64 = System.Convert.ToBase64String(myByteArray, 0, myByteArray.Length);

            //CFDiURL = r.GetP5sistema("CFDiURL", "gen", Empresa);
            ////object request = TryCast(System.Net.WebRequest.Create(CFDiURL), System.Net.HttpWebRequest)
            //WebRequest request = HttpWebRequest.Create(CFDiURL);
            //request.Method = "POST";
            //request.Timeout = 3600000;

            string json;
            //string responseFromServer = "";
            json = "{\"credentials\": {\"id\": \"" + idtoken + "\",\"token\": \"" + token + "\"},\"issuer\": {\"rfc\": \"" + RFCemisor + "\"},\"document\": {\"ref-id\": \"" + IDunico + "\",\"certificate-number\": \"" + CFDI33certificado + "\",\"section\": \"all\",\"format\": \"xml\",\"template\": \"letter\",\"type\": \"application/vnd.diverza.cfdi_3.3+xml\",\"content\": \"" + Content64 + "\"}}";

            //if (complemento)
            //    json = "{\"credentials\": {\"id\": \"" + idtoken + "\",\"token\": \"" + token + "\"},\"issuer\": {\"rfc\": \"" + RFCemisor + "\"},\"document\": {\"ref-id\": \"" + IDunico + "\",\"certificate-number\": \"" + CFDI33certificado + "\",\"section\": \"all\",\"format\": \"xml\", \"template\": \"letter\",\"type\": \"application/vnd.diverza.cfdi_3.3_complemento+xml\",\"content\": \"" + Content64 + "\"}}";

            //'****** archivo con el json
            string path1 = CFDiRutaRequest + pFOLIO + ".json";
            FileStream fs1 = File.Create(path1);
            Byte[] info1 = new UTF8Encoding(true).GetBytes(json);
            fs1.Write(info1, 0, info1.Length);
            fs1.Close();
            //'**** fin archivo con json

            //Byte[] byteArray = Encoding.UTF8.GetBytes(json);

            //request.ContentType = "application/json; charset=utf-8";
            ////'request.ContentType = "application/json";

            //Stream dataStream = request.GetRequestStream();
            //dataStream.Write(byteArray, 0, byteArray.Length);
            //dataStream.Close();
            ParametrosTest();
            return true;
        }

        private void ParametrosTest()
        {
            CFDiRutaRequest = "c:" + "\\" + "XML33" + "\\";

            //string ErrorResponse = "{\"uuid\": \"011b5022-d468-433d-908c-0decc4c94ef8\",\"ref_id\": \"1543833\",\"content\": \"PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiID8+CjxjZmRpOkNvbXByb2JhbnRlIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTpzY2hlbWFMb2NhdGlvbj0iaHR0cDovL3d3dy5zYXQuZ29iLm14L2NmZC8zIGh0dHA6Ly93d3cuc2F0LmdvYi5teC9zaXRpb19pbnRlcm5ldC9jZmQvMy9jZmR2MzMueHNkIiBWZXJzaW9uPSIzLjMiIFNlcmllPSJURVNUIiBGb2xpbz0iNSIgRmVjaGE9IjIwMjEtMDQtMjFUMTI6MzA6NDUiIFNlbGxvPSJocTNIS2JjOE5IdU9TcllUL2Z0aHBYdTZNS29PcUE1bUcwSm5qS3o0WUpnMGNJakhsVnJ2MHRIRDVNUVNtVXRsSzhOblRDUEJnM3Z6RnJDaWU3bUhMNXI2a0IxUC9tbVB3akRtczBzZUJBbkdpU1diTkdFS21BR2FPU04vUkh2RGFtMjRXUDdGNjh4VEJwSDk0RDRYbXJhU0hJRmtrL0Zub3A2bTBGSjUyOUhXZUxPcW1hRDQ2NzdOOEdLYnBUK012cGs3Y3ZmeWhhaVdUcG9kNjBZa0p0YUtkQUlhNnZmZkhrQnk3NWI1QTdMREUwZFlYK2NGbzRveXI1UXpqSzRwSXNjakdDL0xxVnlBRmhFcUZsOUZWbnJjZnduNVV5bytNeW5sVkF2N2tqYTUwNWQzSkhPRDRRcTV4TjFUem1lZldtOXNvcmJRYWVlRHJEb2RQVWhQcEE9PSIgRm9ybWFQYWdvPSIwMSIgTm9DZXJ0aWZpY2Fkbz0iMzAwMDEwMDAwMDAzMDAwMjM3MDgiIENlcnRpZmljYWRvPSJNSUlGK1RDQ0ErR2dBd0lCQWdJVU16QXdNREV3TURBd01EQXpNREF3TWpNM01EZ3dEUVlKS29aSWh2Y05BUUVMQlFBd2dnRm1NU0F3SGdZRFZRUUREQmRCTGtNdUlESWdaR1VnY0hKMVpXSmhjeWcwTURrMktURXZNQzBHQTFVRUNnd21VMlZ5ZG1samFXOGdaR1VnUVdSdGFXNXBjM1J5WVdOcHc3TnVJRlJ5YVdKMWRHRnlhV0V4T0RBMkJnTlZCQXNNTDBGa2JXbHVhWE4wY21GamFjT3piaUJrWlNCVFpXZDFjbWxrWVdRZ1pHVWdiR0VnU1c1bWIzSnRZV05wdzdOdU1Ta3dKd1lKS29aSWh2Y05BUWtCRmhwaGMybHpibVYwUUhCeWRXVmlZWE11YzJGMExtZHZZaTV0ZURFbU1DUUdBMVVFQ1F3ZFFYWXVJRWhwWkdGc1oyOGdOemNzSUVOdmJDNGdSM1ZsY25KbGNtOHhEakFNQmdOVkJCRU1CVEEyTXpBd01Rc3dDUVlEVlFRR0V3Sk5XREVaTUJjR0ExVUVDQXdRUkdsemRISnBkRzhnUm1Wa1pYSmhiREVTTUJBR0ExVUVCd3dKUTI5NWIyRmp3NkZ1TVJVd0V3WURWUVF0RXd4VFFWUTVOekEzTURGT1RqTXhJVEFmQmdrcWhraUc5dzBCQ1FJTUVsSmxjM0J2Ym5OaFlteGxPaUJCUTBSTlFUQWVGdzB4TnpBMU1UZ3dNelUwTlRaYUZ3MHlNVEExTVRnd016VTBOVFphTUlIbE1Ta3dKd1lEVlFRREV5QkJRME5GVFNCVFJWSldTVU5KVDFNZ1JVMVFVa1ZUUVZKSlFVeEZVeUJUUXpFcE1DY0dBMVVFS1JNZ1FVTkRSVTBnVTBWU1ZrbERTVTlUSUVWTlVGSkZVMEZTU1VGTVJWTWdVME14S1RBbkJnTlZCQW9USUVGRFEwVk5JRk5GVWxaSlEwbFBVeUJGVFZCU1JWTkJVa2xCVEVWVElGTkRNU1V3SXdZRFZRUXRFeHhCUVVFd01UQXhNREZCUVVFZ0x5QklSVWRVTnpZeE1EQXpORk15TVI0d0hBWURWUVFGRXhVZ0x5QklSVWRVTnpZeE1EQXpUVVJHVWs1T01Ea3hHekFaQmdOVkJBc1VFa05UUkRBeFgwRkJRVEF4TURFd01VRkJRVENDQVNJd0RRWUpLb1pJaHZjTkFRRUJCUUFEZ2dFUEFEQ0NBUW9DZ2dFQkFKZFVjc0hJRUlnd2l2dkFhbnRHbllWSU8zKzd5VGREMXRrS29wYkwrdEtTalJGbzFFclBkR0p4UDNneFQ1TytBQ0lEUVhOK0hTOXVNV0RZbmFVUmFsU0lGOUNPRkNkaC9PSDJQbitVbWtONGN1bHIyRGFuS3p0VklPOGlkWE02YzlhSG41aE9vN2hEeFhNQzN1T3VHVjNGUzRPYmt4VFYrOU5zdk9BVjJsTWUyN1NIclNCMERodUx1clViWndYbSsvcjRkdHozYjJ1TGdCYytEaXk5NVBHK01JdTdvTktNODlhQk5HY2pUSncrOWsrV3pKaVBkM1pwUWdJZWRZQkQrOFFXeGxZQ2d4aG50YTNrOXlsZ1hLWVhDWWswazBxYXV2QkoxalNSVmY1QmpqSVViT3N0YVFwNTlua2dIaDQ1YzlnbndKUlY2MThOVzBmTWVEenVLUjBDQXdFQUFhTWRNQnN3REFZRFZSMFRBUUgvQkFJd0FEQUxCZ05WSFE4RUJBTUNCc0F3RFFZSktvWklodmNOQVFFTEJRQURnZ0lCQUJLajBEQ05MMWxoNDR5K09jV0ZyVDJpY25LRjdXeVNPVmloeDBvUitIUHJXS0JNWHhvOUt0cm9kbkIxdGdJeDhmK1hqcXlwaGhidytqdURTZURyYjk5UGhDNCtFNkplWE9rZFFjSnQ1MEt5b2RsOVVScENWV05XalViM0YveXBhOG9UY2ZmL2VNZnRRWlQ3TVExTHFodCt4bTNRaFZveFRJQVNjZTBqanNuQlRHRDJKUTR1VDNvQ2VtOGJtb01YVi9mazlhSjN2MCtaSUw0Mk1wWTRQT0dVYS9pVGFhd2tsS1JBTDFYajlJZElSMDZSSzY4UlM2eHJHazZqd2JEVEVLeEpwbVozU1BMdGxzbVBVVE8xa3JhVFBJbzlGQ21VL3paa1dHcGQ4WkVBQUZ3K1pmSStiZFhCZnZkRHdhTTJpTUdUUVpUVEVnVTVLS1RJdmtBbkhvOU80NVNxU0p3cVY5TkxmUEF4Q281ZVJSMk9HaWJkOWpoSGU4MXpVc3A1R2RFMW1aaVNxSlU4MkgzY3U2QmlFK0QzWWJaZVpuanJOU3hCZ0tUSWY4dytLTllQTTRhV251VU1sMG1MZ3RPeFRVWGk5TUtuVWNjcTNHWkxBN2J4N1puMjExeVBScUVqU0FxeWJVTVZJT2hvNmFxemtmYzNXTFo2TG5HVStoeUh1WlVmUHdibkNsYjdvRkZ6MVBsdkdPcE5Ec1ViMHFQNDJRQ0dCaVRVc2VHdWdBenFPUDZFWXBWUEM3M2dGb3VybWRCUWdmYXlhRXZpM3hqTmFuRmtQbFcxWEVZTnJZSkI0eU5qcGhGcnZXd1RZODZ2TDJvOGdaTjBVdG1jNWZub0JUZk05cjJ6VkttRWk2RlVlSjFpYURhVk52NDd0ZTlpUzFhaTRWNHZCWThyIiBDb25kaWNpb25lc0RlUGFnbz0iQ09OVEFETyIgU3ViVG90YWw9IjIwMC4wMCIgTW9uZWRhPSJNWE4iIFRvdGFsPSIyMzIuMDAiIFRpcG9EZUNvbXByb2JhbnRlPSJJIiBNZXRvZG9QYWdvPSJQVUUiIEx1Z2FyRXhwZWRpY2lvbj0iNjQ3MDAiIHhtbG5zOmNmZGk9Imh0dHA6Ly93d3cuc2F0LmdvYi5teC9jZmQvMyI+CiAgPGNmZGk6RW1pc29yIFJmYz0iTURJOTkxMjE0QTc0IiBOb21icmU9Ik1JTEVOSU8gRElBUklPLCBTLkEuIERFIEMuVi4iIFJlZ2ltZW5GaXNjYWw9IjYwMSIvPgogIDxjZmRpOlJlY2VwdG9yIFJmYz0iWEFYWDAxMDEwMTAwMCIgTm9tYnJlPSJWRU5UQSBBTCBQVUJMSUNPIEdFTkVSQUwiIFVzb0NGREk9IkcwMyIvPgogIDxjZmRpOkNvbmNlcHRvcz4KICAgIDxjZmRpOkNvbmNlcHRvIENsYXZlUHJvZFNlcnY9IjgyMTAxNTA0IiBDYW50aWRhZD0iMS4wMCIgQ2xhdmVVbmlkYWQ9IkU0OCIgVW5pZGFkPSJOTyBBUExJQ0EiIERlc2NyaXBjaW9uPSJQT1IgQ09OQ0VQVE8gREUgUFVCTElDSURBRCBNLiBESUFSSU8gRkVEIiBWYWxvclVuaXRhcmlvPSIyMDAuMDAwMDAiIEltcG9ydGU9IjIwMC4wMDAwMCI+CiAgICAgIDxjZmRpOkltcHVlc3Rvcz4KICAgICAgICA8Y2ZkaTpUcmFzbGFkb3M+CiAgICAgICAgICA8Y2ZkaTpUcmFzbGFkbyBCYXNlPSIyMDAuMDAiIEltcHVlc3RvPSIwMDIiIFRpcG9GYWN0b3I9IlRhc2EiIFRhc2FPQ3VvdGE9IjAuMTYwMDAwIiBJbXBvcnRlPSIzMi4wMCIvPgogICAgICAgIDwvY2ZkaTpUcmFzbGFkb3M+CiAgICAgIDwvY2ZkaTpJbXB1ZXN0b3M+CiAgICA8L2NmZGk6Q29uY2VwdG8+CiAgPC9jZmRpOkNvbmNlcHRvcz4KICA8Y2ZkaTpJbXB1ZXN0b3MgVG90YWxJbXB1ZXN0b3NUcmFzbGFkYWRvcz0iMzIuMDAiPgogICAgPGNmZGk6VHJhc2xhZG9zPgogICAgICA8Y2ZkaTpUcmFzbGFkbyBJbXB1ZXN0bz0iMDAyIiBUaXBvRmFjdG9yPSJUYXNhIiBUYXNhT0N1b3RhPSIwLjE2MDAwMCIgSW1wb3J0ZT0iMzIuMDAiLz4KICAgIDwvY2ZkaTpUcmFzbGFkb3M+CiAgPC9jZmRpOkltcHVlc3Rvcz4KICA8Y2ZkaTpDb21wbGVtZW50bz4KICAgICAgICA8dGZkOlRpbWJyZUZpc2NhbERpZ2l0YWwgeG1sbnM6dGZkPSJodHRwOi8vd3d3LnNhdC5nb2IubXgvVGltYnJlRmlzY2FsRGlnaXRhbCIgeHNpOnNjaGVtYUxvY2F0aW9uPSJodHRwOi8vd3d3LnNhdC5nb2IubXgvVGltYnJlRmlzY2FsRGlnaXRhbCBodHRwOi8vd3d3LnNhdC5nb2IubXgvc2l0aW9faW50ZXJuZXQvY2ZkL1RpbWJyZUZpc2NhbERpZ2l0YWwvVGltYnJlRmlzY2FsRGlnaXRhbHYxMS54c2QiIFZlcnNpb249IjEuMSIgVVVJRD0iMDExYjUwMjItZDQ2OC00MzNkLTkwOGMtMGRlY2M0Yzk0ZWY4IiBSZmNQcm92Q2VydGlmPSJTUFIxOTA2MTNJNTIiIEZlY2hhVGltYnJhZG89IjIwMjEtMDQtMjJUMTY6MTU6NTMiIFNlbGxvQ0ZEPSJocTNIS2JjOE5IdU9TcllUL2Z0aHBYdTZNS29PcUE1bUcwSm5qS3o0WUpnMGNJakhsVnJ2MHRIRDVNUVNtVXRsSzhOblRDUEJnM3Z6RnJDaWU3bUhMNXI2a0IxUC9tbVB3akRtczBzZUJBbkdpU1diTkdFS21BR2FPU04vUkh2RGFtMjRXUDdGNjh4VEJwSDk0RDRYbXJhU0hJRmtrL0Zub3A2bTBGSjUyOUhXZUxPcW1hRDQ2NzdOOEdLYnBUK012cGs3Y3ZmeWhhaVdUcG9kNjBZa0p0YUtkQUlhNnZmZkhrQnk3NWI1QTdMREUwZFlYK2NGbzRveXI1UXpqSzRwSXNjakdDL0xxVnlBRmhFcUZsOUZWbnJjZnduNVV5bytNeW5sVkF2N2tqYTUwNWQzSkhPRDRRcTV4TjFUem1lZldtOXNvcmJRYWVlRHJEb2RQVWhQcEE9PSIgTm9DZXJ0aWZpY2Fkb1NBVD0iMzAwMDEwMDAwMDA0MDAwMDI0OTUiIFNlbGxvU0FUPSJjK3pMK3V0N3BpMGFOdVdvVlhva1lxcmg5T0g0em5LWnJuYVZEaTgxQ204aXVWOHU2UHVCbjJMZHBvL3lDVVR5NCtjSUdKVzk2ZGxieG9BazZvaXAvUm5IVUpiMzdXQUhPRG5ObzBqQVVlbkVONG5SWXhwaUdLMCtyNDdqa0JwalluUDVoRlJHTGRzeG1mRGJLMXhIREJ3Uyt6WURCdHpibHEwdlgrTDlST0FUR1pYd3VZMnhZeFBKZUJrcG5PdlVRVGlMUWRGYTVoMUNzTUw0d2NpdHlVcnovUTJ4ZWd2UktUUDJyajl5bTRYZ01LRUQ0cU1KNHJSZUdIU2RvQjNydFpMVkJNU3pEVy91c2Y0NEhoVjV5a21LcmU4U204R1hwQWxuc3VXWjFuNE45R0YrdDBQY0ZzdXpmdHltcm95WExvcXNNcFhFOXBSSmpyWXp3bkhySUE9PSIvPgogICAgPC9jZmRpOkNvbXBsZW1lbnRvPjxjZmRpOkFkZGVuZGE+CiAgICA8ZGl2ZXJ6YSB4c2k6c2NoZW1hTG9jYXRpb249Imh0dHA6Ly93d3cuZGl2ZXJ6YS5jb20vbnMvYWRkZW5kYS9kaXZlcnphLzEgZmlsZTovVXNlcnMvb3N2YWxkb3NhbmNoZXovRG9jdW1lbnRzL0RJVkVSWkEvQWRkZW5kYV9EaXZlcnphX3YxLjEueHNkIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiB4bWxuczp0ZENGREk9Imh0dHA6Ly93d3cuc2F0LmdvYi5teC9zaXRpb19pbnRlcm5ldC9jZmQvdGlwb0RhdG9zL3RkQ0ZESSIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy5kaXZlcnphLmNvbS9ucy9hZGRlbmRhL2RpdmVyemEvMSI+CiAgICAgIDxnZW5lcmFsZXMgdGlwb0RvY3VtZW50bz0iRmFjdHVyYSIgdG90YWxDb25MZXRyYT0iKCBET1NDSUVOVE9TIFRSRUlOVEEgWSBET1MgIFBFU09TICAwMC8xMDAgTS5OLiApIiBudW1lcm9PcmRlbj0iIi8+CiAgICAgIDxjbGF2ZXNEZXNjcmlwY2lvbiBjX0Zvcm1hUGFnbz0iRWZlY3Rpdm8iIGNfTW9uZWRhPSJNWE4iIGNfVGlwb0RlQ29tcHJvYmFudGU9IkkiIGNfTWV0b2RvUGFnbz0iUGFnbyBlbiB1bmEgc29sYSBleGhpYmljacOzbiIgY19MdWdhckV4cGVkaWNpb249IjY0NzAwIiBjX1JlZ2ltZW5GaXNjYWw9IkdlbmVyYWwgZGUgTGV5IFBlcnNvbmFzIE1vcmFsZXMiIGNfVXNvQ0ZEST0iR2FzdG9zIGVuIGdlbmVyYWwiLz4KICAgICAgPGVtaXNvciB2ZW5kZWRvcj0iMSI+CiAgICAgICAgPGRhdG9zQ29udGFjdG9FIHRlbGVmb25vPSJURUwuICg4MSkgODE1MC01NTAwIiBlbWFpbENvbWVyY2lhbD0iY2xpZW50ZS5tdHlAbWlsZW5pby5jb20iIGVtYWlsQ29udGFjdG89IkZBWC4gKDgxKSA4MTUwLTU1NjciIHdlYj0id3d3Lm1pbGVuaW8uY29tIi8+CiAgICAgICAgPGRvbWljaWxpb0Zpc2NhbEUgY2FsbGU9Ik1PUkVMT1MiIG51bWVybz0iMTYiIGNvbG9uaWE9IkNFTlRSTyIgY2l1ZGFkPSJERUwuIENVQVVIVEVNT0MiIG11bmljaXBpbz0iREVMLiBDVUFVSFRFTU9DIiBlc3RhZG89IkNJVURBRCBERSBNRVhJQ08iIHBhaXM9Ik1FWElDTyIgY29kaWdvUG9zdGFsPSIwNjA0MCIvPgogICAgICA8L2VtaXNvcj4KICAgICAgPHJlY2VwdG9yIG51bUNsaWVudGU9IjAwMDAwMTU1ICAgICAgICAgICAgIj4KICAgICAgICA8ZGF0b3NDb250YWN0b1IgdGVsZWZvbm89IjIxMjEzMTMiIGVtYWlsQ29tZXJjaWFsPSJjbGllbnRlLm10eUBtaWxlbmlvLmNvbSIgd2ViPSJ3d3cubWlsZW5pby5jb20iLz4KICAgICAgICA8ZG9taWNpbGlvRmlzY2FsUiBjYWxsZT0iTU9SRUxPUyIgbnVtZXJvPSIxNiIgY29sb25pYT0iQ0VOVFJPIiBjaXVkYWQ9IkNVQVVIVEVNT0MiIG11bmljaXBpbz0iQ1VBVUhURU1PQyIgZXN0YWRvPSJDSVVEQUQgREUgTUVYSUNPIiBwYWlzPSJNRVhJQ08iIGNvZGlnb1Bvc3RhbD0iMDYwNDAiLz4KICAgICAgPC9yZWNlcHRvcj4KICAgICAgPGNvbmNlcHRvcyBudW1lcm9Db25jZXB0b3M9IjEiPgogICAgICAgIDxjb25jZXB0byBpZGVudGlmaWNhZG9yMT0iSUQxIiBtZW5zYWplPSJDT01FUkNJQUwiLz4KICAgICAgPC9jb25jZXB0b3M+CiAgICA8L2RpdmVyemE+CiAgPC9jZmRpOkFkZGVuZGE+CjwvY2ZkaTpDb21wcm9iYW50ZT4=\"}";
            //string ErrorResponse = "{\"uuid\": \"011b5022-d468-433d-908c-0decc4c94ef8\",\"ref_id\": \"1543833\",\"content\": \"PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxjZmRpOkNvbXByb2JhbnRlIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTpzY2hlbWFMb2NhdGlvbj0iaHR0cDovL3d3dy5zYXQuZ29iLm14L2NmZC8zIGh0dHA6Ly93d3cuc2F0LmdvYi5teC9zaXRpb19pbnRlcm5ldC9jZmQvMy9jZmR2MzMueHNkIiBWZXJzaW9uPSIzLjMiIFNlcmllPSJURVNUIiBGb2xpbz0iNyIgRmVjaGE9IjIwMjEtMDQtMjNUMTY6MDk6NTkiIFNlbGxvPSIiIEZvcm1hUGFnbz0iMDEiIE5vQ2VydGlmaWNhZG89IjMwMDAxMDAwMDAwMzAwMDIzNzA4IiBDZXJ0aWZpY2Fkbz0iIiBDb25kaWNpb25lc0RlUGFnbz0iQ09OVEFETyIgU3ViVG90YWw9IjIwMC4wMDAwIiBNb25lZGE9Ik1YTiIgVG90YWw9IjIzMi4wMDAwIiBUaXBvRGVDb21wcm9iYW50ZT0iSSIgTWV0b2RvUGFnbz0iUFVFIiBMdWdhckV4cGVkaWNpb249IjY0NzAwIiB4bWxuczpjZmRpPSJodHRwOi8vd3d3LnNhdC5nb2IubXgvY2ZkLzMiPg0KICA8Y2ZkaTpFbWlzb3IgUmZjPSJNREk5OTEyMTRBNzQiIE5vbWJyZT0iTUlMRU5JTyBESUFSSU8sIFMuQS4gREUgQy5WLiIgUmVnaW1lbkZpc2NhbD0iNjAxIiAvPg0KICA8Y2ZkaTpSZWNlcHRvciBSZmM9IlhBWFgwMTAxMDEwMDAiIE5vbWJyZT0iVkVOVEEgQUwgUFVCTElDTyBHRU5FUkFMIiBVc29DRkRJPSJHMDMiIC8+DQogIDxjZmRpOkNvbmNlcHRvcz4NCiAgICA8Y2ZkaTpDb25jZXB0byBDbGF2ZVByb2RTZXJ2PSI4MjEwMTUwNCIgQ2FudGlkYWQ9IjEuMDAwMDAiIENsYXZlVW5pZGFkPSJFNDgiIFVuaWRhZD0iTk8gQVBMSUNBIiBEZXNjcmlwY2lvbj0iUE9SIENPTkNFUFRPIERFIFBVQkxJQ0lEQUQgTS4gRElBUklPIEZFRCIgVmFsb3JVbml0YXJpbz0iMjAwLjAwMDAwIiBJbXBvcnRlPSIyMDAuMDAwMDAiPg0KICAgICAgPGNmZGk6SW1wdWVzdG9zPg0KICAgICAgICA8Y2ZkaTpUcmFzbGFkb3M+DQogICAgICAgICAgPGNmZGk6VHJhc2xhZG8gQmFzZT0iMjAwLjAwIiBJbXB1ZXN0bz0iMDAyIiBUaXBvRmFjdG9yPSJUYXNhIiBUYXNhT0N1b3RhPSIwLjE2MDAwMCIgSW1wb3J0ZT0iMzIuMDAiIC8+DQogICAgICAgIDwvY2ZkaTpUcmFzbGFkb3M+DQogICAgICA8L2NmZGk6SW1wdWVzdG9zPg0KICAgIDwvY2ZkaTpDb25jZXB0bz4NCiAgPC9jZmRpOkNvbmNlcHRvcz4NCiAgPGNmZGk6SW1wdWVzdG9zIFRvdGFsSW1wdWVzdG9zVHJhc2xhZGFkb3M9IjMyLjAwIj4NCiAgICA8Y2ZkaTpUcmFzbGFkb3M+DQogICAgICA8Y2ZkaTpUcmFzbGFkbyBJbXB1ZXN0bz0iMDAyIiBUaXBvRmFjdG9yPSJUYXNhIiBUYXNhT0N1b3RhPSIwIiBJbXBvcnRlPSIzMi4wMCIgLz4NCiAgICA8L2NmZGk6VHJhc2xhZG9zPg0KICA8L2NmZGk6SW1wdWVzdG9zPg0KICA8Y2ZkaTpBZGRlbmRhPg0KICAgIDxkaXZlcnphIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhtbG5zOnRkQ0ZEST0iaHR0cDovL3d3dy5zYXQuZ29iLm14L3NpdGlvX2ludGVybmV0L2NmZC90aXBvRGF0b3MvdGRDRkRJIiB4c2k6c2NoZW1hTG9jYXRpb249Imh0dHA6Ly93d3cuZGl2ZXJ6YS5jb20vbnMvYWRkZW5kYS9kaXZlcnphLzEgZmlsZTovVXNlcnMvb3N2YWxkb3NhbmNoZXovRG9jdW1lbnRzL0RJVkVSWkEvQWRkZW5kYV9EaXZlcnphX3YxLjEueHNkIiB2ZXJzaW9uPSIxLjEiIHhtbG5zPSJodHRwOi8vd3d3LmRpdmVyemEuY29tL25zL2FkZGVuZGEvZGl2ZXJ6YS8xIj4NCiAgICAgIDxnZW5lcmFsZXMgdGlwb0RvY3VtZW50bz0iRmFjdHVyYSIgdG90YWxDb25MZXRyYT0iKCBET1NDSUVOVE9TIFRSRUlOVEEgWSBET1MgIFBFU09TICAwMC8xMDAgTS5OLiApIiBudW1lcm9PcmRlbj0iIiAvPg0KICAgICAgPGNsYXZlc0Rlc2NyaXBjaW9uIGNfRm9ybWFQYWdvPSJFZmVjdGl2byIgY19Nb25lZGE9Ik1YTiIgY19UaXBvRGVDb21wcm9iYW50ZT0iSSIgY19NZXRvZG9QYWdvPSJQYWdvIGVuIHVuYSBzb2xhIGV4aGliaWNpw7NuIiBjX0x1Z2FyRXhwZWRpY2lvbj0iNjQ3MDAiIGNfUmVnaW1lbkZpc2NhbD0iR2VuZXJhbCBkZSBMZXkgUGVyc29uYXMgTW9yYWxlcyIgY19Vc29DRkRJPSJHYXN0b3MgZW4gZ2VuZXJhbCIgLz4NCiAgICAgIDxlbWlzb3IgdmVuZGVkb3I9IjEiPg0KICAgICAgICA8ZGF0b3NDb250YWN0b0UgdGVsZWZvbm89IlRFTC4gKDgxKSA4MTUwLTU1MDAiIGVtYWlsQ29tZXJjaWFsPSJjbGllbnRlLm10eUBtaWxlbmlvLmNvbSIgZW1haWxDb250YWN0bz0iRkFYLiAoODEpIDgxNTAtNTU2NyIgd2ViPSJ3d3cubWlsZW5pby5jb20iIC8+DQogICAgICAgIDxkb21pY2lsaW9GaXNjYWxFIGNhbGxlPSJNT1JFTE9TIiBudW1lcm89IjE2IiBjb2xvbmlhPSJDRU5UUk8iIGNpdWRhZD0iREVMLiBDVUFVSFRFTU9DIiBtdW5pY2lwaW89IkRFTC4gQ1VBVUhURU1PQyIgZXN0YWRvPSJDSVVEQUQgREUgTUVYSUNPIiBwYWlzPSJNRVhJQ08iIGNvZGlnb1Bvc3RhbD0iMDYwNDAiIC8+DQogICAgICA8L2VtaXNvcj4NCiAgICAgIDxyZWNlcHRvciBudW1DbGllbnRlPSIwMDAwMDE1NSAgICAgICAgICAgICI+DQogICAgICAgIDxkYXRvc0NvbnRhY3RvUiB0ZWxlZm9ubz0iMjEyMTMxMyIgZW1haWxDb21lcmNpYWw9ImNsaWVudGUubXR5QG1pbGVuaW8uY29tIiB3ZWI9Ind3dy5taWxlbmlvLmNvbSIgLz4NCiAgICAgICAgPGRvbWljaWxpb0Zpc2NhbFIgY2FsbGU9Ik1PUkVMT1MiIG51bWVybz0iMTYiIGNvbG9uaWE9IkNFTlRSTyIgY2l1ZGFkPSJDVUFVSFRFTU9DIiBtdW5pY2lwaW89IkNVQVVIVEVNT0MiIGVzdGFkbz0iQ0lVREFEIERFIE1FWElDTyIgcGFpcz0iTUVYSUNPIiBjb2RpZ29Qb3N0YWw9IjA2MDQwIiAvPg0KICAgICAgPC9yZWNlcHRvcj4NCiAgICAgIDxjb25jZXB0b3MgbnVtZXJvQ29uY2VwdG9zPSIxIj4NCiAgICAgICAgPGNvbmNlcHRvIGlkZW50aWZpY2Fkb3IxPSJJRDEiIG1lbnNhamU9IkNPTUVSQ0lBTCIgLz4NCiAgICAgIDwvY29uY2VwdG9zPg0KICAgIDwvZGl2ZXJ6YT4NCiAgPC9jZmRpOkFkZGVuZGE+DQo8L2NmZGk6Q29tcHJvYmFudGU=\"}";
            //string ErrorResponse = "{\"uuid\": \"011b5022-d468-433d-908c-0decc4c94ef8\",\"ref_id\": \"1543833\",\"content\": \"PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxjZmRpOkNvbXByb2JhbnRlIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTpzY2hlbWFMb2NhdGlvbj0iaHR0cDovL3d3dy5zYXQuZ29iLm14L2NmZC8zIGh0dHA6Ly93d3cuc2F0LmdvYi5teC9zaXRpb19pbnRlcm5ldC9jZmQvMy9jZmR2MzMueHNkIiBWZXJzaW9uPSIzLjMiIFNlcmllPSJURVNUIiBGb2xpbz0iOCIgRmVjaGE9IjIwMjEtMDQtMjNUMTg6MjE6NTQiIFNlbGxvPSIiIEZvcm1hUGFnbz0iMDEiIE5vQ2VydGlmaWNhZG89IjMwMDAxMDAwMDAwMzAwMDIzNzA4IiBDZXJ0aWZpY2Fkbz0iIiBDb25kaWNpb25lc0RlUGFnbz0iQ09OVEFETyIgU3ViVG90YWw9IjIwMC4wMDAwIiBNb25lZGE9Ik1YTiIgVG90YWw9IjIzMi4wMDAwIiBUaXBvRGVDb21wcm9iYW50ZT0iSSIgTWV0b2RvUGFnbz0iUFVFIiBMdWdhckV4cGVkaWNpb249IjY0NzAwIiB4bWxuczpjZmRpPSJodHRwOi8vd3d3LnNhdC5nb2IubXgvY2ZkLzMiPg0KICA8Y2ZkaTpFbWlzb3IgUmZjPSJNREk5OTEyMTRBNzQiIE5vbWJyZT0iTUlMRU5JTyBESUFSSU8sIFMuQS4gREUgQy5WLiIgUmVnaW1lbkZpc2NhbD0iNjAxIiAvPg0KICA8Y2ZkaTpSZWNlcHRvciBSZmM9IlhBWFgwMTAxMDEwMDAiIE5vbWJyZT0iVkVOVEEgQUwgUFVCTElDTyBHRU5FUkFMIiBVc29DRkRJPSJHMDMiIC8+DQogIDxjZmRpOkNvbmNlcHRvcz4NCiAgICA8Y2ZkaTpDb25jZXB0byBDbGF2ZVByb2RTZXJ2PSI4MjEwMTUwNCIgQ2FudGlkYWQ9IjEuMDAwMDAiIENsYXZlVW5pZGFkPSJFNDgiIFVuaWRhZD0iTk8gQVBMSUNBIiBEZXNjcmlwY2lvbj0iUE9SIENPTkNFUFRPIERFIFBVQkxJQ0lEQUQgTS4gRElBUklPIEZFRCIgVmFsb3JVbml0YXJpbz0iMjAwLjAwMDAwIiBJbXBvcnRlPSIyMDAuMDAwMDAiPg0KICAgICAgPGNmZGk6SW1wdWVzdG9zPg0KICAgICAgICA8Y2ZkaTpUcmFzbGFkb3M+DQogICAgICAgICAgPGNmZGk6VHJhc2xhZG8gQmFzZT0iMjAwLjAwIiBJbXB1ZXN0bz0iMDAyIiBUaXBvRmFjdG9yPSJUYXNhIiBUYXNhT0N1b3RhPSIwLjE2MDAwMCIgSW1wb3J0ZT0iMzIuMDAiIC8+DQogICAgICAgIDwvY2ZkaTpUcmFzbGFkb3M+DQogICAgICA8L2NmZGk6SW1wdWVzdG9zPg0KICAgIDwvY2ZkaTpDb25jZXB0bz4NCiAgPC9jZmRpOkNvbmNlcHRvcz4NCiAgPGNmZGk6SW1wdWVzdG9zIFRvdGFsSW1wdWVzdG9zVHJhc2xhZGFkb3M9IjMyLjAwIj4NCiAgICA8Y2ZkaTpUcmFzbGFkb3M+DQogICAgICA8Y2ZkaTpUcmFzbGFkbyBJbXB1ZXN0bz0iMDAyIiBUaXBvRmFjdG9yPSJUYXNhIiBUYXNhT0N1b3RhPSIwIiBJbXBvcnRlPSIzMi4wMCIgLz4NCiAgICA8L2NmZGk6VHJhc2xhZG9zPg0KICA8L2NmZGk6SW1wdWVzdG9zPg0KICA8Y2ZkaTpBZGRlbmRhPg0KICAgIDxkaXZlcnphIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhtbG5zOnRkQ0ZEST0iaHR0cDovL3d3dy5zYXQuZ29iLm14L3NpdGlvX2ludGVybmV0L2NmZC90aXBvRGF0b3MvdGRDRkRJIiB4c2k6c2NoZW1hTG9jYXRpb249Imh0dHA6Ly93d3cuZGl2ZXJ6YS5jb20vbnMvYWRkZW5kYS9kaXZlcnphLzEgZmlsZTovVXNlcnMvb3N2YWxkb3NhbmNoZXovRG9jdW1lbnRzL0RJVkVSWkEvQWRkZW5kYV9EaXZlcnphX3YxLjEueHNkIiB2ZXJzaW9uPSIxLjEiIHhtbG5zPSJodHRwOi8vd3d3LmRpdmVyemEuY29tL25zL2FkZGVuZGEvZGl2ZXJ6YS8xIj4NCiAgICAgIDxnZW5lcmFsZXMgdGlwb0RvY3VtZW50bz0iRmFjdHVyYSIgdG90YWxDb25MZXRyYT0iKCBET1NDSUVOVE9TIFRSRUlOVEEgWSBET1MgIFBFU09TICAwMC8xMDAgTS5OLiApIiBudW1lcm9PcmRlbj0iIiAvPg0KICAgICAgPGNsYXZlc0Rlc2NyaXBjaW9uIGNfRm9ybWFQYWdvPSJFZmVjdGl2byIgY19Nb25lZGE9Ik1YTiIgY19UaXBvRGVDb21wcm9iYW50ZT0iSSIgY19NZXRvZG9QYWdvPSJQYWdvIGVuIHVuYSBzb2xhIGV4aGliaWNpw7NuIiBjX0x1Z2FyRXhwZWRpY2lvbj0iNjQ3MDAiIGNfUmVnaW1lbkZpc2NhbD0iR2VuZXJhbCBkZSBMZXkgUGVyc29uYXMgTW9yYWxlcyIgY19Vc29DRkRJPSJHYXN0b3MgZW4gZ2VuZXJhbCIgLz4NCiAgICAgIDxlbWlzb3IgdmVuZGVkb3I9IjEiPg0KICAgICAgICA8ZGF0b3NDb250YWN0b0UgdGVsZWZvbm89IlRFTC4gKDgxKSA4MTUwLTU1MDAiIGVtYWlsQ29tZXJjaWFsPSJjbGllbnRlLm10eUBtaWxlbmlvLmNvbSIgZW1haWxDb250YWN0bz0iRkFYLiAoODEpIDgxNTAtNTU2NyIgd2ViPSJ3d3cubWlsZW5pby5jb20iIC8+DQogICAgICAgIDxkb21pY2lsaW9GaXNjYWxFIGNhbGxlPSJNT1JFTE9TIiBudW1lcm89IjE2IiBjb2xvbmlhPSJDRU5UUk8iIGNpdWRhZD0iREVMLiBDVUFVSFRFTU9DIiBtdW5pY2lwaW89IkRFTC4gQ1VBVUhURU1PQyIgZXN0YWRvPSJDSVVEQUQgREUgTUVYSUNPIiBwYWlzPSJNRVhJQ08iIGNvZGlnb1Bvc3RhbD0iMDYwNDAiIC8+DQogICAgICA8L2VtaXNvcj4NCiAgICAgIDxyZWNlcHRvciBudW1DbGllbnRlPSIwMDAwMDE1NSAgICAgICAgICAgICI+DQogICAgICAgIDxkYXRvc0NvbnRhY3RvUiB0ZWxlZm9ubz0iMjEyMTMxMyIgZW1haWxDb21lcmNpYWw9ImNsaWVudGUubXR5QG1pbGVuaW8uY29tIiB3ZWI9Ind3dy5taWxlbmlvLmNvbSIgLz4NCiAgICAgICAgPGRvbWljaWxpb0Zpc2NhbFIgY2FsbGU9Ik1PUkVMT1MiIG51bWVybz0iMTYiIGNvbG9uaWE9IkNFTlRSTyIgY2l1ZGFkPSJDVUFVSFRFTU9DIiBtdW5pY2lwaW89IkNVQVVIVEVNT0MiIGVzdGFkbz0iQ0lVREFEIERFIE1FWElDTyIgcGFpcz0iTUVYSUNPIiBjb2RpZ29Qb3N0YWw9IjA2MDQwIiAvPg0KICAgICAgPC9yZWNlcHRvcj4NCiAgICAgIDxjb25jZXB0b3MgbnVtZXJvQ29uY2VwdG9zPSIxIj4NCiAgICAgICAgPGNvbmNlcHRvIGlkZW50aWZpY2Fkb3IxPSJJRDEiIG1lbnNhamU9IkNPTUVSQ0lBTCIgLz4NCiAgICAgIDwvY29uY2VwdG9zPg0KICAgIDwvZGl2ZXJ6YT4NCiAgPC9jZmRpOkFkZGVuZGE+DQo8L2NmZGk6Q29tcHJvYmFudGU+\"}";
            string ErrorResponse= "{\"uuid\":\"92980451-3734-4ce0-89fc-02d0ad89d98f\",\"ref_id\":\"1543844\",\"content\":\"PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiID8+CjxjZmRpOkNvbXByb2JhbnRlIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTpzY2hlbWFMb2NhdGlvbj0iaHR0cDovL3d3dy5zYXQuZ29iLm14L2NmZC8zIGh0dHA6Ly93d3cuc2F0LmdvYi5teC9zaXRpb19pbnRlcm5ldC9jZmQvMy9jZmR2MzMueHNkIiBWZXJzaW9uPSIzLjMiIFNlcmllPSJURVNUIiBGb2xpbz0iMTYiIEZlY2hhPSIyMDIxLTA0LTI3VDE4OjUwOjQ2IiBTZWxsbz0iZ0N1ZFJKL0JPRjRvekEyWnJxUllHem1Yd0s3VWlXUStMK0xaN3N1eHlXTzBXTG1KMCtGN01ZNDhwRFYreXVFc01uSVFBZkhjb3BwNVAyMFB0aU5RL1BMYjhJeGRvb0VISUhkMW5oS0pzNW5FZHJvcW9UQTByaXlmb1ZualpDRnJZSG93ME5ucnN5aGNxWWgySnpPYXkzdGJjYnIwRWo5VVZqZmhwR3NjNUpERzJKbXFmYXU3MVY4LzN2bEpIdU1SV3RGcFIzT05xZFp3U1YrT0wxNkd0TEt0QXNKbXo5Y0kxOUNIbXgrWWg2V0IwVERCbHIzbzM3dktwMXprRHBvRGRCU2E3UVdjMElSV3g4NmJBcDNaYkcrRDBlMWd6UEdLUURPKys1NjVaZi9NMCtYcUZsWU5yRVZYa3IzeUJhK1lWQi96ZkhPWkk0dDZ3WHI3dDBVaW1BPT0iIEZvcm1hUGFnbz0iMDEiIE5vQ2VydGlmaWNhZG89IjMwMDAxMDAwMDAwMzAwMDIzNzA4IiBDZXJ0aWZpY2Fkbz0iTUlJRitUQ0NBK0dnQXdJQkFnSVVNekF3TURFd01EQXdNREF6TURBd01qTTNNRGd3RFFZSktvWklodmNOQVFFTEJRQXdnZ0ZtTVNBd0hnWURWUVFEREJkQkxrTXVJRElnWkdVZ2NISjFaV0poY3lnME1EazJLVEV2TUMwR0ExVUVDZ3dtVTJWeWRtbGphVzhnWkdVZ1FXUnRhVzVwYzNSeVlXTnB3N051SUZSeWFXSjFkR0Z5YVdFeE9EQTJCZ05WQkFzTUwwRmtiV2x1YVhOMGNtRmphY096YmlCa1pTQlRaV2QxY21sa1lXUWdaR1VnYkdFZ1NXNW1iM0p0WVdOcHc3TnVNU2t3SndZSktvWklodmNOQVFrQkZocGhjMmx6Ym1WMFFIQnlkV1ZpWVhNdWMyRjBMbWR2WWk1dGVERW1NQ1FHQTFVRUNRd2RRWFl1SUVocFpHRnNaMjhnTnpjc0lFTnZiQzRnUjNWbGNuSmxjbTh4RGpBTUJnTlZCQkVNQlRBMk16QXdNUXN3Q1FZRFZRUUdFd0pOV0RFWk1CY0dBMVVFQ0F3UVJHbHpkSEpwZEc4Z1JtVmtaWEpoYkRFU01CQUdBMVVFQnd3SlEyOTViMkZqdzZGdU1SVXdFd1lEVlFRdEV3eFRRVlE1TnpBM01ERk9Uak14SVRBZkJna3Foa2lHOXcwQkNRSU1FbEpsYzNCdmJuTmhZbXhsT2lCQlEwUk5RVEFlRncweE56QTFNVGd3TXpVME5UWmFGdzB5TVRBMU1UZ3dNelUwTlRaYU1JSGxNU2t3SndZRFZRUURFeUJCUTBORlRTQlRSVkpXU1VOSlQxTWdSVTFRVWtWVFFWSkpRVXhGVXlCVFF6RXBNQ2NHQTFVRUtSTWdRVU5EUlUwZ1UwVlNWa2xEU1U5VElFVk5VRkpGVTBGU1NVRk1SVk1nVTBNeEtUQW5CZ05WQkFvVElFRkRRMFZOSUZORlVsWkpRMGxQVXlCRlRWQlNSVk5CVWtsQlRFVlRJRk5ETVNVd0l3WURWUVF0RXh4QlFVRXdNVEF4TURGQlFVRWdMeUJJUlVkVU56WXhNREF6TkZNeU1SNHdIQVlEVlFRRkV4VWdMeUJJUlVkVU56WXhNREF6VFVSR1VrNU9NRGt4R3pBWkJnTlZCQXNVRWtOVFJEQXhYMEZCUVRBeE1ERXdNVUZCUVRDQ0FTSXdEUVlKS29aSWh2Y05BUUVCQlFBRGdnRVBBRENDQVFvQ2dnRUJBSmRVY3NISUVJZ3dpdnZBYW50R25ZVklPMys3eVRkRDF0a0tvcGJMK3RLU2pSRm8xRXJQZEdKeFAzZ3hUNU8rQUNJRFFYTitIUzl1TVdEWW5hVVJhbFNJRjlDT0ZDZGgvT0gyUG4rVW1rTjRjdWxyMkRhbkt6dFZJTzhpZFhNNmM5YUhuNWhPbzdoRHhYTUMzdU91R1YzRlM0T2JreFRWKzlOc3ZPQVYybE1lMjdTSHJTQjBEaHVMdXJVYlp3WG0rL3I0ZHR6M2IydUxnQmMrRGl5OTVQRytNSXU3b05LTTg5YUJOR2NqVEp3KzlrK1d6SmlQZDNacFFnSWVkWUJEKzhRV3hsWUNneGhudGEzazl5bGdYS1lYQ1lrMGswcWF1dkJKMWpTUlZmNUJqaklVYk9zdGFRcDU5bmtnSGg0NWM5Z253SlJWNjE4TlcwZk1lRHp1S1IwQ0F3RUFBYU1kTUJzd0RBWURWUjBUQVFIL0JBSXdBREFMQmdOVkhROEVCQU1DQnNBd0RRWUpLb1pJaHZjTkFRRUxCUUFEZ2dJQkFCS2owRENOTDFsaDQ0eStPY1dGclQyaWNuS0Y3V3lTT1ZpaHgwb1IrSFByV0tCTVh4bzlLdHJvZG5CMXRnSXg4ZitYanF5cGhoYncranVEU2VEcmI5OVBoQzQrRTZKZVhPa2RRY0p0NTBLeW9kbDlVUnBDVldOV2pVYjNGL3lwYThvVGNmZi9lTWZ0UVpUN01RMUxxaHQreG0zUWhWb3hUSUFTY2UwampzbkJUR0QySlE0dVQzb0NlbThibW9NWFYvZms5YUozdjArWklMNDJNcFk0UE9HVWEvaVRhYXdrbEtSQUwxWGo5SWRJUjA2Uks2OFJTNnhyR2s2andiRFRFS3hKcG1aM1NQTHRsc21QVVRPMWtyYVRQSW85RkNtVS96WmtXR3BkOFpFQUFGdytaZkkrYmRYQmZ2ZER3YU0yaU1HVFFaVFRFZ1U1S0tUSXZrQW5IbzlPNDVTcVNKd3FWOU5MZlBBeENvNWVSUjJPR2liZDlqaEhlODF6VXNwNUdkRTFtWmlTcUpVODJIM2N1NkJpRStEM1liWmVabmpyTlN4QmdLVElmOHcrS05ZUE00YVdudVVNbDBtTGd0T3hUVVhpOU1LblVjY3EzR1pMQTdieDdabjIxMXlQUnFFalNBcXliVU1WSU9obzZhcXprZmMzV0xaNkxuR1UraHlIdVpVZlB3Ym5DbGI3b0ZGejFQbHZHT3BORHNVYjBxUDQyUUNHQmlUVXNlR3VnQXpxT1A2RVlwVlBDNzNnRm91cm1kQlFnZmF5YUV2aTN4ak5hbkZrUGxXMVhFWU5yWUpCNHlOanBoRnJ2V3dUWTg2dkwybzhnWk4wVXRtYzVmbm9CVGZNOXIyelZLbUVpNkZVZUoxaWFEYVZOdjQ3dGU5aVMxYWk0VjR2Qlk4ciIgQ29uZGljaW9uZXNEZVBhZ289IkNPTlRBRE8iIFN1YlRvdGFsPSIxMDAiIE1vbmVkYT0iTVhOIiBUb3RhbD0iMTE2IiBUaXBvRGVDb21wcm9iYW50ZT0iSSIgTWV0b2RvUGFnbz0iUFVFIiBMdWdhckV4cGVkaWNpb249IjY0NzAwIiB4bWxuczpjZmRpPSJodHRwOi8vd3d3LnNhdC5nb2IubXgvY2ZkLzMiPgogIDxjZmRpOkVtaXNvciBSZmM9Ik1ESTk5MTIxNEE3NCIgTm9tYnJlPSJNSUxFTklPIERJQVJJTywgUy5BLiBERSBDLlYuIiBSZWdpbWVuRmlzY2FsPSI2MDEiLz4KICA8Y2ZkaTpSZWNlcHRvciBSZmM9IlhBWFgwMTAxMDEwMDAiIE5vbWJyZT0iVkVOVEEgQUwgUFVCTElDTyBHRU5FUkFMIiBVc29DRkRJPSJHMDMiLz4KICA8Y2ZkaTpDb25jZXB0b3M+CiAgICA8Y2ZkaTpDb25jZXB0byBDbGF2ZVByb2RTZXJ2PSI4MjEwMTUwNCIgQ2FudGlkYWQ9IjEuMDAwMDAiIENsYXZlVW5pZGFkPSJFNDgiIFVuaWRhZD0iTk8gQVBMSUNBIiBEZXNjcmlwY2lvbj0iUE9SIENPTkNFUFRPIERFIFBVQkxJQ0lEQUQgTS4gRElBUklPIEZFRCIgVmFsb3JVbml0YXJpbz0iMTAwLjAwMDAwIiBJbXBvcnRlPSIxMDAuMDAwMDAiPgogICAgICA8Y2ZkaTpJbXB1ZXN0b3M+CiAgICAgICAgPGNmZGk6VHJhc2xhZG9zPgogICAgICAgICAgPGNmZGk6VHJhc2xhZG8gQmFzZT0iMTAwLjAwIiBJbXB1ZXN0bz0iMDAyIiBUaXBvRmFjdG9yPSJUYXNhIiBUYXNhT0N1b3RhPSIwLjE2MDAwMCIgSW1wb3J0ZT0iMTYuMDAiLz4KICAgICAgICA8L2NmZGk6VHJhc2xhZG9zPgogICAgICA8L2NmZGk6SW1wdWVzdG9zPgogICAgPC9jZmRpOkNvbmNlcHRvPgogIDwvY2ZkaTpDb25jZXB0b3M+CiAgPGNmZGk6SW1wdWVzdG9zIFRvdGFsSW1wdWVzdG9zVHJhc2xhZGFkb3M9IjE2LjAwIj4KICAgIDxjZmRpOlRyYXNsYWRvcz4KICAgICAgPGNmZGk6VHJhc2xhZG8gSW1wdWVzdG89IjAwMiIgVGlwb0ZhY3Rvcj0iVGFzYSIgVGFzYU9DdW90YT0iMC4xNjAwMDAiIEltcG9ydGU9IjE2LjAwIi8+CiAgICA8L2NmZGk6VHJhc2xhZG9zPgogIDwvY2ZkaTpJbXB1ZXN0b3M+CiAgPGNmZGk6Q29tcGxlbWVudG8+CiAgICAgICAgPHRmZDpUaW1icmVGaXNjYWxEaWdpdGFsIHhtbG5zOnRmZD0iaHR0cDovL3d3dy5zYXQuZ29iLm14L1RpbWJyZUZpc2NhbERpZ2l0YWwiIHhzaTpzY2hlbWFMb2NhdGlvbj0iaHR0cDovL3d3dy5zYXQuZ29iLm14L1RpbWJyZUZpc2NhbERpZ2l0YWwgaHR0cDovL3d3dy5zYXQuZ29iLm14L3NpdGlvX2ludGVybmV0L2NmZC9UaW1icmVGaXNjYWxEaWdpdGFsL1RpbWJyZUZpc2NhbERpZ2l0YWx2MTEueHNkIiBWZXJzaW9uPSIxLjEiIFVVSUQ9IjkyOTgwNDUxLTM3MzQtNGNlMC04OWZjLTAyZDBhZDg5ZDk4ZiIgUmZjUHJvdkNlcnRpZj0iU1BSMTkwNjEzSTUyIiBGZWNoYVRpbWJyYWRvPSIyMDIxLTA0LTI3VDE4OjUzOjEzIiBTZWxsb0NGRD0iZ0N1ZFJKL0JPRjRvekEyWnJxUllHem1Yd0s3VWlXUStMK0xaN3N1eHlXTzBXTG1KMCtGN01ZNDhwRFYreXVFc01uSVFBZkhjb3BwNVAyMFB0aU5RL1BMYjhJeGRvb0VISUhkMW5oS0pzNW5FZHJvcW9UQTByaXlmb1ZualpDRnJZSG93ME5ucnN5aGNxWWgySnpPYXkzdGJjYnIwRWo5VVZqZmhwR3NjNUpERzJKbXFmYXU3MVY4LzN2bEpIdU1SV3RGcFIzT05xZFp3U1YrT0wxNkd0TEt0QXNKbXo5Y0kxOUNIbXgrWWg2V0IwVERCbHIzbzM3dktwMXprRHBvRGRCU2E3UVdjMElSV3g4NmJBcDNaYkcrRDBlMWd6UEdLUURPKys1NjVaZi9NMCtYcUZsWU5yRVZYa3IzeUJhK1lWQi96ZkhPWkk0dDZ3WHI3dDBVaW1BPT0iIE5vQ2VydGlmaWNhZG9TQVQ9IjMwMDAxMDAwMDAwNDAwMDAyNDk1IiBTZWxsb1NBVD0iUk9DN1g3NTdjMDVjQXZiQTBhQm1SWVJzb0I1S3NiNFBnRXN5TjY1U2RnRUtmZVhoNXZ3SGNHbXM2VkdnWjQvNFFLTUUxSmRRRldLR00xelBESnhQUnFFNFQrS0RoU290enF4TU8xVCsvYWxHa25sWVdaMUROWnExTDBPWXR3djBDazZsNnlHN0VONnhVYWZYVXp5SUtzRWVlR0JuS2k4Um5Ka1IzZU9xZElSMmxhTUR4dFp1d214OWRYalV4a1YxZjhoZnFWZmMyMlI2eWcrV05sUTZwcnJiN0ZpbGhzNWdVOVdWTjBkL2hzaGVaQUR4QWU3aHpYNHdrUVNwWE80UmVuLzJpUWE5aStRb1FmVCtSSmoyLzV2alRjbW9qMnFoV0VDQVIyT1FpYnlGVk0zSStTSnkwMG1xLytuSHljSmhFb3dJV0g3Z3EwRW1zMk5MVy9YMG13PT0iLz4KICAgIDwvY2ZkaTpDb21wbGVtZW50bz48Y2ZkaTpBZGRlbmRhPgogICAgPGRpdmVyemEgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeG1sbnM6dGRDRkRJPSJodHRwOi8vd3d3LnNhdC5nb2IubXgvc2l0aW9faW50ZXJuZXQvY2ZkL3RpcG9EYXRvcy90ZENGREkiIHhzaTpzY2hlbWFMb2NhdGlvbj0iaHR0cDovL3d3dy5kaXZlcnphLmNvbS9ucy9hZGRlbmRhL2RpdmVyemEvMSBmaWxlOi9Vc2Vycy9vc3ZhbGRvc2FuY2hlei9Eb2N1bWVudHMvRElWRVJaQS9BZGRlbmRhX0RpdmVyemFfdjEuMS54c2QiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cuZGl2ZXJ6YS5jb20vbnMvYWRkZW5kYS9kaXZlcnphLzEiPgogICAgICA8Z2VuZXJhbGVzIHRpcG9Eb2N1bWVudG89IkZhY3R1cmEiIHRvdGFsQ29uTGV0cmE9IiggQ0lFTlRPIERJRUNJU0VJUyAgUEVTT1MgIDAwLzEwMCBNLk4uICkiIG51bWVyb09yZGVuPSIiLz4KICAgICAgPGNsYXZlc0Rlc2NyaXBjaW9uIGNfRm9ybWFQYWdvPSJFZmVjdGl2byIgY19Nb25lZGE9Ik1YTiIgY19UaXBvRGVDb21wcm9iYW50ZT0iSSIgY19NZXRvZG9QYWdvPSJQYWdvIGVuIHVuYSBzb2xhIGV4aGliaWNpw7NuIiBjX0x1Z2FyRXhwZWRpY2lvbj0iNjQ3MDAiIGNfUmVnaW1lbkZpc2NhbD0iR2VuZXJhbCBkZSBMZXkgUGVyc29uYXMgTW9yYWxlcyIgY19Vc29DRkRJPSJHYXN0b3MgZW4gZ2VuZXJhbCIvPgogICAgICA8ZW1pc29yIHZlbmRlZG9yPSIxIj4KICAgICAgICA8ZGF0b3NDb250YWN0b0UgdGVsZWZvbm89IlRFTC4gKDgxKSA4MTUwLTU1MDAiIGVtYWlsQ29tZXJjaWFsPSJjbGllbnRlLm10eUBtaWxlbmlvLmNvbSIgZW1haWxDb250YWN0bz0iRkFYLiAoODEpIDgxNTAtNTU2NyIgd2ViPSJ3d3cubWlsZW5pby5jb20iLz4KICAgICAgICA8ZG9taWNpbGlvRmlzY2FsRSBjYWxsZT0iTU9SRUxPUyIgbnVtZXJvPSIxNiIgY29sb25pYT0iQ0VOVFJPIiBjaXVkYWQ9IkRFTC4gQ1VBVUhURU1PQyIgbXVuaWNpcGlvPSJERUwuIENVQVVIVEVNT0MiIGVzdGFkbz0iQ0lVREFEIERFIE1FWElDTyIgcGFpcz0iTUVYSUNPIiBjb2RpZ29Qb3N0YWw9IjA2MDQwIi8+CiAgICAgIDwvZW1pc29yPgogICAgICA8cmVjZXB0b3IgbnVtQ2xpZW50ZT0iMDAwMDAxNTUgICAgICAgICAgICAiPgogICAgICAgIDxkYXRvc0NvbnRhY3RvUiB0ZWxlZm9ubz0iMjEyMTMxMyIgZW1haWxDb21lcmNpYWw9ImNsaWVudGUubXR5QG1pbGVuaW8uY29tIiB3ZWI9Ind3dy5taWxlbmlvLmNvbSIvPgogICAgICAgIDxkb21pY2lsaW9GaXNjYWxSIGNhbGxlPSJNT1JFTE9TIiBudW1lcm89IjE2IiBjb2xvbmlhPSJDRU5UUk8iIGNpdWRhZD0iQ1VBVUhURU1PQyIgbXVuaWNpcGlvPSJDVUFVSFRFTU9DIiBlc3RhZG89IkNJVURBRCBERSBNRVhJQ08iIHBhaXM9Ik1FWElDTyIgY29kaWdvUG9zdGFsPSIwNjA0MCIvPgogICAgICA8L3JlY2VwdG9yPgogICAgICA8Y29uY2VwdG9zIG51bWVyb0NvbmNlcHRvcz0iMSI+CiAgICAgICAgPGNvbmNlcHRvIGlkZW50aWZpY2Fkb3IxPSJJRDEiIG1lbnNhamU9IkNPTUVSQ0lBTCIvPgogICAgICA8L2NvbmNlcHRvcz4KICAgIDwvZGl2ZXJ6YT4KICA8L2NmZGk6QWRkZW5kYT4KPC9jZmRpOkNvbXByb2JhbnRlPg==\"}";
        System.Web.Script.Serialization.JavaScriptSerializer serializador = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> diccionario = (Dictionary<string, object>)(serializador.DeserializeObject(ErrorResponse));
            string Respuesta = diccionario["content"].ToString();
            System.Xml.Serialization.XmlSerializer Z = new System.Xml.Serialization.XmlSerializer(Respuesta.GetType());
            StreamWriter writer = new StreamWriter(CFDiRutaRequest + "\\RTest0006.XML");
            Z.Serialize(writer, Respuesta);

            string sPDF = "";
            Byte[] aFile;
            FileStream fs;
            FileStream fs2;
            //byte[] GraficoCB;
            aFile = Convert.FromBase64String(Respuesta);

            try
            {
                //'2. El archivo que se le entrega al Cliente viene en el attributo "archivo" y se graba en binario, se trae a ruta local para extraer datos
                if (aFile != null && aFile.Length > 0)
                {
                    sPDF = CFDiRutaRequest + "Test0006.XML";
                    fs = new FileStream(sPDF, FileMode.Create);
                    fs.Write(aFile, 0, aFile.Length);
                    fs.Close();
                }
                else
                {
                    RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Generar versión 3.3:  ");
                }
                
            }
            catch (Exception ex)
            {
                RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Genera:" + sPDF + " :" + ex.ToString());

                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();
            }
            
            //'EXTRACCION DEL SELLO SAT
            string SelloSAt = "";
            string luuid = "";
            string lCertificadoSat = "";
            DateTime lFecha = DateTime.Now;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            try
            {
                using (XmlReader reader = XmlReader.Create(sPDF, settings))
                {

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && "Comprobante" == reader.LocalName)
                        {
                            lCertificadoSat = reader.GetAttribute("Certificado").ToString();
                        }
                        if (reader.NodeType == XmlNodeType.Element && "TimbreFiscalDigital" == reader.LocalName)
                        {
                            luuid = reader.GetAttribute("UUID").ToString();
                            SelloSAt = reader.GetAttribute("SelloSAT").ToString();
                            lFecha = Convert.ToDateTime(reader.GetAttribute("FechaTimbrado").ToString());
                        }
                    }
                }
                //Ldia = lFecha.Day;
                //Lmes = lFecha.Month;
                //Lanio = lFecha.Year;
                //CreaDirectorios();

                //if (aFile != null && aFile.Length > 0)
                //{
                //    sPDF = RutaXMLGraba + "\\" + lArchivo + ".XML";
                //    Registralog("GeneraXML", pFOLIO, Operacion, "SE GENERARA XML:" + sPDF);
                //    fs2 = new FileStream(sPDF, FileMode.Create);
                //    fs2.Write(aFile, 0, aFile.Length);
                //    fs2.Close();
                //}
                //else
                //    RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Genera: Nodo (Respuesta.InfoCFDi.archivo) esta vacio ");
            }
            catch (Exception ex)
            {
                RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Genera:" + sPDF + " :" + ex.ToString());

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

        private void Parametros()
        {
            try
            {
                Timer1.Enabled = true;
                Timer2.Enabled = true;
                Etiqueta.Text = "Esperando";
                //r.Servidor = "192.168.17.38";
                //r.BaseDatos = "p5milenio";                
                r.Servidor = ConfigurationManager.AppSettings["Server"].ToString();
                r.BaseDatos = ConfigurationManager.AppSettings["DataBase"].ToString();
                //NotifyIcon1.ShowBalloonTip(10000);
                CFDIncluirRefBancaria = r.GetP5sistema("CFDIncluirRefBancaria", "gen", "mmx");
                CreaRequestXML = r.GetP5sistema("CreaRequestXML", "gen", "mmx");                
                Int32.TryParse(r.GetP5sistema("TiempoEsperaCDFi", "gen", "mmx"), out TiempoEspera);
                STRSolicitaPDF = r.GetP5sistema("SolicitaPDF", "gen", "mmx");
                RutaXMLCFD = r.GetP5sistema("RutaXMLCFD", "gen", "mmx");
                RutaPDFCFD = r.GetP5sistema("RutaPDFCFD", "gen", "mmx");
                FormatoRutaCFD = r.GetP5sistema("FormatoRutaCFD", "gen", "mmx");
                RutaCertificado = r.GetP5sistema("RutaCertificado", "gen", "mmx");
                PswCertificado = r.GetP5sistema("PswCertificado", "gen", "mmx");
                CFDiURLSoriana = r.GetP5sistema("CFDiURLSoriana", "gen", "mmx");
                CFDiURLpdf = r.GetP5sistema("CFDiURLpdf", "gen", Empresa);
                CFDiRutaRequest = r.GetP5sistema("CFDiRutaRequest", "gen", "mmx");
                CFDiURLSoriana = r.GetP5sistema("CFDiURLSoriana", "gen", "mmx");
                UsaLog = r.GetP5sistema("CFDLOG", "GRAL", "MMX") == "S" ? true : false;
                ParamArticuloesp25 = r.GetP5sistema("ARTRET2.5", "adv", "mmx");
                ClaveImpRetencion = r.GetP5sistema("ClaveRet2.5", "adv", "mmx");
                string CFDiURLPrametro = r.GetP5sistema("CFDiURL", "gen", "MMY");

                if (CFDiRutaRequest == "")
                    CFDiRutaRequest = "c:" + "\\" + "XML33" + "\\";

                VersionINE = r.GetP5sistema("VersionINE", "gen", "mmx");
                VersionINE = (VersionINE).ToString().Trim();
                listaDeCliente = r.GetP5sistema("CFDListaCliente", "gen", "mmx");

                //'' * *************************************************************
                //''Comentar las siguientes lineas de pruebas
                //'' * *********                                                                                                    ****************************************************
                //'' * *************************************************************
                //'CFDiURL = "https://demonegocios.buzonfiscal.com/bfcorpcfdi32ws"
                //' ''"https://demonegocios.buzonfiscal.com/bfcorpcfdi32ws"
                //'CFDiURLpdf = CFDiURL

                //'RutaCertificado = "C:\AAA010101AAA.pfx"
                //'PswCertificado = "AAA010101AAA"
                //'' * ************************************************************
                //'' * ************************************************************
                
                UsaReferencia = r.GetP5sistema("CFDUsaRef", "GEN", "MMX") == "S" ? true : false;
                Label3.Text = "Servidor:" + r.Servidor;
                Label4.Text = "BD:      " + r.BaseDatos;
                Label5.Text = "Certificado:" + RutaCertificado;
                Label6.Text = "Log Activo:" + (UsaLog == true ? "SI" : "NO");
                Label7.Text = "XML: " + RutaXMLCFD;
                Label8.Text = "URL Timbre: " + CFDiURLPrametro;
                Label9.Text = "";

                //Etiqueta.Text = "";
                EtiquetaSQL.Text = "";
            }
            catch (Exception ex)
            {
                Label3.Text = "Servidor:" + r.Servidor;
                Label4.Text = "BD:      " + r.BaseDatos;
                Label5.Text = "Certificado:";
                Label6.Text = "Log Activo:" + (UsaLog == true ? "SI" : "NO");
                Label7.Text = "XML: ";
                Label8.Text = "URL Timbre: ";
                Label9.Text = "";
                Etiqueta.Text = "Error al tratar de obtener los parametros iniciales";
                EtiquetaSQL.Text = ex.ToString();
                Button1.Enabled = false;

                //carlos log
                //string ruta = "c:\"xml33\"fichero.txt";
                //StreamWriter escritor;
                //escritor = File.AppendText(ruta);
                //escritor.Write(ex.ToString());
                //escritor.Flush();
                //    escritor.Close();
                string a = r.GetP5sistema("TiempoEsperaCDFi", "gen", "mmx");
                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}"," ----  " + a + "  ---- "+  ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();

            }
        }

        private void Registralog(string aProceso, string aFolio, string aOperacion, string aDescripcion)
        {
            //UsaLog = true;
            if (UsaLog)
            {
                r.exeQry("Exec CFDGrabaLog '" + aProceso + "','" + aFolio + "','" + aOperacion + "','" + aDescripcion + "'");
            }
        }
        //Acción Principal
        private void Button1_Click(object sender, EventArgs e)
        {
            /// ENVIAR DOCUMENTOS CARLOS LOERA
            //Registralog("ENVIOPORBOTON",  "GENERAL", "gENERAL", "INICIA ENVIO DE FACTURAS PORQUE SE PRESIONO EL BOTON")
            //Registralog("ENVIOPORBOTON", "GENERAL", "gENERAL", "INICIA ENVIO DE FACTURAS PORQUE SE PRESIONO EL BOTON");

            //ConsultaEstatusFacturasSAT();
            //GenerXMLEnviaTest();
            //EnviaCFD();


            ////PASO 1
            CancelaFacturasSAt();
            ////PASO 2 CONSULTA ESTATUS DE FACTURAS
            ConsultaEstatusFacturasSAT();
            CancelaPagosSAT();
            Button1.Enabled = true;


        }

        private void ActulizaSegundero() {
            Label9.Refresh();
            Label9.Text = "Segundos en espera:" + Segundos.ToString();
            Label9.Refresh();
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            //Registralog("ENVIOAUTOMATICO",  "", "", "SE INICIA ENVIO AUTOMATICO DE DOCUMENTOS")
            Registralog("ENVIOAUTOMATICO", "", "", "SE INICIA ENVIO AUTOMATICO DE DOCUMENTOS  A CANCELAR");
            CancelaFacturasSAt();
            ////PASO 2 CONSULTA ESTATUS DE FACTURAS
            ConsultaEstatusFacturasSAT();
            CancelaPagosSAT();
            Segundos = 0;
            Label9.Text = "Segundos en espera:" + Segundos.ToString().Trim();
            this.Refresh();
            Button1.Enabled = true;
            //EnviaCFD();
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            Segundos = Segundos + 1;
            ActulizaSegundero();
            if (Segundos > (Timer2.Interval / 1000) + 1)
                Timer2.Enabled = true;
        }

        public void RegistraError(string lIDunico, string Mensaje, string Proceso)
        {
            //' Dim dverror As New DataView
            try
            {
                string path = "c:\\xml33\\";
                StreamWriter w1 = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w1.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                        w1.WriteLine("PAso RegistraError " + lIDunico + ", " + Mensaje + " ," + Proceso );
                w1.WriteLine("-------------------------------");
                w1.Close();

                r.exeQry("Exec CFdRegistraError '" + pFOLIO + "','" + Operacion + "','" + Proceso + "','" + Mensaje.Replace("'", "''") + " :" + lIDunico + "','" + Empresa + "'");
                //' r.exeQry("Update CFDFActura set estatus ='X' where iddocto = " & IdDocto & " and operacion ='" & Operacion & "'")
                if (Proceso == "Addenda")
                {
                    StreamWriter w2 = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                    //w.Write("\r\nLog Entry : ");
                    w2.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        DateTime.Now.ToLongDateString());
                    w2.WriteLine("ADENDA");
                    w2.WriteLine("-------------------------------");
                    w2.Close();
                    r.exeQry("Update CFDFActura set estatus ='Q' where id = " + lIDunico + " and operacion ='" + Operacion + "'");
                }
                else
                {
                    StreamWriter w3 = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                    //w.Write("\r\nLog Entry : ");
                    w3.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        DateTime.Now.ToLongDateString());
                    w3.WriteLine(" no ADENDA");
                    w3.WriteLine("-------------------------------");
                    w3.Close();
                    r.exeQry("Update CFDFActura set estatus ='X' where id = " + lIDunico + " and operacion ='" + Operacion + "' and estatus <> 'C' ");
                }
            }
            catch (Exception ex)
            {
                this.EtiquetaSQL.Text = "Error al intentar conectar con la BD. Proceso: RegistraError";
                this.Refresh();
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
        //Timbrado de Facturas/Notas de Credito
        private void EnviaCFD()
        {
            UsaLog = true;
            Button1.Enabled = true;
            Etiqueta.Text = "";
            EtiquetaSQL.Text = "";
            try
            {

                if (Procesando)
                {
                    Timer1.Enabled = false;
                }

                //On Error Resume Next
                Button1.Enabled = false;
                Registralog("INICIAENVIO", "GENERAL", "GENERAL", "SE PROCEDE A ENVIAR DOCUMENTOS CFDI");

                Procesando = true;
                DataView DVFacturas = new DataView();
                DataView dv = new DataView();
                DataView dvadenda = new DataView();

                int i = 0;
                int l = 0;

                Etiqueta.Text = "Buscando Facturas";
                this.Refresh();

                //'********* LINEA DE PRODUCCION *************************************
                DVFacturas = r.GetDataView("select  FolioParnet, Operacion, Empresa, Documento, Plaza, Id , Estatus, idDocto , FolioFactura   from CFDI33Pendientes");
                //'********* LINEA DE PRODUCCION *************************************

                //'********* LINEA DE PRUEBAS *************************************
                //'DVFacturas = r.GetDataView("select  FolioParnet, Operacion, Empresa, Documento, Plaza, Id , Estatus, idDocto , FolioFactura   from CFDI33PendientesbORRAR")
                //'select FolioParnet, Operacion, Empresa, Documento,  dbo.fnPlazaEmpresa (Empresa) as Plaza , Id , Estatus, idDocto     from cfdfactura where folioparnet ='B0003171'
                //'**********  FIN DE LINEA DE PRUEBAS
                Segundos = 0;
                Timer2.Enabled = false;
                ActulizaSegundero();

                //Registralog("BUSCANDOFAC",  "GENERAL", "GENERAL", "SE PROCEDE A ENVIAR DOCUMENTOS CFDI");
                Registralog("BUSCANDOFAC", "GENERAL", "GENERAL", "SE PROCEDE A ENVIAR DOCUMENTOS CFDI");
                Timer1.Enabled = false;
                //'Try
                if (DVFacturas.Count > 0)
                {
                    i = DVFacturas.Count;
                    Registralog("BUSCANDOFAC", "GENERAL", "GENERAL", "SE ENCONTRARON " + i.ToString() + " DOCUMENTOS");

                    //'Para no pedir facturas  -- en pruebas
                    //'i = 0

                    //for n = 0 To i -1[n]
                    for (int n = 0; n < i; n++)
                    {
                        pFOLIO = DVFacturas[n]["FolioParnet"].ToString().ToUpper().Trim();
                        Operacion = DVFacturas[n]["Operacion"].ToString().ToUpper().Trim();
                        Empresa = DVFacturas[n]["Empresa"].ToString().ToUpper().Trim();
                        Documento = DVFacturas[n]["Documento"].ToString().ToUpper().Trim();
                        Plaza = DVFacturas[n]["Plaza"].ToString().ToUpper().Trim();
                        if (Operacion == "CNCRED")
                            FolioFActura = DVFacturas[n]["FolioFactura"].ToString().ToUpper().Trim(); //  'Factura que se aplica en NC para complemento INE
                        else
                            FolioFActura = pFOLIO;

                        //'*****************Quitar + 50
                        IDunico = DVFacturas[n]["Id"].ToString();
                        Registralog("DOCUMENTOLOCALIZADO", pFOLIO, Operacion, "DOCUMENTO POR PROCESAR.  ESTATUS :" + DVFacturas[n]["Estatus"].ToString().ToUpper().Trim());
                        if (Operacion == "FACTURA")
                        {
                            Etiqueta.Text = "Procesando factura: " + pFOLIO;
                            this.Refresh();
                            if (DVFacturas[n]["Estatus"].ToString().ToUpper().Trim() == "S")
                            {
                                Registralog("PROCESOFACTURA", pFOLIO, Operacion, "SE IDENTIFICO COMO FACTUARA, SE BUSCARAN SUS CONCEPTOS");
                                r.exeQry("exec CFDdatosFacturaPrevio   " + DVFacturas[n]["idDocto"].ToString());
                                dvadenda = new DataView();
                                dvadenda = r.GetDataView("exec CFDdatosFactura " + DVFacturas[n]["idDocto"].ToString());

                                if (dvadenda[0]["RetVal"].ToString() == "0")
                                {
                                    dvadenda = new DataView();
                                    dvadenda = r.GetDataView("select I.*, isnull(V.Nombre,'') AgenteVEntas , E.Telefono, e.Fax, " +
                                    " email = (dbo.fnClienteEmail(I.ClaveCliente, i.Empresa,'') ),isnull(Ex.observacion1,'') Mensaje1,isnull(Ex.observacion2,'') Mensaje2,isnull(Ex.observacion3,'') Mensaje3,  '----' as comprador,referenciaempresa, " +
                                    " e.codigopostal as lugarexpedicion, e.regimenfiscal " +
                                    " from CFDFacturaImpresaMilenio I ,CFDFacturaImpresaMilenioExtra Ex, Vendedor v ,EmpresaCFD e" +
                                    " where(i.Empresa = e.Clave And i.Vendedor *= v.clave)  and I.Folio *=Ex.folio and I.Empresa *= Ex.Empresa and i.Operacion *= Ex.Operacion  " +
                                    " and  i.Empresa = '" + DVFacturas[n]["Empresa"].ToString() + "' AND i.Folio = '" + DVFacturas[n]["FolioParnet"].ToString() + "' and i.operacion = 'FACTURA'");
                                    //'Store P5GeneraDatosParaXmlFacturaV33
                                    dv = new DataView();
                                    dv = r.GetDataView("exec P5GeneraDatosParaXmlFacturaV33_T2021  " + DVFacturas[n]["idDocto"].ToString());


                                    if (dv.Count > 0) {
                                        Registralog("SEDETECTOFACTURA", pFOLIO, Operacion, "LLAMADO AL PROCESO GenerXMLEnvia.NET");
                                        //'CREAR EL XML Y HACER LA SOLICITUD DE TIMBRADO
                                        if (Operacion != "FACTURA")
                                        {
                                            System.Threading.Thread.Sleep(500);
                                            GC.Collect();
                                        }

                                        //Err.Clear();
                                        int idDocto = 0;
                                        Int32.TryParse(DVFacturas[n]["idDocto"].ToString(), out idDocto);
                                        GenerXMLEnvia(dv, dvadenda, idDocto, Operacion);

                                        //if (Err.Description != "") {
                                        //    GC.Collect();
                                        //    UsaLog = true;
                                        //    EtiquetaSQL.Text = Err.Description;
                                        //    Registralog("ENVIOGENERA", pFOLIO, Operacion, Err.Description + ":" + Err.Number);
                                        //    UsaLog = r.GetP5sistema("CFDLOG", "GRAL", "MMX") == "S" ? true : false;

                                        //}
                                    }
                                    else
                                        Registralog("NOPROCESADO", pFOLIO, Operacion, "PROCEDIMIENTO P5GeneraDatosParaXmlFacturaV5  ARROJO UN VALOR NO DESEADO");
                                }
                                else
                                    Registralog("NOPROCESADO", pFOLIO, Operacion, "PROCEDIMIENTO CFDdatosFactura ARROJO UN VALOR NO DESEADO");
                            }
                            else
                            {
                                //'CANCELACION DE FACTURA ESTA PENDIENTE PARA LA NUEVA VERSION
                                if (DVFacturas[n]["Estatus"].ToString().ToUpper().Trim() == "B")
                                {
                                    Registralog("CANCELAFACTURA33", pFOLIO, Operacion, "SE IDENTIFICO QUE DEBE CANCELARSE");
                                    dv = r.GetDataView("exec P5GeneraDatosParaXmlFacturaCancelaV5  " + IDunico);
                                    Registralog("CANCELAFACTURA33", pFOLIO, Operacion, "SE LLAMO AL PROCESO P5GeneraDatosParaXmlFacturaCancelaV5");
                                    int idDocto = 0;
                                    Int32.TryParse(DVFacturas[n]["idDocto"].ToString(), out idDocto);
                                    GenerXMLEnviaCancela(dv, idDocto);
                                }
                            }
                        }
                        if (Operacion == "CNCRED") {
                            Etiqueta.Text = "Procesando nota de crédito: " + pFOLIO;
                            this.Refresh();
                            if (DVFacturas[n]["Estatus"].ToString().ToUpper().Trim() == "S")
                            {
                                r.exeQry("exec CFDdatosCNCREDPrevio " + DVFacturas[n]["idDocto"].ToString());
                                dvadenda = new DataView();
                                dvadenda = r.GetDataView("exec CFDdatosCNCRED " + DVFacturas[n]["idDocto"].ToString());
                                if (dvadenda[0]["RetVal"].ToString() == "0")
                                {
                                    dvadenda = new DataView();
                                    dvadenda = r.GetDataView("select I.*, isnull(V.Nombre,'') AgenteVEntas , E.Telefono, e.Fax, " +
                                       " email = (dbo.fnClienteEmail(I.ClaveCliente, i.Empresa,'') ), isnull(Ex.observacion1,'') Mensaje1,isnull(Ex.observacion2,'') Mensaje2,isnull(Ex.observacion3,'') Mensaje3,  '----' as comprador,referenciaempresa   " +
                                       " from CFDFacturaImpresaMilenio I ,CFDFacturaImpresaMilenioExtra Ex, Vendedor v ,EmpresaCFD e " +
                                       " where(i.Empresa = e.Clave And i.Vendedor *= v.clave) and I.Folio *=Ex.folio and I.Empresa *= Ex.Empresa and i.Operacion *= Ex.Operacion " +
                                       " and  i.Empresa = '" + DVFacturas[n]["Empresa"].ToString() + "' AND i.Folio = '" + DVFacturas[n]["FolioParnet"].ToString() + "' and i.operacion = 'CNCRED'");
                                    dv = new DataView();
                                    dv = r.GetDataView("exec [P5GeneraDatosParaXmlCXCNCredV33_T2021]  " + DVFacturas[n]["idDocto"].ToString());
                                    if (dv.Count > 0)
                                    {
                                        Registralog("SEDETECTONCRED", pFOLIO, Operacion, "SE ENVIARA A PROCESO");
                                        int idDocto = 0;
                                        Int32.TryParse(DVFacturas[n]["idDocto"].ToString(), out idDocto);
                                        GenerXMLEnvia(dv, dvadenda, idDocto, Operacion);
                                        GC.Collect();
                                    }
                                    else
                                        Registralog("NOTACREDITOFALLO", pFOLIO, Operacion, "EL PROCEDIMIENTO NO REGRESO RESULTADOS P5GeneraDatosParaXmlCXCNCredV33_T2021");
                                }
                                else
                                {
                                    r.exeQry("Update cfdFActura set Estatus ='X' where id = " + IDunico);
                                    RegistraError(IDunico, "No se pudieron generar los detales", "PrevioEnvio");
                                    Registralog("NOTACREDITOFALLO", pFOLIO, Operacion, "EL PROCEDIMIENTO NO REGRESO RESULTADOS CFDdatosCNCRED");

                                }

                                //'**************************************************
                                //'dvadenda = r.GetDataView("exec CFDdatosCNCRED " + DVFacturas[n]["idDocto").ToString())
                                //'dv = r.GetDataView("exec P5GeneraDatosParaXmlCXCNCredV5  " + DVFacturas[n]["idDocto").ToString())
                                //'ifNot (dv Is res) Then GenerXMLEnvia(dv, dvadenda, DVFacturas[n]["idDocto").ToString(), Operacion)
                            }
                            else {
                                //'PENDIENTE PARA CANCELACION DE CFDI VERSION 3.3
                                if (DVFacturas[n]["Estatus"].ToString().ToUpper().Trim() == "B")
                                {
                                    //'CANCELACION DE NOTA DE CREDITO
                                    dv = r.GetDataView("exec P5GeneraDatosParaXmlFacturaCancelaV5 " + IDunico);
                                    int idDocto = 0;
                                    Int32.TryParse(DVFacturas[n]["idDocto"].ToString(), out idDocto);

                                    GenerXMLEnviaCancela(dv, idDocto);
                                }
                            }
                        }
                        //GC.Collect();
                    }
                }
                //'Catch ex As Exception


                //'    r.exeQry("CFDRegErrorGeneral  '" + IDunico + " Folio: " + pFOLIO.ToString + ":" + Mid(ex.Message.ToString, 1, 6000) + "'")

                //'End Try

                // Proceso obsoleto
                //if (STRSolicitaPDF == "S") {
                //    SolicitaPDFPendientes();
                //    GC.Collect();
                //}


                //' se separo el codigo y se hizo otro fuenta para las addendas
                //' EnviaAddendas()

            }
            catch (Exception ex)
            {
                GC.Collect();
                UsaLog = true;
                EtiquetaSQL.Text = ex.Message.ToString();
                Registralog("ENVIOGENERA", pFOLIO, Operacion, ex.Message.ToString());
                UsaLog = r.GetP5sistema("CFDLOG", "GRAL", "MMX") == "S" ? true : false;

                

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
                Timer2.Enabled = true;
                Timer1.Enabled = true;
                Button1.Enabled = true;
                Procesando = false;
                Etiqueta.Text = "Esperando";
            }
            this.Refresh();
        }
        //Consulta estatus de Facturas con cancelación // Carlos Loera 1O De Enero 2021
        private void ConsultaEstatusFacturasSAT()
        {
            int i = 0;
            string idDocto ;
            try
            {

                DataView DVFacturasPendCancelar = new DataView();
                Button1.Enabled = true;
                Etiqueta.Text = "";
                EtiquetaSQL.Text = "";
                Button1.Enabled = false;
                Registralog("CANCELACIONSAT", "GENERAL", "GENERAL", "EMPIEZA PROCESO CONSULTA DE DOCUMENTOS CANCELADOS..");
                Etiqueta.Text = "Buscando Facturas Pend Cancelar";
                if (Procesando)
                {
                    Timer1.Enabled = false;
                }


                //'********* LINEA DE PRODUCCION *************************************
                DVFacturasPendCancelar = r.GetDataView("select id,IdDocto,Operacion,Documento,Empresa,FolioParnet,Estatus,EstatusDoc,FechaCaptura,FechaCancelaDocto from cfdencabezadocancela where Estatus='B'; ");
                if (DVFacturasPendCancelar.Count > 0)
                {
                    Etiqueta.Text = "Facturas Pendientes de Cancelar: " + DVFacturasPendCancelar.Count.ToString();
                    i = DVFacturasPendCancelar.Count;
                    for (int n = 0; n < i; n++)
                    {
                        pFOLIO = DVFacturasPendCancelar[n]["FolioParnet"].ToString().ToUpper().Trim();
                        idDocto = DVFacturasPendCancelar[n]["idDocto"].ToString().ToUpper().Trim();
                        Etiqueta.Text = "Consultando en SAT Estatus de: "+ pFOLIO;
                        //Llamamos al metodo que consulta el estatus:

                        ConsultaCancelacionSATFactura(idDocto);

                    }


                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error de programa:" + e.Message.ToString());
            }

        }
        //Cancelación de facturas SAT. // Carlos Loera 1O De Enero 2021==================================**********************
        private void CancelaFacturasSAt()
        {
            EscribeBitcora("====Buscando Facturas Pendientes de Cancelar======");

            int i = 0;
            string idDocto;
            try
            {

                DataView DVFacturasPendCancelar = new DataView();
                Button1.Enabled = true;
                Etiqueta.Text = "";
                EtiquetaSQL.Text = "";
                Button1.Enabled = false;
                Registralog("CANCELACIONSAT", "GENERAL", "GENERAL", "EMPIEZA PROCESO CONSULTA DE DOCUMENTOS CANCELADOS..");
                Etiqueta.Text = "Buscando Facturas Pend Cancelar";
                if (Procesando)
                {
                    Timer1.Enabled = false;
                }


                //'********* LINEA DE PRODUCCION *************************************
                DVFacturasPendCancelar = r.GetDataView("select t1.id,t1.IdDocto,t1.Operacion,t1.Documento,t1.Empresa,t1.FolioParnet,t1.Estatus,t1.EstatusDoc,t1.FechaCaptura,FechaCancelaDocto from cfdencabezadocancela t1 inner join facturaencabezado t2  on t1.FolioParnet = t2.Folio and t1.Empresa = t2.Empresa inner join SAT_MotivoCancelacion t3 on t2.MotivoCancelacion = t3.idmotivo where t1.Estatus = 'I'; ");
                if (DVFacturasPendCancelar.Count > 0)
                {
                    Etiqueta.Text = "Solicitud de facturas Pendientes de Cancelar: " + DVFacturasPendCancelar.Count.ToString();
                    i = DVFacturasPendCancelar.Count;
                    for (int n = 0; n < i; n++)
                    {
                        pFOLIO = DVFacturasPendCancelar[n]["FolioParnet"].ToString().ToUpper().Trim();
                        idDocto = DVFacturasPendCancelar[n]["idDocto"].ToString().ToUpper().Trim();
                        EscribeBitcora("Intentando cancelar Factura : " + pFOLIO);
                        Etiqueta.Text = "Intentando cancelar Factura : " + pFOLIO;
                        //Llamamos al metodo que consulta el estatus:
                        EnvioCancelacionFacturaSAT(idDocto);
                        //ConsultaCancelacionSATFactura(idDocto);

                    }


                }
                else
                {
                    EscribeBitcora("==No se encontrarón facturas Pendientes de Cancelar==") ;
                    Etiqueta.Text = "No se encontrarón facturas Pendientes de Cancelar: ";
                }
            }
            catch (Exception e)
            {
                EscribeBitcora("Error al buscar facturas pendientes de cancelar" + e.Message.ToString());
                Etiqueta.Text = "Error al buscar facturas pendientes de cancelar:" + e.Message.ToString();
            }
        }
        //Cancelacion de Facturas SAT /**************************************************************************************

            //Envia al SAT la factura para ser cancelada // 10 de Enero 2021
            private void EnvioCancelacionFacturaSAT( string IdDocto)
        {
            string uuid = "";
            string empresa, rfc_emisor, rfc_receptor, total_cfdi, id, certificatenumber, idtoken, motive, replacement_folio,  token = "" ;
            string operacion = "Factura";

            DataView DVDatosFactura = new DataView();
            DataView DVInsertarBitacora = new DataView();
            DVDatosFactura = r.GetDataView("exec [dbo].[uspGeneraDatosConsultaEstatusCancelacion] @IDDocto = '" + IdDocto + "'");
            empresa = DVDatosFactura[0]["empresa"].ToString();
            uuid = DVDatosFactura[0]["uuid"].ToString();
            rfc_emisor = DVDatosFactura[0]["rfc"].ToString();
            rfc_receptor = DVDatosFactura[0]["rfc_receptor"].ToString();
            total_cfdi = DVDatosFactura[0]["total_cfdi"].ToString();
            idtoken = DVDatosFactura[0]["idtoken"].ToString();
            token = DVDatosFactura[0]["token"].ToString();
            motive = DVDatosFactura[0]["motive"].ToString();
            replacement_folio = DVDatosFactura[0]["replace_folio"].ToString();


            certificatenumber   = DVDatosFactura[0]["certificatenumber"].ToString();
            //llamado del servicio.
            //'Adaptacion código web service para cfdi 3.3
            //MemoryStream MemStream = FileToMemory(CFDiRutaRequest + pFOLIO + ".XML");
            //Byte[] myByteArray = MemStream.ToArray();



            CFDiURL = r.GetP5sistema("cfdiURLCancela", "gen", empresa);
            //CFDiURL = CFDiURL.Replace("uuid", uuid);
            CFDiURL = CFDiURL  + uuid +"/cancel";
            //object request = TryCast(System.Net.WebRequest.Create(CFDiURL), System.Net.HttpWebRequest)
            WebRequest request = HttpWebRequest.Create(CFDiURL);
            request.Method = "PUT";
            request.Timeout = 3600000;


            string json;
            string responseFromServer = "";
            if (replacement_folio == "") //Si no tiene UUID de reemplazo
            {
                json = "{\"credentials\": {\"id\": \"" + idtoken + "\",\"token\": \"" + token + "\"},\"issuer\": {\"rfc\": \"" + rfc_emisor + "\"},\"document\": {\"certificate-number\": \"" + certificatenumber + "\",\"rfc_receptor\": \"" + rfc_receptor + "\",\"total_cfdi\": \"" + total_cfdi + "\",\"motive\": \"" + motive + "\" }}";
            }
            else // si  tiene UUID de Reemplazo
            {
                json = "{\"credentials\": {\"id\": \"" + idtoken + "\",\"token\": \"" + token + "\"},\"issuer\": {\"rfc\": \"" + rfc_emisor + "\"},\"document\": {\"certificate-number\": \"" + certificatenumber + "\",\"rfc_receptor\": \"" + rfc_receptor + "\",\"total_cfdi\": \"" + total_cfdi + "\",\"motive\": \"" + motive + "\",\"replacement-folio\": \"" + replacement_folio + "\" }}";
            }
        

                //'****** archivo con el json
                string path1 = CFDiRutaRequest + pFOLIO + ".json";
            FileStream fs1 = File.Create(path1);
            Byte[] info1 = new UTF8Encoding(true).GetBytes(json);
            fs1.Write(info1, 0, info1.Length);
            fs1.Close();
            //'**** fin archivo con json

            Byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentType = "application/json; charset=utf-8";
            //'request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            try
            {
                //   Dim responseX = TryCast(request.GetResponse(), System.Net.HttpWebResponse)
                HttpWebResponse responseX = (HttpWebResponse)request.GetResponse();
                dataStream = responseX.GetResponseStream();
                //' Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                //' Read the content.  
                responseFromServer = reader.ReadToEnd();
                //' Display the content.  
                //' Clean up the streams.  
                reader.Close();
                dataStream.Close();
                responseX.Close();
            }
            catch (WebException ex)
            {
                //MessageBox.Show("Error al intentar cancelar la factura " + ex.ToString());
                EscribeBitcora("Error al intentar cancelar factura :" + pFOLIO + " : " + ex.ToString());
                Etiqueta.Text = "Error al intentar cancelar factura :" + pFOLIO + " : " + ex.ToString();
                //update cfdEncabezadoCancela set Estatus = 'B' where IdDocto =
                r.GetDataView("update cfdEncabezadoCancela set Estatus = 'X' where IdDocto =" + IdDocto);

            }

            try
            {

           
            //Si no existió Error deserializamos la respuesta============CONTESTACION DE DIVERZA
            Etiqueta.Text = "Cancelación aceptada del folio:" + pFOLIO;
            System.Web.Script.Serialization.JavaScriptSerializer serializador = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> diccionario = ((Dictionary<string, object>)(serializador.DeserializeObject(responseFromServer)));//responseFromServer//ErrorResponse
            string Respuesta = diccionario["status"].ToString();
            string FechaSolicitud = diccionario["date"].ToString();
            string validacion_efos = "";
            string estatus_cancelacion = diccionario["status"].ToString();
            string respuestaBitacora = "Json: CFDI en proceso de cancelación";
            //string estatus_cancelacion = diccionario["estatus_cancelacion"].ToString();

            if (Respuesta== "pending")
            {
                //update cfdEncabezadoCancela set Estatus = 'B' where IdDocto =
                r.GetDataView("update cfdEncabezadoCancela set Estatus = 'B' where IdDocto =" + IdDocto);
                //Insertamos en la bitacora el resultado de la consulta
                DVInsertarBitacora = r.GetDataView("exec [dbo].[uspBitacoraCancelacion] '" + IdDocto + "', '" + empresa + "', '" + operacion + "', '" + pFOLIO + "',' " + validacion_efos + "', '" + respuestaBitacora + "'");
            }
            else
            {
                DVInsertarBitacora = r.GetDataView("exec [dbo].[uspBitacoraCancelacion] '" + IdDocto + "', '" + empresa + "', '" + operacion + "', '" + pFOLIO + "',' " + validacion_efos + "', '" + Respuesta + "'");
            }


            Etiqueta.Text = "Estatus de la cancelación:" + pFOLIO + "Es: " + Respuesta;
            Etiqueta.Text = "Insertado en bitacora:" + pFOLIO + "Es: " + Respuesta;
                        EscribeBitcora("Estatus de la cancelación:" + pFOLIO + "Es: " + Respuesta);
                        EscribeBitcora("Insertado en bitacora:" + pFOLIO + "Es: " + Respuesta);
            }
            catch (Exception e)
            {
                EscribeBitcora("Error al intentar captura el Mensaje de diverza en la funcion EnvioCancelacionFacturaSAT: " + e.Message.ToString());
            }


        }

        //=============CANCELACIONES
        //Cancelación de Pagos SAT. // Carlos Loera 17 d De Enero 2021==================================**********************
        private void CancelaPagosSAT()
        {
            EscribeBitcora("====Buscando Pagos Pendientes======");

            int i = 0;
            string idDocto;
            try
            {

                DataView DVpagosPendCancelar = new DataView();
                Button1.Enabled = true;
                Etiqueta.Text = "";
                EtiquetaSQL.Text = "";
                Button1.Enabled = false;
                Registralog("CANCELACIONSAT", "GENERAL", "GENERAL", "EMPIEZA PROCESO CONSULTA DE DOCUMENTOS CANCELADOS..");
                Etiqueta.Text = "Buscando pagos Pendientes de cancelar";
                if (Procesando)
                {
                    Timer1.Enabled = false;
                }


                //'********* LINEA DE PRODUCCION *************************************
                DVpagosPendCancelar = r.GetDataView("EXECUTE uspGetcobrosPendientesCancelar; ");
                if (DVpagosPendCancelar.Count > 0)
                {
                    EscribeBitcora("Se encontraron pagos pendientee de cancelar : " + DVpagosPendCancelar.Count.ToString());
                    Etiqueta.Text = "Se encontraron pagos pendientee de cancelar : " + DVpagosPendCancelar.Count.ToString();
                    i = DVpagosPendCancelar.Count;
                    for (int n = 0; n < i; n++)
                    {
                        pFOLIO = DVpagosPendCancelar[n]["Folio"].ToString().ToUpper().Trim();
                        idDocto = DVpagosPendCancelar[n]["id"].ToString().ToUpper().Trim();
                        EscribeBitcora("Intentando cancelar Pago Folio : " + pFOLIO);
                        Etiqueta.Text = "Intentando cancelar Pago Folio : " + pFOLIO;
                        //Llamamos al metodo que consulta el estatus:
                        //EnvioCancelacionFacturaSAT(idDocto);
                        //ConsultaCancelacionSATFactura(idDocto);
                        EnvioCancelacionPagosSAT(idDocto);
                    }


                }
                else
                {
                    EscribeBitcora("==No se encontrarón Pagos Pendientes de Cancelar==");
                    Etiqueta.Text = "No se encontrarón Pagos Pendientes de Cancelar: ";
                }
            }
            catch (Exception e)
            {
                EscribeBitcora("Error al buscar Pagos pendientes de cancelar" + e.Message.ToString());
                Etiqueta.Text = "Error al buscar Pagos pendientes de cancelar:" + e.Message.ToString();
            }
        }
        //Cancelacion de Facturas SAT /**************************************************************************************

        //Envia al SAT los Pagos para ser cancelada // 17de Enero 2021
        private void EnvioCancelacionPagosSAT(string IdDocto)
        {
            string uuid = "";
            string empresa, rfc_emisor, rfc_receptor, total_cfdi, id, certificatenumber, idtoken, motive, replacement_folio, token = "";
            string operacion = "Factura";

            DataView DVDatosPagos = new DataView();
            DataView DVInsertarBitacora = new DataView();
            DVDatosPagos = r.GetDataView("exec [dbo].[uspGeneraDatosConsultaEstatusCancelacionPagos] @IDDocto = '" + IdDocto + "'");
            empresa = DVDatosPagos[0]["empresa"].ToString();
            uuid = DVDatosPagos[0]["uuid"].ToString();
            rfc_emisor = DVDatosPagos[0]["rfc"].ToString();
            rfc_receptor = DVDatosPagos[0]["rfc_receptor"].ToString();
            total_cfdi = DVDatosPagos[0]["total_cfdi"].ToString();
            idtoken = DVDatosPagos[0]["idtoken"].ToString();
            token = DVDatosPagos[0]["token"].ToString();
            motive = DVDatosPagos[0]["motive"].ToString();
            replacement_folio = DVDatosPagos[0]["replace_folio"].ToString();


            certificatenumber = DVDatosPagos[0]["certificatenumber"].ToString();
            //llamado del servicio.
            //'Adaptacion código web service para cfdi 3.3
            //MemoryStream MemStream = FileToMemory(CFDiRutaRequest + pFOLIO + ".XML");
            //Byte[] myByteArray = MemStream.ToArray();



            CFDiURL = r.GetP5sistema("cfdiURLCancela", "gen", empresa);
            //CFDiURL = CFDiURL.Replace("uuid", uuid);
            CFDiURL = CFDiURL + uuid + "/cancel";
            //object request = TryCast(System.Net.WebRequest.Create(CFDiURL), System.Net.HttpWebRequest)
            WebRequest request = HttpWebRequest.Create(CFDiURL);
            request.Method = "PUT";
            request.Timeout = 3600000;


            string json;
            string responseFromServer = "";
            if (replacement_folio == "") //Si no tiene UUID de reemplazo
            {
                json = "{\"credentials\": {\"id\": \"" + idtoken + "\",\"token\": \"" + token + "\"},\"issuer\": {\"rfc\": \"" + rfc_emisor + "\"},\"document\": {\"certificate-number\": \"" + certificatenumber + "\",\"rfc_receptor\": \"" + rfc_receptor + "\",\"total_cfdi\": \"" + total_cfdi + "\",\"motive\": \"" + motive + "\" }}";
            }
            else // si  tiene UUID de Reemplazo
            {
                json = "{\"credentials\": {\"id\": \"" + idtoken + "\",\"token\": \"" + token + "\"},\"issuer\": {\"rfc\": \"" + rfc_emisor + "\"},\"document\": {\"certificate-number\": \"" + certificatenumber + "\",\"rfc_receptor\": \"" + rfc_receptor + "\",\"total_cfdi\": \"" + total_cfdi + "\",\"motive\": \"" + motive + "\",\"replacement-folio\": \"" + replacement_folio + "\" }}";
            }


            //'****** archivo con el json
            string path1 = CFDiRutaRequest + "Cancelapago_" + pFOLIO + ".json";
            FileStream fs1 = File.Create(path1);
            Byte[] info1 = new UTF8Encoding(true).GetBytes(json);
            fs1.Write(info1, 0, info1.Length);
            fs1.Close();
            //'**** fin archivo con json

            Byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentType = "application/json; charset=utf-8";
            //'request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            try
            {
                //   Dim responseX = TryCast(request.GetResponse(), System.Net.HttpWebResponse)
                HttpWebResponse responseX = (HttpWebResponse)request.GetResponse();
                dataStream = responseX.GetResponseStream();
                //' Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                //' Read the content.  
                responseFromServer = reader.ReadToEnd();
                //' Display the content.  
                //' Clean up the streams.  
                reader.Close();
                dataStream.Close();
                responseX.Close();
            }
            catch (WebException ex)
            {

                //Registra el error en un fichero

                if (ex.Message.IndexOf("Error en el servidor remoto: (500) Error interno del servidor.") != -1)
                    ErrorResponse = "Error en el servidor remoto: (500) Error interno del servidor.";
                else
                    if (ex.Message.IndexOf("Se termin") != -1)
                    ErrorResponse = "SE TERMINO EL TIEMPO DE ESPERA";
                else
                {
                    using (var response = (HttpWebResponse)ex.Response)
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                            {
                                ErrorResponse = reader.ReadToEnd();
                            }
                        }
                    }
                }
                //using (Stream responseStream = ex.Response.GetResponseStream())
                //{
                //    using (StreamReader responseReader = new StreamReader(responseStream))
                //    {
                //        ErrorResponse = responseReader.ReadToEnd();
                //    }
                //}
                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Bitacoracancelacionescobros" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();

                //MessageBox.Show("Error al intentar cancelar la factura " + ex.ToString());
                EscribeBitcora("Error al intentar cancelar el cobro :" + pFOLIO + " : " + ex.ToString());
                Etiqueta.Text = "Error al intentar cancelar el cobro  :" + pFOLIO + " : " + ex.ToString();
                //update cfdEncabezadoCancela set Estatus = 'B' where IdDocto =
                r.GetDataView("update CFDICOMPLEMENTOPAGO set Estatus = 'X' where id =" + IdDocto);

            }

            try
            {

                //Si no existió Error deserializamos la respuesta============CONTESTACION DE DIVERZA
                Etiqueta.Text = "Cancelación aceptada del folio:" + pFOLIO;
                System.Web.Script.Serialization.JavaScriptSerializer serializador = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> diccionario = ((Dictionary<string, object>)(serializador.DeserializeObject(responseFromServer)));//responseFromServer//ErrorResponse
                string Respuesta = diccionario["status"].ToString();
                string FechaSolicitud = diccionario["date"].ToString();
                string validacion_efos = "";
                string estatus_cancelacion = diccionario["status"].ToString();
                string respuestaBitacora = "Json: CFDI en proceso de cancelación";
                //string estatus_cancelacion = diccionario["estatus_cancelacion"].ToString();

                if (Respuesta == "pending")
                {
                    //update cfdEncabezadoCancela set Estatus = 'B' where IdDocto =
                    r.GetDataView("update CFDICOMPLEMENTOPAGO set Estatus = 'C' where id =" + IdDocto);
                    //Insertamos en la bitacora el resultado de la consulta
                    //DVInsertarBitacora = r.GetDataView("exec [dbo].[uspBitacoraCancelacion] '" + IdDocto + "', '" + empresa + "', '" + operacion + "', '" + pFOLIO + "',' " + validacion_efos + "', '" + respuestaBitacora + "'");
                }
                else
                {
                    //DVInsertarBitacora = r.GetDataView("exec [dbo].[uspBitacoraCancelacion] '" + IdDocto + "', '" + empresa + "', '" + operacion + "', '" + pFOLIO + "',' " + validacion_efos + "', '" + Respuesta + "'");
                }


                Etiqueta.Text = "Estatus de la cancelación del cobro:" + pFOLIO + "Es: " + Respuesta;
                Etiqueta.Text = "Insertado en bitacora:" + pFOLIO + "Es: " + Respuesta;
                EscribeBitcora("Estatus de la cancelación del cobro::" + pFOLIO + "Es: " + Respuesta);
                EscribeBitcora("Insertado en bitacora:" + pFOLIO + "Es: " + Respuesta);
            }
            catch (Exception e)
            {
                EscribeBitcora("Error al intentar captura el Mensaje de diverza en la funcion EnvioCancelacionFacturaSAT: " + e.Message.ToString());
            }


        }









        private void RegistralogX(string aProceso, string aFolio, string aOperacion, string aDescripcion)
        {
            //' Dim dverror As New DataView
            aDescripcion = aDescripcion.Replace("'", "*");
            r.exeQry("Exec CFDGrabaLog '" + aProceso + "','" + aFolio + "','" + aOperacion + "','" + aDescripcion + "'");
            //'r.exeQry("CFDRegErrorGeneral  '" & iddocto.ToString & " folio: " & pFOLIO.ToString & ":" & Mid(ex.Message.ToString.Replace("'", ""), 1, 6000) & "'")
            //'RegistralogX("EnviaCancela ", pFOLIO, Operacion, "ERROR la procesar respuesta de cancelacion" & Err.Description)
        }

        private void GenerXMLEnviaCancela(DataView datos, int iddocto)
        {
            try
            {
                GC.Collect();
                idtoken = r.GetP5sistema("idtoken", "gen", Empresa);
                token = r.GetP5sistema("token", "gen", Empresa);
                CFDI33certificado = r.GetP5sistema("CFDI33Certificado", "gen", Empresa);
                CFDiURLCancela = r.GetP5sistema("CFDiURLCancela", "gen", Empresa);
                Registralog("CANCELAFACTURA33", pFOLIO, Operacion, "ENTRANDO AL PROCEDIMIENTO GenerXMLEnviaCancela.NET");
                Etiqueta.Text = "Cancelación de factura CFDI 33: " + pFOLIO;
                this.Refresh();

                if (datos.Count > 0) {
                    if (datos[0]["uuid"].ToString().Trim() != "")
                    {
                        Registralog("CANCELAFACTURA", pFOLIO, Operacion, "SE INVOCA WS DE CANCELACION --UUID:" + datos[0]["uuid"].ToString() + " RFC:" + RFCemisor);

                        //On Error GoTo 0
                        CancelaTimbraDiverza33(iddocto, datos);
                        //On Error GoTo erroresCan
                        ////'la factura no se alcanzo a enviar solo se marca
                    }
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                GC.Collect();
                RegistralogX("EnviaCancela ", pFOLIO, Operacion, "ERROR AL procesar respuesta de cancelacion: " + ex.Message);

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
        public bool CancelaTimbraDiverza33(int iddocto, DataView vdatos)
        {
            string Content64;
            string url;
            string vRFC;
            string responseFromServer;
            string json;

            vRFC = vdatos[0]["RFCEmisor"].ToString().Trim();
            url = CFDiURLCancela + vdatos[0]["uuid"].ToString() + "/cancel";

            WebRequest request = HttpWebRequest.Create(url);
            //' Set the Method property of the request to POST.  
            request.Method = "PUT";
            request.Timeout = 60000;


            //        ' Create POST data and convert it to a byte array.  
            json = "{\"credentials\": {\"id\": \"" + idtoken + "\",\"token\": \"" + token + "\"},\"issuer\": {\"rfc\": \"" + vRFC + "\"},\"document\": {\"certificate-number\": \"" + CFDI33certificado + "\"}}";

            Byte[] byteArray = Encoding.UTF8.GetBytes(json);
            request.ContentType = "application/json";

            //' Set the ContentLength property of the WebRequest.  
            request.ContentLength = byteArray.Length;
            //' Get the request stream.  


            Stream dataStream = request.GetRequestStream();
            //' Write the data to the request stream.  
            dataStream.Write(byteArray, 0, byteArray.Length);
            //' Close the Stream object.  
            dataStream.Close();

            try
            {

                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();

                StreamReader oreader = new StreamReader(dataStream);
                responseFromServer = oreader.ReadToEnd();
                oreader.Close();
                dataStream.Close();
                response.Close();

            }
            catch (WebException ex)
            {

                if (ex.Message.IndexOf("Error en el servidor remoto: (500) Error interno del servidor.") != -1)
                {
                    ErrorResponse = "Error en el servidor remoto: (500) Error interno del servidor.";
                }
                else
                {
                    if (ex.Message.IndexOf("Se termin") != -1 || ex.Message.IndexOf("Se excedi") != -1)
                    {
                        ErrorResponse = "SE TERMINO EL TIEMPO DE ESPERA";
                    }
                    else
                    {
                        using (var response = (HttpWebResponse)ex.Response)
                        {
                            using (var stream = response.GetResponseStream())
                            {
                                using (var reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                                {
                                    ErrorResponse = reader.ReadToEnd();
                                }
                            }
                        }
                        //using (Stream responseStream = ex.Response.GetResponseStream())
                        //{
                        //    using (Stream responseReader = new StreamReader(responseStream))
                        //    {
                        //        ErrorResponse = responseReader.ReadToEnd();
                        //    }
                        //}
                    }
                }

                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();
            }

            if (ErrorResponse != "")
            { // 'Si hubo error
                if (ErrorResponse == "SE TERMINO EL TIEMPO DE ESPERA" || ErrorResponse == "Error en el servidor remoto: (500) Error interno del servidor.")
                {
                    RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "SE TERMINO EL TIEMPO DE ESPERA");
                    RegistraError(IDunico, "SE TERMINO EL TIEMPO DE ESPERA", "GenerXMLEnvia");
                }
                else
                {
                    Etiqueta.Text = "Error de codigo CFDi: " + pFOLIO + " json : " + json;
                    this.Refresh();
                    if (TiempoEspera > 0)
                    {
                        System.Threading.Thread.Sleep(TiempoEspera + 50);
                    }

                    //string json = @"{""key1"":""value1"",""key2"":""value2""}";

                    //var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                    //Dictionary<string, Object> diccionario = CType(serializador.DeserializeObject(ErrorResponse), Dictionary<string, Object>);
                    System.Web.Script.Serialization.JavaScriptSerializer serializador = new System.Web.Script.Serialization.JavaScriptSerializer();
                    Dictionary<string, object> diccionario = ((Dictionary<string, object>)(serializador.DeserializeObject(ErrorResponse)));
                    string jsonError;
                    jsonError = diccionario["message"].ToString() + " detalles : " + diccionario["error_details"].ToString();
                    RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, jsonError);
                    RegistraError(IDunico, jsonError, "GenerXMLEnvia");
                    //GoTo Salir
                }
            }
            else
            {
                //'Exito  SE PUDO PROCESAR EL CFDi  
                Etiqueta.Text = "CANCELANDO Documento CFDI 3.3 con éxito:" + pFOLIO;
                this.Refresh();
                Registralog("GenerXMLEnvia", pFOLIO, Operacion, "ARCHIVO ENVIADO CON EXITO");
                RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "Se canceló el documento: " + DateTime.Now.ToString());
                System.Web.Script.Serialization.JavaScriptSerializer serializador = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> diccionario = ((Dictionary<string, object>)(serializador.DeserializeObject(ErrorResponse)));
                ProcesaRespuestaCancelar(diccionario["acknowledgement"].ToString(), iddocto, diccionario["date"].ToString());

                string sPathPDF = vdatos[0]["ArchivoPDF"].ToString().Trim();
                string sPathXML = vdatos[0]["ArchivoXML"].ToString().Trim();
                string sPathJPG = vdatos[0]["ArchivoXML"].ToString().Trim();



                string sNombreGeneral = vdatos[0]["FolioParnet"].ToString().Trim();

                string sNuevoPath = "";
                string SnuevoNombre;


                GrabaComprobanteCancelacion(sPathPDF);
                
                sPathJPG = sPathJPG.Replace(sNombreGeneral + ".XML", "b" + sNombreGeneral.Trim() + ".JPG");

                SnuevoNombre = sNombreGeneral.Trim() + "-Cancelado.PDF";
                sNuevoPath = sPathPDF.Replace(sNombreGeneral + ".PDF", SnuevoNombre);
                //' Renombrarlo con la función renameFile  
                if (sNuevoPath != "")
                {
                    //My.Computer.FileSystem.RenameFile(sPathPDF, SnuevoNombre);
                    //cambia el nombre generico por el real
                    File.Move(sPathPDF, sNuevoPath);
                    r.exeQry("Update cfdfactura set ArchivoPDF ='" + sNuevoPath + "'  where id =" + vdatos[0]["Id"].ToString());
                }

                SnuevoNombre = sNombreGeneral.Trim() + "-Cancelado.XML";
                sNuevoPath = sPathXML.Replace(sNombreGeneral + ".XML", SnuevoNombre);
                //' Renombrarlo con la función renameFile  
                if (sNuevoPath != "")
                {
                    //My.Computer.FileSystem.RenameFile(sPathXML, SnuevoNombre);
                    //cambia el nombre generico por el real
                    File.Move(sPathXML, sNuevoPath);
                    r.exeQry("Update cfdfactura set ArchivoXML ='" + sNuevoPath + "'  where id =" + vdatos[0]["Id"].ToString());
                }
                String fecha;
                fecha = DateTime.Today.ToString("yyyyMMdd hh:mm:ss"); //Format(Date.Today, "yyyyMMdd hh:mm:ss");
                r.exeQry("CFDRegistraCancelacion  '" + IDunico + "','" + fecha + "','0',''");
            }
            request = null;
            return true;
        }
        public void ProcesaRespuestaCancelar(string Respuesta, Int64 id, string vFecha)
        {
            try {
                DataView dvRespuesta = new DataView();
                string Mensaje;
                string Fecha;
                int Procede;
                Mensaje = "";
                Procede = 0;

                Registralog("PROCESANDORESPUESTACANC", pFOLIO, Operacion, "INICIA PROCESO DE RESPUESTA DE WS DE CANCELACION");
                Fecha = Convert.ToDateTime(vFecha).ToString("yyyyMMdd hh:mm:ss");
                //Procede = 0;

                Registralog("PROCESANDORESPUESTACANC", pFOLIO, Operacion, "WS RESPONDIO QUE SI SE PUDO CANCELAR");

                //'Else
                //'    'registrar error de cancelacion
                //'    Registralog(aProceso="PROCESANDORESPUESTACANC",pFOLIO, aOperacion:=Operacion,"WS RESPONDIO QUE NO SE PUDO,  NOOOO SE  CANCELO")
                //'    Fecha = Format(Date.Today, "yyyyMMdd hh:mm:ss")
                //'    Procede = -1
                //'    Mensaje = Respuesta.Result.Message(0).message.ToString
                //'    RegistraError(IDunico, Mensaje, "Cancelacion")

                //'    Dim Z As New Xml.Serialization.XmlSerializer(Respuesta.GetType)
                //'    Dim writer As New StreamWriter(RutaXMLGraba + "c:\R_" + pFOLIO + ".XML")
                //'    Z.Serialize(writer, Respuesta)

                //'    Exit Sub
                //'End If
                //Registralog(aProceso= "PROCESANDORESPUESTACANC", pFOLIO, Operacion, "SE CORRE CFDRegistraCancelacion PARA ACTUALIZAR REGISTROS- FIN DE CANCELACION")

                r.exeQry("CFDRegistraCancelacion  '" + IDunico + "','" + Fecha + "','" + Procede + "','" + Mensaje + "'");

                //' Catch ex As Exception
                //Exit Sub
            }
            catch (Exception ex)
            {
                RegistralogX("ProcesaResCan ", pFOLIO, Operacion, "ERROR la procesar respuesta de cancelacion" + ex.ToString());

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

        public bool TimbraDiverza33(int iddocto, bool complemento)
        {
            //'Adaptacion código web service para cfdi 3.3
            //MemoryStream MemStream = FileToMemory(CFDiRutaRequest + pFOLIO + ".XML");
            //Byte[] myByteArray = MemStream.ToArray();
            Byte[] myByteArray = FileToMemory(CFDiRutaRequest + pFOLIO + ".XML");
            string Content64;
            Content64 = System.Convert.ToBase64String(myByteArray, 0, myByteArray.Length);

            CFDiURL = r.GetP5sistema("CFDiURL", "gen", Empresa);
            //object request = TryCast(System.Net.WebRequest.Create(CFDiURL), System.Net.HttpWebRequest)
            WebRequest request = HttpWebRequest.Create(CFDiURL);
            request.Method = "POST";
            request.Timeout = 3600000;


            string json;
            string responseFromServer = "" ;
            json = "{\"credentials\": {\"id\": \"" + idtoken + "\",\"token\": \"" + token + "\"},\"issuer\": {\"rfc\": \"" + RFCemisor + "\"},\"document\": {\"ref-id\": \"" + IDunico + "\",\"certificate-number\": \"" + CFDI33certificado + "\",\"section\": \"all\",\"format\": \"xml\",\"template\": \"letter\",\"type\": \"application/vnd.diverza.cfdi_3.3+xml\",\"content\": \"" + Content64 + "\"}}";

            if (complemento)
                json = "{\"credentials\": {\"id\": \"" + idtoken + "\",\"token\": \"" + token + "\"},\"issuer\": {\"rfc\": \"" + RFCemisor + "\"},\"document\": {\"ref-id\": \"" + IDunico + "\",\"certificate-number\": \"" + CFDI33certificado + "\",\"section\": \"all\",\"format\": \"xml\", \"template\": \"letter\",\"type\": \"application/vnd.diverza.cfdi_3.3_complemento+xml\",\"content\": \"" + Content64 + "\"}}";


            //'****** archivo con el json
            string path1 = CFDiRutaRequest + pFOLIO + ".json";
            FileStream fs1 = File.Create(path1);
            Byte[] info1 = new UTF8Encoding(true).GetBytes(json);
            fs1.Write(info1, 0, info1.Length);
            fs1.Close();
            //'**** fin archivo con json

            Byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentType = "application/json; charset=utf-8";
            //'request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            try
            {
                //   Dim responseX = TryCast(request.GetResponse(), System.Net.HttpWebResponse)
                HttpWebResponse responseX = (HttpWebResponse)request.GetResponse();
                dataStream = responseX.GetResponseStream();
                //' Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                //' Read the content.  
                responseFromServer = reader.ReadToEnd();
                //' Display the content.  
                //' Clean up the streams.  
                reader.Close();
                dataStream.Close();
                responseX.Close();
            }
            catch (WebException ex)
            {

                if (ex.Message.IndexOf("Error en el servidor remoto: (500) Error interno del servidor.") != -1)
                    ErrorResponse = "Error en el servidor remoto: (500) Error interno del servidor.";
                else
                    if (ex.Message.IndexOf("Se termin") != -1)
                    ErrorResponse = "SE TERMINO EL TIEMPO DE ESPERA";
                else
                {
                    using (var response = (HttpWebResponse)ex.Response)
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                            {
                                ErrorResponse = reader.ReadToEnd();
                            }
                        }
                    }
                }
                //using (Stream responseStream = ex.Response.GetResponseStream())
                //{
                //    using (StreamReader responseReader = new StreamReader(responseStream))
                //    {
                //        ErrorResponse = responseReader.ReadToEnd();
                //    }
                //}
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
                if (ErrorResponse != "")
                {
                    string path8 = "c:\\xml33\\";
                    StreamWriter w7 = File.AppendText(path8 + "test" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                    //w.Write("\r\nLog Entry : ");
                    w7.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        DateTime.Now.ToLongDateString());
                    w7.WriteLine(IDunico + "   ---   " + ErrorResponse.ToString());
                    w7.WriteLine("-------------------------------");
                    w7.Close();
                    //'Si hubo error
                    if (ErrorResponse == "SE TERMINO EL TIEMPO DE ESPERA" || ErrorResponse == "Error en el servidor remoto: (500) Error interno del servidor.")
                    {
                        RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "SE TERMINO EL TIEMPO DE ESPERA");
                        RegistraError(IDunico, "SE TERMINO EL TIEMPO DE ESPERA", "GenerXMLEnvia");
                        //GoTo Salir
                    }
                    else
                    {
                        Etiqueta.Text = "Error de codigo CFDi: " + pFOLIO + " json : " + json;
                        if (TiempoEspera > 0)
                            System.Threading.Thread.Sleep(TiempoEspera + 50);

                        System.Web.Script.Serialization.JavaScriptSerializer serializador = new System.Web.Script.Serialization.JavaScriptSerializer();
                        Dictionary<string, object> diccionario = ((Dictionary<string, object>)(serializador.DeserializeObject(ErrorResponse)));
                        string jsonError;

                        foreach (KeyValuePair<string, object> pair in diccionario)
                        {
                            StreamWriter w8 = File.AppendText(path8 + "test" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                            //w.Write("\r\nLog Entry : ");
                            w8.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                                DateTime.Now.ToLongDateString());
                            w8.WriteLine(pair.Key + " - " + pair.Value.ToString());
                            w8.WriteLine("-------------------------------");
                            w8.Close();
                        }

                        jsonError = diccionario["message"].ToString() + " detalles : " + diccionario["error_details"].ToString();
                        RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, jsonError);
                        RegistraError(IDunico, jsonError, "GenerXMLEnvia");

                        StreamWriter w9 = File.AppendText(path8 + "test" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                        w9.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                            DateTime.Now.ToLongDateString());
                        w9.WriteLine("Termino el proceso del error de json");
                        w9.WriteLine("-------------------------------");
                        w9.Close();
                    }
                }
                else
                {
                    // 'Exito  SE PUDO PROCESAR EL CFDi  
                    Etiqueta.Text = "Recibiendo Documento CFDI 3.3 con éxito:" + pFOLIO;
                    this.Refresh();
                    Registralog("GenerXMLEnvia", pFOLIO, Operacion, "ARCHIVO ENVIADO CON EXITO");
                    RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "Se timbro el documento: " + DateTime.Now.ToString());

                    //System.Web.Script.Serialization.JavaScriptSerializer serializador = new System.Web.Script.Serialization.JavaScriptSerializer();
                    //Dictionary<string, Object> diccionario = CType(serializador.DeserializeObject(responseFromServer), Dictionary<string, Object>);

                    System.Web.Script.Serialization.JavaScriptSerializer serializador = new System.Web.Script.Serialization.JavaScriptSerializer();
                    Dictionary<string, object> diccionario = ((Dictionary<string, object>)(serializador.DeserializeObject(responseFromServer)));//responseFromServer//ErrorResponse

                    ProcesaRespuesta(diccionario["content"].ToString(), iddocto);
                }
            }
            return true;
        }
        private bool ProcesaRespuesta(string Respuesta, Int64 iddocto)
        {
            try
            {

                //Err.Clear()
                DataView dvProc = new DataView();
                DataView dvGraba = new DataView();
                DataView dvafecta = new DataView();

                DateTime lFecha = DateTime.Now;
                string lArchivo;
                string CadenaOriginal;
                string lCertificadoSat;
                string lnoCertificado;
                string luuid;

                if (Respuesta == "")
                    RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "El documento de respuesta esta vacio: Revisar en Buzon el estado de factura " + DateTime.Now.ToString());
                //Err.Clear()


                CadenaOriginal = "";

                lCertificadoSat = "";
                lnoCertificado = "";
                Registralog("ProcesaRespuesta", pFOLIO, Operacion, "ARCHIVO ENVIADO CON EXITO");

                string SelloSAt;

                lArchivo = pFOLIO;

                //' 1) Archivo Original de respuesta  (serializamos toda la clase -- Respuesta )
                System.Xml.Serialization.XmlSerializer Z = new System.Xml.Serialization.XmlSerializer(Respuesta.GetType());
                StreamWriter writer = new StreamWriter(CFDiRutaRequest + "\\R" + lArchivo + ".XML");
                Z.Serialize(writer, Respuesta);

                //If Err.Number <> 0 Then
                //    RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Generar (Serializar):" & RutaXMLGraba + "\R" + lArchivo + ".XML " & " :" & Err.Description)
                //End If

                //Err.Clear()

                string sPDF = "";
                Byte[] aFile;
                FileStream fs;
                FileStream fs2;
                //byte[] GraficoCB;
                aFile = Convert.FromBase64String(Respuesta);

                try
                {
                    //'2. El archivo que se le entrega al Cliente viene en el attributo "archivo" y se graba en binario, se trae a ruta local para extraer datos
                    if (aFile != null && aFile.Length > 0)
                    {
                        sPDF = CFDiRutaRequest + "\\" + lArchivo + ".XML";
                        fs = new FileStream(sPDF, FileMode.Create);
                        fs.Write(aFile, 0, aFile.Length);
                        fs.Close();
                    }
                    else
                    {
                        RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Generar versión 3.3:  ");
                    }
                }
                catch (Exception ex)
                {
                    RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Genera:" + sPDF + " :" + ex.ToString());

                    string path = "c:\\xml33\\";
                    StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                    //w.Write("\r\nLog Entry : ");
                    w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        DateTime.Now.ToLongDateString());
                    w.WriteLine("  {0}", ex.ToString());
                    w.WriteLine("-------------------------------");
                    w.Close();
                }
                //'EXTRACCION DEL SELLO SAT
                SelloSAt = "";
                luuid = "";
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                try
                {
                    using (XmlReader reader = XmlReader.Create(sPDF, settings))
                    {

                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element && "Comprobante" == reader.LocalName)
                            {
                                lCertificadoSat = reader.GetAttribute("Certificado").ToString();
                            }
                            if (reader.NodeType == XmlNodeType.Element && "TimbreFiscalDigital" == reader.LocalName)
                            {
                                luuid = reader.GetAttribute("UUID").ToString();
                                SelloSAt = reader.GetAttribute("SelloSAT").ToString();
                                lFecha = Convert.ToDateTime(reader.GetAttribute("FechaTimbrado").ToString());
                            }
                        }
                        
                    }
                    Ldia = lFecha.Day;
                    Lmes = lFecha.Month;
                    Lanio = lFecha.Year;
                    CreaDirectorios();

                    if (aFile != null && aFile.Length > 0)
                    {
                        sPDF = RutaXMLGraba + "\\" + lArchivo + ".XML";
                        Registralog("GeneraXML", pFOLIO, Operacion, "SE GENERARA XML:" + sPDF);
                        fs2 = new FileStream(sPDF, FileMode.Create);
                        fs2.Write(aFile, 0, aFile.Length);
                        fs2.Close();
                    }
                    else
                        RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Genera: Nodo (Respuesta.InfoCFDi.archivo) esta vacio ");
                }
                catch (Exception ex)
                {
                    RegistralogX("GeneraXML ", pFOLIO, Operacion, "ERROR al Genera:" + sPDF + " :" + ex.ToString());

                    string path = "c:\\xml33\\";
                    StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                    //w.Write("\r\nLog Entry : ");
                    w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        DateTime.Now.ToLongDateString());
                    w.WriteLine("  {0}", ex.ToString());
                    w.WriteLine("-------------------------------");
                    w.Close();
                }
                //'SE ACTULIZAN LOS REGISTROS DEL DOCUMENTO
                dvGraba = r.GetDataView("exec  CDFI33ActualizaCFDFactura '" + IDunico + "','" + Lmes + "','" + Ldia + "','" + Lanio + "','" + CadenaOriginal + "','" + lCertificadoSat + "','" + lFecha.ToString("yyyyMMdd hh:mm:ss") + "','" + lnoCertificado + "','" + Lanio + "','comprobante ','" + luuid + "', '" + lCertificadoSat + "'");
                dvafecta = r.GetDataView("P5CFDAFECTACXCCLIENTE " + IDunico + " ,'" + lFecha.ToString("yyyyMMdd") + "' ");
                //'dvGraba = r.GetDataView("exec  CDFActaulizaCFDFactura '" & iddocto.ToString & "','" & Lmes & "','" & Ldia & "','" & Lanio & "','" & CadenaOriginal & "','" & Respuesta.InfoCFDi.noCertificadoSAT & "','" & Respuesta.InfoCFDi.fechaTimbrado.ToString("yyyymmdd") & "','" & Respuesta.InfoCFDi.noCertificado & "','2011','comprobantebuscarlo'")
                //' dvafecta = r.GetDataView("P5CFDAFECTACXCCLIENTECARGO " & iddocto.ToString & " '" & Format(Respuesta.InfoCFDi.fechaTimbrado, "yyyymmdd") & "' ")

                //'lArchivo = Respuesta.InfoCFDi.serie.ToString.Trim & Respuesta.InfoCFDi.folio.ToString.Trim
                Registralog("ProcesaRespuesta", pFOLIO, Operacion, "ACTUALIZACION DE REGISTROS");
                r.exeQry(" update CFDFactura set SelloDigital = '" + SelloSAt + "' where    id = " + IDunico);

                //Err.Clear()
                
                dvProc = r.GetDataView(" update CFDFactura set ArchivoXML = '" + RutaXMLGraba + "\\" + lArchivo + ".XML'  where  id = " + IDunico);
                //'YA NO SE SOLICITA EL PDF EN LINEA, SOLO SE SOLICITA EN LOTES
                r.exeQry(" update CFDFactura set ArchivoPDF = '', IntentoPDF = isnull(IntentoPDF,0)+1 , SolicitaPDF = -1  where    id = " + IDunico);
                Etiqueta.Text = "Genera XML: " + pFOLIO;
                this.Refresh();
                Registralog("ProcesaRespuesta", pFOLIO, Operacion, "SE CREO PDF");

            }
            catch (Exception ex)
            {
                RegistralogX("ProcesaRespuesta ", pFOLIO, Operacion, "ERROR AL procesar respuesta" + ex.ToString());

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

            }
            return true;
        }


        public void CreaDirectorios()
        {
            string Directorio;
            Directorio = FormatoRutaCFD.ToLower();

            Directorio = Directorio.Replace("operacion", Operacion.ToUpper().Trim());
            Directorio = Directorio.Replace("plaza", Plaza.ToUpper().Trim());
            Directorio = Directorio.Replace("documento", Documento.Trim());
            Directorio = Directorio.Replace("anio", Lanio.ToString().Trim());
            Directorio = Directorio.Replace("mes", Lmes.ToString().Trim());
            Directorio = Directorio.Replace("dia", Ldia.ToString().Trim());
            Directorio = Directorio.Replace("empresa", Empresa.Trim());

            //On Error Resume Next
            //'RutaPDFCFD = "C:\PDF\"
            //'RutaXMLCFD = RutaPDFCFD
            try
            {
                RutaPDFGraba = RutaPDFCFD + Directorio;
                RutaXMLGraba = RutaXMLCFD + Directorio;



                if (!ExisteArchivo(RutaXMLGraba))
                {
                    DirectoryInfo di = Directory.CreateDirectory(RutaXMLGraba);
                    //System.Threading.Thread.Sleep(2000);
                }
                //'MkDir (RutaPDFGraba)

                //if Err.Number <> 75 And Err.Number <> 0 Then     'error 75 = ya existe el directorio--- cualquier otro error sera reportado
                //MsgBox("Error a crear el directorio: " & Err.Description & ":" & RutaPDFGraba)
                //End If

                //RutaPDFGraba = RutaPDFCFD + Directorio;
                //if (!ExisteArchivo(RutaPDFGraba))
                //{
                //    //Shell("cmd.exe /c md " + RutaPDFGraba);
                //    Process.Start("cmd.exe /c md " + RutaPDFGraba);
                //    System.Threading.Thread.Sleep(1000);
                //}

                //'  MkDir (RutaXMLGraba)
                //If Err.Number <> 75 And Err.Number <> 0 Then     'error 75 = ya existe el directorio--- cualquier otro error sera reportado
                //RegistralogX("CreaDirectorio", pFOLIO, Operacion, "Error a crear el directorio: " + Err.Description + ":" + RutaXMLGraba);
                //End If

                //Err.Clear()
            }
            catch (Exception ex)
            {
                //MsgBox("Error a crear el directorio: " + Err.Description + ":" + RutaPDFGraba);
                RegistralogX("CreaDirectorios", pFOLIO, Operacion, "Error a crear el directorio");
                RegistraError(IDunico, "Error a crear el directorio: " + ex.ToString(), "CreaDirectorios");

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
        public bool ExisteArchivo(string sNombreArchivo)
        {
            bool result = false;
            try
            {
                result = File.Exists(sNombreArchivo);
                //Object AttrDev%;
                ////On Error Resume Next
                //AttrDev = GetAttr(sNombreArchivo);
                ////If Err.Number Then
                ////    Err.Clear()

            } catch (Exception ex)
            {
                RegistraError(IDunico, "ExisteArchivo : " + ex.ToString(), "ExisteArchivo");

                try
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
                catch (Exception e)
                {

                    this.Etiqueta.Text = e.ToString();
                }
            }
            return result;
        }


        public byte[] FileToMemory(string Filename) //As System.IO.MemoryStream
        {
            System.IO.FileStream FS = new System.IO.FileStream(Filename, System.IO.FileMode.Open);
            System.IO.MemoryStream MS = new System.IO.MemoryStream();
            //byte[] BA = new byte[FS.Length - 1];
            byte[] BA = new byte[FS.Length];
            FS.Read(BA, 0, BA.Length);
            FS.Close();
            //MS.Write(BA, 0, BA.Length);
            return BA;
        }

        private void GrabaComprobanteCancelacion(string RutaArchivo)
        {
            try
            {
                string TextoHTL;
                string Eliminar;
                DataView dv = new DataView();
                System.IO.FileStream fs = new System.IO.FileStream(RutaArchivo, FileMode.Create, FileAccess.Write);
                //RutaArchivo = Mid$(RutaArchivo, 1, InStrRev(RutaArchivo, "\"))
                //Revisar   
                Eliminar = RutaArchivo.Split('\\').Last();
                RutaArchivo = RutaArchivo.Replace(Eliminar, "");

                dv = r.GetDataView("DECLARE @HTML VARCHAR(8000) exec  P5CorreoCancelacion  '" + Empresa + "', '" + pFOLIO + "', '" + Operacion + "', @HTML output seLECT isnull(@HTML,'') html");
                TextoHTL = dv[0]["html"].ToString();
                RutaArchivo = RutaArchivo + pFOLIO + "Cancela.html";
                StreamWriter s = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1252));
                s.WriteLine(TextoHTL);
                s.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                RegistraError(IDunico, "Grabar Comprobante Cancelacion  : " + ex.ToString(), "GrabaComprobanteCancelacion");

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
        private void GenerXMLEnvia(DataView datos, DataView addenda, int iddocto, string Operacion)
        {
            //Dim res As DBNull
            int i;
            //int n;
            int m;
            int L;
            int contAdd;
            int det;
            int detR;
            int Complementos;
            Int64 num;
            decimal TasaIVa;
            Byte[] VAriableArchivoByte;
            bool Sustitucion = false;
            bool Continuar = true;
            string MensajeError = "";
            DataView dvRelacionados = new DataView();

            Comprobante oComprobante = new Comprobante();
            ComprobanteEmisor oEmisor = new ComprobanteEmisor();
            ComprobanteCfdiRelacionados oCFDIRel = new ComprobanteCfdiRelacionados();
            ComprobanteCfdiRelacionadosCfdiRelacionado oCFDIRelRel = new ComprobanteCfdiRelacionadosCfdiRelacionado();
            ComprobanteReceptor oReceptor = new ComprobanteReceptor();
            ComprobanteImpuestos oComprobanteImpuestos = new ComprobanteImpuestos();
            ComprobanteImpuestosRetencion oComprobanteImpuestosRetencion = new ComprobanteImpuestosRetencion();
            ComprobanteImpuestosTraslado oComprobanteImpuestosTraslado = new ComprobanteImpuestosTraslado();
            ComprobanteComplemento oComprobanteComplemento = new ComprobanteComplemento();
            List<ComprobanteComplemento> lstComprobanteComplemento = new List<ComprobanteComplemento>();
            ComprobanteAddenda oComprobanteAddenda = new ComprobanteAddenda();
            ComprobanteConcepto oConcepto = new ComprobanteConcepto();
            ComprobanteConceptoImpuestos oConceptoImpuestos = new ComprobanteConceptoImpuestos();
            ComprobanteConceptoImpuestosTraslado oConceptoImpuestosTraslado = new ComprobanteConceptoImpuestosTraslado();
            ComprobanteConceptoImpuestosRetencion oConceptoImpuestosRetencion = new ComprobanteConceptoImpuestosRetencion();
            //redim
            List<ComprobanteImpuestosTraslado> lstComprobanteImpuestosTraslado = new List<ComprobanteImpuestosTraslado>();
            List<ComprobanteImpuestosRetencion> lstComprobanteImpuestosRetencion = new List<ComprobanteImpuestosRetencion>();
            List<ComprobanteCfdiRelacionadosCfdiRelacionado> lstCFDIRelRel = new List<ComprobanteCfdiRelacionadosCfdiRelacionado>();
            List<ComprobanteConcepto> lstConcepto = new List<ComprobanteConcepto>();
            List<ComprobanteConceptoImpuestos> lstConceptoImpuestos = new List<ComprobanteConceptoImpuestos>();
            List<ComprobanteConceptoImpuestosTraslado> lstConceptoImpuestosTraslado = new List<ComprobanteConceptoImpuestosTraslado>();
            List<ComprobanteConceptoImpuestosRetencion> lstConceptoImpuestosRetencion = new List<ComprobanteConceptoImpuestosRetencion>();


            Boolean tieneAddendaSoriana = false;

            Boolean llevacomplementoINE = false;

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalDigits = 2;

            try
            {
                if (TiempoEspera > 0)
                    System.Threading.Thread.Sleep(TiempoEspera + 50);

                GC.Collect();
                Ldia = DateTime.Now.Day;
                Lmes = DateTime.Now.Month;
                Lanio = DateTime.Now.Year;

                Registralog("GenerXMLEnvia", pFOLIO, Operacion, "INICIANDO PROCESO GenerXMLEnvia.NET");

                //    '       cRequest = New WSDiverza32.RequestGeneraCFDiType

                m = 0;

                ErrorResponse = "";

                //'*******************************************
                //' ARMADO DE LA INFORMACION QUE SE ENVIA
                //'*******************************************
                //'Dim proxy As New TimbreFiscal.TimbradoCFDI()

                //'carlos villarreal: valida que sea una sustitución de fatura o que tenga retención del 6% para la empresa SEY
                Decimal totalImpuestosRetenidos = 0;
                decimal.TryParse(datos[0]["totalImpuestosRetenidos"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out totalImpuestosRetenidos);

                //if(Operacion == "FACTURA" && totalImpuestosRetenidos > 0 && ParamArticuloesp25 != datos[0]["primerArticulo"].ToString().Trim() && Empresa.Trim().ToUpper() == "SEY" && datos[0]["TipodeRelacion"].ToString().Trim() == "04") {
                if (Operacion == "FACTURA" && datos[0]["TipodeRelacion"].ToString().Trim() == "04" && Empresa.Trim().ToUpper() == "SEY")
                {
                    //string path = "c:\\xml33\\";
                    //StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                    ////w.Write("\r\nLog Entry : ");
                    //w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    //    DateTime.Now.ToLongDateString());
                    //w.WriteLine(datos[0]["UUIDRelacionado"].ToString().Trim() + "  " + Operacion + " " + datos[0]["TipodeRelacion"].ToString().Trim() + " " + Empresa.Trim().ToUpper());
                    //w.WriteLine("-------------------------------");
                    //w.Close();

                    Sustitucion = true;
                    if (datos[0]["UUIDRelacionado"].ToString().Trim().Length <= 0)
                    {
                        //StreamWriter w1 = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                        ////w.Write("\r\nLog Entry : ");
                        //w1.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        //    DateTime.Now.ToLongDateString());
                        //w1.WriteLine("paso " + datos[0]["UUIDRelacionado"].ToString().Trim());
                        //w1.WriteLine("-------------------------------");
                        //w1.Close();

                        Continuar = false;
                        MensajeError = "No se encontro un UUID asociado";
                        this.Etiqueta.Text = MensajeError;
                        System.Threading.Thread.Sleep(TiempoEspera + 50);
                    }
                }

                if (Operacion == "CNCRED" && totalImpuestosRetenidos > 0 && ParamArticuloesp25 != datos[0]["primerArticulo"].ToString().Trim() && Empresa.Trim().ToUpper() == "SEY" && datos[0]["TipodeRelacion"].ToString().Trim() != "")
                {
                    MensajeError = datos[0]["TipodeRelacion"].ToString().Trim();
                    this.Etiqueta.Text = MensajeError;
                    Continuar = false;
                    System.Threading.Thread.Sleep(TiempoEspera + 50);
                }

                if (datos.Count > 0)
                {
                    
                    if (Continuar)
                    {
                        decimal total = 0;
                        decimal SubTotal = 0;
                        decimal TipoCambio = 0;

                        RFCemisor = datos[0]["rfcEmisor"].ToString();
                        RFCreceptor = datos[0]["rfcReceptor"].ToString();

                        //'*****************COMPROBANTE <---NODO
                        oComprobante.FormaPagoSpecified = true;
                        oComprobante.MetodoPagoSpecified = true;
                        oComprobante.LugarExpedicion = datos[0]["lugarExpedicion"].ToString();
                        oComprobante.CondicionesDePago = datos[0]["condicionesDePago"].ToString().Trim();
                        oComprobante.MetodoPago = datos[0]["sat_metodopago"].ToString().Trim();
                        oComprobante.TipoDeComprobante = datos[0]["tipoDeComprobante"].ToString();
                        //oComprobante.Total = Decimal.Parse(string.Format("{0: F}", datos[0]["Total"]));                
                        decimal.TryParse(datos[0]["Total"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out total);

                        oComprobante.Total = (Math.Truncate(total * 100) / 100) + 0.00M;
                        if (datos[0]["Moneda"].ToString() != "MXN")
                        {
                            decimal tipoCambio = 0;
                            decimal.TryParse(datos[0]["TipoCambio"].ToString(), out tipoCambio);
                            //if (datos[0]["TipoCambio"] == null)
                            //{
                            //    tipoCambio = 0.00m;
                            //}
                            //else {
                            //    tipoCambio = Decimal.Parse(string.Format("{0: F}", datos[0]["TipoCambio"]));
                            //}                            
                            oComprobante.TipoCambio = (Math.Truncate(tipoCambio * 100) / 100) + 0.00M;
                            //decimal.TryParse(datos[0]["TipoCambio"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out TipoCambio);
                            //oComprobante.TipoCambio = TipoCambio;

                            oComprobante.TipoCambioSpecified = true;
                        }
                        oComprobante.Moneda = datos[0]["Moneda"].ToString();
                        //oComprobante.SubTotal = Decimal.Parse(string.Format("{0: F}", datos[0]["SubTotal"]));
                        decimal.TryParse(datos[0]["SubTotal"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out SubTotal);
                        SubTotal = (Math.Truncate(SubTotal * 100) / 100) + 0.00M;
                        oComprobante.SubTotal = SubTotal;

                        oComprobante.Certificado = ""; //'datos[0]["Certificado"];
                        oComprobante.NoCertificado = datos[0]["NoCertificado"].ToString();
                        oComprobante.FormaPago = datos[0]["sat_formapago"].ToString().Trim();
                        oComprobante.Fecha = Convert.ToDateTime(datos[0]["Fecha"].ToString().Trim());
                        oComprobante.Folio = datos[0]["Folio"].ToString();

                        if (datos[0]["Serie"].ToString() != "")
                            oComprobante.Serie = datos[0]["Serie"].ToString();

                        oComprobante.Version = datos[0]["Version"].ToString();
                        oComprobante.Sello = "";

                        if (datos[0]["tipoDeComprobante"].ToString() == "E")
                        {

                            dvRelacionados = r.GetDataView("SELECT * FROM v_CFDIRelacionados Where Empresa = '" + Empresa + "' AND Folio= '" + pFOLIO + "'");
                            i = dvRelacionados.Count;

                            if (i > 0)
                            {
                                //oComprobante.CfdiRelacionados = oCFDIRel;
                                //oCFDIRel.TipoRelacion = datos[0]["sat_tiporelacion"].ToString().Trim();
                                for (int n = 0; n < i; n++)
                                {
                                    //'**************** CONCEPTOS <---NODOS
                                    //ReDim oCFDIRel.CfdiRelacionado(1)
                                    oCFDIRelRel = new ComprobanteCfdiRelacionadosCfdiRelacionado() { UUID = dvRelacionados[0]["UUID"].ToString() };
                                    lstCFDIRelRel.Add(oCFDIRelRel);

                                    //oCFDIRel.CfdiRelacionado[i] = oCFDIRelRel;
                                    //oCFDIRelRel.UUID = dvRelacionados[0]["UUID"].ToString();
                                }
                                //oCFDIRel.CfdiRelacionado = lstCFDIRelRel.ToArray();
                            }
                        }


                        oComprobante.Emisor = oEmisor;
                        oEmisor.RegimenFiscal = datos[0]["RegimenFiscal"].ToString();
                        oEmisor.Nombre = datos[0]["Emisornombre"].ToString();
                        oEmisor.Rfc = datos[0]["rfcEmisor"].ToString();

                        oComprobante.Receptor = oReceptor;
                        oReceptor.Nombre = datos[0]["Receptornombre"].ToString();
                        oReceptor.Rfc = datos[0]["RFCReceptor"].ToString();
                        oReceptor.UsoCFDI = datos[0]["sat_usoCFDI"].ToString().Trim();

                        Registralog("GenerXMLEnvia", pFOLIO, Operacion, "SE GENERO NODO COMPROBANTE-EMISOR-RECEPTOr FOLIO =" + oComprobante.Serie + oComprobante.Folio + ",SUC:" + datos[0]["aliasSucursal"].ToString());

                        Double Descuento = VBVal(datos[0]["descuento"].ToString());
                        if (Descuento > 0)
                        {
                            // oComprobante.Descuento = Decimal.Parse(string.Format("#########0.00", datos[0]["descuento"]));
                            Decimal PDescuento = 0;
                            decimal.TryParse(datos[0]["descuento"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out PDescuento);
                            PDescuento = (Math.Truncate(PDescuento * 100) / 100) + 0.00M;
                            oComprobante.Descuento = PDescuento;

                            if (VBVal(datos[0]["descuento"].ToString()) > 0)
                                oComprobante.DescuentoSpecified = true;
                            else
                                oComprobante.DescuentoSpecified = false;
                        }

                        //'**************** IMPUESTOS <---NODO
                        //'Remision.Impuestos.totalImpuestosTrasladados = datos[0]["totalImpuestosTrasladados").ToString()
                        if (datos[0]["Excento"].ToString() != "S")
                        {
                            if (VBVal(datos[0]["totalImpuestosTrasladados"].ToString()) >= 0)
                            {
                                oComprobanteImpuestos = new ComprobanteImpuestos();
                                oComprobanteImpuestos.TotalImpuestosTrasladadosSpecified = true;
                                //oComprobanteImpuestos.TotalImpuestosTrasladados = Decimal.Parse(datos[0]["totalImpuestosTrasladados"].ToString());
                                Decimal totalImpuestosTrasladados = 0;
                                decimal.TryParse(datos[0]["totalImpuestosTrasladados"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out totalImpuestosTrasladados);
                                oComprobanteImpuestos.TotalImpuestosTrasladados = totalImpuestosTrasladados;

                            }
                            else
                                oComprobanteImpuestos.TotalImpuestosTrasladadosSpecified = false;
                        }
                        else
                            oComprobanteImpuestos.TotalImpuestosTrasladadosSpecified = false;


                        Complementos = 0;
                        //Decimal.Parse(datos[0]["totalImpuestosRetenidos"].ToString())
                        if ((datos[0]["PrimerArticulo"].ToString().Trim() == ParamArticuloesp25 && totalImpuestosRetenidos > 0) ||
                            ((Empresa.Trim().ToUpper() == "SEY" || listaDeCliente.IndexOf(addenda[0]["ClaveCliente"].ToString()) > 0) && totalImpuestosRetenidos > 0 && datos[0]["TasaRetencion"].ToString() == "3"))
                        {
                            //ReDim oComprobante.Complemento[1]
                            //'Remision.Complemento = New ComplementoComplexType
                            lstComprobanteComplemento = new List<ComprobanteComplemento>();
                            lstComprobanteComplemento.Add(new ComprobanteComplemento());
                            oComprobante.Complemento = lstComprobanteComplemento.ToArray();

                            if (datos[0]["PrimerArticulo"].ToString().Trim() == ParamArticuloesp25 && totalImpuestosRetenidos > 0)
                            {
                                XmlDocument xmlDoc2 = new XmlDocument();
                                xmlDoc2.AppendChild(xmlDoc2.CreateElement("implocal:ImpuestosLocales", "http://www.sat.gob.mx/implocal"));
                                //''--TotaldeRetenciones = "0.00" TotaldeTraslados = "150.00" version = "1.0"
                                xmlDoc2.DocumentElement.SetAttribute("TotaldeRetenciones", datos[0]["totalImpuestosRetenidos"].ToString());
                                xmlDoc2.DocumentElement.SetAttribute("TotaldeTraslados", "0.00");
                                xmlDoc2.DocumentElement.SetAttribute("version", "1.0");

                                //Dim Elemento1 As XmlElement
                                XmlElement Elemento1;
                                //'' < implocal:TrasladosLocales Importe = "150.00" ImpLocTrasladado = "DERECHO AL MILLAR" TasadeTraslado = "5.00" />
                                Elemento1 = xmlDoc2.CreateElement("implocal:RetencionesLocales", "http://www.sat.gob.mx/implocal");
                                Elemento1.SetAttribute("Importe", datos[0]["totalImpuestosRetenidos"].ToString());
                                Elemento1.SetAttribute("ImpLocRetenido", ClaveImpRetencion);
                                Elemento1.SetAttribute("TasadeRetencion", datos[0]["TasaRetencion"].ToString());
                                xmlDoc2.DocumentElement.AppendChild(Elemento1);
                                //checar
                                //ReDim oComprobante.Complemento[0].Any(Complementos)

                                oComprobante.Complemento[0].Any[Complementos].AppendChild(xmlDoc2.FirstChild);
                                Complementos = Complementos + 1;
                            }
                            //'<implocal:ImpuestosLocales version="1.0" TotaldeRetenciones="500.00" TotaldeTraslados="500.00">
                            //'<implocal:RetencionesLocales ImpLocRetenido="Retención 5 % al millar" TasadeRetencion="5.00" Importe="500.00"/>
                            //'<implocal:TrasladosLocales ImpLocTrasladado="5 % al millar" TasadeTraslado="5.00" Importe="500.00"/>
                            //'</implocal:ImpuestosLocales>

                            //InStr(listaDeCliente, addenda[0]["ClaveCliente"].ToString(), CompareMethod.Text) es equivalente a indexof  datos[0]["totalImpuestosTrasladados"]

                            //Decimal.Parse(datos[0]["totalImpuestosRetenidos"].ToString())

                            if ((Empresa.Trim().ToUpper() == "SEY" || listaDeCliente.IndexOf(addenda[0]["ClaveCliente"].ToString()) > 0) && totalImpuestosRetenidos > 0 && datos[0]["TasaRetencion"].ToString() == "3")
                            {
                                XmlDocument xmlDoc2 = new XmlDocument();
                                xmlDoc2.AppendChild(xmlDoc2.CreateElement("implocal:ImpuestosLocales", "http://www.sat.gob.mx/implocal"));
                                //''--TotaldeRetenciones = "0.00" TotaldeTraslados = "150.00" version = "1.0";
                                xmlDoc2.DocumentElement.SetAttribute("TotaldeRetenciones", datos[0]["totalImpuestosRetenidos"].ToString());
                                xmlDoc2.DocumentElement.SetAttribute("TotaldeTraslados", "0.00");
                                xmlDoc2.DocumentElement.SetAttribute("version", "1.0");

                                XmlElement Elemento1;

                                //'' < implocal:TrasladosLocales Importe = "150.00" ImpLocTrasladado = "DERECHO AL MILLAR" TasadeTraslado = "5.00" />

                                Elemento1 = xmlDoc2.CreateElement("implocal:RetencionesLocales", "http://www.sat.gob.mx/implocal");
                                Elemento1.SetAttribute("Importe", datos[0]["totalImpuestosRetenidos"].ToString());
                                Elemento1.SetAttribute("ImpLocRetenido", datos[0]["DescrRetencion"].ToString());
                                Elemento1.SetAttribute("TasadeRetencion", datos[0]["TasaRetencion"].ToString());
                                xmlDoc2.DocumentElement.AppendChild(Elemento1);

                                //ReDim
                                //ReDim oComprobante.Complemento[0].Any(Complementos)


                                oComprobante.Complemento[0].Any[Complementos].AppendChild(xmlDoc2.FirstChild);
                                Complementos = Complementos + 1;
                            }
                        }
                        i = datos.Count;
                        det = 0;
                        detR = 0;
                        //ReDim oComprobante.Conceptos(i)
                        //for( n = 0 To i -1)                        

                        for (int n = 0; n < i; n++)
                        {
                            // '**************** CONCEPTOS <---NODOS

                            //''sat_tiporelacion
                            //'If datos[0]["TipodeRelacion") = "04" And Empresa.Trim.ToUpper = "SEY" Then
                            //'    oComprobante.CfdiRelacionados = oCFDIRel
                            //'    oCFDIRel.TipoRelacion = datos[0]["TipodeRelacion").ToString()().Trim()
                            //'    ReDim oCFDIRel.CfdiRelacionado(1)
                            //'    oCFDIRel.CfdiRelacionado(0) = oCFDIRelRel
                            //'    oCFDIRelRel.UUID = datos[0]["UUIDRelacionado")
                            //'End If

                            if (Sustitucion)
                            {
                                oCFDIRelRel = new ComprobanteCfdiRelacionadosCfdiRelacionado() { UUID = datos[0]["UUIDRelacionado"].ToString().Trim() };
                                lstCFDIRelRel.Add(oCFDIRelRel);
                            }

                            oConcepto = new ComprobanteConcepto();
                            lstConceptoImpuestosTraslado = new List<ComprobanteConceptoImpuestosTraslado>();
                            lstConceptoImpuestosRetencion = new List<ComprobanteConceptoImpuestosRetencion>();
                            oConcepto.ClaveProdServ = datos[n]["ClaveProdServ"].ToString();
                            //Decimal.Parse(String.Format("{0:f}", datos[n]["Cantidad")))
                            Decimal Cantidad = 0;
                            Decimal valorUnitario = 0;
                            Decimal Importe = 0;
                            decimal.TryParse(datos[n]["Cantidad"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out Cantidad);
                            decimal.TryParse(datos[n]["valorUnitario"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out valorUnitario);
                            decimal.TryParse(datos[n]["Importe"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out Importe);

                            if(Operacion != "CNCRED")
                            {
                                Cantidad = (Math.Truncate(Cantidad * 100) / 100) + 0.00M;
                                Importe = (Math.Truncate(Importe * 100) / 100) + 0.00M;
                            }

                            oConcepto.Cantidad = Cantidad;
                            oConcepto.ClaveUnidad = datos[n]["sat_claveunidad"].ToString();
                            oConcepto.Unidad = datos[n]["UnidadMedida"].ToString();
                            oConcepto.Descripcion = datos[n]["Descripcion"].ToString();
                            oConcepto.ValorUnitario = valorUnitario;
                            oConcepto.Importe = Importe;                           

                            if (VBVal(datos[n]["TotalDescuentoPartida"].ToString()) > 0)
                            {
                                oConcepto.DescuentoSpecified = true;
                                //oComprobante.Conceptos[n].Descuento = Decimal.Parse(datos[n]["TotalDescuentoPartida"].ToString());
                                Decimal TotalDescuentoPartida = 0;
                                decimal.TryParse(datos[n]["TotalDescuentoPartida"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out TotalDescuentoPartida);
                                oConcepto.Descuento = TotalDescuentoPartida;
                            }

                            if (datos[n]["noIdentificacion"].ToString() != "")
                            {
                                oConcepto.NoIdentificacion = datos[n]["noIdentificacion"].ToString();
                            }

                            oConceptoImpuestos = new ComprobanteConceptoImpuestos();

                            if (VBVal(datos[n]["ConceptoTrasladoimporte"].ToString()) >= 0)
                            {
                                if (datos[0]["Excento"].ToString() == "S")
                                {
                                    //  'oComprobante.Conceptos[n].Impuestos.Traslados(det).ImporteSpecified = False
                                    //    'oComprobante.Conceptos[n].Impuestos.Traslados(det).TasaOCuotaSpecified = False
                                }
                                else
                                {
                                    ////    ReDim oComprobante.Conceptos[n].Impuestos.Traslados(det)
                                    //oComprobante.Conceptos[n].Impuestos.Traslados[det] = new ComprobanteConceptoImpuestosTraslado();
                                    ////oComprobante.Conceptos[n].Impuestos.Traslados[det].Base = Decimal.Parse(datos[n]["ConceptoTrasladoBase"].ToString());
                                    //oComprobante.Conceptos[n].Impuestos.Traslados[det].Base = ConceptoTrasladoBase;
                                    //oComprobante.Conceptos[n].Impuestos.Traslados[det].ImporteSpecified = true;
                                    //oComprobante.Conceptos[n].Impuestos.Traslados[det].TasaOCuotaSpecified = true;

                                    //oComprobante.Conceptos[n].Impuestos.Traslados[det].Importe = ConceptoTrasladoimporte;//Decimal.Parse(datos[n]["ConceptoTrasladoimporte"].ToString());
                                    //oComprobante.Conceptos[n].Impuestos.Traslados[det].Impuesto = datos[n]["ConceptoTrasladoimpuesto"].ToString().Trim();
                                    //oComprobante.Conceptos[n].Impuestos.Traslados[det].TipoFactor = datos[n]["ConceptoTrasladotipofactor"].ToString().Trim();

                                    //oComprobante.Conceptos[n].Impuestos.Traslados[det].TasaOCuota = ConceptoTrasladoTasaOCuota;//Decimal.Parse(datos[n]["ConceptoTrasladoTasaOCuota"].ToString());
                                    //det = det + 1;
                                    Decimal ConceptoTrasladoBase = 0;
                                    decimal.TryParse(datos[n]["ConceptoTrasladoBase"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out ConceptoTrasladoBase);
                                    Decimal ConceptoTrasladoimporte = 0;
                                    decimal.TryParse(datos[n]["ConceptoTrasladoimporte"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out ConceptoTrasladoimporte);
                                    Decimal ConceptoTrasladoTasaOCuota = 0;
                                    decimal.TryParse(datos[n]["ConceptoTrasladoTasaOCuota"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out ConceptoTrasladoTasaOCuota);

                                    //ReDim
                                    //    ReDim oComprobante.Conceptos[n].Impuestos.Traslados(det)
                                    oConceptoImpuestosTraslado = new ComprobanteConceptoImpuestosTraslado();
                                    //oComprobante.Conceptos[n].Impuestos.Traslados[det] = new ComprobanteConceptoImpuestosTraslado();
                                    //oComprobante.Conceptos[n].Impuestos.Traslados[det].Base = Decimal.Parse(datos[n]["ConceptoTrasladoBase"].ToString());
                                    oConceptoImpuestosTraslado.Base = ConceptoTrasladoBase;
                                    oConceptoImpuestosTraslado.ImporteSpecified = true;
                                    oConceptoImpuestosTraslado.TasaOCuotaSpecified = true;
                                    oConceptoImpuestosTraslado.Importe = (Math.Truncate(ConceptoTrasladoimporte * 100) / 100) + 0.00M;//Decimal.Parse(datos[n]["ConceptoTrasladoimporte"].ToString());
                                    oConceptoImpuestosTraslado.Impuesto = datos[n]["ConceptoTrasladoimpuesto"].ToString().Trim();
                                    oConceptoImpuestosTraslado.TipoFactor = datos[n]["ConceptoTrasladotipofactor"].ToString().Trim();
                                    //ConceptoTrasladoTasaOCuota = Math.Truncate(ConceptoTrasladoTasaOCuota * 100) / 100;
                                    //ConceptoTrasladoTasaOCuota = ConceptoTrasladoTasaOCuota;
                                    //ConceptoTrasladoTasaOCuota = Decimal.Parse(string.Format("{0: F}", datos[0]["ConceptoTrasladoTasaOCuota"]));
                                    oConceptoImpuestosTraslado.TasaOCuota = ConceptoTrasladoTasaOCuota;//Decimal.Parse(datos[n]["ConceptoTrasladoTasaOCuota"].ToString());

                                    det = det + 1;

                                    //Llenado de informacion
                                    lstConceptoImpuestosTraslado.Add(oConceptoImpuestosTraslado);
                                }
                            }

                            if (totalImpuestosRetenidos > 0 && ParamArticuloesp25 != datos[0]["primerArticulo"].ToString().Trim() && Empresa.Trim().ToUpper() != "SEY")
                            {
                                if (VBVal(datos[n]["ConceptoRetencionimporte"].ToString()) > 0)
                                {
                                    ////'oComprobante.Conceptos[n].Impuestos = New ComprobanteConceptoImpuestos()
                                    ////  ReDim oComprobante.Conceptos[n].Impuestos.Retenciones(detR)
                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR] = new ComprobanteConceptoImpuestosRetencion();
                                    ////oComprobante.Conceptos[n].Impuestos.Retenciones[detR].Base = Decimal.Parse(datos[n]["ConceptoTrasladoBase"].ToString());
                                    ////oComprobante.Conceptos[n].Impuestos.Retenciones[detR].Importe = Decimal.Parse(datos[n]["ConceptoRetencionimporte"].ToString());
                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR].Base = ConceptoTrasladoBase;
                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR].Importe = ConceptoRetencionimporte;

                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR].Impuesto = datos[n]["ConceptoRetencionimpuesto"].ToString().Trim();
                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR].TipoFactor = datos[n]["ConceptoRetenciontipofactor"].ToString().Trim();
                                    ////oComprobante.Conceptos[n].Impuestos.Retenciones[detR].TasaOCuota = Decimal.Parse(datos[n]["ConceptoRetencionTasaOCuota"].ToString());
                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR].TasaOCuota = ConceptoRetencionTasaOCuota;

                                    Decimal ConceptoTrasladoBase = 0;
                                    decimal.TryParse(datos[n]["ConceptoTrasladoBase"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out ConceptoTrasladoBase);
                                    Decimal ConceptoRetencionimporte = 0;
                                    decimal.TryParse(datos[n]["ConceptoRetencionimporte"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out ConceptoRetencionimporte);
                                    Decimal ConceptoRetencionTasaOCuota = 0;
                                    decimal.TryParse(datos[n]["ConceptoRetencionTasaOCuota"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out ConceptoRetencionTasaOCuota);

                                    oConceptoImpuestosRetencion = new ComprobanteConceptoImpuestosRetencion();
                                    oConceptoImpuestosRetencion.Base = ConceptoTrasladoBase;
                                    oConceptoImpuestosRetencion.Importe = (Math.Truncate(ConceptoRetencionimporte * 100) / 100) + 0.00M;
                                    oConceptoImpuestosRetencion.Impuesto = datos[n]["ConceptoRetencionimpuesto"].ToString().Trim();
                                    oConceptoImpuestosRetencion.TipoFactor = datos[n]["ConceptoRetenciontipofactor"].ToString().Trim();
                                    //ConceptoRetencionTasaOCuota = Math.Truncate(ConceptoRetencionTasaOCuota * 100) / 100;
                                    //ConceptoRetencionTasaOCuota = Decimal.Parse(string.Format("{0: F}", datos[0]["ConceptoRetencionTasaOCuota"]));
                                    oConceptoImpuestosRetencion.TasaOCuota = ConceptoRetencionTasaOCuota;
                                    //oConceptoImpuestosRetencion.TasaOCuota = Math.Truncate(ConceptoRetencionTasaOCuota * 100) / 100;

                                    detR = detR + 1;
                                    lstConceptoImpuestosRetencion.Add(oConceptoImpuestosRetencion);
                                }
                            }

                            //''Modificado carlos loera 14 Enero 2020

                            if (totalImpuestosRetenidos > 0 && ParamArticuloesp25 != datos[0]["primerArticulo"].ToString().Trim() && Empresa.Trim().ToUpper() == "SEY")
                            {
                                if (VBVal(datos[n]["ConceptoRetencionimporte"].ToString()) > 0)
                                {
                                    ////'oComprobante.Conceptos[n].Impuestos = New ComprobanteConceptoImpuestos()
                                    ////ReDim oComprobante.Conceptos[n].Impuestos.Retenciones(detR)
                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR] = new ComprobanteConceptoImpuestosRetencion();
                                    ////oComprobante.Conceptos[n].Impuestos.Retenciones[detR].Base = Decimal.Parse(datos[n]["ConceptoTrasladoBase"].ToString());
                                    ////oComprobante.Conceptos[n].Impuestos.Retenciones[detR].Importe = Decimal.Parse(datos[n]["ConceptoRetencionimporte"].ToString());
                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR].Base = ConceptoTrasladoBase;
                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR].Importe = ConceptoRetencionimporte;

                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR].Impuesto = datos[n]["ConceptoRetencionimpuesto"].ToString().Trim();
                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR].TipoFactor = datos[n]["ConceptoRetenciontipofactor"].ToString().Trim();
                                    ////oComprobante.Conceptos[n].Impuestos.Retenciones[detR].TasaOCuota = Decimal.Parse(datos[n]["ConceptoRetencionTasaOCuota"].ToString());
                                    //oComprobante.Conceptos[n].Impuestos.Retenciones[detR].TasaOCuota = ConceptoRetencionTasaOCuota;

                                    Decimal ConceptoTrasladoBase = 0;
                                    decimal.TryParse(datos[n]["ConceptoTrasladoBase"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out ConceptoTrasladoBase);
                                    Decimal ConceptoRetencionimporte = 0;
                                    decimal.TryParse(datos[n]["ConceptoRetencionimporte"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out ConceptoRetencionimporte);
                                    Decimal ConceptoRetencionTasaOCuota = 0;
                                    decimal.TryParse(datos[n]["ConceptoRetencionTasaOCuota"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out ConceptoRetencionTasaOCuota);

                                    oConceptoImpuestosRetencion = new ComprobanteConceptoImpuestosRetencion();
                                    oConceptoImpuestosRetencion.Base = ConceptoTrasladoBase;
                                    oConceptoImpuestosRetencion.Importe = (Math.Truncate(ConceptoRetencionimporte * 100) / 100) + 0.00M;
                                    oConceptoImpuestosRetencion.Impuesto = datos[n]["ConceptoRetencionimpuesto"].ToString().Trim();
                                    oConceptoImpuestosRetencion.TipoFactor = datos[n]["ConceptoRetenciontipofactor"].ToString().Trim();
                                    //ConceptoRetencionTasaOCuota = Math.Truncate(ConceptoRetencionTasaOCuota * 100) / 100;
                                    //ConceptoRetencionTasaOCuota = Decimal.Parse(string.Format("{0: F}", datos[0]["ConceptoRetencionTasaOCuota"]));
                                    oConceptoImpuestosRetencion.TasaOCuota = ConceptoRetencionTasaOCuota;
                                    //oConceptoImpuestosRetencion.TasaOCuota = Math.Truncate(ConceptoRetencionTasaOCuota * 100) / 100; 

                                    detR = detR + 1;
                                    lstConceptoImpuestosRetencion.Add(oConceptoImpuestosRetencion);
                                }
                            }
                            Decimal ConceptoTrasladoimporte1 = 0;
                            decimal.TryParse(datos[n]["ConceptoTrasladoimporte"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out ConceptoTrasladoimporte1);

                            if (ConceptoTrasladoimporte1 >= 0)
                            {
                                m = 1;//   'Solo se envia el iva por tipo de tasa   (tasa 16 o tasa 11)
                            }

                            //agrega a lista
                            if (lstConceptoImpuestosRetencion.Count > 0)
                                oConceptoImpuestos.Retenciones = lstConceptoImpuestosRetencion.ToArray();

                            if (lstConceptoImpuestosTraslado.Count > 0)
                                oConceptoImpuestos.Traslados = lstConceptoImpuestosTraslado.ToArray();

                            if (lstConceptoImpuestosTraslado.Count > 0 || lstConceptoImpuestosRetencion.Count > 0)
                            {
                                oConcepto.Impuestos = oConceptoImpuestos;                                
                            }
                            lstConcepto.Add(oConcepto);
                        }

                        if (lstConcepto.Count > 0)
                            oComprobante.Conceptos = lstConcepto.ToArray();

                        if (lstCFDIRelRel.Count > 0)
                        {
                            oCFDIRel = new ComprobanteCfdiRelacionados();                            
                            if(Sustitucion)
                            {
                                oCFDIRel.TipoRelacion = datos[0]["TipodeRelacion"].ToString().Trim();
                            }
                            else
                            {
                                oCFDIRel.TipoRelacion = datos[0]["sat_tiporelacion"].ToString().Trim();
                            }
                            oCFDIRel.CfdiRelacionado = lstCFDIRelRel.ToArray();
                            oComprobante.CfdiRelacionados = oCFDIRel;
                        }

                        //''Modificado carlos loera 14 Enero 2020
                        if (totalImpuestosRetenidos > 0 && ParamArticuloesp25 != datos[0]["primerArticulo"].ToString().Trim() && Empresa.Trim().ToUpper() == "SEY")
                        {
                            Decimal TotalImpuestosRetenidos = 0;
                            decimal.TryParse(datos[0]["TotalImpuestosRetenidos"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out TotalImpuestosRetenidos);

                            //oComprobanteImpuestos.TotalImpuestosRetenidos = Decimal.Parse(datos[0]["TotalImpuestosRetenidos"].ToString());
                            //oComprobanteImpuestos.TotalImpuestosRetenidos = TotalImpuestosRetenidos;
                            ////ReDim oComprobante.Impuestos.Retenciones[1]
                            //oComprobante.Impuestos.Retenciones[0] = oComprobanteImpuestosRetencion;
                            ////oComprobanteImpuestosRetencion.Importe = Decimal.Parse(string.Format("##########0.00", datos[0]["totalImpuestosRetenidos"]));
                            //oComprobanteImpuestosRetencion.Importe = totalImpuestosRetenidos;
                            //oComprobanteImpuestosRetencion.Impuesto = datos[0]["Impuesto"].ToString().Trim();

                            oComprobanteImpuestos.TotalImpuestosRetenidosSpecified = true;
                            oComprobanteImpuestos.TotalImpuestosRetenidos = TotalImpuestosRetenidos;
                            //ReDim oComprobante.Impuestos.Retenciones[1]

                            oComprobanteImpuestosRetencion = new ComprobanteImpuestosRetencion()
                            {
                                Importe = (Math.Truncate(totalImpuestosRetenidos * 100) / 100) + 0.00M,
                                Impuesto = datos[0]["Impuesto"].ToString().Trim()
                            };
                            lstComprobanteImpuestosRetencion.Add(oComprobanteImpuestosRetencion);


                        }


                        if (totalImpuestosRetenidos > 0 && ParamArticuloesp25 != datos[0]["primerArticulo"].ToString().Trim() && Empresa.Trim().ToUpper() != "SEY")
                        {
                            Decimal TotalImpuestosRetenidos = 0;
                            decimal.TryParse(datos[0]["TotalImpuestosRetenidos"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out TotalImpuestosRetenidos);

                            //oComprobanteImpuestos.TotalImpuestosRetenidosSpecified = true;
                            ////oComprobanteImpuestos.TotalImpuestosRetenidos = Decimal.Parse(datos[0]["TotalImpuestosRetenidos"].ToString());
                            //oComprobanteImpuestos.TotalImpuestosRetenidos = TotalImpuestosRetenidos;
                            ////ReDim oComprobante.Impuestos.Retenciones[1]
                            //oComprobante.Impuestos.Retenciones[0] = oComprobanteImpuestosRetencion;
                            ////oComprobanteImpuestosRetencion.Importe = Decimal.Parse(string.Format("##########0.00", datos[0]["totalImpuestosRetenidos"]));
                            //oComprobanteImpuestosRetencion.Importe = totalImpuestosRetenidos;
                            //oComprobanteImpuestosRetencion.Impuesto = datos[0]["Impuesto"].ToString().Trim();

                            oComprobanteImpuestos.TotalImpuestosRetenidosSpecified = true;
                            oComprobanteImpuestos.TotalImpuestosRetenidos = TotalImpuestosRetenidos;
                            //ReDim oComprobante.Impuestos.Retenciones[1]

                            oComprobanteImpuestosRetencion = new ComprobanteImpuestosRetencion()
                            {
                                Importe = (Math.Truncate(totalImpuestosRetenidos * 100) / 100) + 0.00M,
                                Impuesto = datos[0]["Impuesto"].ToString().Trim()
                            };
                            lstComprobanteImpuestosRetencion.Add(oComprobanteImpuestosRetencion);
                        }

                        if (m > 0)
                        {
                            m = 0;

                            Decimal totalImpuestosTrasladados = 0;
                            decimal.TryParse(datos[0]["totalImpuestosTrasladados"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out totalImpuestosTrasladados);

                            if (totalImpuestosTrasladados >= 0)
                            {
                                if (datos[0]["Excento"].ToString() == "S")
                                    oComprobanteImpuestos.TotalImpuestosTrasladadosSpecified = false;
                                else
                                {
                                    //decimal Trasladotasa1;
                                    //decimal.TryParse(datos[0]["Trasladotasa"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out Trasladotasa1);

                                    //Decimal Trasladotasa1 = 0;
                                    //decimal.TryParse(datos[0]["Trasladotasa"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out Trasladotasa1);

                                    //Decimal ConceptoTrasladoTasaOCuota = 0;
                                    //decimal.TryParse(datos[0]["Trasladotasa"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out ConceptoTrasladoTasaOCuota);

                                    //decimal Trasladotasa=Convert.ToDecimal()
                                    //string path = "c:\\xml33\\";
                                    //StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                                    ////w.Write("\r\nLog Entry : ");
                                    //w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                                    //    DateTime.Now.ToLongDateString());
                                    //w.WriteLine(ConceptoTrasladoTasaOCuota + " _ " + Convert.ToDecimal(datos[0]["Trasladotasa"]).ToString() + " " + datos[0]["Trasladotasa"] + "  " +datos[0]["Trasladotasa"].ToString() + " datos[0][Trasladotasa] Linea 1838 " + ConceptoTrasladoTasaOCuota);
                                    //w.WriteLine("-------------------------------");
                                    //w.Close();

                                    //ReDim oComprobante.Impuestos.Traslados(1)
                                    oComprobanteImpuestos.TotalImpuestosTrasladadosSpecified = true;
                                    //oComprobante.Impuestos.Traslados[m] = oComprobanteImpuestosTraslado;//oComprobanteImpuestosTraslado.TasaOCuota = Decimal.Parse(datos[0]["Trasladotasa"].ToString().Trim());
                                    //oComprobanteImpuestosTraslado.Importe = totalImpuestosTrasladados;´//oComprobanteImpuestosTraslado.Importe = Decimal.Parse(datos[0]["totalImpuestosTrasladados"].ToString()); //  '  (datos[n]["Trasladoimpuesto").ToString())
                                    //oComprobanteImpuestosTraslado.TasaOCuota = Trasladotasa;
                                    //oComprobanteImpuestosTraslado.Impuesto = datos[0]["Impuesto"].ToString().Trim(); //'impuestoTrasladoSimpleType.IVA ' ver como poner este valor y que opciones tengo
                                    //oComprobanteImpuestosTraslado.TipoFactor = datos[0]["TipoFactor"].ToString().Trim();// oComprobanteImpuestosTraslado.TipoFactor = Format(datos[0]["TipoFactor"]).ToString()().Trim()
                                    Decimal Tasa = Convert.ToDecimal(datos[0]["Trasladotasa"].ToString());
                                    //Tasa = Math.Truncate(Tasa * 100) / 100;

                                    oComprobanteImpuestosTraslado = new ComprobanteImpuestosTraslado()
                                    {
                                        Importe = (Math.Truncate(totalImpuestosTrasladados * 100) / 100) + 0.00M,
                                        TasaOCuota = Tasa,
                                        //TasaOCuota = Decimal.Parse(string.Format("{0: F}", datos[0]["Trasladotasa"])),
                                        Impuesto = datos[0]["Impuesto"].ToString().Trim(),
                                        TipoFactor = datos[0]["TipoFactor"].ToString().Trim()
                                    };
                                    lstComprobanteImpuestosTraslado.Add(oComprobanteImpuestosTraslado);

                                    //StreamWriter w1 = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                                    ////w.Write("\r\nLog Entry : ");
                                    //w1.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                                    //    DateTime.Now.ToLongDateString());
                                    //w1.WriteLine(oComprobanteImpuestosTraslado.TasaOCuota + " line 1866");
                                    //w1.WriteLine("-------------------------------");
                                    //w1.Close();

                                }
                                //' If Empresa.ToUpper.Trim <> "APP" Then   ' analizar bien caso de APP que se pone en los nodos
                                //'End If
                                //' m = m + 1
                            }

                        }
                        else
                        {
                            //' No ENVIAR LOS NODOS CUANDO SEA EXCENTO DE IMPUESTOS
                            //    ' Madar los nodos cuando sea tasa cero
                            //    ' exento = campo delinea de articulo
                            //    ' If datos[0]["Excento") <> "S" Then
                            //    ' analizar bien caso de APP que se pone en los nodos
                            //    ' ****todo circulacion y suscripciones son TASA CERO
                            if (datos[0]["Excento"].ToString() == "S")
                            {
                                // 'no se envian los nodos
                            }
                            else
                            {
                                Decimal totalImpuestosTrasladados = 0;
                                decimal.TryParse(datos[0]["totalImpuestosTrasladados"].ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, nfi, out totalImpuestosTrasladados);

                                if (totalImpuestosTrasladados >= 0)
                                {
                                    //oComprobante.Impuestos = new ComprobanteImpuestos();
                                    oComprobanteImpuestosTraslado = new ComprobanteImpuestosTraslado()
                                    {
                                        Importe = Convert.ToDecimal("0.00") + 0.00M,
                                        TasaOCuota = Convert.ToDecimal("0.00") + 0.00M,
                                        Impuesto = datos[0]["Impuesto"].ToString().Trim(),
                                        TipoFactor = datos[0]["TipoFactor"].ToString().Trim()
                                    };
                                    lstComprobanteImpuestosTraslado.Add(oComprobanteImpuestosTraslado);

                                    //string path = "c:\\xml33\\";
                                    //StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                                    ////w.Write("\r\nLog Entry : ");
                                    //w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                                    //    DateTime.Now.ToLongDateString());
                                    //w.WriteLine(Convert.ToDecimal("0.00").ToString() + " Convert.ToDecimal(0.00) Linea 1789 " );
                                    //w.WriteLine("-------------------------------");
                                    //w.Close();
                                }
                            }
                        }
                        if (oComprobanteImpuestos.TotalImpuestosRetenidosSpecified || oComprobanteImpuestos.TotalImpuestosTrasladadosSpecified)
                        {
                            //actualiza la lista
                            if (lstComprobanteImpuestosTraslado.Count > 0)
                                oComprobanteImpuestos.Traslados = lstComprobanteImpuestosTraslado.ToArray();

                            if (lstComprobanteImpuestosRetencion.Count > 0)
                                oComprobanteImpuestos.Retenciones = lstComprobanteImpuestosRetencion.ToArray();

                            oComprobante.Impuestos = oComprobanteImpuestos;
                        }
                        Registralog("GenerXMLEnvia", pFOLIO, Operacion, "SE GERARON CONCEPTOS Y DETALLES");

                        oComprobante.Addenda = oComprobanteAddenda;

                        diverza oDiverza = new diverza();
                        diverzaGenerales oDiverzaGenerales = new diverzaGenerales();
                        diverzaEmisor oDiverzaEmisor = new diverzaEmisor();
                        diverzaClavesDescripcion oDiverzaClavesDescripcion = new diverzaClavesDescripcion();
                        diverzaConceptos oDiverzaConceptos = new diverzaConceptos();
                        diverzaReceptor oDiverzaReceptor = new diverzaReceptor();
                        ubicacion oDiverzaEmisorDomicilioFiscalE = new ubicacion();
                        datos_Contacto oDiverzaEmisorDatosContactoE = new datos_Contacto();
                        ubicacion oDiverzaReceptorDomicilioFiscalR = new ubicacion();
                        datos_Contacto oDiverzaReceptorDatosContactoR = new datos_Contacto();
                        List<extra> lstDiverzaComplemento = new List<extra>();
                        List<diverzaConceptosConcepto> lstDiverzaConceptosConcepto = new List<diverzaConceptosConcepto>();

                        List<XmlDocument> lstxmlelemntoAdd = new List<XmlDocument>();
                        XmlDocument xmlelemntoAdd = new XmlDocument();
                        //' ADENDA DEPENDERA DEL TIPO DE DOCUMENTO..
                        XmlDocument xmlDoc = new XmlDocument();

                        //string path = "c:\\xml33\\";
                        //StreamWriter w1 = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                        ////w.Write("\r\nLog Entry : ");
                        //w1.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        //    DateTime.Now.ToLongDateString());
                        //w1.WriteLine("Linea 2411 " + Continuar.ToString()+ " ----" + IncluirAddendaBuzonFiscal(addenda[0]["ClaveCliente"].ToString(), Empresa).ToString());
                        //w1.WriteLine("-------------------------------");
                        //w1.Close();

                        if (IncluirAddendaBuzonFiscal(addenda[0]["ClaveCliente"].ToString(), Empresa))
                        {

                            //StreamWriter w5 = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                            ////w.Write("\r\nLog Entry : ");
                            //w5.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                            //    DateTime.Now.ToLongDateString());
                            //w5.WriteLine("Linea 2422 " + Continuar.ToString() + " ---- " + addenda[0]["ClaveCliente"].ToString() + " ---- " + Empresa);
                            //w5.WriteLine("-------------------------------");
                            //w5.Close();

                            oDiverza.generales = oDiverzaGenerales;
                            oDiverza.emisor = oDiverzaEmisor;

                            oDiverzaEmisor.domicilioFiscalE = oDiverzaEmisorDomicilioFiscalE;
                            oDiverzaEmisorDomicilioFiscalE.ciudad = datos[0]["ExpedidoMuncipio"].ToString();
                            oDiverzaEmisorDomicilioFiscalE.calle = datos[0]["ExpedidoCalle"].ToString();
                            oDiverzaEmisorDomicilioFiscalE.numero = datos[0]["ExpedidoNoExterior"].ToString();
                            oDiverzaEmisorDomicilioFiscalE.colonia = datos[0]["Expedidocolonia"].ToString();
                            oDiverzaEmisorDomicilioFiscalE.codigoPostal = datos[0]["ExpedidocodigoPostal"].ToString();
                            oDiverzaEmisorDomicilioFiscalE.municipio = datos[0]["ExpedidoMuncipio"].ToString();
                            oDiverzaEmisorDomicilioFiscalE.estado = datos[0]["Expedidoestado"].ToString();
                            oDiverzaEmisorDomicilioFiscalE.pais = datos[0]["Expedidopais"].ToString();
                            oDiverzaEmisor.datosContactoE = oDiverzaEmisorDatosContactoE;
                            oDiverzaEmisorDatosContactoE.web = datos[0]["web"].ToString();
                            //oDiverzaEmisorDatosContactoE.emailComercial = datos[0]["Email"].ToString();
                            //oDiverzaEmisorDatosContactoE.emailContacto = datos[0]["Expedidofax"].ToString();
                            oDiverzaEmisorDatosContactoE.telefono = datos[0]["ExpedidoTelefono"].ToString();

                            oDiverzaReceptor.domicilioFiscalR = oDiverzaReceptorDomicilioFiscalR;
                            oDiverzaReceptorDomicilioFiscalR.ciudad = datos[0]["DomicilioReceptormunicipio"].ToString();
                            oDiverzaReceptorDomicilioFiscalR.calle = datos[0]["DomicilioReceptorcalle"].ToString();
                            oDiverzaReceptorDomicilioFiscalR.numero = datos[0]["DomicilioReceptornoExterior"].ToString();
                            oDiverzaReceptorDomicilioFiscalR.colonia = datos[0]["DomicilioReceptorcolonia"].ToString();
                            oDiverzaReceptorDomicilioFiscalR.codigoPostal = datos[0]["DomicilioReceptorcodigoPostal"].ToString();
                            oDiverzaReceptorDomicilioFiscalR.municipio = datos[0]["DomicilioReceptormunicipio"].ToString();
                            oDiverzaReceptorDomicilioFiscalR.estado = datos[0]["DomicilioReceptorestado"].ToString();
                            oDiverzaReceptorDomicilioFiscalR.pais = datos[0]["DomicilioReceptorpais"].ToString();
                            oDiverzaReceptor.datosContactoR = oDiverzaReceptorDatosContactoR;
                            oDiverzaReceptorDatosContactoR.web = datos[0]["web"].ToString();
                            //oDiverzaReceptorDatosContactoR.emailComercial = datos[0]["Email"].ToString();
                            oDiverzaReceptorDatosContactoR.telefono = datos[0]["ReceptorTelefono"].ToString();

                            oDiverza.clavesDescripcion = oDiverzaClavesDescripcion;

                            oDiverza.receptor = oDiverzaReceptor;
                            if (Operacion == "FACTURA")
                                oDiverzaGenerales.tipoDocumento = "Factura";
                            else
                                oDiverzaGenerales.tipoDocumento = "Nota de Credito";
                            
                            oDiverzaGenerales.totalConLetra = addenda[0]["TotalConLetra"].ToString();                           
                            oDiverzaGenerales.numeroOrden = addenda[0]["OrdenInterna"].ToString();                            
                            oDiverzaClavesDescripcion.c_FormaPago = datos[0]["sat_desformapago"].ToString();
                            oDiverzaClavesDescripcion.c_Moneda = datos[0]["Moneda"].ToString();
                            if (Operacion == "FACTURA")
                                oDiverzaClavesDescripcion.c_TipoDeComprobante = "I";
                            else
                                oDiverzaClavesDescripcion.c_TipoDeComprobante = "E";

                            oDiverzaClavesDescripcion.c_MetodoPago = datos[0]["sat_desmetodopago"].ToString();
                            oDiverzaClavesDescripcion.c_LugarExpedicion = datos[0]["lugarexpedicion"].ToString();
                            oDiverzaClavesDescripcion.c_RegimenFiscal = datos[0]["sat_desregimenfiscal"].ToString();
                            oDiverzaClavesDescripcion.c_UsoCFDI = datos[0]["sat_desusocfdi"].ToString();
                           // oDiverzaEmisor.vendedor = addenda[0]["AgenteVEntas"].ToString();

                            oDiverzaReceptor.numCliente = addenda[0]["ClaveCliente"].ToString();

                            oDiverza.conceptos = oDiverzaConceptos;

                            oDiverzaConceptos.numeroConceptos = "1";
                            //ReDim oDiverza.conceptos.concepto[1];                    
                            //oDiverzaConceptosConcepto.identificador1 = "ID1";
                            //oDiverzaConceptosConcepto.mensaje = addenda[0]["PorConducto"].ToString();// '+ " " + datos[0]["fechaspublicacion").ToString()
                            lstDiverzaConceptosConcepto.Add(new diverzaConceptosConcepto()
                            {
                                identificador1 = "ID1",
                                mensaje = addenda[0]["PorConducto"].ToString()
                            });
                            oDiverzaConceptos.concepto = lstDiverzaConceptosConcepto.ToArray();



                            //'nota validar como agregar este:
                            //'If datos[0]["totalImpuestosRetenidos") > 0 And ParamArticuloesp25 <> datos[0]["primerArticulo").ToString().Trim And Empresa.Trim.ToUpper <> "SEY" Then
                            //'    '------------------------ -

                            //'    oDiverza.Complemento[0] = oDiverzaComplemento
                            //'    oDiverzaComplemento.atributo = "LERETENIDO"
                            //'    oDiverzaComplemento.valor = "IMPUESTO RETENIDO DE CONFORMIDAD CON LA LEY DEL IMPUESTO AL VALOR AGREGADO"
                            //'    '--------------------------------
                            //'End If

                            int ContM = 0;
                            if (UsaReferencia)
                                if (Operacion == "FACTURA")// '--Empresa.Trim.ToUpper = "MMY" Then
                                    if (CFDIncluirRefBancaria.ToUpper().IndexOf(Empresa.Trim().ToUpper()) > 0)
                                    {
                                        //ReDim oDiverza.complemento(1)
                                        lstDiverzaComplemento.Add(new extra()
                                        {
                                            atributo = "LeyendaEspecial1",
                                            valor = "FAVOR DE EFECTUAR SU DEPOSITO EN BANORTE, NO. EMPRESA: " + addenda[0]["referenciaempresa"].ToString() + "  REFERENCIA: " + addenda[0]["RefBancaria"].ToString()
                                        });
                                        oDiverza.complemento = lstDiverzaComplemento.ToArray();
                                        ContM = ContM + 1;
                                    }

                            //'If addenda[0]("Mensaje1").ToString().Trim <> "" Then
                            //'    ReDim oDiverza.complemento(1)
                            //'    oDiverza.Complemento[0] = oDiverzaComplemento
                            //'    oDiverzaComplemento.valor = addenda[0]("Mensaje1").ToString()
                            //'    If ContM = 1 Then
                            //'        oDiverzaComplemento.atributo = "LeyendaEspecial2"
                            //'    Else
                            //'        oDiverzaComplemento.atributo = "LeyendaEspecial1"
                            //'    End If
                            //'End If


                            //'If addenda[0]("Mensaje2").ToString().Trim <> "" Then

                            //'    ReDim oDiverza.complemento(1)
                            //'    oDiverza.complemento(1) = oDiverzaComplemento
                            //'    oDiverzaComplemento.valor = addenda[0]("Mensaje2").ToString()

                            //'    If ContM = 1 Then
                            //'        oDiverzaComplemento.atributo = "LeyendaEspecial3"
                            //'    Else
                            //'        oDiverzaComplemento.atributo = "LeyendaEspecial2"
                            //'    End If


                            //'End If


                            //'If addenda[0]("Mensaje3").ToString().Trim <> "" And ContM = 0 Then

                            //'    ReDim oDiverza.complemento(1)
                            //'    oDiverza.Complemento[0] = oDiverzaComplemento
                            //'    oDiverzaComplemento.valor = addenda[0]("Mensaje3").ToString()
                            //'    oDiverzaComplemento.atributo = "LeyendaEspecial3"
                            //'End If

                            string strADD;

                            XmlSerializer Addxml = new XmlSerializer(oDiverza.GetType());
                            System.IO.MemoryStream memoria1 = new System.IO.MemoryStream();
                            Addxml.Serialize(memoria1, oDiverza);


                            strADD = memoria1.ToString();

                            memoria1.Position = 0;
                            System.IO.StreamReader sr = new System.IO.StreamReader(memoria1);
                            strADD = sr.ReadToEnd();

                            strADD = strADD.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:tdCFDI=\"http://www.sat.gob.mx/sitio_internet/cfd/tipoDatos/tdCFDI\" ");
                            strADD = strADD.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "xsi:schemaLocation=\"http://www.diverza.com/ns/addenda/diverza/1 file:/Users/osvaldosanchez/Documents/DIVERZA/Addenda_Diverza_v1.1.xsd\"");
                            //''strADD = strADD.Replace("<INE", "<ine:INE")
                            //''strADD = strADD.Replace("</INE>", "</ine:INE>")
                            //''strADD = strADD.Replace("<Entidad", "<ine:Entidad")
                            //''strADD = strADD.Replace("</Entidad>", "</ine:Entidad>")

                            //''strADD = strADD.Replace("<Contabilidad", "<ine:Contabilidad")
                            //''strADD = strADD.Replace("</Contabilidad>", "</ine:Contabilidad>")
                            //''strADD = strADD.Replace("<?xml version=""1.0""?>", "")
                            //'</INE>
                            xmlelemntoAdd.LoadXml(strADD);
                            memoria1.Close();


                            contAdd = 1;
                        }
                        else
                            contAdd = 0;
                        //Checar por que solo se manda un xmlelemntoAdd pero cuando es countADD no se hace caso
                        if (datos[0]["UsaAddenda"].ToString() == "S")
                            contAdd = contAdd + 1;

                        //ReDim oComprobanteAddenda.Any(contAdd)

                        oComprobanteAddenda.Any = new XmlElement[contAdd];
                        //' On Error Resume Next
                        //StreamWriter w2 = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                        ////w.Write("\r\nLog Entry : ");
                        //w2.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        //    DateTime.Now.ToLongDateString());
                        //w2.WriteLine("Linea 2603" + xmlelemntoAdd.DocumentElement.ToString());
                        //w2.WriteLine("-------------------------------");
                        //w2.Close();

                        oComprobanteAddenda.Any[0] = xmlelemntoAdd.DocumentElement;
                        //oComprobanteAddenda.Any(0) = xmlelemntoAdd.DocumentElement;
                        //'xmlns:soriana="http://www2.soriana.com/integracion">
                        string TXTAddendas;

                        //'*************************Para manejo de addendas
                        tieneAddendaSoriana = false;
                        if (datos[0]["UsaAddenda"].ToString() == "S")
                        {

                            DataView dvAdd = new DataView();
                            XmlDocument xmlDocdAddenda = new XmlDocument();
                            dvAdd = r.GetDataView("CfdiGeneraAddendaSoriana " + iddocto);
                            if (dvAdd.Count > 0)
                            {
                                if (dvAdd[0]["Documento"].ToString() == "" || dvAdd[0]["Documento"].ToString().Length < 120)
                                {
                                    RegistraError(IDunico, "El documento no tiene addenda capturada", "Addenda");
                                    RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "SinAddenda");
                                    //GoTo Salir
                                }
                                //'LA ADDENDA SORIANA SE PEGARA AL FINAL PORQUE ESTA MARCANDO ERROR
                                //'LAS SIGUIENTES 4 LINEAS SE COMENTARIAN Y NO SE ENVIARIA LA ADDENDA DE SORIANA, ESTA SE PEGARIA HASTA EL FINAL mvr 20180118
                                else
                                {
                                    TXTAddendas = dvAdd[0]["Documento"].ToString().Replace("xmlns=\"http://www.soriana.mx/Apps/v-Fact/Addendas/Emisor___Receptor___Soriana\"", "");
                                    TXTAddendas = TXTAddendas.Replace("<DSCargaRemisionProv >", "<DSCargaRemisionProv xmlns=\"http://www.soriana.mx/Apps/v-Fact/Addendas/Emisor___Receptor___Soriana\">");
                                    xmlDocdAddenda.LoadXml(TXTAddendas);
                                    //oComprobanteAddenda.Any[1] = xmlDocdAddenda.FirstChild;//      'XmlElement(Replace(dvAdd[0]("Documento").ToString(), "xmlns:soriana=""http://www2.soriana.com/integracion""", ""));
                                    oComprobanteAddenda.Any[1].AppendChild(xmlDocdAddenda.FirstChild);
                                    tieneAddendaSoriana = true;
                                }
                            }
                            else
                            {
                                RegistraError(IDunico, "El documento no tiene addenda capturada", "Addenda");
                                RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "SinAddenda");
                                //GoTo Salir
                            }
                        }
                        //'PARA LAS ADDENDAS QUE SE PEGAN COMO COMPLEMENTOS
                        if ((datos[0]["UsaAddenda"].ToString() == "C" || datos[0]["ClienteFamilia"].ToString().Trim() == "INE") && Operacion == "FACTURA")
                        {
                            // ReDim oComprobanteComplemento.Any(Complementos)
                            //'           ReDim oComprobante.Complemento(1).Any(Complementos)

                            Complementos = Complementos + 1;

                            //' On Error Resume Next

                            DataView dvAdd = new DataView();
                            XmlDocument xmlDocdAddenda = new XmlDocument();
                            dvAdd = r.GetDataView("Select Documento from cdfAddendaFactura (Nolock) where IdDocto = " + iddocto);
                            if (dvAdd.Count > 0 && datos[0]["ClienteFamilia"].ToString().Trim() != "INE" && Operacion == "FACTURA")
                            {
                                if (dvAdd[0]["Documento"].ToString() == "" || dvAdd[0]["Documento"].ToString().Length < 120)
                                {
                                    RegistraError(IDunico, "El documento no tiene addenda capturada", "Addenda");
                                    RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "SinAddenda");
                                    //GoTo Salir
                                }
                                else
                                {
                                    lstComprobanteComplemento = new List<ComprobanteComplemento>();
                                    lstComprobanteComplemento.Add(new ComprobanteComplemento());
                                    oComprobante.Complemento = lstComprobanteComplemento.ToArray();

                                    TXTAddendas = dvAdd[0]["Documento"].ToString().Replace("xmlns:detallista=\"detallista\"", "");
                                    TXTAddendas = TXTAddendas.Replace("<detallista:detallista", "<detallista:detallista xmlns:detallista=\"http://www.sat.gob.mx/detallista\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sat.gob.mx/detallista http://www.sat.gob.mx/sitio_internet/cfd/detallista/detallista.xsd\"");
                                    xmlDocdAddenda.LoadXml(TXTAddendas);
                                    //'oComprobante.Complemento(1).Any[0] = xmlDocdAddenda.FirstChild      'XmlElement(Replace(dvAdd[0]("Documento").ToString(), "xmlns:soriana=""http://www2.soriana.com/integracion""", ""))

                                    //oComprobante.Complemento[0] = oComprobanteComplemento;
                                    ////oComprobanteComplemento.Any[0] = xmlDocdAddenda.FirstChild;

                                    //oComprobanteComplemento.Any[0].AppendChild(xmlDocdAddenda.FirstChild);
                                    ////'oComprobanteComplemento.Any[0] = xmlDocdAddenda.FirstChild
                                    oComprobante.Complemento[0].Any[Complementos].AppendChild(xmlDocdAddenda.FirstChild);
                                }
                            }
                            else
                            {
                                //'********    *********
                                //'******** revisamos si es un complemento INE    ********* 

                                if (datos[0]["ClienteFamilia"].ToString().Trim() == "INE")
                                {
                                    //'*************************************
                                    //' SE PONE EL COMPLEMENTO INE
                                    //'***********************************
                                    DataView dvIneE = new DataView();
                                    DataView dvIneEstado = new DataView();
                                    DataView dvINEidCont = new DataView();

                                    dvIneE = r.GetDataView("Select  isnull(E.idContabilidad ,'') as IdContabilidad, tp.Descripcion as TipoProceso, isnull(Tc.descripcion,'') as TipoComite   from INEopEncabezado E, INETipoproceso Tp, INETipoComite tc   Where e.Empresa  ='" + Empresa + "'  and e.Folio ='" + FolioFActura + "' and E.TipoProceso *= Tp.Clave and E.TipoComite *= TC.clave     ");
                                    if (dvIneE.Count > 0)
                                    {
                                        INE CIne = new INE();
                                        CIne.Version = VersionINE;// ' "1.0";
                                        CIne.TipoProceso = fnINETipoProceso(dvIneE[0]["Tipoproceso"].ToString().Trim());
                                        if (dvIneE[0]["Tipoproceso"].ToString() != "")
                                        {
                                            //'DATO REQUERIDO, LEVANTAR AQUI UN ERROR DE ACUERDO A ESPECIFICACION
                                        }
                                        CIne.TipoComiteSpecified = false;


                                        if (dvIneE[0]["TipoComite"].ToString() != "")
                                        {
                                            CIne.TipoComiteSpecified = true;
                                            CIne.TipoComite = fnINETipoComite(dvIneE[0]["TipoComite"].ToString().Trim());
                                        }

                                        CIne.IdContabilidadSpecified = false;

                                        if (CIne.TipoProceso == INETipoProceso.Ordinario)
                                        {
                                            if (CIne.TipoComite == INETipoComite.EjecutivoNacional)
                                            {
                                                if (dvIneE[0]["IdContabilidad"].ToString() != "")
                                                {
                                                    CIne.IdContabilidadSpecified = true;
                                                    int IdContabilidad = 0;
                                                    Int32.TryParse(dvIneE[0]["IdContabilidad"].ToString(), out IdContabilidad);
                                                    CIne.IdContabilidad = IdContabilidad;
                                                }
                                            }
                                        }

                                        INEEntidad Entidad = new INEEntidad();
                                        INEEntidadContabilidad oINEEntidadContabilidad = new INEEntidadContabilidad();
                                        List<INEEntidad> lstEntidad = new List<INEEntidad>();
                                        List<INEEntidadContabilidad> lstINEEntidadContabilidad = new List<INEEntidadContabilidad>();
                                        int Cont2 = 0;
                                        int cont = 0;
                                        string lEstado;
                                        string lAmbito;
                                        dvIneEstado = new DataView();
                                        dvIneEstado = r.GetDataView("Select E.Clave, isnull(A.Descripcion,'') Ambito   from INEopEstado E,   INEAmbito A  Where e.Empresa  ='" + Empresa + "'  and e.Folio ='" + FolioFActura + "' and e.Ambito *= a.clave ");

                                        cont = dvIneEstado.Count;
                                        //ReDim CIne.Entidad(cont);
                                        //for( n = 0 To i -1)
                                        //For ix = 0 To cont -1
                                        //string path3 = "c:\\xml33\\";
                                        //StreamWriter w = File.AppendText(path3 + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                                        ////w.Write("\r\nLog Entry : ");
                                        //w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                                        //    DateTime.Now.ToLongDateString());
                                        //w.WriteLine(" Llego a la linea2722 ");
                                        //w.WriteLine("-------------------------------");
                                        //w.Close();

                                        for (int ix = 0; ix < cont; ix++)
                                        {

                                            lEstado = dvIneEstado[ix]["Clave"].ToString();
                                            lAmbito = dvIneEstado[ix]["Ambito"].ToString();

                                            Entidad = new INEEntidad();
                                            Entidad.ClaveEntidad = fnINEEStado(lEstado);// '  t_ClaveEntidad.CAM
                                            Entidad.AmbitoSpecified = false;
                                            if (CIne.TipoProceso != INETipoProceso.Ordinario)
                                            {
                                                if (lAmbito != "")
                                                {
                                                    Entidad.Ambito = fnINEAmbito(lAmbito);//   'INEEntidadAmbito.Federal
                                                    Entidad.AmbitoSpecified = true;
                                                }
                                            }

                                            dvINEidCont = new DataView();
                                            if (!(CIne.TipoComite == INETipoComite.EjecutivoNacional && CIne.TipoComiteSpecified == true))
                                            {
                                                dvINEidCont = r.GetDataView("Select E.Clave     from INEopidContabilidad E  Where e.Empresa  ='" + Empresa + "'  and e.Folio ='" + FolioFActura + "' and e.Estado = '" + lEstado + "' ");

                                                Cont2 = dvINEidCont.Count;
                                                //ReDim Entidad.Contabilidad(Cont2);
                                                //for iix = 0 To Cont2 -1
                                                for (int iix = 0; iix < Cont2; iix++)
                                                {
                                                    oINEEntidadContabilidad = new INEEntidadContabilidad();

                                                    //Entidad.Contabilidad[iix] = new INEEntidadContabilidad();
                                                    int Clave = 0;
                                                    Int32.TryParse(dvINEidCont[iix]["Clave"].ToString(), out Clave);
                                                    oINEEntidadContabilidad.IdContabilidad = Clave;
                                                    //Entidad.Contabilidad[iix].IdContabilidad = Clave;
                                                    lstINEEntidadContabilidad.Add(oINEEntidadContabilidad);
                                                }
                                                if (Cont2 > 0)
                                                {
                                                    Entidad.Contabilidad = lstINEEntidadContabilidad.ToArray();
                                                }
                                            }
                                            lstEntidad.Add(Entidad);
                                            //CIne.Entidad[ix] = new INEEntidad();
                                            //CIne.Entidad[ix] = Entidad;
                                        }
                                        if (cont > 0)
                                        {
                                            CIne.Entidad = lstEntidad.ToArray();
                                        }

                                        //StreamWriter w2 = File.AppendText(path3 + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                                        ////w.Write("\r\nLog Entry : ");
                                        //w2.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                                        //    DateTime.Now.ToLongDateString());
                                        //w2.WriteLine(" Llego a la linea2775 ");
                                        //w2.WriteLine("-------------------------------");
                                        //w2.Close();

                                        //for (int ix = 0; ix < cont; ix++)
                                        //{

                                        //    lEstado = dvIneEstado[ix]["Clave"].ToString();
                                        //    lAmbito = dvIneEstado[ix]["Ambito"].ToString();

                                        //    Entidad = new INEEntidad();
                                        //    Entidad.ClaveEntidad = fnINEEStado(lEstado);// '  t_ClaveEntidad.CAM
                                        //    Entidad.AmbitoSpecified = false;
                                        //    if (CIne.TipoProceso != INETipoProceso.Ordinario)
                                        //    {
                                        //        if (lAmbito != "")
                                        //        {
                                        //            Entidad.Ambito = fnINEAmbito(lAmbito);//   'INEEntidadAmbito.Federal
                                        //            Entidad.AmbitoSpecified = true;
                                        //        }
                                        //    }

                                        //    dvINEidCont = new DataView();
                                        //    if (!(CIne.TipoComite == INETipoComite.EjecutivoNacional && CIne.TipoComiteSpecified == true))
                                        //    {
                                        //        dvINEidCont = r.GetDataView("Select E.Clave     from INEopidContabilidad E  Where e.Empresa  ='" + Empresa + "'  and e.Folio ='" + FolioFActura + "' and e.Estado = '" + lEstado + "' ");

                                        //        Cont2 = dvINEidCont.Count;
                                        //        //ReDim Entidad.Contabilidad(Cont2);
                                        //        //for iix = 0 To Cont2 -1
                                        //        for (int iix = 0; iix < Cont2; iix++)
                                        //        {
                                        //            Entidad.Contabilidad[iix] = new INEEntidadContabilidad();
                                        //            int Clave = 0;
                                        //            Int32.TryParse(dvINEidCont[iix]["Clave"].ToString(), out Clave);
                                        //            Entidad.Contabilidad[iix].IdContabilidad = Clave;
                                        //        }
                                        //    }
                                        //    CIne.Entidad[ix] = new INEEntidad();
                                        //    CIne.Entidad[ix] = Entidad;
                                        //}

                                        string strCompCINE;

                                        XmlSerializer Complementoxml = new XmlSerializer(CIne.GetType());
                                        MemoryStream memoria1 = new MemoryStream();
                                        Complementoxml.Serialize(memoria1, CIne);

                                        XmlDocument xmlelemntoCine = new XmlDocument();
                                        strCompCINE = memoria1.ToString();

                                        memoria1.Position = 0;
                                        StreamReader sr = new StreamReader(memoria1);
                                        strCompCINE = sr.ReadToEnd();

                                        strCompCINE = strCompCINE.Replace("xmlns=\"http://www.sat.gob.mx/ine\"", "xmlns:ine=\"http://www.sat.gob.mx/ine\" xsi:schemaLocation=\"http://www.sat.gob.mx/sitio_internet/cfd/ine/ine11.xsd\" ");
                                        strCompCINE = strCompCINE.Replace("<INE", "<ine:INE");
                                        strCompCINE = strCompCINE.Replace("</INE>", "</ine:INE>");
                                        strCompCINE = strCompCINE.Replace("<Entidad", "<ine:Entidad");
                                        strCompCINE = strCompCINE.Replace("</Entidad>", "</ine:Entidad>");

                                        strCompCINE = strCompCINE.Replace("<Contabilidad", "<ine:Contabilidad");
                                        strCompCINE = strCompCINE.Replace("</Contabilidad>", "</ine:Contabilidad>");
                                        strCompCINE = strCompCINE.Replace("<?xml version=\"1.0\"?>", "");
                                        //'</INE>
                                        xmlelemntoCine.LoadXml(strCompCINE);
                                        llevacomplementoINE = true;

                                        lstComprobanteComplemento = new List<ComprobanteComplemento>();
                                        ComprobanteComplemento obComprobanteComplemento = new ComprobanteComplemento();


                                        ////oComprobante.Complemento[0] = oComprobanteComplemento;
                                        ////oComprobanteComplemento.Any[0] = xmlelemntoCine.DocumentElement;
                                        // oComprobante.Complemento[0] = oComprobanteComplemento;
                                        //oComprobanteComplemento.Any[0] = xmlelemntoCine.DocumentElement;

                                        obComprobanteComplemento.Any = new XmlElement[1];
                                        obComprobanteComplemento.Any[0]= xmlelemntoCine.DocumentElement;
                                        lstComprobanteComplemento.Add(obComprobanteComplemento);
                                        oComprobante.Complemento = lstComprobanteComplemento.ToArray();
                                        ///'oComprobante.Complemento[0].Any[1] = xmlelemntoCine.DocumentElement ' Complementoxml.  ' xmlDocdAddenda.FirstChild
                                        memoria1.Close();
                                    }
                                    else
                                    {
                                        if (Operacion == "FACTURA")
                                        {
                                            RegistraError(IDunico, "El documento no tiene complemento capturado", "Complemento");
                                            RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "SinAddenda");
                                            //GoTo Salir
                                        }
                                    }
                                }//  '*******************************************************
                                else
                                {
                                    RegistraError(IDunico, "El documento no tiene addenda capturada", "Addenda");
                                    RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "SinAddenda");

                                    //GoTo Salir
                                }
                            }
                        }

                        //string path3 = "c:\\xml33\\";
                        //StreamWriter w = File.AppendText(path3 + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                        ////w.Write("\r\nLog Entry : ");
                        //w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        //    DateTime.Now.ToLongDateString());
                        //w.WriteLine(" Llego a la linea2928" + oComprobante.TipoCambio);
                        //w.WriteLine("-------------------------------");
                        //w.Close();
                        XmlSerializer x = new XmlSerializer(oComprobante.GetType());
                        MemoryStream memoria = new MemoryStream();
                        x.Serialize(memoria, oComprobante);
                        memoria.Close();

                        RegistralogX("ARMA_XML", pFOLIO.Trim(), Operacion, "Se ha completado el armado del XML buscar una copia en c:\"Request");

                        idtoken = r.GetP5sistema("idtoken", "gen", Empresa);
                        token = r.GetP5sistema("token", "gen", Empresa);
                        CFDI33certificado = r.GetP5sistema("CFDI33Certificado", "gen", Empresa);
                        string usuario;
                        usuario = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
                        //Err.Clear()

                        XmlSerializer YY = new XmlSerializer(oComprobante.GetType());
                        StreamWriter Xwriter = new StreamWriter(CFDiRutaRequest + pFOLIO + ".XML");
                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                        ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        ns.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
                        YY.Serialize(Xwriter, oComprobante, ns);
                        Xwriter.Close();

                        //'si tiene complemento INE  --reordenamos el squmelocation y los xsi
                        if (llevacomplementoINE)
                            ReordenaComplementoINE(CFDiRutaRequest + pFOLIO.Trim() + ".XML");


                        if ((datos[0]["UsaAddenda"].ToString() == "C" || datos[0]["ClienteFamilia"].ToString().Trim() == "INE") && Operacion == "FACTURA")
                        {
                            ReordenaComplementoLIV(CFDiRutaRequest + pFOLIO.Trim() + ".XML");
                            llevacomplementoINE = true;// ' solo para decir que se trata de un complemento y el json se arme correctamente en la funcion timbradiverza33
                        }
                        //'Esta funcion timbraria  V33
                        //On Error GoTo 0

                        //'Dim Xywriter As New StreamWriter(CFDiRutaRequest + pFOLIO + ".XML")

                        //'ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance")
                        //'ns.Add("cfdi", "http://www.sat.gob.mx/cfd/3")
                        //'YY.Serialize(Xywriter, oComprobante, ns)
                        //'Xwriter.Close()

                        //'Dim Z As New Xml.Serialization.XmlSerializer(oComprobante.GetType)
                        //'Dim writer As New StreamWriter(RutaXMLGraba + "\R_" + pFOLIO + ".XML")
                        //'Z.Serialize(writer, oComprobante)
                        //'writer.Close()
                        //Timbra//      
                        
                        //StreamWriter w1 = File.AppendText(path3 + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                        ////w.Write("\r\nLog Entry : ");
                        //w1.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        //    DateTime.Now.ToLongDateString());
                        //w1.WriteLine(" Llego a la linea2982 " + oComprobante.TipoCambio);
                        //w1.WriteLine("-------------------------------");
                        //w1.Close();
                        TimbraDiverza33(iddocto, llevacomplementoINE);
                        //On Error GoTo Salir

                        //'******************************************************************************************************
                        //'/**** de aqui hasta el final se puede pasar a una funcion y habilitar los on error  resume/goto    **
                        //'/*****************************************************************************************************

                        //'Adaptacion código web service para cfdi 3.3
                        //'Dim MemStream As System.IO.MemoryStream = FileToMemory(CFDiRutaRequest + pFOLIO + ".XML")
                        //'Dim myByteArray As Byte() = MemStream.ToArray()
                        //'Dim Content64 As String
                        //'Content64 = System.Convert.ToBase64String(myByteArray, 0, myByteArray.Length)

                        //'CFDiURL = r.GetP5sistema("CFDiURL", "gen", Empresa)
                        //'Dim request = TryCast(System.Net.WebRequest.Create(CFDiURL), System.Net.HttpWebRequest)
                        //'request.Method = "POST"
                        //'request.Timeout = 3600000


                        //'Dim json, responseFromServer As String
                        //'json = "{""credentials"": {""id"": """ + idtoken + """,""token"": """ + token + """},""issuer"": {""rfc"": """ + RFCemisor + """},""document"": {""ref-id"": """ + IDunico + """,""certificate-number"": """ + CFDI33certificado + """,""section"": ""all"",""format"": ""xml"",""template"": ""letter"",""type"": ""application/vnd.diverza.cfdi_3.3+xml"",""content"": """ + Content64 + """}}"

                        //'Dim byteArray As Byte() = System.Text.Encoding.UTF8.GetBytes(json)

                        //'request.ContentType = "application/json; charset=utf-8"


                        //'Dim dataStream As Stream = request.GetRequestStream()
                        //'dataStream.Write(byteArray, 0, byteArray.Length)
                        //'dataStream.Close()



                        //'Try

                        //'    Dim responseX = TryCast(request.GetResponse(), System.Net.HttpWebResponse)
                        //'    dataStream = responseX.GetResponseStream()
                        //'    ' Open the stream using a StreamReader for easy access.
                        //'    Dim reader As New StreamReader(dataStream)
                        //'    ' Read the content.
                        //'    responseFromServer = reader.ReadToEnd()
                        //'    ' Display the content.
                        //'    ' Clean up the streams.
                        //'    reader.Close()
                        //'    dataStream.Close()
                        //'    responseX.Close()
                        //'Catch ex As WebException


                        //'    If ex.Message.IndexOf("Error en el servidor remoto: (500) Error interno del servidor.") <> -1 Then
                        //'        ErrorResponse = "Error en el servidor remoto: (500) Error interno del servidor."
                        //'    Else
                        //'        If ex.Message.IndexOf("Se termin") <> -1 Then
                        //'            ErrorResponse = "SE TERMINO EL TIEMPO DE ESPERA"
                        //'        Else
                        //'            Using responseStream As IO.Stream = ex.Response.GetResponseStream()
                        //'                Using responseReader As New IO.StreamReader(responseStream)
                        //'                    ErrorResponse = responseReader.ReadToEnd()


                        //'                End Using

                        //'            End Using
                        //'        End If
                        //'    End If
                        //'End Try


                        //'If ErrorResponse <> "" Then
                        //'    'Si hubo error
                        //'If ErrorResponse = "SE TERMINO EL TIEMPO DE ESPERA" Or ErrorResponse = "Error en el servidor remoto: (500) Error interno del servidor." Then
                        //'    RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "SE TERMINO EL TIEMPO DE ESPERA")
                        //'    RegistraError(IDunico, "AQUI VA EL MENSAJE DE RESPUESTA", "SE TERMINO EL TIEMPO DE ESPERA")
                        //'    GoTo Salir

                        //'Else
                        //'    Etiqueta.Text = "Error de codigo CFDi: " + pFOLIO + " json : " + json
                        //'    If TiempoEspera > 0 Then
                        //'        System.Threading.Thread.Sleep(TiempoEspera + 50)
                        //'    End If

                        //'    Dim serializador As New System.Web.Script.Serialization.JavaScriptSerializer
                        //'    Dim diccionario As Dictionary(Of String, Object) = _
                        //'    CType(serializador.DeserializeObject(ErrorResponse), Dictionary(Of String, Object))
                        //'    Dim jsonError As String
                        //'    jsonError = diccionario("message").ToString()() + " detalles : " + diccionario("error_details").ToString()()
                        //'    RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, jsonError)
                        //'    RegistraError(IDunico, "AQUI VA EL MENSAJE DE RESPUESTA", jsonError)
                        //'    GoTo Salir
                        //'End If
                        //'Else
                        //'    'Exito SE PUDO PROCESAR EL CFDi
                        //'    Etiqueta.Text = "Recibiendo Documento CFDI 3.3 con éxito:" + pFOLIO
                        //'    Me.Refresh()
                        //'    Registralog("GenerXMLEnvia", pFOLIO, Operacion, "ARCHIVO ENVIADO CON EXITO")
                        //'    RegistralogX("GenerXMLEnvia", pFOLIO, Operacion, "Se timbro el documento: " + Date.Now.ToString())

                        //'    Dim serializador As New System.Web.Script.Serialization.JavaScriptSerializer
                        //'    Dim diccionario As Dictionary(Of String, Object) = _
                        //'    CType(serializador.DeserializeObject(responseFromServer), Dictionary(Of String, Object))

                        //'    ProcesaRespuesta(diccionario("content").ToString()(), iddocto)

                        //'End If


                        //'/*********************************************************************************************************
                        //'/**********************Hasta aqui se pasaria a una nueva funcion 
                        //'/********************************************************************************************************



                        //'*******  MVR 20180118 esto tendria que ser probando en conjunto con el envio de la addende de SORINA
                        //'If tieneAddendaSoriana = True Then
                        //'PegaAddendaSoriAna(iddocto, RutaXMLGraba + "\" + Trim(pFOLIO) + ".XML")
                        //'
                        //'End If
                        //'
                        //'request = Nothing
                        //'response = Nothing




                    }
                    else
                    {
                        //StreamWriter w2 = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                        ////w.Write("\r\nLog Entry : ");
                        //w2.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        //    DateTime.Now.ToLongDateString());
                        //w2.WriteLine("PAso RegistraError " + iddocto.ToString() + ", " + MensajeError + " , GenerXMLEnvia");
                        //w2.WriteLine("-------------------------------");
                        //w2.Close();
                        //RegistraError(IDunico, MensajeError, "GenerXMLEnvia");
                    }
                    //
                }
            }
            catch (Exception ex)
            {
                RegistraError(IDunico, "Generar XML : " + ex.ToString(), "GenerXMLEnvia");

                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Fichero" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();

                this.Etiqueta.Text = "GenerXMLEnvia: " + ex.ToString();
                System.Threading.Thread.Sleep(2000);
            }
            finally
            {
                this.EtiquetaSQL.Text = "";
                this.Etiqueta.Text = "";
                //    Salir:
                //              On Error GoTo 0
                //            'Liberamos los objetos locales
                if (TiempoEspera > 0)
                    System.Threading.Thread.Sleep(TiempoEspera + 50);

                //' cResponse = Nothing
                GC.Collect();
                if (Operacion != "FACTURA")
                {
                    System.Threading.Thread.Sleep(500);
                    GC.Collect();
                }
            }
        }
        private bool IncluirAddendaBuzonFiscal(string lCliente, string lEmpresa)
        {
            //'MCRV PARA PRUEBAS
            bool Resultado = true;
            int intparse;
            DataView dvc = new DataView();

            dvc = r.GetDataView("Select count(*) x from cfdiClienteNoAddendaBuzonFiscal (Nolock) where cliente = '" + lCliente + "' and empresa ='" + lEmpresa + "' ");
            Int32.TryParse(dvc[0]["x"].ToString(), out intparse);

            if (intparse == 0)
                Resultado = true;
            else
                Resultado = false;

            dvc = new DataView();

            return Resultado;
        }
        private void ReordenaComplementoINE(string archivo)
        {
            //'xmlns:cfdi="http://www.sat.gob.mx/cfd/3" xmlns:ine="http://www.sat.gob.mx/ine" 
            XmlDocument xml33 = new XmlDocument();
            string XMLenString;
            xml33.Load(archivo);
            XMLenString = xml33.InnerXml.ToString();
            XMLenString = XMLenString.Replace("<ine:INE xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "<ine:INE ");
            //'quitar
            XMLenString = XMLenString.Replace("xmlns:ine=\"http://www.sat.gob.mx/ine\"", "");
            XMLenString = XMLenString.Replace("xsi:schemaLocation=\"http://www.sat.gob.mx/sitio_internet/cfd/ine/ine11.xsd\"", "");

            //'poner en encabezado
            XMLenString = XMLenString.Replace("xmlns:cfdi=\"http://www.sat.gob.mx/cfd/3\"", "xmlns:cfdi=\"http://www.sat.gob.mx/cfd/3\" xmlns:ine=\"http://www.sat.gob.mx/ine\"");
            XMLenString = XMLenString.Replace("http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd", "http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd http://www.sat.gob.mx/ine http://www.sat.gob.mx/sitio_internet/cfd/ine/INE11.xsd");

            xml33.LoadXml(XMLenString);

            xml33.Save(archivo);
        }
        private void ReordenaComplementoLIV(string archivo)
        {
            XmlDocument xml33 = new XmlDocument();
            string XMLenString;
            xml33.Load(archivo);
            XMLenString = xml33.InnerXml.ToString();

            XMLenString = XMLenString.Replace("<detallista:detallista xmlns:detallista=\"http://www.sat.gob.mx/detallista\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sat.gob.mx/detallista http://www.sat.gob.mx/sitio_internet/cfd/detallista/detallista.xsd\"", "<detallista:detallista ");
            //'quitar
            XMLenString = XMLenString.Replace("xmlns:detallista=\"http://www.sat.gob.mx/detallista\"", "");
            XMLenString = XMLenString.Replace("xsi:schemaLocation=\"http://www.sat.gob.mx/detallista http://www.sat.gob.mx/sitio_internet/cfd/detallista/detallista.xsd\"", "");

            //'poner en encabezado
            XMLenString = XMLenString.Replace("xmlns:cfdi=\"http://www.sat.gob.mx/cfd/3\"", "xmlns:cfdi=\"http://www.sat.gob.mx/cfd/3\" xmlns:detallista=\"http://www.sat.gob.mx/detallista\"");
            XMLenString = XMLenString.Replace("http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd", "http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd http://www.sat.gob.mx/detallista http://www.sat.gob.mx/sitio_internet/cfd/detallista/detallista.xsd");


            xml33.LoadXml(XMLenString);


            xml33.Save(archivo);
        }
        private INETipoProceso fnINETipoProceso(string LTipoProceso)
        {
            //int resultado = 0;
            LTipoProceso = LTipoProceso.ToUpper().Trim();
            switch (LTipoProceso)
            {
                case "CAMPAÑA":
                    return INETipoProceso.Campana;

                case "PRECAMPAÑA":
                    return INETipoProceso.Precampana;

                case "ORDINARIO":
                    return INETipoProceso.Ordinario;
                default:
                    return 0;
            }
        }
        private t_ClaveEntidad fnINEEStado(string lEstado)
        {
            lEstado = lEstado.Trim().ToUpper();
            switch (lEstado)
            {
                case "AGU":
                    return t_ClaveEntidad.AGU;

                case "BCN":
                    return t_ClaveEntidad.BCN;

                case "BCS":
                    return t_ClaveEntidad.BCS;

                case "CAM":
                    return t_ClaveEntidad.CAM;

                case "CDM":
                    return t_ClaveEntidad.DIF;

                case "CHH":
                    return t_ClaveEntidad.CHH;

                case "CHP":
                    return t_ClaveEntidad.CHP;

                case "COA":
                    return t_ClaveEntidad.COA;

                case "COL":
                    return t_ClaveEntidad.COL;

                case "DUR":
                    return t_ClaveEntidad.DUR;

                case "GRO":
                    return t_ClaveEntidad.GRO;

                case "GUA":
                    return t_ClaveEntidad.GUA;

                case "HID":
                    return t_ClaveEntidad.HID;

                case "JAL":
                    return t_ClaveEntidad.JAL;

                case "MEX":
                    return t_ClaveEntidad.MEX;

                case "MIC":
                    return t_ClaveEntidad.MIC;

                case "MOR":
                    return t_ClaveEntidad.MOR;

                case "NAY":
                    return t_ClaveEntidad.NAY;

                case "NLE":
                    return t_ClaveEntidad.NLE;

                case "OAX":
                    return t_ClaveEntidad.OAX;

                case "PUE":
                    return t_ClaveEntidad.PUE;

                case "QTO":
                    return t_ClaveEntidad.QTO;

                case "ROO":
                    return t_ClaveEntidad.ROO;

                case "SIN":
                    return t_ClaveEntidad.SIN;

                case "SLP":
                    return t_ClaveEntidad.SLP;

                case "SON":
                    return t_ClaveEntidad.SON;

                case "TAB":
                    return t_ClaveEntidad.TAB;

                case "TAM":
                    return t_ClaveEntidad.TAM;

                case "TLA":
                    return t_ClaveEntidad.TLA;

                case "VER":
                    return t_ClaveEntidad.VER;

                case "YUC":
                    return t_ClaveEntidad.YUC;

                case "ZAC":
                    return t_ClaveEntidad.ZAC;

                default:
                    return 0;
            }
        }
        private INETipoComite fnINETipoComite(string lTipoComite)
        {
            lTipoComite = lTipoComite.ToUpper().Trim();
            switch (lTipoComite)
            {
                case "COMITE EJECUTIVO ESTATAL":
                    return INETipoComite.EjecutivoEstatal;
                case "COMITE EJECUTIVO NACIONAL":
                    return INETipoComite.EjecutivoNacional;
                case "DIRECTIVO ESTATAL":
                    return INETipoComite.DirectivoEstatal;
                default:
                    return 0;
            }
        }
        private INEEntidadAmbito fnINEAmbito(string lAmbito)
    { lAmbito = lAmbito.ToUpper().Trim();
            switch (lAmbito)
            {
            case "FEDERAL":
                    return INEEntidadAmbito.Federal;
            case "LOCAL":
                    return INEEntidadAmbito.Local;
                default:
                    return 0;
            }
    }
       private static Double VBVal(string sInput)
        {
            Double number;
            string sOutput = string.Empty;
            MatchCollection oMatches = Regex.Matches(sInput, "\\d+(.\\d+)?");

            foreach (Match oMatch in oMatches)
            {
                Console.WriteLine(oMatch);
                sOutput += oMatch.ToString();
            }
            Double.TryParse(sOutput, out number);
            return number;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Timer1.Enabled = false;
            Timer2.Enabled = false;
            Segundos = 0;
            Label9.Text = "Segundos en espera:" + Segundos.ToString();
            this.Refresh();

        } 
        private void frmPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {            
          e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
        }
        private void NotifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }
        private void maximizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }
        private void cerrarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.ExitThread();
        }
        private void button3_Click(object sender, EventArgs e)
        {

        }

       private void ConsultaCancelacionSATFactura(  string idDocto)
        {
            EscribeBitcora("==Consultando Estatus en SAT Factura: " + pFOLIO);
            string uuid = "";
            string empresa, rfc_receptor, total_cfdi, id , token = "";
            string operacion = "Factura";

            DataView DVDatosFactura = new DataView();
            DataView DVInsertarBitacora = new DataView();
            DVDatosFactura = r.GetDataView("exec [dbo].[uspGeneraDatosConsultaEstatusCancelacion] @IDDocto = '" + idDocto + "'");
            empresa = DVDatosFactura[0]["empresa"].ToString();
            uuid = DVDatosFactura[0]["uuid"].ToString();

            rfc_receptor = DVDatosFactura[0]["rfc_receptor"].ToString();
            total_cfdi = DVDatosFactura[0]["total_cfdi"].ToString();
            id = DVDatosFactura[0]["idtoken"].ToString();
            token = DVDatosFactura[0]["token"].ToString();
            //llamado del servicio.
            //'Adaptacion código web service para cfdi 3.3
            //MemoryStream MemStream = FileToMemory(CFDiRutaRequest + pFOLIO + ".XML");
            //Byte[] myByteArray = MemStream.ToArray();



            CFDiURL = r.GetP5sistema("cfdiURLCancelaConEstatus", "gen", empresa);
            CFDiURL = CFDiURL.Replace("uuid", uuid);
            //object request = TryCast(System.Net.WebRequest.Create(CFDiURL), System.Net.HttpWebRequest)
            WebRequest request = HttpWebRequest.Create(CFDiURL);
            request.Method = "PUT";
            request.Timeout = 3600000;


            string json;
            string responseFromServer = "";
            json = "{\"document\": {\"rfc_receptor\": \"" + rfc_receptor + "\",\"total_cfdi\": \"" + total_cfdi + "\"},\"credentials\": {\"id\": \"" + id + "\",\"token\": \"" + token +  "\"}}";
 

            //'****** archivo con el json
            string path1 = CFDiRutaRequest + pFOLIO + ".json";
            FileStream fs1 = File.Create(path1);
            Byte[] info1 = new UTF8Encoding(true).GetBytes(json);
            fs1.Write(info1, 0, info1.Length);
            fs1.Close();
            //'**** fin archivo con json

            Byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentType = "application/json; charset=utf-8";
            //'request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            try
            {
                //   Dim responseX = TryCast(request.GetResponse(), System.Net.HttpWebResponse)
                HttpWebResponse responseX = (HttpWebResponse)request.GetResponse();
                dataStream = responseX.GetResponseStream();
                //' Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                //' Read the content.  
                responseFromServer = reader.ReadToEnd();
                //' Display the content.  
                //' Clean up the streams.  
                reader.Close();
                dataStream.Close();
                responseX.Close();
            }
            catch (WebException ex)
            {
                MessageBox.Show("Error al intentar buscar el estatus de la factura " + ex.ToString());

                if (ex.Message.IndexOf("Error en el servidor remoto: (500) Error interno del servidor.") != -1)
                    ErrorResponse = "Error en el servidor remoto: (500) Error interno del servidor.";
                else
                    if (ex.Message.IndexOf("Se termin") != -1)
                    ErrorResponse = "SE TERMINO EL TIEMPO DE ESPERA";
                else
                {
                    using (var response = (HttpWebResponse)ex.Response)
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                            {
                                ErrorResponse = reader.ReadToEnd();
                            }
                        }
                    }
                }
                //using (Stream responseStream = ex.Response.GetResponseStream())
                //{
                //    using (StreamReader responseReader = new StreamReader(responseStream))
                //    {
                //        ErrorResponse = responseReader.ReadToEnd();
                //    }
                //}
                string path = "c:\\xml33\\";
                StreamWriter w = File.AppendText(path + "Bitacoraconsultaestatusfacturaspendcacelar" + DateTime.Now.ToString("ddMMyyyy") + ".log");
                //w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  {0}", ex.ToString());
                w.WriteLine("-------------------------------");
                w.Close();
            }
            try
            {

            
            //Si no existió Error deserializamos la respuesta
            Etiqueta.Text = "Diverza Contesto:" + pFOLIO;
            System.Web.Script.Serialization.JavaScriptSerializer serializador = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> diccionario = ((Dictionary<string, object>)(serializador.DeserializeObject(responseFromServer)));//responseFromServer//ErrorResponse

                string Respuesta = "" ;
                string estatus_cancelacion = "";
                if ( diccionario["estatus_cancelacion"] == null)
                {
                    Respuesta = diccionario["es_cancelable"].ToString();
                    estatus_cancelacion = diccionario["es_cancelable"].ToString();
                }
                else
                {
                    Respuesta = diccionario["estatus_cancelacion"].ToString();
                    estatus_cancelacion = diccionario["estatus_cancelacion"].ToString();
                }

                
                EscribeBitcora("== Estatus cancelación: "+ pFOLIO +      " :"+ Respuesta);
            string validacion_efos = diccionario["validacion_efos"].ToString();

           
                
            string estado = diccionario["estado"].ToString();

                Etiqueta.Text = "Estatus de la cancelación:" + pFOLIO + "Es: "+ Respuesta;

            if(Respuesta== "Cancelado sin aceptación" || Respuesta == "Cancelado con aceptación" || Respuesta== "Plazo vencido")
            {
                    

                r.GetDataView(" update cfdEncabezadoCancela set Estatus='C' WHERE IdDocto=" + idDocto);
                r.GetDataView(" update cfdfactura set Estatus='C' WHERE IdDocto=" + idDocto);
                    EscribeBitcora("Se  cancela factura: " + pFOLIO + "cfdencabezado y cfdfactura");
 DVInsertarBitacora = r.GetDataView("exec [dbo].[uspBitacoraCancelacion] '" + idDocto + "', '" + empresa + "', '" + operacion + "', '" + pFOLIO + "',' " + validacion_efos + "', '" + estatus_cancelacion + " Estado Actual de la factura: "+ estado+ "'");
                }
                else
                {
                    DVInsertarBitacora = r.GetDataView("exec [dbo].[uspBitacoraCancelacion] '" + idDocto + "', '" + empresa + "', '" + operacion + "', '" + pFOLIO + "',' " + validacion_efos + "', '" + estatus_cancelacion + "'");
                }
                if (Respuesta == "Solicitud rechazada" || Respuesta== "No cancelable") //si la solicitud fue rechazada se pone en X
                {
                    r.GetDataView(" update cfdEncabezadoCancela set Estatus='X' WHERE IdDocto=" + idDocto);
                    DVInsertarBitacora = r.GetDataView("exec [dbo].[uspBitacoraCancelacion] '" + idDocto + "', '" + empresa + "', '" + operacion + "', '" + pFOLIO + "',' " + validacion_efos + "', '" + estatus_cancelacion + "'");
                }


            //Insertamos en la bitacora el resultado de la consulta
             

            Etiqueta.Text = "Insertado en bitacora:" + pFOLIO + "Es: " + Respuesta;
            }
            catch(Exception e)
            {
                EscribeBitcora("Error al recibir Datos de Diverz al consultar cancelación: " + pFOLIO + " - " + e.Message.ToString());
            }
        }


        private void EscribeBitcora( string Mensaje)
        {
            richTextBox1.Text = richTextBox1.Text + System.Environment.NewLine + Mensaje.ToString();
            richTextBox1.Refresh();
         }

        private void VistaInicial_Load(object sender, EventArgs e)
        {

        }
    }





}
