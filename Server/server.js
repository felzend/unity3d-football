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
* @
*/

rooms[0] = {
    id: 25,
    models: [], // Garantir que cada jogador vai ser diferente (0 ou 1) nos defaults
    gameTime: 0,
    capacity: 2,
    created_at: new Date().getTime(),
    players: [],
    ball: {

    }
}

io.on('connection', function (socket) {
    
    let player = generatePlayer();
    socket.pid = player.id;

    player.socket = socket;

    socket.on('disconnect', function () {
        exitRoom(socket.pid);
        destroy(socket.pid);
    });
});

var containsPlayer = function (id) {
    for (let a = 0; a < players.length; a++) {
        if (players[a].id === id) return players[a];
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
    while (containsEntity(id)) {
        id = random.integer(min, max);
    }

    let player = {
        id: id,
        name: 'Player_' + id,
        room: null,        
        model: null,
        socket: null,
        type: 'player',
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

    entities.push(player);
    return player;
}

var generateObject = function() {
    
    var id = random.integer(min, max);
    while( containsEntity(id) )
    {
        id = random.integer(min, max);
    }

    let object = {
        id: id,
        name: 'Object_'+id,
        room: null,
        model: null,
        type: 'object',
        created_at: new Date().getTime(),
    }
    
    entities.push(object);
    return object;
}

var enterRoom = function (player, room) {
    if (getRoom(room) == null) return;
    player.room = room;
}

var exitRoom = function (player) {
    var room = getRoom(player.room);
    player.room = null;
}