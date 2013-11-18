using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using InfinityScript;

namespace LSDZombie
{
    public class MapEdit : BaseScript
    {
        private Entity _airdropCollision;
        private Random _rng = new Random();
        private string _mapname;
        public MapEdit()
            : base()
        {
            Entity care_package = Call<Entity>("getent", "care_package", "targetname");
            _airdropCollision = Call<Entity>("getent", care_package.GetField<string>("target"), "targetname");
            _mapname = Call<string>("getdvar", "mapname");
            Call("precachemodel", getAlliesFlagModel(_mapname));
            Call("precachemodel", getAxisFlagModel(_mapname));
            Call("precachemodel", "prop_flag_neutral");
            Call("precacheshader", "waypoint_flag_friendly");
            Call("precacheshader", "compass_waypoint_target");
            Call("precacheshader", "compass_waypoint_bomb");
            Call("precachemodel", "weapon_scavenger_grenadebag");
            Call("precachemodel", "weapon_oma_pack");

            if (File.Exists("scripts\\maps\\" + _mapname + ".txt"))
                loadMapEdit(_mapname);

            PlayerConnected += new Action<Entity>(player =>
            {
                player.SetField("attackeddoor", 0); // debounce timer
                player.SetField("repairsleft", 0); // door repairs remaining

                // usable notifications
                player.Call("notifyonplayercommand", "triggeruse", "+activate");
                player.OnNotify("triggeruse", (ent) => HandleUseables(player));
                UsablesHud(player);

                HudElem perk1 = HudElem.CreateIcon(player, "", 30, 30);
                perk1.SetPoint("BOTTOM LEFT", "BOTTOM LEFT", 0, 0);
                perk1.HideWhenInMenu = true;
                perk1.Foreground = true;

                HudElem perk2 = HudElem.CreateIcon(player, "", 30, 30);
                perk2.SetPoint("BOTTOM LEFT", "BOTTOM LEFT", 32, 0);
                perk2.HideWhenInMenu = true;
                perk2.Foreground = true;

                HudElem perk3 = HudElem.CreateIcon(player, "", 30, 30);
                perk3.SetPoint("BOTTOM LEFT", "BOTTOM LEFT", 64, 0);
                perk3.HideWhenInMenu = true;
                perk3.Foreground = true;

                HudElem perk4 = HudElem.CreateIcon(player, "", 30, 30);
                perk4.SetPoint("BOTTOM LEFT", "BOTTOM LEFT", 96, 0);
                perk4.HideWhenInMenu = true;
                perk4.Foreground = true;

                HudElem perk5 = HudElem.CreateIcon(player, "", 30, 30);
                perk5.SetPoint("BOTTOM LEFT", "BOTTOM LEFT", 128, 0);
                perk5.HideWhenInMenu = true;
                perk5.Foreground = true;

                HudElem perk6 = HudElem.CreateIcon(player, "", 30, 30);
                perk6.SetPoint("BOTTOM LEFT", "BOTTOM LEFT", 160, 0);
                perk6.HideWhenInMenu = true;
                perk6.Foreground = true;

                player.SetField("perk1bought", "");
                player.SetField("perk2bought", "");
                player.SetField("perk3bought", "");
                player.SetField("perk4bought", "");
                player.SetField("perk5bought", "");
                player.SetField("perk6bought", "");

                player.SetField("speedcolaDone", 0);
                player.SetField("juggerDone", 0);
                player.SetField("speedyDone", 0);
                player.SetField("stalkerDone", 0);
                player.SetField("mulekickDone", 0);
                player.SetField("doubletapDone", 0);

                player.OnInterval(100, (ent) =>
                {
                    if (player.GetField<string>("perk1bought") == "")
                    {
                        perk1.Alpha = 0;
                    }
                    else if (player.GetField<string>("perk1bought") != "")
                    {
                        perk1.SetShader(player.GetField<string>("perk1bought"), 30, 30);
                        perk1.Alpha = 1;
                    }
                    if (player.GetField<string>("perk2bought") == "")
                    {
                        perk2.Alpha = 0;
                    }
                    else if (player.GetField<string>("perk2bought") != "")
                    {
                        perk2.SetShader(player.GetField<string>("perk2bought"), 30, 30);
                        perk2.Alpha = 1;
                    }
                    if (player.GetField<string>("perk3bought") == "")
                    {
                        perk3.Alpha = 0;
                    }
                    else if (player.GetField<string>("perk3bought") != "")
                    {
                        perk3.SetShader(player.GetField<string>("perk3bought"), 30, 30);
                        perk3.Alpha = 1;
                    }
                    if (player.GetField<string>("perk4bought") == "")
                    {
                        perk4.Alpha = 0;
                    }
                    else if (player.GetField<string>("perk4bought") != "")
                    {
                        perk4.SetShader(player.GetField<string>("perk4bought"), 30, 30);
                        perk4.Alpha = 1;
                    }
                    if (player.GetField<string>("perk5bought") == "")
                    {
                        perk5.Alpha = 0;
                    }
                    else if (player.GetField<string>("perk5bought") != "")
                    {
                        perk5.SetShader(player.GetField<string>("perk5bought"), 30, 30);
                        perk5.Alpha = 1;
                    }
                    if (player.GetField<string>("perk6bought") == "")
                    {
                        perk6.Alpha = 0;
                    }
                    else if (player.GetField<string>("perk6bought") != "")
                    {
                        perk6.SetShader(player.GetField<string>("perk6bought"), 30, 30);
                        perk6.Alpha = 1;
                    }

                    return true;
                });
            });
        }

        public void CreateRamp(Vector3 top, Vector3 bottom)
        {
            float distance = top.DistanceTo(bottom);
            int blocks = (int)Math.Ceiling(distance / 30);
            Vector3 A = new Vector3((top.X - bottom.X) / blocks, (top.Y - bottom.Y) / blocks, (top.Z - bottom.Z) / blocks);
            Vector3 temp = Call<Vector3>("vectortoangles", new Parameter(top - bottom));
            Vector3 BA = new Vector3(temp.Z, temp.Y + 90, temp.X);
            for (int b = 0; b <= blocks; b++)
            {
                spawnCrate(bottom + (A * b), BA);
            }
        }

