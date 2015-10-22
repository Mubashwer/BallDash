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
        public FloorUnitGameObject(MazeGame game, string shaderName, Vector3 position, float unitWidth, float unitHeight) {
            this.game = game;
            this.unitWidth = unitWidth;
            this.unitHeight = unitHeight;
            type = GameObjectType.FloorUnit;
            myModel = game.Assets.GetModel("FloorUnit"+unitWidth.ToString()+":"+unitHeight.ToString(), CreateFloorModel);
            transform = new Transform(position);
            ShaderName = shaderName;
        }

        public MyModel CreateFloorModel() {
            return game.Assets.CreateTexturedPlane(unitWidth, unitHeight, 1, "wooden_floor.dds");
        }

        public override void Update(GameTime gametime) {
           
        }
    }
    
}
