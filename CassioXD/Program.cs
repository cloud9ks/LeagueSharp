using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace CassioXD
{
    class Program
    {
        public const string ChampionName  = "Cassiopeia";
        public static Menu Option;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Hero Player;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        private static long LastQCast = 0;
        private static long LastECast = 0;
        public static List<Obj_AI_Hero> Targets = new List<Obj_AI_Hero>();
        public static Obj_AI_Hero MainTarget;
        public static TargetingMode TMode = TargetingMode.FastKill;
        public static AimMode AMode = AimMode.Normal;
        public static LaneClearMode LMode = LaneClearMode.Normal;
        public static HitChance Chance = HitChance.VeryHigh;
        public static bool listed = true;
        public static bool Nopsntarget = true;
        public static bool aastatus;



        private static string[] p1 = new string[] { "Alistar", "Amumu", "Bard", "Blitzcrank", "Braum", "Cho'Gath", "Dr. Mundo", "Garen", "Gnar",
                "Hecarim", "Janna", "Jarvan IV", "Leona", "Lulu", "Malphite", "Nami", "Nasus", "Nautilus", "Nunu",
                "Olaf", "Rammus", "Renekton", "Sejuani", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "Sona",
                "Soraka", "Taric", "Thresh", "Volibear", "Warwick", "MonkeyKing", "Yorick", "Zac", "Zyra" };

        private static string[] p2 = new string[] { "Aatrox", "Darius", "Elise", "Evelynn", "Galio", "Gangplank", "Gragas", "Irelia", "Jax",
                "Lee Sin", "Maokai", "Morgana", "Nocturne", "Pantheon", "Poppy", "Rengar", "Rumble", "Ryze", "Swain",
                "Trundle", "Tryndamere", "Udyr", "Urgot", "Vi", "XinZhao" };

        private static string[] p3 = new string[] { "Akali", "Diana", "Fiddlesticks", "Fiora", "Fizz", "Heimerdinger", "Jayce", "Kassadin",
                "Kayle", "Kha'Zix", "Lissandra", "Mordekaiser", "Nidalee", "Riven", "Shaco", "Vladimir", "Yasuo",
                "Zilean" };

        private static string[] p4 = new string[] { "Ahri", "Anivia", "Annie", "Ashe", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                "Ezreal", "Graves", "Jinx", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "LeBlanc", "Lucian",
                "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon", "Teemo",
                "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath", "Zed",
                "Ziggs" };

        public enum TargetingMode
        {
            FastKill = 1,
            AutoPriority = 0
        }

        public enum AimMode
        {
            Normal = 1,
            HitChance = 0
        }

        public enum LaneClearMode
        {
            Normal = 1,
            Logical = 0
        }


        static void Main(string[] args)
        {
            LeagueSharp.Common.CustomEvents.Game.OnGameLoad += onGameLoad;
        }


        /*
        private static float GetManaLeft(Obj_AI_Hero enemy)
        {
            return 0;
        }

        private static float GetHpLeft(Obj_AI_Hero enemy)
        {
            int Ecount;

            if (enemy.IsValid && enemy != null)
            {
                if (enemy.HasBuffOfType(BuffType.Poison))
                    Ecount = Convert.ToInt32(GetPoisonBuffEndTime(enemy)/0.5);
            }
            return 0;
        }*/


#region Targetlist
        private static double FindPrioForTarget(Obj_AI_Hero enemy, TargetingMode TMode)
        {
            switch (TMode)
            {
                case TargetingMode.AutoPriority:
                    {
                        if (p1.Contains(enemy.BaseSkinName))
                        {
                            return 4;
                        }
                        else if (p2.Contains(enemy.BaseSkinName))
                        {
                            return 3;
                        }
                        else if (p3.Contains(enemy.BaseSkinName))
                        {
                            return 2;
                        }
                        else if (p4.Contains(enemy.BaseSkinName))
                        {
                            return 1;
                        }
                        else
                        {
                            return 5;
                        }
                    }
                case TargetingMode.FastKill:
                    {
                        if (enemy.IsValid && enemy != null && enemy.IsVisible && !enemy.IsDead)
                            return (enemy.Health / Player.GetSpellDamage(enemy, SpellSlot.E));
                        else
                            return 1000000;
                    }
                default:
                    return 0;
            }
        }

        private static void Targetlist(TargetingMode TMode)
        {
            int i1, i2;
            Obj_AI_Hero Buf;
            {
                if (Targets.Count != ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Team != ObjectManager.Player.Team).Count())
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Team != ObjectManager.Player.Team))
                {
                    Targets.Add(enemy);
                }
                

                for (i1 = 0; i1 < Targets.Count; i1++)
                    for (i2 = 0; i2 < Targets.Count; i2++)
                        if (FindPrioForTarget(Targets[i1], TMode) > FindPrioForTarget(Targets[i2], TMode))
                        {
                            Buf = Targets[i2];
                            Targets[i1] = Targets[i1];
                            Targets[i2] = Buf;
                        }
                        else if (FindPrioForTarget(Targets[i1], TMode) < FindPrioForTarget(Targets[i2], TMode))
                        {
                            Buf = Targets[i1];
                            Targets[i1] = Targets[i2];
                            Targets[i2] = Buf;
                        }
            }
        }

        private static float GetPoisonBuffEndTime(Obj_AI_Base target)
        {
            var buffEndTime = target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Type == BuffType.Poison)
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
            return buffEndTime;
        }

