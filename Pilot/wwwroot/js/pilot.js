var connection = new signalR.HubConnectionBuilder().withUrl("/pilotHub").build();

connection.on("ReceiveMessage", function (message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = "user says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("playButton").addEventListener("click", function (event) {
    connection.invoke("SendMessage", "play").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});