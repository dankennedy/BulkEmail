using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace BulkEmail
{
    /// <summary>
    ///   Base class used to parse arguments specified on the command line
    /// </summary>
    public abstract class CommandLineArgumentsBase
    {
        private StringDictionary _parameters;

        protected static readonly Regex ArgumentExtractor = new Regex(@"(['""][^""]+['""])\s*|([^\s]+)\s*",
                                                                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected static readonly Regex ArgumentSplitter = new Regex(@"^([/-]|--){1}(?<name>\w+|\?{1})(?<value>.+)?$",
                                                                     RegexOptions.IgnoreCase | RegexOptions.Compiled);

        ///<summary>
        ///  Must be overridden to display a message to the console detailing project specific arguments/options and how
        ///  the application should be called/used
        ///</summary>
        public abstract string UsageMessage { get; }

        /// <summary>
        ///   Must be overriden to provide project specific validation of the arguments specified
        /// </summary>
        /// <param name = "validationErrors">Collection of validation errors used to return messages to the calling function</param>
        /// <returns>True if all arguments, and their combination, are valid otherwise False</returns>
        public abstract bool Validate(IList validationErrors);

        /// <summary>
        ///   Creates a new instance with the tokenized array of command line arguments
        /// </summary>
        /// <param name = "args">Tokenized arguments</param>
        protected CommandLineArgumentsBase(string[] args)
        {
            Extract(args);
        }

        /// <summary>
        ///   Creates a new instance from the command line string used to execute the application
        /// </summary>
        /// <param name = "commandLine">Full command line string including the executable path and arguments</param>
        protected CommandLineArgumentsBase(string commandLine)
        {
            if (commandLine == null)
                throw new ArgumentNullException("commandLine");

            var matches = ArgumentExtractor.Matches(commandLine.Trim());
            var parts = new string[matches.Count - 2];

            // first argument ommitted as enviroment.commandline includes the executable path
            for (var i = 1; i < matches.Count; i++)
                parts[i - 1] = matches[i].Value.Trim();

            Extract(parts);
        }

        /// <summary>
        ///   Extracts the argument/value pairs from the specified string array and builds a dictionary of items
        /// </summary>
        /// <param name = "args">Tokenized array of command line arguments</param>
        private void Extract(string[] args)
        {
            _parameters = new StringDictionary();
            string parameter = null;
            char[] trimChars = {'"'};

            foreach (var arg in args)
            {
                var part = ArgumentSplitter.Match(arg);
                if (!part.Success)
                {
                    // Found a value (for the last parameter found (space separator))
                    if (parameter != null)
                        _parameters[parameter] = arg.Trim(trimChars);
                }
                else
                {
                    // Matched a name, optionally with inline value
                    parameter = part.Groups["name"].Value;
                    _parameters.Add(parameter, part.Groups["value"].Value.Trim(trimChars));
                }
            }
        }

        /// <summary>
        ///   Gets the argument with the specified key
        /// </summary>
        public string this[string key]
        {
            get { return _parameters[key]; }
        }

        /// <summary>
        ///   The collection of all arguments
        /// </summary>
        protected StringDictionary Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        ///   Outputs the collection of arguments and their values
        /// </summary>
        /// <param name = "outputStream">the destination stream to output the parameter details to</param>
        public void Output(TextWriter outputStream)
        {
            if (outputStream == null)
                throw new ArgumentNullException("outputStream");

            foreach (string key in _parameters.Keys)
                outputStream.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0}={1}", key, _parameters[key]));
        }
    }
}