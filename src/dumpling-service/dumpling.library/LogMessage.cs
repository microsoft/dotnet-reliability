// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace DumplingLib
{
    public class LogMessage
    {
        public string MessageType { get; set; }
        public string Message { get; set; }

        public LogMessage(string message_type, string message)
        {
            MessageType = message_type;
            Message = message;
        }

        public static LogMessage Error(string message)
        {
            return new LogMessage("error", message);
        }
        public static LogMessage Informational(string message)
        {
            return new LogMessage("information", message);
        }
        public static LogMessage Verbose(string message)
        {
            return new LogMessage("verbose", message);
        }
    }
}
