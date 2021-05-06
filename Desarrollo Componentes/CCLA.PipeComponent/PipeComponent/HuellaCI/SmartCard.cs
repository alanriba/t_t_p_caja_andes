using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TPAuto.HuellaCI.Exceptions;


namespace TPAuto.HuellaCI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SCARD_IO_REQUEST
    {
        public UInt32 dwProtocol;
        public UInt32 cbPciLength;
    }

    public struct SCard_ReaderState
    {
        public string m_szReader;
        public IntPtr m_pvUserData;
        public UInt32 m_dwCurrentState;
        public UInt32 m_dwEventState;
        public UInt32 m_cbAtr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] m_rgbAtr;
    }

    public enum Scope
    {
        User,
        Terminal,
        System
    }

    public enum Share
    {
        Exclusive = 1,
        Shared,
        Direct
    }

    public enum Protocol
    {
        Undefined = 0x00000000,
        T0 = 0x00000001,
        T1 = 0x00000002,
        Raw = 0x00010000,
        Default = unchecked((int)0x80000000),
        T0orT1 = T0 | T1
    }

    public class SmartCard
    {
        private IntPtr _hCard = IntPtr.Zero;
        private IntPtr _hContext = IntPtr.Zero;
        private uint _protocol = 1;
        protected const uint WAIT_TIME = 250;

        public int LastErrorCode { get; private set; }

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardTransmit(IntPtr hCard,
            [In] ref SCARD_IO_REQUEST pioSendPci,
            byte[] pbSendBuffer,
            UInt32 cbSendLength,
            IntPtr pioRecvPci,
            [Out] byte[] pbRecvBuffer,
            out UInt32 pcbRecvLength
            );

        [DllImport("winscard.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int SCardConnect(IntPtr hContext,
            [MarshalAs(UnmanagedType.LPTStr)] string szReader,
            UInt32 dwShareMode,
            UInt32 dwPreferredProtocols,
            IntPtr phCard,
            IntPtr pdwActiveProtocol);

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardEstablishContext(UInt32 dwScope,
            IntPtr pvReserved1,
            IntPtr pvReserved2,
            IntPtr phContext);

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardGetStatusChange(IntPtr hContext,
            UInt32 dwTimeout,
            [In, Out] SCard_ReaderState[] rgReaderStates,
            UInt32 cReaders);

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardBeginTransaction(IntPtr hCard);

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardIsValidContext(IntPtr hContext);

        public void EstablishContext(Scope scope)
        {
            IntPtr hContext = Marshal.AllocHGlobal(Marshal.SizeOf(_hContext));
            
            LastErrorCode = SCardEstablishContext((uint)scope, IntPtr.Zero, IntPtr.Zero, hContext);
            if (LastErrorCode != 0)
            {
                Marshal.FreeHGlobal(hContext);
                ThrowSmartcardException("SCardEstablishContext", LastErrorCode);
            }

            _hContext = Marshal.ReadIntPtr(hContext);
            Marshal.FreeHGlobal(hContext);
        }

        public void Connect(string reader, Share shareMode, Protocol protocol)
        {
            EstablishContext(Scope.User);

            IntPtr hCard = Marshal.AllocHGlobal(Marshal.SizeOf(_hCard));
            IntPtr pProtocol = Marshal.AllocHGlobal(Marshal.SizeOf(_protocol));
            
            Task t = Task.Factory.StartNew(() => 
            {
                LastErrorCode = SCardConnect(_hContext, reader, (uint)shareMode, (uint)protocol, hCard, pProtocol);
            });
            bool taskCompleted = t.Wait(5000);

            if (!taskCompleted)
            {                
                Marshal.FreeHGlobal(hCard);
                Marshal.FreeHGlobal(pProtocol);
                throw new TimeoutException("El método SCardConnect no respondió dentro del tiempo esperado.");
            }

            if (LastErrorCode != 0)
            {
                Marshal.FreeHGlobal(hCard);
                Marshal.FreeHGlobal(pProtocol);
                ThrowSmartcardException("SCardConnect", LastErrorCode);
            }
            
            _hCard = Marshal.ReadIntPtr(hCard);
            _protocol = (uint)Marshal.ReadInt32(pProtocol);

            Marshal.FreeHGlobal(hCard);
            Marshal.FreeHGlobal(pProtocol);
        }

        public byte[] Transmit(byte[] apduCmd)
        {
            uint recvLength = 512;
            byte[] apduResponse = new byte[recvLength];
            SCARD_IO_REQUEST ioRequest = new SCARD_IO_REQUEST();
            ioRequest.dwProtocol = _protocol;
            ioRequest.cbPciLength = 8;

            LastErrorCode = SCardTransmit(_hCard, ref ioRequest, apduCmd, (uint)apduCmd.Length, IntPtr.Zero, apduResponse, out recvLength);

            if (LastErrorCode != 0)
                throw new SmartCardException("SCardTransmit error: " + LastErrorCode);

            return apduResponse;
        }

        public string GetDedo()
        {
            string cardFpHand, cardFpFinger;
            int i, startingBlock = 0, fingerPos;
            byte[] RxData = null;

            RxData = Transmit(ApduCommand.SeleccionarAplicacion);
            RxData = Transmit(ApduCommand.ObtenerInfoHuella);

            for (i = 0; i < RxData.Length - 1; i++)
            {
                if (RxData[i] == 0x7F && RxData[i + 1] == 0x60)
                {
                    startingBlock = i;
                    break;
                }
            }

            fingerPos = Array.IndexOf(RxData, (byte)0x82, startingBlock) + 2;
            cardFpHand = GetHandText(RxData[fingerPos]);
            cardFpFinger = GetFingerText(RxData[fingerPos]);
            return (cardFpFinger + " " + cardFpHand).Trim();
        }

        private string GetHandText(int identifier)
        {
            try
            {
                string biometricInfo = Convert.ToString(identifier, 2).PadLeft(8, '0');
                switch (biometricInfo.Substring(6, 2))
                {
                    case "01": return "Derecho";
                    case "10": return "Izquierdo";
                    default: return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private string GetFingerText(int identifier)
        {
            try
            {
                string biometricInfo = Convert.ToString(identifier, 2).PadLeft(8, '0');
                switch (biometricInfo.Substring(3, 3))
                {
                    case "001": return "Pulgar";
                    case "010": return "Indice";
                    case "011": return "Medio";
                    case "100": return "Anular";
                    case "101": return "Menor";
                    default: return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private void ThrowSmartcardException(string methodName, long errCode)
        {
            if (errCode != 0)
                throw new SmartCardException(string.Format("{0} error: {1:X02}", methodName, errCode));
        }
    }
}
