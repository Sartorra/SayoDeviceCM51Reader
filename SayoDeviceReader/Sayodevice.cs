using HidSharp;
using HidSharp.Reports;
using System.Diagnostics;

namespace SayoDeviceReader
{
    internal class SayoDevice
    {
        // Const/Static values
        private const int vendorID = 0x8089;
        private const int pID = 0x0009;
        private const uint usagePage = 0xFF110002;
        private const byte generalReport = 0x00;
        private const byte analogReport = 0x12;
        private readonly long requestIntervalMs = 2;

        // Patterns
        private static readonly byte[] cpuUsagePattern = [0x0B, 0x00, 0xFF, 0x00, 0xC0]; // Offset of 4.

        // Packets
        private static readonly byte[] getAnalogPacket = [0x21, 0x12, 0x3B, 0x12, 0x05, 0x00, 0x15, .. new byte[57]];

        // Offsets
        private const int cpuUsageOffset = 14;
        private const int baseAnalogKeyOffset = 8;


        // Class properties
        private HidDevice deviceHandle;
        private HidStream streamHandle;
        private Stopwatch timeSinceLastRequest;
        private bool shouldBackgroundWorkerRun;
        public byte[] analogKeyStates;
        public byte cpuUsage;

        // Functions
        public SayoDevice()
        {
            // Grab the device.
            HidDevice? foundDevice = GetDeviceHandle();
            if (foundDevice == null) {
                throw new Exception("Unable to grab sayodevice.");
            }
            deviceHandle = foundDevice;

            // Open a handle to the device.
            streamHandle = foundDevice.Open();

            // Initialize variables.
            analogKeyStates = new byte[10];
            cpuUsage = 0;

            // Setup background worker
            timeSinceLastRequest = new Stopwatch();
            shouldBackgroundWorkerRun = true;
            Task.Run(BackgroundWorker);
        }

        ~SayoDevice()
        {
            // Turn off background worker.
            shouldBackgroundWorkerRun = false;

            // Close any stream handles.
            streamHandle.Close();
        }

        private HidDevice? GetDeviceHandle()
        {
            // Grab all devices
            IEnumerable<HidDevice> devices = DeviceList.Local.GetHidDevices(vendorID, pID);
            HidDevice? foundDevice = null;
            foreach (HidDevice device in devices)
            {
                // I am unsure if there is a better way to grab the usage page from devices in HIDSharp. This is the method I found to work.
                ReportDescriptor rawDescriptor = device.GetReportDescriptor();

                foreach (DeviceItem item in rawDescriptor.DeviceItems)
                {
                    IEnumerable<uint> pages = item.Usages.GetValuesFromIndex(0);
                    foreach (uint usage in pages)
                    {
                        if (usage == usagePage)
                        {
                            foundDevice = device;
                            break;
                        }
                    }
                }
            };

            // Return what was or wasn't found.
            return foundDevice;
        }


        private void SendPacket(byte[] packet)
        {
            streamHandle.Write(packet, 0, 64);
        }

        private void BackgroundWorker()
        {
            // Start the timer.
            timeSinceLastRequest.Start();

            while(shouldBackgroundWorkerRun)
            {
                // https://sayodevice.com/ will send a packet every so often to get analog inputs out of the device.
                if (timeSinceLastRequest.ElapsedMilliseconds >= requestIntervalMs)
                {
                    SendPacket(getAnalogPacket);
                    timeSinceLastRequest.Restart();
                }

                // Read data from the device.
                byte[] readBuffer = new byte[64];
                int replySize = streamHandle.Read(readBuffer, 0, 64);

                // Update any data according to any flags.
                byte protocolType = readBuffer[1];
                switch (protocolType)
                {
                    case generalReport:
                        if (PatternScan(readBuffer, 4, cpuUsagePattern))
                        {
                            GetHeartbeat(readBuffer);
                        }
                        break;
                    case analogReport:
                        GetAnalogInputs(readBuffer);
                        break;
                }
            }
        }

        // Scans for a pattern in a byte array. Then returns if it found the pattern.
        private bool PatternScan(byte[] dataBuffer, int offset, byte[] pattern)
        {
            // Check for the pattern.
            for(int index = 0; index < pattern.Length; index++)
            {
                if (dataBuffer[index + offset]  != pattern[index])
                {
                    // Something didn't match.
                    return false;
                }
            }

            // Everything matched.
            return true;
        }

        private void GetHeartbeat(byte[] dataBuffer) => cpuUsage = dataBuffer[cpuUsageOffset];

        private void GetAnalogInputs(byte[] dataBuffer)
        {
            for (int index = 0; index <=  9; index++)
            {
                analogKeyStates[index] = dataBuffer[index + baseAnalogKeyOffset];
            }
        }
    }
}
