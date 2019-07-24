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
//update
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
        private static AIHeroClient target { get; set; }
        
        private static AIHeroClient Player { get { return ObjectManager.Player; } }
        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += GameEvent_OnGameLoad;
        }

        private static void GameEvent_OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Leblanc")
            {
                return;
            }



            Q = new Spell(SpellSlot.Q, 700f); //skillshot
            Q.SetTargetted(.401f, float.MaxValue);

            W = new Spell(SpellSlot.W, 650f); //charge spell
            W2 = new Spell(SpellSlot.W, 650f);

            W.SetSkillshot(0.25f,80f, float.MaxValue,false,SkillshotType.Circle);
            W2.SetTargetted(0.25f, float.MaxValue);

            E = new Spell(SpellSlot.E, 950f); //self cast
            E.SetSkillshot(.25f, 70f, 1600f, true, SkillshotType.Line);

            R = new Spell(SpellSlot.R);

            MainMenu = new Menu("Leblanc UAC", "Leblanc UAC", true);

            // combo menu
            var comboMenu = new Menu("Combo", "Smart Combo ");
            var subCombo = new MenuList("Combomode", "Smart", new[] { "Q + R", "E + R", "W + R" });
            comboMenu.Add(new MenuBool("comboQ", "Use Q", true));
            comboMenu.Add(new MenuBool("comboW", "Use W", true));
            comboMenu.Add(new MenuBool("comboE", "Use E", true));
            comboMenu.Add(new MenuBool("comboR", "Use R", true));

            MainMenu.Add(comboMenu);

            //harras menu
            var HarrasMenu = new Menu("Harras", "Harras Settings");
            HarrasMenu.Add(new MenuBool("HarassQ", "Use Q In Harass", true));
            HarrasMenu.Add(new MenuBool("HarassW", "Use W In Harass", true));
            HarrasMenu.Add(new MenuBool("HarassE", "Use E In Harass", true));
            MainMenu.Add(HarrasMenu);

            //laneClear menu

            var laneClear = new Menu("LaneClear", "LaneClear Settings");
            laneClear.Add(new MenuBool("LaneClearQ", "Use Q in LaneClear", true));
            laneClear.Add(new MenuBool("LaneClearW", "Use W in LaneClear", false));
            laneClear.Add(new MenuBool("LaneClearE", "Use E in LaneClear", false));
            MainMenu.Add(laneClear);

            ////jungleclear menu
            //var jngClear = new Menu("jngClear", "Jungle Clear Settings");
            //jngClear.Add(new MenuBool("jngClearQ", "Use Q in Jungle Clear", true));
            //jngClear.Add(new MenuBool("jngClearW", "Use W in Jungle Clear", true));
            //jngClear.Add(new MenuBool("jngClearE", "Use E in Jungle Clear", false));
            //MainMenu.Add(jngClear);

            // draw menu 
            var drawMenu = new Menu("Draw", "Draw Settings");
            drawMenu.Add(new MenuBool("drawQ", "Draw Q Range", true));
            drawMenu.Add(new MenuBool("drawW", "Draw W Range", true));
            drawMenu.Add(new MenuBool("drawE", "Draw E Range", true));
            drawMenu.Add(new MenuBool("dmg", "Draw damage indicator",true));
            MainMenu.Add(drawMenu);

            
            MainMenu.Add(new MenuBool("isDead", "if Player is Dead not Draw Range", true));

            
            MainMenu.Attach();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;

        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (MainMenu["Draw"]["dmg"].GetValue<MenuBool>().Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && x.IsVisibleOnScreen))
                {
                    var dmg = GetDmg(target);
                    if (dmg > 0)
                    {
                        var barPos = target.HPBarPosition;
                        var xPos = barPos.X - 45;
                        var yPos = barPos.Y - 19;
                        if (target.CharacterName == "Annie")
                        {
                            yPos += 2;
                        }

                        var remainHealth = target.Health - dmg;
                        var x1 = xPos + (target.Health / target.MaxHealth * 104);
                        var x2 = (float)(xPos + ((remainHealth > 0 ? remainHealth : 0) / target.MaxHealth * 103.4));
                        Drawing.DrawLine(x1, yPos, x2, yPos, 11, Color.FromArgb(255, 250, 50));
                    }
                }
            }
            
        }
        private static void Clear()
        {


            if (MainMenu["LaneClear"]["LaneClearQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                var minionss = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) && x.IsMinion() && x.IsMinion()).Cast<AIBaseClient>().ToList();
                foreach (AIBaseClient minion in minionss)
            {
                    if (Q.IsReady())
                    {
                          Q.CastOnUnit(minion);
                    }
                }

            }
            if (MainMenu["LaneClear"]["LaneClearW"].GetValue<MenuBool>().Enabled && W.IsReady())
            {
                
                var minionss = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(W.Range) && x.IsMinion() && x.IsMinion()).Cast<AIBaseClient>().ToList();
                if (minionss.Any())
                {
                    var Wfarm = W.GetCircularFarmLocation(minionss);
                    if (Wfarm.Position.IsValid() && Wfarm.MinionsHit >= 2 && !Wfarm.Position.IsUnderEnemyTurret())
                    {
                        W.Cast(Wfarm.Position);
                    }
                }

            }
            if (MainMenu["LaneClear"]["LaneClearE"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                var minionss = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range) && x.IsMinion() && x.IsMinion()).Cast<AIBaseClient>().ToList();
                foreach (AIBaseClient minion in minionss)
                {
                    if (E.IsReady() && E.GetDamage(minion) >= minion.Health)
                    {
                        E.Cast(minion);
                    }
                }

            }

        }
        private static void Harras()
        {
            if (MainMenu["Harras"]["HarassQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                var targetQ = TargetSelector.GetTarget(Q.Range);

                if (targetQ != null && targetQ.IsValidTarget(Q.Range))
                {
                    Q.Cast(targetQ);

                }
            }
            if (MainMenu["Harras"]["HarassW"].GetValue<MenuBool>().Enabled && W.IsReady())
            {
                var targetW = TargetSelector.GetTarget(W.Range);

                if (targetW != null && targetW.IsValidTarget(W.Range))
                {
                    //W.Cast(targetW);
                    if (Player.Spellbook.GetSpell(SpellSlot.W).Name == "LeblancW")
                    {

                        W2.Cast(targetW);

                    }
                    //if (Player.Spellbook.GetSpell(SpellSlot.W).Name == "LeblancWReturn" /*&& targetW.HealthPercent<=30*/ && targetW.Health<Q.GetDamage(targetW))
                    //{
                    //    W.Cast(targetW);
                    //}

                }
            }
            if (MainMenu["Harras"]["HarassE"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                var targetE = TargetSelector.GetTarget(E.Range);

                if (targetE != null && targetE.IsValidTarget(E.Range))
                {
                    var pred = E.GetPrediction(targetE, false, 0, CollisionObjects.Minions);
                    if (pred.Hitchance >= HitChance.High)
                    {

                        E.Cast(pred.CastPosition);

                    }

                }
            }
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
                    if (!W.IsReady() && targetW==null || targetW.IsDead)
                    {
                        W.Cast();
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
        private static float GetDmg(AIBaseClient target)
        {
            if (target == null || !target.IsValidTarget())
            {
                return 0;
            }

            return E.GetDamage(target) + E.GetDamage(target, DamageStage.Buff) + Q.GetDamage(target) + Q.GetDamage(target, DamageStage.Buff) + W.GetDamage(target) + W.GetDamage(target, DamageStage.Buff)+ R.GetDamage(target) + R.GetDamage(target, DamageStage.Buff); ;
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
                case OrbwalkerMode.Harass:
                    Harras();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clear();
                    break;
            }
        }
    }
}
