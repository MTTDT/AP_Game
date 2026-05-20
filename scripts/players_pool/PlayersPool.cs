using Godot;

namespace main
{
    public partial class PlayersPool : Node
    {
        private ItemList _itemList;
        private PlayersManager _playersManager;
        private Button _startGameBtn;

        private readonly Color PanelBgColor = new Color(0.12f, 0.15f, 0.20f, 0.75f);
        private readonly Color AccentColor = new Color(0.25f, 0.60f, 0.95f, 1.00f);
        private readonly Color TextMutedColor = new Color(0.70f, 0.75f, 0.80f, 1.00f);

        public override void _Ready()
        {
            _itemList = GetNode<ItemList>("ItemList");
            _playersManager = GetNode<PlayersManager>("/root/PlayersManager");
            _startGameBtn = GetNode<Button>("Button");

            if (GameState.Role == GameState.NetworkRole.Server)
            {
                _startGameBtn.Pressed += OnStartGamePressed;
                StyleHostButton(_startGameBtn, true);
            }
            else
            {
                _startGameBtn.Disabled = true;
                _startGameBtn.Text = "Waiting for host...";
                StyleHostButton(_startGameBtn, false);
            }

            StyleItemList(_itemList);

            SetupModernSelectionUI();

            _playersManager.Players.OnPlayersChanged += RefreshList;
            RefreshList();
        }

        private void SetupModernSelectionUI()
        {
            float listRightEdge = _itemList.Position.X + _itemList.Size.X;
            if (listRightEdge <= 0)
            {
                listRightEdge = 340f;
            }

            var selectionVBox = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(480, 0)
            };
            selectionVBox.AddThemeConstantOverride("separation", 20);
            AddChild(selectionVBox);

            selectionVBox.AnchorsPreset = (int)Control.LayoutPreset.CenterLeft;

            selectionVBox.OffsetLeft = listRightEdge + 40f;
            selectionVBox.OffsetTop = -210f;
            selectionVBox.OffsetRight = listRightEdge + 40f + 480f;
            selectionVBox.OffsetBottom = 210f;

            var bodyHBox = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            bodyHBox.AddThemeConstantOverride("separation", 15);

            var bodySection = CreateSectionPanel("Select Your Body", bodyHBox);
            selectionVBox.AddChild(bodySection);

            AddSelectionCard(bodyHBox, "Default", BodyType.Default, "Balanced", "100 HP\nAbility: Heal", true);
            AddSelectionCard(bodyHBox, "Bulky", BodyType.Bulky, "Tanky", "150 HP\nAbility: Durability", true);
            AddSelectionCard(bodyHBox, "Snappy", BodyType.Snappy, "Fast", "70 HP\nAbility: Speed Boost", true);

            var gunHBox = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            gunHBox.AddThemeConstantOverride("separation", 15);

            var gunSection = CreateSectionPanel("Select Your Weapon", gunHBox);
            selectionVBox.AddChild(gunSection);

            AddSelectionCard(gunHBox, "Default", GunType.Default, "Balanced", "Rate: 0.5s\n[E] Giant Bullet", false);
            AddSelectionCard(gunHBox, "Sniper", GunType.Sniper, "Long Range", "Rate: 1.2s\n[E] Pierce Shot", false);
            AddSelectionCard(gunHBox, "Machine Gun", GunType.MachineGun, "Rapid Fire", "Rate: 0.15s\n[E] Overdrive", false);

            if (_startGameBtn.GetParent() != null) _startGameBtn.GetParent().RemoveChild(_startGameBtn);
            AddChild(_startGameBtn);
            _startGameBtn.AnchorsPreset = (int)Control.LayoutPreset.BottomRight;
            _startGameBtn.OffsetLeft = -250;
            _startGameBtn.OffsetTop = -80;
            _startGameBtn.OffsetRight = -40;
            _startGameBtn.OffsetBottom = -30;
        }


