namespace Pilot.Models
{
    public class Lyric
    {
        public int Milliseconds { get; set; }
        public string Text { get; set; }

        public Lyric(int milliseconds, string text)
        {
            Milliseconds = milliseconds;
            Text = text;
        }
    }
}
