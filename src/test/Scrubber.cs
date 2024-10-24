// ---------------------------------------------------------------------------
// <copyright file="Scrubber.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace ScrubberNamespace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class Scrubber
    {
        public const string EmailRegExPattern = @"[a-zA-Z0-9!#$+\-^_~]+(?:\.[a-zA-Z0-9!#$+\-^_~]+)*@(?:[a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,6}";
        public static string ScrubData(string data, char replacementChar)
        {
            Regex rx = new Regex(EmailRegExPattern);
            foreach (Match match in rx.Matches(data))
            {
                string replacementString = new string(replacementChar, match.Value.Length);
                data = data.Replace(match.Value, replacementString);
            }

            return data;
        }
    }
}