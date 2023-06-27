using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemRelay {
    public class FileManager {
        Dictionary<string, FileIdentifier> files = new Dictionary<string, FileIdentifier>();
        public Dictionary<string, FileIdentifier> Files { get => files; set => files = value; }
    }
}
