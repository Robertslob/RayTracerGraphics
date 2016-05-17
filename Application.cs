﻿using System;
using System.IO;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Application
{
	public class OpenTKApp : GameWindow
	{
		static int screenID;
		static Raytracer raytracer;
		static bool terminated = false;
        private Camera camera;

		protected override void OnLoad( EventArgs e )
		{
			// called upon app init
			GL.ClearColor( Color.Black );
			GL.Enable( EnableCap.Texture2D );
			GL.Disable( EnableCap.DepthTest );
			GL.Hint( HintTarget.PerspectiveCorrectionHint, HintMode.Nicest );
			raytracer = new Raytracer();
			raytracer.screen = new Surface( Width, Height );
			Sprite.target = raytracer.screen;
			screenID = raytracer.screen.GenTexture();
			camera = raytracer.Init();
		}

		protected override void OnUnload( EventArgs e )
		{
			// called upon app close
			GL.DeleteTextures( 1, ref screenID );
			Environment.Exit( 0 ); // bypass wait for key on CTRL-F5
		}

		protected override void OnResize( EventArgs e )
		{
			// called upon window resize
			GL.Viewport(0, 0, Width, Height);
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadIdentity();
			GL.Ortho( -1.0, 1.0, -1.0, 1.0, 0.0, 4.0 );
		}

		protected override void OnUpdateFrame( FrameEventArgs e )
		{
			// called once per frame; app logic
			var keyboard = OpenTK.Input.Keyboard.GetState();
			if (keyboard[OpenTK.Input.Key.Escape]) this.Exit();
		}

        // called once per frame
		protected override void OnRenderFrame( FrameEventArgs e )
		{
            //we process user input to move the camera
            cameraMovement();
			//we render with our raytracer
			raytracer.Render();

            // tell OpenTK we're done rendering
            SwapBuffers();

			if (terminated) 
			{
				Exit();
				return;
			}
			// convert Game.screen to OpenGL texture
			GL.BindTexture( TextureTarget.Texture2D, screenID );
			GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
						   raytracer.screen.width, raytracer.screen.height, 0, 
						   OpenTK.Graphics.OpenGL.PixelFormat.Bgra, 
						   PixelType.UnsignedByte, raytracer.screen.pixels 
						 );
			// clear window contents
			GL.Clear( ClearBufferMask.ColorBufferBit );
			// setup camera
			GL.MatrixMode( MatrixMode.Modelview );
			GL.LoadIdentity();
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadIdentity();
			// draw screen filling quad
            GL.Color3(0, 0, 0);
			GL.Begin( PrimitiveType.Quads );
			GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex2( -1.0f, -1.0f );
			GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex2(  1.0f, -1.0f );
			GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex2(  1.0f,  1.0f );
			GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex2( -1.0f,  1.0f );
			GL.End();
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Disable(EnableCap.Texture2D);
            GL.Clear(ClearBufferMask.DepthBufferBit);

			
		}

        public void cameraMovement()
        {
            var state = OpenTK.Input.Keyboard.GetState();
            Matrix4 m = Matrix4.CreateRotationY(-0.02f);
            Matrix4 m2 = Matrix4.CreateRotationY(0.02f);
            if (state[Key.Q])
            {
                camera.direction = Vector3.Transform(camera.direction, m);
            }
            if (state[Key.E])
            {
                camera.direction = Vector3.Transform(camera.direction, m2);
            }
            if (state[Key.R])
            {
                camera.distancePlane += 0.5f;
            }
            if (state[Key.F])
            {
                camera.distancePlane -= 0.5f;
                if (camera.distancePlane < 1) camera.distancePlane = 1;
            }
            if (state[Key.A])
            {
                camera.position -= Vector3.UnitX;
            }
            if (state[Key.D])
            {
                camera.position += Vector3.UnitX;
            }
            if (state[Key.W])
            {
                camera.position += Vector3.UnitZ;
            }
            if (state[Key.S])
            {
                camera.position -= Vector3.UnitZ;
            }
            camera.direction.Normalize();
            camera.UpdatePlane();
        }

		public static void Main( string[] args ) 
		{ 
			// entry point
			using (OpenTKApp app = new OpenTKApp()) { app.Run( 30.0, 0.0 ); }
		}
	}
}