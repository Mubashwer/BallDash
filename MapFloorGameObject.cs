using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Toolkit;

namespace Project {
    public class MapFloorGameObject : GameObject {
        public MapFloorGameObject(LabGame game, String shaderName) {
            this.game = game;
            type = GameObjectType.Floor;
            myModel = game.assets.GetModel("floor", CreateFloorModel);
            pos = new SharpDX.Vector3(0, 0, 0);
            transform = new Transform(pos);
            ShaderName = shaderName;
        }

        public MyModel CreateFloorModel() {
            return game.assets.CreateTexturedPlane(100f, 100f, 1, "wooden_floor.jpg");
        }

        public override void Update(GameTime gametime) {
           
        }
    }
}
