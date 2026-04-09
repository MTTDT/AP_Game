using Godot;
using System;
namespace main
{
	//Creates body as Character2D
	public partial class Body : CharacterBody2D
	{
		private string TexturePath { get; set; }
		private Color Color { get; set; }
		private Node Owner { get; set; }
		private Vector2 Size { get; set; }
		private Vector2 _Position { get; set; }

		[Export]
		public int Speed { get; set; } = 600;

		[Export]
		public float RotationSpeed { get; set; } = 5f;

		private float _rotationDirection;
		private int _hp = 100;
		private Label _hpLabel;

		private Timer _resetTimer;


		public Body(string texturePath, Color color, Node owner, Vector2 size, Vector2 position)
		{
			this.TexturePath = texturePath;
			this.Color = color;
			this.Owner = owner;
			this.Size = size;
			this._Position = position;



		}

		// Creates complete body
		public override void _Ready()
		{

			Position = _Position;

			CollisionLayer = 1;
			CollisionMask = 2;


			Sprite2D sprite = new Sprite2D();
			sprite.Texture = GD.Load<Texture2D>(TexturePath);
			sprite.Modulate = Color;
			// sprite.Scale = Size / sprite.Texture.GetSize();
			AddChild(sprite);

			if (IsMultiplayerAuthority())
			{
				Camera2D camera = new Camera2D();
				camera.Enabled = true;
				AddChild(camera);
			}


			Gun gun = new Gun("res://gun.svg", Color);
			gun.Name = "Gun";
			gun.SetMultiplayerAuthority(GetMultiplayerAuthority());
			AddChild(gun);



			_hpLabel = new Label();
			_hpLabel.Text = "HP: 100";
			_hpLabel.Position = new Vector2(-20f, -90f);

			AddChild(_hpLabel);





			Area2D hitbox = new Area2D();
			hitbox.Name = "Hitbox";

			hitbox.CollisionLayer = 2;
			hitbox.CollisionMask = 4;

			CollisionShape2D hitShape = new CollisionShape2D();
			RectangleShape2D square = new RectangleShape2D();
			square.Size = new Vector2(90f, 70f);
			hitShape.Shape = square;
			hitbox.AddChild(hitShape);

			hitbox.BodyEntered += OnBulletHit;

			AddChild(hitbox);

			_resetTimer = new Timer();
			_resetTimer.WaitTime = 5.0;
			_resetTimer.OneShot = true;
			_resetTimer.Timeout += OnResetTimer;
			AddChild(_resetTimer);
		}

		[Signal]
		public delegate void BodyDestroyedEventHandler();
		// Called whenever a physics body enters the hitbox
		private void OnBulletHit(Node body)
		{
			if (body is not Bullet) return;

			body.QueueFree();

			_hp = Mathf.Max(0, _hp - 10);
			_hpLabel.Text = $"HP: {_hp}";

			_resetTimer.Start();

			if (_hp <= 0)
			{
				GD.Print("Dead!");
				EmitSignal(SignalName.BodyDestroyed);
			}
		}
		


		private void OnResetTimer()
		{
			_hp = 100;
			_hpLabel.Text = "HP: 100";
		}

	

		// Accounts for inputs
		public void GetInput()
		{
			_rotationDirection = Input.GetAxis("a", "d");
			Velocity = Transform.Y * Input.GetAxis("w", "s") * Speed;
		}

		//Constantly waits for input and acounts for it
		public override void _PhysicsProcess(double delta)
		{

			// Only process movement for YOUR body
			if (!IsMultiplayerAuthority()) return;

			GetInput();
			Rotation += _rotationDirection * RotationSpeed * (float)delta;
			MoveAndSlide();

			Vector2 offset = new Vector2(-20f, -90f);
			_hpLabel.Position = offset.Rotated(-Rotation);
			_hpLabel.Rotation = -Rotation;
			// Sync our position and rotation to all other peers
			Rpc(nameof(SyncTransform), Position, Rotation, _hpLabel.Position, _hpLabel.Rotation);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
		private void SyncTransform(Vector2 pos, float rot)
		{
			Position = pos;
			Rotation = rot;
		}
		

	}
}
