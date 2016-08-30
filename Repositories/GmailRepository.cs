using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gmail10.Repositories
{
    class GmailRepository : IGmailRepository
    {
        private UserCredential credential;
        private GmailService service;
        static GmailRepository _gmailRepository;

        static public GmailRepository GetInstance()
        {
            if (_gmailRepository == null)
                _gmailRepository = new GmailRepository();
            return _gmailRepository;
        }

        private async Task AuthenticateAsync()
        {
            if (service != null)
                return;

            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new Uri("ms-appx:///Assets/client_secret.json"),
                new[] { GmailService.Scope.MailGoogleCom },
                "user",
                CancellationToken.None);

            var initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Gmail10",
            };

            service = new GmailService(initializer);
        }

        #region IGmailRepository members

        public async Task<bool> GetAuthenticated()
        {
            try
            {
                await AuthenticateAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Label>> GetLabelsAsync(string userId)
        {
            await AuthenticateAsync();
            List<Label> result = new List<Label>();
            try
            {
                ListLabelsResponse response = service.Users.Labels.List(userId).Execute();
                foreach (Label label in response.Labels)
                {
                    result.Add(label);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("An error occurred: " + e.Message);
            }
            return result;
        }

        public List<Thread> GetThreadsAsync(string userId)
        {
            List<Thread> result = new List<Thread>();
            UsersResource.ThreadsResource.ListRequest request = service.Users.Threads.List(userId);

            do
            {
                try
                {
                    ListThreadsResponse response = request.Execute();
                    result.AddRange(response.Threads);
                    request.PageToken = response.NextPageToken;
                }
                catch (Exception e)
                {
                    //Console.WriteLine("An error occurred: " + e.Message);
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return result;
        }

        public List<History> GetHistoryAsync(string userId, ulong startHistoryId)
        {
            List<History> result = new List<History>();
            UsersResource.HistoryResource.ListRequest request = service.Users.History.List(userId);
            request.StartHistoryId = startHistoryId;

            do
            {
                try
                {
                    ListHistoryResponse response = request.Execute();
                    if (response.History != null)
                    {
                        result.AddRange(response.History);
                    }
                    request.PageToken = response.NextPageToken;
                }
                catch (Exception e)
                {
                    //Console.WriteLine("An error occurred: " + e.Message);
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return result;
        }

        public async Task<IEnumerable<Profile>> GetProfilesAsync(string userId)
        {
            await AuthenticateAsync();
            List<Profile> result = new List<Profile>();
            UsersResource.GetProfileRequest request = service.Users.GetProfile(userId);

            try
            {
                Profile response = request.Execute();
                result.Add(response);
            }
            catch (Exception e)
            {

            }
            return result;
        }

        public List<Message> GetMessagesAsync(string userId)
        {
            List<Message> result = new List<Message>();
            UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List(userId);
            
            do
            {
                try
                {
                    ListMessagesResponse response = request.Execute();
                    result.AddRange(response.Messages);
                    request.PageToken = response.NextPageToken;
                }
                catch (Exception e)
                {
                    //Console.WriteLine("An error occurred: " + e.Message);
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return result;
        }

        public async Task<ListMessagesResponse> GetMessagesAsync(string userId, string pageToken)
        {
            await AuthenticateAsync();
            UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List(userId);
            request.PageToken = pageToken;
            ListMessagesResponse response = new ListMessagesResponse();
            try
            {
                response = request.Execute();
            }
            catch (Exception e)
            {
                //Console.WriteLine("An error occurred: " + e.Message);
            }

            return response;
        }

        public async Task<ListMessagesResponse> GetMessagesAsync(string userId, string labelId, long maxResults)
        {
            await AuthenticateAsync();
            UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List(userId);
            request.LabelIds = labelId;
            request.MaxResults = maxResults;
            ListMessagesResponse response = new ListMessagesResponse();
            try
            {
                response = request.Execute();
            }
            catch (Exception e)
            {
                //Console.WriteLine("An error occurred: " + e.Message);
            }
            return response;
        }

        public Message GetMessage(string userId, string messageId)
        {
            var message = service.Users.Messages.Get("me", messageId).Execute();
            return message;
        }

        public async Task<string> GetCount(string userId,string labelId)
        {
            await AuthenticateAsync();
            UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List(userId);
            request.LabelIds = labelId;
            try
            {
                var response = request.Execute();
                return response.ResultSizeEstimate.ToString();
            }
            catch { return null; }
        }

        #endregion
    }
}
