using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Template {

	class Game
	{
		public Surface screen;
		Sprite small;
		public void Init()
		{
			screen.Clear( 0x2222ff );
			small = new Sprite("../../assets/small.png");
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
