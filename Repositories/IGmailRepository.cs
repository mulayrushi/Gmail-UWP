using Google.Apis.Gmail.v1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gmail10.Repositories
{
    interface IGmailRepository
    {
        /// <summary>Gets all post for the specified blog.</summary>
        Task<bool> GetAuthenticated();

        /// <summary>Get all user's blogs.</summary>
        Task<IEnumerable<Label>> GetLabelsAsync(string userId);

        /// <summary>Get all user's profiles.</summary>
        Task<IEnumerable<Profile>> GetProfilesAsync(string userId);
    }
}
