using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPAuto.HuellaCI.Exceptions;

namespace TPAuto.HuellaCI
{
    class ValidadorCedulaNueva
    {
        private SmartCard smartCard;
        private Dictionary<int, string> CodeToMessage = new Dictionary<int, string>() {
            { 0x6300, "Usuario no autenticado" },
            { 0x6700, "Tamaño especificado incorrecto" },
            { 0x6982, "Estado de seguridad no establecido" },
            { 0x6985, "Condiciones de uso no satisfactorias" },
            { 0x6999, "Comando no permitido" },
            { 0x6A80, "Parámetros incorrectos en el campo DATA" },
            { 0x6A81, "Función no soportada. No se pudo obtener datos" },
            { 0x6A86, "Parámetros P1-P2 no válidos o el campo P1 es diferente a '00'" },
            { 0x6A88, "Datos de referencia no encontrados" },
            { 0x6D00, "Código de instrucción no válido o no soportado" }
        };

        public ValidadorCedulaNueva(SmartCard smartCard)
        {
            this.smartCard = smartCard;
        }

        public bool HuellaEsValida(byte[] huella)
        {
            List<byte> aux;
            byte[] TxData;
            byte[] response = new byte[1024];

            // --------------------------------------- Selección de la aplicación --------------------------------------- //
            response = smartCard.Transmit(ApduCommand.SeleccionarAplicacion);
            if (response[0] != 0x90) throw new ApduResponseErrorException();

            // -------------------------------------------- Cotejamiento 1:1 -------------------------------------------- //
            aux = ApduCommand.ValidarHuella.ToList();
            aux[4] = (byte)(huella.Length + 5);
            aux[7] = (byte)(huella.Length + 2);
            aux[9] = (byte)(huella.Length);
            aux.AddRange(huella.ToList());
            aux.Add(0x00);
            TxData = aux.ToArray();
            response = smartCard.Transmit(TxData);
            if (response[0] != 0x90)
            {
                // Las huellas no coinciden. Reintentar con 2da huella almacenada en cedula
                TxData[3] = 0x92;
                response = smartCard.Transmit(TxData);
                if (response[0] != 0x90)
                {
                    int code = (response[0] << 8) + response[1];
                    if (code == 0x6300) return false;
                    string message;
                    throw new MismatchValidationException(CodeToMessage.TryGetValue(code, out message) ? message : string.Format("Error desconocido ({0})", code));
                }
            }
            
            return true;
        }
    }
}
