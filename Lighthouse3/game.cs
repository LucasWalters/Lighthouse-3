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
        public const float moveSpeed = 1f;
        public const float rotateSpeed = 25f;

        public Surface screen;
        //Sprite small;
        Camera camera;
		Scene scene;

        int frames = 0;
        float fps = 0f;
        float timePassed = 0f;

        bool showStats = true;
		public void Init()
		{
            //screen.Clear(0xcc33ff);
            //small = new Sprite("../../assets/small.png");

			scene = Scene.MirrorScene();
            camera = scene.mainCamera;
        }

		public void Tick(FrameEventArgs e)
		{
            if (HandleUserInput((float)e.Time))
            {
                camera.UpdateCamera();
            }
		}

        public void DebugRay(int x, int y)
        {
            camera.DebugRay(scene, x, y);
        }

		public void Render(FrameEventArgs e)
        {
            camera.MultithreadedFrame(scene);
            if (camera.pixelsChanged)
            {
                screen.SetPixels(camera.pixels);
                camera.pixelsChanged = false;
            }
            if (!showStats)
                return;

            timePassed += (float)e.Time;
            if (++frames > 10)
            {
                fps = frames / timePassed;
                frames = 0;
                timePassed = 0f;
            }

            float diagonal = camera.diagonalLength;
            float FOV = 2 * ((float)(Math.Atan(camera.diagonalLength / (2 * camera.screenDistance)) * (180 / Math.PI)));
            screen.Bar(0, 0, 300, 60, Color.ToARGB(Color.White));
            screen.Print("FPS: " + fps, 0, 0, 1);
            screen.Print("Pos: " + camera.position, 0, 15, 1);
            screen.Print("Rot: " + camera.direction, 0, 30, 1);
            screen.Print("FOV: " + FOV, 0, 45, 1);
			// render stuff over the backbuffer (OpenGL, sprites)
		}

        // Keybindings
        private bool HandleUserInput(float deltaTime)
        {
            bool keyPressed = false;
            var keyboard = Keyboard.GetState();
            if (keyboard[Key.Up]) { camera.RotateX(-rotateSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.Down]) { camera.RotateX(rotateSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.Left]) { camera.RotateY(-rotateSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.Right]) { camera.RotateY(rotateSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.T]) { camera.RotateZ(rotateSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.Z]) { camera.RotateZ(rotateSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.W]) { camera.MoveZ(moveSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.S]) { camera.MoveZ(-moveSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.A]) { camera.MoveX(moveSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.D]) { camera.MoveX(-moveSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.Q]) { camera.MoveY(moveSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.E]) { camera.MoveY(-moveSpeed * deltaTime); keyPressed = true; }
            if (keyboard[Key.F]) { camera.screenDistance = Calc.Clamp(camera.screenDistance - 0.5f * deltaTime, 0.25f, 10); keyPressed = true; }
            if (keyboard[Key.G]) { camera.screenDistance = Calc.Clamp(camera.screenDistance + 0.5f * deltaTime, 0.25f, 10); keyPressed = true; }
            if (keyboard[Key.H]) { showStats = false; }
            if (keyboard[Key.J]) { showStats = true; }
            return keyPressed;
        }
    }

} // namespace Template
