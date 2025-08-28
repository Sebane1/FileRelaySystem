using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayUploadProtocol
{
    public class Structs
    {
        public enum AgeGroup
        {
            Everyone = 0,
            AdultsOnly = 1
        }
        public enum ServerContentRating
        {
            SafeForWorkOnly = 0,
            NotSafeForWork = 1,
        }
        public enum ServerContentType
        {
            General = 0,
            Outfits = 1,
            Memes = 2,
            Other = 3
        }
        public enum RequestType
        {
            AddTemporaryFile = 0,
            GetTemporaryFile = 1,
            ClearState = 2,
            AddPersistedFile = 3,
            GetPersistedFile = 4,
            CheckPersistedFileChanged = 5,
            BanUser = 6,
            IssueAccessToken = 7,
            GetServerAlias = 8,
            SetServerAlias = 9,
            GetServerRules = 10,
            SetServerRules = 11,
            GetServerDescription = 12,
            SetServerDescription = 13,
            GetAgeGroup = 14,
            SetAgeGroup = 15,
            GetContentRating = 16,
            SetContentRating = 17,
            GetServerContent = 18,
            SetServerContentType = 19,
            GetPublicServerInfo = 20
        }
    }
}
