namespace Palantir.CoreLib.Models.Entities
{
    public class User
    {
        public long Id { get; set; }
        public List<string> Names { get; set; } = new List<string>();
        public List<string> Bios { get; set; } = new List<string>();
        public List<string> Usernames { get; set; } = new List<string>();
        public string? Name => Names.LastOrDefault();
        public string? Username => Usernames.LastOrDefault();
        public string? Bio => Bios.LastOrDefault();
    }
}
