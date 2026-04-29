using Godot;

namespace main
{
    public interface IBodyAbility
    {
        string AbilityName { get; }

        bool CanActivate { get; }

        float Cooldown { get; }

        void Activate();
    }
}