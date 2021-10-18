using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pilot.Models;

namespace Pilot.Logic.Managers
{
    public class LyricsManager
    {
        private static readonly string _nonBreakingSpace = "&nbsp;";

        internal Lyric[] ParseLyrics(string lyricsFilePath)
        {
            SortedDictionary<int, string> lyrics = new SortedDictionary<int, string>();
            var lines = File.ReadLines(lyricsFilePath);

            foreach (var line in lines)
            {
                ProcessLine(line, lyrics);
            }

            return LyricsArray(lyrics);
        }

        private void ProcessLine(string line, SortedDictionary<int, string> lyrics)
        {
            List<int> timestamps = new List<int>();
            bool inTag = false;
            string lyric = null;

            for (int i = 0; i < line.Length; i++)
            {
                char currentChar = line[i];
                if (currentChar == '[')
                {
                    inTag = true;
                }
                else if (currentChar == ']')
                {
                    inTag = false;
                }
                else
                {
                    if (inTag)
                    {
                        if (Char.IsDigit(currentChar))
                        {
                            timestamps.Add(ParseTimestamp(line.Substring(i, 8)));
                            i = i + 7;
                        }
                        else
                        {
                            //line contains metadata, skip it
                            return;
                        }
                    }
                    else //text found
                    {
                        lyric = line.Substring(i);
                        break;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(lyric))
            {
                lyric = _nonBreakingSpace;
            }

            foreach (var timestamp in timestamps)
            {
                lyrics[timestamp] = lyric;
            }
        }

        /// <param name="timestampString">Timestamp in 00:34.45 format</param>
        /// <returns>Timestamp in miliseconds</returns>
        private int ParseTimestamp(string timestampString)
        {
            int minutes = int.Parse(timestampString.Substring(0, 2));
            int seconds = int.Parse(timestampString.Substring(3, 2));
            int milliseconds = int.Parse(timestampString.Substring(6, 2)) * 10;
            return minutes * 60 * 1000 + seconds * 1000 + milliseconds;
        }

        private Lyric[] LyricsArray(SortedDictionary<int, string> lyrics)
        {
            return lyrics.Select(kv => new Lyric(kv.Key, kv.Value)).ToArray();
        }
    }
}
