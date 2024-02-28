namespace TravkinGames.Saves
{
    public interface IGameData
    {
        public int Version { get; set; }
        
        public bool OnboardingFinished { get; set; }
        public bool SoundsEnabled { get; set; }
        public bool MusicEnabled { get; set; }
        public string Language { get; set; }
        
        public void Initialize();
    }
}