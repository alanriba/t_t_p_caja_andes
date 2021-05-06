using System;
using System.Linq;
using System.Text;
using SecuGen.FDxSDK.Windows;
using SecuGen.FDxSDKPro.Windows;
using TPAuto.HuellaCI.Exceptions;

namespace TPAuto.HuellaCI
{
    class Secugen
    {
        public static string GetErrorDescription(int errorCode)
        {
            switch (errorCode)
            {
                case 0: return "Error none";
                case 1: return "Can not create object";
                case 2: return "Function Failed";
                case 3: return "Invalid Parameter";
                case 4: return "Not used function";
                case 5: return "Can not create object";
                case 6: return "Can not load device driver";
                case 7: return "Can not load sgfpamx.dll";
                case 51: return "Can not load driver kernel file";
                case 52: return "Failed to initialize the device";
                case 53: return "Data transmission is not good";
                case 54: return "Time out";
                case 55: return "Device not found";
                case 56: return "Can not load driver file";
                case 57: return "Wrong Image";
                case 58: return "Lack of USB Bandwith";
                case 59: return "Device is already opened";
                case 60: return "Device serial number error";
                case 61: return "Unsupported device";
                case 101: return "The number of minutiae is too small";
                case 102: return "Template is invalid";
                case 103: return "1st template is invalid";
                case 104: return "2nd template is invalid";
                case 105: return "Minutiae extraction failed";
                case 106: return "Matching failed";
                default: return "Unknown";
            }
        }

        public static string GetSerialNumber()
        {
            SGFPMDeviceInfoParam pInfo = new SGFPMDeviceInfoParam();
            SGFingerPrintManager manager = new SGFingerPrintManager();
            int error;

            error = manager.EnumerateDevice();
            if (error != (int)SGFPMError.ERROR_NONE)
                throw new InvalidOperationException("Error en método EnumerateDevice: " + Secugen.GetErrorDescription(error));

            if (manager.NumberOfDevice > 0)
            {
                SGFPMDeviceList device = new SGFPMDeviceList();
                error = manager.GetEnumDeviceInfo(0, device);
                if (error != (int)SGFPMError.ERROR_NONE)
                    throw new InvalidOperationException("Error en método GetEnumDeviceInfo: " + Secugen.GetErrorDescription(error));

                return Encoding.Default.GetString(device.DevSN.Where(x => x != 0x00).ToArray()).Trim();
            }
            else
            {
                throw new DeviceNotConnectedException("No se detectó ningún lector de huella.");
            }
        }
    }
}
