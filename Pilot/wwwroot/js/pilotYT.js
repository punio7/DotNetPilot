var signalRConnection;
var songCurrentPosition = 0;
var songLength = 1;
var interval = null;
var player;

//document.getElementById("playButton").addEventListener("click", function (event) {
//    connection.invoke("SendMessage", "play").catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});

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

    signalRConnection.start().catch(function (err) {
        return console.error(err.toString());
    });
});

function onYouTubeIframeAPIReady() {
    player = new YT.Player('player', {
        height: '360',
        width: '640',
        videoId: 'M7lc1UVf-VE',
        events: {
            'onReady': onPlayerReady,
            'onStateChange': onPlayerStateChange
        }
    });
}

function onPlayerReady(event) {
    event.target.playVideo();
}

function onPlayerStateChange(event) {
    if (event.data === YT.PlayerState.PLAYING && !done) {
        //setTimeout(stopVideo, 6000);
        //done = true;
    }
}

function ajaxAction(actionName, callback) {
    var ajaxOptions = {
        url: '/Pilot/' + actionName
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
    //ajaxAction('Info', songUpdateCallback);
}

function songUpdateCallback(songInfo) {
    $('#songTitle').val(songInfo.title);
    $('#songArtist').val(songInfo.artist);
    $('#songAlbum').val(songInfo.album);
    d = new Date();
    $('#songImage').attr('src', '/Pilot/Image?' + d.getTime());
    songLength = songInfo.length;
    songCurrentPosition = songInfo.currentPosition;
    

    if (songLength > 0) {
        updateSongProgress();
        if (interval === null) {
            interval = setInterval(updateSongProgress, 1000);
        }
    }
    else {
        if (interval !== null) {
            clearInterval(interval);
            interval = null;
        }
        resetSongProgress();
    }
}

function updateSongProgress() {
    var percentage = (songCurrentPosition / songLength) * 100;
    $('#songProgress').css('width', percentage + '%');
    $('#songTime').text(formatSeconds(songCurrentPosition) + ' / ' + formatSeconds(songLength));
    songCurrentPosition++;
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