using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlayPauseFixer
{
    internal class Program
    {
        private static void Main()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, args) =>
            {
                Console.WriteLine("Cancelling...");
                cancellationTokenSource.Cancel();
                args.Cancel = true;
            };

            try
            {
                WorkAsync(cancellationTokenSource.Token).GetAwaiter().GetResult();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Cancelled!");
            }
        }
        
        public static async Task WorkAsync(CancellationToken cancellationToken)
        {
            Console.Write("Searching for Device...");
            while (true)
            {
                var devices = HIDBrowse.EnumerateDevices();

                //Search for the Sony MDR-1000X device
                var headsetDevice = devices.FirstOrDefault(dev => dev.Vid == 0x045E && dev.Pid == 0x0627);

                if (headsetDevice == null || !headsetDevice.TryOpen(out var deviceStream))
                {
                    Console.Write(".");
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    continue;
                }

                Console.WriteLine();
                Console.WriteLine("Connected!");
                try
                {
                    //The MDR-1000X always sends a 5 byte report
                    //The first byte is always 0x01 and only the second byte contains usefull information
                    var report = new byte[5];
                    
                    while (deviceStream.CanRead)
                    {
                        await deviceStream.ReadAsync(report, 0, report.Length, cancellationToken);

                        Console.WriteLine(BitConverter.ToString(report));

                        // Check which action the user performed.
                        switch (report[1])
                        {
                            case 0xB0: //Both 0xB0 and 0xB1 seem to be sent on the pause gesture, not sure what the difference is
                            case 0xB1:
                                Native.KeyPress(Native.VK_MEDIA_PLAY_PAUSE);
                                break;
                        }
                    }
                }
                catch (Exception e) when (!(e is TaskCanceledException))
                {
                    //For now we just ignore any exceptions
                }
                finally
                {
                    deviceStream.Dispose();
                }
                Console.WriteLine("Disconnected!");

                Console.Write("Searching for Device...");
            }
        }
    }
}
