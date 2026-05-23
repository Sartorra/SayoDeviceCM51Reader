using SayoDeviceReader;

namespace SayoMonitor
{
    class Program
    {

        static void Main(string[] args)
        {
            SayoDevice deviceHandle = new SayoDevice();

            Console.WriteLine("SayoDevice obtained successfully!");
            Console.CursorVisible = false;

            while (true)
            {
                string fullMenu = "";
                Console.SetCursorPosition(0, 1);
                fullMenu += $"CPU Usage {deviceHandle.cpuUsage:D2}%\n";
                for (int i = 0; i < 10; i++)
                {
                    fullMenu += $"{GetAnalogKeyString(i + 1, deviceHandle.analogKeyStates[i], 20)}\n";
                }
                Console.WriteLine(fullMenu);
            }
        }

        private static string GetAnalogKeyString(int keyNumber, int keyValue, int width)
        {
            double percentage = Math.Clamp((double)keyValue / 79.0, 0, 1);
            int filledWidth = (int)(percentage * width);
            string outputString = $"Key {keyNumber:D2} [{new String('█', filledWidth)}{new String(' ', width - filledWidth)}] {percentage * 100,3:F0}%";
            return outputString;
        }
    }
}