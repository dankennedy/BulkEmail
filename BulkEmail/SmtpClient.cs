using System;
using System.Net.Mail;
using System.Reflection;

namespace BulkEmail
{
    public class SmtpClient
    {
        public void Send(MailMessage message)
        {
            var client = new System.Net.Mail.SmtpClient();
            DisableNtlmAuthentication(client);
            client.Send(message);
        }

        private static void DisableNtlmAuthentication(System.Net.Mail.SmtpClient client)
        {
            var transport = client.GetType().GetField("transport", BindingFlags.NonPublic | BindingFlags.Instance);

            var authModules = transport.GetValue(client).GetType()
                                             .GetField("authenticationModules",
                                                       BindingFlags.NonPublic | BindingFlags.Instance);

            var modulesArray = authModules.GetValue(transport.GetValue(client)) as Array;
            modulesArray.SetValue(modulesArray.GetValue(2), 1);
        }
    }
}