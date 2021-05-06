using PipeComponent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PipeComponent.Peripherals
{
    class AutentiaControl
    {
        public ValidationResponse ValidateUser(Rut rut)
        {
            ValidationResponse response = new ValidationResponse();
            AutentiaNet.AutentiaNet autentia = new AutentiaNet.AutentiaNet();

            autentia.ParamsInit("Rut,DV,serieCedula,Erc,ErcDesc,NroAudit,CodLector");
            autentia.ParamsSet(1, rut.Mantisa.ToString());
            autentia.ParamsSet(2, rut.DigitoVerificador.ToString());
            autentia.Transaccion2("../TOTALPACK/verifica");
            //autentia.Transaccion2("verifica_kiosko");
            if (autentia.ParamsGet(4).Equals("0"))
            {
                response.Successful = true;
            }
            else
            {
                response.Successful = false;
                response.Information = autentia.ParamsGet(5);
            }

            return response;
        }
    }
}