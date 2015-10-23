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
    using Windows.UI.Xaml;

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

        private Windows.Foundation.Point? touchPosition { get; set; }

        private Vector3 startPosition;

        /// <summary>
        /// Triggered when the player dies
        /// </summary>
        public event EventHandler PlayerDied;

        public event EventHandler CompletedLevel;

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
            CollisionReboundDampening = 0.7f;

            game.Input.gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            game.Input.gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;
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

            if (game.GameSettings.TouchControlsEnabled) {
                if (touchPosition != null) {
                    tiltX = 2 * Math.PI * ((double)90 / 360) * (touchPosition.Value.X);
                    tiltY = 2 * Math.PI * ((double)-90 / 360) * (touchPosition.Value.Y);
                }
            }

            // also consider arrow keys for getting input tilt
            if (game.KeyboardState.IsKeyDown(Keys.Up)) {
                tiltY = 2 * Math.PI * ((double)90 / 360); // tilt forwards 90 degrees
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
            double ballXAccel = Math.Sin(tiltX) * 0.01;
            double ballYAccel = Math.Sin(tiltY) * 0.01;
            double ballZAccel = 0.04;

            // add ball acceleration to the ball's velocity
            velocity.X += (float)(ballXAccel * elapsedMs);
            velocity.Y += (float)(ballYAccel * elapsedMs);
            velocity.Z += (float)(ballZAccel * elapsedMs);

            // add drag to the ball
            velocity -= Get2DLength(velocity) * velocity * 0.00001f * (float)elapsedMs;

            // constrain the player's maximum 2D velocity
            velocity = Constrain2DLength(velocity, maximumVelocity);

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
            //    a) Determine the angle of the collided point
            //    b) Add this to a running average vector
            //    c) Do a) and b) for additional points until one is hit that is not a collision
            //    d) Calculate the angle of the average vector with arctan. This is the collision normal
            //    e) Calculate the incident angle of the ball against the plane using the collision normal
            //    f) Reflect the velocity using the incident angle
            //    g) Apply dampening on velocity only in the angle of the collision normal
            //    i) Continue until all points are checked.
            //    h) Trigger a collision event so a sound effect can be playedsirable)

            //1.

            bool hardCollisionOccured = false;
            bool collisionOccured = false;

            Vector3 currentBallCenterWorldPosition = GetPlayerWorldPosition();
            Vector2 currentBallMapPos = GetPlayerMapPosition();
            Point currentBallMapPoint = GetPlayerMapPoint();

            // Gravity: Check if the current square is a floor, and if so, do not change the height
            Map.UnitType floorType = game.CurrentLevel.Map[(int)currentBallMapPos.X, (int)currentBallMapPos.Y];

            if (!game.RainbowModeOn) {
                if (floorType.HasFlag(Map.UnitType.Rainbow)) {
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

                // disable rainbow
                this.game.RainbowModeOn = false;
            }

            // detect the ground
            if (floorType.HasFlag(Map.UnitType.Floor)) {
                position.Z = -radius;
                velocity.Z = 0;
            }

            // detect the player reaching the end tile
            if (floorType.HasFlag(Map.UnitType.PlayerEnd)) {
                if (CompletedLevel != null) {
                    CompletedLevel(this, null);
                }
            }

            int points = 512; // the number of points to check, 16 is a good approximation
            for (int k = 0; k < points; k++) {
                // calculate the vector from the center of the ball to the current point being checked
                double currentPointAngle = (k / (double)points) * 2 * Math.PI;

                Vector2 pointOffsetVector = new Vector2((float)Math.Cos(currentPointAngle),
                (float)Math.Sin(currentPointAngle)) * radius;

                // calculate the world coordinate of the current point being checked
                Vector2 pointPositionVector = new Vector2(currentBallCenterWorldPosition.X + pointOffsetVector.X,
                currentBallCenterWorldPosition.Y + pointOffsetVector.Y);

                // get the map coordinates for the current point being checked
                Vector2 pointMapPos = game.CurrentLevel.Map.GetMapUnitCoordinates(pointPositionVector);

                // get the map unit type that this point is located within
                var pointFloorType = game.CurrentLevel.Map[(int)pointMapPos.X, (int)pointMapPos.Y];

                // if the map unit type is a wall, we have a collision.
                if (pointFloorType.HasFlag(Map.UnitType.Wall)) {
                    collisionOccured = true;

                    // find length and angle of current ball velocity
                    float ballVelocityMag = Get2DLength(velocity);

                    // trigger the sound effect if ball hit wall with enough velocity
                    if (ballVelocityMag > 2) {
                        hardCollisionOccured = true;
                    }

                    // find the current angle of the ball's velocity vector
                    float ballVelocityAngle = (float)Math.Atan2(velocity.Y, velocity.X);

                    // calculate reflection angle
                    // reflection angle is 180 degrees minus the angle between the perpendicular vector of the collision normal
                    // and the ball velocity angle
                    float newBallVelocityAngle = (2 * (float)currentPointAngle) - ballVelocityAngle + (float)Math.PI;

                    // calculate new ball X and Y velocity componants
                    velocity.X = (float)Math.Cos(newBallVelocityAngle) * ballVelocityMag;
                    velocity.Y = (float)Math.Sin(newBallVelocityAngle) * ballVelocityMag;

                    // dampen the velocity of the ball in the direction of the collided point's normal

                    //// calculate the normal of the surface the ball hit, which is 180 degrees plus the collision normal angle
                    //float surfaceNormalAngle = (float)currentPointAngle + (float)Math.PI;

                    //// calculate component of new velocity that is away from the wall normal
                    //float normalReboundVelocity = ballVelocityMag * (float)Math.Sin(surfaceNormalAngle - newBallVelocityAngle + Math.PI / 2);

                    //// the amount to dampen the rebound of the ball by
                    //float normalDampeningVelocity = normalReboundVelocity * 0.6f;

                    //// now calculate the dampening vector
                    //Vector2 dampeningVector = new Vector2((float)Math.Cos(surfaceNormalAngle) * normalDampeningVelocity,
                    //    (float)Math.Sin(surfaceNormalAngle) * normalDampeningVelocity);

                    //// apply the dampening vector to the velocity by subtracting them
                    //velocity.X -= dampeningVector.X;
                    //velocity.Y -= dampeningVector.Y;
                }
            }

            if (collisionOccured) {
                // add the new velocity to the last known uncollided player position
                position = lastPosition;// + velocity * (float)(elapsedMs / 1000f);
            }

            if (hardCollisionOccured) {
                if (Collision != null) {
                    Collision(this, null);
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
                    + Environment.NewLine + "Tilt X: " + RadiansToDegrees(tiltX) + " degrees"
                    + Environment.NewLine + "Tilt Y: " + RadiansToDegrees(tiltY) + " degrees"
                    + Environment.NewLine + "Ball Acc X: " + ballXAccel
                    + Environment.NewLine + "Ball Acc Y: " + ballYAccel
                    + Environment.NewLine + "Ball Acc Z: " + ballZAccel
                    + Environment.NewLine + "Ball Vel X: " + velocity.X
                    + Environment.NewLine + "Ball Vel Y: " + velocity.Y
                    + Environment.NewLine + "Ball Vel Z: " + velocity.Z
                    + Environment.NewLine + "Ball Vel Abs: " + Get2DLength(velocity)
                    + Environment.NewLine + "Ball Pos X: " + position.X
                    + Environment.NewLine + "Ball Pos Y: " + position.Y
                    + Environment.NewLine + "Ball Pos Z: " + position.Z
                    + Environment.NewLine + "Ball Map X: " + currentBallMapPos.X
                    + Environment.NewLine + "Ball Map Y: " + currentBallMapPos.Y
                    + Environment.NewLine + "Ball Map Point X: " + currentBallMapPoint.X
                    + Environment.NewLine + "Ball Map Point Y: " + currentBallMapPoint.Y
                    + Environment.NewLine + "Tile Type: " + floorType
                    + Environment.NewLine + "Collision Occured: " + collisionOccured;
                //+ Environment.NewLine + "Collision Normal Angle: " + RadiansToDegrees(collisionNormalAngle) + " degrees"
                //+ Environment.NewLine + "Collision Rebound Angle: " + RadiansToDegrees(newBallVelocityAngle) + " degrees";


                if (touchPosition != null) {
                    stats += Environment.NewLine + "Touch X: " + touchPosition.Value.X
                    + Environment.NewLine + "Touch Y: " + touchPosition.Value.Y;
                }

                game.GameOverlayPage.UpdateStats(stats);
            }
        }

        private double GetAngleDifference(double angle1, double angle2) {
            double xTotal = 0;
            double yTotal = 0;

            xTotal = Math.Cos(angle1) + Math.Cos(angle2);
            yTotal = Math.Sin(angle1) + Math.Sin(angle2);

            return Math.Atan2(yTotal, xTotal);
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

        private float Get2DLength(Vector3 vector) {
            Vector2 vector2d = new Vector2(vector.X, vector.Y);
            return vector2d.Length();
        }

        private Vector3 Constrain2DLength(Vector3 vector, float maxLength) {
            Vector2 vector2d = new Vector2(vector.X, vector.Y);

            if (vector2d.Length() > maxLength) {
                vector2d.Normalize();
                vector2d *= maxLength;
                vector.X = vector2d.X;
                vector.Y = vector2d.Y;
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
            Vector2 currentBallMapPos = game.CurrentLevel.Map.GetMapUnitCoordinates(new Vector2(currentBallCenterWorldPosition.X, currentBallCenterWorldPosition.Y));
            return currentBallMapPos;
        }

        public Point GetPlayerMapPoint() {
            Vector2 playerMapPosition = GetPlayerMapPosition();
            return new Point((int)playerMapPosition.X, (int)playerMapPosition.Y);
        }

        public override void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args) {
            touchPosition = new Windows.Foundation.Point((args.Position.X / Window.Current.Bounds.Width) - 0.5, (args.Position.Y / Window.Current.Bounds.Height) - 0.5);
        }

        public override void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args) {
            touchPosition = null;
        }
    }
}