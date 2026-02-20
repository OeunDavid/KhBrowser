using OpenPop.Mime;
using OpenPop.Pop3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToolKHBrowser.ToolLib.Mail
{
    public interface IClientEmail
    {
        string Host { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        int Port { get; set; }
        void connect();
        void DeleteAllMessages(int count = 1000);
        bool Connected { get; }
        Message GetMessage(int count=10);
        EmailContent LastContent();
        int TotalEmail();
        string GetVerifyPrimary(string realEmail, string verifyEmail, string password);
    }
    public class ClientEmail:IClientEmail
    {
        private Pop3Client pop3Client;
        private string _host;
        private string _password;
        private int _port;
        private string _username;
        private bool _connected;
        public ClientEmail()
        {
            pop3Client = new Pop3Client();
        }
        public string Host { get => _host; set => _host = value; }
        public string Password { get => _password; set => _password = value; }
        public int Port { get => _port; set => _port = value; }
        public string Username { get => _username; set => _username = value; }
        public bool Connected { get => _connected; }

        public void connect()
        {
            pop3Client.Connect(Host, Port, true);
            pop3Client.Authenticate(Username, Password, AuthenticationMethod.UsernameAndPassword);
            _connected = true;
        }
        public Message GetMessage(int count = 10)
        {
            if (Connected)
            {
                return pop3Client.GetMessage(count);
            }

            return null;
        }
        public void DeleteAllMessages(int count = 1000)
        {
            if (Connected)
            {
                pop3Client.DeleteAllMessages();
            }
        }
        public EmailContent LastContent()
        {
            Message message = pop3Client.GetMessage(30);
            if (message != null)
            {
                string subject = message.Headers.Subject;
                string from = message.Headers.From.Address;
                string to = message.Headers.To.FirstOrDefault().Address;
                string body = "";
                var msgPart = message.FindFirstPlainTextVersion();
                if (msgPart != null && msgPart.IsText)
                {
                    body = msgPart.GetBodyAsText();
                }
                else
                {
                    body = message.MessagePart.GetBodyAsText();
                }
                return new EmailContent
                {
                    Subject = subject,
                    From = from,
                    Body = body,
                    To = to
                };

            }
            return null;
        }
        public EmailContent GetTextMessage(int index)
        {
            Message message = pop3Client.GetMessage(index);
            if (message != null)
            {
                string subject = message.Headers.Subject;
                string from = message.Headers.From.Address;
                string to = message.Headers.To.FirstOrDefault().Address;
                string body = "";
                var msgPart = message.FindFirstPlainTextVersion();
                if (msgPart != null && msgPart.IsText)
                {
                    body = msgPart.GetBodyAsText();
                }
                else
                {
                    body = message.MessagePart.GetBodyAsText();
                }
                return new EmailContent
                {
                    Subject = subject,
                    From = from,
                    Body = body,
                    To = to
                };

            }
            return null;
        }
        public int TotalEmail()
        {
            if (Connected)
            {
                return pop3Client.GetMessageCount();
            }
            return 0;
        }
        public string GetVerifyCode(string realEmail, string verifyEmail, string password)
        {
            string code = "";
            ClientEmail smtpEmail = new ClientEmail();
            smtpEmail.Username = realEmail;
            smtpEmail.Password = password;
            smtpEmail.Port = 995;
            smtpEmail.Host = "pop.yandex.com";
            smtpEmail.connect();
            int counter = 5, preTotal = 0;
            bool isStop = false;
            do
            {
                int total = smtpEmail.TotalEmail();
                if (total > preTotal)
                {
                    int num = total - preTotal;
                    preTotal = total;
                    for (int i = 1; i <= 10 && num > 0; i++)
                    {
                        var message = smtpEmail.GetTextMessage(i);
                        Console.WriteLine("Subject: " + message.Subject + ", Email: " + message.To + ", Verify code: " + message.Body);
                        if (message.To.Contains(verifyEmail))
                        {
                            string subject = message.Subject;
                            if (subject.Contains("FB-"))
                            {
                                code = subject.Substring(3, 5);
                                isStop = true;
                                break;
                            }
                        }
                        if (i >= num)
                        {
                            isStop = true;
                            break;
                        }
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    isStop = true;
                }
            } while (!isStop && counter-- > 0);

            return code;
        }
        public string GetVerifyPrimary(string realEmail, string verifyEmail, string password)
        {
            string code = "";
            ClientEmail smtpEmail = new ClientEmail();
            smtpEmail.Username = realEmail;
            smtpEmail.Password = password;
            smtpEmail.Port = 995;
            smtpEmail.Host = "pop.yandex.com";
            smtpEmail.connect();

            //smtpEmail.DeleteAllMessages();

            int counter = 5, preTotal = 0;
            bool isStop = false;
            int totalMail = 20;
            do
            {
                int total = smtpEmail.TotalEmail();
                if (total > preTotal)
                {
                    int num = total - preTotal;
                    preTotal = total;
                    for (int i = 1; i <= totalMail && num > 0 && !isStop; i++)
                    {
                        var message = smtpEmail.GetTextMessage(i);
                        //Console.WriteLine("Subject: " + message.Subject + ", Email: " + message.To + ", Verify code: " + message.Body);
                        if (message.To.Contains(verifyEmail))
                        {
                            string[] arr = message.Body.Split('\n');
                            for (int j = 0; j < arr.Length && !isStop; j++)
                            {
                                if (arr[j].Contains("confirmation code:"))
                                {
                                    string[] a = arr[j].Split(':');
                                    if (a.Length > 1)
                                    {
                                        try
                                        {
                                            code = a[1].Trim().Substring(0, 5);
                                            isStop = true;
                                            break;
                                        }
                                        catch (Exception) { }
                                    }
                                }
                            }
                        }
                        if (i >= num)
                        {
                            isStop = true;
                            break;
                        }
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    isStop = true;
                }
            } while (!isStop && counter-- > 0);

            return code;
        }
    }
    public class EmailContent
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
    }
}
