using System;
using SpatialSim.Engine.Core;
using SpatialSim.Game;

namespace SpatialSim
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Window.Init(GameManager.Init, GameManager.Update, GameManager.FixedUpdate);
        }
    }
}