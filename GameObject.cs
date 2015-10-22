﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SharpDX;
using SharpDX.Toolkit;
using Windows.UI.Input;
using Windows.UI.Core;

namespace Project {
    using System.Collections.Concurrent;
    using SharpDX.Toolkit.Graphics;

    // Base class for all game objects.
    abstract public class GameObject {
        public MyModel myModel;
        public Transform transform;
        public MazeGame game;
        public Vector3 position;

        private Effect effect;
        public string ShaderName { get; set; }
        public bool IsHintObject = false;

        public abstract void Update(GameTime gametime);

        public void Draw(GameTime gametime) {
            // Some objects such as the Enemy Controller have no model and thus will not be drawn
            if (myModel != null) {
                SetupEffect();
                GetParamsFromModel();
                game.GraphicsDevice.SetBlendState(game.GraphicsDevice.BlendStates.AlphaBlend);
                // Setup the vertices
                game.GraphicsDevice.SetVertexBuffer(0, myModel.vertexBuffer, myModel.vertexStride);
                game.GraphicsDevice.SetVertexInputLayout(myModel.inputLayout);

                // Setup the index Buffer
                game.GraphicsDevice.SetIndexBuffer(myModel.indexBuffer, myModel.IsIndex32Bits);

                // Apply the effect technique and draw the object
                effect.CurrentTechnique.Passes[0].Apply();

                game.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, myModel.indexBuffer.ElementCount);
            }
        }


        public void SetupEffect() {
            if (!game.GraphicCache.ShaderCache.TryGetValue(ShaderName, out effect)) {
                effect = game.Content.Load<Effect>(ShaderName);
                game.GraphicCache.ShaderCache[ShaderName] = effect;
            }

           
            if (myModel.TextureName != null) {
                Texture2D texture;
                if (!game.GraphicCache.TextureCache.TryGetValue(myModel.TextureName, out texture))
                {
                    texture = game.Content.Load<Texture2D>(myModel.TextureName);
                    game.GraphicCache.TextureCache[myModel.TextureName] = texture;
                }

                // only pass texture to the shader if the shader is capable of taking it
                if (effect.Parameters.Contains("shaderTexture")) {
                    effect.Parameters["shaderTexture"].SetResource(texture);
                }
            }
        }

        public void GetParamsFromModel() {
            if (effect != null) {
                effect.Parameters["World"].SetValue(transform.World);
                effect.Parameters["Projection"].SetValue(game.Camera.Projection);
                effect.Parameters["View"].SetValue(game.Camera.View);
                effect.Parameters["cameraPos"].SetValue(game.Camera.Position);
                effect.Parameters["worldInvTrp"].SetValue(transform.WorldInverseTranspose);
                effect.Parameters["lightPosition"].SetValue(game.Lights.Select(l => l.LightPosition).ToArray<Vector3>());
                effect.Parameters["lightColor"].SetValue(game.Lights.Select(l => l.LightColor).ToArray<Color4>());
                effect.Parameters["lightIntensity"].SetValue(game.Lights.Select(l => l.LightIntensity).ToArray<float>());

                // only apply Ka if the shader is capable of using it
                if (effect.Parameters.Contains("Ka")) {
                    if (game.MazeSolver.Enabled)
                    {
                        if (IsHintObject) effect.Parameters["Ka"].SetValue(1.0f);
                        else effect.Parameters["Ka"].SetValue(0.7f);
                    }
                    else
                    {
                        effect.Parameters["Ka"].SetValue(1.0f);
                    }
                }
            }
        }

        // These virtual voids allow any object that extends GameObject to respond to tapped and manipulation events
        public virtual void OnTapped(GestureRecognizer sender, TappedEventArgs args) {

        }

        public virtual void OnManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args) {

        }

        public virtual void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args) {

        }

        public virtual void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args) {

        }
    }
}
