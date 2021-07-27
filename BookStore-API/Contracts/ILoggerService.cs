using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Contracts
{
    public interface ILoggerService
    {
        void LogInfo(String message);

        void LogWarn(String message);

        void LogDebug(String message);

        void LogError(string message);
    }
}
