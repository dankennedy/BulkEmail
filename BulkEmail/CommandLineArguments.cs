using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BulkEmail
{
	internal class CommandLineArguments : CommandLineArgumentsBase
    {
        private const string ARG_HELP = "?";
        private const string ARG_SUBJECT = "s";
        private const string ARG_FROM = "f";
        private const string ARG_MSGFILE = "msgf";
        private const string ARG_RECIPIENTFILE = "rcpf";
        private const string ARG_HTML = "html";
        private const string ARG_BATCHSIZE = "b";

        private static readonly string[] AllArgs = {
                                                       ARG_HELP,
                                                       ARG_SUBJECT,
                                                       ARG_FROM,
                                                       ARG_MSGFILE,
                                                       ARG_RECIPIENTFILE,
                                                       ARG_BATCHSIZE
                                                   };
        public CommandLineArguments(string[] args) : base(args)
        {
            Array.Sort(AllArgs);
        }
        public CommandLineArguments(string commandLine) : base(commandLine)
        {
            Array.Sort(AllArgs);
        }

        public bool HelpRequested
        {
            get { return Parameters.ContainsKey(ARG_HELP); }
        }

        public string Subject
        {
            get { return Parameters[ARG_SUBJECT] ?? String.Empty; }
        }

        public string From
        {
            get { return Parameters[ARG_FROM] ?? String.Empty; }
        }

        public string MessageFilePath
        {
            get { return Parameters[ARG_MSGFILE] ?? ""; }
        }

        public string RecipientFilePath
        {
            get { return Parameters[ARG_RECIPIENTFILE] ?? ""; }
        }

        public bool IsHtml
        {
            get { return Parameters.ContainsKey(ARG_HTML); }
        }


        public int BatchSize
        {
            get
            {
                var stringBatch = Parameters[ARG_BATCHSIZE] ?? String.Empty;
                if (string.IsNullOrEmpty(stringBatch))
                    return 50;

                int test;
                if (!int.TryParse(stringBatch, out test))
                    throw new ArgumentException(string.Format("Invalid batch szie specified ({0})", stringBatch), "Batch");

                return test;
            }
        }

        public override string UsageMessage
        {
            get
            {
                return string.Concat("BulkEmail - Sends bulk emails out\r\n\r\n",
									 "Usage: BulkEmail.exe\r\n",
                                     "             [-?]\r\n",
                                     "             [-s Subject]\r\n",
                                     "             [-f From]\r\n",
                                     "             [-msgf Path to file containing message text]\r\n",
                                     "             [-rcpf Path to file containing email addresses]\r\n",
                                     "             [-b Batch size, default 50]\r\n",
                                     "             [-html Send in Html format, default false]\r\n",
                                     "\r\n",
                                     "Examples:\r\n",
                                     "BulkEmail.exe -s \"Hello world\" -f me@my.com -msgf MessageFile.txt -rcpf Recipients.txt\r\n",
                                     "BulkEmail.exe -s \"Hello world\" -f me@my.com -msgf MessageFile.txt -rcpf Recipients.txt -b 100 -html\r\n");
            }
        }

        public override bool Validate(IList validationErrors)
        {
            if (validationErrors == null)
                validationErrors = new List<string>();

            var errorCountOnEntry = validationErrors.Count;

            foreach (string param in Parameters.Keys)
            {
                if (Array.BinarySearch(AllArgs, param) < 0)
                    validationErrors.Add(String.Format("Invalid argument '{0}'", param));
            }

            if (string.IsNullOrEmpty(MessageFilePath) || !File.Exists(MessageFilePath))
                validationErrors.Add("Invalid message file path");

            if (string.IsNullOrEmpty(RecipientFilePath) || !File.Exists(RecipientFilePath))
                validationErrors.Add("Invalid recipient file path");

            if (string.IsNullOrEmpty(Subject))
                validationErrors.Add("No subject specified");

            if (string.IsNullOrEmpty(From))
                validationErrors.Add("No from address specified");

            return validationErrors.Count == errorCountOnEntry;
        }
    }
}