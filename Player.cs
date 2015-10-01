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
            pos = new SharpDX.Vector3(0, game.boundaryBottom + diameter * 3, 0);
            transform = new Transform(pos);
            //SetupBasicEffect();
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
            float deltaX = (float)game.accelerometerReading.AccelerationX;
            float deltaY = (float)game.accelerometerReading.AccelerationY;

            // Keep within the boundaries.
            if (pos.X + deltaX < game.boundaryLeft + radius) { deltaX = 0; }
            if (pos.X + deltaX > game.boundaryRight - radius) { deltaX = 0; }
            if (pos.Y + deltaY < game.boundaryBottom + radius) { deltaY = 0; }
            if (pos.Y + deltaY > game.boundaryTop - radius) { deltaY = 0; }

            if (deltaX == 0 && deltaY == 0) return;
            pos.X += deltaX;
            pos.Y += deltaY;

            // Get angle of rotation
            float angle = ((float)Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY))) / radius;
            // Get axis of rotation in local space
            Vector3 rotationAxis = Vector3.Cross(Vector3.Transform(new Vector3(deltaX, deltaY, 0), transform.rotation), transform.World.Backward);
            // Get axis of rotation in world space by reversing rotation
            rotationAxis = Vector3.Normalize(Vector3.Transform(rotationAxis, Quaternion.Invert(transform.rotation)));
            // Rotate and translate
            transform.Rotate(rotationAxis, angle);
            transform.Position = pos;

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