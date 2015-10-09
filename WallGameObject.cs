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

        public static readonly float Width = 6f;
        public static readonly float Height = 6f;
        public WallGameObject(LabGame game, String shaderName, Vector3 position)
        {
            this.game = game;
            type = GameObjectType.Wall;
            myModel = game.assets.GetModel("Wall" + position.ToString(), CreateWallModel);
            transform = new Transform(position);
            ShaderName = shaderName;
        }

        public MyModel CreateWallModel()
        {
            return game.assets.CreateTexturedCube(Width, "wooden_wall.jpg");
        }

        public override void Update(GameTime gametime)
        {

        }
    }

}
