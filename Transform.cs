using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
   
    public class Transform
    {
        public Matrix World;
        public Matrix WorldInverseTranspose;
        public Vector3 up;
        public Vector3 forward;

        private Vector3 position;
        public Vector3 Position 
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
                World = Matrix.Translation(this.position);
                WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));
            }
        }


        
        public Transform()
        {
            Position = Vector3.Zero;
            up = Vector3.UnitY;
            forward = Vector3.UnitZ;
        }
        
        public Transform(Vector3 position, Vector3 up, Vector3 forward) {
            this.Position = position;
            this.up = up;
            this.forward = forward;
        }

    }
}
