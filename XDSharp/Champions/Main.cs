using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace XDSharp.Champions
{
    class Main
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static SpellSlot TSSpell()
        {
            switch (Player.ChampionName)
            {/*
                case "Cassiopeia":
                    return SpellSlot.E;
                case "LeeSin":
                    return SpellSlot.Q;
                case "Blitzcrank":
                    return SpellSlot.Q;
                case "Ekko":
                    return SpellSlot.Q;*/
                case "Karthus":
                    return SpellSlot.Q;
            }
            return SpellSlot.Q;
        }
    }
}