#endregion

#region Targetselect

        private static Obj_AI_Hero GetQTarget()
        {
            if (MainTarget == null || MainTarget.IsDead || !MainTarget.IsVisible)
                foreach (var target in Targets)
                {
                    if (target != null && target.IsVisible && !target.IsDead)
                    {
                        if (!target.HasBuffOfType(BuffType.Poison) || GetPoisonBuffEndTime(target) < (Game.Time + Q.Delay))
                        {
                            if (Player.ServerPosition.Distance(Q.GetPrediction(target, true).CastPosition) < Q.Range)
                            {
                                return target;
                            }
                        }
                    }
                }
            else
                return MainTarget;
            return null;
        }

        private static Obj_AI_Hero GetWTarget()
        {
            if (MainTarget == null || MainTarget.IsDead || !MainTarget.IsVisible)
                foreach (var target in Targets)
                {
                    if (target != null && target.IsVisible && !target.IsDead)
                    {
                        if (!target.HasBuffOfType(BuffType.Poison) || (Player.ServerPosition.Distance(Q.GetPrediction(target, true).CastPosition) > Q.Range))
                        {
                            if (Player.ServerPosition.Distance(W.GetPrediction(target, true).CastPosition) < W.Range)
                            {
                                return target;
                            }
                        }
                    }
                }
            else
                return MainTarget;
            return null;
        }

        private static Obj_AI_Hero GetETarget()
        {
            if (MainTarget == null || MainTarget.IsDead || !MainTarget.IsVisible)
                foreach (var target in Targets)
                {
                    if (target != null && target.IsVisible && !target.IsDead)
                    {
                        if ((target.HasBuffOfType(BuffType.Poison) && GetPoisonBuffEndTime(target) > (Game.Time + E.Delay)) || Player.GetSpellDamage(target, SpellSlot.E) > target.Health)
                        {
                            if (target.IsValidTarget(E.Range))
                            {
                                return target;
                            }
                        }
                    }
                }
            else
                return MainTarget;
            return null;

        }

        private static Obj_AI_Hero GetRFaceTarget()
        {
            var FaceEnemy = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget() && enemy.IsFacing(Player) && R.WillHit(enemy, R.GetPrediction(enemy, true).CastPosition)).ToList();
            foreach (var target in Targets)
            {
                if (target != null && target.IsVisible && !target.IsDead)
                    foreach (var fenemy in FaceEnemy)
                        if (fenemy != null && fenemy.IsVisible && !fenemy.IsDead)
                        {
                            if (target.BaseSkinName == fenemy.BaseSkinName)
                                return fenemy;
                        }
            }
            return null;

        }

        private static Obj_AI_Hero GetRTarget()
        {
            var Enemy = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget() && R.WillHit(enemy, R.GetPrediction(enemy, true).CastPosition)).ToList();

            foreach (var target in Targets)
            {
                if (target != null && target.IsVisible && !target.IsDead)
                foreach (var enemy in Enemy)
                    if (enemy != null && enemy.IsVisible && !enemy.IsDead)
                {
                    if (target.BaseSkinName == enemy.BaseSkinName)
                        return enemy;
                }
            }
            return null;
        }

