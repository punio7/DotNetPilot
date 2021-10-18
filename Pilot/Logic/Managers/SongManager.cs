using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Pilot.Hubs;
using Pilot.Logic.Configuration;
using Pilot.Models;
using TagLib;

namespace Pilot.Logic.Managers
{
    public class SongManager
    {
        public SongInfo CurrentSong { get; set; }
        public LyricsManager LyricsManager { get; set; }
        public static SongManager Instance;

        private FileSystemWatcher watcher;
        private readonly SongInfo emptySong;
        private readonly string lyricsPath;
        private readonly IHubContext<PilotHub> pilotHubContext;

        private SongManager(string nowPlayingFilePath, string lyricsPath, IHubContext<PilotHub> pilotHubContext)
        {
            emptySong = new SongInfo()
            {
                Path = string.Empty,
                Track = null,
                Title = string.Empty,
                Artist = string.Empty,
                Album = string.Empty,
                Year = null,
                Genere = string.Empty,
                Image = System.IO.File.ReadAllBytes(@"wwwroot\images\noImage.png"),
                ImageMimeType = "image/png",
                Length = 0,
                CurrentPosition = 0,
            };
            CurrentSong = emptySong;
            LyricsManager = new LyricsManager();
            this.lyricsPath = lyricsPath;
            this.pilotHubContext = pilotHubContext;
            CreateWatcher(nowPlayingFilePath);
        }

        public static void CreateInstance(IConfiguration configuration, IHubContext<PilotHub> pilotHubContext)
        {
            if (Instance != null)
            {
                return;
            }
            string nowPlayingFilePath = configuration.GetPilotConfig().NowPlayingFilePath;
            string lyricsPath = configuration.GetPilotConfig().LyricsPath;
            Instance = new SongManager(nowPlayingFilePath, lyricsPath, pilotHubContext);
        }

        private void CreateWatcher(string nowPlayingFilePath)
        {
            if (!System.IO.File.Exists(nowPlayingFilePath))
            {
                watcher = null;
            }
            else
            {
                watcher = new FileSystemWatcher();
                FileInfo fileInfo = new FileInfo(nowPlayingFilePath);
                watcher.Path = fileInfo.DirectoryName;
                watcher.Filter = fileInfo.Name;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Changed += NowPlayingFileChanged;
                watcher.EnableRaisingEvents = true;
                NowPlayingFileChanged(this, new FileSystemEventArgs(WatcherChangeTypes.All, fileInfo.DirectoryName, fileInfo.Name));
            }
        }

        private void NowPlayingFileChanged(object sender, FileSystemEventArgs e)
        {
            int numberOfTries = 0;
            while (numberOfTries++ < 10)
            {
                try
                {
                    string nowPlayingContent = System.IO.File.ReadAllText(e.FullPath);
                    ProcessSongChange(nowPlayingContent);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
            }
        }

        void ProcessSongChange(string nowPlayingContent)
        {
            var nowPlayingArray = nowPlayingContent.Split(Environment.NewLine);
            string songPath = nowPlayingArray[0];

            if (songPath == "n/a" || !System.IO.File.Exists(songPath))
            {
                CurrentSong = emptySong;
                SendSignalRAlert();
            }
            else if (CurrentSong.Path == songPath)
            {
                UpdateCurrentPosition(CurrentSong, nowPlayingArray);
                SendSignalRAlertPosition();
            }
            else
            {
                LoadNewSongInfo(nowPlayingArray, songPath); 
                SendSignalRAlert();
            }
        }

        private void LoadNewSongInfo(string[] nowPlayingArray, string songPath)
        {
            using (var tagFile = TagLib.File.Create(songPath))
            {
                if (tagFile.Tag == null)
                {
                    CurrentSong = emptySong;
                }
                else
                {
                    SongInfo newSongInfo = new SongInfo()
                    {
                        Path = songPath,
                        Track = tagFile.Tag.Track != default ? tagFile.Tag.Track : (uint?)null,
                        Title = GetTitle(tagFile),
                        Artist = GetAlbumArtist(tagFile),
                        Album = tagFile.Tag.Album,
                        Year = tagFile.Tag.Year != default ? tagFile.Tag.Year : (uint?)null,
                        Genere = tagFile.Tag.JoinedGenres,
                        Length = (int)tagFile.Properties.Duration.TotalSeconds,
                    };
                    UpdateCurrentPosition(newSongInfo, nowPlayingArray);
                    var picture = SelectPictureFromArray(tagFile.Tag.Pictures);
                    ProcessSongPicture(newSongInfo, picture);
                    ProcessLyrics(newSongInfo);

                    CurrentSong = newSongInfo;
                }
            }
        }

        private void ProcessLyrics(SongInfo songInfo)
        {
            if (string.IsNullOrEmpty(lyricsPath) || !Directory.Exists(lyricsPath))
            {
                return;
            }
            string lyricsFilePath = $"{lyricsPath}\\{songInfo.Artist} - {songInfo.Title}.lrc";
            if (!System.IO.File.Exists(lyricsFilePath))
            {
                return;
            }
            songInfo.Lyrics = LyricsManager.ParseLyrics(lyricsFilePath);
        }

        private static string GetTitle(TagLib.File tagFile)
        {
            if (!string.IsNullOrEmpty(tagFile.Tag.Title))
            {
                return tagFile.Tag.Title;
            }
            return tagFile.Name;
        }

        private static string GetAlbumArtist(TagLib.File tagFile)
        {
            if (!string.IsNullOrEmpty(tagFile.Tag.FirstAlbumArtist))
            {
                return tagFile.Tag.FirstAlbumArtist; 
            }
            else if (tagFile.Tag.AlbumArtists.Any())
            {
                return string.Join(",", tagFile.Tag.AlbumArtists);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete. AlbumArtists sometimes doesn't return Artists
                return string.Join(",", tagFile.Tag.Artists);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        private void UpdateCurrentPosition(SongInfo songInfo, string[] nowPlayingArray)
        {
            string currentPositionString = nowPlayingArray.Length > 1 ? nowPlayingArray[1] : null;
            int currentPosition = !string.IsNullOrEmpty(currentPositionString) ? int.Parse(currentPositionString) : 0;
            songInfo.CurrentPosition = currentPosition;
        }

        private void SendSignalRAlert()
        {
            pilotHubContext.Clients.All.SendAsync(PilotHub.ClientMethods.SongUpdate);
        }

        private void SendSignalRAlertPosition()
        {
            pilotHubContext.Clients.All.SendAsync(PilotHub.ClientMethods.SongUpdatePosition, CurrentSong.CurrentPosition);
        }

        IPicture SelectPictureFromArray(IPicture[] pictureArray)
        {
            if (pictureArray == null)
            {
                return null;
            }
            var frontCoverPicture = pictureArray.FirstOrDefault((p) => p.Type == PictureType.FrontCover);
            if (frontCoverPicture != null)
            {
                return frontCoverPicture;
            }
            else
            {
                return pictureArray.FirstOrDefault();
            }
        }

        private void ProcessSongPicture(SongInfo songInfo, IPicture picture)
        {
            if (picture == null || picture.Data == null)
            {
                songInfo.Image = emptySong.Image;
                songInfo.ImageMimeType = emptySong.ImageMimeType;
            }
            else
            {
                songInfo.Image = picture.Data.Data;
                songInfo.ImageMimeType = picture.MimeType;
            }
        }
    }
}
