﻿var signalRConnection;
var songCurrentPosition = 0;
var songLength = 1;

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
    $('#songImage').attr('src', '/Pilot/Image?' + d.getTime());
    songLength = songInfo.length;
    songCurrentPosition = songInfo.currentPosition;

    if (songLength > 0) {
        document.title = "▶ " + songInfo.title + " - Pilot";
        updateSongProgress();
    }
    else {
        document.title = "⏹ Zatrzymany - Pilot";
        resetSongProgress();
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