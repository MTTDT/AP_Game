using Godot;
using System;
namespace main
{
    
    //Creates Dummy as Area2D node
    public partial class Dummy : Area2D
    {
        private String TexturePath { get; set; }
        private Vector2 _Position { get; set; }
        private int _health = 100;

        private Label _hpLabel;

        //Constructor
        public Dummy(string texturePath, Vector2 position)
        {
            this.TexturePath = texturePath;

            this._Position = position;

        }

        //Creates complete Dummy 
        public override void _Ready()
        {

            Position = _Position;
            CollisionLayer = 2;
            CollisionMask = 4;

            BodyEntered += OnBodyEntered;

            Sprite2D sprite = new Sprite2D();
            sprite.Texture = GD.Load<Texture2D>(TexturePath);
            // sprite.Scale = Size / sprite.Texture.GetSize();
            AddChild(sprite);

            CollisionShape2D col = new CollisionShape2D();
            RectangleShape2D shape = new RectangleShape2D();
            shape.Size = new Vector2(40, 60);
            col.Shape = shape;
            AddChild(col);

            _hpLabel = new Label();
            UpdateHealthText();

            _hpLabel.Position = new Vector2(-20, -60);
            _hpLabel.HorizontalAlignment = HorizontalAlignment.Center;

            AddChild(_hpLabel);




        }

        //Checks if any bullets are inside the collision area and acounts for health
        private void OnBodyEntered(Node2D body)
        {
            if (body is Bullet bullet)
            {
                _health -= 10;
                UpdateHealthText();

                bullet.QueueFree();
                GD.Print("Dummy hit! HP: ", _health);

                if (_health <= 0)
                    QueueFree();
            }
        }


        //Updates health label.
        private void UpdateHealthText()
        {
            if (_hpLabel != null)
            {
                _hpLabel.Text = $"HP: {_health}";

                // Optional: Change color to red when low health
                if (_health < 30)
                {
                    _hpLabel.Modulate = Colors.Red;
                }
            }
        }
    }
}
