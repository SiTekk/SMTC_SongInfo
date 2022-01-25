using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Windows.Media.Control;
using Windows.Storage.Streams;
using Windows.Foundation;

namespace SMTC_SongInfoNew
{
    internal static class Program
    {
        private static string directory;
        private static TypedEventHandler<GlobalSystemMediaTransportControlsSessionManager, SessionsChangedEventArgs> handler = async (e, s) => { await GetsFired(); };
        private static GlobalSystemMediaTransportControlsSessionManager mediaControl;
        private static GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties;
        //private static string previousTitle;

        private static async Task GetsFired()
        {
            mediaControl.SessionsChanged -= handler;
            while (true)
            {
                try
                {
                    Console.WriteLine("Try Again");
                    Task task = Task.Run(() => SMTC_SongInfoAsync());
                    await Task.WhenAll(task);
                    Console.WriteLine("\nFinished");
                }
                catch (Exception ex)
                {
                    //Console.Write(new string(' ', Console.WindowWidth - 2) + "\r"); //To clear the line
                    Console.WriteLine(ex.Message + "\r");
                    File.WriteAllText(directory, "");
                    Thread.Sleep(1000);
                    continue;
                }
                Console.WriteLine("Add Handler");
                mediaControl.SessionsChanged += handler;
                break;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileStream"></param>
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static async Task Main(string[] args) //https://stackoverflow.com/questions/57580053/using-systemmediatransportcontrols-to-get-current-playing-song-info-from-other-a
        {
            mediaControl = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            //mediaControl.SessionsChanged += handler;
            //mediaControl.CurrentSessionChanged += handler;
            var session = mediaControl.GetCurrentSession();
            session.MediaPropertiesChanged += (s, e) => { Console.WriteLine("MediaPropertiesChaged"); };
            mediaControl.CurrentSessionChanged += (s, e) => { Console.WriteLine("Current Session"); };
            mediaControl.SessionsChanged += (s, e) => { Console.WriteLine("Sessions General"); };

            mediaControl = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            var test = await mediaControl.GetCurrentSession().TryGetMediaPropertiesAsync();

            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnConsoleCancleEvent);
            directory = Path.Combine(Directory.GetCurrentDirectory(), "SMTC_SongInfo.txt");
            Console.WriteLine("q terminates the Program.");
            await GetsFired();
            do
            {
                ;
            }
            while (Console.ReadKey().Key != ConsoleKey.Q);
            File.WriteAllText(directory, "");
        }

        /// <summary>
        /// Cleares the File when the Program gets terminated with Ctrl + C
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnConsoleCancleEvent(object sender, ConsoleCancelEventArgs args)
        {
            File.WriteAllText(directory, "");
        }

        /// <summary>
        /// Gets the current SMTC Manager and reads the Artist and the Title from it and writes it into a file. If there is no SMTC it throws a NullReferenceException.
        /// </summary>
        /// <returns></returns>
        private static async Task SMTC_SongInfoAsync()
        {
            mediaControl = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            mediaProperties = await mediaControl.GetCurrentSession().TryGetMediaPropertiesAsync();

            File.WriteAllText(directory, mediaProperties.Artist.Replace(" - Topic", "") + " - " + mediaProperties.Title + " | "); //Replace " - Topic" is needed for YT-Music becuase it appends it to the Artist (IDK why).

            //Console.Write(new string(' ', Console.WindowWidth - 2) + "\r"); //To clear the line
            Console.WriteLine(mediaProperties.Artist.Replace(" - Topic", "") + " - " + mediaProperties.Title + " | \r");

            var fileStream = await mediaProperties.Thumbnail.OpenReadAsync();
            GetThumbnail(fileStream);
        }
    }
}