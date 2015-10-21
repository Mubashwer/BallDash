using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX;
using SharpDX.Toolkit;
namespace Project {
    using SharpDX.Toolkit.Input;

    public class Camera {
        public Matrix View;
        public Matrix Projection;

        public float yaw = 0;
        public float pitch = 0;
        public float roll = 0;
        public bool cameraMoved;

        private Vector3 defaultCameraTarget;
        private Vector3 defaultUp;
        private Vector3 up;
        private Vector3 playerPosition;
        private float defaultCameraHeight = -15;
        private float maxCameraHeight = -25;
        private float minCameraHeight = -6f;


        public Vector3 Position { get; private set; }

        private Vector3 cameraTarget;
        public Vector3 CameraTarget {
            get {
                return cameraTarget;
            }
        }


        public MazeGame game;


        // Ensures that all objects are being rendered from a consistent viewpoint
        public Camera(MazeGame game) {
            playerPosition = game.Player.transform.Position;
            defaultCameraTarget = new Vector3(playerPosition.X, playerPosition.Y, 1);
            cameraTarget = defaultCameraTarget;
            defaultUp = Vector3.UnitY;
            up = defaultUp;
            Position = new Vector3(playerPosition.X, playerPosition.Y, defaultCameraHeight);
            View = Matrix.LookAtLH(Position, defaultCameraTarget, defaultUp);
            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.01f, 1000.0f);
            this.game = game;

        }

        // If the screen is resized, the projection matrix will change
        public void Update() {
            Position = Position - playerPosition + game.Player.transform.Position;
            defaultCameraTarget = defaultCameraTarget - playerPosition + game.Player.transform.Position;
            cameraTarget = cameraTarget - playerPosition + game.Player.transform.Position;
            playerPosition = game.Player.transform.Position;


            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);
            View = Matrix.LookAtLH(Position, cameraTarget, up);
            /*

            if (!cameraMoved) return;
            cameraMoved = false;
            AngleReset();
            Matrix rotation = Matrix.RotationYawPitchRoll(yaw, pitch, roll);
            cameraTarget = Vector3.TransformCoordinate(defaultCameraTarget, rotation);
            up = Vector3.TransformCoordinate(defaultUp, rotation);
            cameraTarget = Position + cameraTarget;
            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);
            View = Matrix.LookAtLH(Position, cameraTarget, up);
            */
        }

        // Pinch to zoom in/out within given limits
        public void Zoom(float amount)
        {
            float positionZ = Position.Z * amount;

            if(positionZ < maxCameraHeight) positionZ = maxCameraHeight;

            if (positionZ > minCameraHeight) positionZ = minCameraHeight;

            Position = new Vector3(Position.X, Position.Y, positionZ);
        }

        void AngleReset() {
            if (yaw >= (float)(2 * Math.PI)) {
                yaw -= (float)(2 * Math.PI);
            }
            else if (yaw <= -(float)(2 * Math.PI)) {
                yaw += (float)(2 * Math.PI);
            }
            if (pitch >= (float)(2 * Math.PI)) {
                pitch -= (float)(2 * Math.PI);
            }
            else if (pitch <= -(float)(2 * Math.PI)) {
                pitch += (float)(2 * Math.PI);
            }
            if (roll >= (float)(2 * Math.PI)) {
                roll -= (float)(2 * Math.PI);
            }
            else if (roll <= -(float)(2 * Math.PI)) {
                roll += (float)(2 * Math.PI);
            }
        }

    }
}
