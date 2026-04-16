using Godot;

public partial class Dummy : CharacterBody2D
{
	private string _texturePath;
	private Vector2 _startPos;

	public int Health { get; private set; } = 100;

	public Dummy() {}

	public Dummy(string texturePath, Vector2 startPos)
	{
		_texturePath = texturePath;
		_startPos = startPos;
	}

	public override void _Ready()
	{
		Position = _startPos;

		SetDeferred("collision_layer", 2);
		SetDeferred("collision_mask", 1 | 4);

		Sprite2D sprite = new Sprite2D();
		sprite.Texture = GD.Load<Texture2D>(_texturePath);
		AddChild(sprite);

		CollisionShape2D shape = new CollisionShape2D();
		RectangleShape2D rect = new RectangleShape2D();
		rect.Size = new Vector2(64, 64);
		shape.Shape = rect;
		AddChild(shape);
	}

	public void TakeDamage(int amount)
	{
		Health -= amount;

		if (Health <= 0)
			QueueFree();
	}
}
