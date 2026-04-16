using Godot;
using System;
namespace main
{
	//Creates body as Character2D
	public partial class Body : CharacterBody2D
	{
		[Signal]
        public delegate void BodyDestroyedEventHandler();
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
		public int _hp { get; private set; } = 100;
		private Label _hpLabel;

		private Timer _resetTimer;

		private Timer _increasedHealtTimer;

		private Rect2 MapBounds;
		private Camera2D _camera;



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

			SetDeferred("collision_layer", 1);
			SetDeferred("collision_mask", 2 | 3 | 5);
			Position = _Position;


			Sprite2D sprite = new Sprite2D();
			sprite.Texture = GD.Load<Texture2D>(TexturePath);
			sprite.Modulate = Color;
			sprite.Centered = true;

			// sprite.Scale = Size / sprite.Texture.GetSize();
			AddChild(sprite);

			

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

			Sprite2D map = GetTree().CurrentScene.GetNode<Sprite2D>("Sprite2D");

			Vector2 mapPos = map.GlobalPosition;
			Vector2 mapSize = map.Texture.GetSize() * map.Scale;

			MapBounds = new Rect2(mapPos - mapSize / 2f, mapSize);

			_camera = new Camera2D();
			_camera.Enabled = IsMultiplayerAuthority();

			_camera.LimitLeft = (int)MapBounds.Position.X;
			_camera.LimitTop = (int)MapBounds.Position.Y;
			_camera.LimitRight = (int)(MapBounds.Position.X + MapBounds.Size.X);
			_camera.LimitBottom = (int)(MapBounds.Position.Y + MapBounds.Size.Y);

			AddChild(_camera);

			// hitbox.BodyEntered += OnBulletHit;

			// AddChild(hitbox);

			_resetTimer = new Timer();
			_resetTimer.WaitTime = 5.0;
			_resetTimer.OneShot = true;
			_resetTimer.Timeout += OnResetTimer;
			AddChild(_resetTimer);

			BodyDodge bodyAbility = new BodyDodge();
			AddChild(bodyAbility);

			_increasedHealtTimer = new Timer();
			_increasedHealtTimer.WaitTime = 5.0;
			_increasedHealtTimer.OneShot = true;
			_increasedHealtTimer.Timeout += OnIncreasedHealthTimer;
			AddChild(_increasedHealtTimer);

			BodyHeal bodyHeal = new BodyHeal();
			bodyHeal.HealBody += IncreaceHealth; 
			AddChild(bodyHeal);


		}



		// Called whenever a physics body enters the hitbox
		public void TakeDamage(int amount)
		{
			_hp -= amount;
			_hpLabel.Text = _hp.ToString();

			if (_hp <= 0)
			{
				EmitSignal(nameof(BodyDestroyed));
				QueueFree();
			}
		}
		


		private void OnResetTimer()
		{
			_hp = 100;
			_hpLabel.Text = "HP: 100";
		}

		private void OnIncreasedHealthTimer()
		{
			if (_hp > 100)
			{
				_hp = 100;
				_hpLabel.Text = "HP: 100";
			}
		}

		private void IncreaceHealth()
		{
			_hp = 150;
			_hpLabel.Text = "HP: 150";
			_increasedHealtTimer.Start();
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

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
     	TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
		private void SyncTransform(Vector2 pos, float rot, Vector2 labelPos, float labelRot)
		{
			Position = pos;
			Rotation = rot;
			_hpLabel.Position = labelPos;
			_hpLabel.Rotation = labelRot;
		}
		

	}
}
