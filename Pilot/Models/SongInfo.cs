namespace Pilot.Models
{
    public class SongInfo
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public byte[] Image { get; set; }
        public string ImageMimeType { get; set; }
        public int Length { get; set; }
        public int CurrentPosition { get; set; }
        public string Path { get; internal set; }
        public uint? Track { get; internal set; }
        public uint? Year { get; internal set; }
        public string Genere { get; internal set; }
        public Lyric[] Lyrics { get; set; }
    }
}
