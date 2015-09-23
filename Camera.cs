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
    public class Camera
    {
        public Matrix View;
        public Matrix Projection;
        
        private Vector3 cameraTarget;
        public Vector3 CameraTarget
        {
            get
            {
                return this.cameraTarget;
            }
            set
            {
                this.cameraTarget = value;
                transform.forward = Vector3.Normalize(cameraTarget - transform.Position);
                // TODO: Up and World Matrix needs to change as well
            }
        }

        public Game game;
        public Transform transform;

        // Ensures that all objects are being rendered from a consistent viewpoint
        public Camera(Game game) {
            transform = new Transform(new Vector3(0, 0, -10), Vector3.UnitY, Vector3.UnitZ);
            View = Matrix.LookAtLH(transform.Position, new Vector3(0, 0, 0), Vector3.UnitY);
            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.01f, 1000.0f);
            this.game = game;
            Debug.WriteLine(transform.Position);
        }

        // If the screen is resized, the projection matrix will change
        public void Update()
        {
            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);
            View = Matrix.LookAtLH(transform.Position, new Vector3(0, 0, 0), Vector3.UnitY);
        }

    }
}
