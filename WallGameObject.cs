using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
    public class WallGameObject : GameObject
    {
        public WallGameObject(MazeGame game, string shaderName, Vector3 position)
        {
            this.game = game;
            type = GameObjectType.Wall;
            myModel = game.Assets.GetModel("Wall" + position.ToString(), CreateWallModel);
            transform = new Transform(position);
            ShaderName = shaderName;
        }

        public MyModel CreateWallModel()
        {
            return game.Assets.CreateTexturedCube(Map.WorldUnitWidth, "wooden_wall.jpg");
        }

        public override void Update(GameTime gametime)
        {

        }
    }

}
