using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp.SDK.MenuUI.Values;

namespace LeblancUAC
{
    internal class MenuWrapper
    {
        public class Combat
        {
            public static readonly MenuBool Q = new MenuBool("Q", "Use Q");
            public static readonly MenuBool w = new MenuBool("W", "Use W");
            public static readonly MenuBool E = new MenuBool("E", "Use E");
           
        }
        public class Draw
        {
            public static readonly MenuBool R = new MenuBool("R", "Draw R Range");
            public static readonly MenuBool OnlyReady = new MenuBool("od", "Only Spell Ready");
        }
    }
}
