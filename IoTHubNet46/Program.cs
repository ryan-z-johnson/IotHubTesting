using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace IoTHubNet46
{
    class Program
    {
        private static TransportType GetTransportType(string protocol)
        {
            switch (protocol.ToLowerInvariant())
            {
                case "http": return TransportType.Http1;
                case "amqp": return TransportType.Amqp;
                case "amqp_tcp": return TransportType.Amqp_Tcp_Only;
                case "amqp_websock": return TransportType.Amqp_WebSocket_Only;
                case "mqtt": return TransportType.Mqtt;
                case "mqtt_tcp": return TransportType.Mqtt_Tcp_Only;
                case "mqtt_websock": return TransportType.Mqtt_WebSocket_Only;
                default: return TransportType.Amqp_WebSocket_Only;
            }
        }

        private static DeviceClient CreateDeviceClient()
        {
            var conString = ConfigurationManager.AppSettings["IotHub.ConnectionString"];
            var protocol = ConfigurationManager.AppSettings["IotHub.Protocol"];
            var timeout = ConfigurationManager.AppSettings["IotHub.OperationTimeoutMs"];

            var deviceClient = DeviceClient.CreateFromConnectionString(conString, GetTransportType(protocol));
            deviceClient.OperationTimeoutInMilliseconds = uint.Parse(timeout);
            deviceClient.RetryPolicy = RetryPolicyType.No_Retry;

            return deviceClient;
        }

        private static IEnumerable<Message> CreateAzureMessage()
        {
            var rnd = new Random();
            var list = new List<Message>();
            for (var i = 0; i < 5; i++)
            {
                var bytes = new byte[10];
                rnd.NextBytes(bytes);
                var msg = new Message(bytes);
                msg.Properties["dummytest"] = "yes";
                list.Add(msg);
            }
            return list;
        }

        private static async Task RunTest(DeviceClient dc)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Send");
                    var msg = CreateAzureMessage();
                    await dc.SendEventBatchAsync(msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                await Task.Delay(500);
            }
        }


        static void Main(string[] args)
        {
            var dv = CreateDeviceClient();
            RunTest(dv).Wait();
        }
    }
}
