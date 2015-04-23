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
        public static Menu Einstellung;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Hero Player;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        private static long dtLastQCast = 0;
        public static List<Obj_AI_Hero> Targets = new List<Obj_AI_Hero>();
        public static TargetingMode TMode = TargetingMode.FastKill;
        public static AimMode AMode = AimMode.Normal;
        public static HitChance Chance = HitChance.VeryHigh;
        public static bool listed = true;
        public static bool aastatus;
        public static int kills = 0;
        public static Random rand = new Random();

        public static List<string> Messages;



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

        static void setupMessages()
        {
            Messages = new List<string>
            {
                "gj", "good job", "very gj", "very good job",
                "wp", "well played",
                "nicely played",
                "amazing",
                "nice", "nice1", "nice one",
                "well done",
                "sweet",                
            };

        }

        static string getRandomElement(List<string> collection, bool firstEmpty = true)
        {
            if (firstEmpty && rand.Next(3) == 0)
                return collection[0];

            return collection[rand.Next(collection.Count)];
        }

        static string generateMessage()
        {
            string message = getRandomElement(Messages, false);
            return message;
        }


        static void Main(string[] args)
        {
            LeagueSharp.Common.CustomEvents.Game.OnGameLoad += onGameLoad;
            setupMessages();
        }

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

        private static Obj_AI_Hero GetQTarget()
        {

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
            return null;
        }

        private static Obj_AI_Hero GetWTarget()
        {

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
            return null;
        }

        private static Obj_AI_Hero GetETarget()
        {
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
            return null;

        }


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

                Einstellung = new Menu("CassioXD", "CassioXD", true);

                Einstellung.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
                Orbwalker = new Orbwalking.Orbwalker(Einstellung.SubMenu("Orbwalking"));

                Einstellung.AddSubMenu(new Menu("Zucht", "Zucht"));
                Einstellung.SubMenu("Zucht").AddItem(new MenuItem("TargetingMode", "Target Mode").SetValue(new StringList(Enum.GetNames(typeof(TargetingMode)))));
                Einstellung.SubMenu("Zucht").AddItem(new MenuItem("AimMode", "Aim Mode").SetValue(new StringList(Enum.GetNames(typeof(AimMode)))));
                Einstellung.SubMenu("Zucht").AddItem(new MenuItem("Hitchance", "Hitchance Mode").SetValue(new StringList(Enum.GetNames(typeof(HitChance)))));
                Einstellung.SubMenu("Zucht").AddItem(new MenuItem("Fun", "Fun").SetValue(true));
                Einstellung.SubMenu("Zucht").AddItem(new MenuItem("DrawList", "DrawList").SetValue(true));
                Einstellung.SubMenu("Zucht").AddItem(new MenuItem("DrawPrediction", "DrawPrediction").SetValue(true));
                Einstellung.AddToMainMenu();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private static void OnTick(EventArgs args)
        {

            try
            {
                var menuItem = Einstellung.Item("TargetingMode").GetValue<StringList>();
                Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out TMode);

                var Fun = Einstellung.Item("Fun").GetValue<bool>();
                
                foreach  (var enemy in Targets)
                    if (enemy.IsValid && enemy.IsDead && Player.ChampionsKilled > kills && Fun)
                    {
                        kills = Player.ChampionsKilled;
                        Game.Say("/all " + generateMessage() + " " + enemy.Name);
                    }

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
        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if ((Player.Mana < E.Instance.ManaCost) || (E.Instance.Level == 0) || ((E.Instance.CooldownExpires - Game.ClockTime) > 0.7))
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
                if ((Player.ManaPercentage() < 70) || (E.Instance.Level == 0) || ((E.Instance.CooldownExpires - Game.ClockTime) > 0.7))
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

        public static void Combo()
        {
            var menuItem3 = Einstellung.Item("AimMode").GetValue<StringList>();
            Enum.TryParse(menuItem3.SList[menuItem3.SelectedIndex], out AMode);

            var menuItem2 = Einstellung.Item("Hitchance").GetValue<StringList>();
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
                        dtLastQCast = Environment.TickCount;
                        break;
                    case AimMode.Normal:
                        Q.Cast(GetQTarget(), false, true);
                        break;
                }
            }
            if (W.IsReady() && (Player.ServerPosition.Distance(W.GetPrediction(GetWTarget(), true).CastPosition) < W.Range) && Environment.TickCount > dtLastQCast + Q.Delay * 1000)
            {
                switch (AMode)
                {
                    case AimMode.HitChance:
                        W.CastIfHitchanceEquals(GetWTarget(), Chance, false);
                        dtLastQCast = Environment.TickCount;
                        break;
                    case AimMode.Normal:
                        W.Cast(GetWTarget(), false, true);
                        break;
                }
            }

        }

        public static void Harass()
        {
            var menuItem3 = Einstellung.Item("AimMode").GetValue<StringList>();
            Enum.TryParse(menuItem3.SList[menuItem3.SelectedIndex], out AMode);

            var menuItem2 = Einstellung.Item("Hitchance").GetValue<StringList>();
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
                        dtLastQCast = Environment.TickCount;
                        break;
                    case AimMode.Normal:
                        Q.Cast(GetQTarget(), false, true);
                        break;
                }
            }
        }

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

        public static void WaveClear()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All, MinionTeam.Enemy).Where(x => !x.HasBuffOfType(BuffType.Poison) || GetPoisonBuffEndTime(x) < Game.Time + Q.Delay || ((Q.GetDamage(x) / 3) + 20) > x.Health || ((Q.GetDamage(x) / 3)) < x.Health).ToList();
            var rangedMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.Ranged, MinionTeam.Enemy).Where(x => !x.HasBuffOfType(BuffType.Poison) || GetPoisonBuffEndTime(x) < Game.Time + Q.Delay || ((Q.GetDamage(x) / 3) + 20) > x.Health || ((Q.GetDamage(x) / 3)) < x.Health).ToList();
            var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range + W.Width, MinionTypes.All, MinionTeam.Enemy).Where(x => !x.HasBuffOfType(BuffType.Poison) || GetPoisonBuffEndTime(x) < Game.Time + W.Delay || (W.GetDamage(x) + 20) > x.Health || (W.GetDamage(x)) < x.Health).ToList();
            var rangedMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range + W.Width, MinionTypes.Ranged, MinionTeam.Enemy).Where(x => !x.HasBuffOfType(BuffType.Poison) || GetPoisonBuffEndTime(x) < Game.Time + W.Delay || (W.GetDamage(x) + 20) > x.Health || (W.GetDamage(x)) < x.Health).ToList();

            if (Q.IsReady())
            {
                var FLr = Q.GetCircularFarmLocation(rangedMinionsQ, Q.Width);
                var FLa = Q.GetCircularFarmLocation(allMinionsQ, Q.Width);

                if (FLr.MinionsHit >= 3 && Player.Distance(FLr.Position) < (Q.Range + Q.Width))
                {
                    Q.Cast(FLr.Position);
                    dtLastQCast = Environment.TickCount;
                    return;
                }
                else
                    if (FLa.MinionsHit >= 2 || allMinionsQ.Count() == 1 && Player.Distance(FLr.Position) < (Q.Range + Q.Width))
                    {
                        Q.Cast(FLa.Position);
                        dtLastQCast = Environment.TickCount;
                        return;
                    }
            }

            if (W.IsReady() && Environment.TickCount > dtLastQCast + Q.Delay * 1000)
            {
                var FLr = W.GetCircularFarmLocation(rangedMinionsW, W.Width);
                var FLa = W.GetCircularFarmLocation(allMinionsW, W.Width);

                if (FLr.MinionsHit >= 3 && Player.Distance(FLr.Position) < (W.Range + W.Width))
                {
                    W.Cast(FLr.Position);
                    return;
                }
                else
                    if (FLa.MinionsHit >= 2 || allMinionsW.Count() == 1 && Player.Distance(FLr.Position) < (W.Range + W.Width))
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
                        if (Player.GetSpellDamage(minion, SpellSlot.E) > minion.Health || Player.ManaPercentage() > 70)
                        {
                            E.Cast(minion);
                        }
                    }
                }
            }

        }

        public static void Freeze()
        {
            if (!Orbwalking.CanMove(40)) return;

            if (E.IsReady())
            {
                var MinionList = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);

                foreach (var minion in MinionList.Where(x => x.HasBuffOfType(BuffType.Poison)))
                {
                    var buffEndTime = GetPoisonBuffEndTime(minion);
                    if (buffEndTime > Game.Time + E.Delay)
                    {
                        if (Player.GetSpellDamage(minion, SpellSlot.E) > minion.Health)
                        {
                            E.Cast(minion);
                        }
                    }
                }
            }

        }

        public static Tuple<int, List<Obj_AI_Hero>> GetHits(Spell spell)
        {
            var hits = new List<Obj_AI_Hero>();
            var range = spell.Range * spell.Range;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget() && Player.ServerPosition.Distance(h.ServerPosition, true) < range))
            {
                if (spell.WillHit(enemy, Player.ServerPosition) && Player.ServerPosition.Distance(enemy.ServerPosition, true) < spell.Width * spell.Width)
                {
                    hits.Add(enemy);
                }
            }
            return new Tuple<int, List<Obj_AI_Hero>>(hits.Count, hits);
        }

        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {

            if (args.Slot == SpellSlot.R && GetHits(R).Item1 == 0)
            {
                args.Process = false;
            }
        }

        private static void OnDraw(EventArgs args)
        {
            int i;
            try
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Khaki);

                var DrawList = Einstellung.Item("DrawList").GetValue<bool>();
                var DrawPrediction = Einstellung.Item("DrawPrediction").GetValue<bool>();


                if (DrawList)
                {
                    Drawing.DrawText(100.0f, 100.0f - 10, System.Drawing.Color.White, "Targetlist:");
                    for (i = 0; i < Targets.Count; i++)
                    {
                        if (GetQTarget() != null && GetQTarget().BaseSkinName == Targets[i].BaseSkinName)
                            Drawing.DrawText(100.0f, 100.0f + (i * 10), System.Drawing.Color.Green, "{0}.Target: {1}", (i + 1), Targets[i].BaseSkinName);
                        else
                            Drawing.DrawText(100.0f, 100.0f + (i * 10), System.Drawing.Color.White, "{0}.Target: {1}", (i + 1), Targets[i].BaseSkinName);
                    }
                }
                if (DrawPrediction)
                {
                    foreach (var target in Targets)
                    {
                        if (target.IsVisible && !target.IsDead)
                        {
                            if (Player.ServerPosition.Distance(Q.GetPrediction(target, true).CastPosition) < Q.Range)
                            {
                                Render.Circle.DrawCircle(Q.GetPrediction(target, true).CastPosition, 75f, System.Drawing.Color.Aquamarine);
                            }
                            else
                            {
                                Render.Circle.DrawCircle(Q.GetPrediction(target, true).CastPosition, 75f, System.Drawing.Color.Red);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Game.PrintChat(ex.ToString());
            }
        }

    }
}
