var signalRConnection;
var songCurrentPosition = 0;
var songLength = 1;
var karaokeTimeout = null;
var karaokeCurrentLine = 0;
var karaokeLineCount = 0;
var karaokeLyrics = null;
var lyricsOffset = 500;

$(document).ready(() => {
    $("#mediaButton").click((e) => {
        ajaxAction('LaunchMedia');
    });
    $("#playButton").click((e) => {
        ajaxAction('Play');
    });
    $("#prevButton").click((e) => {
        ajaxAction('Previous');
    });
    $("#nextButton").click((e) => {
        ajaxAction('Next');
    });
    $("#stopButton").click((e) => {
        ajaxAction('Stop');
    });
    $("#infoButton").click((e) => {
        updateSongInfo();
    });

    updateSongInfo();

    signalRConnection = new signalR.HubConnectionBuilder().withUrl("/pilotHub").build();

    signalRConnection.on("songUpdate", () => {
        updateSongInfo();
    });
    signalRConnection.on("songUpdatePosition", (newPosition) => {
        songCurrentPosition = newPosition;
        updateSongProgress();
    });

    signalRConnection.start().catch(function (err) {
        return console.error(err.toString());
    });
});

function ajaxAction(actionName, callback) {
    var ajaxOptions = {
        url: '/PilotLocal/' + actionName
    };
    if (callback !== undefined) {
        ajaxOptions.success = callback;
    }
    else {
        ajaxOptions.success = ajaxOnSuccess;
    }

    $.ajax(ajaxOptions);
}

function ajaxOnSuccess(value) {
    console.info('ajax action: ' + value);
}

function updateSongInfo() {
    ajaxAction('Info', songUpdateCallback);
}

function songUpdateCallback(songInfo) {
    updateTagField("#songTitle", songInfo.title);
    updateTagField("#songArtist", songInfo.artist);
    updateTagField("#songAlbum", songInfo.album);
    updateTagField("#songTrack", songInfo.track);
    updateTagField("#songYear", songInfo.year);
    updateTagField("#songGenere", songInfo.genere);
    d = new Date();
    $('#songImage').attr('src', '/PilotLocal/Image?' + d.getTime());
    songLength = songInfo.length;
    songCurrentPosition = songInfo.currentPosition;
    karaokeLyrics = songInfo.lyrics;
    loadKaraoke();

    if (songLength > 0) {
        document.title = "▶ " + songInfo.title + " - Pilot";
        updateSongProgress();
        startKaraoke();
    }
    else {
        document.title = "⏹ Zatrzymany - Pilot";
        resetSongProgress();
        resetKaraoke();
    }
}

function updateTagField(selector, value) {
    if (value === undefined || value === null || value === "") {
        $(selector).parent().hide();
        $(selector).val("");
    }
    else {
        $(selector).parent().show();
        $(selector).val(value);
    }
}

function updateSongProgress() {
    var percentage = (songCurrentPosition / songLength) * 100;
    $('#songProgress').css('width', percentage + '%');
    $('#songTime').text(formatSeconds(songCurrentPosition) + ' / ' + formatSeconds(songLength));
}

function resetSongProgress() {
    $('#songProgress').css('width', '0%');
    $('#songTime').text('');
}

function formatSeconds(totalSeconds) {
    var minutes = Math.floor(totalSeconds / 60);
    var seconds = (totalSeconds % 60)
    if (seconds < 10) {
        seconds = '0' + seconds;
    }
    return minutes + ':' + seconds;
}

function loadKaraoke() {
    var karaokeBox = $('#karaokeBox');
    karaokeBox.empty();
    $('#songImage').removeClass('karaoke-background');
    if (karaokeLyrics != null) {
        $('#songImage').addClass('karaoke-background');
        karaokeBox.append('<div style="height:112px;">&nbsp;</div>');
        karaokeLyrics.forEach((lyric) => {
            karaokeBox.append('<div>' + lyric.text + '</div>');
        })
    }
}

function startKaraoke() {
    resetKaraoke();
    if (karaokeLyrics != null) {
        karaokeLineCount = karaokeLyrics.length;
        var milliseconds = karaokeLyrics[0].milliseconds;
        if (milliseconds > lyricsOffset) {
            milliseconds -= lyricsOffset;
        }
        karaokeQueueNextLine(milliseconds);
    }
}

function resetKaraoke() {
    if (karaokeTimeout !== null) {
        window.clearTimeout(karaokeTimeout);
    }
    karaokeCurrentLine = 0;
    karaokeLineCount = 0;
    karaokeScroll(0);
}

function karaokeNextLine() {
    if (karaokeCurrentLine > karaokeLineCount) {
        return;
    }
    karaokeCurrentLine++;
    var millisecondsDifference =
        karaokeLyrics[karaokeCurrentLine].milliseconds - karaokeLyrics[karaokeCurrentLine - 1].milliseconds;
    karaokeQueueNextLine(millisecondsDifference);
    karaokeGoToLine(karaokeCurrentLine);
}

function karaokeQueueNextLine(miliseconds) {
    karaokeTimeout = window.setTimeout(karaokeNextLine, miliseconds);
}

function karaokeGoToLine(newLine) {
    $('#karaokeBox div:nth-child(' + newLine + ')').removeClass('karaoke-current-line');
    var currentLineElement = $('#karaokeBox div:nth-child(' + (newLine + 1) + ')');
    currentLineElement.addClass('karaoke-current-line');
    var newPosition = currentLineElement[0].offsetTop - 112;
    karaokeScroll(newPosition);
}

function karaokeScroll(topPixels) {
    $("#karaokeBox")[0].scroll({
        top: topPixels,
        left: 0,
        behavior: 'smooth'
    });
}