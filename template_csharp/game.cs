using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Lighthouse3 {

	class Game
	{
		public Surface screen;
		Sprite small;
		public void Init()
		{
			screen.Clear( 0x2222ff );
			small = new Sprite("../../assets/small.png");
			Camera cam = new Camera(new Vector3(0, 20, 0), new Vector3(0, 0, -1f).Normalized(), new Vector2(500, 500), 1f);
		}
		public void Tick()
		{
			screen.Print( "hello world!", 2, 2, 0xffffff );
		}
		public void Render()
		{
			small.Draw(20, 20, 1f);
			// render stuff over the backbuffer (OpenGL, sprites)
		}
	}

} // namespace Template
