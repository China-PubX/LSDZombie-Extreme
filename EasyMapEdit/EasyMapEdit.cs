using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using InfinityScript;

namespace EasyMapEdit
{
    public class EasyMapEdit : BaseScript
    {
        bool creating = false;
        float startx;
        float starty;
        float startz;
        float endx;
        float endy;
        float endz;

        public EasyMapEdit()
            : base()
        {
            PlayerConnected += new Action<Entity>(player =>
            {
                player.Call("notifyonplayercommand", "fly", "+frag");
                player.OnNotify("fly", (ent) =>
                {
                    if (player.GetField<string>("sessionstate") != "spectator")
                    {
                        player.Call("allowspectateteam", "freelook", true);
                        player.SetField("sessionstate", "spectator");
                        player.Call("setcontents", 0);
                    }
                    else
                    {
                        player.Call("allowspectateteam", "freelook", false);
                        player.SetField("sessionstate", "playing");
                        player.Call("setcontents", 100);
                    }
                });
            });
        }
        public override void OnSay(Entity player, string playerName, string text)
        {
            var mapname = Call<string>("getdvar", "mapname");

            string[] msg = text.Split(' ');


            if (msg[0] == "!restart")
            {
                Utilities.ExecuteCommand("map_restart");
            }

            if (msg[0] == "!wall" && !creating)
            {
                player.Call("iprintlnbold", "^2START SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                creating = true;
            }
            else if (msg[0] == "!wall" && creating)
            {
                player.Call("iprintlnbold", "^2END SET: " + player.Origin);
                endx = player.Origin.X;
                endy = player.Origin.Y;
                endz = player.Origin.Z;
                creating = false;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "wall: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!ramp" && !creating)
            {
                player.Call("iprintlnbold", "^2START SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                creating = true;
            }
            else if (msg[0] == "!ramp" && creating)
            {
                player.Call("iprintlnbold", "^2END SET: " + player.Origin);
                endx = player.Origin.X;
                endy = player.Origin.Y;
                endz = player.Origin.Z;
                creating = false;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "ramp: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!tp" && !creating)
            {
                player.Call("iprintlnbold", "^2START SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                creating = true;
            }
            else if (msg[0] == "!tp" && creating)
            {
                player.Call("iprintlnbold", "^2END SET: " + player.Origin);
                endx = player.Origin.X;
                endy = player.Origin.Y;
                endz = player.Origin.Z;
                creating = false;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "elevator: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!htp" && !creating)
            {
                player.Call("iprintlnbold", "^2START SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                creating = true;
            }
            else if (msg[0] == "!htp" && creating)
            {
                player.Call("iprintlnbold", "^2END SET: " + player.Origin);
                endx = player.Origin.X;
                endy = player.Origin.Y;
                endz = player.Origin.Z;
                creating = false;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "HiddenTP: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!floor" && !creating)
            {
                player.Call("iprintlnbold", "^2START SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                creating = true;
            }
            else if (msg[0] == "!floor" && creating)
            {
                player.Call("iprintlnbold", "^2END SET: " + player.Origin);
                endx = player.Origin.X;
                endy = player.Origin.Y;
                endz = player.Origin.Z;
                creating = false;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "floor: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!turret" && !creating)
            {
                Vector3 angles = new Vector3();
                angles.X = 0;
                angles.Y = player.GetField<Vector3>("angles").Y;
                angles.Z = 0;
                player.Call("iprintlnbold", "^2TURRET SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                endx = angles.X;
                endy = angles.Y;
                endz = angles.Z;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "turret: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!randomweapon" && !creating)
            {
                Vector3 angles = new Vector3();
                angles.X = 0;
                angles.Y = player.GetField<Vector3>("angles").Y;
                angles.Z = 0;
                player.Call("iprintlnbold", "^2RANDOMWEAPON SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                endx = angles.X;
                endy = angles.Y;
                endz = angles.Z;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "randomWeapon: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!upgrade" && !creating)
            {
                Vector3 angles = new Vector3();
                angles.X = 0;
                angles.Y = player.GetField<Vector3>("angles").Y;
                angles.Z = 0;
                player.Call("iprintlnbold", "^2UPGRADE SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                endx = angles.X;
                endy = angles.Y;
                endz = angles.Z;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "upgrade: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!ammo" && !creating)
            {
                Vector3 angles = new Vector3();
                angles.X = 0;
                angles.Y = player.GetField<Vector3>("angles").Y;
                angles.Z = 0;
                player.Call("iprintlnbold", "^2AMMOBOX SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                endx = angles.X;
                endy = angles.Y;
                endz = angles.Z;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "ammo: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!gambler" && !creating)
            {
                Vector3 angles = new Vector3();
                angles.X = 0;
                angles.Y = player.GetField<Vector3>("angles").Y;
                angles.Z = 0;
                player.Call("iprintlnbold", "^2GAMBLER SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                endx = angles.X;
                endy = angles.Y;
                endz = angles.Z;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "gambler: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!speedcola" && !creating)
            {
                Vector3 angles = new Vector3();
                angles.X = 0;
                angles.Y = player.GetField<Vector3>("angles").Y;
                angles.Z = 0;
                player.Call("iprintlnbold", "^2SPEEDCOLA SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                endx = angles.X;
                endy = angles.Y;
                endz = angles.Z;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "speedCola: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!jugger" && !creating)
            {
                Vector3 angles = new Vector3();
                angles.X = 0;
                angles.Y = player.GetField<Vector3>("angles").Y;
                angles.Z = 0;
                player.Call("iprintlnbold", "^2JUGGER SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                endx = angles.X;
                endy = angles.Y;
                endz = angles.Z;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "jugger: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!speedy" && !creating)
            {
                Vector3 angles = new Vector3();
                angles.X = 0;
                angles.Y = player.GetField<Vector3>("angles").Y;
                angles.Z = 0;
                player.Call("iprintlnbold", "^2SPEEDY SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                endx = angles.X;
                endy = angles.Y;
                endz = angles.Z;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "speedy: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!stalker" && !creating)
            {
                Vector3 angles = new Vector3();
                angles.X = 0;
                angles.Y = player.GetField<Vector3>("angles").Y;
                angles.Z = 0;
                player.Call("iprintlnbold", "^2STALKER SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                endx = angles.X;
                endy = angles.Y;
                endz = angles.Z;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "stalker: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!mulekick" && !creating)
            {
                Vector3 angles = new Vector3();
                angles.X = 0;
                angles.Y = player.GetField<Vector3>("angles").Y;
                angles.Z = 0;
                player.Call("iprintlnbold", "^2MULEKICK SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                endx = angles.X;
                endy = angles.Y;
                endz = angles.Z;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "muleKick: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!doubletap" && !creating)
            {
                Vector3 angles = new Vector3();
                angles.X = 0;
                angles.Y = player.GetField<Vector3>("angles").Y;
                angles.Z = 0;
                player.Call("iprintlnbold", "^2DOUBLETAP SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                endx = angles.X;
                endy = angles.Y;
                endz = angles.Z;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "doubleTap: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }
        }
    }
}
