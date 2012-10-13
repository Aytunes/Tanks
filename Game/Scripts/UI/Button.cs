using System.Collections.Generic;
using CryEngine;

namespace CryGameCode.UI
{
	public class Button
	{
		private static int TextureId = Renderer.LoadTexture("Textures/UI/button.dds");
		private static ButtonEntity Instance;

		public int Width { get; set; }
		public int Height { get; set; }
		public int XPos { get; set; }
		public int YPos { get; set; }
		public string Text { get; set; }

		public Button(string text, int xpos, int ypos, int width = 128, int height = 64)
		{
			if(Instance == null || Instance.IsDestroyed)
				Instance = Entity.Spawn<ButtonEntity>("ButtonHandler");

			Debug.LogAlways("Creating new button, {0}", text);

			Text = text;
			XPos = xpos;
			YPos = ypos;
			Width = width;
			Height = height;

			Instance.Buttons.Add(this);
		}

		[Entity(Flags = EntityClassFlags.Invisible)]
		private class ButtonEntity : Entity
		{
			public List<Button> Buttons { get; set; }

			public override void OnSpawn()
			{
				ReceiveUpdates = true;
				Buttons = new List<Button>();
			}

			public override void OnUpdate()
			{
				foreach(var button in Buttons)
				{
					Renderer.DrawTexture(button.XPos, button.YPos, button.Width, button.Height, TextureId);
					Renderer.DrawTextToScreen(button.XPos, button.YPos, 2, Color.White, button.Text);
				}
			}
		}
	}
}
