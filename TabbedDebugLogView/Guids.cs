// Guids.cs
// MUST match guids.h
using System;

namespace DarknessvsLightness.TabbedDebugLogView
{
    static class GuidList
    {
        public const string guidTabbedDebugLogViewPkgString = "b3476850-a371-4b55-b698-3cf5dcc38fd3";
        public const string guidTabbedDebugLogViewCmdSetString = "9120329b-187c-4019-9c25-7d9a3c03000a";
        public const string guidToolWindowPersistanceString = "940a189d-a66d-4c5c-b63d-f22671b016ea";

        public static readonly Guid guidTabbedDebugLogViewCmdSet = new Guid(guidTabbedDebugLogViewCmdSetString);
    };
}