using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX;
using SharpDX.Toolkit;
using Windows.UI.Input;
using Windows.UI.Core;

namespace Project
{
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;
    // Player class.
    class Player : GameObject
    {
        public float diameter = 2f;
        private float radius;
        public int tessellation = 32;
        public Player(LabGame game)
        {
            this.game = game;
            radius = diameter / 2.0f;
            type = GameObjectType.Player;
            myModel = game.assets.GetModel("player", CreatePlayerModel);
            pos = new SharpDX.Vector3(0, game.boundaryBottom + diameter, 0);
            transform = new Transform(pos, Vector3.UnitY, Vector3.UnitZ);
            SetupBasicEffect();
            SetupEffect("Phong");
        }

        public MyModel CreatePlayerModel()
        {
            return game.assets.CreateTexturedSphere(diameter, tessellation, "marble.jpg");
        }
      

        // Frame update.
        public override void Update(GameTime gameTime)
        {

            // TASK 1: Determine velocity based on accelerometer reading
            pos.X += (float)game.accelerometerReading.AccelerationX;
            pos.Y += (float)game.accelerometerReading.AccelerationY;

            // Keep within the boundaries.
            if (pos.X < game.boundaryLeft + radius) { pos.X = game.boundaryLeft + radius; }
            if (pos.X > game.boundaryRight - radius) { pos.X = game.boundaryRight - radius; }
            if (pos.Y < game.boundaryBottom + radius) { pos.Y = game.boundaryBottom + radius; }
            if (pos.Y > game.boundaryTop - radius) { pos.Y = game.boundaryTop - radius; }

            //transform.World = Matrix.Translation(pos);
            if(transform.Position != pos) transform.Position = pos;

        }

        // React to getting hit by an enemy bullet.
        public void Hit()
        {

        }

        public override void Tapped(GestureRecognizer sender, TappedEventArgs args)
        {
        
        }

        public override void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            pos.X += (float)args.Delta.Translation.X / 100;
            pos.Y += (float)args.Delta.Translation.Y / 100;
            transform.Position = pos;
        }
    }
}