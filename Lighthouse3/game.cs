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

			scene = Scene.MirrorScene();
			pixels = scene.mainCamera.Frame(scene);
			screen.SetPixels(pixels);
            camera = scene.mainCamera;
        }

		public void Tick()
		{
            Vector3 initialCameraPos = camera.position;
            Vector3 initialCameraDirection = camera.direction;


            if (HandleUserInput())
            {
            //    camera.direction = camera.direction.Normalized();
            //    Console.WriteLine(camera.position);
            //    Console.WriteLine(camera.direction);
                camera.UpdateCamera();
                pixels = camera.Frame(scene);
                screen.SetPixels(pixels);
            }
		}

		public void Render()
		{
            float diagonal = camera.diagonalLength;
            float FOV = 2 * ((float)(Math.Atan(camera.diagonalLength / 2 * camera.screenDistance) * (180 / Math.PI)));

            screen.Print("Pos: " + camera.position, 0, 0, 1);
            screen.Print("Rot: " + camera.direction, 0, 15, 1);
            screen.Print("FOV: " + FOV, 0, 30, 1);
			// render stuff over the backbuffer (OpenGL, sprites)
		}

        // Keybindings
        private bool HandleUserInput()
        {
            bool keyPressed = false;
            var keyboard = OpenTK.Input.Keyboard.GetState();
            if (keyboard[OpenTK.Input.Key.Up]) { camera.RotateX(-5); keyPressed = true; }
            if (keyboard[OpenTK.Input.Key.Down]) { camera.RotateX(5); keyPressed = true; }
            if (keyboard[OpenTK.Input.Key.Left]) { camera.RotateY(-5); keyPressed = true; }
            if (keyboard[OpenTK.Input.Key.Right]) { camera.RotateY(5); keyPressed = true; }
            if (keyboard[OpenTK.Input.Key.W]) { camera.MoveY(0.25f); keyPressed = true; }
            if (keyboard[OpenTK.Input.Key.S]) { camera.MoveY(-0.25f); keyPressed = true; }
            if (keyboard[OpenTK.Input.Key.A]) { camera.MoveX(0.25f); keyPressed = true; }
            if (keyboard[OpenTK.Input.Key.D]) { camera.MoveX(-0.25f); keyPressed = true; }
            if (keyboard[OpenTK.Input.Key.Q]) { camera.MoveZ(0.25f); keyPressed = true; }
            if (keyboard[OpenTK.Input.Key.E]) { camera.MoveZ(-0.25f); keyPressed = true; }
            if (keyboard[OpenTK.Input.Key.F]) { camera.screenDistance = Calc.Clamp(camera.screenDistance - 0.25f, 0.25f, 10); keyPressed = true; }
            if (keyboard[OpenTK.Input.Key.G]) { camera.screenDistance = Calc.Clamp(camera.screenDistance + 0.25f, 0.25f, 10); keyPressed = true; }
            return keyPressed;
        }
    }

} // namespace Template
