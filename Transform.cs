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

    public class Transform
    {
        public Matrix World;
        public Matrix WorldInverseTranspose;
        public Quaternion rotation;


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
            rotation = Quaternion.Identity;

        }

        public Transform(Vector3 position)
        {
            World = Matrix.Translation(position);
            WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));
            rotation = Quaternion.Identity;
        }


        public void Rotate(Vector3 axis, float angle)
        {
            rotation = Quaternion.Multiply(Quaternion.RotationAxis(axis, angle), rotation);
            rotation = Quaternion.Normalize(rotation);
            World = Matrix.Multiply(Matrix.RotationQuaternion(rotation), Matrix.Identity);
            WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));
        }
    }
}
