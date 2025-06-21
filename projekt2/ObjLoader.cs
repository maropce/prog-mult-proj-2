using OpenTK.Graphics.OpenGL4;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace PMLabs
{
    public class Model
    {
        int vao, vbo, ebo, count;
        public vec3[] V; public vec3[] N; public vec2[] UV; public int[] E;
        public void SetupGL()
        {
            vao = GL.GenVertexArray(); vbo = GL.GenBuffer(); ebo = GL.GenBuffer();
            GL.BindVertexArray(vao);
            int vcount = V.Length;
            float[] buf = new float[vcount * 8];
            for (int i = 0; i < vcount; i++)
            {
                buf[i * 8 + 0] = V[i].x; buf[i * 8 + 1] = V[i].y; buf[i * 8 + 2] = V[i].z;
                buf[i * 8 + 3] = N[i].x; buf[i * 8 + 4] = N[i].y; buf[i * 8 + 5] = N[i].z;
                buf[i * 8 + 6] = UV[i].x; buf[i * 8 + 7] = UV[i].y;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, buf.Length * 4, buf, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, E.Length * 4, E, BufferUsageHint.StaticDraw);
            int stride = 8 * 4;
            GL.EnableVertexAttribArray(0); GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1); GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * 4);
            GL.EnableVertexAttribArray(2); GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * 4);
            GL.BindVertexArray(0);
            count = E.Length;
        }
        public void Draw() { GL.BindVertexArray(vao); GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, 0); }
    }

    public static class ObjLoader
    {
        public static Model Load(string file)
        {
            var V = new List<vec3>(); var N = new List<vec3>(); var UV = new List<vec2>();
            var vI = new List<int>(); var vtI = new List<int>(); var vnI = new List<int>();

            foreach (var l in File.ReadAllLines(file))
            {
                if (string.IsNullOrWhiteSpace(l)) continue;
                if (l.StartsWith("#")) continue;

                var s = l.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (s.Length == 0) continue;

                if (s[0] == "v" && s.Length >= 4)
                    V.Add(new vec3(
                        float.Parse(s[1], CultureInfo.InvariantCulture),
                        float.Parse(s[2], CultureInfo.InvariantCulture),
                        float.Parse(s[3], CultureInfo.InvariantCulture)));

                else if (s[0] == "vn" && s.Length >= 4)
                    N.Add(new vec3(
                        float.Parse(s[1], CultureInfo.InvariantCulture),
                        float.Parse(s[2], CultureInfo.InvariantCulture),
                        float.Parse(s[3], CultureInfo.InvariantCulture)));

                else if (s[0] == "vt" && s.Length >= 3)
                    UV.Add(new vec2(
                        float.Parse(s[1], CultureInfo.InvariantCulture),
                        float.Parse(s[2], CultureInfo.InvariantCulture)));

                else if (s[0] == "f" && s.Length >= 4)
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        var parts = s[i].Split('/');
                        if (parts.Length >= 3)
                        {
                            vI.Add(int.Parse(parts[0]) - 1);
                            vtI.Add(int.Parse(parts[1]) - 1);
                            vnI.Add(int.Parse(parts[2]) - 1);
                        }
                    }
                }
            }

            var m = new Model();
            int n = vI.Count;
            m.V = new vec3[n]; m.UV = new vec2[n]; m.N = new vec3[n];
            m.E = new int[n];
            for (int i = 0; i < n; i++)
            {
                m.V[i] = V[vI[i]];
                if (vtI[i] >= 0 && vtI[i] < UV.Count)
                    m.UV[i] = UV[vtI[i]];
                else
                    m.UV[i] = new vec2(0, 0);

                if (vnI[i] >= 0 && vnI[i] < N.Count)
                    m.N[i] = N[vnI[i]];
                else
                    m.N[i] = new vec3(0, 0, 1);

                m.E[i] = i;
            }

            return m;
        }
    }
}
