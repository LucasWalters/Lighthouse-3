using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Lighthouse3.Primitives;

namespace Lighthouse3 {

	class Game
	{
		public static int SCREEN_WIDTH = 960;
		public static int SCREEN_HEIGHT = 540;
		public Surface screen;
		//Sprite small;
		Camera camera;
		Scene scene;
		int[] pixels;
		public void Init()
		{
			//screen.Clear(0xcc33ff);
			//small = new Sprite("../../assets/small.png");
			camera = new Camera(new Vector3(0, 0, 0), new Vector3(1f, 0.0005f, 0).Normalized(), SCREEN_WIDTH, SCREEN_HEIGHT, 1f);
			scene = Scene.BasicScene();
			pixels = camera.Frame(scene);
			screen.SetPixels(pixels);
		}
		public void Tick()
		{
			//screen.Print( "hello world!", 2, 2, 0xffffff );
			//screen.Plot(1, 1, 0xffffff);
			screen.SetPixels(pixels);
		}
		public void Render()
		{
			//small.Draw(20, 20, 1f);
			// render stuff over the backbuffer (OpenGL, sprites)
		}
	}

} // namespace Template
