using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    internal static class GraphicConnectorExtension
    {
        public static void SetVerticesDirtyEx(this Graphic graphic)
        {
            GraphicConnector.FindConnector(graphic).SetVerticesDirty(graphic);
        }

        public static void SetMaterialDirtyEx(this Graphic graphic)
        {
            GraphicConnector.FindConnector(graphic).SetMaterialDirty(graphic);
        }

        public static T GetComponentInParentEx<T>(this Component component, bool includeInactive = false) where T : MonoBehaviour
        {
            if (!component) return null;
            var trans = component.transform;

            while (trans)
            {
                var c = trans.GetComponent<T>();
                if (c && (includeInactive || c.isActiveAndEnabled)) return c;

                trans = trans.parent;
            }

            return null;
        }
    }


    public class GraphicConnector
    {
        private static readonly List<GraphicConnector> s_Connectors = new List<GraphicConnector>();
        private static readonly Dictionary<Type, GraphicConnector> s_ConnectorMap = new Dictionary<Type, GraphicConnector>();
        private static readonly GraphicConnector s_EmptyConnector = new GraphicConnector();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            AddConnector(new GraphicConnector());
        }

        protected static void AddConnector(GraphicConnector connector)
        {
            s_Connectors.Add(connector);
            s_Connectors.Sort((x, y) => y.priority - x.priority);
        }

        public static GraphicConnector FindConnector(Graphic graphic)
        {
            if (!graphic) return s_EmptyConnector;

            var type = graphic.GetType();
            GraphicConnector connector = null;
            if (s_ConnectorMap.TryGetValue(type, out connector)) return connector;

            foreach (var c in s_Connectors)
            {
                if (!c.IsValid(graphic)) continue;

                s_ConnectorMap.Add(type, c);
                return c;
            }

            return s_EmptyConnector;
        }

        /// <summary>
        /// Connector priority.
        /// </summary>
        protected virtual int priority
        {
            get { return -1; }
        }

        /// <summary>
        /// The connector is valid for the component.
        /// </summary>
        protected virtual bool IsValid(Graphic graphic)
        {
            return true;
        }

        public virtual void SetVerticesDirty(Graphic graphic)
        {
            if (graphic)
                graphic.SetVerticesDirty();
        }

        public virtual void SetMaterialDirty(Graphic graphic)
        {
            if (graphic)
                graphic.SetMaterialDirty();
        }
    }
}
