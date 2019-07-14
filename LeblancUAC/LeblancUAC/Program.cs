using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.Utility;
namespace LeblancUAC
{
    using Color = System.Drawing.Color;
    public class Program
    {
        private static Menu MainMenu;
        private static Spell Q;
        private static Spell W;
        private static Spell W2;
        private static Spell E;
        private static Spell R;

        
        private static AIHeroClient Player { get { return ObjectManager.Player; } }
        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += GameEvent_OnGameLoad;
        }

        private static void GameEvent_OnGameLoad()
        {
            //if (ObjectManager.Player.CharacterName != "Leblanc")
            //{
            //    return;
            //}



            Q = new Spell(SpellSlot.Q, 700f); //skillshot
            Q.SetTargetted(.401f, float.MaxValue);

            W = new Spell(SpellSlot.W, 780f); //charge spell
            W2 = new Spell(SpellSlot.W, 780f);

            W.SetSkillshot(0.25f,80f, float.MaxValue,false,SkillshotType.Circle);
            W2.SetTargetted(0.25f, float.MaxValue);

            E = new Spell(SpellSlot.E, 950f); //self cast
            E.SetSkillshot(.25f, 70f, 1600f, true, SkillshotType.Line);

            R = new Spell(SpellSlot.R);

            MainMenu = new Menu("Leblanc UAC", "Leblanc UAC", true);

            // combo menu
            var comboMenu = new Menu("Combo", "Combo Settings");
            var subCombo = new MenuList("Combomode", "Smart", new[] { "Q + R", "E + R", "W + R" });
            comboMenu.Add(new MenuBool("comboQ", "Use Q", true));
            comboMenu.Add(new MenuBool("comboW", "Use W", true));
            comboMenu.Add(new MenuBool("comboE", "Use E", true));
            comboMenu.Add(new MenuBool("comboR", "Use R", true));

            MainMenu.Add(comboMenu);

            // draw menu 
            var drawMenu = new Menu("Draw", "Draw Settings");
            drawMenu.Add(new MenuBool("drawQ", "Draw Q Range", true));
            drawMenu.Add(new MenuBool("drawW", "Draw W Range", true));
            drawMenu.Add(new MenuBool("drawE", "Draw E Range", true));

            MainMenu.Add(drawMenu);

            //example boolean on MainMenu
            MainMenu.Add(new MenuBool("isDead", "if Player is Dead not Draw Range", true));

            // init MainMenu
            MainMenu.Attach();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            

        }
        private static void Combo()
        {
            
            if(MainMenu["Combo"]["comboQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                var targetQ = TargetSelector.GetTarget(Q.Range);
                
                if(targetQ!=null && targetQ.IsValidTarget(Q.Range))
                {
                    Q.Cast(targetQ);

                }
            }
            if (MainMenu["Combo"]["comboW"].GetValue<MenuBool>().Enabled && W.IsReady())
            {
                var targetW = TargetSelector.GetTarget(W.Range);

                if (targetW != null && targetW.IsValidTarget(W.Range))
                {
                    //W.Cast(targetW);
                    if(Player.Spellbook.GetSpell(SpellSlot.W).Name == "LeblancW")
                    {
                        
                         W2.Cast(targetW);
                        
                    }
                    //if (Player.Spellbook.GetSpell(SpellSlot.W).Name == "LeblancWReturn" /*&& targetW.HealthPercent<=30*/ && targetW.Health<Q.GetDamage(targetW))
                    //{
                    //    W.Cast(targetW);
                    //}





                }
            }
            if (MainMenu["Combo"]["comboE"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                var targetE = TargetSelector.GetTarget(E.Range);

                if (targetE != null && targetE.IsValidTarget(E.Range) )
                {
                    var pred = E.GetPrediction(targetE,false,0,CollisionObjects.Minions);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        
                        E.Cast(pred.CastPosition);
                        
                    }

                }
            }

            if (MainMenu["Combo"]["comboR"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                var targetR = TargetSelector.GetTarget(R.Range);
                
                if (targetR != null && targetR.IsValidTarget(R.Range))
                {

                    R.Cast(targetR);

                }
            }


        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            
            
            if (MainMenu["isDead"].GetValue<MenuBool>().Enabled)
            {
                if (ObjectManager.Player.IsDead)
                {
                    return;
                }
                if (MainMenu["Draw"]["drawQ"].GetValue<MenuBool>().Enabled)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Yellow);

                }

                if (MainMenu["Draw"]["drawW"].GetValue<MenuBool>().Enabled)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Red);

                }
                if (MainMenu["Draw"]["drawE"].GetValue<MenuBool>().Enabled)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Blue);

                }
            }
             
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
            }
        }
    }
}
