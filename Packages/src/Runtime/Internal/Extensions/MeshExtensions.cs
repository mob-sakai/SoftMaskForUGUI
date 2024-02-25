using UnityEngine;

namespace Coffee.UISoftMaskInternal
{
    internal static class MeshExtensions
    {
        internal static readonly ObjectPool<Mesh> s_MeshPool = new ObjectPool<Mesh>(
            () =>
            {
                var mesh = new Mesh
                {
                    hideFlags = HideFlags.DontSave | HideFlags.NotEditable
                };
                mesh.MarkDynamic();
                return mesh;
            },
            mesh => mesh,
            mesh =>
            {
                if (mesh)
                {
                    mesh.Clear();
                }
            });

        public static Mesh Rent()
        {
            return s_MeshPool.Rent();
        }

        public static void Return(ref Mesh mesh)
        {
            s_MeshPool.Return(ref mesh);
        }

        public static void CopyTo(this Mesh self, Mesh dst)
        {
            if (!self || !dst) return;

            var vector3List = ListPool<Vector3>.Rent();
            var vector4List = ListPool<Vector4>.Rent();
            var color32List = ListPool<Color32>.Rent();
            var intList = ListPool<int>.Rent();

            dst.Clear(false);

            self.GetVertices(vector3List);
            dst.SetVertices(vector3List);

            self.GetTriangles(intList, 0);
            dst.SetTriangles(intList, 0);

            self.GetNormals(vector3List);
            dst.SetNormals(vector3List);

            self.GetTangents(vector4List);
            dst.SetTangents(vector4List);

            self.GetColors(color32List);
            dst.SetColors(color32List);

            self.GetUVs(0, vector4List);
            dst.SetUVs(0, vector4List);

            self.GetUVs(1, vector4List);
            dst.SetUVs(1, vector4List);

            self.GetUVs(2, vector4List);
            dst.SetUVs(2, vector4List);

            self.GetUVs(3, vector4List);
            dst.SetUVs(3, vector4List);

            dst.RecalculateBounds();
            ListPool<Vector3>.Return(ref vector3List);
            ListPool<Vector4>.Return(ref vector4List);
            ListPool<Color32>.Return(ref color32List);
            ListPool<int>.Return(ref intList);
        }
    }
}
