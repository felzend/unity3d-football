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
        gameTime: 0,
        capacity: 2,
        created_at: new Date().getTime(),
        players: [],
        ball: {
            id: 50000,
            name: "Ball",
            model: "Ball/ball",
            type: "ball",
            spawned: true,
            position: { x: 0, y: 32, z: 0 },
            rotation: { x: 0, y: 0, z: 0 },
            scale: { x: 2, y: 2, z: 2 },
            defaults: {
                position: {
                    x: -5,
                    y: 32,
                    z: 0,
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
                }
            }
        }
    });   
});

io.on('connection', function (socket) {
    
    let player = generatePlayer();
    socket.pid = player.id;

    player.socket = socket;
    console.log("Player " + socket.pid + " has connected.");

    socket.on('player_id', function () {
        this.emit('player_id', { player_id: player.id, player_name: player.name });
    });

    socket.on('is_pivot', function (data) {
        var player = getPlayerFromRoom(parseInt(data.player), parseInt(data.room));

        if (player == null) return;

        this.emit('is_pivot', { is_pivot: player.pivot });
    });

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

        if( room.ball.spawned ) roomData.entities.push(room.ball);

        this.emit('room_data', { data: roomData });
    });

    socket.on('update_player', function (data) {
        var player = getPlayerFromRoom(parseInt(data.player), parseInt(data.room));
        if (player == null) return;

        var position = {
            x: parseFloat(data.pos_x),
            y: parseFloat(data.pos_y),
            z: parseFloat(data.pos_z)
        };

        var rotation = {
            x: parseFloat(data.rot_x),
            y: parseFloat(data.rot_y),
            z: parseFloat(data.rot_z)
        }

        player.position = position;
        player.rotation = rotation;
    });

    socket.on('update_ball', function (data) {
        var room = getRoom(parseInt(data.room));
        var except_player = parseInt(data.except_player);

        if (room == null) return;

        var position = {
            x: parseFloat(data.pos_x),
            y: parseFloat(data.pos_y),
            z: parseFloat(data.pos_z)
        };

        var rotation = {
            x: parseFloat(data.rot_x),
            y: parseFloat(data.rot_y),
            z: parseFloat(data.rot_z)
        }        

        for(let a = 0; a < room.players.length; a++)
        {
            var player = room.players[a];
            if (player.id == except_player) continue;
            
            player.socket.emit('update_ball', {
                name: room.ball.name,
                position: position,
                rotation: rotation
            });
        }

        room.ball.position = position;
        room.ball.rotation = rotation;
    });

    socket.on('move_player', function (data) {
        var player = getPlayerFromRoom( parseInt(data.player), parseInt(data.room) );
        if (player == null) return;

        if (data.x != undefined) player.position.x = parseFloat(data.x);
        if (data.y != undefined) player.position.y = parseFloat(data.y);
        if (data.z != undefined) player.position.z = parseFloat(data.z);
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

var getPlayerFromRoom = function (player, room) {
    var Room = getRoom(room);
    var Player = getPlayer(player);
    if (Room == null) {
        console.log("** Invalid room.");
        return null;
    }
    if (Player == null) {
        console.log("** Invalid player.");
        return null;
    }

    for(let a = 0; a < Room.players.length; a++)
    {
        if( Room.players[a].id === player ) return Room.players[a];
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
        type: 'character',
        standard: null,
        position: { x: 0, y: 2, z: 0 },
        rotation: { x: 0, y: 0, z: 0 },
        scale: { x: 1, y: 1, z: 1 },
        
        defaults: {
            0: {
                position: {
                    x: 20,
                    y: 5,
                    z: 0,
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
                }
            },
            1: {
                position: {
                    x: -20,
                    y: 5,
                    z: 0,
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

var deletePlayerFromRoom = function (id) { // Broadcast for deleting GameObject from Scene.
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

var createBall = function (room) {
    var room = getRoom(room);
    if (room == null) return;

    room.ball.position = room.ball.defaults.position;
    room.ball.rotation = room.ball.defaults.rotation;
    room.ball.scale = room.ball.defaults.scale;
    room.ball.spawned = true;
}

var deleteBall = function (room) {
    var room = getRoom(room);
    if (room == null) return;

    room.ball.spawned = false;

    for (let a = 0; a < room.players.length; a++)
    {
        room.players[a].socket.emit('delete', { object: room.ball.name });
    }
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
        Player.socket.emit('message', { message: "Room " + Room.id + " is full and you can't enter." });
        return;
    }

    var standard = random.integer(0, Room.capacity - 1);

    while (roomContainsStandard(room, standard))
    {
        standard = random.integer(0, Room.capacity - 1);
    }

    if (Room.players.length == 0) {
        Player.pivot = true;
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

    player.socket.emit('unset_pivot');

    for (let a = 0; a < room.players.length; a++)
    {
        if (room.players[a].id === id) {
            room.players.splice(a, 1);

            if (room.players.length > 0) { // Defines the New Pivot, if there are still players on the room.
                var newPivot = random.integer(0, room.players.length - 1);
                room.players[newPivot].pivot = true;
                getPlayer(room.players[newPivot].id).socket.emit('update_pivot', { is_pivot: true });
            }
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