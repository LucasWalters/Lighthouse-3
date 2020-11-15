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
		Sphere[] spheres;
		int[] pixels;
		public void Init()
		{
			//screen.Clear(0xcc33ff);
			//small = new Sprite("../../assets/small.png");
			camera = new Camera(new Vector3(0, 0, 0), new Vector3(0, 0, 1), SCREEN_WIDTH, SCREEN_HEIGHT, 1.5f);

			spheres = new Sphere[]
			{ 
				//new Sphere(new Vector3(4, 0, 7), 2),
				new Sphere(new Material(), new Vector3(-3, 3.5f, 8), 1.5f),
				//new Sphere(new Vector3(0, 0, 20), 3f)
				//new Sphere(new Vector3(0, 5, 20), 1),
				//new Sphere(new Vector3(3, 3, 10), 4)
			};
			pixels = camera.Frame(spheres);
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
