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
			camera = new Camera(new Vector3(-50, 20, -10), new Vector3(1, 0, 0).Normalized(), SCREEN_WIDTH, SCREEN_HEIGHT, 1f);
			scene = Scene.BasicScene();
			pixels = camera.Frame(scene);
			screen.SetPixels(pixels);
		}

		public void Tick()
		{
            //Vector3 initialCameraPos = camera.position;
            //Vector3 initialCameraDirection = camera.direction;
            //var keyboard = OpenTK.Input.Keyboard.GetState();
            //if (keyboard[OpenTK.Input.Key.Up]) camera.position += Vector3.UnitY;
            //if (keyboard[OpenTK.Input.Key.Down]) camera.position -= Vector3.UnitY;
            //if (keyboard[OpenTK.Input.Key.Left]) camera.position -= Vector3.UnitX;
            //if (keyboard[OpenTK.Input.Key.Right]) camera.position += Vector3.UnitX;
            //if (keyboard[OpenTK.Input.Key.Q]) camera.position -= Vector3.UnitZ;
            //if (keyboard[OpenTK.Input.Key.E]) camera.position += Vector3.UnitZ;
            //if (keyboard[OpenTK.Input.Key.R]) camera.direction += Vector3.UnitX * 0.1f;
            //if (keyboard[OpenTK.Input.Key.T]) camera.direction -= Vector3.UnitX * 0.1f;
            //if (keyboard[OpenTK.Input.Key.F]) camera.direction += Vector3.UnitY * 0.1f;
            //if (keyboard[OpenTK.Input.Key.G]) camera.direction -= Vector3.UnitY * 0.1f;
            //if (keyboard[OpenTK.Input.Key.V]) camera.direction += Vector3.UnitZ * 0.1f;
            //if (keyboard[OpenTK.Input.Key.B]) camera.direction -= Vector3.UnitZ * 0.1f;

            //if(initialCameraPos != camera.position || initialCameraDirection != camera.direction)
            //{
            //    camera.direction = camera.direction.Normalized();
            //    Console.WriteLine(camera.position);
            //    Console.WriteLine(camera.direction);
            //    camera.UpdateCamera();
            //    pixels = camera.Frame(scene);
                screen.SetPixels(pixels);
            //}
		}

		public void Render()
		{
			//small.Draw(20, 20, 1f);
			// render stuff over the backbuffer (OpenGL, sprites)
		}
	}

} // namespace Template
