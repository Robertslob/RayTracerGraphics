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
			ClientSize = new Size( 1024, 512 );
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
        // called once per frame; render
		protected override void OnRenderFrame( FrameEventArgs e )
		{
            //update the camera position and direction
            cameraMovement();           
            
            //check if we have to exit
			if (terminated) 
			{
				Exit();
				return;
			}
            
            //renders the rays
            raytracer.Render();			
			
			// clear window contents            
            GL.Clear(ClearBufferMask.ColorBufferBit);

			// setup camera
			GL.MatrixMode( MatrixMode.Modelview );
			GL.LoadIdentity();
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadIdentity();

            //does something i don't know
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Color3(1f, 1f, 1f);

            // convert Game.screen to OpenGL texture
            GL.BindTexture(TextureTarget.Texture2D, screenID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                           raytracer.screen.width, raytracer.screen.height, 0,
                           OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                           PixelType.UnsignedByte, raytracer.screen.pixels
                         );


            // draw screen filling quad
			GL.Begin( PrimitiveType.Quads );
			GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex2( -1.0f, -1.0f );
			GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex2(  1.0f, -1.0f );
			GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex2(  1.0f,  1.0f );
            GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex2( -1.0f,  1.0f );
			GL.End();

            //reverts the stuff i don't know
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Texture2D);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            //renders the debug
            raytracer.debugOutput();

            //tell openTK we are gonna work on or next frame
            SwapBuffers();
		}

        public void cameraMovement()
        {
            var state = OpenTK.Input.Keyboard.GetState();
            Matrix4 my = Matrix4.CreateRotationY(-0.02f);
            Matrix4 my2 = Matrix4.CreateRotationY(0.02f);
            Matrix4 mx = Matrix4.CreateRotationX(-0.02f);
            Matrix4 mx2 = Matrix4.CreateRotationX(0.02f);

            //rotate the camera with the pijltjestoetsen
            if (state[Key.Left])
            {
                camera.direction = Vector3.Transform(camera.direction, my);
            }
            if (state[Key.Right])
            {
                camera.direction = Vector3.Transform(camera.direction, my2);
            }
            if (state[Key.Up])
            {
                camera.direction = Vector3.Transform(camera.direction, mx);
            }
            if (state[Key.Down])
            {
                camera.direction = Vector3.Transform(camera.direction, mx2);
            }

            //move the camera in 2 dimensions with WASD
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

            //change the camera height with Z and X
            if (state[Key.Z])
            {
                camera.position += Vector3.UnitY;
            }
            if (state[Key.X])
            {
                camera.position -= Vector3.UnitY;
            }

            //zoom in and out with E and Q
            if (state[Key.E])
            {
                camera.distancePlane += 0.5f;
            }
            if (state[Key.Q])
            {
                camera.distancePlane -= 0.5f;
                if (camera.distancePlane < 1) camera.distancePlane = 1;
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