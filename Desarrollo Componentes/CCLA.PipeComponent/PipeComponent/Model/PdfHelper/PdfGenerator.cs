//version Cristian

using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PipeComponent.Model.PdfHelper
{
    public enum DocumentType
    {
        //Eliminar una vez finalizado de lo Caja los Andes
        EstadoPagoPeajes,
        TransitosFacturados,
        TransitosFacturados_newPage,
        TransitosNoFacturados,
        TransitosNoFacturados_newPage,

        //Caja los Andes
        CertificadoEducacion,
        CertificadoEducacionCarga,
        CertificadoSalud,
        CertificadoSaludCarga,
        CertificadoFines,
        CertificadoFinesCarga,
        CertificadoCargaVigente,
        CertificadoCargaPendiente,
        CertificadoCargaPendiente2,
        CertificadoCargaEstado, 

        LicenciaDetalle, 
        LiquidacionPago

    }

    public class PdfGenerator
    {
        public string CreateDocument(DocumentType documentType, string data, string templateName, string filename)
        {
            string destinationName, path;

            //destinationName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            destinationName = Path.Combine(@"C:\TEMP\pdfCCLA", filename);
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "htmlTemplates", templateName);

            return CreateDocumentFromHtml(path, destinationName, data, documentType);
        }

        public string CreateDocumentHorizontal(DocumentType documentType, string data, string templateName, string filename)
        {
            string destinationName, path;

            //destinationName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            destinationName = Path.Combine(@"C:\TEMP\pdfCCLA", filename);
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "htmlTemplates", templateName);

            return CreateDocumentFromHtmlHorizontal(path, destinationName, data, documentType);
        }

        private string CreateDocumentFromHtml(string templatePath, string filename, string data, DocumentType documentType)
        {
            string template = File.ReadAllText(templatePath);
            byte[] pdfByteArray;

            using (var ms = new MemoryStream())
            using (var doc = new Document())
            using (var writer = PdfWriter.GetInstance(doc, ms))
            {
                doc.Open();
                ReplaceVariables(ref template, data, documentType);

                using (var srHtml = new StringReader(template))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                }

                doc.NewPage();
                doc.Close();
                pdfByteArray = ms.ToArray();
                File.WriteAllBytes(filename, pdfByteArray);
                return filename;
            }
        }

        private string CreateDocumentFromHtmlHorizontal(string templatePath, string filename, string data, DocumentType documentType)
        {
            string template = File.ReadAllText(templatePath);
            byte[] pdfByteArray;

            using (var ms = new MemoryStream())
            using (var doc = new Document(iTextSharp.text.PageSize.A4.Rotate(), 10, 10, 10, 10))
            using (var writer = PdfWriter.GetInstance(doc, ms))
            {
                doc.Open();
                ReplaceVariables(ref template, data, documentType);

                using (var srHtml = new StringReader(template))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                }

                doc.NewPage();
                doc.SetPageSize(PageSize.A4.Rotate());
                doc.Close();
                pdfByteArray = ms.ToArray();
                File.WriteAllBytes(filename, pdfByteArray);
                return filename;
            }
        }

        private void ReplaceVariables(ref string html, string data, DocumentType documentType)
        {
            switch (documentType)
            {
                // caja los andes
                // certificado de educación sin carga
                case DocumentType.CertificadoEducacion:
                    ReplaceVariable_CertificadoEducacion(ref html, data);
                    break;

                // certificado de educacion con carga
                case DocumentType.CertificadoEducacionCarga:
                    ReplaceVariables_CertificadoEducacionCarga(ref html, data);
                    break;

                // certificado de salud sin carga
                case DocumentType.CertificadoSalud:
                    ReplaceVariables_CertificadoSalud(ref html, data);
                    break;

                // certificado de salud con carga
                case DocumentType.CertificadoSaludCarga:
                    ReplaceVariables_CertificadoSaludCarga(ref html, data);
                    break;


                // certificado de salud sin carga
                case DocumentType.CertificadoFines:
                    ReplaceVariables_CertificadoFines(ref html, data);
                    break;

                // certificado de salud con carga
                case DocumentType.CertificadoFinesCarga:
                    ReplaceVariables_CertificadoFinesCarga(ref html, data);
                    break;

                // certificado de cargas vigentes
                case DocumentType.CertificadoCargaVigente:
                    ReplaceVariables_CertificadoCargaVigente(ref html, data);
                    break;

                // certificado de cargas pendientes
                case DocumentType.CertificadoCargaPendiente:
                    ReplaceVariables_CertificadoCargaPendiente(ref html, data);
                    break;
                // certificado sin cargas pendientes
                case DocumentType.CertificadoCargaPendiente2:
                    ReplaceVariables_CertificadoCargaPendiente2(ref html, data);
                    break;
                    // detalle de licencias
                case DocumentType.LicenciaDetalle:
                    ReplaceVariables_LicenciaDetalle(ref html, data);
                    break;
                    // liquidacion de pago licencia
                case DocumentType.LiquidacionPago:
                    ReplaceVariables_LiquidacionPago(ref html, data);
                    break;

                /*
            case DocumentType.EstadoPagoPeajes:
                ReplaceVariables_EstadoPagoPeajes(ref html, data);
                break;

            case DocumentType.TransitosFacturados:
                ReplaceVariables_TransitosFacturados(ref html, data);
                break;
            case DocumentType.TransitosFacturados_newPage:
                ReplaceVariables_TransitosFacturados_newPage(ref html, data);
                break;
            case DocumentType.TransitosNoFacturados:
                ReplaceVariables_TransitosNoFacturados(ref html, data);
                break;
            case DocumentType.TransitosNoFacturados_newPage:
                ReplaceVariables_TransitosNoFacturados_newPage(ref html, data);
                break;*/
                default:
                    throw new Exception("The document type is not implemented.");
            }
        }

        private void ReplaceVariables_EstadoPagoPeajes(ref string html, string data)
        {
            string[] rows = data.Split(';');
            string[] cols;
            string htmlTable = null;

            foreach (var row in rows)
            {
                cols = row.Split(',');
                htmlTable += string.Format(@"                
                    <tr>
                        <td>{0}</td>
                        <td>{1}</td>
                        <td>{2}</td>
                        <td>{3}</td>
                    </tr>", cols[0], cols[1], cols[2], cols[3]);
            }

            html = html.Replace("$[FECHA]", DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy"));
            html = html.Replace("$[PATENTES]", htmlTable);
        }

        // certificado educacion sin carga
        private void ReplaceVariable_CertificadoEducacion(ref string html, string data)
        {
            string[] rows = data.Split(',');
            string[] cols;

            html = html.Replace("$[fechaEmision]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt"));
            html = html.Replace("$[rutTitular]", rows[0]);
            html = html.Replace("$[nombreTitular]", rows[1]);

            html = html.Replace("$[rutEmpresa]", rows[5]);
            html = html.Replace("$[nombreEmpresa]", rows[6]);
            html = html.Replace("$[nombreUniInt]", rows[7]);


        }

        private void ReplaceVariables_CertificadoEducacionCarga(ref string html, string data)
        {
            string[] rows = data.Split(',');

            html = html.Replace("$[fechaEmision]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt"));
            html = html.Replace("$[rutTitular]", rows[0]);
            html = html.Replace("$[nombreTitular]", rows[1]);

            html = html.Replace("$[rutEmpresa]", rows[5]);
            html = html.Replace("$[nombreEmpresa]", rows[6]);

            html = html.Replace("$[rutBeneficiario]", rows[3]);
            html = html.Replace("$[nombreBeneficiario]", rows[4]);
            html = html.Replace("$[nombreUniInt]", rows[7]);
        }

        private void ReplaceVariables_CertificadoSalud(ref string html, string data)
        {
            string[] rows = data.Split(',');

            html = html.Replace("$[fechaEmision]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt"));
            html = html.Replace("$[rutTitular]", rows[0]);
            html = html.Replace("$[nombreTitular]", rows[1]);

            html = html.Replace("$[rutEmpresa]", rows[5]);
            html = html.Replace("$[nombreEmpresa]", rows[6]);
            html = html.Replace("$[nombreUniInt]", rows[7]);
        }

        private void ReplaceVariables_CertificadoSaludCarga(ref string html, string data)
        {
            string[] rows = data.Split(',');

            html = html.Replace("$[fechaEmision]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt"));
            html = html.Replace("$[rutTitular]", rows[0]);
            html = html.Replace("$[nombreTitular]", rows[1]);

            html = html.Replace("$[rutEmpresa]", rows[5]);
            html = html.Replace("$[nombreEmpresa]", rows[6]);

            html = html.Replace("$[rutBeneficiario]", rows[3]);
            html = html.Replace("$[nombreBeneficiario]", rows[4]);
            html = html.Replace("$[nombreUniInt]", rows[7]);
        }

        private void ReplaceVariables_CertificadoFines(ref string html, string data)
        {
            string[] rows = data.Split(',');

            html = html.Replace("$[fechaEmision]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt"));
            html = html.Replace("$[rutTitular]", rows[0]);
            html = html.Replace("$[nombreTitular]", rows[1]);

            html = html.Replace("$[rutEmpresa]", rows[5]);
            html = html.Replace("$[nombreEmpresa]", rows[6]);
            html = html.Replace("$[nombreUniInt]", rows[7]);
        }

        private void ReplaceVariables_CertificadoFinesCarga(ref string html, string data)
        {
            string[] rows = data.Split(',');

            html = html.Replace("$[fechaEmision]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt"));
            html = html.Replace("$[rutTitular]", rows[0]);
            html = html.Replace("$[nombreTitular]", rows[1]);

            html = html.Replace("$[rutEmpresa]", rows[5]);
            html = html.Replace("$[nombreEmpresa]", rows[6]);

            html = html.Replace("$[rutBeneficiario]", rows[3]);
            html = html.Replace("$[nombreBeneficiario]", rows[4]);
            html = html.Replace("$[nombreUniInt]", rows[7]);
        }

        private void ReplaceVariables_CertificadoCargaVigente(ref string html, string data)
        {
            string[] rows = data.Split('@');
            string[] afiliado;
            string[] carga;
            string[] valor;
            string htmlTable = null;
            var numeroBoleta = string.Empty;

            // TODO: Buscar una mejor solución
            var rutCarga = string.Empty;
            var parentescoCarga = string.Empty;
            var nombreCarga = string.Empty;
            var sexoCarga = string.Empty;
            var fechaInicio = string.Empty;
            var fechaNacimiento = string.Empty;
            var fechaVencimiento = string.Empty;

            // Llenar encabezado principal

            afiliado = rows[0].Split(',');

            html = html.Replace("$[rutAfiliado]", afiliado[0]);
            html = html.Replace("$[nombreAfiliado]", afiliado[1]);
            html = html.Replace("$[sucursal]", afiliado[2]);
            html = html.Replace("$[rutRazonSocial]", afiliado[3]);
            html = html.Replace("$[razonSocial]", afiliado[4]);
            html = html.Replace("$[cargasAutorizadas]", afiliado[5]);
            html = html.Replace("$[fechaVigencia]", afiliado[6]);
            html = html.Replace("$[fechaEmision]", DateTime.Now.ToString("dd-MM-yyyy"));
            html = html.Replace("$[timbreCaja]", DateTime.Now.ToString("hh:MM:ss tt"));



            for (var i = 1; i < rows.Length; i++)
            {
                var algo = rows[1].Substring(1, rows[1].Length - 1);

                string[] carga2 = algo.Split(';');

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    rutCarga = rutCarga += "<p>" + valor[0] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    parentescoCarga = parentescoCarga += "<p>" + valor[1] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    nombreCarga = nombreCarga += "<p>" + valor[2] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    sexoCarga = sexoCarga += "<p>" + valor[3] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    fechaInicio = fechaInicio += "<p>" + valor[4] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    fechaNacimiento = fechaNacimiento += "<p>" + valor[5] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    fechaVencimiento = fechaVencimiento += "<p>" + valor[6] + "</p>";
                }

                // reemplazamos las variables
                html = html.Replace("$[rutCarga]", rutCarga);
                html = html.Replace("$[parentescoCarga]", parentescoCarga);
                html = html.Replace("$[nombreCarga]", nombreCarga);
                html = html.Replace("$[sexoCarga]", sexoCarga);
                html = html.Replace("$[fechaInicio]", fechaInicio);
                html = html.Replace("$[fechaNacimiento]", fechaNacimiento);
                html = html.Replace("$[fechaVencimiento]", fechaVencimiento);
            }
        }

        private void ReplaceVariables_CertificadoCargaPendiente(ref string html, string data)
        {
            string[] rows = data.Split('@');
            string[] afiliado;
            string[] carga;
            string[] valor;
            string htmlTable = null;
            var numeroBoleta = string.Empty;

            // TODO: Buscar una mejor solución
            var rutCarga = string.Empty;
            var parentescoCarga = string.Empty;
            var nombreCarga = string.Empty;
            var sexoCarga = string.Empty;
            var fechaInicio = string.Empty;
            var fechaNacimiento = string.Empty;
            var fechaVencimiento = string.Empty;

            // Llenar encabezado principal

            afiliado = rows[0].Split(',');

            html = html.Replace("$[rutAfiliado]", afiliado[0]);
            html = html.Replace("$[nombreAfiliado]", afiliado[1]);
            html = html.Replace("$[sucursal]", afiliado[2]);
            html = html.Replace("$[rutRazonSocial]", afiliado[3]);
            html = html.Replace("$[razonSocial]", afiliado[4]);
            html = html.Replace("$[cargasAutorizadas]", afiliado[5]);
            html = html.Replace("$[fechaVigencia]", afiliado[6]);
            html = html.Replace("$[fechaEmision]", DateTime.Now.ToString("dd-MM-yyyy"));
            html = html.Replace("$[timbreCaja]", DateTime.Now.ToString("hh:MM:ss tt"));



            for (var i = 1; i < rows.Length; i++)
            {
                var algo = rows[1].Substring(1, rows[1].Length - 1);

                string[] carga2 = algo.Split(';');

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    rutCarga = rutCarga += "<p>" + valor[0] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    parentescoCarga = parentescoCarga += "<p>" + valor[1] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    nombreCarga = nombreCarga += "<p>" + valor[2] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    sexoCarga = sexoCarga += "<p>" + valor[3] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    fechaInicio = fechaInicio += "<p>" + valor[4] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    fechaNacimiento = fechaNacimiento += "<p>" + valor[5] + "</p>";
                }

                foreach (var a in carga2)
                {
                    valor = a.Split(',');
                    fechaVencimiento = fechaVencimiento += "<p>" + valor[6] + "</p>";
                }

                // reemplazamos las variables
                html = html.Replace("$[rutCarga]", rutCarga);
                html = html.Replace("$[parentescoCarga]", parentescoCarga);
                html = html.Replace("$[nombreCarga]", nombreCarga);
                html = html.Replace("$[sexoCarga]", sexoCarga);
                html = html.Replace("$[fechaInicio]", fechaInicio);
                html = html.Replace("$[fechaNacimiento]", fechaNacimiento);
                html = html.Replace("$[fechaVencimiento]", fechaVencimiento);
            }
        }

        private void ReplaceVariables_CertificadoCargaPendiente2(ref string html, string data)
        {
            string[] rows = data.Split(',');
            string[] afiliado;
            string[] valor;
            var numeroBoleta = string.Empty;

            // Llenar encabezado principal

            afiliado = rows;

            html = html.Replace("$[rutAfiliado]", afiliado[0]);
            html = html.Replace("$[nombreAfiliado]", afiliado[1]);
            html = html.Replace("$[sucursal]", afiliado[2]);
            html = html.Replace("$[rutRazonSocial]", afiliado[3]);
            html = html.Replace("$[razonSocial]", afiliado[4]);
            html = html.Replace("$[cargasAutorizadas]", afiliado[5]);
            html = html.Replace("$[fechaVigencia]", afiliado[6]);
            html = html.Replace("$[fechaEmision]", DateTime.Now.ToString("dd-MM-yyyy"));
            html = html.Replace("$[timbreCaja]", DateTime.Now.ToString("hh:MM:ss tt"));

            // reemplazamos las variables
            html = html.Replace("$[rutCarga]", "");
            html = html.Replace("$[parentescoCarga]", "");
            html = html.Replace("$[nombreCarga]", "");
            html = html.Replace("$[sexoCarga]", "");
            html = html.Replace("$[fechaInicio]", "");
            html = html.Replace("$[fechaNacimiento]", "");
            html = html.Replace("$[fechaVencimiento]", "");
        }




        private void ReplaceVariables_LicenciaDetalle(ref string html, string data)
        {
            string[] rows = data.Split('@');

            //Fecha emision
            html = html.Replace("$[fechaEmisionDocumento]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt"));
            //numero licencia
            html = html.Replace("$[numeroLicencia]", rows[0]);
            //estado licencia
            html = html.Replace("$[estadoLicencia]", rows[1]);
            //tipo licencia
            html = html.Replace("$[tipoLicencia]", rows[2]);
            //folio licencia
            html = html.Replace("$[folioLicencia]", rows[3]);
            //fecha emision licencia
            html = html.Replace("$[fechaEmisionLicencia]", rows[4]);
            //tipo subsidio
            html = html.Replace("$[tipoSubsidio]", rows[5]);
            //dias reposo
            html = html.Replace("$[diasReposo]", rows[6]);
            //fecha inicio reposo
            html = html.Replace("$[fechaInicioReposo]", rows[7]);
            //fecha termino reposo
            html = html.Replace("$[fechaTerminoReposo]", rows[8]);
            //fecha recepcion caja
            html = html.Replace("$[fechaRecepcionCaja]", rows[9]);
            //fecha envio compin
            html = html.Replace("$[fechaEnvioCompin]", rows[10]);
            //fecha probable pago
            html = html.Replace("$[fechaProbablePago]", rows[11]);
        }

        private void ReplaceVariables_LiquidacionPago(ref string html, string data)
        {
            string[] rows = data.Split('@');
            string[] afiliado;
            
            string[] valor;
            
            var numeroBoleta = string.Empty;

            // TODO: Buscar una mejor solución
            var rutCarga = string.Empty;
            var parentescoCarga = string.Empty;
            var nombreCarga = string.Empty;
            var sexoCarga = string.Empty;
            var fechaInicio = string.Empty;
            var fechaNacimiento = string.Empty;
            var fechaVencimiento = string.Empty;
            string htmlTable = null;

            // Llenar encabezado principal

            afiliado = rows[0].Split('#');

            html = html.Replace("$[fechaEmision]", DateTime.Now.ToString("dd-MM-yyyy"));

            html = html.Replace("$[rutAfiliado]", afiliado[0]);
            html = html.Replace("$[nombreAfiliado]", afiliado[1]);
            html = html.Replace("$[tipoLicencia]", afiliado[2]);
            html = html.Replace("$[folioLicencia]", afiliado[3]);
            html = html.Replace("$[resolucion]", afiliado[4]);
            html = html.Replace("$[diasLicencia]", afiliado[5]);
            html = html.Replace("$[diasSubsidio]", afiliado[6]);
            html = html.Replace("$[continuidad]", afiliado[7]);
            html = html.Replace("$[cuotaLicencia]", afiliado[8]);

            html = html.Replace("$[fechaInicio]", afiliado[9]);
            html = html.Replace("$[fechaTermino]", afiliado[10]);
            html = html.Replace("$[baseCalculoSubsidio]", afiliado[11]);
            html = html.Replace("$[baseCalculoCotizacion]", afiliado[12]);
            html = html.Replace("$[montoSubsidio]", afiliado[13]);
            html = html.Replace("$[montoCotizado]", afiliado[14]);
            html = html.Replace("$[tipoPago]", afiliado[15]);
            html = html.Replace("$[formaPago]", afiliado[16]);
            html = html.Replace("$[entidadPrevisional]", afiliado[17]);
            html = html.Replace("$[cotizacionPension]", afiliado[18]);
            html = html.Replace("$[cotizacionSalud]", string.Format("{0:N0}", double.Parse(afiliado[19])));
            html = html.Replace("$[rutEmpresa]", afiliado[20]);
            html = html.Replace("$[seguroCesantia]", afiliado[21]);
            html = html.Replace("$[montoCotizado]", afiliado[14]);
            html = html.Replace("$[fechaInicio]", DateTime.Now.ToString("dd-MM-yyyy"));
            html = html.Replace("$[subsidioDiario]", afiliado[22]);
            html = html.Replace("$[montoRecibir]", afiliado[23]);


            //for (var i = 1; i < rows.Length; i++)
            //{
                var algo = rows[1].Substring(1, rows[1].Length - 1);

                string[] carga2 = algo.Split(';');

                foreach (var a in carga2)
                {
                    valor = a.Split('#');
                    htmlTable += string.Format(@"                
                    <tr>
                        <td style=""text-align: center;"">{0}</td>
                        <td style=""text-align: center;""> {1}</td>
                        <td style=""text-align: center;"">$ {2}</td>
                        <td style=""text-align: center;"">$ {3}</td>
                        <td style=""text-align: center;"">$ {4}</td>
                        <td style=""text-align: center;"">$ {5}</td>
                        <td style=""text-align: center;"">$ {6}</td>
                    </tr>", valor[0], valor[2], valor[1], valor[3], valor[4], valor[5], valor[6]);
                }
            //}

            html = html.Replace("$[detallePago]", htmlTable);
            
        }

        private void ReplaceVariables_TransitosFacturados(ref string html, string data)
        {
            string[] rows = data.Split(';');
            string[] cols;
            string htmlTable = null;
            var numeroBoleta = string.Empty;

            foreach (var row in rows)
            {
                cols = row.Split(',');
                htmlTable += string.Format(@"                
                    <tr>
                        <td>{0}</td>
                        <td>{1}</td>
                        <td>{2}</td>
                        <td>{3}</td>
                        <td>{4}</td>
                    </tr>", cols[0], cols[1], cols[2], cols[3], cols[4]);
                numeroBoleta = cols[5];

            }



            html = html.Replace("$[FACTURAS]", htmlTable);
            html = html.Replace("$[NROBOLETA]", numeroBoleta);
        }

        private void ReplaceVariables_TransitosFacturados_newPage(ref string html, string data)
        {
            string[] rows = data.Split(';');
            string[] cols;
            string htmlTable = null;
            var numeroBoleta = string.Empty;

            foreach (var row in rows)
            {
                cols = row.Split(',');
                htmlTable += string.Format(@"                
                    <tr>
                        <td>{0}</td>
                        <td>{1}</td>
                        <td>{2}</td>
                        <td>{3}</td>
                        <td>{4}</td>
                    </tr>", cols[0], cols[1], cols[2], cols[3], cols[4]);
                numeroBoleta = cols[5];

            }



            html = html.Replace("$[FACTURAS]", htmlTable);
            html = html.Replace("$[NROBOLETA]", numeroBoleta);
        }

        private void ReplaceVariables_TransitosNoFacturados(ref string html, string data)
        {
            string[] rows = data.Split(';');
            string[] cols;
            string htmlTable = null;

            foreach (var row in rows)
            {
                cols = row.Split(',');
                htmlTable += string.Format(@"                
                    <tr>
                        <td>{0}</td>
                        <td>{1}</td>
                        <td>{2}</td>
                        <td>{3}</td>
                        <td>{4}</td>
                    </tr>", cols[0], cols[1], cols[2], cols[3], cols[4]);
            }

            html = html.Replace("$[FACTURAS]", htmlTable);
        }

        private void ReplaceVariables_TransitosNoFacturados_newPage(ref string html, string data)
        {
            string[] rows = data.Split(';');
            string[] cols;
            string htmlTable = null;

            foreach (var row in rows)
            {
                cols = row.Split(',');
                htmlTable += string.Format(@"                
                    <tr>
                        <td>{0}</td>
                        <td>{1}</td>
                        <td>{2}</td>
                        <td>{3}</td>
                        <td>{4}</td>
                    </tr>", cols[0], cols[1], cols[2], cols[3], cols[4]);
            }

            html = html.Replace("$[FACTURAS]", htmlTable);
        }

        public string copyAndCreatePreview(string documentRef)
        {
            string fileName = Path.GetFileName(documentRef);
            string sourcePath = @"C:\Users\Public\TestFolder";
            string targetPath = @"C:\inetpub\wwwroot\CCLA_pdfPreview";

            // Use Path class to manipulate file and directory paths.
            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
            string destFile = System.IO.Path.Combine(targetPath, fileName);

            // To copy a folder's contents to a new location:
            // Create a new target folder, if necessary.
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.
            System.IO.File.Copy(documentRef, destFile, true);

            // To copy all the files in one directory to another directory.
            // Get the files in the source folder. (To recursively iterate through
            // all subfolders under the current directory, see
            // "How to: Iterate Through a Directory Tree.")
            // Note: Check for target path was performed previously
            //       in this code example.
            /*if (System.IO.Directory.Exists(sourcePath))
            {
                string[] files = System.IO.Directory.GetFiles(sourcePath);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    fileName = System.IO.Path.GetFileName(s);
                    destFile = System.IO.Path.Combine(targetPath, fileName);
                    System.IO.File.Copy(s, destFile, true);
                }
            }
            else
            {
                Console.WriteLine("Source path does not exist!");
            }*/

            // Keep console window open in debug mode.
            // Console.WriteLine("Press any key to exit.");
            // Console.ReadKey();
            return destFile;
        }
    }
}
