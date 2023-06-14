
using DotnetBleServer.Advertisements;
using DotnetBleServer.Core;
using DotnetBleServer.Gatt;
using DotnetBleServer.Gatt.Description;
using DotnetBleServer.Utilities;
using System.Text;

namespace Ahsoka.Example;

public class Example
{
    internal class ExampleCharacteristicSource : ICharacteristicSource
    {
        // NOTE: This if the function that is going to be run whenever a write request is going to be received.
        public Task WriteValueAsync(byte[] value)
        {
            Console.WriteLine("Writing value");
            return Task.Run(() => Console.WriteLine(Encoding.ASCII.GetChars(value)));
        }

        // NOTE: In this example this function is going to be the one that is going to take care of returning the value
        // that is going to be received by the BLE host. i.e. the phone running nrf connect.
        public Task<byte[]> ReadValueAsync()
        {
            Console.WriteLine("Reading value");
            return Task.FromResult(Encoding.ASCII.GetBytes("Hello BLE"));
        }
    }

    public static async Task Main(string[] args)
    {
        Console.WriteLine("hello");
        using (var serverContext = new ServerContext())
        {
            await serverContext.Connect();
            string bluetoothAdapter = (await BluetoothAdapterUtils.GetBluetoothAdapters(serverContext.Connection))[0];

            // Trying to turn on the bluetooth device within the code.
            Console.WriteLine("Trying to turn on the bluetooth adapter.");
            IAdapter1 BLEAdapter = serverContext.Connection.CreateProxy<IAdapter1>("org.bluez", bluetoothAdapter);
            await Adapter1Extensions.SetPoweredAsync(BLEAdapter, true);
            if (!await Adapter1Extensions.GetPoweredAsync(BLEAdapter))
                throw new Exception("This didn't work.");

            var advertisementProperties = new AdvertisementProperties
            {
                Type = "peripheral",
                ServiceUUIDs = new[] { "12345678-1234-5678-1234-56789abcdef0"},
                LocalName = "Something",
            };

            AdvertisingManager adManager = new AdvertisingManager(serverContext, bluetoothAdapter);
            await adManager.CreateAdvertisement(advertisementProperties);

            // NOTE: I really don't like these description objects, they are literally just the DBus objects, but
            // simplified and don't provide anything meaningful to the library...
            var gattServiceDescription = new GattServiceDescription
            {
                UUID = "12345678-1234-5678-1234-56789abcdef0",
                Primary = true
            };

            // NOTE: I really don't like these description objects, they are literally just the DBus objects, but
            // simplified and don't provide anything meaningful to the library...
            var gattCharacteristicDescription = new GattCharacteristicDescription
            {
                CharacteristicSource = new ExampleCharacteristicSource(),
                UUID = "12345678-1234-5678-1234-56789abcdef1",
                Flags = CharacteristicFlags.Read | CharacteristicFlags.Write | CharacteristicFlags.WritableAuxiliaries
            };

            // NOTE: I really don't like these description objects, they are literally just the DBus objects, but
            // simplified and don't provide anything meaningful to the library...
            var gattDescriptorDescription = new GattDescriptorDescription
            {
                Value = new[] {(byte) 't'},
                UUID = "12345678-1234-5678-1234-56789abcdef2",
                Flags = new[] {"read", "write"}
            };

            // Add all the services and characteristics we are interested in before we register the app.
            var gab = new GattApplicationBuilder();
            gab.AddService(gattServiceDescription);
            gattServiceDescription.AddCharacteristic(gattCharacteristicDescription);
            gattCharacteristicDescription.AddDescriptor(gattDescriptorDescription);

            // Register the application that we want to be discoverable.
            var gattAppManager = new GattApplicationManager(serverContext, bluetoothAdapter);
            await gattAppManager.RegisterGattApplication(gab.BuildServiceDescriptions());

            Console.WriteLine("Press CTRL+C to quit");
            await Task.Delay(-1);
        }
    }
}
