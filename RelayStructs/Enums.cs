using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayUploadProtocol
{
    public class Enums
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
        public enum ServerUploadAllowance
        {
            OneFilePerUser,
            MultipleFilesPerUser
        }
        public enum ServerRole
        {
            None,
            Moderator,
            Admin,
            Creator
        }
        public enum RequestType
        {
            AddTemporaryFile = 0,
            GetTemporaryFile = 1,
            ClearState = 2,
            AddPersistedFile = 3,
            GetPersistedFile = 4,
            CheckIfPersistedFileChanged = 5,
            CheckIfFileExists = 6,
            BanUser = 7,
            IssueAccessToken = 8,
            GetUploadAllowance = 9,
            SetUploadAllowance = 10,
            GetServerAlias = 11,
            SetServerAlias = 12,
            GetServerRules = 13,
            SetServerRules = 14,
            GetServerDescription = 15,
            SetServerDescription = 16,
            GetAgeGroup = 17,
            SetAgeGroup = 18,
            GetContentRating = 19,
            SetContentRating = 20,
            GetServerContentType = 21,
            SetServerContentType = 22,
            GetSynchronizationContext = 23,
            SetSynchronizationContext = 24,
            GetMaxFileSizeInMb = 25,
            SetMaxFileSizeInMb = 26,
            GetGeneralUserLifespan = 27,
            SetGeneralUserLifespan = 28,
            GetPublicServerInfo = 29,
        }
    }
}
