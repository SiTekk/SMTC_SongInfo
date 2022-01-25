using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace SMTC_SongInfo
{
    internal static class Program
    {
        private static GlobalSystemMediaTransportControlsSessionManager mediaControl;
        private static GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties;
        private static string previousTitle;
        private static string songInfoFilePath;

        /// <summary>
        /// Gets the thumbnail of the SMT Control and safes it relative to the .exe
        /// </summary>
        private static void GetThumbnail(IRandomAccessStreamWithContentType fileStream)
        {
            using (var reader = new BinaryReader(fileStream.AsStream()))
            {
                try
                {
                    File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "Thumbnail.jpg"), reader.ReadBytes((int)fileStream.Size));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static async Task Main(string[] args) //https://stackoverflow.com/questions/57580053/using-systemmediatransportcontrols-to-get-current-playing-song-info-from-other-a
        {
            mediaControl = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnConsoleCancleEvent);
            songInfoFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SMTC_SongInfo.txt");

            Console.WriteLine("Ctrl + C terminates the Program.");

            while (true)
            {
                try
                {
                    Task task = Task.Run(() => SMTC_SongInfoAsync());
                    await Task.WhenAll(task);
                }
                catch (Exception ex)
                {
                    Console.Write(new string(' ', Console.WindowWidth - 2) + "\r"); //To clear the line
                    Console.Write(ex.Message + "\r");
                    File.WriteAllText(songInfoFilePath, "");
                    //File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "Thumbnail.jpg"));
                }
                Thread.Sleep(2000);
                //GC.Collect();
            }
        }

        /// <summary>
        /// Cleares the Files when the Program gets terminated with Ctrl + C
        /// </summary>
        private static void OnConsoleCancleEvent(object sender, ConsoleCancelEventArgs args)
        {
            File.WriteAllText(songInfoFilePath, "");
            File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "Thumbnail.jpg"));
        }

        /// <summary>
        /// Gets the current SMTC Manager and reads the Artist and the Title from it and writes it into a file. If there is no SMTC it throws a NullReferenceException.
        /// </summary>
        private static async Task SMTC_SongInfoAsync()
        {
            //mediaControl = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync
            mediaProperties = await mediaControl.GetCurrentSession().TryGetMediaPropertiesAsync();

            File.WriteAllText(songInfoFilePath, mediaProperties.Artist.Replace(" - Topic", "") + " - " + mediaProperties.Title + " | "); //Replace " - Topic" is needed for YT-Music becuase it appends it to the Artist (IDK why).

            Console.Write(new string(' ', Console.WindowWidth - 2) + "\r"); //To clear the line
            Console.Write(mediaProperties.Artist.Replace(" - Topic", "") + " - " + mediaProperties.Title + " | \r");

            //Gets the new Thumbnail when the Titel changes
            if (previousTitle != mediaProperties.Title)
            {
                previousTitle = mediaProperties.Title;
                var fileStream = await mediaProperties.Thumbnail.OpenReadAsync();
                GetThumbnail(fileStream);
            }
        }
    }
}