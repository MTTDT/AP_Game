namespace main
{
    public class Player
    {
        public long Id { get; private set; }
        public Body Body { get; private set; }
        public string Name { get; set; }
        public BodyType BodyType { get; set; } = BodyType.Default;

        public Player(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public Player(long id, Body body, string name)
        {
            Id = id;
            Body = body;
            Name = name;
        }
    }
}