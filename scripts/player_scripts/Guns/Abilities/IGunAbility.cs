namespace main
{
    /// <summary>
    /// Common contract for all gun abilities (big bullet, wall-pierce, rapid-fire burst).
    /// Each ability node implements this and is added as a child of its Gun.
    /// </summary>
    public interface IGunAbility
    {
        string AbilityName { get; }
        bool CanActivate { get; }
        float Cooldown { get; }
        void Activate();
        
        float GetTimeLeft();
    }
}
