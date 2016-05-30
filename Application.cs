using System;
using System.IO;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Diagnostics;

namespace Application
{
	public class OpenTKApp : GameWindow
	{
		static int screenID;
		static Raytracer raytracer;
		static bool terminated = false;
        private Camera camera;
        Stopwatch stopwatch = new Stopwatch();
        int processedframes = 0;

		protected override void OnLoad( EventArgs e )
		{
			// called upon app init
			GL.ClearColor( Color.Black );
			GL.Enable( EnableCap.Texture2D );
			GL.Disable( EnableCap.DepthTest );
			GL.Hint( HintTarget.PerspectiveCorrectionHint, HintMode.Nicest );
			ClientSize = new Size( 1024, 512 );
			raytracer = new Raytracer();
			raytracer.screen = new Surface( Raytracer.WIDTH, Raytracer.WIDTH);
			Sprite.target = raytracer.screen;
			screenID = raytracer.screen.GenTexture();
			camera = raytracer.Init();

            //start counting
            stopwatch.Start();
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
            //variabels for average framerate calculations
            processedframes += 1;            

            //update the camera position and direction
            cameraMovement();           
            
            //check if we have to exit
			if (terminated) 
			{
				Exit();
				return;
			}
            
            //renders the rays
            int box = Math.Min(Width, Height);
            GL.Viewport(0, 0, box, box);
            raytracer.Render();			
			
			// clear window contents            
            GL.Clear(ClearBufferMask.ColorBufferBit);

			// setup camera
			GL.MatrixMode( MatrixMode.Modelview );
			GL.LoadIdentity();
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadIdentity();

            //prepares to draw the screen texture
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Color3(1.0f, 1.0f, 1.0f);

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

            //reverts the needed properties to convert the screen to texture
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Texture2D);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            //renders the debug
            GL.PushAttrib(AttribMask.ViewportBit); //Push current viewport attributes to a sta ck
            GL.Viewport(Width >> 1, 0, box, box); //Create a new viewport bottom left for the debug output.
            raytracer.debugOutput();
            //we want to texture stuff again and restor our viewport
            
            GL.PopAttrib();//Reset to the old viewport.
            GL.Enable(EnableCap.Texture2D);
            //GL.Viewport(0, 0, , 512);
            //tell openTK we are gonna work on or next frame
            SwapBuffers();

            //write how long the frame took to complete
            Console.WriteLine(processedframes/(stopwatch.ElapsedMilliseconds * 0.001f));
		}

        public void cameraMovement()
        {
            var state = OpenTK.Input.Keyboard.GetState();
            Matrix4 my = Matrix4.CreateRotationY(-0.1f);
            Matrix4 my2 = Matrix4.CreateRotationY(0.1f);
            Matrix4 mx = Matrix4.CreateFromAxisAngle(camera.left, -0.1f);
            Matrix4 mx2 = Matrix4.CreateFromAxisAngle(camera.left, 0.1f);

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
                camera.position += camera.left;
            }
            if (state[Key.D])
            {
                camera.position -= camera.left;
            }
            if (state[Key.W])
            {
                camera.position += new Vector3(camera.direction.X, 0, camera.direction.Z);
            }
            if (state[Key.S])
            {
                camera.position -= new Vector3(camera.direction.X, 0, camera.direction.Z);
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
                if (camera.distancePlane < 1)
                    camera.distancePlane = 1;
            }   
            if(state[Key.KeypadPlus])
            {
                camera.viewDegree *= 1.25f;
            }
            if (state[Key.KeypadMinus])
            {
                camera.viewDegree /= 1.25f;
            }


            //camera.direction.Normalize();
            camera.UpdatePlane();
        }

		public static void Main( string[] args ) 
		{ 
			// entry point
			using (OpenTKApp app = new OpenTKApp()) { app.Run( 30.0f, 0.0f ); }
		}
	}
}