using Gmail10.Common;
using Gmail10.Repositories;
using Google.Apis.Gmail.v1.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Gmail10.Views
{
    public sealed partial class Shell : Page, INotifyPropertyChanged
    {
        //Shell
        private void Hamburger(object parameter)
        {
            try { ShellSplitView.IsPaneOpen = !ShellSplitView.IsPaneOpen; }
            catch { }
        }

        private async void Inbox(object parameter)
        {
            if (contentFrame.CurrentSourcePageType != typeof(InboxPage))
            {
                this.contentFrame.Navigate(typeof(InboxPage));
                InboxProgressBarVisibility = Visibility.Visible;
                InboxLoadMoreIsEnabled = false;
                GetPrimaryMailsAsync();
                GetSocialMailsAsync();
                GetPromotionsMailsAsync();
                InboxLoadMoreIsEnabled = true;
                InboxProgressBarVisibility = Visibility.Collapsed;
            }
        }

        private void SentMails(object parameter)
        {
            if (contentFrame.CurrentSourcePageType != typeof(SentMailsPage))
            {
                this.contentFrame.Navigate(typeof(SentMailsPage));
                GetSentMailsAsync();
            }
        }

        private void Drafts(object parameter)
        {
            if (contentFrame.CurrentSourcePageType != typeof(DraftsPage))
            {
                this.contentFrame.Navigate(typeof(DraftsPage));
                GetDraftMailsAsync();
            }
        }

        private void SpamMails(object parameter)
        {
            if (contentFrame.CurrentSourcePageType != typeof(SpamMailsPage))
            {
                this.contentFrame.Navigate(typeof(SpamMailsPage));
                GetSpamMailsAsync();
            }
        }

        private void DeletedMails(object parameter)
        {
            if (contentFrame.CurrentSourcePageType != typeof(DeletedMailsPage))
            {
                this.contentFrame.Navigate(typeof(DeletedMailsPage));
                GetTrashMailsAsync();
            }
        }

        private void Settings(object parameter)
        {
            if (contentFrame.CurrentSourcePageType != typeof(SettingsPage))
            {
                this.contentFrame.Navigate(typeof(SettingsPage));
            }
        }

        private async void AddAccount(object parameter)
        {
            //await DatabaseHelper.AddNewAccountAsync();
        }

        private async void Login(object parameter)
        {
            try
            {
                SplashTextBlock.Text = "Please wait ....";
                SplashGrid.Visibility = Visibility.Visible;
                MenuVisibility = Visibility.Collapsed;
                LoginButtonVisibility = Visibility.Collapsed;
                var userIsAuthentic = await _gmailrepository.GetAuthenticated();
                SplashTextBlock.Text = "Opening gmail services ....";
                if (userIsAuthentic)
                {
                    SplashTextBlock.Text = "Getting user info .... ";
                    GetProfilesAsync();
                    GetLabelsAsync();

                    var InboxCount = await Task.Run(() =>_gmailrepository.GetCount("me","INBOX"));
                    if (InboxCount != null)
                        InboxText = "Inbox (" + InboxCount + ")";
                    InboxCount = await Task.Run(() => _gmailrepository.GetCount("me", "SENT"));
                    if (InboxCount != null)
                        SentMailsText = "Sent Mails (" + InboxCount + ")";
                    InboxCount = await Task.Run(() => _gmailrepository.GetCount("me", "DRAFT"));
                    if (InboxCount != null)
                        DraftsText = "Drafts (" + InboxCount + ")";
                    InboxCount = await Task.Run(() => _gmailrepository.GetCount("me", "SPAM"));
                    if (InboxCount != null)
                        SpamMailsText = "Spam Mails (" + InboxCount + ")";
                    InboxCount = await Task.Run(() => _gmailrepository.GetCount("me", "TRASH"));
                    if (InboxCount != null)
                        TrashMailsText = "Trash (" + InboxCount + ")";
                    

                    Inbox(null);
                    this.FindName("GmailCommandBar");
                    this.FindName("GmailWebView");
                    MenuVisibility = Visibility.Visible;
                    SplashGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SplashTextBlock.Text = "Login Failed.Try Again";
                    LoginButtonVisibility = Visibility.Visible;
                }
            }
            catch (Exception e)
            {

            }
        }

        private async void LoadMore(object parameter)
        {
            try
            {
                ListMessagesResponse _response = new ListMessagesResponse();
                if (contentFrame.CurrentSourcePageType != typeof(InboxPage))
                {

                }
                if (contentFrame.CurrentSourcePageType != typeof(SentMailsPage))
                {
                    var last = SentMailsMessages.Last();
                    _response = await Task.Run(() => _gmailrepository.GetMessagesAsync("me",last.NextPageToken));
                }
                if (contentFrame.CurrentSourcePageType != typeof(DraftsPage))
                {
                    var last = DraftsMessages.Last();
                    _response = await Task.Run(() => _gmailrepository.GetMessagesAsync("me", last.NextPageToken));
                }
                if (contentFrame.CurrentSourcePageType != typeof(SpamMailsPage))
                {
                    var last = SpamMailsMessages.Last();
                    _response = await Task.Run(() => _gmailrepository.GetMessagesAsync("me", last.NextPageToken));
                }
                if (contentFrame.CurrentSourcePageType != typeof(DeletedMailsPage))
                {
                    var last = TrashMessages.Last();
                    _response = await Task.Run(() => _gmailrepository.GetMessagesAsync("me", last.NextPageToken));
                }

                var _messages = _response.Messages;
                if (_messages.Count > 0)
                {
                    foreach (var m in _messages)
                    {
                        try
                        {
                            var message = await Task.Run(() => _gmailrepository.GetMessage("me", m.Id));
                            var customMessage = new CustomMessage();
                            customMessage.MessageId = message.Id;
                            customMessage.Snippet = message.Snippet;
                            customMessage.LabelIds = message.LabelIds;
                            customMessage.ThreadId = message.ThreadId;

                            for (int i = 0; i < message.Payload.Headers.Count; i++)
                            {
                                if (message.Payload.Headers[i].Name == "Subject")
                                {
                                    customMessage.Subject = message.Payload.Headers[i].Value;
                                }
                                if (message.Payload.Headers[i].Name == "From")
                                {
                                    var from = message.Payload.Headers[i].Value;
                                    string[] split = from.Split('<');
                                    customMessage.From = split[0];
                                    customMessage.FromEmail = split[1].Replace(">", "");
                                }
                                if (message.Payload.Headers[i].Name == "To")
                                {
                                    customMessage.ToEmail = message.Payload.Headers[i].Value;
                                }
                            }

                            if (message.Payload.Parts != null)
                            {
                                if (message.Payload.Parts[0].Parts != null)
                                {
                                    var part = message.Payload.Parts[0].Parts[1].Body.Data;
                                    var data = Base64UrlDecode(part);
                                    customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                                }
                                else
                                {
                                    try
                                    {
                                        var part = message.Payload.Parts[0].Body.Data + message.Payload.Parts[1].Body.Data;
                                        var data = Base64UrlDecode(part);
                                        customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            var part0 = message.Payload.Parts[0].Body.Data;
                                            var part1 = message.Payload.Parts[1].Body.Data;
                                            var data = Base64UrlDecode(part1);
                                            customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                                        }
                                        catch { }
                                    }
                                }
                            }
                            else
                            {
                                var data = Base64UrlDecode(message.Payload.Body.Data);
                                customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                            }

                            if (message.LabelIds.Contains("STARRED"))
                            {
                                customMessage.Starred = 1;
                            }

                            var datetime = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(message.InternalDate));
                            customMessage.Date = datetime.ToString("MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

                            SentMailsMessages.Add(customMessage);
                        }
                        catch (Exception e) { }
                    }
                }
            }
            catch { }
        }

        private void SelectedMessage(object parameter)
        {
            var item = parameter as ItemClickEventArgs;
            var message = (CustomMessage)item.ClickedItem;
            if(message.Body != null)
            {
                GmailWebView.NavigateToString(message.Body);

                if (Window.Current.Bounds.Width < 800)
                {
                    this.contentFrame.Navigate(typeof(MessagePage), message.Body);
                }
            }
        }

        public async void GetLabelsAsync()
        {
            var labels = await Task.Run(() => _gmailrepository.GetLabelsAsync("me"));

            Labels.Clear();
            foreach (var l in labels)
            {
                Labels.Add(new Label
                {
                    Name = l.Name,
                    Id = l.Id
                });
            }
        }

        public async void GetProfilesAsync()
        {
            var _profiles = await Task.Run(() => _gmailrepository.GetProfilesAsync("me"));

            Profiles.Clear();
            foreach (var p in _profiles)
            {
                Profiles.Add(p);
            }
        }

        public async void GetPrimaryMailsAsync()
        {
            var _response = await Task.Run(() => _gmailrepository.GetMessagesAsync("me", "CATEGORY_PERSONAL", 20));
            var _messages = _response.Messages;
            PersonalMessages.Clear();
            foreach (var m in _messages)
            {
                try
                {
                    var message = await Task.Run(() => _gmailrepository.GetMessage("me", m.Id));
                    var customMessage = new CustomMessage();
                    customMessage.MessageId = message.Id;
                    customMessage.Snippet = message.Snippet;
                    customMessage.LabelIds = message.LabelIds;
                    customMessage.ThreadId = message.ThreadId;

                    for (int i = 0; i < message.Payload.Headers.Count; i++)
                    {
                        if (message.Payload.Headers[i].Name == "Subject")
                        {
                            customMessage.Subject = message.Payload.Headers[i].Value;
                        }
                        if (message.Payload.Headers[i].Name == "From")
                        {
                            var from = message.Payload.Headers[i].Value;
                            string[] split = from.Split('<');
                            customMessage.From = split[0];
                            customMessage.FromEmail = split[1].Replace(">", "");
                        }
                        if (message.Payload.Headers[i].Name == "To")
                        {
                            customMessage.ToEmail = message.Payload.Headers[i].Value;
                        }
                    }

                    if (message.Payload.Parts != null)
                    {
                        if (message.Payload.Parts[0].Parts != null)
                        {
                            var part = message.Payload.Parts[0].Parts[1].Body.Data;
                            var data = Base64UrlDecode(part);
                            customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                        }
                        else
                        {
                            try
                            {
                                var part = message.Payload.Parts[0].Body.Data + message.Payload.Parts[1].Body.Data;
                                var data = Base64UrlDecode(part);
                                customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                            }
                            catch
                            {
                                try
                                {
                                    var part0 = message.Payload.Parts[0].Body.Data;
                                    var part1 = message.Payload.Parts[1].Body.Data;
                                    var data = Base64UrlDecode(part1);
                                    customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        var data = Base64UrlDecode(message.Payload.Body.Data);
                        customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                    }

                    if (message.LabelIds.Contains("STARRED"))
                    {
                        customMessage.Starred = 1;
                    }

                    customMessage.NextPageToken = _response.NextPageToken;
                    
                    var datetime = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(message.InternalDate));
                    customMessage.Date = datetime.ToString("MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

                    PersonalMessages.Add(customMessage);
                }
                catch (Exception e) { }
            }
        }

        public async void GetSocialMailsAsync()
        {
            var _response = await Task.Run(() => _gmailrepository.GetMessagesAsync("me", "CATEGORY_SOCIAL", 20));
            var _messages = _response.Messages;
            SocialMessages.Clear();
            foreach (var m in _messages)
            {
                try
                {
                    var message = await Task.Run(() => _gmailrepository.GetMessage("me", m.Id));
                    var customMessage = new CustomMessage();
                    customMessage.MessageId = message.Id;
                    customMessage.Snippet = message.Snippet;
                    customMessage.LabelIds = message.LabelIds;
                    customMessage.ThreadId = message.ThreadId;

                    for (int i = 0; i < message.Payload.Headers.Count; i++)
                    {
                        if (message.Payload.Headers[i].Name == "Subject")
                        {
                            customMessage.Subject = message.Payload.Headers[i].Value;
                        }
                        if (message.Payload.Headers[i].Name == "From")
                        {
                            var from = message.Payload.Headers[i].Value;
                            string[] split = from.Split('<');
                            customMessage.From = split[0];
                            customMessage.FromEmail = split[1].Replace(">", "");
                        }
                        if (message.Payload.Headers[i].Name == "To")
                        {
                            customMessage.ToEmail = message.Payload.Headers[i].Value;
                        }
                    }

                    if (message.Payload.Parts != null)
                    {
                        if (message.Payload.Parts[0].Parts != null)
                        {
                            var part = message.Payload.Parts[0].Parts[1].Body.Data;
                            var data = Base64UrlDecode(part);
                            customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                        }
                        else
                        {
                            try
                            {
                                var part = message.Payload.Parts[0].Body.Data + message.Payload.Parts[1].Body.Data;
                                var data = Base64UrlDecode(part);
                                customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                            }
                            catch
                            {
                                try
                                {
                                    var part0 = message.Payload.Parts[0].Body.Data;
                                    var part1 = message.Payload.Parts[1].Body.Data;
                                    var data = Base64UrlDecode(part1);
                                    customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        var data = Base64UrlDecode(message.Payload.Body.Data);
                        customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                    }

                    if (message.LabelIds.Contains("STARRED"))
                    {
                        customMessage.Starred = 1;
                    }

                    customMessage.NextPageToken = _response.NextPageToken;

                    var datetime = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(message.InternalDate));
                    customMessage.Date = datetime.ToString("MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

                    SocialMessages.Add(customMessage);
                }
                catch (Exception e) { }
            }
        }

        public async void GetPromotionsMailsAsync()
        {
            var _response = await Task.Run(() => _gmailrepository.GetMessagesAsync("me", "CATEGORY_PROMOTIONS", 20));
            var _messages = _response.Messages;
            PromotionalMessages.Clear();
            foreach (var m in _messages)
            {
                try
                {
                    var message = await Task.Run(() => _gmailrepository.GetMessage("me", m.Id));
                    var customMessage = new CustomMessage();
                    customMessage.MessageId = message.Id;
                    customMessage.Snippet = message.Snippet;
                    customMessage.LabelIds = message.LabelIds;
                    customMessage.ThreadId = message.ThreadId;

                    for (int i = 0; i < message.Payload.Headers.Count; i++)
                    {
                        if (message.Payload.Headers[i].Name == "Subject")
                        {
                            customMessage.Subject = message.Payload.Headers[i].Value;
                        }
                        if (message.Payload.Headers[i].Name == "From")
                        {
                            var from = message.Payload.Headers[i].Value;
                            string[] split = from.Split('<');
                            customMessage.From = split[0];
                            customMessage.FromEmail = split[1].Replace(">", "");
                        }
                        if (message.Payload.Headers[i].Name == "To")
                        {
                            customMessage.ToEmail = message.Payload.Headers[i].Value;
                        }
                    }

                    if (message.Payload.Parts != null)
                    {
                        if (message.Payload.Parts[0].Parts != null)
                        {
                            var part = message.Payload.Parts[0].Parts[1].Body.Data;
                            var data = Base64UrlDecode(part);
                            customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                        }
                        else
                        {
                            try
                            {
                                var part = message.Payload.Parts[0].Body.Data + message.Payload.Parts[1].Body.Data;
                                var data = Base64UrlDecode(part);
                                customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                            }
                            catch
                            {
                                try
                                {
                                    var part0 = message.Payload.Parts[0].Body.Data;
                                    var part1 = message.Payload.Parts[1].Body.Data;
                                    var data = Base64UrlDecode(part1);
                                    customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        var data = Base64UrlDecode(message.Payload.Body.Data);
                        customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                    }

                    if (message.LabelIds.Contains("STARRED"))
                    {
                        customMessage.Starred = 1;
                    }

                    customMessage.NextPageToken = _response.NextPageToken;

                    var datetime = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(message.InternalDate));
                    customMessage.Date = datetime.ToString("MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

                    PromotionalMessages.Add(customMessage);
                }
                catch (Exception e) { }
            }
        }

        public async void GetSentMailsAsync()
        {
            var _response = await Task.Run(() => _gmailrepository.GetMessagesAsync("me", "SENT", 20));
            var _messages = _response.Messages;
            SentMailsMessages.Clear();
            foreach (var m in _messages)
            {
                try
                {
                    var message = await Task.Run(() => _gmailrepository.GetMessage("me", m.Id));
                    var customMessage = new CustomMessage();
                    customMessage.MessageId = message.Id;
                    customMessage.Snippet = message.Snippet;
                    customMessage.LabelIds = message.LabelIds;
                    customMessage.ThreadId = message.ThreadId;

                    for (int i = 0; i < message.Payload.Headers.Count; i++)
                    {
                        if (message.Payload.Headers[i].Name == "Subject")
                        {
                            customMessage.Subject = message.Payload.Headers[i].Value;
                        }
                        if (message.Payload.Headers[i].Name == "From")
                        {
                            var from = message.Payload.Headers[i].Value;
                            string[] split = from.Split('<');
                            customMessage.From = split[0];
                            customMessage.FromEmail = split[1].Replace(">", "");
                        }
                        if (message.Payload.Headers[i].Name == "To")
                        {
                            customMessage.ToEmail = message.Payload.Headers[i].Value;
                        }
                    }

                    if (message.Payload.Parts != null)
                    {
                        if (message.Payload.Parts[0].Parts != null)
                        {
                            var part = message.Payload.Parts[0].Parts[1].Body.Data;
                            var data = Base64UrlDecode(part);
                            customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                        }
                        else
                        {
                            try
                            {
                                var part = message.Payload.Parts[0].Body.Data + message.Payload.Parts[1].Body.Data;
                                var data = Base64UrlDecode(part);
                                customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                            }
                            catch
                            {
                                try
                                {
                                    var part0 = message.Payload.Parts[0].Body.Data;
                                    var part1 = message.Payload.Parts[1].Body.Data;
                                    var data = Base64UrlDecode(part1);
                                    customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        var data = Base64UrlDecode(message.Payload.Body.Data);
                        customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                    }

                    if (message.LabelIds.Contains("STARRED"))
                    {
                        customMessage.Starred = 1;
                    }

                    customMessage.NextPageToken = _response.NextPageToken;

                    var datetime = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(message.InternalDate));
                    customMessage.Date = datetime.ToString("MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

                    SentMailsMessages.Add(customMessage);
                }
                catch (Exception e) { }
            }
        }

        public async void GetDraftMailsAsync()
        {
            var _response = await Task.Run(() => _gmailrepository.GetMessagesAsync("me", "DRAFT", 20));
            var _messages = _response.Messages;
            DraftsMessages.Clear();
            foreach (var m in _messages)
            {
                try
                {
                    var message = await Task.Run(() => _gmailrepository.GetMessage("me", m.Id));
                    var customMessage = new CustomMessage();
                    customMessage.MessageId = message.Id;
                    customMessage.Snippet = message.Snippet;
                    customMessage.LabelIds = message.LabelIds;
                    customMessage.ThreadId = message.ThreadId;

                    for (int i = 0; i < message.Payload.Headers.Count; i++)
                    {
                        if (message.Payload.Headers[i].Name == "Subject")
                        {
                            customMessage.Subject = message.Payload.Headers[i].Value;
                        }
                        if (message.Payload.Headers[i].Name == "From")
                        {
                            var from = message.Payload.Headers[i].Value;
                            string[] split = from.Split('<');
                            customMessage.From = split[0];
                            customMessage.FromEmail = split[1].Replace(">", "");
                        }
                        if (message.Payload.Headers[i].Name == "To")
                        {
                            customMessage.ToEmail = message.Payload.Headers[i].Value;
                        }
                    }

                    if (message.Payload.Parts != null)
                    {
                        if (message.Payload.Parts[0].Parts != null)
                        {
                            var part = message.Payload.Parts[0].Parts[1].Body.Data;
                            var data = Base64UrlDecode(part);
                            customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                        }
                        else
                        {
                            try
                            {
                                var part = message.Payload.Parts[0].Body.Data + message.Payload.Parts[1].Body.Data;
                                var data = Base64UrlDecode(part);
                                customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                            }
                            catch
                            {
                                try
                                {
                                    var part0 = message.Payload.Parts[0].Body.Data;
                                    var part1 = message.Payload.Parts[1].Body.Data;
                                    var data = Base64UrlDecode(part1);
                                    customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        var data = Base64UrlDecode(message.Payload.Body.Data);
                        customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                    }

                    if (message.LabelIds.Contains("STARRED"))
                    {
                        customMessage.Starred = 1;
                    }

                    customMessage.NextPageToken = _response.NextPageToken;

                    var datetime = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(message.InternalDate));
                    customMessage.Date = datetime.ToString("MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

                    DraftsMessages.Add(customMessage);
                }
                catch (Exception e) { }
            }
        }

        public async void GetSpamMailsAsync()
        {
            var _response = await Task.Run(() => _gmailrepository.GetMessagesAsync("me", "SPAM", 20));
            var _messages = _response.Messages;
            SpamMailsMessages.Clear();
            foreach (var m in _messages)
            {
                try
                {
                    var message = await Task.Run(() => _gmailrepository.GetMessage("me", m.Id));
                    var customMessage = new CustomMessage();
                    customMessage.MessageId = message.Id;
                    customMessage.Snippet = message.Snippet;
                    customMessage.LabelIds = message.LabelIds;
                    customMessage.ThreadId = message.ThreadId;

                    for (int i = 0; i < message.Payload.Headers.Count; i++)
                    {
                        if (message.Payload.Headers[i].Name == "Subject")
                        {
                            customMessage.Subject = message.Payload.Headers[i].Value;
                        }
                        if (message.Payload.Headers[i].Name == "From")
                        {
                            var from = message.Payload.Headers[i].Value;
                            string[] split = from.Split('<');
                            customMessage.From = split[0];
                            customMessage.FromEmail = split[1].Replace(">", "");
                        }
                        if (message.Payload.Headers[i].Name == "To")
                        {
                            customMessage.ToEmail = message.Payload.Headers[i].Value;
                        }
                    }

                    if (message.Payload.Parts != null)
                    {
                        if (message.Payload.Parts[0].Parts != null)
                        {
                            var part = message.Payload.Parts[0].Parts[1].Body.Data;
                            var data = Base64UrlDecode(part);
                            customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                        }
                        else
                        {
                            try
                            {
                                var part = message.Payload.Parts[0].Body.Data + message.Payload.Parts[1].Body.Data;
                                var data = Base64UrlDecode(part);
                                customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                            }
                            catch
                            {
                                try
                                {
                                    var part0 = message.Payload.Parts[0].Body.Data;
                                    var part1 = message.Payload.Parts[1].Body.Data;
                                    var data = Base64UrlDecode(part1);
                                    customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        var data = Base64UrlDecode(message.Payload.Body.Data);
                        customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                    }

                    if (message.LabelIds.Contains("STARRED"))
                    {
                        customMessage.Starred = 1;
                    }

                    customMessage.NextPageToken = _response.NextPageToken;

                    var datetime = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(message.InternalDate));
                    customMessage.Date = datetime.ToString("MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

                    SpamMailsMessages.Add(customMessage);
                }
                catch (Exception e) { }
            }
        }

        public async void GetTrashMailsAsync()
        {
            var _response = await Task.Run(() => _gmailrepository.GetMessagesAsync("me", "TRASH", 20));
            var _messages = _response.Messages;
            TrashMessages.Clear();
            if(_messages != null)
            {
                foreach (var m in _messages)
                {
                    try
                    {
                        var message = await Task.Run(() => _gmailrepository.GetMessage("me", m.Id));
                        var customMessage = new CustomMessage();
                        customMessage.MessageId = message.Id;
                        customMessage.Snippet = message.Snippet;
                        customMessage.LabelIds = message.LabelIds;
                        customMessage.ThreadId = message.ThreadId;

                        for (int i = 0; i < message.Payload.Headers.Count; i++)
                        {
                            if (message.Payload.Headers[i].Name == "Subject")
                            {
                                customMessage.Subject = message.Payload.Headers[i].Value;
                            }
                            if (message.Payload.Headers[i].Name == "From")
                            {
                                var from = message.Payload.Headers[i].Value;
                                string[] split = from.Split('<');
                                customMessage.From = split[0];
                                customMessage.FromEmail = split[1].Replace(">", "");
                            }
                            if (message.Payload.Headers[i].Name == "To")
                            {
                                customMessage.ToEmail = message.Payload.Headers[i].Value;
                            }
                        }

                        if (message.Payload.Parts != null)
                        {
                            if (message.Payload.Parts[0].Parts != null)
                            {
                                var part = message.Payload.Parts[0].Parts[1].Body.Data;
                                var data = Base64UrlDecode(part);
                                customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                            }
                            else
                            {
                                try
                                {
                                    var part = message.Payload.Parts[0].Body.Data + message.Payload.Parts[1].Body.Data;
                                    var data = Base64UrlDecode(part);
                                    customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                                }
                                catch
                                {
                                    try
                                    {
                                        var part0 = message.Payload.Parts[0].Body.Data;
                                        var part1 = message.Payload.Parts[1].Body.Data;
                                        var data = Base64UrlDecode(part1);
                                        customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                                    }
                                    catch { }
                                }
                            }
                        }
                        else
                        {
                            var data = Base64UrlDecode(message.Payload.Body.Data);
                            customMessage.Body = System.Text.Encoding.UTF8.GetString(data);
                        }

                        if (message.LabelIds.Contains("STARRED"))
                        {
                            customMessage.Starred = 1;
                        }

                        customMessage.NextPageToken = _response.NextPageToken;

                        var datetime = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(message.InternalDate));
                        customMessage.Date = datetime.ToString("MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

                        TrashMessages.Add(customMessage);
                    }
                    catch (Exception e) { }
                }
            }
        }

        private byte[] Base64UrlDecode(string arg)
        {
            try
            {
                string s = arg;
                s = s.Replace('-', '+'); // 62nd char of encoding
                s = s.Replace('_', '/'); // 63rd char of encoding
                switch (s.Length % 4) // Pad with trailing '='s
                {
                    case 0: break; // No pad chars in this case
                    case 2: s += "=="; break; // Two pad chars
                    case 3: s += "="; break; // One pad char
                    default:
                        throw new System.Exception(
                 "Illegal base64url string!");
                }
                return Convert.FromBase64String(s); // Standard base64 decoder
            }
            catch { return null; }
        }

        private async void Initialize()
        {
            this.HamburgerCommand = new DelegateCommand((parameter) => this.Hamburger(parameter), () => this.CommandEnabled);

            this.InboxCommand = new DelegateCommand((parameter) => this.Inbox(parameter), () => this.CommandEnabled);
            this.SentMailsCommand = new DelegateCommand((parameter) => this.SentMails(parameter), () => this.CommandEnabled);
            this.DraftsCommand = new DelegateCommand((parameter) => this.Drafts(parameter), () => this.CommandEnabled);
            this.SpamMailsCommand = new DelegateCommand((parameter) => this.SpamMails(parameter), () => this.CommandEnabled);
            this.TrashCommand = new DelegateCommand((parameter) => this.DeletedMails(parameter), () => this.CommandEnabled);

            this.SettingsCommand = new DelegateCommand((parameter) => this.Settings(parameter), () => this.CommandEnabled);
            this.AddAccountCommand = new DelegateCommand((parameter) => this.AddAccount(parameter), () => this.CommandEnabled);

            this.LoginCommand = new DelegateCommand((parameter) => this.Login(parameter), () => this.CommandEnabled);

            this.LoadMoreCommand = new DelegateCommand((parameter) => this.LoadMore(parameter), () => this.CommandEnabled);

            this.SelectedMessageCommand =  new DelegateCommand((parameter) => this.SelectedMessage(parameter), () => this.CommandEnabled);

            InboxText = "Inbox";
            SentMailsText = "Sent Mails";
            DraftsText = "Drafts";
            SpamMailsText = "Spam Mails";
            TrashMailsText = "Trash";

            LoginButtonVisibility = Visibility.Collapsed;

            _gmailrepository = Repositories.GmailRepository.GetInstance();

            Profiles = new ObservableCollection<Profile>();
            Labels = new ObservableCollection<Label>();
            PersonalMessages = new ObservableCollection<CustomMessage>();
            SocialMessages = new ObservableCollection<CustomMessage>();
            PromotionalMessages = new ObservableCollection<CustomMessage>();
            SentMailsMessages = new ObservableCollection<CustomMessage>();
            DraftsMessages = new ObservableCollection<CustomMessage>();
            SpamMailsMessages = new ObservableCollection<CustomMessage>();
            TrashMessages = new ObservableCollection<CustomMessage>();

            Login(null);
            //this.GetBlogsCommand = new DelegateCommand(async () => await GetLabelsAsync());
        }

        public Frame contentFrame;
        private readonly IGmailRepository repository;
        Repositories.GmailRepository _gmailrepository;
        public Shell(Frame frame)
        {
            this.contentFrame = frame;
            this.InitializeComponent();
            Initialize();
            this.ShellSplitView.Content = frame;
            this.DataContext = this;
        }

        protected bool CommandEnabled { get { return true; } }


        private string m_InboxText;
        public string InboxText
        {
            get { return m_InboxText; }
            set
            {
                m_InboxText = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("InboxText"));
            }
        }


        private string m_SentMailsText;
        public string SentMailsText
        {
            get { return m_SentMailsText; }
            set
            {
                m_SentMailsText = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SentMailsText"));
            }
        }


        private string m_DraftsText;
        public string DraftsText
        {
            get { return m_DraftsText; }
            set
            {
                m_DraftsText = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("DraftsText"));
            }
        }


        private string m_SpamMailsText;
        public string SpamMailsText
        {
            get { return m_SpamMailsText; }
            set
            {
                m_SpamMailsText = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SpamMailsText"));
            }
        }


        private string m_TrashMailsText;
        public string TrashMailsText
        {
            get { return m_TrashMailsText; }
            set
            {
                m_TrashMailsText = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("TrashMailsText"));
            }
        }


        private Visibility m_MenuVisibility;
        public Visibility MenuVisibility
        {
            get { return m_MenuVisibility; }
            set
            {
                m_MenuVisibility = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("MenuVisibility"));
            }
        }


        private Visibility m_LoginButtonVisibility;
        public Visibility LoginButtonVisibility
        {
            get { return m_LoginButtonVisibility; }
            set
            {
                m_LoginButtonVisibility = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("LoginButtonVisibility"));
            }
        }


        private Visibility m_InboxProgressBarVisibility;
        public Visibility InboxProgressBarVisibility
        {
            get { return m_InboxProgressBarVisibility; }
            set
            {
                m_InboxProgressBarVisibility = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("InboxProgressBarVisibility"));
            }
        }
        

        private bool m_InboxLoadMoreIsEnabled;
        public bool InboxLoadMoreIsEnabled
        {
            get { return m_InboxLoadMoreIsEnabled; }
            set
            {
                m_InboxLoadMoreIsEnabled = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("InboxLoadMoreIsEnabled"));
            }
        }
        

        public ICommand HamburgerCommand { get; set; }

        public ICommand InboxCommand { get; set; }
        public ICommand SentMailsCommand { get; set; }
        public ICommand DraftsCommand { get; set; }
        public ICommand SpamMailsCommand { get; set; }
        public ICommand TrashCommand { get; set; }

        public ICommand SettingsCommand { get; set; }
        public ICommand AddAccountCommand { get; set; }

        public ICommand LoginCommand { get; set; }

        public ICommand LoadMoreCommand { get; set; }

        public ICommand SelectedMessageCommand { get; set; }

        public ObservableCollection<Label> Labels { get; private set; }

        public ObservableCollection<Profile> Profiles { get; private set; }

        public ObservableCollection<CustomMessage> PersonalMessages { get; private set; }
        public ObservableCollection<CustomMessage> SocialMessages { get; private set; }
        public ObservableCollection<CustomMessage> PromotionalMessages { get; private set; }
        public ObservableCollection<CustomMessage> SentMailsMessages { get; private set; }
        public ObservableCollection<CustomMessage> DraftsMessages { get; private set; }
        public ObservableCollection<CustomMessage> SpamMailsMessages { get; private set; }
        public ObservableCollection<CustomMessage> TrashMessages { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    
    public class CustomMessage : INotifyPropertyChanged
    {

        private string m_MessageId;
        public string MessageId
        {
            get { return m_MessageId; }
            set
            {
                m_MessageId = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("MessageId"));
            }
        }


        private string m_From;
        public string From
        {
            get { return m_From; }
            set
            {
                m_From = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("From"));
            }
        }


        private string m_FromEmail;
        public string FromEmail
        {
            get { return m_FromEmail; }
            set
            {
                m_FromEmail = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("FromEmail"));
            }
        }
        
        private string m_ToEmail;
        public string ToEmail
        {
            get { return m_ToEmail; }
            set
            {
                m_ToEmail = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ToEmail"));
            }
        }
        
        private string m_Subject;
        public string Subject
        {
            get { return m_Subject; }
            set
            {
                m_Subject = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Subject"));
            }
        }

        private string m_Body;
        public string Body
        {
            get { return m_Body; }
            set
            {
                m_Body = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Body"));
            }
        }

        private string m_Date;
        public string Date
        {
            get { return m_Date; }
            set
            {
                m_Date = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Date"));
            }
        }

        private IEnumerable<string> m_LabelIds;
        public IEnumerable<string> LabelIds
        {
            get { return m_LabelIds; }
            set
            {
                m_LabelIds = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("LabelIds"));
            }
        }
        
        private string m_ThreadId;
        public string ThreadId
        {
            get { return m_ThreadId; }
            set
            {
                m_ThreadId = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ThreadId"));
            }
        }
        
        private string m_Snippet;
        public string Snippet
        {
            get { return m_Snippet; }
            set
            {
                m_Snippet = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Snippet"));
            }
        }


        private int m_Starred;
        public int Starred
        {
            get { return m_Starred; }
            set
            {
                m_Starred = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Starred"));
            }
        }


        private string m_NextPageToken;
        public string NextPageToken
        {
            get { return m_NextPageToken; }
            set
            {
                m_NextPageToken = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("NextPageToken"));
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
    }
}
