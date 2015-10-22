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
        public Light(Color4 lightColor, float lightIntensity, Vector3 rate)
        {
            LightColor = lightColor;
            LightIntensity = lightIntensity;
            Rate = rate;

        }
    }
}
