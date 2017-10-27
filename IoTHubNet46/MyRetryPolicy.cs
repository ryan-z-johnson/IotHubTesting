using System;
using Microsoft.Azure.Devices.Client;

namespace IoTHubNet46
{
    internal class MyRetryPolicy : IRetryPolicy
    {
        public bool ShouldRetry(int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
        {
            retryInterval = new TimeSpan(1);
            return true;
        }
    }
}