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

        public Vector3 Position 
        {
            get
            {
                return World.TranslationVector;
            }
            set
            {
                World.TranslationVector = value;
                WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));
                
            }
        }

        public Vector3 Up
        {
            get
            {
                return World.Up + Position;
            }
        }

        public Vector3 Forward
        {
            get
            {
                return World.Backward + Position;
            }
        }

        
        public Transform()
        {
            World = Matrix.Identity;
            WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));

        }

        public Transform(Vector3 position)
        {
            World = Matrix.Translation(position);
            WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));
        }

        public void Rotate(Vector3 axis, float angle) {
            Matrix rotation = Matrix.RotationAxis(axis, (float)((angle/180)*Math.PI));
            World = Matrix.Multiply(rotation, World);
            WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));
        }

        public void RotateInRad(Vector3 axis, float angle)
        {
            Matrix rotation = Matrix.RotationAxis(axis, angle);
            World = Matrix.Multiply(rotation, World);
            WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));
        }
    }
}
