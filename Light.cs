using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class Light 
    { 
        public Vector3 LightPosition { get; set; }
        public Color4 LightColor { get; set; }
        public float LightIntensity { get; set; }

        public Vector3 Rate { get; set; } // rate of movement
        public Light(MazeGame game, Color4 lightColor, float lightIntensity, Vector3 rate)
        {
            var map = game.CurrentLevel.Map;
            LightPosition = new Vector3(map.Width / 2f * map.MapUnitWidth, map.Height * map.MapUnitHeight / 2f, game.DefaultLightHeight);
            LightColor = lightColor;
            LightIntensity = lightIntensity;
            Rate = rate;

        }
    }
}
