using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RelayCommonData {
    public class Hashing {
        public static string SHA512Hash(string value) {
            using (var alg = SHA512.Create()) {
                var message = Encoding.UTF8.GetBytes(value);
                var hashValue = alg.ComputeHash(message);
                string hex = "";
                foreach (byte x in hashValue) {
                    hex += string.Format("{0:x2}", x);
                }
                return hex;
            }
        }
    }
}
