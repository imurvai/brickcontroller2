using System;
using System.Collections.Generic;
using Android.Bluetooth.LE;
using Android.Runtime;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public class BluetoothLENewScanner : ScanCallback
    {
        private readonly Action<BrickController2.PlatformServices.BluetoothLE.ScanResult> _scanCallback;

        public BluetoothLENewScanner(Action<BrickController2.PlatformServices.BluetoothLE.ScanResult> scanCallback)
        {
            _scanCallback = scanCallback;
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult? result)
        {
            if (result is null ||
                result.ScanRecord is null ||
                string.IsNullOrEmpty(result?.Device?.Name) ||
                string.IsNullOrEmpty(result?.Device?.Address))
            {
                return;
            }

            var bytes = result.ScanRecord.GetBytes();
            if (bytes is null)
            {
                return;
            }

            var advertismentData = ScanRecordProcessor.GetAdvertismentData(bytes);
            _scanCallback(new BrickController2.PlatformServices.BluetoothLE.ScanResult(result.Device.Name, result.Device.Address, advertismentData));
        }

        public override void OnBatchScanResults(IList<ScanResult>? results)
        {
            if (results is null)
            {
                return;
            }

            foreach (var result in results)
            {
                if (result is not null)
                {
                    OnScanResult(ScanCallbackType.AllMatches, result);
                }
            }
        }

        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
        }
    }
}