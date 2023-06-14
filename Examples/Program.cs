using System;
using System.Threading.Tasks;
using DotnetBleServer.Core;
using DotnetBleServer.Utilities;

namespace Examples
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hello Bluetooth");
            Console.ReadKey();

            Task.Run(async () =>
            {
                using (var serverContext = new ServerContext())
                {
                    await serverContext.Connect();
                    string bluetoothAdapter = (await BluetoothAdapterUtils.GetBluetoothAdapters(serverContext.Connection))[0];
                    await SampleAdvertisement.RegisterSampleAdvertisement(bluetoothAdapter, serverContext);
                    await SampleGattApplication.RegisterGattApplication(bluetoothAdapter, serverContext);

                    Console.WriteLine("Press CTRL+C to quit");
                    await Task.Delay(-1);
                }
            }).Wait();
        }
    }
}