        public static List<Entity> usables = new List<Entity>();
        public void HandleUseables(Entity player)
        {
            foreach (Entity ent in usables)
            {
                if (player.Origin.DistanceTo(ent.Origin) < ent.GetField<int>("range"))
                {
                    switch (ent.GetField<string>("usabletype"))
                    {
                        case "door":
                            usedDoor(ent, player);
                            break;
                        case "randomWeapon":
                            if (Unitily.GetPlayerTeam(player) == "axis")
                            {
                                return;
                            }
                            else
                            {
                                usedRandomWeaponBox(ent, player);
                            }
                            break;
                        case "upgrade":
                            if (Unitily.GetPlayerTeam(player) == "axis")
                            {
                                return;
                            }
                            else
                            {
                                usedUpgrade(ent, player);
                            }
                            break;
                        case "ammo":
                            if (Unitily.GetPlayerTeam(player) == "axis")
                            {
                                return;
                            }
                            else
                            {
                                usedAmmoBox(ent, player);
                            }
                            break;
                        case "gambler":
                            if (Unitily.GetPlayerTeam(player) == "axis")
                            {
                                return;
                            }
                            else
                            {
                                usedGambler(ent, player);
                            }
                            break;
                        case "speedCola":
                            if (Unitily.GetPlayerTeam(player) == "axis")
                            {
                                return;
                            }
                            else
                            {
                                usedSpeedCola(ent, player);
                            }
                            break;
                        case "jugger":
                            if (Unitily.GetPlayerTeam(player) == "axis")
                            {
                                return;
                            }
                            else
                            {
                                usedJugger(ent, player);
                            }
                            break;
                        case "speedy":
                            if (Unitily.GetPlayerTeam(player) == "axis")
                            {
                                return;
                            }
                            else
                            {
                                usedSpeedy(ent, player);
                            }
                            break;
                        case "stalker":
                            if (Unitily.GetPlayerTeam(player) == "axis")
                            {
                                return;
                            }
                            else
                            {
                                usedStalker(ent, player);
                            }
                            break;
                        case "muleKick":
                            if (Unitily.GetPlayerTeam(player) == "axis")
                            {
                                return;
                            }
                            else
                            {
                                usedMuleKick(ent, player);
                            }
                            break;
                        case "doubleTap":
                            if (Unitily.GetPlayerTeam(player) == "axis")
                            {
                                return;
                            }
                            else
                            {
                                usedDoubleTap(ent, player);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public static void runOnUsable(Func<Entity, bool> func, string type)
        {
            foreach (Entity ent in usables)
            {
                if (ent.GetField<string>("usabletype") == type)
                {
                    func.Invoke(ent);
                }
            }
        }

        public static void notifyUsables(string notify)
        {
            foreach (Entity usable in usables)
            {
                usable.Notify(notify);
            }
        }

        public void UsablesHud(Entity player)
        {
            HudElem message = HudElem.CreateFontString(player, "hudbig", 0.6f);
            message.SetPoint("CENTER", "CENTER", 0, -50);
            OnInterval(100, () =>
            {
                bool _changed = false;
                foreach (Entity ent in usables)
                {
                    if (player.Origin.DistanceTo(ent.Origin) < ent.GetField<int>("range"))
                    {
                        switch (ent.GetField<string>("usabletype"))
                        {
                            case "door":
                                message.SetText(getDoorText(ent, player));
                                break;
                            case "randomWeapon":
                                message.SetText(getRandomWeaponText(ent, player));
                                break;
                            case "upgrade":
                                message.SetText(getUpgradeText(ent, player));
                                break;
                            case "ammo":
                                message.SetText(getAmmoText(ent, player));
                                break;
                            case "gambler":
                                message.SetText(getGamblerText(ent, player));
                                break;
                            case "speedCola":
                                message.SetText(getSpeedColaText(ent, player));
                                break;
                            case "jugger":
                                message.SetText(getJuggerText(ent, player));
                                break;
                            case "speedy":
                                message.SetText(getSpeedyText(ent, player));
                                break;
                            case "stalker":
                                message.SetText(getStalkerText(ent, player));
                                break;
                            case "muleKick":
                                message.SetText(getMuleKickText(ent, player));
                                break;
                            case "doubleTap":
                                message.SetText(getDoubleTapText(ent, player));
                                break;
                            default:
                                message.SetText("");
                                break;
                        }
                        _changed = true;
                    }
                }
                if (!_changed)
                {
                    message.SetText("");
                }
                return true;
            });
        }

        public string getDoorText(Entity door, Entity player)
        {
            int hp = door.GetField<int>("hp");
            int maxhp = door.GetField<int>("maxhp");
            if (Unitily.GetPlayerTeam(player) == "allies")
            {
                switch (door.GetField<string>("state"))
                {
                    case "open":
                        if (player.CurrentWeapon == "defaultweapon_mp")
                            return "Door is Open. Press ^3[{+activate}] ^7to repair it [修复门]. (" + hp + "/" + maxhp + ")";
                        return "Door is Open. Press ^3[{+activate}] ^7to close it [关闭门]. (" + hp + "/" + maxhp + ")";
                    case "close":
                        if (player.CurrentWeapon == "defaultweapon_mp")
                            return "Door is Closed. Press ^3[{+activate}] ^7to repair it [修复门]. (" + hp + "/" + maxhp + ")";
                        return "Door is Closed. Press ^3[{+activate}] ^7to open it [开启门]. (" + hp + "/" + maxhp + ")";
                    case "broken":
                        if (player.CurrentWeapon == "defaultweapon_mp")
                            return "Door is Broken. Press ^3[{+activate}] ^7to repair it [修复门]. (" + hp + "/" + maxhp + ")";
                        return "^1Door is Broken [门已损毁].";
                }
            }
            else if (Unitily.GetPlayerTeam(player) == "axis")
            {
                switch (door.GetField<string>("state"))
                {
                    case "open":
                        return "Door is Open [门已开启].";
                    case "close":
                        return "Press ^3[{+activate}] ^7to attack the door [破坏门].";
                    case "broken":
                        return "^1Door is Broken [门已损毁].";
                }
            }
            return "";
        }

        public string getRandomWeaponText(Entity box, Entity player)
        {
            if (Unitily.GetPlayerTeam(player) == "allies")
            {
                switch (box.GetField<string>("state"))
                {
                    case "idle":
                        return "Press ^3[{+activate}] ^7to random weapon [抽取武器] [$350].";
                    case "using":
                        if (box.GetField<string>("player") == player.GUID.ToString())
                        {
                            return "^2Random Weapon...";
                        }
                        else
                        {
                            return "^1Random Weapon is busy.";
                        }
                    case "waiting":
                        if (box.GetField<string>("player") == player.GUID.ToString())
                        {
                            return "Press ^3[{+activate}] ^7to trade you weapon.";
                        }
                        else
                        {
                            return "^1Random Weapon is busy.";
                        }
                }
            }
            return "";
        }

        public string getUpgradeText(Entity box, Entity player)
        {
            if (Unitily.GetPlayerTeam(player) == "allies")
            {
                switch (box.GetField<string>("state"))
                {
                    case "idle":
                        return "Press ^3[{+activate}] ^7to upgrade you ^1currect weapon ^7[升级你的^1手持武器^7] [$500].";
                    case "using":
                        if (box.GetField<string>("player") == player.GUID.ToString())
                        {
                            return "^2Upgrade Weapon...";
                        }
                        else
                        {
                            return "^1Upgrade is busy.";
                        }
                    case "waiting":
                        if (box.GetField<string>("player") == player.GUID.ToString())
                        {
                            return "Press ^3[{+activate}] ^7to trade you weapon.";
                        }
                        else
                        {
                            return "^1Upgrade is busy.";
                        }
                }
            }
            return "";
        }

        public string getAmmoText(Entity box, Entity player)
        {
            if (Unitily.GetPlayerTeam(player) == "allies")
            {
                return "Press ^3[{+activate}] ^7to buy ammo [子弹] [$300].";
            }
            return "";
        }


        public string getGamblerText(Entity box, Entity player)
        {
            if (Unitily.GetPlayerTeam(player) == "allies")
            {
                if (box.GetField<string>("state") == "idle")
                {
                    return "Press ^3[{+activate}] ^7to use Gambler [赌博机] [$500].";
                }
                else
                {
                    return "^1Gambler is busy.";
                }
            }
            return "";
        }

        public string getSpeedColaText(Entity box, Entity player)
        {
            if (Unitily.GetPlayerTeam(player) == "allies")
            {
                return "Press ^3[{+activate}] ^7to buy Speed Cola [快手] [$500].";
            }
            return "";
        }

        public string getJuggerText(Entity box, Entity player)
        {
            if (Unitily.GetPlayerTeam(player) == "allies")
            {
                return "Press ^3[{+activate}] ^7to buy Juggernog [重装] [$600].";
            }
            return "";
        }

        public string getSpeedyText(Entity box, Entity player)
        {
            if (Unitily.GetPlayerTeam(player) == "allies")
            {
                return "Press ^3[{+activate}] ^7to buy Stamin-Up [快速移动] [$600].";
            }
            return "";
        }

        public string getStalkerText(Entity box, Entity player)
        {
            if (Unitily.GetPlayerTeam(player) == "allies")
            {
                return "Press ^3[{+activate}] ^7to buy Stalker Soda [追踪者] [$500].";
            }
            return "";
        }

        public string getMuleKickText(Entity box, Entity player)
        {
            if (Unitily.GetPlayerTeam(player) == "allies")
            {
                return "Press ^3[{+activate}] ^7to buy Mule Kick [武器射速提升] [$500].";
            }
            return "";
        }

        public string getDoubleTapText(Entity box, Entity player)
        {
            if (Unitily.GetPlayerTeam(player) == "allies")
            {
                return "Press ^3[{+activate}] ^7to buy Double Tap [双倍武器伤害] [$500].";
            }
            return "";
        }

        public void MakeUsable(Entity ent, string type, int range)
        {
            ent.SetField("usabletype", type);
            ent.SetField("range", range);
            usables.Add(ent);
        }

        public void CreateDoor(Vector3 open, Vector3 close, Vector3 angle, int size, int height, int hp, int range)
        {
            double offset = (((size / 2) - 0.5) * -1);
            Entity center = Call<Entity>("spawn", "script_model", new Parameter(open));
            for (int j = 0; j < size; j++)
            {
                Entity door = spawnCrate(open + (new Vector3(0, 30, 0) * (float)offset), new Vector3(0, 0, 0));
                door.Call("setModel", "com_plasticcase_enemy");
                door.Call("enablelinkto");
                door.Call("linkto", center);
                for (int h = 1; h < height; h++)
                {
                    Entity door2 = spawnCrate(open + (new Vector3(0, 30, 0) * (float)offset) - (new Vector3(70, 0, 0) * h), new Vector3(0, 0, 0));
                    door2.Call("setModel", "com_plasticcase_enemy");
                    door2.Call("enablelinkto");
                    door2.Call("linkto", center);
                }
                offset += 1;
            }
            center.SetField("angles", new Parameter(angle));
            center.SetField("state", "open");
            center.SetField("hp", hp);
            center.SetField("maxhp", hp);
            center.SetField("open", new Parameter(open));
            center.SetField("close", new Parameter(close));

            MakeUsable(center, "door", range);
        }

        private void repairDoor(Entity door, Entity player)
        {
            if (player.GetField<int>("repairsleft") == 0) return; // no repairs left on weapon

            if (door.GetField<int>("hp") < door.GetField<int>("maxhp"))
            {
                door.SetField("hp", door.GetField<int>("hp") + 1);
                player.SetField("repairsleft", player.GetField<int>("repairsleft") - 1);
                player.Call("iprintlnbold", "Repaired Door! (" + player.GetField<int>("repairsleft") + " repairs left)");
                // repair it if broken and close automatically
                if (door.GetField<string>("state") == "broken")
                {
                    door.Call(33399, new Parameter(door.GetField<Vector3>("close")), 1); // moveto
                    AfterDelay(300, () =>
                    {
                        door.SetField("state", "close");
                    });
                }
            }
            else
            {
                player.Call("iprintlnbold", "Door has full health!");
            }
        }

        private void usedDoor(Entity door, Entity player)
        {
            if (!player.IsAlive) return;
            // has repair weapon. do repair door
            if (player.CurrentWeapon.Equals("defaultweapon_mp"))
            {
                repairDoor(door, player);
                return;
            }
            if (door.GetField<int>("hp") > 0)
            {
                if (Unitily.GetPlayerTeam(player) == "allies")
                {
                    if (door.GetField<string>("state") == "open")
                    {
                        door.Call(33399, new Parameter(door.GetField<Vector3>("close")), 1); // moveto
                        AfterDelay(300, () =>
                        {
                            door.SetField("state", "close");
                        });
                    }
                    else if (door.GetField<string>("state") == "close")
                    {
                        door.Call(33399, new Parameter(door.GetField<Vector3>("open")), 1); // moveto
                        AfterDelay(300, () =>
                        {
                            door.SetField("state", "open");
                        });
                    }
                }
                else
                {
                    if (door.GetField<string>("state") == "close")
                    {
                        if (player.GetField<int>("attackeddoor") == 0)
                        {
                            int hitchance = 0;
                            switch (player.Call<string>("getstance"))
                            {
                                case "prone":
                                    hitchance = 20;
                                    break;
                                case "couch":
                                    hitchance = 45;
                                    break;
                                case "stand":
                                    hitchance = 90;
                                    break;
                                default:
                                    break;
                            }
                            if (_rng.Next(100) < hitchance)
                            {
                                door.SetField("hp", door.GetField<int>("hp") - 1);
                                player.Call("iprintlnbold", "HIT: " + door.GetField<int>("hp") + "/" + door.GetField<int>("maxhp"));
                            }
                            else
                            {
                                player.Call("iprintlnbold", "^1MISS");
                            }
                            player.SetField("attackeddoor", 1);
                            player.AfterDelay(1000, (e) => player.SetField("attackeddoor", 0));
                        }
                    }
                }
            }
            else if (door.GetField<int>("hp") == 0 && door.GetField<string>("state") != "broken")
            {
                if (door.GetField<string>("state") == "close")
                    door.Call(33399, new Parameter(door.GetField<Vector3>("open")), 1f); // moveto
                door.SetField("state", "broken");
            }
        }

        public Entity CreateWall(Vector3 start, Vector3 end)
        {
            float D = new Vector3(start.X, start.Y, 0).DistanceTo(new Vector3(end.X, end.Y, 0));
            float H = new Vector3(0, 0, start.Z).DistanceTo(new Vector3(0, 0, end.Z));
            int blocks = (int)Math.Round(D / 55, 0);
            int height = (int)Math.Round(H / 30, 0);

            Vector3 C = end - start;
            Vector3 A = new Vector3(C.X / blocks, C.Y / blocks, C.Z / height);
            float TXA = A.X / 4;
            float TYA = A.Y / 4;
            Vector3 angle = Call<Vector3>("vectortoangles", new Parameter(C));
            angle = new Vector3(0, angle.Y, 90);
            Entity center = Call<Entity>("spawn", "script_origin", new Parameter(new Vector3(
                (start.X + end.X) / 2, (start.Y + end.Y) / 2, (start.Z + end.Z) / 2)));
            for (int h = 0; h < height; h++)
            {
                Entity crate = spawnCrate((start + new Vector3(TXA, TYA, 10) + (new Vector3(0, 0, A.Z) * h)), angle);
                crate.Call("enablelinkto");
                crate.Call("linkto", center);
                for (int i = 0; i < blocks; i++)
                {
                    crate = spawnCrate(start + (new Vector3(A.X, A.Y, 0) * i) + new Vector3(0, 0, 10) + (new Vector3(0, 0, A.Z) * h), angle);
                    crate.Call("enablelinkto");
                    crate.Call("linkto", center);
                }
                crate = spawnCrate(new Vector3(end.X, end.Y, start.Z) + new Vector3(TXA * -1, TYA * -1, 10) + (new Vector3(0, 0, A.Z) * h), angle);
                crate.Call("enablelinkto");
                crate.Call("linkto", center);
            }
            return center;
        }
        public Entity CreateFloor(Vector3 corner1, Vector3 corner2)
        {
            float width = corner1.X - corner2.X;
            if (width < 0) width = width * -1;
            float length = corner1.Y - corner2.Y;
            if (length < 0) length = length * -1;

            int bwide = (int)Math.Round(width / 50, 0);
            int blength = (int)Math.Round(length / 30, 0);
            Vector3 C = corner2 - corner1;
            Vector3 A = new Vector3(C.X / bwide, C.Y / blength, 0);
            Entity center = Call<Entity>("spawn", "script_origin", new Parameter(new Vector3(
                (corner1.X + corner2.X) / 2, (corner1.Y + corner2.Y) / 2, corner1.Z)));
            for (int i = 0; i < bwide; i++)
            {
                for (int j = 0; j < blength; j++)
                {
                    Entity crate = spawnCrate(corner1 + (new Vector3(A.X, 0, 0) * i) + (new Vector3(0, A.Y, 0) * j), new Vector3(0, 0, 0));
                    crate.Call("enablelinkto");
                    crate.Call("linkto", center);
                }
            }
            return center;
        }

        private int curObjID = 0;

        public void CreateElevator(Vector3 enter, Vector3 exit)
        {
            Entity flag = Call<Entity>("spawn", "script_model", new Parameter(enter));
            flag.Call("setModel", getAlliesFlagModel(_mapname));
            Entity flag2 = Call<Entity>("spawn", "script_model", new Parameter(exit));
            flag2.Call("setModel", getAxisFlagModel(_mapname));

            int _curObjID = 31 - curObjID++;
            Call(431, _curObjID, "active"); // objective_add
            Call(435, _curObjID, new Parameter(flag.Origin)); // objective_position
            Call(434, _curObjID, "compass_waypoint_bomb"); // objective_icon

            OnInterval(100, () =>
            {
                foreach (Entity player in Unitily.getPlayerList())
                {
                    if (player.Origin.DistanceTo(enter) <= 50)
                    {
                        player.Call("setorigin", new Parameter(exit));
                    }
                }
                return true;
            });
        }

        public void CreateHiddenTP(Vector3 enter, Vector3 exit)
        {
            Entity flag = Call<Entity>("spawn", "script_model", new Parameter(enter));
            flag.Call("setModel", "weapon_scavenger_grenadebag");
            Entity flag2 = Call<Entity>("spawn", "script_model", new Parameter(exit));
            flag2.Call("setModel", "weapon_oma_pack");
            OnInterval(100, () =>
            {
                foreach (Entity player in Unitily.getPlayerList())
                {
                    if (player.Origin.DistanceTo(enter) <= 50)
                    {
                        player.Call("setorigin", new Parameter(exit));
                    }
                }
                return true;
            });
        }

        public Entity spawnModel(string model, Vector3 origin, Vector3 angles)
        {
            Entity ent = Call<Entity>("spawn", "script_model", new Parameter(origin));
            ent.Call("setmodel", model);
            ent.SetField("angles", new Parameter(angles));
            return ent;
        }

        public Entity spawnCrate(Vector3 origin, Vector3 angles)
        {
            Entity ent = Call<Entity>("spawn", "script_model", new Parameter(origin));
            ent.Call("setmodel", "com_plasticcase_friendly");
            ent.SetField("angles", new Parameter(angles));
            ent.Call(33353, _airdropCollision);
            return ent;
        }
        public Entity[] getSpawns(string name)
        {
            return Call<Entity[]>("getentarray", name, "classname");
        }
        public void removeSpawn(Entity spawn)
        {
            spawn.Call("delete");
        }
        public void createSpawn(string type, Vector3 origin, Vector3 angle)
        {
            Entity spawn = Call<Entity>("spawn", type, new Parameter(origin));
            spawn.SetField("angles", new Parameter(angle));
        }

        private static void print(string format, params object[] p)
        {
            Log.Write(LogLevel.All, format, p);
        }

        public void LaptopRotate(Entity laptop)
        {
            OnInterval(7000, () =>
            {
                laptop.Call("rotateyaw", -360, 7);
                return true;
            });
        }

        public void CreateTurret(Vector3 origin, Vector3 angles)
        {
            Entity turret = Call<Entity>("spawn", "script_model");
            turret.SetField("angles", new Parameter(angles));
            if (angles.Equals(null))
                angles = new Vector3(0f, 0f, 0f);
            turret = Call<Entity>("spawnTurret", "misc_turret", new Parameter(origin), "pavelow_minigun_mp");
            turret.Call("setmodel", "weapon_minigun");
            turret.SetField("angles", angles);
        }

        //LSDZombie Extreme
        public void CreateBoxShader(Entity box, string shader)
        {
            HudElem hudicon = HudElem.NewTeamHudElem("allies");
            hudicon.SetShader(shader, 20, 20);
            hudicon.Alpha = 0.6f;
            hudicon.X = box.Origin.X;
            hudicon.Y = box.Origin.Y;
            hudicon.Z = box.Origin.Z + 40;
            hudicon.Call("SetWayPoint", true, true);
        }

        public void CreateRandomBox(Vector3 origin, Vector3 angle)
        {
            Call("precacheshader", "specialty_quickdraw");
            Entity crate = Call<Entity>("spawn", "script_model", new Parameter(origin));
            crate.Call("setmodel", "com_plasticcase_friendly");
            crate.SetField("angles", new Parameter(angle));
            crate.Call("clonebrushmodeltoscriptmodel", _airdropCollision);
            crate.SetField("state", "idle");
            crate.SetField("weapon", "");
            crate.SetField("player", "");

            CreateBoxShader(crate, "specialty_quickdraw");

            int _curObjID = 31 - curObjID++;
            Call("objective_state", _curObjID, "active");
            Call("objective_position", _curObjID, new Parameter(origin));
            Call("objective_team", _curObjID, "allies");
            Call("objective_icon", _curObjID, "specialty_quickdraw");

            MakeUsable(crate, "randomWeapon", 50);
        }

        public static string[] weaponlist = 
        {
            "iw5_1887_mp",
            "iw5_44magnum_mp",
            "iw5_aa12_mp",
            "iw5_acr_mp_camo13", 
            "iw5_ak47_mp",
            "iw5_as50_mp_as50scope",
            "iw5_barrett_mp_barrettscope", 
            "iw5_cm901_mp",
            "iw5_deserteagle_mp",
            "iw5_dragunov_mp_dragunovscope",
            "iw5_fad_mp",
            "iw5_fmg9_mp",
            "iw5_fnfiveseven_mp",
            "iw5_g18_mp", 
            "iw5_g36c_mp",
            "iw5_ksg_mp", 
            "iw5_l96a1_mp_l96a1scope",
            "iw5_m16_mp",
            "iw5_m4_mp",
            "iw5_m60_mp",
            "iw5_m9_mp",
            "iw5_m9_mp_eotechsmg_camo08",
            "iw5_mg36_mp",
            "iw5_mk14_mp",
            "iw5_mk46_mp",
            "iw5_mp412_mp",
            "iw5_mp5_mp",
            "iw5_mp7_mp",
            "iw5_mp9_mp",
            "iw5_mp9_mp_eotechsmg_silencer02",
            "iw5_msr_mp_msrscope",
            "iw5_p90_mp",
            "iw5_p99_mp",
            "iw5_pecheneg_mp",
            "iw5_pp90m1_mp",
            "iw5_rsass_mp_rsassscope",
            "iw5_sa80_mp",
            "iw5_scar_mp_camo13",
            "iw5_skorpion_mp",
            "iw5_spas12_mp", 
            "iw5_striker_mp",
            "iw5_type95_mp", 
            "iw5_ump45_mp",
            "iw5_usas12_mp", 
            "iw5_usp45_mp",
            "riotshield_mp",
            "iw5_smaw_mp",
            "xm25_mp",
            "javelin_mp",
            "stinger_mp",
        };

        public int getWeaponsNum(Entity player)
        {
            int num = 0;
            foreach (string item in weaponlist)
            {
                if (player.HasWeapon(item))
                {
                    num++;
                }
            }
            foreach (string item in upgradeweaponlist)
            {
                if (player.HasWeapon(item))
                {
                    num++;
                }
            }
            return num;
        }

        private Entity weapon;
        private bool isDestroy = false;
        public void usedRandomWeaponBox(Entity box, Entity player)
        {
            if (!player.IsAlive) return;
            if (Unitily.GetPlayerTeam(player) == "axis") return;
            else
            {
                if (box.GetField<string>("state") == "using") return;
                if (box.GetField<string>("state") == "waiting" && box.GetField<string>("player") != player.GUID.ToString()) return;
                if (box.GetField<string>("state") == "waiting" && box.GetField<string>("player") == player.GUID.ToString())
                {
                    if (player.HasWeapon(box.GetField<string>("weapon")))
                    {
                        player.Call("givemaxammo", box.GetField<string>("weapon"));
                        player.SwitchToWeapon(box.GetField<string>("weapon"));
                    }
                    else
                    {
                        if (getWeaponsNum(player) > 1)
                        {
                            player.TakeWeapon(player.CurrentWeapon);
                        }
                        player.GiveWeapon(box.GetField<string>("weapon"));
                        player.Call("givemaxammo", box.GetField<string>("weapon"));
                        player.SwitchToWeapon(box.GetField<string>("weapon"));
                    }
                    weapon.Call("delete");
                    box.SetField("weapon", "");
                    box.SetField("player", "");
                    AfterDelay(2000, () => box.SetField("state", "idle"));
                    isDestroy = true;
                }
                else if (box.GetField<string>("state") == "idle")
                {
                    if (player.GetField<int>("lsd_money") >= 350)
                    {
                        weapon = Call<Entity>("spawn", "script_model", new Parameter(box.Origin));
                        weapon.SetField("angles", box.GetField<Vector3>("angles"));
                        player.SetField("lsd_money", player.GetField<int>("lsd_money") - 350);
                        int? rng = new Random().Next(weaponlist.Length);
                        Vector3 temp = box.Origin;
                        temp.Z = temp.Z + 40;
                        weapon.Call("moveto", temp, 3);
                        box.SetField("player", player.GUID.ToString());
                        box.SetField("state", "using");
                        for (int i = 0; i < 3000; i += 100)
                        {
                            AfterDelay(0 + i, () =>
                            {
                                rng = new Random().Next(weaponlist.Length);
                                int r = rng.Value;
                                string _weapon = weaponlist[r];
                                if (_weapon.StartsWith("iw5_acr"))
                                {
                                    weapon.Call("setmodel", new Parameter(Call<string>("GetWeaponModel", _weapon, 13)));
                                }
                                else
                                {
                                    weapon.Call("setmodel", new Parameter(Call<string>("GetWeaponModel", _weapon, 0)));
                                }
                            });
                        }
                        for (int i = 0; i < 1000; i += 300)
                        {
                            AfterDelay(3000 + i, () =>
                            {
                                rng = new Random().Next(weaponlist.Length);
                                int r = rng.Value;
                                string _weapon = weaponlist[r];
                                if (_weapon.StartsWith("iw5_acr"))
                                {
                                    weapon.Call("setmodel", new Parameter(Call<string>("GetWeaponModel", _weapon, 13)));
                                }
                                else
                                {
                                    weapon.Call("setmodel", new Parameter(Call<string>("GetWeaponModel", _weapon, 0)));
                                }
                            });
                        }

                        AfterDelay(4000, () =>
                        {
                            int r = rng.Value;
                            string _weapon = weaponlist[r];
                            if (_weapon.StartsWith("iw5_acr") || _weapon.StartsWith("iw5_scar"))
                            {
                                weapon.Call("setmodel", new Parameter(Call<string>("GetWeaponModel", _weapon, 13)));
                            }
                            else
                            {
                                weapon.Call("setmodel", new Parameter(Call<string>("GetWeaponModel", _weapon, 0)));
                            }
                            temp.Z = temp.Z - 40;
                            weapon.Call("moveto", temp, 8);
                            box.SetField("state", "waiting");
                            box.SetField("weapon", _weapon);
                        });
                        AfterDelay(12000, () =>
                        {
                            if (box.GetField<string>("state") == "waiting" && isDestroy == false)
                            {
                                box.SetField("state", "idle");
                                box.SetField("weapon", "");
                                box.SetField("player", "");
                                weapon.Call("delete");
                            }
                            isDestroy = false;
                        });
                    }
                    else
                    {
                        player.Call("iprintln", "^1Random weapon need $350.");
                    }
                }
            }
        }

        public void CreateUpgrade(Vector3 origin, Vector3 angle)
        {
            Entity crate = Call<Entity>("spawn", "script_model", new Parameter(origin));
            crate.Call("setmodel", "com_plasticcase_enemy");
            crate.SetField("angles", new Parameter(angle));
            crate.Call("clonebrushmodeltoscriptmodel", _airdropCollision);
            crate.SetField("state", "idle");
            crate.SetField("weapon", "");
            crate.SetField("player", "");

            CreateBoxShader(crate, "cardicon_tf141");

            int _curObjID = 31 - curObjID++;
            Call("objective_state", _curObjID, "active");
            Call("objective_position", _curObjID, new Parameter(origin));
            Call("objective_team", _curObjID, "allies");
            Call("objective_icon", _curObjID, "cardicon_tf141");

            MakeUsable(crate, "upgrade", 50);

        }

        public static string[] upgradeweaponlist = 
        {
            "iw5_1887_mp_camo09",
            "iw5_44magnum_mp_akimbo_xmags",
            "iw5_aa12_mp_grip_xmags_camo09",
            "iw5_acr_mp_xmags_camo12", 
            "iw5_ak47_mp_xmags_camo09",
            "iw5_as50_mp_acog_xmags_camo09",
            "iw5_barrett_mp_acog_xmags_camo09", 
            "iw5_cm901_mp_eotech_xmags_camo09",
            "iw5_deserteagle_mp_akimbo_xmags",
            "iw5_dragunov_mp_acog_xmags_camo09",
            "iw5_fad_mp_xmags_camo09",
            "iw5_fmg9_mp_akimbo_eotechsmg_xmags",
            "iw5_fnfiveseven_mp_akimbo_xmags",
            "iw5_g18_mp_akimbo_eotechsmg_xmags", 
            "iw5_g36c_mp_hybrid_xmags_camo09",
            "iw5_ksg_mp_grip_xmags_camo09", 
            "iw5_l96a1_mp_acog_xmags_camo09",
            "iw5_m16_mp_eotech_xmags_camo09",
            "iw5_m4_mp_eotech_xmags_camo09",
            "iw5_m60jugg_mp_eotechlmg_camo07",
            "iw5_m9_mp_rof_xmags_camo09",
            "iw5_m9_mp_eotechsmg_xmags_camo08",
            "iw5_mg36_mp_grip_rof_xmags_camo09",
            "iw5_mk14_mp_rof_xmags_camo09",
            "iw5_mk46_mp_grip_reflexlmg_xmags_camo09",
            "iw5_mp412_mp_akimbo_xmags",
            "iw5_mp5_mp_rof_xmags_camo09",
            "iw5_mp7_mp_rof_xmags_camo09",
            "iw5_mp9_mp_akimbo_xmags",
            "iw5_mp9_mp_eotechsmg_silencer02_xmags",
            "iw5_msr_mp_acog_xmags_camo09",
            "iw5_p90_mp_rof_xmags_camo09",
            "iw5_p99_mp_akimbo_xmags",
            "iw5_pecheneg_mp_grip_reflexlmg_xmags_camo09",
            "iw5_pp90m1_mp_rof_xmags_camo09",
            "iw5_rsass_mp_acog_xmags_camo09",
            "iw5_sa80_mp_grip_xmags_camo09",
            "iw5_scar_mp_xmags_camo12",
            "iw5_skorpion_mp_akimbo_xmags",
            "iw5_spas12_mp_grip_xmags_camo09", 
            "iw5_striker_mp_grip_xmags_camo09",
            "iw5_type95_mp_reflex_xmags_camo09", 
            "iw5_ump45_mp_rof_xmags_camo09",
            "iw5_usas12_mp_grip_xmags_camo09", 
            "iw5_usp45_mp_silencer02",
            "iw5_riotshieldjugg_mp",
            "rpg_mp",
            "uav_strike_marker_mp",
            "iw5_usp45_mp_akimbo_xmags",
            "iw5_striker_mp_grip_silencer03_camo09"
        };

        private Entity weapon2;
        private bool isDestroy2 = false;
        public void usedUpgrade(Entity box, Entity player)
        {
            if (!player.IsAlive) return;
            if (Unitily.GetPlayerTeam(player) == "axis") return;
            else
            {
                if (box.GetField<string>("state") == "using") return;
                if (box.GetField<string>("state") == "waiting" && box.GetField<string>("player") != player.GUID.ToString()) return;
                if (box.GetField<string>("state") == "waiting" && box.GetField<string>("player") == player.GUID.ToString())
                {
                    player.GiveWeapon(box.GetField<string>("weapon"));
                    player.Call("givemaxammo", box.GetField<string>("weapon"));
                    player.SwitchToWeapon(box.GetField<string>("weapon"));
                    weapon2.Call("delete");
                    box.SetField("weapon", "");
                    box.SetField("player", "");
                    AfterDelay(2000, () => box.SetField("state", "idle"));
                    this.isDestroy2 = true;
                }
                else if (box.GetField<string>("state") == "idle")
                {
                    if (player.GetField<int>("lsd_money") >= 500)
                    {
                        if (weaponlist.Contains(player.CurrentWeapon))
                        {
                            weapon2 = Call<Entity>("spawn", "script_model", new Parameter(box.Origin));
                            weapon2.SetField("angles", box.GetField<Vector3>("angles"));
                            player.SetField("lsd_usingboxnum", box.EntRef);
                            player.SetField("lsd_money", player.GetField<int>("lsd_money") - 500);
                            box.SetField("state", "using");
                            box.SetField("player", player.GUID.ToString());
                            weapon2.SetField("working", 1);
                            string _weapon = player.CurrentWeapon;
                            if (_weapon.StartsWith("iw5_acr") || _weapon.StartsWith("iw5_scar"))
                            {
                                weapon2.Call("setmodel", new Parameter(Call<string>("GetWeaponModel", _weapon, 13)));
                            }
                            else
                            {
                                weapon2.Call("setmodel", new Parameter(Call<string>("GetWeaponModel", _weapon, 0)));
                            }
                            Vector3 temp = player.Origin;
                            temp.Z = temp.Z + 50;
                            player.TakeWeapon(player.CurrentWeapon);
                            weapon2.Origin = temp;
                            weapon2.Call("moveto", box.Origin, 3);
                            AfterDelay(3000, () =>
                            {
                                for (int i = 0; i < weaponlist.Length; i++)
                                {
                                    if (weaponlist[i] == _weapon)
                                    {
                                        _weapon = upgradeweaponlist[i];
                                        break;
                                    }
                                }
                                if (_weapon.StartsWith("iw5_acr") || _weapon.StartsWith("iw5_scar"))
                                {
                                    weapon2.Call("setmodel", new Parameter(Call<string>("GetWeaponModel", _weapon, 12)));
                                }
                                else if (_weapon.StartsWith("iw5_m9_mp_eotechsmg"))
                                {
                                    weapon2.Call("setmodel", new Parameter(Call<string>("GetWeaponModel", _weapon, 8)));
                                }
                                else if (_weapon.StartsWith("iw5_m60"))
                                {
                                    weapon2.Call("setmodel", "weapon_steyr_blue_tiger");
                                }
                                else
                                {
                                    weapon2.Call("setmodel", new Parameter(Call<string>("GetWeaponModel", _weapon, 9)));
                                }
                                temp = box.Origin;
                                temp.Z = temp.Z + 50;
                                weapon2.Call("moveto", temp, 3);
                            });
                            AfterDelay(6000, () =>
                            {
                                temp = box.Origin;
                                box.SetField("state", "waiting");
                                box.SetField("weapon", _weapon);
                                weapon2.Call("moveto", temp, 8);
                            });
                            AfterDelay(14000, () =>
                            {
                                if (box.GetField<string>("state") == "waiting" && this.isDestroy2 == false)
                                {
                                    box.SetField("state", "idle");
                                    box.SetField("weapon", "");
                                    box.SetField("player", "");
                                    weapon.Call("delete");
                                }
                                isDestroy2 = false;
                            });
                        }
                        else
                        {
                            player.Call("iprintlnbold", "^1You currect weapon can not upgrade.");
                        }
                    }
                    else
                    {
                        player.Call("iprintln", "^1Upgrade need $500.");
                    }
                }
            }
        }

        public void CreateAmmoBox(Vector3 origin, Vector3 angle)
        {
            Entity crate = Call<Entity>("spawn", "script_model", new Parameter(origin));
            crate.Call("setmodel", "com_plasticcase_friendly");
            crate.SetField("angles", new Parameter(angle));
            crate.Call("clonebrushmodeltoscriptmodel", _airdropCollision);

            CreateBoxShader(crate, "waypoint_ammo_friendly");

            Vector3 laptopvec = origin;
            laptopvec.Z = laptopvec.Z + 17;
            Entity laptop = Call<Entity>("spawn", "script_model", new Parameter(laptopvec));
            laptop.Call("setmodel", "com_laptop_2_open");
            LaptopRotate(laptop);

            int _curObjID = 31 - curObjID++;
            Call("objective_state", _curObjID, "active");
            Call("objective_position", _curObjID, new Parameter(origin));
            Call("objective_team", _curObjID, "allies");
            Call("objective_icon", _curObjID, "waypoint_ammo_friendly");

            MakeUsable(crate, "ammo", 50);
        }

        public void GiveAmmo(Entity player)
        {
            foreach (string item in weaponlist)
            {
                if (player.HasWeapon(item))
                {
                    player.Call("givemaxammo", item);
                }
            }
            foreach (string item in upgradeweaponlist)
            {
                if (player.HasWeapon(item))
                {
                    player.Call("givemaxammo", item);
                }
            }
        }

        public void usedAmmoBox(Entity box, Entity player)
        {
            if (!player.IsAlive) return;
            if (Unitily.GetPlayerTeam(player) == "axis") return;
            else
            {
                if (player.GetField<int>("lsd_money") >= 300)
                {
                    player.SetField("lsd_money", player.GetField<int>("lsd_money") - 300);
                    GiveAmmo(player);
                    player.Call("playlocalsound", "ammo_crate_use");
                    Utilities.RawSayTo(player, "^2Ammo Givened.");
                }
                else
                {
                    player.Call("iprintln", "^1Ammo need $500.");
                }
            }
        }

        public void CreateGambler(Vector3 origin, Vector3 angle)
        {
            Entity crate = Call<Entity>("spawn", "script_model", new Parameter(origin));
            crate.Call("setmodel", "com_plasticcase_friendly");
            crate.SetField("angles", new Parameter(angle));
            crate.Call("clonebrushmodeltoscriptmodel", _airdropCollision);
            crate.SetField("state", "idle");

            CreateBoxShader(crate, "cardicon_8ball");

            Vector3 laptopvec = origin;
            laptopvec.Z = laptopvec.Z + 17;
            Entity laptop = Call<Entity>("spawn", "script_model", new Parameter(laptopvec));
            laptop.Call("setmodel", "com_laptop_2_open");
            LaptopRotate(laptop);
            crate.SetField("state", "idle");

            int _curObjID = 31 - curObjID++;
            Call("objective_state", _curObjID, "active");
            Call("objective_position", _curObjID, new Parameter(origin));
            Call("objective_team", _curObjID, "allies");
            Call("objective_icon", _curObjID, "cardicon_8ball");

            MakeUsable(crate, "gambler", 50);

        }

        public void usedGambler(Entity box, Entity player)
        {
            if (!player.IsAlive) return;
            if (Unitily.GetPlayerTeam(player) == "axis") return;
            else
            {
                if (box.GetField<string>("state") == "using") return;
                if (box.GetField<string>("state") == "idle" && player.GetField<int>("lsd_money") >= 500)
                {
                    box.SetField("state", "using");
                    player.SetField("lsd_money", player.GetField<int>("lsd_money") - 500);
                    GamblerThink(player);
                    AfterDelay(10000, () =>
                    {
                        box.SetField("state", "idle");
                    });
                }
                else
                {
                    player.Call("iprintln", "^1Gambler need $500.");
                }
            }
        }

        public void setPlayerTeam(Entity player, string team)
        {
            if (player.GetField<string>("sessionteam") == team) return;
            player.Notify("menuresponse", "team_marinesopfor", team);
            player.Call("suicide");
        }

        public void GamblerThink(Entity player)
        {
            player.Call("iprintlnbold", ("^210"));
            player.Call("playlocalsound", "ui_mp_nukebomb_timer");
            AfterDelay(1000, () => player.Call("iprintlnbold", ("^29")));
            AfterDelay(1000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
            AfterDelay(2000, () => player.Call("iprintlnbold", ("^28")));
            AfterDelay(2000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
            AfterDelay(3000, () => player.Call("iprintlnbold", ("^27")));
            AfterDelay(3000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
            AfterDelay(4000, () => player.Call("iprintlnbold", ("^26")));
            AfterDelay(4000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
            AfterDelay(5000, () => player.Call("iprintlnbold", ("^25")));
            AfterDelay(5000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
            AfterDelay(6000, () => player.Call("iprintlnbold", ("^24")));
            AfterDelay(6000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
            AfterDelay(7000, () => player.Call("iprintlnbold", ("^23")));
            AfterDelay(7000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
            AfterDelay(8000, () => player.Call("iprintlnbold", ("^22")));
            AfterDelay(8000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
            AfterDelay(9000, () => player.Call("iprintlnbold", ("^21")));
            AfterDelay(9000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
            AfterDelay(10000, () =>
            {
                int? rng = new Random().Next(14);
                switch (rng.Value)
                {
                    case 0:
                        player.Call("iprintlnbold", ("^1You win nothing."));
                        player.Call("playlocalsound", "mp_bonus_end");
                        break;
                    case 1:
                        player.Call("iprintlnbold", ("^2You win 500."));
                        player.Call("playlocalsound", "mp_bonus_start");
                        player.SetField("lsd_money", player.GetField<int>("lsd_money") + 500);
                        break;
                    case 2:
                        player.Call("iprintlnbold", ("^2You win 1000."));
                        player.Call("playlocalsound", "mp_bonus_start");
                        player.SetField("lsd_money", player.GetField<int>("lsd_money") + 1000);
                        break;
                    case 3:
                        player.Call("iprintlnbold", ("^2You win 2000."));
                        player.Call("playlocalsound", "mp_bonus_start");
                        player.SetField("lsd_money", player.GetField<int>("lsd_money") + 2000);
                        break;
                    case 4:
                        player.Call("iprintlnbold", ("^2You win a Ray Gun."));
                        player.Call("playlocalsound", "mp_bonus_start");
                        if (getWeaponsNum(player) > 1)
                        {
                            player.TakeWeapon(player.CurrentWeapon);
                        }
                        player.GiveWeapon("iw5_m9_mp_eotechsmg_camo08");
                        player.Call("givemaxammo", "iw5_m9_mp_eotechsmg_camo08");
                        player.SwitchToWeapon("iw5_m9_mp_eotechsmg_camo08");
                        break;
                    case 5:
                        player.Call("iprintlnbold", ("^1You lose 500."));
                        player.Call("playlocalsound", "mp_bonus_end");
                        player.SetField("lsd_money", player.GetField<int>("lsd_money") - 500);
                        break;
                    case 6:
                        player.Call("iprintlnbold", ("^1You lose all money."));
                        player.Call("playlocalsound", "mp_bonus_end");
                        player.SetField("lsd_money", 0);
                        break;
                    case 7:
                        player.Call("iprintlnbold", ("^2You win max ammo."));
                        player.Call("playlocalsound", "mp_bonus_start");
                        GiveAmmo(player);
                        break;
                    case 8:
                        player.Call("iprintlnbold", ("^1You lose all perks."));
                        player.Call("playlocalsound", "mp_bonus_end");
                        player.Call("clearperks");
                        player.SetField("speedcolaDone", 0);
                        player.SetField("juggerDone", 0);
                        player.SetField("speedyDone", 0);
                        player.SetField("stalkerDone", 0);
                        player.SetField("mulekickDone", 0);
                        player.SetField("doubletapDone", 0);
                        player.SetField("perk1bought", 0);
                        player.SetField("perk2bought", 0);
                        player.SetField("perk3bought", 0);
                        player.SetField("perk4bought", 0);
                        player.SetField("perk5bought", 0);
                        player.SetField("perk6bought", 0);
                        player.Notify("lsd_clearperks", player);
                        break;
                    case 9:
                        player.Call("iprintlnbold", ("^2You win 10000."));
                        player.Call("playlocalsound", "mp_bonus_start");
                        player.SetField("lsd_money", player.GetField<int>("lsd_money") + 10000);
                        break;
                    case 10:
                        player.Call("iprintlnbold", ("^2You win a USAS."));
                        player.Call("playlocalsound", "mp_bonus_start");
                        if (getWeaponsNum(player) > 1)
                        {
                            player.TakeWeapon(player.CurrentWeapon);
                        }
                        player.GiveWeapon("iw5_usas12_mp");
                        player.Call("givemaxammo", "iw5_usas12_mp");
                        player.SwitchToWeapon("iw5_usas12_mp");
                        break;
                    case 11:
                        player.Call("iprintlnbold", ("^1You live or die in 5 second."));
                        player.Call("playlocalsound", "mp_bonus_end");
                        AfterDelay(1000, () => player.Call("iprintlnbold", ("^14")));
                        AfterDelay(1000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
                        AfterDelay(2000, () => player.Call("iprintlnbold", ("^13")));
                        AfterDelay(2000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
                        AfterDelay(3000, () => player.Call("iprintlnbold", ("^12")));
                        AfterDelay(3000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
                        AfterDelay(4000, () => player.Call("iprintlnbold", ("^11")));
                        AfterDelay(4000, () => player.Call("playlocalsound", "ui_mp_nukebomb_timer"));
                        AfterDelay(5000, () =>
                        {
                            rng = new Random().Next(2);
                            switch (rng)
                            {
                                case 0:
                                    player.Call("iprintlnbold", ("^2You live!"));
                                    break;
                                case 1:
                                    player.Call("iprintlnbold", ("^1You die!"));
                                    player.Call("suicide");
                                    break;
                            }
                        });
                        break;
                    case 12:
                        player.Call("iprintlnbold", ("^2Gambler restart."));
                        player.Call("playlocalsound", "mp_bonus_end");
                        AfterDelay(1000, () => GamblerThink(player));
                        break;
                    case 13:
                        player.Call("iprintlnbold", ("^1You lose you curret weapon."));
                        player.Call("playlocalsound", "mp_bonus_end");
                        player.TakeWeapon(player.CurrentWeapon);
                        break;
                }
            });
        }

        public void updatePerkHUD(Entity player, string perk)
        {
            if (player.GetField<string>("perk1bought") == "")
            {
                player.SetField("perk1bought", perk);
                return;
            }
            if (player.GetField<string>("perk1bought") != "" && player.GetField<string>("perk2bought") == "")
            {
                player.SetField("perk2bought", perk);
                return;
            }
            if (player.GetField<string>("perk1bought") != "" && (player.GetField<string>("perk2bought") != "" && player.GetField<string>("perk3bought") == ""))
            {
                player.SetField("perk3bought", perk);
                return;
            }
            if (player.GetField<string>("perk1bought") != "" && (player.GetField<string>("perk2bought") != "" && (player.GetField<string>("perk3bought") != "" && player.GetField<string>("perk4bought") == "")))
            {
                player.SetField("perk4bought", perk);
                return;
            }
            if (player.GetField<string>("perk1bought") != "" && (player.GetField<string>("perk2bought") != "" && (player.GetField<string>("perk3bought") != "" && (player.GetField<string>("perk4bought") != "" && player.GetField<string>("perk5bought") == ""))))
            {
                player.SetField("perk5bought", perk);
                return;
            }
            if (player.GetField<string>("perk1bought") != "" && (player.GetField<string>("perk2bought") != "" && (player.GetField<string>("perk3bought") != "" && (player.GetField<string>("perk4bought") != "" && (player.GetField<string>("perk5bought") != "" && player.GetField<string>("perk6bought") == "")))))
            {
                player.SetField("perk6bought", perk);
                return;
            }
        }

        public void CreateSpeedCola(Vector3 origin, Vector3 angle)
        {
            Call("precacheshader", "specialty_fastreload_upgrade");
            Entity crate = Call<Entity>("spawn", "script_model", new Parameter(origin));
            crate.Call("setmodel", "com_plasticcase_friendly");
            crate.SetField("angles", new Parameter(angle));
            crate.Call("clonebrushmodeltoscriptmodel", _airdropCollision);

            CreateBoxShader(crate, "specialty_fastreload_upgrade");

            Vector3 laptopvec = origin;
            laptopvec.Z = laptopvec.Z + 17;
            Entity laptop = Call<Entity>("spawn", "script_model", new Parameter(laptopvec));
            laptop.Call("setmodel", "com_laptop_2_open");
            LaptopRotate(laptop);

            int _curObjID = 31 - curObjID++;
            Call("objective_state", _curObjID, "active");
            Call("objective_position", _curObjID, new Parameter(origin));
            Call("objective_team", _curObjID, "allies");
            Call("objective_icon", _curObjID, "specialty_fastreload_upgrade");

            MakeUsable(crate, "speedCola", 50);
        }

        public void usedSpeedCola(Entity box, Entity player)
        {
            if (!player.IsAlive) return;
            if (Unitily.GetPlayerTeam(player) == "axis") return;
            else
            {
                if (player.GetField<int>("speedcolaDone") == 1)
                {
                    player.Call("iprintln", "^1You already have Speed Cola.");
                    return;
                }
                if (player.GetField<int>("lsd_money") >= 500)
                {
                    player.SetField("lsd_money", player.GetField<int>("lsd_money") - 500);
                    player.SetPerk("specialty_fastreload", true, false);
                    player.SetPerk("specialty_quickswap", true, false);
                    player.SetPerk("specialty_quickdraw", true, false);
                    Utilities.RawSayTo(player, "^2Fast reload; quick swap; quick draw.");
                    player.Call("playlocalsound", "earn_perk");
                    player.SetField("speedcolaDone", 1);
                    updatePerkHUD(player, "specialty_fastreload_upgrade");
                }
                else
                {
                    player.Call("iprintln", "^1Speed Cola need $500.");
                }
            }
        }

        public void CreateJugger(Vector3 origin, Vector3 angle)
        {
            Call("precacheshader", "cardicon_juggernaut_1");
            Entity crate = Call<Entity>("spawn", "script_model", new Parameter(origin));
            crate.Call("setmodel", "com_plasticcase_friendly");
            crate.SetField("angles", new Parameter(angle));
            crate.Call("clonebrushmodeltoscriptmodel", _airdropCollision);

            CreateBoxShader(crate, "cardicon_juggernaut_1");

            Vector3 laptopvec = origin;
            laptopvec.Z = laptopvec.Z + 17;
            Entity laptop = Call<Entity>("spawn", "script_model", new Parameter(laptopvec));
            laptop.Call("setmodel", "com_laptop_2_open");
            LaptopRotate(laptop);

            int _curObjID = 31 - curObjID++;
            Call("objective_state", _curObjID, "active");
            Call("objective_position", _curObjID, new Parameter(origin));
            Call("objective_team", _curObjID, "allies");
            Call("objective_icon", _curObjID, "cardicon_juggernaut_1");

            MakeUsable(crate, "jugger", 50);
        }

        public void usedJugger(Entity box, Entity player)
        {
            if (!player.IsAlive) return;
            if (Unitily.GetPlayerTeam(player) == "axis") return;
            else
            {
                if (player.GetField<int>("juggerDone") == 1)
                {
                    player.Call("iprintln", "^1You already have Juggernog.");
                    return;
                }
                if (player.GetField<int>("lsd_money") >= 600)
                {
                    player.SetField("lsd_money", player.GetField<int>("lsd_money") - 600);
                    player.SetField("maxhealth", 400);
                    player.Health = 400;
                    player.SetPerk("_specialty_blastshield", true, false);
                    player.SetPerk("specialty_sharp_focus", true, false);
                    player.SetPerk("specialty_armorvest", true, false);
                    player.Call("setmodel", "mp_fullbody_ally_juggernaut");
                    Utilities.RawSayTo(player, "^2400 Health.");
                    player.Call("playlocalsound", "earn_perk");
                    player.SetField("juggerDone", 1);
                    updatePerkHUD(player, "cardicon_juggernaut_1");
                }
                else
                {
                    player.Call("iprintln", "^1Juggernog need $600.");
                }
            }
        }

        public void CreateSpeedy(Vector3 origin, Vector3 angle)
        {
            Call("precacheshader", "specialty_longersprint_upgrade");
            Entity crate = Call<Entity>("spawn", "script_model", new Parameter(origin));
            crate.Call("setmodel", "com_plasticcase_friendly");
            crate.SetField("angles", new Parameter(angle));
            crate.Call("clonebrushmodeltoscriptmodel", _airdropCollision);

            CreateBoxShader(crate, "specialty_longersprint_upgrade");

            Vector3 laptopvec = origin;
            laptopvec.Z = laptopvec.Z + 17;
            Entity laptop = Call<Entity>("spawn", "script_model", new Parameter(laptopvec));
            laptop.Call("setmodel", "com_laptop_2_open");
            LaptopRotate(laptop);

            int _curObjID = 31 - curObjID++;
            Call("objective_state", _curObjID, "active");
            Call("objective_position", _curObjID, new Parameter(origin));
            Call("objective_team", _curObjID, "allies");
            Call("objective_icon", _curObjID, "specialty_longersprint_upgrade");

            MakeUsable(crate, "speedy", 50);
        }

        public void usedSpeedy(Entity box, Entity player)
        {
            if (!player.IsAlive) return;
            if (Unitily.GetPlayerTeam(player) == "axis") return;
            else
            {
                if (player.GetField<int>("speedyDone") == 1)
                {
                    player.Call("iprintln", "^1You already have Stamin-Up.");
                    return;
                }
                if (player.GetField<int>("lsd_money") >= 600)
                {
                    player.SetField("lsd_money", player.GetField<int>("lsd_money") - 600);
                    player.SetPerk("specialty_lightweight", true, false);
                    player.SetPerk("specialty_longersprint", true, false);
                    OnInterval(100, () =>
                    {
                        player.Call("setmovespeedscale", new Parameter((float)1.3f));
                        if (!player.IsAlive) return false;
                        return true;
                    });
                    Utilities.RawSayTo(player, "^2Extra speed.");
                    player.Call("playlocalsound", "earn_perk");
                    player.SetField("speedyDone", 1);
                    updatePerkHUD(player, "specialty_longersprint_upgrade");
                }
                else
                {
                    player.Call("iprintln", "^1Stamin-Up need $1500.");
                }
            }
        }

        public void CreateStalker(Vector3 origin, Vector3 angle)
        {
            Call("precacheshader", "specialty_stalker_upgrade");
            Entity crate = Call<Entity>("spawn", "script_model", new Parameter(origin));
            crate.Call("setmodel", "com_plasticcase_friendly");
            crate.SetField("angles", new Parameter(angle));
            crate.Call("clonebrushmodeltoscriptmodel", _airdropCollision);

            CreateBoxShader(crate, "specialty_stalker_upgrade");

            Vector3 laptopvec = origin;
            laptopvec.Z = laptopvec.Z + 17;
            Entity laptop = Call<Entity>("spawn", "script_model", new Parameter(laptopvec));
            laptop.Call("setmodel", "com_laptop_2_open");
            LaptopRotate(laptop);

            int _curObjID = 31 - curObjID++;
            Call("objective_state", _curObjID, "active");
            Call("objective_position", _curObjID, new Parameter(origin));
            Call("objective_team", _curObjID, "allies");
            Call("objective_icon", _curObjID, "specialty_stalker_upgrade");

            MakeUsable(crate, "stalker", 50);
        }

        public void usedStalker(Entity box, Entity player)
        {
            if (!player.IsAlive) return;
            if (Unitily.GetPlayerTeam(player) == "axis") return;
            else
            {
                if (player.GetField<int>("stalkerDone") == 1)
                {
                    player.Call("iprintln", "^1You already have Stalker Soda.");
                    return;
                }
                if (player.GetField<int>("lsd_money") >= 500)
                {
                    player.SetField("lsd_money", player.GetField<int>("lsd_money") - 500);
                    player.SetPerk("specialty_stalker", true, false);
                    player.SetPerk("specialty_delaymine", true, false);
                    Utilities.RawSayTo(player, "^2Extra ADS speed and delay mine timer.");
                    player.Call("playlocalsound", "earn_perk");
                    player.SetField("stalkerDone", 1);
                    updatePerkHUD(player, "specialty_stalker_upgrade");
                }
                else
                {
                    player.Call("iprintln", "^1Stalker Soda need $1000.");
                }
            }
        }

        public void CreateMuleKick(Vector3 origin, Vector3 angle)
        {
            Call("precacheshader", "specialty_twoprimaries_upgrade");
            Entity crate = Call<Entity>("spawn", "script_model", new Parameter(origin));
            crate.Call("setmodel", "com_plasticcase_friendly");
            crate.SetField("angles", new Parameter(angle));
            crate.Call("clonebrushmodeltoscriptmodel", _airdropCollision);

            CreateBoxShader(crate, "specialty_twoprimaries_upgrade");

            Vector3 laptopvec = origin;
            laptopvec.Z = laptopvec.Z + 17;
            Entity laptop = Call<Entity>("spawn", "script_model", new Parameter(laptopvec));
            laptop.Call("setmodel", "com_laptop_2_open");
            LaptopRotate(laptop);

            int _curObjID = 31 - curObjID++;
            Call("objective_state", _curObjID, "active");
            Call("objective_position", _curObjID, new Parameter(origin));
            Call("objective_team", _curObjID, "allies");
            Call("objective_icon", _curObjID, "specialty_twoprimaries_upgrade");

            MakeUsable(crate, "muleKick", 50);
        }

        public void usedMuleKick(Entity box, Entity player)
        {
            if (!player.IsAlive) return;
            if (Unitily.GetPlayerTeam(player) == "axis") return;
            else
            {
                if (player.GetField<int>("mulekickDone") == 1)
                {
                    player.Call("iprintln", "^1You already have Mule Kick.");
                    return;
                }
                if (player.GetField<int>("lsd_money") >= 500)
                {
                    player.SetField("lsd_money", player.GetField<int>("lsd_money") - 500);
                    player.SetPerk("specialty_rof", true, false);
                    player.SetPerk("specialty_fastermelee", true, false);
                    Utilities.RawSayTo(player, "^2Extra fire speed and knife speed.");
                    player.Call("playlocalsound", "earn_perk");
                    player.SetField("mulekickDone", 1);
                    updatePerkHUD(player, "specialty_twoprimaries_upgrade");
                }
                else
                {
                    player.Call("iprintln", "^1Mule Kick need $500.");
                }
            }
        }

        public void CreateDoubleTap(Vector3 origin, Vector3 angle)
        {
            Call("precacheshader", "specialty_moredamage");
            Entity crate = Call<Entity>("spawn", "script_model", new Parameter(origin));
            crate.Call("setmodel", "com_plasticcase_friendly");
            crate.SetField("angles", new Parameter(angle));
            crate.Call("clonebrushmodeltoscriptmodel", _airdropCollision);

            CreateBoxShader(crate, "specialty_moredamage");

            Vector3 laptopvec = origin;
            laptopvec.Z = laptopvec.Z + 17;
            Entity laptop = Call<Entity>("spawn", "script_model", new Parameter(laptopvec));
            laptop.Call("setmodel", "com_laptop_2_open");
            LaptopRotate(laptop);

            int _curObjID = 31 - curObjID++;
            Call("objective_state", _curObjID, "active");
            Call("objective_position", _curObjID, new Parameter(origin));
            Call("objective_team", _curObjID, "allies");
            Call("objective_icon", _curObjID, "specialty_moredamage");

            MakeUsable(crate, "doubleTap", 50);
        }

        public void usedDoubleTap(Entity box, Entity player)
        {
            if (!player.IsAlive) return;
            if (Unitily.GetPlayerTeam(player) == "axis") return;
            else
            {
                if (player.GetField<int>("doubletapDone") == 1)
                {
                    player.Call("iprintln", "^1You already have Double Tap.");
                    return;
                }
                if (player.GetField<int>("lsd_money") >= 500)
                {
                    player.SetField("lsd_money", player.GetField<int>("lsd_money") - 500);
                    player.SetPerk("specialty_moredamage", true, false);
                    Utilities.RawSayTo(player, "^2Double damage for all gun.");
                    player.Call("playlocalsound", "earn_perk");
                    player.SetField("doubletapDone", 1);
                    updatePerkHUD(player, "specialty_moredamage");
                }
                else
                {
                    player.Call("iprintln", "^1Double Tap need $500.");
                }
            }
        }

        private void loadMapEdit(string mapname)
        {
            try
            {
                StreamReader map = new StreamReader("scripts\\maps\\" + mapname + ".txt");
                while (!map.EndOfStream)
                {
                    string line = map.ReadLine();
                    if (line.StartsWith("//") || line.Equals(string.Empty))
                    {
                        continue;
                    }
                    string[] split = line.Split(':');
                    if (split.Length < 1)
                    {
                        continue;
                    }
                    string type = split[0];
                    switch (type)
                    {
                        case "crate":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            spawnCrate(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "ramp":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateRamp(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "elevator":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateElevator(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "HiddenTP":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateHiddenTP(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "door":
                            split = split[1].Split(';');
                            if (split.Length < 7) continue;
                            CreateDoor(parseVec3(split[0]), parseVec3(split[1]), parseVec3(split[2]), int.Parse(split[3]), int.Parse(split[4]), int.Parse(split[5]), int.Parse(split[6]));
                            break;
                        case "wall":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateWall(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "floor":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateFloor(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "model":
                            split = split[1].Split(';');
                            if (split.Length < 3) continue;
                            spawnModel(split[0], parseVec3(split[1]), parseVec3(split[2]));
                            break;
                        case "turret":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateTurret(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "randomWeapon":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateRandomBox(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "upgrade":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateUpgrade(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "ammo":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateAmmoBox(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "gambler":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateGambler(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "speedCola":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateSpeedCola(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "jugger":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateJugger(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "speedy":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateSpeedy(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "stalker":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateStalker(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "muleKick":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateMuleKick(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "doubleTap":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateDoubleTap(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        default:
                            print("Unknown MapEdit Entry {0}... ignoring", type);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                print("error loading mapedit for map {0}: {1}", mapname, e.Message);
            }
        }

        private Vector3 parseVec3(string vec3)
        {
            vec3 = vec3.Replace(" ", string.Empty);
            if (!vec3.StartsWith("(") && !vec3.EndsWith(")")) throw new IOException("Malformed MapEdit File!");
            vec3 = vec3.Replace("(", string.Empty);
            vec3 = vec3.Replace(")", string.Empty);
            String[] split = vec3.Split(',');
            if (split.Length < 3) throw new IOException("Malformed MapEdit File!");
            return new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
        }

        private string getAlliesFlagModel(string mapname)
        {
            switch (mapname)
            {
                case "mp_alpha":
                case "mp_dome":
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_interchange":
                case "mp_lambeth":
                case "mp_radar":
                case "mp_cement":
                case "mp_hillside_ss":
                case "mp_morningwood":
                case "mp_overwatch":
                case "mp_park":
                case "mp_qadeem":
                case "mp_restrepo_ss":
                case "mp_terminal_cls":
                case "mp_roughneck":
                case "mp_boardwalk":
                case "mp_moab":
                case "mp_nola":
                    return "prop_flag_delta";
                case "mp_bootleg":
                case "mp_bravo":
                case "mp_carbon":
                case "mp_mogadishu":
                case "mp_village":
                case "mp_shipbreaker":
                    return "prop_flag_pmc";
                case "mp_paris":
                    return "prop_flag_gign";
                case "mp_plaza2":
                case "mp_seatown":
                case "mp_underground":
                case "mp_aground_ss":
                case "mp_courtyard_ss":
                case "mp_italy":
                case "mp_meteora":
                    return "prop_flag_sas";
            }
            return "";
        }
        private string getAxisFlagModel(string mapname)
        {
            switch (mapname)
            {
                case "mp_alpha":
                case "mp_bootleg":
                case "mp_dome":
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_interchange":
                case "mp_lambeth":
                case "mp_paris":
                case "mp_plaza2":
                case "mp_radar":
                case "mp_underground":
                case "mp_cement":
                case "mp_hillside_ss":
                case "mp_overwatch":
                case "mp_park":
                case "mp_restrepo_ss":
                case "mp_terminal_cls":
                case "mp_roughneck":
                case "mp_boardwalk":
                case "mp_moab":
                case "mp_nola":
                    return "prop_flag_speznas";
                case "mp_bravo":
                case "mp_carbon":
                case "mp_mogadishu":
                case "mp_village":
                case "mp_shipbreaker":
                    return "prop_flag_africa";
                case "mp_seatown":
                case "mp_aground_ss":
                case "mp_courtyard_ss":
                case "mp_meteora":
                case "mp_morningwood":
                case "mp_qadeem":
                case "mp_italy":
                    return "prop_flag_ic";
            }
            return "";
        }
    }
}
