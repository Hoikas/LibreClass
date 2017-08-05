/*    This file is part of LibreClass.
 *
 *    LibreClass is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    LibreClass is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with LibreClass.  If not, see <http://www.gnu.org/licenses/>.
 */

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
