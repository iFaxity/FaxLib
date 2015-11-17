using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaxLib {
    class JSON {
        Hashtable ParseObject(string str) { throw new NotImplementedException(); }
        ArrayList ParseArray(string str) { throw new NotImplementedException(); }
        object ParseValue(string str) {
            decimal d;
            bool b;
            if(decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                return d;
            else if(bool.TryParse(str, out b))
                return b;
            else if(str == "null")
                return null;
            else
                return str;
            throw new NotImplementedException();
        }

        void ParseFile(string file) { throw new NotImplementedException(); }
        void Parse(string s) { throw new NotImplementedException(); }
        void Stringify(object o) { throw new NotImplementedException(); }

        void Deserialize<T>(T obj) { throw new NotImplementedException(); }
        void Serialize<T>(T obj) { throw new NotImplementedException(); }

        private class FaxJson {

        }
    }
}
