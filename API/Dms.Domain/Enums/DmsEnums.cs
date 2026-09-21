using Google.Apis.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dms.Domain.Enums
{
    public class DmsEnums
    {
        public enum SystemSetting
        {
            [StringValue("")]
            Draft = 1,

            [StringValue("ProcedureStatus_Active")]
            Active = 2,

            [StringValue("ProcedureStatus_Inactive")]
            Inactive = 3
        }
    }
}
