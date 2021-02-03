using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
 
namespace Lighthouse3
{
	public class OpenTKApp : GameWindow
	{
		static int screenID;
		static Game game;
		protected override void OnLoad( EventArgs e )
		{
			// called upon app init
			GL.ClearColor( Color4.Black );
			GL.Enable( EnableCap.Texture2D );
			GL.Hint( HintTarget.PerspectiveCorrectionHint, HintMode.Nicest );
			ClientSize = new Size(Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT);
			//Fix screen too big
			Game.SCREEN_WIDTH = Width;
			Game.SCREEN_HEIGHT = Height;
			game = new Game(this);
			game.screen = new Surface( Width, Height );
			Sprite.target = game.screen;
			screenID = game.screen.GenTexture();
			game.Init();
		}

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            game.DebugRay(e.X, e.Y);
        }

        protected override void OnUnload(EventArgs e)
		{
			// called upon app close
			GL.DeleteTextures( 1, ref screenID );
		}
		protected override void OnResize(EventArgs e)
		{
			// called upon window resize
			GL.Viewport(0, 0, Width, Height);
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadIdentity();
			GL.Ortho( -1.0, 1.0, -1.0, 1.0, 0.0, 4.0 );
		}
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			// called once per frame; app logic
			var keyboard = OpenTK.Input.Keyboard.GetState();
			if (keyboard[OpenTK.Input.Key.Escape]) this.Exit();
            game.Tick(e);
        }
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			// called once per frame; render
			GL.BindTexture( TextureTarget.Texture2D, screenID );
			GL.TexImage2D( TextureTarget.Texture2D, 
						   0, 
						   PixelInternalFormat.Rgba, 
						   game.screen.width, 
						   game.screen.height, 
						   0, 
						   OpenTK.Graphics.OpenGL.PixelFormat.Bgra, 
						   PixelType.UnsignedByte, 
						   game.screen.pixels 
						 );
			GL.Clear( ClearBufferMask.ColorBufferBit );
			GL.MatrixMode( MatrixMode.Modelview );
			GL.LoadIdentity();
			GL.BindTexture( TextureTarget.Texture2D, screenID );
			GL.Begin( PrimitiveType.Quads );
			GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex2( -1.0f, -1.0f );
			GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex2(  1.0f, -1.0f );
			GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex2(  1.0f,  1.0f );
			GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex2( -1.0f,  1.0f );
			GL.End();
			game.Render(e);
			SwapBuffers();
		}
		[STAThread]
		public static void Main() 
		{ 
			// entry point
			using (OpenTKApp app = new OpenTKApp()) 
			{
				double fps = 30;
				//app.VSync = OpenTK.VSyncMode.Off;
				app.Run(fps, 0); 
			} 
		}
	}
}