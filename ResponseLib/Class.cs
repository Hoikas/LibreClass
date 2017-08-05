using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreClass.Response {
    public interface IRoster {
        string Name { get; set; }
        List<IStudent> Students { get; }
    }

    public interface IStudent {
        string Name { get; set; }
        ushort SrsDeviceID { get; set; }
    }
}
