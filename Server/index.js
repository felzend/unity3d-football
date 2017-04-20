var http = require('http').Server();
var io = require('socket.io')(http);
var port = 3000;

var random = require("random-js")();
var players = new Array();
var rooms = new Array();

io.on('connection', function(socket) {

    // Generate the new player    
    socket.playerId = generatePlayerId();
    players.push({ id: socket.playerId, name: "Player_"+socket.playerId, socket: socket, room: null });

    console.log("New Connection. " + players.length + " online players.");
    socket.on('request_id', function (data) {
        socket.emit('retrieve_id', { id: socket.playerId });
    });

    socket.on('room_data', function (data) {
        var room = getRoom(parseInt(data.room));
        var json = new Array();

        for(let a = 0; a < room.players.length; a++)
        {
            if(room.players[a].id !== false)
            {
                json.push(room.players[a]);
            }
        }

        json.push(room.ball);

        socket.emit('room_data', { json: json });
    });

    socket.on('enter_room', function (data) {
        var room = enterRoom(socket.playerId, parseInt(data.id));
        var result = { id: room };

        socket.emit('entered_room', result);
    });

    socket.on('spawn', function (data) {
        var pid = parseInt(data.player);
        var rid = parseInt(data.room);
        var json = [];        

        var room = getRoom(rid);

        for (let a = 0; a < room.players.length; a++)
        {
            var p = room.players[a];
            if (p.id !== false)
            {
                p.spawned = true;
                json.push(p);
            }
        }

        this.emit('spawn', { json: json } );
    });    

    socket.on('disconnect', function () {
        exitRoom(this.playerId);
        disconnectPlayer(this.playerId);

        console.log("Disconnection. " + players.length + " online players.");
    });
})

http.listen(port, function (err) {
    if (err) throw (err);
    console.log("Successfully started server at", port);

    // Temporário até criar multiplas salas.

    {
        id:
        running:
        created:
        gameTime:
        capacity: 2,
        players: {
            id:
            name:
            model: @;6
        }
    }

    rooms.push({        
        id: 25,
        running: false,
        timestamp: 0,
        gameTime: 0,
        playing: 0,
        capacity: 2,        
        players: [ // TODO: trocar ID e NAME por Player, e adaptar no Client.
            {
                id: false,
                name: "",
                score: 0,
                model: "Blue_Character/blue_character",
                components: ['Character'],
                spawned: false,
                visible: true,
                position: {},
                rotation: {},
                scale: {},
                defaults: {
                    position: {
                        x: 0,
                        y: 2,
                        z: 20,
                    },
                    rotation: {
                        x: 0,
                        y: 90,
                        z: 0,
                    },
                    scale: {
                        x: 1,
                        y: 1,
                        z: 1,
                    },
                }
            },
            {
                id: false,
                name: "",
                score: 0,
                model: "Orange_Character/orange_character",
                components: ['Character'],
                spawned: false,
                visible: true,
                position: {},
                rotation: {},
                scale: {},
                defaults: {
                    position: {
                        x: 0,
                        y: 2,
                        z: -20,
                    },
                    rotation: {
                        x: 0,
                        y: 270,
                        z: 0,
                    },
                    scale: {
                        x: 1,
                        y: 1,
                        z: 1,
                    },
                }
            },
        ],
        ball: {
            name: "Ball",
            visible: true,
            model: "Ball/ball",
            components: ['Ball'],
            position: {},
            rotation: {},
            scale: {},
            defaults: {
                position: {
                    x: 0,
                    y: 2,
                    z: 4.76,
                },
                rotation: {
                    x: 0,
                    y: 0,
                    z: 0,
                },
                scale: {
                    x: 2,
                    y: 2,
                    z: 2,
                },
            }
        },
    });
});

function getPlayer(id)
{
    for (let a = 0; a < players.length; a++)
        if (players[a].id === id)
            return players[a];
    return null;
}

function getRoom(id)
{
    for (let a = 0; a < rooms.length; a++)
        if (rooms[a].id === id)
            return rooms[a];
    return null;
}

function enterRoom(playerId, roomId)
{
    var room = getRoom(roomId);
    if( room == null) {
        console.log("Tried to enter into a invalid Room.");
        return -1;
    }

    if (room.playing >= room.capacity) return 0;
    var pid = random.integer(0, room.players.length - 1);

    while (room.players[pid].id !== false)
    {
        pid = random.integer(0, room.players.length - 1);
    }

    room.playing++;
    room.players[pid].id = playerId;
    room.players[pid].name = getPlayer(playerId).name;

    getPlayer(playerId).room = roomId;

    console.log("Player " + playerId + " has entered Room " + room.id + " (" + room.playing + "/" + room.capacity + ")");

    if (room.playing === room.capacity)
    {
        for(let a = 0; a < room.players.length; a++)
        {
            var id = room.players[a].id;
            if (id !== false) {
                console.log("Sent to " + id);
                getPlayer(id).socket.emit("start", { seconds: 5 });
            }
        }
    }

    return roomId;
}

function exitRoom(playerId)
{
    var player = getPlayer(playerId);
    if (player.room == null) return;

    var room = getRoom(player.room);

    for(let p = 0; p < room.players.length; p++)
    {
        if (room.players[p].id == playerId)
        {
            room.playing--;
            room.players[p].id = false;
            room.players[p].name = "";
            room.players[p].position = {};
            room.players[p].rotation = {};
            room.players[p].scale = {};
            player.room = null;

            console.log("Player " + playerId + " has exited Room " + room.id + " (" + room.playing + "/" + room.capacity + ")");

            if (room.playing < room.capacity) // Will make stop the match immediatelly
            {
                for (let a = 0; a < room.players.length; a++)
                {
                    var id = room.players[a].id;
                    if(id !== false) getPlayer(id).socket.emit("end");
                }
            }

            return true;
        }
    }

    return false;
}

function generatePlayerId()
{	
    var gid = random.integer(1, 10000);

    for(let a = 0; a < players.length; a++) 
    {
        if (players[a].id === gid)
        {
            gid = random.integer(1, 100);
            a = 0;
        }
    }

    return gid;
}

function generateRoom()
{
    var gid = random.integer(1, 10000);

    for (let a = 0; a < rooms.length; a++)
    {
        if (rooms[a].id === gid)
        {
            gid = random.integer(1, 10000);
            a = 0;
        }
    }

    return gid;
}

function disconnectPlayer(id)
{
    for(let a = 0; a < players.length; a++)
    {        
        if (players[a].id === id) {
            io.emit('remove_object', {name: players[a].name});
            players.splice(a, 1);
        }
    }
}