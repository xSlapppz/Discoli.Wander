﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using OQ.MineBot.GUI.Protocol.Movement.Maps;
using OQ.MineBot.PluginBase.Base.Plugin.Tasks;
using OQ.MineBot.PluginBase.Classes;
using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;
using OQ.MineBot.PluginBase.Classes.Base;
using OQ.MineBot.PluginBase.Classes.Entity;
using OQ.MineBot.PluginBase.Classes.Physics;
using OQ.MineBot.PluginBase.Movement.Events;
using OQ.MineBot.PluginBase.Movement.Maps;
using OQ.MineBot.Protocols.Classes.Base;

namespace ShieldPlugin
{
    class Wander : ITask, ITickListener
    {
        private static Random rnd = new Random();
        public MapOptions LowDetailOption;
        private int ticks;
        private bool moving;
        private CancelToken stopToken;

        public Wander()
        {
            MapOptions mapOptions = new MapOptions();
            mapOptions.Look = true;
            mapOptions.Quality = (SearchQuality)152;
            this.LowDetailOption = mapOptions;
            this.stopToken = new CancelToken();
            return;
        }

        public void OnStart()
        {
            Console.WriteLine("Starting Up Wanderer");
        }

        public override bool Exec()
        {
            return !status.entity.isDead && !status.eating;
        }

        public void OnTick()
        {
                if (player.status.entity.isDead)
                    return;
                ++this.ticks;
                this.ticks = 0;
                if (player.physicsEngine.path != null && !player.physicsEngine.path.Complete && (player.physicsEngine.path.Valid || this.moving))
                {
                    return;
                }
                int num1 = Convert.ToInt32(((IEntity)player.status.entity).location.X) + Wander.rnd.Next(-10, 20);
                int int32 = Convert.ToInt32(((IEntity)player.status.entity).location.Y);
                int num2 = Convert.ToInt32(((IEntity)player.status.entity).location.Z) + Wander.rnd.Next(-10, 20);
                IAsyncMap location = player.functions.AsyncMoveToLocation((ILocation)new Location(num1, (float)int32, num2), (IStopToken)this.stopToken, this.LowDetailOption);
                // ISSUE: method pointer
                ((IAreaMap)location).Completed += OnPathReached;
                // ISSUE: method pointer
                ((IAreaMap)location).Cancelled += OnPathFailed;
                location.Start();
                if (((IAreaMap)location).Valid)
                    player.functions.LookAtBlock((ILocation)new Location(num1, (float)int32, num2), false);
                this.moving = true;
                if (((IAreaMap)location).Searched && ((IAreaMap)location).Complete && ((IAreaMap)location).Valid)
                    this.OnPathReached((IAreaMap)location);
            }

        private void OnPathReached(IAreaMap map)
        {
            this.ticks = 0;
            this.moving = false;
        }

        private void OnPathFailed(IAreaMap map, OQ.MineBot.PluginBase.Movement.Geometry.IAreaCuboid cuboid)
        {
            this.ticks = -10;
            this.moving = false;
        }
    }
}
