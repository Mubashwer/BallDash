using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

namespace Project {
    public class FloorUnitGameObject : GameObject {

        private string textureName;
        public FloorUnitGameObject(MazeGame game, string shaderName, string textureName, Vector3 position) {
            this.game = game;
            type = GameObjectType.FloorUnit;
            this.textureName = textureName;
            myModel = game.Assets.GetModel("FloorUnit"+position.ToString(), CreateFloorModel);
            transform = new Transform(position);
            ShaderName = shaderName;
            
        }

        public MyModel CreateFloorModel() {
            return game.Assets.CreateTexturedPlane(Map.WorldUnitWidth, Map.WorldUnitHeight, 1, textureName);
        }

        public override void Update(GameTime gametime) {
           
        }
    }
    
}
