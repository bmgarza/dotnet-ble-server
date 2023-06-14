﻿using System.Threading.Tasks;
using DotnetBleServer.Advertisements;
using DotnetBleServer.Core;

namespace Examples
{
    public class SampleAdvertisement
    {
        public static async Task RegisterSampleAdvertisement(string bluezAdapterPath, ServerContext serverContext)
        {
            var advertisementProperties = new AdvertisementProperties
            {
                Type = "peripheral",
                ServiceUUIDs = new[] { "12345678-1234-5678-1234-56789abcdef0"},
                LocalName = "A",
            };

            await new AdvertisingManager(serverContext, bluezAdapterPath).CreateAdvertisement(advertisementProperties);
        }
    }
}