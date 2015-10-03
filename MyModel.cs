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
    
    public enum ModelType
    {
        Colored, Textured
    }
    public class MyModel
    {
        public ModelType modelType;
        public float collisionRadius;
        
        public Buffer vertexBuffer;
        public Buffer indexBuffer;
        public bool IsIndex32Bits;
        public VertexInputLayout inputLayout;
        public int vertexStride;
        public String TextureName {get; set;}

        
        public MyModel(LabGame game, VertexPositionNormalTexture[] vertices, int[] indices, String textureName, float collisionRadius)
        {
            this.vertexBuffer = Buffer.Vertex.New(game.GraphicsDevice, vertices);
            this.indexBuffer = Buffer.Vertex.New(game.GraphicsDevice, indices);
            this.inputLayout = VertexInputLayout.New<VertexPositionNormalTexture>(0);
            vertexStride = Utilities.SizeOf<VertexPositionNormalTexture>();
            modelType = ModelType.Textured;
            TextureName = textureName;
            this.collisionRadius = collisionRadius;
        }


        public MyModel(LabGame game, Buffer vertexBuffer, Buffer indexBuffer, bool IsIndex32Bits,  String textureName, float collisionRadius)
        {
            this.vertexBuffer = vertexBuffer;
            this.indexBuffer = indexBuffer;
            this.inputLayout = VertexInputLayout.New<VertexPositionNormalTexture>(0);
            vertexStride = Utilities.SizeOf<VertexPositionNormalTexture>();
            this.IsIndex32Bits = IsIndex32Bits; 
            modelType = ModelType.Textured;
            TextureName = textureName; 
            this.collisionRadius = collisionRadius;
        }
    }
}
