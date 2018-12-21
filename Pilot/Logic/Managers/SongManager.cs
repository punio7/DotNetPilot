using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Pilot.Hubs;
using Pilot.Models;
using TagLib;

namespace Pilot.Logic.Managers
{
    public class SongManager
    {
        public SongInfo CurrentSong { get; set; }
        public static SongManager Instance;

        private FileSystemWatcher watcher;
        private SongInfo emptySong;
        private IHubContext<PilotHub> pilotHubContext;

        private SongManager(string nowPlayingFilePath, IHubContext<PilotHub> pilotHubContext)
        {
            emptySong = new SongInfo()
            {
                Title = string.Empty,
                Artist = string.Empty,
                Album = string.Empty,
                Image = System.IO.File.ReadAllBytes(@"wwwroot\images\noImage.png"),
                ImageMimeType = "image/png",
                StartTime = DateTime.Now,
                Length = 0,
                CurrentPosition = 0,
            };
            CurrentSong = emptySong;
            this.pilotHubContext = pilotHubContext;
            CreateWatcher(nowPlayingFilePath);
        }

        public static void CreateInstance(string nowPlayingFilePath, IHubContext<PilotHub> pilotHubContext)
        {
            if (Instance != null)
            {
                return;
            }
            Instance = new SongManager(nowPlayingFilePath, pilotHubContext);
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
                    DateTime editTime = System.IO.File.GetLastWriteTime(e.FullPath);
                    ProcessSongChange(nowPlayingContent, editTime);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
            }
        }

        void ProcessSongChange(string songPath, DateTime playStartTime)
        {
            if (!System.IO.File.Exists(songPath))
            {
                CurrentSong = emptySong;
                SendSignalRAlert();
                return;
            }
            var tagFile = TagLib.File.Create(songPath);
            if (tagFile.Tag == null)
            {
                CurrentSong = emptySong;
                SendSignalRAlert();
                return;
            }
            SongInfo newSongInfo = new SongInfo()
            {
                Title = tagFile.Tag.Title,
                Artist = tagFile.Tag.FirstAlbumArtist,
                Album = tagFile.Tag.Album,
                Length = (int)tagFile.Properties.Duration.TotalSeconds,
                StartTime = playStartTime,
            };
            var picture = SelectPictureFromArray(tagFile.Tag.Pictures);
            ProcessSongPicture(newSongInfo, picture);

            CurrentSong = newSongInfo;
            SendSignalRAlert();
        }

        private void SendSignalRAlert()
        {
            pilotHubContext.Clients.All.SendAsync("songUpdate");
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
