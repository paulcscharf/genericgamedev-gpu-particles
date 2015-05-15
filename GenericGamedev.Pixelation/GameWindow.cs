using System;
using System.Collections.Generic;
using System.Diagnostics;
using amulware.Graphics;
using amulware.Graphics.ShaderManagement;
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
        private const int batchParticleCount = 1000;

        private Matrix4Uniform modelviewUniform;
        private Matrix4Uniform projectionUniform;

        private int glHeight;
        private int glWidth;

        private Stopwatch fpsTimer;
        private float time;
        private float nextSpawnTime;

        private IParticleSystem system = new DummyParticleSystem();

        private Dictionary<Key, Func<IParticleSystem>> systemMakers;
        private int frames;
        private int fps;

        public GameWindow()
            : base(1280, 720, GraphicsMode.Default,
            "GameDev<T> Particles",
            GameWindowFlags.Default, DisplayDevice.Default,
            3, 0, GraphicsContextFlags.ForwardCompatible)
        {
            this.SetVSync(false);
        }

        protected override void OnLoad(EventArgs e)
        {
            var acceleration = new Vector3(0, 0, -100);

            var shaderLoader = ShaderFileLoader.CreateDefault("shaders");

            var shaderMan = new ShaderManager();

            shaderMan.Add(shaderLoader.Load(""));

            #region gpuShader

            var gpuShader = shaderMan.BuildShaderProgram()
                .WithVertexShader("gpu-particle")
                .WithGeometryShader("gpu-particle")
                .WithFragmentShader("particle")
                .As("gpu-particle");

            var gpuSurface = new BatchedVertexSurface<GPUVertex>(PrimitiveType.Points);

            gpuSurface.AddSettings(
                new Vector3Uniform("acceleration", acceleration),
                this.modelviewUniform = new Matrix4Uniform("modelviewMatrix"),
                this.projectionUniform = new Matrix4Uniform("projectionMatrix")
                );

            gpuShader.UseOnSurface(gpuSurface);

            #endregion
            #region almostGpuShader

            var almostGpuShader = shaderMan.BuildShaderProgram()
                .WithVertexShader("almost-gpu-particle")
                .WithGeometryShader("gpu-particle")
                .WithFragmentShader("particle")
                .As("almost-gpu-particle");

            var almostGpuSurface = new VertexSurface<AlmostGPUVertex>(PrimitiveType.Points);

            almostGpuSurface.AddSettings(
                new Vector3Uniform("acceleration", acceleration),
                this.modelviewUniform,
                this.projectionUniform
                );

            almostGpuShader.UseOnSurface(almostGpuSurface);

            #endregion

            #region cpuQuadShader

            var cpuQuadShader = shaderMan.BuildShaderProgram()
                .WithVertexShader("quad-particle")
                .WithFragmentShader("particle")
                .As("cpu-quad");

            var cpuQuadSurface = new IndexedSurface<CPUQuadVertex>();

            cpuQuadSurface.AddSettings(
                this.modelviewUniform,
                this.projectionUniform
                );

            cpuQuadShader.UseOnSurface(cpuQuadSurface);
            
            #endregion

            #region cpuGeoShader

            var cpuGeoShader = shaderMan.BuildShaderProgram()
                .WithVertexShader("cpu-particle")
                .WithGeometryShader("gpu-particle")
                .WithFragmentShader("particle")
                .As("cpu-geo");

            var cpuGeoSurface = new VertexSurface<CPUGeoVertex>(PrimitiveType.Points);

            cpuGeoSurface.AddSettings(
                this.modelviewUniform,
                this.projectionUniform
                );

            cpuGeoShader.UseOnSurface(cpuGeoSurface);

            #endregion

            this.systemMakers = new Dictionary<Key, Func<IParticleSystem>>
            {
                { Key.Number1, () => new DummyParticleSystem() },
                { Key.Number2, () => new CPUSimpleParticles(cpuQuadSurface, this.time, acceleration) },
                { Key.Number3, () => new CPUParametricParticles(cpuQuadSurface, this.time, acceleration) },
                { Key.Number4, () => new CPUStructParticles(cpuQuadSurface, this.time, acceleration) },
                { Key.Number5, () => new CPUStructDirectVertexWriteParticles(cpuQuadSurface, this.time, acceleration) },
                { Key.Number6, () => new CPUStructDirectVertexWritePointParticles(cpuGeoSurface, this.time, acceleration) },
                { Key.Number7, () => new AlmostGPUParticles(almostGpuSurface, this.time) },
                { Key.Number0, () => new GPUParticles(gpuSurface, this.time, batchParticleCount) },
            };

            InputManager.Initialize(this.Mouse);


            this.fpsTimer = Stopwatch.StartNew();
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
            this.frames++;

            if (this.fpsTimer.Elapsed > TimeSpan.FromSeconds(1))
            {
                this.fpsTimer.Restart();
                this.fps = this.frames;
                this.frames = 0;
            }

            InputManager.Update();

            this.time += e.ElapsedTimeInSf;

            var angle = this.time * 0.5f;

            this.modelviewUniform.Matrix = Matrix4.LookAt(
                new Vector3(0, 0, 30) + new Vector3(GameMath.Cos(angle), GameMath.Sin(angle), 0.2f) * 110,
                new Vector3(0, 0, 30), Vector3.UnitZ
                );

            foreach (var systemMaker in this.systemMakers)
            {
                if (InputManager.IsKeyHit(systemMaker.Key))
                {
                    this.system.Dispose();
                    this.system = systemMaker.Value();
                    this.fps = 0;
                    this.fpsTimer.Restart();
                }
            }

            while (this.nextSpawnTime <= this.time)
            {
                this.system.SpawnParticleGroup(batchParticleCount);
                this.nextSpawnTime += 0.01f;
            }

            this.system.Update(this.time);

            this.Title = string.Format(
                "GameDev<T> Particles: {0} | ~{1}k particles | ~{2} fps",
                this.system.Name, this.system.AliveParticles / batchParticleCount, this.fps);
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

            this.system.Draw();

            this.SwapBuffers();
        }
    }
}
