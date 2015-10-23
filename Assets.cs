using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;
namespace Project
{
    using SharpDX.Toolkit.Graphics;
    public class Assets {
        MazeGame game;
        List<GeometricPrimitive> primitives;
        public Assets(MazeGame game) {
            this.game = game;
            primitives = new List<GeometricPrimitive>();
        }

        // Dictionary of currently loaded models.
        // New/existing models are loaded by calling GetModel(modelName, modelMaker).
        public Dictionary<String, MyModel> modelDict = new Dictionary<String, MyModel>();

        // Load a model from the model dictionary.
        // If the model name hasn't been loaded before then modelMaker will be called to generate the model.
        public delegate MyModel ModelMaker();
        public MyModel GetModel(String modelName, ModelMaker modelMaker) {
            if (!modelDict.ContainsKey(modelName)) {
                modelDict[modelName] = modelMaker();
            }
            return modelDict[modelName];
        }

        // Create a sphere with a texture
        public MyModel CreateTexturedSphere(float diameter, int tessellation, string textureName) {
            GeometricPrimitive sphere = GeometricPrimitive.Sphere.New(game.GraphicsDevice, diameter, tessellation, true);
            primitives.Add(sphere);
            return new MyModel(game, sphere.VertexBuffer, sphere.IndexBuffer, sphere.IsIndex32Bits, textureName, diameter / 2.0f);
        }

        public MyModel CreateTexturedCube(float size, string textureName) {
            GeometricPrimitive cube = GeometricPrimitive.Cube.New(game.GraphicsDevice, size, true);
            primitives.Add(cube);
            return new MyModel(game, cube.VertexBuffer, cube.IndexBuffer, cube.IsIndex32Bits, textureName, size / 2.0f);
        }

        public MyModel CreateTexturedPlane(float width, float height, int tessellation, string textureName) {
            GeometricPrimitive plane = GeometricPrimitive.Plane.New(game.GraphicsDevice, width, height, tessellation,false);
            primitives.Add(plane);
            return new MyModel(game, plane.VertexBuffer, plane.IndexBuffer, plane.IsIndex32Bits, textureName, 0);
        }


        public MyModel CreateTexturedPlane(float width, float height, string textureName) {

            // Plane is centered at origin
            float offsetX = width / 2f;
            float offsetY = height / 2f;
            int x = 0, y = 0;

            // Vertex vectors of the plane
            //v2    v3
            //v1    v4 
            Vector3 v1 = new Vector3(x - offsetX, y - offsetY, 0);
            Vector3 v2 = new Vector3(x- offsetX, y + offsetY, 0);
            Vector3 v3 = new Vector3(x + offsetX , y + offsetY, 0);
            Vector3 v4 = new Vector3(x + offsetX, y - offsetY, 0);

            // facing towards negative Z axis
            Vector3 normal = -Vector3.UnitZ;

            VertexPositionNormalTexture[] vertexList = new VertexPositionNormalTexture[4];
            int[] indices = new int[6];

            // Define the vertices and use texture
            vertexList[0] = new VertexPositionNormalTexture(v1, normal, new Vector2(0, 1));
            vertexList[1] = new VertexPositionNormalTexture(v2, normal, new Vector2(0, 0));
            vertexList[2] = new VertexPositionNormalTexture(v3, normal, new Vector2(1, 0));
            vertexList[3] = new VertexPositionNormalTexture(v4, normal, new Vector2(1, 1));


            // For orientation, v1->v2->v4 then v4->v2>v3
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 3;
            indices[3] = 3;
            indices[4] = 1;
            indices[5] = 2;

            Buffer vertexBuffer = Buffer.Vertex.New(game.GraphicsDevice, vertexList);
            Buffer indexBuffer = Buffer.Index.New(game.GraphicsDevice, indices);
            return new MyModel(game, vertexBuffer, indexBuffer, true, textureName, 0);
        }



        //MUST BE CHANGED TO SUPPORT INDEXED BUFFER

        /*/ Create a cube with one texture for all faces.
        public MyModel CreateTexturedCube(String textureName, float size)
        {
            return CreateTexturedCube(textureName, new Vector3(size, size, size));
        }

        public MyModel CreateTexturedCube(String texturePath, Vector3 size)
        {
            VertexPositionTexture[] shapeArray = new VertexPositionTexture[]{
            new VertexPositionTexture(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(0.0f, 1.0f)), // Front
            new VertexPositionTexture(new Vector3(-1.0f, 1.0f, -1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1.0f, 1.0f, -1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f, 1.0f, -1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1.0f, -1.0f, -1.0f), new Vector2(1.0f, 1.0f)),

            new VertexPositionTexture(new Vector3(-1.0f, -1.0f, 1.0f), new Vector2(1.0f, 1.0f)), // BACK
            new VertexPositionTexture(new Vector3(1.0f, 1.0f, 1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, 1.0f, 1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, -1.0f, 1.0f), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f, -1.0f, 1.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f, 1.0f, 1.0f), new Vector2(0.0f, 0.0f)),

            new VertexPositionTexture(new Vector3(-1.0f, 1.0f, -1.0f), new Vector2(0.0f, 1.0f)), // Top
            new VertexPositionTexture(new Vector3(-1.0f, 1.0f, 1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1.0f, 1.0f, 1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, 1.0f, -1.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f, 1.0f, 1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1.0f, 1.0f, -1.0f), new Vector2(1.0f, 1.0f)),

            new VertexPositionTexture(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(0.0f, 0.0f)), // Bottom
            new VertexPositionTexture(new Vector3(1.0f, -1.0f, 1.0f), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, -1.0f, 1.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1.0f, -1.0f, -1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1.0f, -1.0f, 1.0f), new Vector2(1.0f, 1.0f)),

            new VertexPositionTexture(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(1.0f, 1.0f)), // Left
            new VertexPositionTexture(new Vector3(-1.0f, -1.0f, 1.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, 1.0f, 1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, 1.0f, 1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, 1.0f, -1.0f), new Vector2(1.0f, 0.0f)),

            new VertexPositionTexture(new Vector3(1.0f, -1.0f, -1.0f), new Vector2(0.0f, 1.0f)), // Right
            new VertexPositionTexture(new Vector3(1.0f, 1.0f, 1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1.0f, -1.0f, 1.0f), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f, -1.0f, -1.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f, 1.0f, -1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1.0f, 1.0f, 1.0f), new Vector2(1.0f, 0.0f)),
            };

            for (int i = 0; i < shapeArray.Length; i++)
            {
                shapeArray[i].Position.X *= size.X / 2;
                shapeArray[i].Position.Y *= size.Y / 2;
                shapeArray[i].Position.Z *= size.Z / 2;
            }

            float collisionRadius = (size.X + size.Y + size.Z) / 6 ;
            return new MyModel(game, shapeArray, texturePath, collisionRadius);
        }*/
    }
}