#endregion

#region onGameLoad
        private static void onGameLoad(EventArgs args)
        {
            try
            {
                Player = ObjectManager.Player;
                if (Player.BaseSkinName != ChampionName) return;

                Game.PrintChat("CassioXD");

                Game.OnUpdate += OnTick;
                Drawing.OnDraw += OnDraw;
                Spellbook.OnCastSpell += Spellbook_OnCastSpell;
                Game.OnWndProc += Game_OnWndProc;

                Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

                Targetlist(TargetingMode.AutoPriority);

                Q = new Spell(SpellSlot.Q, 850);
                Q.SetSkillshot(0.6f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);

                W = new Spell(SpellSlot.W, 850);
                W.SetSkillshot(0.5f, 90f, 2500, false, SkillshotType.SkillshotCircle);

                E = new Spell(SpellSlot.E, 700);
                E.SetTargetted(0.2f, float.MaxValue);

                R = new Spell(SpellSlot.R, 800);
                R.SetSkillshot(0.3f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);

                SpellList.Add(Q);
                SpellList.Add(W);
                SpellList.Add(E);
                SpellList.Add(R);

                Option = new Menu("CassioXD", "CassioXD", true);

                Option.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
                Orbwalker = new Orbwalking.Orbwalker(Option.SubMenu("Orbwalking"));

                Option.AddSubMenu(new Menu("Zucht", "Zucht"));
                Option.SubMenu("Zucht").AddItem(new MenuItem("TargetingMode", "Target Mode").SetValue(new StringList(Enum.GetNames(typeof(TargetingMode)))));
                Option.SubMenu("Zucht").AddItem(new MenuItem("AimMode", "Aim Mode").SetValue(new StringList(Enum.GetNames(typeof(AimMode)))));
                Option.SubMenu("Zucht").AddItem(new MenuItem("Hitchance", "Hitchance Mode").SetValue(new StringList(Enum.GetNames(typeof(HitChance)))));
                //Option.SubMenu("Zucht").AddItem(new MenuItem("LaneMode", "Lane Clear Mode").SetValue(new StringList(Enum.GetNames(typeof(LaneClearMode)))));
                Option.SubMenu("Zucht").AddItem(new MenuItem("Edelay", "Ecombo delay").SetValue(new Slider(0, 0, 5)));
                Option.SubMenu("Zucht").AddItem(new MenuItem("Qlaneclear", "Q Lane Clear").SetValue(true));
                Option.SubMenu("Zucht").AddItem(new MenuItem("Wlaneclear", "W Lane Clear").SetValue(true));
                Option.SubMenu("Zucht").AddItem(new MenuItem("Elasthit", "E Lasthit no psn").SetValue(true));
                Option.SubMenu("Zucht").AddItem(new MenuItem("LaneClearMana", "Lane Clear Mana").SetValue(new Slider(70, 0, 100)));
                Option.SubMenu("Zucht").AddItem(new MenuItem("BlockR", "BlockR").SetValue(true));
                Option.SubMenu("Zucht").AddItem(new MenuItem("AssistedUltKey", "Assisted Ult Key").SetValue((new KeyBind("R".ToCharArray()[0], KeyBindType.Press))));
                Option.SubMenu("Zucht").AddItem(new MenuItem("DrawQ", "DrawQ").SetValue(true));
                Option.AddToMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
#endregion

#region OnTick
        private static void OnTick(EventArgs args)
        {

            try
            {
                var menuItem = Option.Item("TargetingMode").GetValue<StringList>();
                Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out TMode);
                
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        JungleClear();
                        WaveClear();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Freeze();
                        break;
                    default:
                        break;
                }
                switch (TMode)
                {
                    case TargetingMode.AutoPriority:
                        if (listed == false)
                        Targetlist(TargetingMode.AutoPriority);
                        listed = true;
                        break;
                    case TargetingMode.FastKill:
                        Targetlist(TargetingMode.FastKill);
                        listed = false;
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
#endregion

#region BeforeAttack
        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if ((Player.Mana < E.Instance.ManaCost) || (E.Instance.Level == 0) || ((E.Instance.CooldownExpires - Game.ClockTime) > 0.7) || Player.HasBuffOfType(BuffType.Silence))
                {
                    args.Process = true;
                    aastatus = true;
                }
                else
                {
                    args.Process = false;
                    aastatus = false;
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var LaneClearMana = Option.Item("LaneClearMana").GetValue<Slider>().Value;
                if ((Player.ManaPercentage() < LaneClearMana) || (E.Instance.Level == 0) || ((E.Instance.CooldownExpires - Game.ClockTime) > 0.7) || Nopsntarget || Player.HasBuffOfType(BuffType.Silence))
                {
                    args.Process = true;
                    aastatus = true;
                }
                else
                {
                    args.Process = false;
                    aastatus = false;
                }
            }
        }
#endregion

#region Combo

        public static void Combo()
        {
            var menuItem3 = Option.Item("AimMode").GetValue<StringList>();
            Enum.TryParse(menuItem3.SList[menuItem3.SelectedIndex], out AMode);

            var menuItem2 = Option.Item("Hitchance").GetValue<StringList>();
            Enum.TryParse(menuItem2.SList[menuItem2.SelectedIndex], out Chance);
            var EDelay = Option.Item("Edelay").GetValue<Slider>().Value;

            if (E.IsReady() && GetETarget() != null)
            {
                if (Environment.TickCount >= LastECast + (EDelay * 100))
                E.Cast(GetETarget());
            }

            if (Q.IsReady() && (Player.ServerPosition.Distance(Q.GetPrediction(GetQTarget(), true).CastPosition) < Q.Range))
            {
                switch (AMode)
                { 
                    case AimMode.HitChance:
                        Q.CastIfHitchanceEquals(GetQTarget(), Chance, false);
                        break;
                    case AimMode.Normal:
                        Q.Cast(GetQTarget(), false, true);
                        break;
                }
            }
            if (W.IsReady() && (Player.ServerPosition.Distance(W.GetPrediction(GetWTarget(), true).CastPosition) < W.Range) && Environment.TickCount > LastQCast + Q.Delay * 1000)
            {
                switch (AMode)
                {
                    case AimMode.HitChance:
                        W.CastIfHitchanceEquals(GetWTarget(), Chance, false);
                        break;
                    case AimMode.Normal:
                        W.Cast(GetWTarget(), false, true);
                        break;
                }
            }

        }
#endregion

#region Harras

        public static void Harass()
        {
            var menuItem3 = Option.Item("AimMode").GetValue<StringList>();
            Enum.TryParse(menuItem3.SList[menuItem3.SelectedIndex], out AMode);

            var menuItem2 = Option.Item("Hitchance").GetValue<StringList>();
            Enum.TryParse(menuItem2.SList[menuItem2.SelectedIndex], out Chance);

            if (E.IsReady() && GetETarget() != null)
            {
                E.Cast(GetETarget());
            }
            if (Q.IsReady() && (Player.ServerPosition.Distance(Q.GetPrediction(GetQTarget(), true).CastPosition) < Q.Range))
            {
                switch (AMode)
                {
                    case AimMode.HitChance:
                        Q.CastIfHitchanceEquals(GetQTarget(), Chance, false);
                        break;
                    case AimMode.Normal:
                        Q.Cast(GetQTarget(), false, true);
                        break;
                }
            }
        }

#endregion

#region Jungle

        public static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (!mobs.Any())
                return;

            var mob = mobs.First();

            if (Q.IsReady() && mob.IsValidTarget(Q.Range))
            {
                Q.Cast(mob.ServerPosition);
            }

            if (E.IsReady() && mob.HasBuffOfType(BuffType.Poison) && mob.IsValidTarget(E.Range))
            {
                E.Cast(mob);
            }

            if (W.IsReady() && mob.IsValidTarget(W.Range))
            {
                W.Cast(mob.ServerPosition);
            }

        }

#endregion

#region Farm
        public static void WaveClear()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All, MinionTeam.Enemy).ToList();
            var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range + W.Width, MinionTypes.All, MinionTeam.Enemy).ToList();
            var allMinionsQnopsn = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All, MinionTeam.Enemy).Where(x => !x.HasBuffOfType(BuffType.Poison) || GetPoisonBuffEndTime(x) <= (Game.Time + Q.Delay)).ToList();
            var rangedMinionsQnopsn = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.Ranged, MinionTeam.Enemy).Where(x => !x.HasBuffOfType(BuffType.Poison) || GetPoisonBuffEndTime(x) <= (Game.Time + Q.Delay)).ToList();
            var allMinionsWnopsn = MinionManager.GetMinions(Player.ServerPosition, W.Range + W.Width, MinionTypes.All, MinionTeam.Enemy).Where(x => !x.HasBuffOfType(BuffType.Poison) || GetPoisonBuffEndTime(x) <= (Game.Time + W.Delay)).ToList();
            var rangedMinionsWnopsn = MinionManager.GetMinions(Player.ServerPosition, W.Range + W.Width, MinionTypes.Ranged, MinionTeam.Enemy).Where(x => !x.HasBuffOfType(BuffType.Poison) || GetPoisonBuffEndTime(x) <= (Game.Time + W.Delay)).ToList();

            var Qlaneclear = Option.Item("Qlaneclear").GetValue<bool>();
            var Wlaneclear = Option.Item("Wlaneclear").GetValue<bool>();
            var LaneClearMana = Option.Item("LaneClearMana").GetValue<Slider>().Value;

            if (allMinionsQnopsn.Count() == allMinionsQ.Count())
                Nopsntarget = true;
            else
                Nopsntarget = false;

            if (Q.IsReady() && allMinionsQnopsn.Count() == allMinionsQ.Count() && Qlaneclear)
            {
                var FLr = Q.GetCircularFarmLocation(rangedMinionsQnopsn, Q.Width);
                var FLa = Q.GetCircularFarmLocation(allMinionsQnopsn, Q.Width);

                if (FLr.MinionsHit >= 3 && Player.Distance(FLr.Position) < (Q.Range + Q.Width))
                {
                    Q.Cast(FLr.Position);
                    return;
                }
                else
                    if (FLa.MinionsHit >= 2 || allMinionsQnopsn.Count() == 1 && Player.Distance(FLr.Position) < (Q.Range + Q.Width))
                    {
                        Q.Cast(FLa.Position);
                        return;
                    }
            }

            if (W.IsReady() && allMinionsWnopsn.Count() == allMinionsW.Count() && Wlaneclear && Environment.TickCount > (LastQCast + Q.Delay * 1000))
            {
                var FLr = W.GetCircularFarmLocation(rangedMinionsWnopsn, W.Width);
                var FLa = W.GetCircularFarmLocation(allMinionsWnopsn, W.Width);

                if (FLr.MinionsHit >= 3 && Player.Distance(FLr.Position) < (W.Range + W.Width))
                {
                    W.Cast(FLr.Position);
                    return;
                }
                else
                    if (FLa.MinionsHit >= 2 || allMinionsWnopsn.Count() == 1 && Player.Distance(FLr.Position) < (W.Range + W.Width))
                    {
                        W.Cast(FLa.Position);
                        return;
                    }
            }

            if (E.IsReady())
            {
                var MinionList = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);

                foreach (var minion in MinionList.Where(x => x.HasBuffOfType(BuffType.Poison)))
                {
                    var buffEndTime = GetPoisonBuffEndTime(minion);
                    if (buffEndTime > Game.Time + E.Delay)
                    {
                        if (Player.GetSpellDamage(minion, SpellSlot.E) > minion.Health || Player.ManaPercentage() > LaneClearMana)
                        {
                            E.Cast(minion);
                        }
                    }
                }
            }

        }

