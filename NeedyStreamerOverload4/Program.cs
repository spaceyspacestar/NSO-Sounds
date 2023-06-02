using System;
using System.Threading;
using EventHook;
using NAudio.Wave;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.Uwp.Notifications;

namespace NeedyStreamerOverload4
{
    class Program
    {
        //Object variables!!
        public static MouseWatcher mouseWatcher;
        public static int lastx;
        public static int lasty;
        public static bool holding = false;
        public static bool sfxplayed = false;
        private static AudioFileReader dragSFX;
        private static WaveOutEvent playSFX;
        //Hide Console
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void Main(string[] args)
        {
            dragSFX = new AudioFileReader("mousedrag.wav");
            playSFX = new WaveOutEvent();
            playSFX.Init(dragSFX);
            var handle = GetConsoleWindow();
            //This is a ram eater apparently
            var eventFactoryWatcher = new EventHookFactory();
            using (eventFactoryWatcher)
            {
                mouseWatcher = eventFactoryWatcher.GetMouseWatcher();
                mouseWatcher.Start();
                mouseWatcher.OnMouseInput += MouseWatchingViaMouseInput;
            }
            ShowWindow(handle, SW_SHOW);
            PlaySound("mouseclick.wav");
            new ToastContentBuilder()
            .AddArgument("action", "viewConversation")
            .AddArgument("conversationId", 9813)
            .AddText("Hello World")
            .AddText("Close me in the icon tray")
            //.AddAppLogoOverride(new Uri(""), ToastGenericAppLogoCrop.Circle)
            .Show();
            Console.Read();
        }

        //Get Mouse Input
        private static void MouseWatchingViaMouseInput(object sender, MouseEventArgs e)
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
                        {
                            playSFX.Stop();
                            dragSFX.Position = 0;
                            sfxplayed = false;
                            PlaySound("mouseclick.wav");
                        }
                    }
                    break;
            }

            if ((e.Message == EventHook.Hooks.MouseMessages.WM_MOUSEMOVE))
            {
                if ((lastx != e.Point.x) && (lasty != e.Point.y) && holding == true)
                {
                    PlayMouseDraggingSound("mousedrag.wav");
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

        private static void PlayMouseDraggingSound(string wave)
        {
            if (sfxplayed == false)
            {
                playSFX.Play();
                sfxplayed = true;
            }
        }
    }
}
