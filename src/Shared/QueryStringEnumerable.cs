// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Internal
{
    // A mechanism for reading key/value pairs from a querystring without having
    // to allocate and populate an entire dictionary
    internal readonly struct QueryStringEnumerable
    {
        private readonly string _queryString;

        public QueryStringEnumerable(string queryString)
        {
            _queryString = queryString;
        }

        public Enumerator GetEnumerator()
            => new Enumerator(_queryString);

        public struct Enumerator
        {
            private readonly string queryString;
            private readonly int textLength;
            private int scanIndex;
            private int equalIndex;
            private string? currentName;
            private string? currentValue;

            public Enumerator(string? query)
            {
                if (string.IsNullOrEmpty(query))
                {
                    this = default;
                    queryString = string.Empty;
                }
                else
                {
                    currentName = null;
                    currentValue = null;
                    queryString = query;
                    scanIndex = queryString[0] == '?' ? 1 : 0;
                    textLength = queryString.Length;
                    equalIndex = queryString.IndexOf('=');
                    if (equalIndex == -1)
                    {
                        equalIndex = textLength;
                    }
                }
            }

            public (string Key, string Value) Current
                => (currentName!, currentValue!);

            public bool MoveNext()
            {
                currentName = null;
                currentValue = null;

                if (scanIndex < textLength)
                {
                    var delimiterIndex = queryString.IndexOf('&', scanIndex);
                    if (delimiterIndex == -1)
                    {
                        delimiterIndex = textLength;
                    }

                    if (equalIndex < delimiterIndex)
                    {
                        while (scanIndex != equalIndex && char.IsWhiteSpace(queryString[scanIndex]))
                        {
                            ++scanIndex;
                        }

                        var name = queryString.Substring(scanIndex, equalIndex - scanIndex);
                        var value = queryString.Substring(equalIndex + 1, delimiterIndex - equalIndex - 1);
                        currentName = Uri.UnescapeDataString(name.Replace('+', ' '));
                        currentValue = Uri.UnescapeDataString(value.Replace('+', ' '));

                        equalIndex = queryString.IndexOf('=', delimiterIndex);
                        if (equalIndex == -1)
                        {
                            equalIndex = textLength;
                        }
                    }
                    else
                    {
                        if (delimiterIndex > scanIndex)
                        {
                            var name = queryString.Substring(scanIndex, delimiterIndex - scanIndex);
                            currentName = Uri.UnescapeDataString(name.Replace('+', ' '));
                            currentValue = string.Empty;
                        }
                    }

                    scanIndex = delimiterIndex + 1;
                }

                return currentName is not null;
            }
        }
    }
}
