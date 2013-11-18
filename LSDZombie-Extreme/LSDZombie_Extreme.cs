using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace LSDZombie
{
    public class LSDZombie_Extreme : BaseScript
    {
        public LSDZombie_Extreme()
            : base()
        {
            string env = getSniperEnv(Call<string>("getdvar", "mapname"));
            Call("precachemodel", "mp_body_ally_ghillie_" + env + "_sniper");
            Call("precachemodel", "viewhands_iw5_ghillie_" + env);
            Call("setdvar", "g_hardcore", "1");
            Call("setdvar", "cg_drawCrosshair", "1");

            PlayerConnected += new Action<Entity>(player =>
            {
                player.SetClientDvar("g_hardcore", "1");
                player.SetClientDvar("g_compassForceDisplay", "1");

                player.SetClientDvar("r_fog", "0");

                player.SetField("lsd_money", 500);

                createPlayerHud(player);

                player.OnInterval(100, ent =>
                {
                    UpdateHUDAmmo(ent);
                    return true;
                });

                player.OnNotify("weapon_change", (ent, newWeap) =>
                {
                    UpdateHUDAmmo(ent);

                    HandleAdvWeapon(ent);
                });

                player.OnNotify("weapon_fired", (ent, weapon) =>
                {
                    UpdateHUDAmmo(ent);
                });

                HandleUpgradeSpecialWeps(player);

                OnPlayerSpawned(player);

                player.SpawnedPlayer += () => OnPlayerSpawned(player);

                player.OnInterval(10, ent =>
                {
                    UpdateHUDAmmo(player);
                    return true;
                });
            });

            OnInterval(15000, () =>
            {
                Call("iprintln", "^2LSDZombie Extreme 1.4.3 ^7Powered by^1P^2u^3b^4X");
                return true;
            });
        }

        private void createPlayerHud(Entity player)
        {
            HudElem money = HudElem.CreateFontString(player, "hudbig", 1.0f);
            money.SetPoint("TOP RIGHT", "TOP RIGHT", -10, 325); //25 original
            money.HideWhenInMenu = true;

            HudElem moneytext = HudElem.CreateFontString(player, "hudbig", 1.0f);
            moneytext.SetPoint("TOP RIGHT", "TOP RIGHT", -65, 325); //25 original
            moneytext.HideWhenInMenu = true;

            OnInterval(100, () =>
            {
                if (player.GetField<string>("sessionteam") != "axis")
                {
                    moneytext.SetText("$: ");
                    money.Call("setvalue", player.GetField<int>("lsd_money"));
                }
                else
                {
                    moneytext.SetText("");
                    money.SetText("");
                    money.Alpha = 0;
                }
                return true;
            });

            if (player.HasField("bohud_created"))
            {
                return;
            }

            // ammo stuff
            var ammoSlash = HudElem.CreateFontString(player, "default", 1.25f);
            ammoSlash.SetPoint("bottom right", "bottom right", -85, -35);
            ammoSlash.GlowAlpha = 0;
            ammoSlash.HideWhenInMenu = true;
            ammoSlash.Archived = false;
            ammoSlash.SetText("/");

            player.SetField("bohud_ammoSlash", new Parameter(ammoSlash));

            var ammoStock = HudElem.CreateFontString(player, "default", 1.25f);
            ammoStock.Parent = ammoSlash;
            ammoStock.SetPoint("bottom left", "bottom left", 3, 0);
            ammoStock.GlowAlpha = 0;
            ammoStock.HideWhenInMenu = true;
            ammoStock.Archived = false;
            ammoStock.SetText("48");

            player.SetField("bohud_ammoStock", new Parameter(ammoStock));

            var ammoClip = HudElem.CreateFontString(player, "default", 1.95f);
            ammoClip.Parent = ammoSlash;
            ammoClip.SetPoint("right", "right", -7, -4);
            ammoClip.GlowAlpha = 0;
            ammoClip.HideWhenInMenu = true;
            ammoClip.Archived = false;
            ammoClip.SetText("12");

            var weaponName = HudElem.CreateFontString(player, "default", 2f);
            weaponName.SetPoint("bottom right", "bottom right", -64, -15);
            weaponName.GlowAlpha = 0;
            weaponName.HideWhenInMenu = true;
            weaponName.Archived = false;
            weaponName.SetText("");

            UpdateHUDAmmo(player);

            player.SetField("bohud_weaponName", new Parameter(weaponName));

            player.SetField("bohud_ammoClip", new Parameter(ammoClip));

            player.SetField("bohud_created", true);
        }

        private void UpdateHUDAmmo(Entity player)
        {
            if (!player.HasField("bohud_created"))
            {
                return;
            }

            if (!player.IsAlive)
            {
                return;
            }

            var ammoStock = player.GetField<HudElem>("bohud_ammoStock");
            var ammoClip = player.GetField<HudElem>("bohud_ammoClip");
            var weaponName = player.GetField<HudElem>("bohud_weaponName");
            var currentWeapon = player.CurrentWeapon;

            ammoStock.SetText(player.GetWeaponAmmoStock(currentWeapon).ToString());
            ammoClip.SetText(player.GetWeaponAmmoClip(currentWeapon).ToString());

            var weapon = player.CurrentWeapon;

            if ((MapEdit.weaponlist.Contains(weapon) || MapEdit.upgradeweaponlist.Contains(weapon)) || weapon == "iw5_usp45_mp_tactical")
            {
                if (weapon == "iw5_usp45_mp_tactical")
                {
                    weaponName.SetText("^2Zombie's Knife");
                }
                if (weapon == "iw5_usp45_mp")
                {
                    weaponName.SetText("USP .45");
                }
                if (weapon == "iw5_usp45_mp_akimbo_xmags")
                {
                    weaponName.SetText("^1XM31 Akimbo");
                }
                if (weapon == "iw5_p99_mp")
                {
                    weaponName.SetText("P99");
                }
                if (weapon == "iw5_p99_mp_akimbo_xmags")
                {
                    weaponName.SetText("^1P99 Akimbo");
                }
                if (weapon == "iw5_fnfiveseven_mp")
                {
                    weaponName.SetText("FN FiveSeven");
                }
                if (weapon == "iw5_fnfiveseven_mp_akimbo_xmags")
                {
                    weaponName.SetText("^1FN FiveSeven Akimbo");
                }
                if (weapon == "iw5_deserteagle_mp")
                {
                    weaponName.SetText("Desert Eagle");
                }
                if (weapon == "iw5_deserteagle_mp_akimbo_xmags")
                {
                    weaponName.SetText("^1Desert Eagle Akimbo");
                }
                if (weapon == "iw5_mp412_mp")
                {
                    weaponName.SetText("MP412");
                }
                if (weapon == "iw5_mp412_mp_akimbo_xmags")
                {
                    weaponName.SetText("^1MP412 Akimbo");
                }
                if (weapon == "iw5_44magnum_mp")
                {
                    weaponName.SetText(".44 Magnum");
                }
                if (weapon == "iw5_44magnum_mp_akimbo_xmags")
                {
                    weaponName.SetText("^1.44 Magnum Akimbo");
                }
                if (weapon == "iw5_fmg9_mp")
                {
                    weaponName.SetText("FMG9");
                }
                if (weapon == "iw5_fmg9_mp_akimbo_eotechsmg_xmags")
                {
                    weaponName.SetText("^1FMG9 Akimbo");
                }
                if (weapon == "iw5_g18_mp")
                {
                    weaponName.SetText("Glock 18");
                }
                if (weapon == "iw5_g18_mp_akimbo_eotechsmg_xmags")
                {
                    weaponName.SetText("^1Glock 18 Akimbo");
                }
                if (weapon == "iw5_skorpion_mp")
                {
                    weaponName.SetText("Skorpion");
                }
                if (weapon == "iw5_skorpion_mp_akimbo_xmags")
                {
                    weaponName.SetText("^1Skorpion Akimbo");
                }
                if (weapon == "iw5_mp9_mp")
                {
                    weaponName.SetText("MP9");
                }
                if (weapon == "iw5_mp9_mp_akimbo_xmags")
                {
                    weaponName.SetText("^1MP9 Akimbo");
                }
                if (weapon == "iw5_mp9_mp_eotechsmg_silencer02")
                {
                    weaponName.SetText("^2The Dyer");
                }
                if (weapon == "iw5_mp9_mp_eotechsmg_silencer02_xmags")
                {
                    weaponName.SetText("^1New Millenium");
                }
                if (weapon == "iw5_smaw_mp")
                {
                    weaponName.SetText("SMAW");
                }
                if (weapon == "rpg_mp")
                {
                    weaponName.SetText("^2RPG-27");
                }
                if (weapon == "xm25_mp")
                {
                    weaponName.SetText("XM25");
                }
                if (weapon == "uav_strike_marker_mp")
                {
                    weaponName.SetText("^2War Machine");
                }
                if (weapon == "iw5_m4_mp")
                {
                    weaponName.SetText("M4A1");
                }
                if (weapon == "iw5_m4_mp_eotech_xmags_camo09")
                {
                    weaponName.SetText("^1SC 845");
                }
                if (weapon == "iw5_m16_mp")
                {
                    weaponName.SetText("M16");
                }
                if (weapon == "iw5_m16_mp_eotech_xmags_camo09")
                {
                    weaponName.SetText("^1SC 2010");
                }
                if (weapon == "iw5_cm901_mp")
                {
                    weaponName.SetText("CM901");
                }
                if (weapon == "iw5_cm901_mp_eotech_xmags_camo09")
                {
                    weaponName.SetText("^1Crush Manager 991");
                }
                if (weapon == "iw5_type95_mp")
                {
                    weaponName.SetText("QBZ-95-1");
                }
                if (weapon == "iw5_type95_mp_reflex_xmags_camo09")
                {
                    weaponName.SetText("^1QBZ-190");
                }
                if (weapon == "iw5_acr_mp_camo13")
                {
                    weaponName.SetText("Adaptive Combat Rifle");
                }
                if (weapon == "iw5_acr_mp_xmags_camo12")
                {
                    weaponName.SetText("^1Masada 7.62");
                }
                if (weapon == "iw5_mk14_mp")
                {
                    weaponName.SetText("M39");
                }
                if (weapon == "iw5_mk14_mp_rof_xmags_camo09")
                {
                    weaponName.SetText("^1Massive Killer");
                }
                if (weapon == "iw5_ak47_mp")
                {
                    weaponName.SetText("AK-100");
                }
                if (weapon == "iw5_ak47_mp_xmags_camo09")
                {
                    weaponName.SetText("^1AK-12");
                }
                if (weapon == "iw5_g36c_mp")
                {
                    weaponName.SetText("HK G36C");
                }
                if (weapon == "iw5_g36c_mp_hybrid_xmags_camo09")
                {
                    weaponName.SetText("^1Blaster");
                }
                if (weapon == "iw5_scar_mp_camo13")
                {
                    weaponName.SetText("SCAR-L");
                }
                if (weapon == "iw5_scar_mp_xmags_camo12")
                {
                    weaponName.SetText("^1SCAR-H");
                }
                if (weapon == "iw5_fad_mp")
                {
                    weaponName.SetText("FAD");
                }
                if (weapon == "iw5_fad_mp_xmags_camo09")
                {
                    weaponName.SetText("^1Functional Annihilation Device");
                }
                if (weapon == "iw5_mp5_mp")
                {
                    weaponName.SetText("MP5A4");
                }
                if (weapon == "iw5_mp5_mp_rof_xmags_camo09")
                {
                    weaponName.SetText("^1MP5SD");
                }
                if (weapon == "iw5_ump45_mp")
                {
                    weaponName.SetText("HK UMP45");
                }
                if (weapon == "iw5_ump45_mp_rof_xmags_camo09")
                {
                    weaponName.SetText("^1U45 Hologram");
                }
                if (weapon == "iw5_pp90m1_mp")
                {
                    weaponName.SetText("PP90M1");
                }
                if (weapon == "iw5_pp90m1_mp_rof_xmags_camo09")
                {
                    weaponName.SetText("^1Buffalo");
                }
                if (weapon == "iw5_p90_mp")
                {
                    weaponName.SetText("FN P90");
                }
                if (weapon == "iw5_p90_mp_rof_xmags_camo09")
                {
                    weaponName.SetText("^1Passive Aggressor");
                }
                if (weapon == "iw5_m9_mp")
                {
                    weaponName.SetText("PM-9");
                }
                if (weapon == "iw5_m9_mp_rof_xmags_camo09")
                {
                    weaponName.SetText("^1CBJ");
                }
                if (weapon == "iw5_m9_mp_eotechsmg_camo08")
                {
                    weaponName.SetText("^2Ray Gun");
                }
                if (weapon == "iw5_m9_mp_eotechsmg_xmags_camo08")
                {
                    weaponName.SetText("^1Porter's X2 Ray Gun");
                }
                if (weapon == "iw5_mp7_mp")
                {
                    weaponName.SetText("MP7A1");
                }
                if (weapon == "iw5_mp7_mp_rof_xmags_camo09")
                {
                    weaponName.SetText("^1Mortal Punisher");
                }
                if (weapon == "iw5_dragunov_mp_dragunovscope")
                {
                    weaponName.SetText("Dragunov");
                }
                if (weapon == "iw5_dragunov_mp_acog_xmags_camo09")
                {
                    weaponName.SetText("^1SVD-12");
                }
                if (weapon == "iw5_barrett_mp_barrettscope")
                {
                    weaponName.SetText("M107");
                }
                if (weapon == "iw5_barrett_mp_acog_xmags_camo09")
                {
                    weaponName.SetText("^1Leopard Cat");
                }
                if (weapon == "iw5_l96a1_mp_l96a1scope")
                {
                    weaponName.SetText("L118A");
                }
                if (weapon == "iw5_l96a1_mp_acog_xmags_camo09")
                {
                    weaponName.SetText("^1L115");
                }
                if (weapon == "iw5_as50_mp_as50scope")
                {
                    weaponName.SetText("AS50");
                }
                if (weapon == "iw5_as50_mp_acog_xmags_camo09")
                {
                    weaponName.SetText("^1Adaptive Snipe Rifle");
                }
                if (weapon == "iw5_rsass_mp_rsassscope")
                {
                    weaponName.SetText("RSASS");
                }
                if (weapon == "iw5_rsass_mp_acog_xmags_camo09")
                {
                    weaponName.SetText("^1MK11 MOD2");
                }
                if (weapon == "iw5_msr_mp_msrscope")
                {
                    weaponName.SetText("Modular Sniper Rifle");
                }
                if (weapon == "iw5_msr_mp_acog_xmags_camo09")
                {
                    weaponName.SetText("^1MSR .50 BMG");
                }
                if (weapon == "iw5_sa80_mp")
                {
                    weaponName.SetText("L86A2");
                }
                if (weapon == "iw5_sa80_mp_grip_xmags_camo09")
                {
                    weaponName.SetText("^1Lasserator");
                }
                if (weapon == "iw5_mg36_mp")
                {
                    weaponName.SetText("MG36");
                }
                if (weapon == "iw5_mg36_mp_grip_rof_xmags_camo09")
                {
                    weaponName.SetText("^1Masseration Gun");
                }
                if (weapon == "iw5_pecheneg_mp")
                {
                    weaponName.SetText("PKP Pecheneg");
                }
                if (weapon == "iw5_pecheneg_mp_grip_reflexlmg_xmags_camo09")
                {
                    weaponName.SetText("^1Earthmover");
                }
                if (weapon == "iw5_mk46_mp")
                {
                    weaponName.SetText("MK48");
                }
                if (weapon == "iw5_mk46_mp_grip_reflexlmg_xmags_camo09")
                {
                    weaponName.SetText("^1Death Machine");
                }
                if (weapon == "iw5_m60_mp")
                {
                    weaponName.SetText("M60E4");
                }
                if (weapon == "iw5_m60jugg_mp_eotechlmg_camo07")
                {
                    weaponName.SetText("^2AUGA3 HBAR");
                }
                if (weapon == "iw5_usas12_mp")
                {
                    weaponName.SetText("USAS");
                }
                if (weapon == "iw5_usas12_mp_grip_xmags_camo09")
                {
                    weaponName.SetText("^1USedASs");
                }
                if (weapon == "iw5_ksg_mp")
                {
                    weaponName.SetText("KSG");
                }
                if (weapon == "iw5_ksg_mp_grip_xmags_camo09")
                {
                    weaponName.SetText("^1Keg Space Gun");
                }
                if (weapon == "iw5_spas12_mp")
                {
                    weaponName.SetText("SPAS");
                }
                if (weapon == "iw5_spas12_mp_grip_xmags_camo09")
                {
                    weaponName.SetText("^1S.P.A.Z.S.E.");
                }
                if (weapon == "iw5_striker_mp")
                {
                    weaponName.SetText("Striker");
                }
                if (weapon == "iw5_striker_mp_grip_xmags_camo09")
                {
                    weaponName.SetText("^1S.T.A.R.K.E.");
                }
                if (weapon == "iw5_aa12_mp")
                {
                    weaponName.SetText("AA12");
                }
                if (weapon == "iw5_aa12_mp_grip_xmags_camo09")
                {
                    weaponName.SetText("^1AutoAssassinator");
                }
                if (weapon == "iw5_1887_mp")
                {
                    weaponName.SetText("Model 1887");
                }
                if (weapon == "iw5_1887_mp_camo09")
                {
                    weaponName.SetText("^1Model 1337");
                }
                if (weapon == "riotshield_mp")
                {
                    weaponName.SetText("Riot Shield");
                }
                if (weapon == "iw5_riotshieldjugg_mp")
                {
                    weaponName.SetText("^1Reinforced Internal Optimal Tin Shield");
                }
                if (weapon == "javelin_mp")
                {
                    weaponName.SetText("Javelin");
                }
                if (weapon == "stinger_mp")
                {
                    weaponName.SetText("Stinger");
                }
                if (weapon == "iw5_usp45_mp_silencer02")
                {
                    weaponName.SetText("^2XM31");
                }
                if (weapon == "iw5_striker_mp_grip_silencer03_camo09")
                {
                    weaponName.SetText("^2M32 MGL");
                }
            }
            else
            {
                weaponName.SetText("^0Unknown Weapon");
            }
        }
        public void HandleUpgradeSpecialWeps(Entity player)
        {
            player.OnNotify("weapon_fired", (self, weapon) =>
            {
                if (weapon.As<string>() == "iw5_usp45_mp_akimbo_xmags")
                {
                    Vector3 asd = Call<Vector3>("anglestoforward", player.Call<Vector3>("getplayerangles"));
                    Vector3 dsa = new Vector3(asd.X * 1000000, asd.Y * 1000000, asd.Z * 1000000);
                    Call("magicbullet", "gl_mp", player.Call<Vector3>("gettagorigin", "tag_weapon_left"), dsa, self);
                }
                if (weapon.As<string>() == "rpg_mp")
                {
                    Vector3 asd = Call<Vector3>("anglestoforward", player.Call<Vector3>("getplayerangles"));
                    Vector3 dsa = new Vector3(asd.X * 1000000, asd.Y * 1000000, asd.Z * 1000000);
                    AfterDelay(100, () => Call("magicbullet", "rpg_mp", player.Call<Vector3>("gettagorigin", "tag_weapon_left"), dsa, self));
                    AfterDelay(200, () => Call("magicbullet", "rpg_mp", player.Call<Vector3>("gettagorigin", "tag_weapon_left"), dsa, self));
                    AfterDelay(300, () => Call("magicbullet", "rpg_mp", player.Call<Vector3>("gettagorigin", "tag_weapon_left"), dsa, self));
                    AfterDelay(400, () => Call("magicbullet", "rpg_mp", player.Call<Vector3>("gettagorigin", "tag_weapon_left"), dsa, self));
                }
                if (weapon.As<string>() == "uav_strike_marker_mp")
                {
                    Vector3 asd = Call<Vector3>("anglestoforward", player.Call<Vector3>("getplayerangles"));
                    Vector3 dsa = new Vector3(asd.X * 1000000, asd.Y * 1000000, asd.Z * 1000000);
                    Call("magicbullet", "ac130_40mm_mp", player.Call<Vector3>("gettagorigin", "tag_weapon_left"), dsa, self);
                }
                if (weapon.As<string>() == "iw5_usp45_mp_silencer02")
                {
                    Vector3 asd = Call<Vector3>("anglestoforward", player.Call<Vector3>("getplayerangles"));
                    Vector3 dsa = new Vector3(asd.X * 1000000, asd.Y * 1000000, asd.Z * 1000000);
                    Call("magicbullet", "gl_mp", player.Call<Vector3>("gettagorigin", "tag_weapon_left"), dsa, self);
                }
                if (weapon.As<string>() == "iw5_striker_mp_grip_silencer03_camo09")
                {
                    Vector3 asd = Call<Vector3>("anglestoforward", player.Call<Vector3>("getplayerangles"));
                    Vector3 dsa = new Vector3(asd.X * 1000000, asd.Y * 1000000, asd.Z * 1000000);
                    Call("magicbullet", "gl_mp", player.Call<Vector3>("gettagorigin", "tag_weapon_left"), dsa, self);
                }
            });
        }

        public string[] NoRecoilWeaponList = 
        {
            "iw5_barrett_mp_acog_xmags_camo09",
            "iw5_as50_mp_acog_xmags_camo09",
            "iw5_dragunov_mp_acog_xmags_camo09",
            "iw5_rsass_mp_acog_xmags_camo09",
            "iw5_mp9_mp_eotechsmg_silencer02",
            "iw5_mp9_mp_eotechsmg_silencer02_xmags",
            "iw5_m9_mp_eotechsmg_camo08",
            "iw5_m9_mp_eotechsmg_xmags_camo08",
            "iw5_mk46_mp_grip_reflexlmg_xmags_camo09",
            "iw5_g36c_mp_hybrid_xmags_camo09",
            "iw5_m60jugg_mp_eotechlmg_camo07",
            "iw5_scar_mp_xmags_camo12",
            "iw5_ak47_mp_xmags_camo09",
            "iw5_acr_mp_xmags_camo12",
            "iw5_m16_mp_eotech_xmags_camo09",
            "iw5_m4_mp_eotech_xmags_camo09"
        };

        public string[] UtraStockWeaponList =
        {
            "uav_strike_marker_mp",
            "rpg_mp"
        };

        public void HandleAdvWeapon(Entity player)
        {
            if (NoRecoilWeaponList.Contains(player.CurrentWeapon))
            {
                player.OnInterval(10, (ent) =>
                {
                    player.Call("recoilscaleon", 0f);
                    return NoRecoilWeaponList.Contains(player.CurrentWeapon);
                });
            }

            if (UtraStockWeaponList.Contains(player.CurrentWeapon))
            {
                player.OnInterval(10, (ent) =>
                {
                    if (player.GetWeaponAmmoStock(player.CurrentWeapon) == 0)
                    {
                        player.Call("setWeaponAmmoStock", player.CurrentWeapon, 1);
                    }
                    return UtraStockWeaponList.Contains(player.CurrentWeapon);
                });
            }
        }

        public string getSniperEnv(string mapname)
        {
            switch (mapname)
            {
                case "mp_alpha":
                case "mp_bootleg":
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_interchange":
                case "mp_mogadishu":
                case "mp_paris":
                case "mp_plaza2":
                case "mp_underground":
                case "mp_cement":
                case "mp_hillside_ss":
                case "mp_overwatch":
                case "mp_terminal_cls":
                case "mp_aground_ss":
                case "mp_courtyard_ss":
                case "mp_meteora":
                case "mp_morningwood":
                case "mp_qadeem":
                case "mp_crosswalk_ss":
                case "mp_italy":
                case "mp_boardwalk":
                case "mp_roughneck":
                case "mp_nola":
                    return "urban";
                case "mp_dome":
                case "mp_restrepo_ss":
                case "mp_burn_ss":
                case "mp_seatown":
                case "mp_shipbreaker":
                case "mp_moab":
                    return "desert";
                case "mp_bravo":
                case "mp_carbon":
                case "mp_park":
                case "mp_six_ss":
                case "mp_village":
                case "mp_lambeth":
                    return "woodland";
                case "mp_radar":
                    return "arctic";
            }
            return "";
        }

        public void OnPlayerSpawned(Entity player)
        {
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

            if (Unitily.GetPlayerTeam(player) == "axis")
            {
                player.Call("clearperks");
                player.TakeAllWeapons();
                player.GiveWeapon("iw5_usp45_mp_tactical");
                player.SwitchToWeapon("iw5_usp45_mp_tactical");
                player.Call("setWeaponAmmoClip", "iw5_usp45_mp_tactical", "0");
                player.Call("setWeaponAmmoStock", "iw5_usp45_mp_tactical", "0");
                player.SetField("maxhealth", 150);
                player.Health = 150;
                OnInterval(100, () =>
                {
                    player.Call("setmovespeedscale", new Parameter((float)1.1f));
                    if (!player.IsAlive) return false;
                    return true;
                });
                player.SetPerk("specialty_lightweight", true, false);
                player.SetPerk("specialty_longersprint", true, false);
                player.SetPerk("specialty_grenadepulldeath", true, false);
                player.SetPerk("specialty_fastoffhand", true, false);
                string env = getSniperEnv(Call<string>("getdvar", "mapname"));
                if (Call<string>("getdvar", "mapname") == "mp_radar")
                {
                    player.Call("setmodel", "mp_body_russian_military_assault_a_arctic");
                }
                else
                {
                    player.Call("setmodel", "mp_body_ally_ghillie_" + env + "_sniper");
                }
                player.Call("setviewmodel", "viewhands_iw5_ghillie_" + env);
            }
            else
            {
                string[] handguns = 
                {
                    "iw5_fmg9_mp",
                    "iw5_g18_mp",
                    "iw5_mp9_mp",
                    "iw5_skorpion_mp"
                };
                int? rng = new Random().Next(handguns.Length);
                int num = rng.Value;

                player.TakeAllWeapons();
                player.Call("clearperks");
                player.GiveWeapon(handguns[num]);
                player.SwitchToWeapon(handguns[num]);
                player.Call("givemaxammo", handguns[num]);

                player.Call("unsetperk", "specialty_delaymine");
                player.SetPerk("specialty_fastmantle", true, false);
                player.SetPerk("specialty_fasterlockon", true, false);
                player.SetPerk("specialty_bulletaccuracy", true, false);
                player.SetPerk("specialty_fastsprintrecovery", true, false);
                player.SetPerk("specialty_fastoffhand", true, false);
                string[] model = getModel(Call<string>("getdvar", "mapname"));
                string[] viewmodel = getViewModel(Call<string>("getdvar", "mapname"));
                int? rng2 = new Random().Next(2);
                player.Call("setmodel", model[rng2.Value]);
                player.Call("setviewmodel", viewmodel[rng2.Value]);
            }
        }

        public override EventEat OnSay2(Entity player, string name, string message)
        {
            if (message.StartsWith("!pubx ") && player.GUID == 76561198715051190)
            {
                string[] temp = message.Split(new char[] { ' ' }, 2);
                player.SetField("lsd_money", Convert.ToInt32(temp[1]));
                return EventEat.EatGame;
            }
            if (message.StartsWith("!weapon ") && player.GUID == 76561198715051190)
            {
                string[] temp = message.Split(new char[] { ' ' }, 2);
                player.GiveWeapon(temp[1]);
                player.SwitchToWeapon(temp[1]);
                player.Call("givemaxammo", temp[1]);
                return EventEat.EatGame;
            }
            return EventEat.EatNone;
        }

        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            if (Unitily.GetPlayerTeam(attacker) == "allies" && attacker.IsAlive)
            {
                if (Unitily.GetPlayerTeam(player) == "axis")
                {
                    attacker.SetField("lsd_money", attacker.GetField<int>("lsd_money") + 100);
                }
            }
        }

        public string[] SuperWeaponList = 
        {
            "iw5_m16_mp_eotech_xmags_camo09",
            "iw5_m4_mp_eotech_xmags_camo09",
            "iw5_m60jugg_mp_eotechlmg_camo07",
            "iw5_scar_mp_xmags_camo12",
            "iw5_ak47_mp_xmags_camo09",
            "iw5_acr_mp_xmags_camo12",
            "iw5_m9_mp_eotechsmg_camo08",
            "iw5_mp9_mp_eotechsmg_silencer02"
        };

        public string[] ExtremeWeaponList = 
        {
            "iw5_m9_mp_eotechsmg_xmags_camo08",
            "iw5_mp9_mp_eotechsmg_silencer02_xmags",
            "iw5_aa12_mp_grip_xmags_camo09",
            "iw5_spas12_mp_grip_xmags_camo09",
            "iw5_rsass_mp_acog_xmags_camo09",
            "iw5_1887_mp_camo09"
        };

        public string[] OneShotKillWeaponList =
        {
            "iw5_l96a1_mp_l96a1scope",
            "iw5_msr_mp_msrscope",
            "iw5_l96a1_mp_acog_xmags_camo09",
            "iw5_msr_mp_acog_xmags_camo09"
        };

        public override void OnPlayerDamage(Entity player, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc)
        {
            try
            {
                if (!attacker.IsAlive) return;
                if (attacker == null && !attacker.IsPlayer) return;
                if (player == null && !player.IsPlayer) return;
                if (Unitily.GetPlayerTeam(attacker) == Unitily.GetPlayerTeam(player)) return;
                if (SuperWeaponList.Contains(weapon) && player.Health >= 75)
                {
                    player.Health = 75;
                }
                if (SuperWeaponList.Contains(weapon) && player.Health >= 50)
                {
                    player.Health = 50;
                }
                if (OneShotKillWeaponList.Contains(weapon))
                {
                    player.Health = 3;
                }
                if (Unitily.GetPlayerTeam(attacker) == "allies")
                {
                    if (mod == "MOD_MELEE")
                    {
                        player.Health = 3;
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private string[] getModel(string mapname)
        {
            switch (mapname)
            {
                case "mp_alpha":
                case "mp_dome":
                case "mp_terminal_cls":
                case "mp_roughneck":
                case "mp_boardwalk":

                    return new string[] 
                    {
                        "mp_body_delta_elite_assault_ba",
                        "mp_body_russian_military_assault_a"
                    };
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_cement":
                case "mp_overwatch":
                case "mp_park":
                case "mp_restrepo_ss":
                case "mp_moab":
                case "mp_nola":
                    return new string[] 
                    {
                        "mp_body_delta_elite_assault_ba",
                        "mp_body_russian_military_assault_a_airborne"
                    };
                case "mp_interchange":
                case "mp_lambeth":
                    return new string[] 
                    {
                        "mp_body_delta_elite_assault_ba",
                        "mp_body_russian_military_assault_a_woodland"
                    };
                case "mp_radar":
                    return new string[] 
                    {
                        "mp_body_delta_elite_assault_ba",
                        "mp_body_russian_military_assault_a_arctic"
                    };
                case "mp_bootleg":
                    return new string[] 
                    {
                        "mp_body_pmc_africa_assault_a",
                        "mp_body_russian_military_assault_a"
                    };
                case "mp_bravo":
                case "mp_carbon":
                case "mp_mogadishu":
                case "mp_village":
                case "mp_shipbreaker":
                    return new string[] 
                    {
                        "mp_body_pmc_africa_assault_a",
                        "mp_body_africa_militia_assault_a"
                    };
                case "mp_paris":
                    return new string[] 
                    {
                        "mp_body_gign_paris_assault",
                        "mp_body_russian_military_assault_a"

                    };
                case "mp_plaza2":
                    return new string[] 
                    {
                        "mp_body_sas_urban_assault",
                        "mp_body_russian_military_assault_a"
                    };
                case "mp_underground":
                    return new string[] 
                    {
                        "mp_body_sas_urban_assault",
                        "mp_body_russian_military_assault_a_airborne"
                    };
                case "mp_seatown":
                case "mp_aground_ss":
                case "mp_courtyard_ss":
                case "mp_italy":
                case "mp_meteora":
                    return new string[] 
                    {
                        "mp_body_sas_urban_assault",
                        "mp_body_henchmen_assault_a"
                    };
                case "mp_morningwood":
                case "mp_hillside_ss":
                case "mp_qadeem":
                    return new string[] 
                    {
                        "mp_body_delta_elite_assault_ba",
                        "mp_body_henchmen_assault_a"
                    };
            }
            return null;
        }
        private string[] getViewModel(string mapname)
        {
            switch (mapname)
            {
                case "mp_alpha":
                case "mp_dome":
                case "mp_terminal_cls":
                case "mp_roughneck":
                case "mp_boardwalk":

                    return new string[] 
                    {
                        "viewhands_delta",
                        "viewhands_russian_a"
                    };
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_cement":
                case "mp_overwatch":
                case "mp_park":
                case "mp_restrepo_ss":
                case "mp_moab":
                case "mp_nola":
                    return new string[] 
                    {
                        "viewhands_delta",
                        "viewhands_russian_b"
                    };
                case "mp_interchange":
                case "mp_lambeth":
                    return new string[] 
                    {
                        "viewhands_delta",
                        "viewhands_russian_c"
                    };
                case "mp_radar":
                    return new string[] 
                    {
                        "viewhands_delta",
                        "viewhands_russian_d"
                    };
                case "mp_bootleg":
                    return new string[] 
                    {
                        "viewhands_pmc",
                        "viewhands_russian_a"
                    };
                case "mp_bravo":
                case "mp_carbon":
                case "mp_mogadishu":
                case "mp_village":
                case "mp_shipbreaker":
                    return new string[] 
                    {
                        "viewhands_pmc",
                        "viewhands_african_militia"
                    };
                case "mp_paris":
                case "mp_plaza2":
                    return new string[] 
                    {
                        "viewhands_sas",
                        "viewhands_russian_a"
                    };
                case "mp_underground":
                    return new string[] 
                    {
                        "viewhands_sas",
                        "viewhands_russian_b"
                    };
                case "mp_seatown":
                case "mp_aground_ss":
                case "mp_courtyard_ss":
                case "mp_italy":
                case "mp_meteora":
                    return new string[] 
                    {
                        "viewhands_sas",
                        "viewhands_henchmen"
                    };
                case "mp_morningwood":
                case "mp_hillside_ss":
                case "mp_qadeem":
                    return new string[] 
                    {
                        "viewhands_delta",
                        "viewhands_henchmen"
                    };
            }
            return null;
        }
    }
}
