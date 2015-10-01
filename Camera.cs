using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX;
using SharpDX.Toolkit;
namespace Project
{
    using SharpDX.Toolkit.Input;

    public class Camera
    {
        public Matrix View;
        public Matrix Projection;

        public float yaw = 0;
        public float pitch = 0;
        public float roll = 0;
        public bool cameraMoved;

        private Vector3 defaultCameraTarget;
        private Vector3 defaultUp;
        private Vector3 position;
        public Vector3 Position { get; private set; }

        private Vector3 cameraTarget;
        public Vector3 CameraTarget
        {
            get
            {
                return cameraTarget;
            }
        }


        public Game game;


        // Ensures that all objects are being rendered from a consistent viewpoint
        public Camera(Game game) {
            defaultCameraTarget = new Vector3(0, 0, 1);
            defaultUp = Vector3.UnitY;
            position = new Vector3(0, 0, -10);
            View = Matrix.LookAtLH(position, defaultCameraTarget, defaultUp);
            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.01f, 1000.0f);
            this.game = game;

        }

        // If the screen is resized, the projection matrix will change
        public void Update()
        {
            if (!cameraMoved) return;
            cameraMoved = false;
            AngleReset();
            Matrix rotation = Matrix.RotationYawPitchRoll(yaw, pitch, roll);
            cameraTarget = Vector3.TransformCoordinate(defaultCameraTarget, rotation);
            Vector3 up = Vector3.TransformCoordinate(defaultUp, rotation);
            cameraTarget = position + cameraTarget;
            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);
            View = Matrix.LookAtLH(position, cameraTarget, up);
        }

        void AngleReset()
        {
            if (yaw >= (float)(2 * Math.PI))
            {
                yaw -= (float)(2 * Math.PI);
            }
            else if (yaw <= -(float)(2 * Math.PI))
            {
                yaw += (float)(2 * Math.PI);
            }
            if (pitch >= (float)(2 * Math.PI))
            {
                pitch -= (float)(2 * Math.PI);
            }
            else if (pitch <= -(float)(2 * Math.PI))
            {
                pitch += (float)(2 * Math.PI);
            }
            if (roll >= (float)(2 * Math.PI))
            {
                roll -= (float)(2 * Math.PI);
            }
            else if (roll <= -(float)(2 * Math.PI))
            {
                roll += (float)(2 * Math.PI);
            }
        }

    }
}
