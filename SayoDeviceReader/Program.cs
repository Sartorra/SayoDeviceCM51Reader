using SayoDeviceReader;

namespace SayoMonitor
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            SayoDevice deviceHandle = new SayoDevice();
            SayoCat catRenderer = new SayoCat(&deviceHandle);

            Console.WriteLine("SayoDevice obtained successfully!");

            // This use to be used to output it to console, since then SFML has been used to display a cat.
            while (true)
            {
                //string fullMenu = "";
                //Console.SetCursorPosition(0, 1);
                //fullMenu += $"CPU Usage {deviceHandle.cpuUsage:D2}%\n";
                //for (int i = 0; i < 10; i++)
                //{
                //    fullMenu += $"{GetAnalogKeyString(i + 1, deviceHandle.analogKeyStates[i], 20)}\n";
                //}
                //Console.WriteLine(fullMenu);
            }
        }

        //private static string GetAnalogKeyString(int keyNumber, int keyValue, int width)
        //{
        //    double percentage = Math.Clamp((double)keyValue / 79.0, 0, 1);
        //    int filledWidth = (int)(percentage * width);
        //    string outputString = $"Key {keyNumber:D2} [{new String('█', filledWidth)}{new String(' ', width - filledWidth)}] {percentage * 100,3:F0}%";
        //    return outputString;
        //}
    }
}