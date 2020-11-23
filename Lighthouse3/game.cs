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
		Scene scene;
		int[] pixels;
		public void Init()
		{
			//screen.Clear(0xcc33ff);
			//small = new Sprite("../../assets/small.png");
			float mult = -20000f;
			Vector3 white = Color.White;
			Color4 color = new Color4(white.X, white.Y, white.Z, 1);
			Console.WriteLine(color);
			Console.WriteLine(color.ToArgb());
			color = new Color4(white.X * mult, white.Y * mult, white.Z * mult, 1);
			Console.WriteLine(color);
			Console.WriteLine(color.ToArgb());


			scene = Scene.MirrorScene();
			pixels = scene.mainCamera.Frame(scene);
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
