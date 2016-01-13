using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FaxLib.Classes {
    class FaxCore {
        public static Version Version {
            get {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
    }
}
