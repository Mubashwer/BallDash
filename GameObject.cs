using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SharpDX;
using SharpDX.Toolkit;
using Windows.UI.Input;
using Windows.UI.Core;

namespace Project
{
    using SharpDX.Toolkit.Graphics;
    public enum GameObjectType
    {
        None, Player, Enemy
    }

    // Super class for all game objects.
    abstract public class GameObject
    {
        public MyModel myModel;
        public Transform transform;
        public LabGame game;
        public GameObjectType type = GameObjectType.None;
        public Vector3 pos;
        public Effect effect;
        public BasicEffect basicEffect;

        public abstract void Update(GameTime gametime);
        public void Draw(GameTime gametime)
        {
            // Some objects such as the Enemy Controller have no model and thus will not be drawn
            if (myModel != null)
            {
                GetParamsFromModel();

                // Setup the vertices
                game.GraphicsDevice.SetVertexBuffer(0, myModel.vertexBuffer, myModel.vertexStride);
                game.GraphicsDevice.SetVertexInputLayout(myModel.inputLayout);
                
                // Setup the index Buffer
                game.GraphicsDevice.SetIndexBuffer(myModel.indexBuffer,myModel.IsIndex32Bits);

                // Apply the effect technique and draw the object
                if (effect != null)
                {
                    effect.CurrentTechnique.Passes[0].Apply();
                }
                else if (basicEffect != null)
                {
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                }
                game.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, myModel.indexBuffer.ElementCount);
            }
        }

        public void SetupBasicEffect()
        {

            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                View = game.camera.View,
                Projection = game.camera.Projection,
                World = transform.World,
                Texture = myModel.texture,
                TextureEnabled = (myModel.modelType == ModelType.Textured),
                VertexColorEnabled = (myModel.modelType == ModelType.Colored)
            };

        }

        public void SetupEffect(string shaderName)
        {
            effect = game.Content.Load<Effect>(shaderName);
            effect.Parameters["shaderTexture"].SetResource(myModel.texture);
        }

        public void GetParamsFromModel()
        {
            if (effect != null)
            {
                effect.Parameters["World"].SetValue(transform.World);
                effect.Parameters["Projection"].SetValue(game.camera.Projection);
                effect.Parameters["View"].SetValue(game.camera.View);
                effect.Parameters["cameraPos"].SetValue(game.camera.transform.Position);
                effect.Parameters["worldInvTrp"].SetValue(transform.WorldInverseTranspose);
            }
            if (basicEffect != null)
            {
                basicEffect.View = game.camera.View;
                basicEffect.Projection = game.camera.Projection;
                basicEffect.World = transform.World;
            }
        }

        // These virtual voids allow any object that extends GameObject to respond to tapped and manipulation events
        public virtual void Tapped(GestureRecognizer sender, TappedEventArgs args)
        {

        }

        public virtual void OnManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {

        }

        public virtual void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {

        }

        public virtual void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {

        }
    }
}
