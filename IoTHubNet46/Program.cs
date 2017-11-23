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
        private static string proto;
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
            var conString = ""; // ConfigurationManager.AppSettings["IotHub.ConnectionString"];
            var protocol = "mqtt_websock";//ConfigurationManager.AppSettings["IotHub.Protocol"];
            var timeout = "1";// ConfigurationManager.AppSettings["IotHub.OperationTimeoutMs"];

            proto = GetTransportType(protocol).ToString();
            var deviceClient = DeviceClient.CreateFromConnectionString(conString, GetTransportType(protocol));
            deviceClient.OperationTimeoutInMilliseconds = uint.Parse(timeout);
            //deviceClient.SetRetryPolicy(new MyRetryPolicy());
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
            int count = 0;
            var msg = CreateAzureMessage();
            bool stop = false;
            while (!stop)
            {
                try
                {
                    // Console.WriteLine("Send");
                    await dc.SendEventBatchAsync(msg);
                }
                catch (Exception ex)
                {
                   // Console.WriteLine(ex.ToString());
                }
                count++;

                if(count % 1000 == 0)
                {
                    GC.Collect(2);
                }
                if(count % 50 == 0)
                {
                    Console.WriteLine(count + " " + proto);
                }
                // await Task.Delay(500);
                //stop = true;
            }
        }


        static void Main(string[] args)
        {
            var dv = CreateDeviceClient();
            RunTest(dv).Wait();
            Console.ReadLine();
        }
    }
}
