using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

namespace Project {
    public class FloorUnitGameObject : GameObject {

        public static readonly float Width = 6f;
        public static readonly float Height = 6f;
        public FloorUnitGameObject(LabGame game, String shaderName, Vector3 position) {
            this.game = game;
            type = GameObjectType.FloorUnit;
            myModel = game.assets.GetModel("FloorUnit"+position.ToString(), CreateFloorModel);
            transform = new Transform(position);
            ShaderName = shaderName;
        }

        public MyModel CreateFloorModel() {
            return game.assets.CreateTexturedPlane(Width, Height, 1, "wooden_floor.jpg");
        }

        public override void Update(GameTime gametime) {
           
        }
    }
    
}
