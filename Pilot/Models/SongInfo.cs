using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public DateTime StartTime { get; set; }
        public int CurrentPosition { get; set; }
    }
}
