using System;
using System.Collections.Generic;

namespace IdentityService
{
    public static class IdentityServiceFactory
    {
        public static IIdentityService CreateFromJson(string pathToJsonFile)
        {
            // Todo
            
            throw new NotImplementedException();
        }

        public static IIdentityService CreateFromMemory(IEnumerable<string> users, IEnumerable<string> passwords)
        {
            // Todo
            
            throw new NotImplementedException();
        }
    }
}