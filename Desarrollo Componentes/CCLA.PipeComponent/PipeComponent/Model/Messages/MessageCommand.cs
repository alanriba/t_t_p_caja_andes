using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PipeComponent.Model.Messages
{
    enum MessageCommand
    {
        //Periferico
        ValidateFingerprint = 100,
        PrintOnLaserPrinter = 101,
        PrintVoucher = 102,
        StatusPrinter3 = 103,
        // Huella
        ValidaHuella_ObtenerRut = 104,
        ValidaHuella_ObtenerDedo = 105,
        ValidaHuella_Valida = 106,
        // Otros
        CreateAndPrintPDF = 107,
        //No Periferico
        BanmedicaToken = 200,
        //Cerrar Procesos Kiosko
        CloseKiosk = 300,

        //Previsualización de documentos
        preViewDocumentPdf = 500,
        printPdf = 600,

        // Evniar Correo
        sendEmail = 700
    }
}
