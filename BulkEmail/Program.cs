using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using log4net;

namespace BulkEmail
{
    internal class Program
    {
        public const string EmailRegexPattern =
    @"^[\w-'\.]+(\.[\w-'\.]+)*@([a-z0-9-]+(\.[a-z0-9-]+)*?\.[a-z]{2,6}|(\d{1,3}\.){3}\d{1,3})(:\d{4})?$";

        static readonly Regex EmailRegex = new Regex(EmailRegexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        public static CommandLineArguments Options { get; private set; }

        public static ILog Log { get; set; }

        private static int Main(string[] args)
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();
                Log = LogManager.GetLogger(typeof(Program));

                Options = new CommandLineArguments(args);

                if (Options.HelpRequested)
                {
                    Log.Info(Options.UsageMessage);
                    return 0;
                }

                var validationErrors = new List<string>();
                if (!Options.Validate(validationErrors))
                {
                    foreach (var error in validationErrors)
                        Log.Error(error);

                    return -1;
                }

                SendMail();

                Log.Info("Finished");
                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return -1;
            }
        }

        private static void SendMail()
        {
            var messageBody = File.ReadAllText(Options.MessageFilePath);
            var subject = Options.Subject;
            var smtpClient = new SmtpClient();

            using (var reader = File.OpenText(Options.RecipientFilePath))
            {
                Log.InfoFormat("Opened recipient file {0}", Options.RecipientFilePath);

                string line;
                var msg = new MailMessage
                    {
                        Subject = subject, 
                        Body = messageBody, 
                        IsBodyHtml = Options.IsHtml, 
                        From = new MailAddress(Options.From)
                    };

                if (!string.IsNullOrEmpty(Options.AttachmentFilePath))
                    msg.Attachments.Add(new Attachment(Options.AttachmentFilePath));

                while ((line = reader.ReadLine()) != null)
                {
                    if (!EmailRegex.IsMatch(line))
                    {
                        Log.WarnFormat("Invalid email address {0}", line);
                        continue;
                    }

                    msg.Bcc.Add(new MailAddress(line));

                    if (msg.Bcc.Count < Options.BatchSize) 
                        continue;

                    smtpClient.Send(msg);
                    Log.InfoFormat("Sent email to batch of " + Options.BatchSize);

                    msg = new MailMessage
                        {
                            Subject = subject,
                            Body = messageBody,
                            IsBodyHtml = Options.IsHtml,
                            From = new MailAddress(Options.From)
                        };

                    if (!string.IsNullOrEmpty(Options.AttachmentFilePath))
                        msg.Attachments.Add(new Attachment(Options.AttachmentFilePath));
                }

                if (msg.Bcc.Count <= 0) 
                    return;

                smtpClient.Send(msg);
                Log.InfoFormat("Sent email to batch of " + msg.Bcc.Count);
            }
        }
    }
}
