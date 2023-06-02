using EventHook;
using Microsoft.Toolkit.Uwp.Notifications;
using NAudio.Wave;
using System.Threading;
using System.Windows.Forms;

namespace NeedyStreamerOverload4
{
    class Program
    {
        //Object variables
        public static MouseWatcher mouseWatcher;
        public static int lastx;
        public static int lasty;
        public static bool holding = false;
        public static bool sfxplayed = false;
        private static AudioFileReader dragSFX;
        private static WaveOutEvent playSFX;
        private static NotifyIcon tray;

        //Main Program
        static void Main(string[] args)
        {
            dragSFX = new AudioFileReader("mousedrag.wav");
            playSFX = new WaveOutEvent();
            playSFX.Init(dragSFX);
            //This is a ram eater apparently
            var eventFactoryWatcher = new EventHookFactory();
            using (eventFactoryWatcher)
            {
                mouseWatcher = eventFactoryWatcher.GetMouseWatcher();
                mouseWatcher.Start();
                mouseWatcher.OnMouseInput += MouseWatchingViaMouseInput;
            }
            //Send notification to indicate the program is actually running
            NotifyUser();

            //Send to icon tray
            tray = new NotifyIcon();
            tray.Visible = true;
            tray.Icon = NeedyStreamerOverload.Properties.Resources.Icon;
            tray.Text = "You wouldn't leave me now would you? ⌃ ◌ ⌃";
            tray.DoubleClick += (s, e) =>
            {
                tray.Visible = false;
                Application.Exit();
            };
            Application.Run();
        }

        //Get Mouse Input
        private static void MouseWatchingViaMouseInput(object sender, EventHook.MouseEventArgs e)
        {
            //Console.WriteLine(string.Format("Mouse event {0} at point {1},{2}", e.Message.ToString(), e.Point.x, e.Point.y));
            switch (e.Message)
            {
                case EventHook.Hooks.MouseMessages.WM_LBUTTONDOWN:
                    PlaySound("mouseclick.wav");
                    lastx = e.Point.x;
                    lasty = e.Point.y;
                    holding = true;
                    break;
                case EventHook.Hooks.MouseMessages.WM_LBUTTONUP:
                    lastx = e.Point.x;
                    lasty = e.Point.y;
                    holding = false;
                    //Make it more like the game (Restart dragging audio)
                    while (playSFX.PlaybackState == PlaybackState.Playing)
                    {
                        if (holding == false)
                            playSFX.Stop();
                        PlaySound("mouseclick.wav");
                    }
                    //So it doesn't break the dragging sound
                    sfxplayed = false;
                    dragSFX.Position = 0;
                    break;
            }

            if ((e.Message == EventHook.Hooks.MouseMessages.WM_MOUSEMOVE))
            {
                if ((lastx != e.Point.x) && (lasty != e.Point.y) && holding == true)
                {
                    PlayMouseDraggingSound();
                }
            }
        }

        //Audio Bullshit
        private static void PlaySound(string wave)
        {
            new Thread(() =>
            {
                using (var audioFile = new AudioFileReader(wave))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }).Start();
        }
        private static void PlayMouseDraggingSound()
        {
            if (sfxplayed == false)
            {
                playSFX.Play();
                sfxplayed = true;
            }
        }

        private static void NotifyUser()
        {
            //No image but fuck it
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 9813)
                .AddText("Message from JINE")
                .AddText("You can stop these sounds by closing me in the icon tray.")
                .Show();
            PlaySound("notification.wav");
        }
    }
}