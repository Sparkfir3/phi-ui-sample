using UnityEngine;
using UnityEngine.UI;

namespace Sparkfire.UI
{
    public class ParallelogramImage : MaskableGraphic
    {
        [SerializeField, Range(0f, 90f)]
        private float targetAngle = 75;

        /* Parallelogram Points:
         *     1-------3---4
         *    /|       |  /
         *   / |       | /
         *  /  |       |/
         * 0---2-------5
         *     x1      x2
         * [–b–] <- length of triangle base
         */
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
            float b = r.height / Mathf.Tan(targetAngle * Mathf.Deg2Rad);
            float x1 = Mathf.Clamp(v.x + b, v.x, v.x + r.width / 2f);
            float x2 = Mathf.Clamp(v.z - b, v.x + r.width / 2f, v.z);

            Color32 color32 = color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(x1, v.w), color32, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(x1, v.y), color32, new Vector2((b / r.width), 0f));
            vh.AddVert(new Vector3(x2, v.w), color32, new Vector2((1f - b / r.width), 0f));
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(x2, v.y), color32, new Vector2(1f, 1f));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(3, 4, 5);
            vh.AddTriangle(1, 2, 3);
            vh.AddTriangle(2, 3, 5);
        }
    }
}
