using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

namespace Project {
    public class FloorUnitGameObject : GameObject {
        private float unitWidth;
        private float unitHeight;
        private string textureName;
        public FloorUnitGameObject(MazeGame game, string shaderName, string textureName, Vector3 position, float unitWidth, float unitHeight) {
            this.game = game;
            this.unitWidth = unitWidth;
            this.unitHeight = unitHeight;
            this.textureName = textureName;
            myModel = game.Assets.GetModel("FloorUnit" + unitWidth.ToString() + ":" + unitHeight.ToString(), CreateFloorModel);
            transform = new Transform(position);
            ShaderName = shaderName;
            
        }

        public MyModel CreateFloorModel() {
            return game.Assets.CreateTexturedPlane(unitWidth, unitHeight, 1, textureName);
        }

        public override void Update(GameTime gametime) {
           
        }
    }
    
}
