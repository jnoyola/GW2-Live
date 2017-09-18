namespace GW2_Live.GameInterface
{
    public struct CharacterIdentity
    {
        #pragma warning disable 0649
        // These fields are assigned by Json deserialization.

        public string name;
        public int profession;
        public int race;
        public int map_id;
        public int world_id;
        public int team_color_id;
        public bool commander;
        public double fov;

        #pragma warning restore 0649
    }
}
