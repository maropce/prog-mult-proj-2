using GLFW;
using GlmSharp;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Shaders;
using System;
using System.Drawing;

namespace PMLabs
{
    public class BC : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName) =>
            Glfw.GetProcAddress(procName);
    }

    class Program
    {
        static ShaderProgram shader;
        static Model model;
        static float rotX, rotY;
        static KeyCallback kc = KeyProcessor;

        public static void KeyProcessor(IntPtr w, Keys key, int sc, InputState st, ModifierKeys mk)
        {
            if (st == InputState.Press || st == InputState.Repeat)
            {
                switch (key)
                {
                    case Keys.Up: rotX -= 5; break;
                    case Keys.Down: rotX += 5; break;
                    case Keys.Left: rotY -= 5; break;
                    case Keys.Right: rotY += 5; break;
                }
            }
        }

        public static int ReadTexture(string fn, TextureUnit tu = TextureUnit.Texture0)
        {
            int tex = GL.GenTexture();
            GL.ActiveTexture(tu);
            GL.BindTexture(TextureTarget.Texture2D, tex);
            Bitmap bmp = new Bitmap(fn);
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          data.Width, data.Height, 0,
                          PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data); bmp.Dispose();
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            return tex;
        }

        static void Init(Window win)
        {
            GL.ClearColor(0, 0, 0, 1);
            shader = new ShaderProgram("v_shader.glsl", "f_shader.glsl");
            Glfw.SetKeyCallback(win, kc);
            GL.Enable(EnableCap.DepthTest);

            model = ObjLoader.Load("../../../M.obj");
            model.SetupGL();

            vec3 centroid = new vec3(0, 0, 0);
            int count = model.V.Length;

            for (int i = 0; i < count; i++)
            {
                centroid += model.V[i];
            }
            centroid /= count;

            for (int i = 0; i < count; i++)
            {
                model.V[i] -= centroid;
            }

            model.SetupGL();


            int t = ReadTexture("../../../marble.jpg", TextureUnit.Texture0);
            shader.Use();
            GL.Uniform1(shader.U("tex0"), 0);
        }

        static void Draw(Window win)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            float w = 800, h = 600;
            var P = mat4.Perspective(glm.Radians(50f), w / h, 0.1f, 100f);
            var V = mat4.LookAt(new vec3(0, 1, 5), new vec3(0, 0, 0), new vec3(0, 1, 0));

            float scale = 0.02f;
            var M = mat4.Scale(new vec3(scale)) * mat4.RotateX(glm.Radians(rotX)) * mat4.RotateY(glm.Radians(rotY));

            shader.Use();
            GL.UniformMatrix4(shader.U("P"), 1, false, P.Values1D);
            GL.UniformMatrix4(shader.U("V"), 1, false, V.Values1D);
            GL.UniformMatrix4(shader.U("M"), 1, false, M.Values1D);

           
            float t = (float)Glfw.Time;
            vec3 c1 = new vec3(0.078f, 0.118f, 0.314f);  // #141e50
            vec3 c2 = new vec3(0.0f, 0.423f, 0.980f);  // #006cfa
            vec3 c3 = new vec3(0.137f, 0.741f, 1.0f);    // #23bdff

            float phase = (t % 6f) / 2f;
            vec3 clr;

            if (phase < 1f)
                clr = (1 - phase) * c1 + phase * c2;
            else if (phase < 2f)
                clr = (2 - phase) * c2 + (phase - 1f) * c3;
            else
                clr = (3 - phase) * c3 + (phase - 2f) * c1;

            GL.Uniform3(shader.U("lightColor"), clr.x, clr.y, clr.z);

            vec3 lightPos = new vec3(5, 5, 5);
            GL.Uniform3(shader.U("lightPos"), lightPos.x, lightPos.y, lightPos.z);


            model.Draw();
            Glfw.SwapBuffers(win);
        }

        static void Main()
        {
            Glfw.Init();
            var win = Glfw.CreateWindow(800, 600, "Litera M", GLFW.Monitor.None, Window.None);
            Glfw.MakeContextCurrent(win);
            Glfw.SwapInterval(1);

            GL.LoadBindings(new BC());
            Init(win);

            Glfw.Time = 0;
            while (!Glfw.WindowShouldClose(win))
            {
                Draw(win);
                Glfw.PollEvents();
            }

            Glfw.Terminate();
        }
    }
}
