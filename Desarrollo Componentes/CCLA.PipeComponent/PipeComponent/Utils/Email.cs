using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace PipeComponent.Utils
{
    public class Email
    {
        public void EnviarComprobante(string destinatarios, string documento)
        {
            using (var mail = new MailMessage())
            using (var client = new SmtpClient("relay.cajalosandes.cl", 25))
            {
                //var pathPlantilla = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailComprobante.html");
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templateMail", "TemplateCorreo.html");
                var url = @"C:\TEMP\";
                // var emailBody = "";

                // var plantillaEmail = File.ReadAllText(pathPlantilla);
                // var barcodeWriter = new BarcodeWriter();

                // barcodeWriter.Format = BarcodeFormat.QR_CODE;


                // emailBody = plantillaEmail;

                mail.To.Add(destinatarios);
                mail.From = new MailAddress("noresponder@cajalosandes.cl", "Servicio de Auto-Consulta");
                mail.Subject = "CCLA - Auto-Consulta";

                var bodyHtml = "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
                        "<head>" +
                        "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=iso-8859-1\" />" +
                        "<title> Caja Los Andes</title>" +
                        "</head>" +
                        "<body>" +
                        "<table width=\"600\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" style=\"border:1px solid #efefef;\">" +
                        "<tr>" +
                        "<td>" +
                        "<table width=\"600\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">" +
                        "<tr>" +
                        "<td width=\"19\">&nbsp;</td>" +
                        "<td width=\"126\"><a href=\"https://www.cajalosandes.cl\" title=\"Caja Los Andes\" target=\"_blank\" style=\"display:block; outline: none; border: 0;\"><img src='cid:logoCaja' height=\"82\" width=\"126\" style=\"vertical-align:bottom; display:block; border: 0;\" alt=\"Caja Los Andes\" /></a></td>" +
                        "<td width=\"455\">" +
                        "<table width=\"455\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">" +
                        "<tr>" +
                        "<td height=\"28\">&nbsp;</td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td height=\"24\">" +
                        "<table width=\"455\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">" +
                        "<tr>" +
                        "<td width=\"283\"> &nbsp;</td>" +
                        "<td width=\"24\">" +
                        "<a href=\"http://www.facebook.com/CajaLosAndesCL/\" title=\"facebook\" target=\"_blank\" style=\"display:block; outline: none; border: 0;\"><img src='cid:logoFacebook' height=\"24\" width=\"24\" style=\"vertical-align:bottom; display:block; border: 0;\" alt=\"Facebook\"/></a>" +
                        "</td>" +
                        "<td>&nbsp;</td>" +
                        "<td width=\"24\"><a href=\"http://twitter.com/cajalosandes\" title=\"twitter\" target=\"_blank\" style=\"display:block; outline: none; border: 0;\"><img src='cid:logoTwitter' height=\"24\" width=\"24\" style=\"vertical-align:bottom; display:block; border: 0;\" alt=\"\"/></a></td>" +
                        "<td>&nbsp;</td>" +
                        "<td width =\"24\"><a href=\"http://www.youtube.com/user/canalandes\" title=\"youtube\" target=\"_blank\" style=\"display:block; outline: none; border: 0;\"><img src='cid:logoYoutube' height =\"24\" width=\"24\" style=\"vertical-align:bottom; display:block; border: 0;\" alt =\"\" /></a></td>" +
                        "<td>&nbsp;</td>" +
                        "<td width =\"24\"><a href=\"http://www.instagram.com/cajalosandes/?hl=es\" title =\"instagram\" target=\"_blank\" style=\"display:block; outline: none; border: 0;\"><img src='cid:logoInstagram' height =\"24\" width =\"24\" style=\"vertical-align:bottom; display:block; border: 0;\" alt =\"\"/></a></td>" +
                        "<td>&nbsp;</td>" +
                        "<td width =\"24\"><a href=\"http://cl.linkedin.com/company/caja-los-andes\" title =\"Linkedin\" target =\"_blank\" style=\"display:block; outline: none; border: 0;\"><img src ='cid:logoLinkedin' height =\"24\" width =\"24\" style=\"vertical-align:bottom; display:block; border: 0;\" alt =\"\"/></a></td>" +
                        "<td width=\"35\">&nbsp;</td>" +
                        "</tr>" +
                        "</table>" +
                        "</td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td height =\"30\"><img src='cid:logoLinea' height =\"30\" width=\"455\" style=\"vertical-align:bottom; display:block; border: 0;\" alt =\"\"/></td>" +
                        "</tr>" +
                        "</table>" +
                        "</td>" +
                        "</tr>" +
                        "</table>" +
                        "</td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td height =\"203\"><img src='cid:logoBanner' height=\"203\" width=\"600\" style=\"vertical-align:bottom; display:block;\" alt =\"Cr&eacute;dito F&aacute;cil\"/></td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td><table width=\"600\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">" +
                        "<tr>" +
                        "<td width=\"50\">&nbsp;</td>" +
                        "<td width=\"500\"><table width=\"500\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">" +
                    "<tr>" +
                        "<td height=\"30\">&nbsp;</td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td valign =\"middle\"><font style=\"font-family:Arial,Tahoma,Verdana,sans-serif;font-size:14px;color:#383a3d\"><span style=\"font-size:14px;color:#383a3d\"> Estimado(a):<br/><br/>" +
                        "Enviamos en archivo adjunto documento solicitado a trav&eacute;s de nuestros Autoservicios.<br/><br/>" +
                        "</span></font></td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td valign =\"middle\"><font style=\"font-family:Arial,Tahoma,Verdana,sans-serif;font-size:14px;color:#383a3d\"><span style=\"font-size:14px;color:#383a3d\">" +
                        "Atentamente,<br/>" +
                        "Caja Los Andes" +
                        "</span></font></td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td height=\"50\">&nbsp;</td>" +
                        "</tr>" +
                        "</table></td>" +
                        "<td width=\"50\">&nbsp;</td>" +
                        "</tr>" +
                        "</table></td> " +
                        "</tr>" +
                        "<tr>" +
                        "<td height=\"90\"><img src='cid:logoFooter' alt=\"\" width=\"600\" height=\"90\" style=\"display:block; vertical-align:bottom;\" border=\"0\"/></td>" +
                        "</tr>" +
                        "</table>" +
                        "</body>" +
                        "</html>";
                mail.Body = bodyHtml;
                mail.IsBodyHtml = true;


                var alternateView = AlternateView.CreateAlternateViewFromString(bodyHtml, null, MediaTypeNames.Text.Html);

                var logoCaja = new LinkedResource(url + @"htmlTemplates/templateMail/images/logo-caja.jpg", MediaTypeNames.Image.Jpeg);
                var logoFacebbok = new LinkedResource(url + @"htmlTemplates/templateMail/images/btn-fb.jpg", MediaTypeNames.Image.Jpeg);
                var logoTwitter = new LinkedResource(url + @"htmlTemplates/templateMail/images/btn-tw.jpg", MediaTypeNames.Image.Jpeg);
                var logoYoutube = new LinkedResource(url + @"htmlTemplates/templateMail/images/btn-ytb.jpg", MediaTypeNames.Image.Jpeg);
                var logoInstagram = new LinkedResource(url + @"htmlTemplates/templateMail/images/btn-inst.jpg", MediaTypeNames.Image.Jpeg);
                var logoLinkedin = new LinkedResource(url + @"htmlTemplates/templateMail/images/btn-lin.jpg", MediaTypeNames.Image.Jpeg);
                var logoLinea = new LinkedResource(url + @"htmlTemplates/templateMail/images/linea.jpg", MediaTypeNames.Image.Jpeg);
                var logoBanner = new LinkedResource(url + @"htmlTemplates/templateMail/images/banner.jpg", MediaTypeNames.Image.Jpeg);
                var logoFooter = new LinkedResource(url + @"htmlTemplates/templateMail/images/footer.jpg", MediaTypeNames.Image.Jpeg);


                logoCaja.ContentId = "logoCaja";
                logoFacebbok.ContentId = "logoFacebook";
                logoTwitter.ContentId = "logoTwitter";
                logoYoutube.ContentId = "logoYoutube";
                logoInstagram.ContentId = "logoInstagram";
                logoLinkedin.ContentId = "logoLinkedin";
                logoLinea.ContentId = "logoLinea";
                logoBanner.ContentId = "logoBanner";
                logoFooter.ContentId = "logoFooter";

                alternateView.LinkedResources.Add(logoCaja);
                alternateView.LinkedResources.Add(logoFacebbok);
                alternateView.LinkedResources.Add(logoTwitter);
                alternateView.LinkedResources.Add(logoYoutube);
                alternateView.LinkedResources.Add(logoInstagram);
                alternateView.LinkedResources.Add(logoLinkedin);
                alternateView.LinkedResources.Add(logoLinea);
                alternateView.LinkedResources.Add(logoBanner);
                alternateView.LinkedResources.Add(logoFooter);

                mail.AlternateViews.Add(alternateView);
              

                //comprobamos si existe el archivo y lo agregamos a los adjuntos
                if (System.IO.File.Exists(documento))
                    mail.Attachments.Add(new Attachment(documento));

                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential();
                //client.Credentials = new NetworkCredential("", "");
                client.EnableSsl = false;
                client.Timeout = 30 * 1000;
                client.Send(mail);
            }
        }
    }
}
