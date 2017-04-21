var http = require('http').Server();
var io = require('socket.io')(http);
var random = require("random-js")();

var port = 3000;
var min = 1;
var max = 10000;

var players = new Array();
var rooms = new Array();

/**
* @ player_id = returns the player id
* @ player = returns the player from Players Database
* @ message = Sends a message for specific player
* @ get_data = Retrieves data from specific room to player
*/

var models = [
    'Blue_Character/blue_character',
    'Orange_Character/orange_character'
];

http.listen(port, function (err) {
    if (err) throw (err);
    console.log("Successfully started server at", port);

    rooms.push({
        id: 25,
        models: 0, // Garantir que cada jogador vai ser diferente (0 ou 1) nos defaults
        gameTime: 0,
        capacity: 2,
        created_at: new Date().getTime(),
        players: [],
        ball: {

        }
    });

});

io.on('connection', function (socket) {
    
    let player = generatePlayer();
    socket.pid = player.id;

    player.socket = socket;
    console.log("Player " + socket.pid + " has connected.");

    socket.emit('player_id', { player_id: socket.pid });

    socket.on('room_data', function (data) {
        var room = getRoom(parseInt(data.room));        
        if (room == null) return;

        var roomData = { id: room.id, entities: [], gameTime: room.gameTime };

        for (let a = 0; a < room.players.length; a++)
        {
            var player = room.players[a];            
            roomData.entities.push({
                id: player.id,
                name: player.name,
                room: player.room,
                model: player.model,
                type: player.type,
                position: player.position,
                rotation: player.rotation,
                scale: player.scale,
                defaults: player.defaults,
                created_at: player.created_at
            });
        }

        roomData.entities.push(room.ball);

        this.emit('room_data', { data: roomData });
    });

    socket.on('enter_room', function (data) {
        var room = parseInt(data.room);
        if (room === undefined) return;

        enterRoom(this.pid, room);
    });

    socket.on('disconnect', function () {
        console.log("--> Player " + this.pid + " has disconnected.");
        exitRoom(this.pid);
        destroy(this.pid);
    });
});

var containsPlayer = function (id) {
    for (let a = 0; a < players.length; a++) {
        if (players[a].id === id) return players[a];
    }

    return null;
}

var getPlayer = function (id) {
    for(let a = 0; a < players.length; a++) {
        if(players[a].id === id) return players[a];
    }

    return null;
}

var getRoom = function(id) {
    for(let a = 0; a < rooms.length; a++) {
        if(rooms[a].id === id) return rooms[a];
    }

    return null;
}

var generatePlayer = function () {
    var id = random.integer(min, max);
    while (containsPlayer(id)) {
        id = random.integer(min, max);
    }

    let player = {
        id: id,
        name: 'Player_' + id,
        room: null,        
        model: null,
        socket: null,
        type: 'player',
        standard: null,
        position: { x: 0, y: 0, z: 0 },
        rotation: { x: 0, y: 0, z: 0 },
        scale: { x: 1, y: 1, z: 1 },
        
        defaults: {
            0: {
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
                }
            },
            1: {
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
        created_at: new Date().getTime(),
    }

    players.push(player);
    return player;
}

var sendRoomMessage = function(room, message) {
    var Room = getRoom(room);
    if (Room == null || room.players === 0) {
        console.log("** Trying to send message to a invalid or empty room.");
        return;
    }

    for(let a = 0; a < Room.players.length; a++)
    {
        Room.players[a].socket.emit('message', { message: message });
    }
}

var deletePlayerFromRoom = function (id) {
    var player = getPlayer(id);    
    if (player.room == null) {
        console.log("** Player " + player.id + " is not inside a room.");
        return;
    }

    var room = getRoom(player.room);
    for (let a = 0; a < room.players.length; a++)
    {
        room.players[a].socket.emit('delete', { object: player.name });
    }
}

var deleteBallFromRoom = function (room) {

}

var roomContainsStandard = function(id, standard) {
    var room = getRoom(id);
    if (room == null) {
        console.log("** Invalid room.");
        return;
    }

    for(let a = 0; a < room.players.length; a++)
    {
        if (room.players[a].standard === standard) return true;
    }

    return false;
}

var enterRoom = function (player, room) {
    var Room = getRoom(room);
    var Player = getPlayer(player);

    if (Room == null) {
        console.log("** Trying to enter on a invalid room.");
        return;
    }
    if (Room.capacity === Room.players.length) {
        console.log("** Trying to enter on a full room ("+room+")");
        Socket.emit('message', { message: "Room " + Room.id + " is full and you can't enter." });
        return;
    }

    var standard = random.integer(0, Room.capacity - 1);

    while (roomContainsStandard(room, standard))
    {
        standard = random.integer(0, Room.capacity - 1);
    }

    Player.room = room;
    Player.standard = standard;
    Player.socket.emit('enter_room', { room_id: room });
    Player.position = Player.defaults[standard].position;
    Player.rotation = Player.defaults[standard].rotation;
    Player.scale = Player.defaults[standard].scale;
    Player.model = models[standard];
    Room.players.push(Player);

    sendRoomMessage(room, Player.id + " has connected to your room (" + Player.room + ") - (" + Room.players.length + "/" + Room.capacity + ")");
}

var exitRoom = function (id) {
    player = getPlayer(id);
    if (player.room == null) {
        console.log("** Player " + player.id + " is not inside a room.");
        return;
    }
    
    var room = getRoom(player.room);
    room.models--;

    for (let a = 0; a < room.players.length; a++)
    {
        if (room.players[a].id === id) {
            room.players.splice(a, 1);
            deletePlayerFromRoom(player.id);
            break;
        }
    }

    sendRoomMessage(player.room, player.id + " has disconnected from your room (" + player.room + ") - ("+room.players.length+"/"+room.capacity+")");
    player.room = null;
    player.model = null;
    player.standard = null;
}

var destroy = function(id) {
    for (let a = 0; a < players.length; a++) {
        if (players[a].id === id) {
            players.splice(a, 1);
        }
    }
}