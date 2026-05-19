using Godot;
using System;

namespace main
{
    public partial class CooldownUI : CanvasLayer
    {
        private Control _anchorContainer;
        private TextureProgressBar _radialBar;
        private Label _textLabel;
        private Timer _trackedTimer;
        private float _maxCooldown;
        private string _shortcutText;

        // Visual states colors
        private readonly Color _readyBgColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);      
        private readonly Color _readyProgressColor = new Color(0.2f, 0.8f, 0.2f, 0.8f); 
        private readonly Color _cooldownBgColor = new Color(0.4f, 0.1f, 0.1f, 0.4f);   
        private readonly Color _cooldownProgressColor = new Color(1f, 0.3f, 0.3f, 0.8f); 

        public enum ScreenCorner
        {
            BottomLeft,
            BottomRight
        }
        public static CooldownUI Create(Node parent, Timer timer, float maxCooldown, ScreenCorner corner, Vector2 offset, string shortcutText, float iconSize = 96f)
        {
            var ui = new CooldownUI(timer, maxCooldown, corner, offset, shortcutText, iconSize);
            parent.AddChild(ui);
            return ui;
        }


        private CooldownUI(Timer timer, float maxCooldown, ScreenCorner corner, Vector2 offset, string shortcutText, float iconSize)
        {
            _trackedTimer = timer;
            _maxCooldown = maxCooldown;
            _shortcutText = shortcutText;

            _anchorContainer = new Control();
            AddChild(_anchorContainer);

            if (corner == ScreenCorner.BottomLeft)
            {
                _anchorContainer.LayoutMode = 1;
                _anchorContainer.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
                _anchorContainer.Position = new Vector2(offset.X, -offset.Y - iconSize); 
            }
            else
            {
                _anchorContainer.LayoutMode = 1;
                _anchorContainer.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
                _anchorContainer.Position = new Vector2(-offset.X - iconSize, -offset.Y - iconSize);
            }

            Texture2D circleTexture = CreateCircleTexture((int)(iconSize / 2)); 

            _radialBar = new TextureProgressBar
            {
                Position = Vector2.Zero, 
                Size = new Vector2(iconSize, iconSize),
                MinValue = 0,
                MaxValue = maxCooldown,
                Step = 0.01f,
                FillMode = (int)TextureProgressBar.FillModeEnum.Clockwise,
                TextureUnder = circleTexture,
                TextureProgress = circleTexture
            };

            _textLabel = new Label
            {
                Size = _radialBar.Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = _shortcutText
            };
            
            _textLabel.AddThemeColorOverride("font_shadow_color", Colors.Black);
            _textLabel.AddThemeConstantOverride("shadow_offset_x", 1);
            _textLabel.AddThemeConstantOverride("shadow_offset_y", 1);

            _radialBar.AddChild(_textLabel);
            _anchorContainer.AddChild(_radialBar);
        }

        public override void _Process(double delta)
        {
            if (_trackedTimer == null || _trackedTimer.IsStopped())
            {
                _radialBar.TintUnder = _readyBgColor;
                _radialBar.TintProgress = _readyProgressColor;
                _radialBar.Value = _maxCooldown;
                _textLabel.Text = _shortcutText;
                return;
            }

            _radialBar.TintUnder = _cooldownBgColor;
            _radialBar.TintProgress = _cooldownProgressColor;
            
            float timeLeft = (float)_trackedTimer.TimeLeft;
            _radialBar.Value = timeLeft;
            _textLabel.Text = timeLeft.ToString("0.0");
        }

        private Texture2D CreateCircleTexture(int radius)
        {
            int size = radius * 2;
            var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - radius;
                    float dy = y - radius;
                    if ((dx * dx) + (dy * dy) <= radius * radius)
                    {
                        image.SetPixel(x, y, Colors.White);
                    }
                }
            }
            return ImageTexture.CreateFromImage(image);
        }
    }
}