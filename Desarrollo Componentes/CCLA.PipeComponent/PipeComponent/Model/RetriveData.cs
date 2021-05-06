using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PipeComponent
{
    //Rodrigo
    class RetriveData
    {
        #region Variables

        private string totemIp1;

        private string serviceId1;

        private string rut1;

        private string hourNumberRequest1;

        
        #endregion
        
        #region Metodos
        public string totemIp
        {
            get { return totemIp1; }
            set { totemIp1 = value; }
        }
        public string rut
        {
            get { return rut1; }
            set { rut1 = value; }
        }
        public string serviceId
        {
            get { return serviceId1; }
            set { serviceId1 = value; }
        }
        public string hourNumberRequest
        {
            get { return hourNumberRequest1; }
            set { hourNumberRequest1 = value; }
        }
        #endregion
    }
}