#endregion

        public static void LaneFreeze()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allEnemyMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All, MinionTeam.Enemy).Where(x => !x.HasBuffOfType(BuffType.Poison)).ToList();


        }

#region Lasthit

        public static void Freeze()
        {
            var elasthit = Option.Item("Elasthit").GetValue<bool>();
            if (!Orbwalking.CanMove(40)) return;

            if (E.IsReady())
            {
                var MinionListQ = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                var MinionListE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                /*
                foreach (var minion in MinionListQ.Where(x => x.Health < (Player.GetSpellDamage(x, SpellSlot.Q)/3)))
                {
                    if (Q.IsReady() && Player.Distance(minion,true) > Player.AttackRange)
                        Q.Cast(minion);
                }*/

                foreach (var minion in MinionListE.Where(x => Player.GetSpellDamage(x, SpellSlot.E) > x.Health))
                {
                    if ((minion.HasBuffOfType(BuffType.Poison) && (GetPoisonBuffEndTime(minion) > Game.Time + E.Delay)) || elasthit)
                    {
                        E.Cast(minion);
                    }

                }
            }

        }

#endregion

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (MenuGUI.IsChatOpen)
                return;

            var AssistedUltKey = Option.Item("AssistedUltKey").GetValue<KeyBind>().Key;

            if (args.WParam == AssistedUltKey)
            {
                args.Process = false;
                CastAssistedUlt();
            }

            if (args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN)
            {

            MainTarget =
                HeroManager.Enemies
                    .FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 40000) // 200 * 200
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();;
            }

        }
