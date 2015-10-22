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
        private double tiltYOffset = 0;
        public bool CollisionsEnabled { get; set; }
        public float CollisionReboundDampening { get; set; }

        private Vector3 startPosition;

        /// <summary>
        /// Triggered when the player dies
        /// </summary>
        public event EventHandler PlayerDied;

        /// <summary>
        /// Triggered when the player suffers a physics collision
        /// </summary>
        public event EventHandler Collision;

        public Player(MazeGame game, string shaderName, Vector2 position) {
            this.game = game;
            radius = diameter / 2.0f;
            myModel = game.Assets.GetModel("player", CreatePlayerModel);

            Vector3 player3dPosition = new Vector3(position, -radius);

            startPosition = player3dPosition;
            base.position = startPosition;

            transform = new Transform(base.position);
            ShaderName = shaderName;
            CollisionsEnabled = true;
            CollisionReboundDampening = 0.4f;
        }

        public MyModel CreatePlayerModel() {
            return game.Assets.CreateTexturedSphere(diameter, tessellation, "marble.dds");
        }


        // Frame update.
        public override void Update(GameTime gameTime) {

            // get the total elapsed time in seconds since the last update
            double elapsedMs = gameTime.ElapsedGameTime.TotalMilliseconds;

            double tiltX = 0;
            double tiltY = 0;

            if (game.AccelerometerReading != null) {
                // Determine acceleration based on accelerometer reading
                // get current accelerometer reading
                var accelReading = game.AccelerometerReading;

                // find the tilt angle in x and y
                // in atan2, y param is opposite and x param is adjacent.
                // x is left and right of device, y is up and down, z is into/out of screen

                tiltX = Math.Atan2(accelReading.AccelerationX, accelReading.AccelerationZ);
                tiltY = Math.Atan2(accelReading.AccelerationY, accelReading.AccelerationZ);

                // add tilt offset
                tiltY += DegreesToRadians(tiltYOffset);
            }

            // also consider arrow keys for getting input tilt
            if (game.KeyboardState.IsKeyDown(Keys.Up)) {
                tiltY = 2 * Math.PI * ((double)90/360); // tilt forwards 90 degrees
            }

            if (game.KeyboardState.IsKeyDown(Keys.Down)) {
                tiltY = 2 * Math.PI * ((double)-90 / 360); // tilt backwards 90 degrees
            }

            if (game.KeyboardState.IsKeyDown(Keys.Left)) {
                tiltX = 2 * Math.PI * ((double)-90 / 360); // tilt backwards 90 degrees
            }

            if (game.KeyboardState.IsKeyDown(Keys.Right)) {
                tiltX = 2 * Math.PI * ((double)90 / 360); // tilt backwards 90 degrees
            }


            // use the tilt angle to calculate the ball's X and Y acceleration
            double ballXAccel = elapsedMs * Math.Sin(tiltX) * 0.01;
            double ballYAccel = elapsedMs * Math.Sin(tiltY) * 0.01;

            // add ball acceleration to the ball's velocity
            velocity.X += (float)ballXAccel;
            velocity.Y += (float)ballYAccel;

            velocity.Z += (float)(0.01 * elapsedMs);

            // add drag to the ball
            velocity -= velocity.Length() * velocity * 0.001f;

            // constrain the player's maximum velocity
            if (velocity.Length() > maximumVelocity) {
                velocity = Vector3.Normalize(velocity) * maximumVelocity;
            }

            // remember the current position
            Vector3 lastPosition = position;

            // add the velocity to the current player position
            position += velocity * (float)(elapsedMs / 1000f);

            // Perform all physics on the object.
            // Detect any collisions with edges here.

            // TODO: Add basic collision detection with wall edges and the floor.
            // STRATEGY:

            // 1. Calculate which square the player is on (last position)
            // 2. Circumnavigate the player ball in the horizontal plane about the radius
            //    for a given number of points at equal angles,checking to see whether any
            //    point lies over an adjacent wall
            // 3. If a point on the player's edge is over a wall, there is a collision. So:
            //    a) Move the player back to their previous uncollided position
            //    b) Determine the angle of the collided point, and determine what axis on the ball this effects
            //    c) Reverse the players velocity  with dampening
            //    d) Trigger a collision event so a sound effect can be played

            //1.
            Vector3 currentBallCenterWorldPosition = GetPlayerWorldPosition();
            Vector2 currentBallMapPos = GetPlayerMapPosition();
            Point currentBallMapPoint = GetPlayerMapPoint();

            // Gravity: Check if the current square is a floor, and if so, do not change the height
            Map.UnitType floorType = game.CurrentMap[(int)currentBallMapPos.X, (int)currentBallMapPos.Y];

            if (!game.RainbowModeOn)
            {
                if (floorType == Map.UnitType.None)
                {
                    game.RainbowModeOn = true;
                }
            }

            if (currentBallCenterWorldPosition.Z > radius) {
                // the ball has half fallen down a hole, consider this a game over event
                if (PlayerDied != null) {
                    PlayerDied(this, null);
                }

                // reset player position
                this.velocity = new Vector3();
                this.position = startPosition;
            }

            if (floorType == Map.UnitType.Floor
                || floorType == Map.UnitType.Wall
                || floorType == Map.UnitType.PlayerStart
                || floorType == Map.UnitType.PlayerEnd) {
                position.Z = -radius;
                velocity.Z = 0;
            }

            //2.
            // Check that, given the current position of the ball, no part of the balls 
            // circumference is touching a wall

            bool collisionUp = false;
            bool collisionDown = false;
            bool collisionLeft = false;
            bool collisionRight = false;

            int points = 16; // the number of points to check, 16 is a good approximation
            for (int k = 0; k < points; k++) {
                float pointX = currentBallCenterWorldPosition.X + (radius * (float)Math.Cos((k / (double)points) * 2 * Math.PI));
                float pointY = currentBallCenterWorldPosition.Y + (radius * (float)Math.Sin((k / (double)points) * 2 * Math.PI));

                // get the map coordinates for this point
                Vector2 pointMapPos = game.CurrentMap.GetMapUnitCoordinates(new Vector2(pointX, pointY));

                // get the unit type that this point is located within
                var pointFloorType = game.CurrentMap[(int)pointMapPos.X, (int)pointMapPos.Y];

                // if it's a wall, we have a collision.
                if (pointFloorType == Map.UnitType.Wall) {
                    // check if the point's map position is to the left or to the right of the current map position
                    if ((int)pointMapPos.X > (int)currentBallMapPos.X) {
                        collisionRight = true;
                    }

                    if ((int)pointMapPos.X < (int)currentBallMapPos.X) {
                        collisionLeft = true;
                    }

                    // check if the point's map position is above or below the current ball map position
                    if ((int)pointMapPos.Y > (int)currentBallMapPos.Y) {
                        collisionUp = true;
                    }

                    if ((int)pointMapPos.Y < (int)currentBallMapPos.Y) {
                        collisionDown = true;
                    }
                }
            }

            if (CollisionsEnabled) {
                bool playSoundEffect = false;
                // now, apply collisions based on what we discovered in the previous step
                if ((collisionLeft && velocity.X < 0)
                    || (collisionRight && velocity.X > 0)) {
                    velocity.X *= -CollisionReboundDampening;
                    position.X = lastPosition.X;

                    if (Math.Abs(velocity.X) > 0.5) {
                        playSoundEffect = true;
                    }
                }

                if ((collisionUp && velocity.Y > 0)
                    || (collisionDown && velocity.Y < 0)) {
                    velocity.Y *= -CollisionReboundDampening;
                    position.Y = lastPosition.Y;

                    if (Math.Abs(velocity.Y) > 0.5) {
                        playSoundEffect = true;
                    }
                }

                if (playSoundEffect) {
                    if (Collision != null) {
                        Collision(this, null);
                    }
                }
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

            if (game.GameSettings.DebugEnabled) {
                // Update debug stats
                string stats = "Update Delta: " + elapsedMs
                    + Environment.NewLine + "Updates/second: " + (1000 / elapsedMs)
                    + Environment.NewLine + "Tilt X: " + RadiansToDegrees(tiltX) + "degrees"
                    + Environment.NewLine + "Tilt Y: " + RadiansToDegrees(tiltY) + "degrees"
                    + Environment.NewLine + "Ball Acc X: " + ballXAccel
                    + Environment.NewLine + "Ball Acc Y: " + ballYAccel
                    + Environment.NewLine + "Ball Vel X: " + velocity.X
                    + Environment.NewLine + "Ball Vel Y: " + velocity.Y
                    + Environment.NewLine + "Ball Vel Z: " + velocity.Z
                    + Environment.NewLine + "Ball Pos X: " + position.X
                    + Environment.NewLine + "Ball Pos Y: " + position.Y
                    + Environment.NewLine + "Ball Pos Z: " + position.Z
                    + Environment.NewLine + "Ball Map X: " + currentBallMapPos.X
                    + Environment.NewLine + "Ball Map Y: " + currentBallMapPos.Y
                    + Environment.NewLine + "Ball Map Point X: " + currentBallMapPoint.X
                    + Environment.NewLine + "Ball Map Point Y: " + currentBallMapPoint.Y
                    + Environment.NewLine + "Tile Type: " + floorType
                    + Environment.NewLine + "Collision Left: " + collisionLeft
                    + Environment.NewLine + "Collision Right: " + collisionRight
                    + Environment.NewLine + "Collision Up: " + collisionUp
                    + Environment.NewLine + "Collision Down: " + collisionDown;

                game.GameOverlayPage.UpdateStats(stats);
            }
        }


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

        public Vector3 GetPlayerWorldPosition() {
            return position;// + radius;// * 1.5f;
        }

        public Vector2 GetPlayerMapPosition() {
            Vector3 currentBallCenterWorldPosition = GetPlayerWorldPosition();
            Vector2 currentBallMapPos = game.CurrentMap.GetMapUnitCoordinates(new Vector2(currentBallCenterWorldPosition.X, currentBallCenterWorldPosition.Y));
            return currentBallMapPos;
        }

        public Point GetPlayerMapPoint() {
            Vector2 playerMapPosition = GetPlayerMapPosition();
            return new Point((int)playerMapPosition.X, (int)playerMapPosition.Y);
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