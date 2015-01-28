using System;
using System.Drawing;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace Unban.exe
{
    internal class Program
    {
        public static Texture Taco;
        public static Sprite Sprite;
        public static Device DxDevice = Drawing.Direct3DDevice;
        public static int I;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("Unban.exe By DZ191 Loaded. Credits to DETUKS");
            Sprite = new Sprite(DxDevice);
            Taco = Texture.FromMemory(
                Drawing.Direct3DDevice,
                (byte[])
                    new ImageConverter().ConvertTo(LoadPicture("http://puu.sh/cP1qD/d23cd24220.jpg"), typeof(byte[])),
                513, 744, 0, Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);

            Drawing.OnEndScene += Drawing_OnEndScene;
            Drawing.OnPreReset += DrawingOnOnPreReset;
            Drawing.OnPostReset += DrawingOnOnPostReset;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnDomainUnload;
        }

        private static void CurrentDomainOnDomainUnload(object sender, EventArgs e)
        {
            Sprite.Dispose();
        }

        private static void DrawingOnOnPostReset(EventArgs args)
        {
            Sprite.OnResetDevice();
        }

        private static void DrawingOnOnPreReset(EventArgs args)
        {
            Sprite.OnLostDevice();
        }

        private static Bitmap LoadPicture(string url)
        {
            var request = WebRequest.Create(url);
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();

            var bitmap2 = new Bitmap(responseStream);
            Console.WriteLine(bitmap2.Size);

            return (bitmap2);
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            DrawTaco();
        }

        private static void DrawTaco()
        {
            if (I == 360)
            {
                I = 0;
            }

            var angle = I * (Math.PI / 180);
            var sin = (float) Math.Sin(angle);
            var cos = (float) Math.Cos(angle);
            Sprite.Begin();
            Sprite.Draw(Taco, new ColorBGRA(255, 255, 255, 255), null, new Vector3(-450 + 200 * cos, 0 + 200 * sin, 0));
            Sprite.End();
            I += 5;
            // Utility.DelayAction.Add(15, () => DrawTaco());
        }
    }
}