        private VBoxContainer CreateSectionPanel(string title, Control contentNode)
        {
            var containerWrapper = new VBoxContainer();

            var label = new Label
            {
                Text = title.ToUpper(),
                ThemeTypeVariation = "HeaderMedium"
            };
            label.AddThemeColorOverride("font_color", AccentColor);
            label.AddThemeConstantOverride("outline_size", 2);
            containerWrapper.AddChild(label);

            var panel = new PanelContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            var styleBox = new StyleBoxFlat
            {
                BgColor = PanelBgColor,
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8,
                ContentMarginLeft = 15,
                ContentMarginTop = 15,
                ContentMarginRight = 15,
                ContentMarginBottom = 15
            };
            panel.AddThemeStyleboxOverride("panel", styleBox);

            panel.AddChild(contentNode);
            containerWrapper.AddChild(panel);

            return containerWrapper;
        }

        private void AddSelectionCard(HBoxContainer parent, string title, object enumType,
                                      string subtitle, string details, bool isBody)
        {
            var cardVBox = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(130, 0)
            };

            var btn = new Button
            {
                Text = title,
                CustomMinimumSize = new Vector2(0, 40),
                MouseDefaultCursorShape = Control.CursorShape.PointingHand
            };

            if (isBody)
            {
                var bodyType = (BodyType)enumType;
                btn.Pressed += () =>
                {
                    _playersManager.SelectBodyType(bodyType);
                    GD.Print($"Selected body type: {bodyType}");
                };
            }
            else
            {
                var gunType = (GunType)enumType;
                btn.Pressed += () =>
                {
                    _playersManager.SelectGunType(gunType);
                    GD.Print($"Selected gun type: {gunType}");
                };
            }

            var subLabel = new Label
            {
                Text = subtitle,
                HorizontalAlignment = HorizontalAlignment.Center,
                ThemeTypeVariation = "HeaderSmall"
            };

            var detailsLabel = new Label
            {
                Text = details,
                HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            detailsLabel.AddThemeColorOverride("font_color", TextMutedColor);
            detailsLabel.AddThemeFontSizeOverride("font_size", 13);

            cardVBox.AddChild(btn);
            cardVBox.AddChild(subLabel);
            cardVBox.AddChild(detailsLabel);
            parent.AddChild(cardVBox);
        }

        private void StyleItemList(ItemList list)
        {
            var styleBox = new StyleBoxFlat
            {
                BgColor = new Color(0.08f, 0.10f, 0.14f, 0.60f),
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8,
                ContentMarginLeft = 15,
                ContentMarginTop = 15,
                ContentMarginRight = 15,
                ContentMarginBottom = 15
            };
            list.AddThemeStyleboxOverride("panel", styleBox);
        }

        private void StyleHostButton(Button btn, bool isActive)
        {
            var styleBox = new StyleBoxFlat
            {
                BgColor = isActive ? AccentColor : new Color(0.2f, 0.25f, 0.3f, 0.8f),
                CornerRadiusTopLeft = 20,
                CornerRadiusTopRight = 20,
                CornerRadiusBottomLeft = 20,
                CornerRadiusBottomRight = 20
            };
            btn.AddThemeStyleboxOverride("normal", styleBox);
            btn.AddThemeStyleboxOverride("disabled", styleBox);
            btn.AddThemeColorOverride("font_color", Colors.White);
        }


        private void RefreshList()
        {
            _itemList.Clear();
            foreach (var player in _playersManager.Players.Players.Values)
            {
                string bodyTag = player.BodyType switch
                {
                    BodyType.Bulky => "[Bulky]",
                    BodyType.Snappy => "[Snappy]",
                    _ => "[Default]"
                };
                string gunTag = player.GunType switch
                {
                    GunType.Sniper => "[Sniper]",
                    GunType.MachineGun => "[MachineGun]",
                    _ => "[DefaultGun]"
                };
                _itemList.AddItem($"{player.Name}  {bodyTag}  {gunTag}");
            }
        }

        public override void _ExitTree()
        {
            if (_playersManager != null)
                _playersManager.Players.OnPlayersChanged -= RefreshList;
        }

        private void OnStartGamePressed() => _playersManager.StartGame();

        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("quit"))
            {
                _playersManager.Players.OnPlayersChanged -= RefreshList;
                _playersManager.PlayerQuit();
            }
        }
    }
}