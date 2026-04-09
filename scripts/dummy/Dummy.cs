using Godot;
using System;

namespace main
{
	public partial class Dummy : CharacterBody2D
	{
		private string TexturePath { get; set; }
		private Vector2 _Position { get; set; }
		private int _health = 100;

		private Label _hpLabel;
		private Area2D _hitbox;

		public Dummy(string texturePath, Vector2 position)
		{
			TexturePath = texturePath;
			_Position = position;
		}

		public override void _Ready()
		{
			Position = _Position;

			CollisionLayer = 3; // dummy
			CollisionMask = 1;  // detecta jugador si quieres

			// Sprite
			Sprite2D sprite = new Sprite2D();
			sprite.Texture = GD.Load<Texture2D>(TexturePath);
			AddChild(sprite);

			// Cuerpo físico del dummy
			CollisionShape2D bodyCol = new CollisionShape2D();
			bodyCol.Shape = new RectangleShape2D { Size = new Vector2(40, 60) };
			AddChild(bodyCol);

			// Hitbox para detectar balas
			_hitbox = new Area2D();
			_hitbox.CollisionLayer = 3; // dummy
			_hitbox.CollisionMask = 4;  // balas
			AddChild(_hitbox);

			CollisionShape2D hitboxCol = new CollisionShape2D();
			hitboxCol.Shape = new RectangleShape2D { Size = new Vector2(40, 60) };
			_hitbox.AddChild(hitboxCol);

			_hitbox.AreaEntered += OnAreaEntered;

			// Label de vida
			_hpLabel = new Label();
			_hpLabel.Position = new Vector2(-20, -60);
			AddChild(_hpLabel);
			UpdateHealthText();
		}

		private void OnAreaEntered(Area2D area)
		{
			if (area is Bullet bullet)
			{
				_health -= 10;
				UpdateHealthText();
				bullet.QueueFree();

				if (_health <= 0)
					QueueFree();
			}
		}

		private void UpdateHealthText()
		{
			_hpLabel.Text = $"HP: {_health}";
			if (_health < 30)
				_hpLabel.Modulate = Colors.Red;
		}
	}
}
