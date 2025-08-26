using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayUploadProtocol {
    public class Structs {
        public enum AgeGroup {
            Everyone = 0,
            AdultsOnly = 1
        }
        public enum ServerContentRating {
            SafeForWorkOnly = 0,
            NotSafeForWork = 1,
        }
        public enum ServerContentType {
            General = 0,
            Outfits = 1,
            Memes = 2,
            Other = 3
        }
        public enum RequestType {
            AddTemporaryFile = 0,
            GetTemporaryFile = 1,
            ClearState = 2,
            AddPersistedFile = 3,
            GetPersistedFile = 4,
            BanUser = 5,
            IssueAccessToken = 6,
        }
    }
}
