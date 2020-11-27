﻿using System;
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
            CheckUserInput();


            if (initialCameraPos != camera.position || initialCameraDirection != camera.direction)
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
			//small.Draw(20, 20, 1f);
			// render stuff over the backbuffer (OpenGL, sprites)
		}

        // Keybindings
        private void CheckUserInput()
        {
            var keyboard = OpenTK.Input.Keyboard.GetState();
            if (keyboard[OpenTK.Input.Key.Up]) camera.RotateX(-5);
            if (keyboard[OpenTK.Input.Key.Down]) camera.RotateX(5);
            if (keyboard[OpenTK.Input.Key.Left]) camera.RotateY(-5);
            if (keyboard[OpenTK.Input.Key.Right]) camera.RotateY(5);
            if (keyboard[OpenTK.Input.Key.W]) camera.MoveY(0.25f);
            if (keyboard[OpenTK.Input.Key.S]) camera.MoveY(-0.25f);
            if (keyboard[OpenTK.Input.Key.A]) camera.MoveX(-0.25f);
            if (keyboard[OpenTK.Input.Key.D]) camera.MoveX(0.25f);
            if (keyboard[OpenTK.Input.Key.Q]) camera.MoveZ(0.25f);
            if (keyboard[OpenTK.Input.Key.E]) camera.MoveZ(-0.25f);
            //if (keyboard[OpenTK.Input.Key.G]) camera.direction -= Vector3.UnitY * 0.1f;
            //if (keyboard[OpenTK.Input.Key.V]) camera.direction += Vector3.UnitZ * 0.1f;
            //if (keyboard[OpenTK.Input.Key.B]) camera.direction -= Vector3.UnitZ * 0.1f;
        }
    }

} // namespace Template
