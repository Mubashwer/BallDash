using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Toolkit;

namespace Project {
    public class MapFloorGameObject : GameObject {
        public MapFloorGameObject(LabGame game) {
            this.game = game;
            type = GameObjectType.Player;
            myModel = game.assets.GetModel("player", CreateFloorModel);
            pos = new SharpDX.Vector3(0, game.boundaryBottom, 0);
            transform = new Transform(pos);
            SetupBasicEffect();
            SetupEffect("Phong");
        }

        public MyModel CreateFloorModel() {
            return game.assets.CreateTexturedPlane(1, 1, 1, "marble.jpg");
        }

        public override void Update(GameTime gametime) {
           
        }
    }
}
