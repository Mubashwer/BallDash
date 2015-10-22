using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

namespace Project {
    public class WallGameObject : GameObject {
        private float size;
        public WallGameObject(MazeGame game, string shaderName, Vector3 position, float size) {
            this.game = game;
            this.size = size;
            myModel = game.Assets.GetModel("Wall" + size.ToString(), CreateWallModel);
            transform = new Transform(position);
            ShaderName = shaderName;
        }

        public MyModel CreateWallModel() {
            return game.Assets.CreateTexturedCube(size, "wooden_wall.dds");
        }

        public override void Update(GameTime gametime) {

        }
    }

}