#region Ultimate
        public static void CastAssistedUlt()
        { 
            var faceEnemy = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget() && enemy.IsFacing(Player) && R.WillHit(enemy, R.GetPrediction(enemy, true).CastPosition)).ToList();
            var Enemy = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget() && R.WillHit(enemy, R.GetPrediction(enemy, true).CastPosition)).ToList();
            
            if (faceEnemy.Count() >= 1 && GetRFaceTarget() != null)
            {
                R.Cast(R.GetPrediction(GetRFaceTarget(), true).CastPosition);
            }
            else
                if (Enemy.Count >= 1 && GetRTarget() != null)
                {
                    R.Cast(R.GetPrediction(GetRTarget(), true).CastPosition);
                }
        }

        public static Tuple<int, List<Obj_AI_Hero>> GetHits(Spell spell)
        {
            var GetenemysHit = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget() && spell.WillHit(enemy, R.GetPrediction(enemy, true).CastPosition)).ToList();

            return new Tuple<int, List<Obj_AI_Hero>>(GetenemysHit.Count, GetenemysHit);
        }
#endregion

        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            var BlockR = Option.Item("BlockR").GetValue<bool>();

            if (args.Slot == SpellSlot.R && GetHits(R).Item1 == 0 && BlockR)
                    args.Process = false;
            if (args.Slot == SpellSlot.Q)
                LastQCast = Environment.TickCount;
            if (args.Slot == SpellSlot.E)
                LastECast = Environment.TickCount;
        }

#region Draw

        private static void OnDraw(EventArgs args)
        {
            var DrawQ = Option.Item("DrawQ").GetValue<bool>();
            try
            {
                if (MainTarget != null && MainTarget.IsVisible)
                    Render.Circle.DrawCircle(MainTarget.Position, 100, System.Drawing.Color.Red);

                if (DrawQ)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Khaki);
            }
            catch (Exception ex)
            {
                Game.PrintChat(ex.ToString());
            }
        }
#endregion

    }
}
