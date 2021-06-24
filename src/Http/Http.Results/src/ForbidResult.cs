// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Http.Result
{
    internal sealed partial class ForbidResult : IResult
    {
        public IList<string> AuthenticationSchemes { get; init; } = Array.Empty<string>();

        public AuthenticationProperties? Properties { get; init; }

        async Task IResult.ExecuteAsync(HttpContext httpContext)
        {
            var logger = httpContext.RequestServices.GetRequiredService<ILogger<ForbidResult>>();

            Log.ForbidResultExecuting(logger, AuthenticationSchemes);

            if (AuthenticationSchemes != null && AuthenticationSchemes.Count > 0)
            {
                for (var i = 0; i < AuthenticationSchemes.Count; i++)
                {
                    await httpContext.ForbidAsync(AuthenticationSchemes[i], Properties);
                }
            }
            else
            {
                await httpContext.ForbidAsync(Properties);
            }
        }


        private static partial class Log
        {
            public static void ForbidResultExecuting(ILogger logger, IList<string> authenticationSchemes)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    ForbidResultExecuting(logger, authenticationSchemes.ToArray());
                }
            }

            [LoggerMessage(1, LogLevel.Information, "Executing ChallengeResult with authentication schemes ({Schemes}).", EventName = "ChallengeResultExecuting", SkipEnabledCheck = true)]
            private static partial void ForbidResultExecuting(ILogger logger, string[] schemes);
        }

    }
}
