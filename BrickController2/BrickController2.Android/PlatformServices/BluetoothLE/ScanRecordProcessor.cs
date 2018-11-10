using System.Collections.Generic;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public static class ScanRecordProcessor
    {
        public static IDictionary<byte, byte[]> GetAdvertismentData(byte[] scanRecord)
        {
            var advertismentData = new Dictionary<byte, byte[]>();

            if (scanRecord == null)
            {
                return advertismentData;
            }

            var isLength = true;
            int length = 0;
            byte type = 0;
            List<byte> data = new List<byte>();
            var dataIndex = 0;

            for (int index = 0; index < scanRecord.Length; index++)
            {
                if (isLength)
                {
                    length = scanRecord[index];
                    if (length == 0)
                    {
                        return advertismentData;
                    }

                    isLength = false;
                    data.Clear();
                    dataIndex = 0;
                }
                else
                {
                    if (dataIndex == 0)
                    {
                        type = scanRecord[index];
                    }
                    else
                    {
                        data.Add(scanRecord[index]);
                    }

                    dataIndex++;
                    if (dataIndex == length)
                    {
                        advertismentData[type] = data.ToArray();
                        isLength = true;
                    }
                }
            }

            return advertismentData;
        }
    }
}