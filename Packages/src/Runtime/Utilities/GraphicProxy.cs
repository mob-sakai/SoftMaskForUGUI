using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    public class GraphicProxy
    {
        private static readonly List<GraphicProxy> s_Proxies = new List<GraphicProxy>()
        {
            new GraphicProxy()
        };

        public static void Register(GraphicProxy proxy)
        {
            // Register only once.
            var count = s_Proxies.Count;
            for (var i = 0; i < count; i++)
            {
                var p = s_Proxies[i];
                if (p.GetType() == proxy.GetType()) return;
            }

            s_Proxies.Add(proxy);
        }

        public static GraphicProxy Find(Graphic graphic)
        {
            if (graphic == null) return null;

            var count = s_Proxies.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var p = s_Proxies[i];
                if (p.IsValid(graphic)) return p;
            }

            return null;
        }

        /// <summary>
        /// Check if the graphic is valid for this proxy.
        /// </summary>
        protected virtual bool IsValid(Graphic graphic)
        {
            return graphic != null;
        }

        public virtual Texture GetMainTexture(Graphic graphic)
        {
            return graphic.mainTexture;
        }

        public virtual float GetAlpha(Graphic graphic)
        {
            return graphic.color.a;
        }

        public virtual void SetMaterialDirty(Graphic graphic)
        {
            graphic.SetMaterialDirty();
        }
    }
}
