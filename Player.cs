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

namespace Project {
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;
    // Player class.
    public class Player : GameObject {
        private float diameter = 2f;
        private float radius;
        private int tessellation = 24;

        private Vector3 velocity;
        private float maximumVelocity = 16;
        private double tiltYOffset = -30;

        public Player(LabGame game, string shaderName, Vector3 position) {
            this.game = game;
            radius = diameter / 2.0f;
            type = GameObjectType.Player;
            myModel = game.assets.GetModel("player", CreatePlayerModel);
            base.position = position;
            base.position.Z = -radius;
            transform = new Transform(base.position);
            ShaderName = shaderName;
        }

        public MyModel CreatePlayerModel() {
            return game.assets.CreateTexturedSphere(diameter, tessellation, "marble.dds");
        }


        Stopwatch watch = new Stopwatch();
        // Frame update.
        public override void Update(GameTime gameTime) {
            watch.Reset();
            watch.Start();
            // get the total elapsed time in seconds since the last update
            double elapsedMs = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Determine acceleration based on accelerometer reading
            // get current accelerometer reading
            var accelReading = game.accelerometerReading;

            // find the tilt angle in x and y
            // in atan2, y param is opposite and x param is adjacent.
            // x is left and right of device, y is up and down, z is into/out of screen

            double tiltX = Math.Atan2(accelReading.AccelerationX, accelReading.AccelerationZ);
            double tiltY = Math.Atan2(accelReading.AccelerationY, accelReading.AccelerationZ);

            // add tilt offset
            tiltY += DegreesToRadians(tiltYOffset);

            // use the tilt angle to calculate the ball's X and Y acceleration
            double ballXAccel = elapsedMs * Math.Sin(tiltX) * 0.01;
            double ballYAccel = elapsedMs * Math.Sin(tiltY) * 0.01;

            // TODO: Constrain maximum acceleration

            // add ball acceleration to the ball's velocity
            velocity.X += (float)ballXAccel;
            velocity.Y += (float)ballYAccel;

            // TODO: Add gravity to allow the ball to fall down holes.

            // add drag to the ball
            velocity -= velocity.Length() * velocity * 0.001f;

            // constrain the player's maximum velocity
            if (velocity.Length() > maximumVelocity) {
                velocity = Vector3.Normalize(velocity) * maximumVelocity;
            }

            // remember the current position
            Vector3 lastPosition = position;

            // add the velocity to the current player position
            // using elapsedMs causes jitter, need to investigate
            position += velocity * (float)(elapsedMs / 1000f);
            //position += velocity * 0.3f;

            // Perform all physics on the object.
            // Detect any collisions with edges here.

            // TODO: Add basic collision detection with wall edges and the floor.
            // STRATEGY:

            // 1. Calculate which square the player is on
            // 2. For the 8 adjacent squares around the player, do the following in both X and Y:
            //    a. Circumnavigate the player ball in the horizontal plane about the radius,
            //       checking to see whether any point lies over an adjacent wall
            //    b. If the player's edge is over a wall:
            //       i)   Move the player back such that it is just touching the wall
            //       ii)  Reverse the players velocity with dampening
            //       iii) Trigger a collision event so a sound effect can be played

            // Keep within the boundaries. (TODO: Shouldn't be needed after map physics is complete)
            if (position.X < game.boundaryLeft + radius) {
                position.X = game.boundaryLeft + radius;
                velocity.X *= -0.3f;
            }
            if (position.X > game.boundaryRight - radius) {
                position.X = game.boundaryRight - radius;
                velocity.X *= -0.3f;
            }
            if (position.Y < game.boundaryBottom + radius) {
                position.Y = game.boundaryBottom + radius;
                velocity.Y *= -0.3f;
            }
            if (position.Y > game.boundaryTop - radius) {
                position.Y = game.boundaryTop - radius;
                velocity.Y *= -0.3f;
            }

            // after all physics is done, calculate the actual distance travelled by the ball
            Vector3 distanceDelta = this.position - lastPosition;

            // Get angle of rotation based on the actual distance travelled
            float angle = ((float)Math.Sqrt((distanceDelta.X * distanceDelta.X) + (distanceDelta.Y * distanceDelta.Y))) / radius;
            // Get axis of rotation in local space
            Vector3 rotationAxis = Vector3.Cross(Vector3.Transform(new Vector3(distanceDelta.X, distanceDelta.Y, 0), transform.rotation), transform.World.Backward);
            // Get axis of rotation in world space by reversing rotation
            rotationAxis = Vector3.Normalize(Vector3.Transform(rotationAxis, Quaternion.Invert(transform.rotation)));
            // Rotate and translate
            transform.Rotate(rotationAxis, angle);
            transform.Position = position;
            watch.Stop();
            if (updateCounter > 4) {
                // Update debug stats
                string stats = "Update Delta: " + elapsedMs
                    + Environment.NewLine + "Update Stopwatch: " + watch.ElapsedTicks
                    + Environment.NewLine + "Tilt X: " + RadiansToDegrees(tiltX) + "degrees"
                    + Environment.NewLine + "Tilt Y: " + RadiansToDegrees(tiltY) + "degrees"
                    + Environment.NewLine + "Ball Acc X: " + ballXAccel
                    + Environment.NewLine + "Ball Acc Y: " + ballYAccel
                    + Environment.NewLine + "Ball Vel X: " + velocity.X
                    + Environment.NewLine + "Ball Vel Y: " + velocity.Y
                    + Environment.NewLine + "Ball Vel Z: " + velocity.Z
                    + Environment.NewLine + "Ball Pos X: " + position.X
                    + Environment.NewLine + "Ball Pos Y: " + position.Y
                    + Environment.NewLine + "Ball Pos Z: " + position.Z;

                game.mainPage.UpdateStats(stats);
                updateCounter = 0;
            }
            updateCounter++;

        }

        private int updateCounter = 0;

        private Vector3 ConstrainVector(Vector3 vector, Vector3 absMax) {
            if (Math.Abs(vector.X) > absMax.X) {
                vector.X = absMax.X * Math.Sign(vector.X);
            }
            if (Math.Abs(vector.Y) > absMax.Y) {
                vector.Y = absMax.Y * Math.Sign(vector.Y);
            }
            if (Math.Abs(vector.Z) > absMax.Z) {
                vector.Z = absMax.Z * Math.Sign(vector.Z);
            }

            return vector;
        }

        private static double RadiansToDegrees(double radian) {
            return radian * (180 / Math.PI);
        }

        private static double DegreesToRadians(double degrees) {
            return degrees / (180 / Math.PI);
        }

        public override void Tapped(GestureRecognizer sender, TappedEventArgs args) {

        }

        public override void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args) {
            position.X += (float)args.Delta.Translation.X / 100;
            position.Y += (float)args.Delta.Translation.Y / 100;
            transform.Position = position;
        }
    }
}