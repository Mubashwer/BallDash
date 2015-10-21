using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class GraphicCache
    {
        public ConcurrentDictionary<string, Texture2D> TextureCache { get; set; }
        public ConcurrentDictionary<string, Effect> ShaderCache { get; set; }

        public GraphicCache()
        {
            TextureCache = new ConcurrentDictionary<string, Texture2D>();
            ShaderCache = new ConcurrentDictionary<string, Effect>();
        }
    }


}
