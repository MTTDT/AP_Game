using Godot;
using System;

namespace main
{
	public partial class Dummy : StaticBody2D
	{
		private string TexturePath { get; set; }
		private Vector2 _Position { get; set; }
		private int _health = 100;

		private Label _hpLabel;

		public Dummy(string texturePath, Vector2 position)
		{
			TexturePath = texturePath;
			_Position = position;
		}

		public override void _Ready()
		{
			Position = _Position;

			SetDeferred("collision_layer", 2);
			SetDeferred("collision_mask", 1 | 4);

			Sprite2D sprite = new Sprite2D();
			sprite.Texture = GD.Load<Texture2D>(TexturePath);
			AddChild(sprite);

			CollisionShape2D col = new CollisionShape2D();
			RectangleShape2D shape = new RectangleShape2D();
			shape.Size = new Vector2(40, 60);
			col.Shape = shape;
			AddChild(col);

			Area2D hitbox = new Area2D();
			hitbox.CollisionLayer = 0; 
			hitbox.CollisionMask = 4;  

			CollisionShape2D hitShape = new CollisionShape2D();
			hitShape.Shape = new RectangleShape2D() { Size = new Vector2(40, 60) };
			hitbox.AddChild(hitShape);

			hitbox.BodyEntered += OnBulletHit;
			AddChild(hitbox);

			_hpLabel = new Label();
			_hpLabel.Position = new Vector2(-20, -60);
			AddChild(_hpLabel);
			UpdateHealthText();
		}

		private void OnBulletHit(Node2D body)
		{
			if (body is Bullet bullet)
			{
				_health -= bullet.Damage;
				UpdateHealthText();
				bullet.QueueFree();

				if (_health <= 0)
					QueueFree();
			}
		}

		private void UpdateHealthText()
		{
			_hpLabel.Text = $"HP: {_health}";
		}
	}
}
