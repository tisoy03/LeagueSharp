using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace NidaPouncer
{
    //Big thanks to blm95
    internal class Program
    {
        public static Dictionary<Vector3, Vector3> Positions;
        public static String ChampName = "Nidalee";
        public static Obj_AI_Base Player;
        public static Menu Menu;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell W;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampName)
            {
                return;
            }

            FillPositions();

            W = new Spell(SpellSlot.W, 375f);
            SpellList.Add(W);

            Menu = new Menu("Nida Pouncer", "NidaPMenu", true);

            Menu.AddSubMenu(new Menu("Pouncer Drawing", "Drawing"));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("PouncerDr", "Draw Pounce Spots").SetValue(true));

            Menu.AddSubMenu(new Menu("Pouncer Flee", "FleeM"));
            Menu.SubMenu("FleeM")
                .AddItem(new MenuItem("FleeKey", "Flee").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Menu.AddToMainMenu();

            Game.PrintChat("Nida Pouncer By DZ191 Loaded");
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            if (!Menu.Item("PouncerDr").GetValue<bool>())
            {
                return;
            }

            foreach (
                var entry in
                    Positions.Where(
                        entry => Player.Distance(entry.Key) <= 1500f && Player.Distance(entry.Value) <= 1500f))
            {
                Drawing.DrawCircle(entry.Key, 75f, Color.GreenYellow);
                Drawing.DrawCircle(entry.Value, 75f, Color.GreenYellow);
            }
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Menu.Item("FleeKey").GetValue<KeyBind>().Active)
            {
                return;
            }

            foreach (var entry in Positions)
            {
                if (!(Player.Distance(entry.Key) <= 220f) && !(Player.Distance(entry.Value) <= 220f))
                {
                    continue;
                }

                var closest = entry.Key;
                var farther = entry.Value;
                if (Player.Distance(entry.Key) < Player.Distance(entry.Value))
                {
                    closest = entry.Key;
                    farther = entry.Value;
                }

                if (Player.Distance(entry.Key) > Player.Distance(entry.Value))
                {
                    closest = entry.Value;
                    farther = entry.Key;
                }

                Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(closest.X, closest.Y)).Send();
                W.Cast(farther, true);
            }
        }

        public static void FillPositions()
        {
            Positions = new Dictionary<Vector3, Vector3>();
            var pos0 = new Vector3(6393.7299804688f, 8341.7451171875f, -63.87451171875f);
            var pos1 = new Vector3(6612.1625976563f, 8574.7412109375f, 56.018413543701f);
            Positions.Add(pos0, pos1);
            var pos2 = new Vector3(7041.7885742188f, 8810.1787109375f, 0f);
            var pos3 = new Vector3(7296.0341796875f, 9056.4638671875f, 55.610824584961f);
            Positions.Add(pos2, pos3);
            var pos4 = new Vector3(4546.0258789063f, 2548.966796875f, 54.257415771484f);
            var pos5 = new Vector3(4185.0786132813f, 2526.5520019531f, 109.35539245605f);
            Positions.Add(pos4, pos5);
            var pos6 = new Vector3(2805.4074707031f, 6140.130859375f, 55.182941436768f);
            var pos7 = new Vector3(2614.3215332031f, 5816.9438476563f, 60.193073272705f);
            Positions.Add(pos6, pos7);
            var pos8 = new Vector3(6696.486328125f, 5377.4013671875f, 61.310482025146f);
            var pos9 = new Vector3(6868.6918945313f, 5698.1455078125f, 55.616455078125f);
            Positions.Add(pos8, pos9);
            var pos10 = new Vector3(1677.9854736328f, 8319.9345703125f, 54.923847198486f);
            var pos11 = new Vector3(1270.2786865234f, 8286.544921875f, 50.334892272949f);
            Positions.Add(pos10, pos11);
            var pos12 = new Vector3(2809.3254394531f, 10178.6328125f, -58.759708404541f);
            var pos13 = new Vector3(2553.8962402344f, 9974.4677734375f, 53.364395141602f);
            Positions.Add(pos12, pos13);
            var pos14 = new Vector3(5102.642578125f, 10322.375976563f, -62.845260620117f);
            var pos15 = new Vector3(5483f, 10427f, 54.5009765625f);
            Positions.Add(pos14, pos15);
            var pos16 = new Vector3(6000.2373046875f, 11763.544921875f, 39.544124603271f);
            var pos17 = new Vector3(6056.666015625f, 11388.752929688f, 54.385917663574f);
            Positions.Add(pos16, pos17);
            var pos18 = new Vector3(1742.34375f, 7647.1557617188f, 53.561042785645f);
            var pos19 = new Vector3(1884.5321044922f, 7995.1459960938f, 54.930736541748f);
            Positions.Add(pos18, pos19);
            var pos20 = new Vector3(3319.087890625f, 7472.4760742188f, 55.027889251709f);
            var pos21 = new Vector3(3388.0522460938f, 7101.2568359375f, 54.486026763916f);
            Positions.Add(pos20, pos21);
            var pos22 = new Vector3(3989.9423828125f, 7929.3422851563f, 51.94282913208f);
            var pos23 = new Vector3(3671.623046875f, 7723.146484375f, 53.906265258789f);
            Positions.Add(pos22, pos23);
            var pos24 = new Vector3(4936.8452148438f, 10547.737304688f, -63.064865112305f);
            var pos25 = new Vector3(5156.7397460938f, 10853.216796875f, 52.951190948486f);
            Positions.Add(pos24, pos25);
            var pos26 = new Vector3(5028.1235351563f, 10115.602539063f, -63.082695007324f);
            var pos27 = new Vector3(5423f, 10127f, 55.15357208252f);
            Positions.Add(pos26, pos27);
            var pos28 = new Vector3(6035.4819335938f, 10973.666015625f, 53.918266296387f);
            var pos29 = new Vector3(6385.4013671875f, 10827.455078125f, 54.63500213623f);
            Positions.Add(pos28, pos29);
            var pos30 = new Vector3(4747.0625f, 11866.421875f, 41.584358215332f);
            var pos31 = new Vector3(4743.23046875f, 11505.842773438f, 51.196254730225f);
            Positions.Add(pos30, pos31);
            var pos32 = new Vector3(6749.4487304688f, 12980.83984375f, 44.903495788574f);
            var pos33 = new Vector3(6701.4965820313f, 12610.278320313f, 52.563804626465f);
            Positions.Add(pos32, pos33);
            var pos34 = new Vector3(3114.1865234375f, 9420.5078125f, -42.718975067139f);
            var pos35 = new Vector3(2757f, 9255f, 53.77322769165f);
            Positions.Add(pos34, pos35);
            var pos36 = new Vector3(2786.8354492188f, 9547.8935546875f, 53.645294189453f);
            var pos37 = new Vector3(3002.0930175781f, 9854.39453125f, -53.198081970215f);
            Positions.Add(pos36, pos37);
            var pos38 = new Vector3(3803.9470214844f, 7197.9018554688f, 53.730079650879f);
            var pos39 = new Vector3(3664.1088867188f, 7543.572265625f, 54.18229675293f);
            Positions.Add(pos38, pos39);
            var pos40 = new Vector3(2340.0886230469f, 6387.072265625f, 60.165466308594f);
            var pos41 = new Vector3(2695.6096191406f, 6374.0634765625f, 54.339839935303f);
            Positions.Add(pos40, pos41);
            var pos42 = new Vector3(3249.791015625f, 6446.986328125f, 55.605854034424f);
            var pos43 = new Vector3(3157.4558105469f, 6791.4458007813f, 54.080295562744f);
            Positions.Add(pos42, pos43);
            var pos44 = new Vector3(3823.6242675781f, 5923.9130859375f, 55.420352935791f);
            var pos45 = new Vector3(3584.2561035156f, 6215.4931640625f, 55.6123046875f);
            Positions.Add(pos44, pos45);
            var pos46 = new Vector3(5796.4809570313f, 5060.4116210938f, 51.673671722412f);
            var pos47 = new Vector3(5730.3081054688f, 5430.1635742188f, 54.921173095703f);
            Positions.Add(pos46, pos47);
            var pos48 = new Vector3(6007.3481445313f, 4985.3803710938f, 51.673641204834f);
            var pos49 = new Vector3(6388.783203125f, 4987f, 51.673400878906f);
            Positions.Add(pos48, pos49);
            var pos50 = new Vector3(7040.9892578125f, 3964.6728515625f, 57.192108154297f);
            var pos51 = new Vector3(6668.0073242188f, 3993.609375f, 51.671356201172f);
            Positions.Add(pos50, pos51);
            var pos52 = new Vector3(7763.541015625f, 3294.3481445313f, 54.872283935547f);
            var pos53 = new Vector3(7629.421875f, 3648.0581054688f, 56.908012390137f);
            Positions.Add(pos52, pos53);
            var pos54 = new Vector3(4705.830078125f, 9440.6572265625f, -62.586814880371f);
            var pos55 = new Vector3(4779.9809570313f, 9809.9091796875f, -63.09009552002f);
            Positions.Add(pos54, pos55);
            var pos56 = new Vector3(4056.7907714844f, 10216.12109375f, -63.152275085449f);
            var pos57 = new Vector3(3680.1550292969f, 10182.296875f, -63.701038360596f);
            Positions.Add(pos56, pos57);
            var pos58 = new Vector3(4470.0883789063f, 12000.479492188f, 41.59789276123f);
            var pos59 = new Vector3(4232.9799804688f, 11706.015625f, 49.295585632324f);
            Positions.Add(pos58, pos59);
            var pos60 = new Vector3(5415.5708007813f, 12640.216796875f, 40.682685852051f);
            var pos61 = new Vector3(5564.4409179688f, 12985.860351563f, 41.373748779297f);
            Positions.Add(pos60, pos61);
            var pos62 = new Vector3(6053.779296875f, 12567.381835938f, 40.587882995605f);
            var pos63 = new Vector3(6045.4555664063f, 12942.313476563f, 41.211364746094f);
            Positions.Add(pos62, pos63);
            var pos64 = new Vector3(4454.66015625f, 8057.1313476563f, 42.799690246582f);
            var pos65 = new Vector3(4577.8681640625f, 7699.3686523438f, 53.31339263916f);
            Positions.Add(pos64, pos65);
            var pos66 = new Vector3(7754.7700195313f, 10449.736328125f, 52.890430450439f);
            var pos67 = new Vector3(8096.2885742188f, 10288.80078125f, 53.66955947876f);
            Positions.Add(pos66, pos67);
            var pos68 = new Vector3(7625.3139648438f, 9465.7001953125f, 55.008113861084f);
            var pos69 = new Vector3(7995.986328125f, 9398.1982421875f, 53.530490875244f);
            Positions.Add(pos68, pos69);
            var pos70 = new Vector3(9767f, 8839f, 53.044532775879f);
            var pos71 = new Vector3(9653.1220703125f, 9174.7626953125f, 53.697280883789f);
            Positions.Add(pos70, pos71);
            var pos72 = new Vector3(10775.653320313f, 7612.6943359375f, 55.35241317749f);
            var pos73 = new Vector3(10665.490234375f, 7956.310546875f, 65.222145080566f);
            Positions.Add(pos72, pos73);
            var pos74 = new Vector3(10398.484375f, 8257.8642578125f, 66.200691223145f);
            var pos75 = new Vector3(10176.104492188f, 8544.984375f, 64.849853515625f);
            Positions.Add(pos74, pos75);
            var pos76 = new Vector3(11198.071289063f, 8440.4638671875f, 67.641044616699f);
            var pos77 = new Vector3(11531.436523438f, 8611.0087890625f, 53.454048156738f);
            Positions.Add(pos76, pos77);
            var pos78 = new Vector3(11686.700195313f, 8055.9624023438f, 55.458232879639f);
            var pos79 = new Vector3(11314.19140625f, 8005.4946289063f, 58.438243865967f);
            Positions.Add(pos78, pos79);
            var pos80 = new Vector3(10707.119140625f, 7335.1752929688f, 55.350387573242f);
            var pos81 = new Vector3(10693f, 6943f, 54.870254516602f);
            Positions.Add(pos80, pos81);
            var pos82 = new Vector3(10395.380859375f, 6938.5009765625f, 54.869094848633f);
            var pos83 = new Vector3(10454.955078125f, 7316.7041015625f, 55.308219909668f);
            Positions.Add(pos82, pos83);
            var pos84 = new Vector3(10358.5859375f, 6677.1704101563f, 54.86909866333f);
            var pos85 = new Vector3(10070.067382813f, 6434.0815429688f, 55.294486999512f);
            Positions.Add(pos84, pos85);
            var pos86 = new Vector3(11161.98828125f, 5070.447265625f, 53.730766296387f);
            var pos87 = new Vector3(10783f, 4965f, -63.57177734375f);
            Positions.Add(pos86, pos87);
            var pos88 = new Vector3(11167.081054688f, 4613.9829101563f, -62.898971557617f);
            var pos89 = new Vector3(11501f, 4823f, 54.571090698242f);
            Positions.Add(pos88, pos89);
            var pos90 = new Vector3(11743.823242188f, 4387.4672851563f, 52.005855560303f);
            var pos91 = new Vector3(11379f, 4239f, -61.565242767334f);
            Positions.Add(pos90, pos91);
            var pos92 = new Vector3(10388.120117188f, 4267.1796875f, -63.61775970459f);
            var pos93 = new Vector3(10033.036132813f, 4147.1669921875f, -60.332069396973f);
            Positions.Add(pos92, pos93);
            var pos94 = new Vector3(8964.7607421875f, 4214.3833007813f, -63.284225463867f);
            var pos95 = new Vector3(8569f, 4241f, 55.544258117676f);
            Positions.Add(pos94, pos95);
            var pos96 = new Vector3(5554.8657226563f, 4346.75390625f, 51.680099487305f);
            var pos97 = new Vector3(5414.0634765625f, 4695.6860351563f, 51.611679077148f);
            Positions.Add(pos96, pos97);
            var pos98 = new Vector3(7311.3393554688f, 10553.6015625f, 54.153884887695f);
            var pos99 = new Vector3(6938.5209960938f, 10535.8515625f, 54.441242218018f);
            Positions.Add(pos98, pos99);
            var pos100 = new Vector3(7669.353515625f, 5960.5717773438f, -64.488967895508f);
            var pos101 = new Vector3(7441.2182617188f, 5761.8989257813f, 54.347793579102f);
            Positions.Add(pos100, pos101);
            var pos102 = new Vector3(7949.65625f, 2647.0490722656f, 54.276401519775f);
            var pos103 = new Vector3(7863.0063476563f, 3013.7814941406f, 55.178623199463f);
            Positions.Add(pos102, pos103);
            var pos104 = new Vector3(8698.263671875f, 3783.1169433594f, 57.178703308105f);
            var pos105 = new Vector3(9041f, 3975f, -63.242683410645f);
            Positions.Add(pos104, pos105);
            var pos106 = new Vector3(9063f, 3401f, 68.192077636719f);
            var pos107 = new Vector3(9275.0751953125f, 3712.8935546875f, -63.257461547852f);
            Positions.Add(pos106, pos107);
            var pos108 = new Vector3(12064.340820313f, 6424.11328125f, 54.830627441406f);
            var pos109 = new Vector3(12267.9375f, 6742.9453125f, 54.83561706543f);
            Positions.Add(pos108, pos109);
            var pos110 = new Vector3(12797.838867188f, 5814.9653320313f, 58.281986236572f);
            var pos111 = new Vector3(12422.740234375f, 5860.931640625f, 54.815074920654f);
            Positions.Add(pos110, pos111);
            var pos112 = new Vector3(11913.165039063f, 5373.34375f, 54.050819396973f);
            var pos113 = new Vector3(11569.1953125f, 5211.7143554688f, 57.787326812744f);
            Positions.Add(pos112, pos113);
            var pos114 = new Vector3(9237.3603515625f, 2522.8937988281f, 67.796775817871f);
            var pos115 = new Vector3(9344.2041015625f, 2884.958984375f, 65.500213623047f);
            Positions.Add(pos114, pos115);
            var pos116 = new Vector3(7324.2783203125f, 1461.2199707031f, 52.594970703125f);
            var pos117 = new Vector3(7357.3852539063f, 1837.4309082031f, 54.282878875732f);
            Positions.Add(pos116, pos117);
        }
    }
}