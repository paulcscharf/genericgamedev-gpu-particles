using System;
using amulware.Graphics;
using amulware.Graphics.ShaderManagement;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Input;
using Bearded.Utilities.Math;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace GenericGamedev.Pixelation
{
    sealed class GameWindow : amulware.Graphics.Program
    {
        private BatchedVertexSurface<ParticleVertex> surface;
        private Matrix4Uniform modelviewUniform;
        private Matrix4Uniform projectionUniform;

        private int glHeight;
        private int glWidth;

        private readonly DeletableObjectList<ParticleSystem> particleSystems = new DeletableObjectList<ParticleSystem>();

        private float time;
        private float particleTime;

        public GameWindow()
            : base(1280, 720, GraphicsMode.Default,
            "GameDev<T> Gpu Particles",
            GameWindowFlags.Default, DisplayDevice.Default,
            3, 0, GraphicsContextFlags.ForwardCompatible)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            var shaderLoader = ShaderFileLoader.CreateDefault("shaders");

            var shaderMan = new ShaderManager();

            shaderMan.Add(shaderLoader.Load(""));

            var shaderProgram = shaderMan.MakeShaderProgram("particle");

            this.surface = new BatchedVertexSurface<ParticleVertex>(PrimitiveType.Points);

            this.surface.AddSettings(
                new Vector3Uniform("acceleration", new Vector3(0, 0, -100)),
                this.modelviewUniform = new Matrix4Uniform("modelviewMatrix"),
                this.projectionUniform = new Matrix4Uniform("projectionMatrix")
                );

            shaderProgram.UseOnSurface(this.surface);

            InputManager.Initialize(this.Mouse);
        }

        private void createProjectionMatrix(int width, int height)
        {
            const float zNear = 0.1f;
            const float zFar = 256f;
            const float fovy = (float)Math.PI / 4;

            float ratio = (float)width / height;

            float yMax = zNear * (float)Math.Tan(0.5f * fovy);
            float yMin = -yMax;
            float xMin = yMin * ratio;
            float xMax = yMax * ratio;

            this.projectionUniform.Matrix = Matrix4.CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
        }

        protected override void OnUpdate(UpdateEventArgs e)
        {
            InputManager.Update();

            this.time += e.ElapsedTimeInSf;

            var angle = this.time * 0.5f;

            this.modelviewUniform.Matrix = Matrix4.LookAt(
                new Vector3(0, 0, 30) + new Vector3(GameMath.Cos(angle), GameMath.Sin(angle), 0.2f) * 110,
                new Vector3(0, 0, 30), Vector3.UnitZ
                );

            if (!InputManager.IsKeyPressed(Key.Space))
            {
                if (InputManager.IsKeyPressed(Key.S))
                {
                    this.particleTime += e.ElapsedTimeInSf * 0.4f;
                }
                else
                {
                    this.particleTime += e.ElapsedTimeInSf;
                }
            }

            if (InputManager.IsKeyHit(Key.W) || InputManager.IsKeyPressed(Key.E))
            {
                this.particleSystems.Add(new ParticleSystem(particleTime, this.surface));
            }

            foreach (var particleSystem in this.particleSystems)
            {
                particleSystem.Update(particleTime);
            }

            this.Title = "GameDev<T> Gpu Particles ~"
                         + (this.surface.ActiveBatches * ParticleSystem.Count) / 1000
                         + "k particles";
        }

        protected override void OnRender(UpdateEventArgs e)
        {
            if (this.Height != this.glHeight || this.Width != this.glWidth)
            {
                this.glHeight = this.Height;
                this.glWidth = this.Width;
                GL.Viewport(0, 0, this.glWidth, this.glHeight);
                this.createProjectionMatrix(this.glWidth, this.glHeight);
            }

            GL.ClearColor(Color.Silver);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.CullFace(CullFaceMode.FrontAndBack);
            SurfaceBlendSetting.PremultipliedAlpha.Set(null);

            this.surface.Render();

            this.SwapBuffers();
        }
    }
}
