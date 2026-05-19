namespace main
{
    public interface IGunAbility
    {
        string AbilityName { get; }
        bool CanActivate { get; }
        float Cooldown { get; }
        void Activate();
        
        float GetTimeLeft();
    }
}
