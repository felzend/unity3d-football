var http = require('http').Server();
var io = require('socket.io')(http);
var port = 3000;

var random = require("random-js")();
var players = new Array();
var rooms = new Array();

io.on('connection', function(socket) {

    socket.playerId = generatePlayerId();
    players.push({ id: socket.playerId });
    console.log("New Connection. " + players.length + " online players.");

    socket.on('request_id', function (data) {
        socket.emit('retrieve_id', { id: socket.playerId });
    });

    socket.on('enter_room', function (data) {
        var room = enterRoom(socket.playerId, parseInt(data.id));
        var result = { id: room };

        if (!Number.isInteger(room)) result.error = room;
        socket.emit('entered_room', result);
    });

    socket.on('disconnect', function () {
        disconnectPlayer(this.playerId);
        console.log("Disconnection. " + players.length + " online players.");
    });
})

http.listen(port, function (err) {
    if (err) throw (err);
    console.log("Successfully started server at", port);

    // Temporário até criar multiplas salas.

    rooms[0] = {
        id: 25,
        timestamp: 0,
        gameTime: 0,
        paused: true,
        players: {
            0: {
                occupied: false,
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
            1: {
                occupied: false,
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
        },
        ball: {
            visible: true,
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
    };
});

function enterRoom(playerId, roomId)
{
    for(let a = 0; a < rooms.length; a++)
    {
        if (rooms[a].id === roomId)
        {
            var cid = random.integer(0, 1);
            return roomId;
        }
    }

    return "A sala " + roomId + " não existe.";
}

function exitRoom(playerId, roomId)
{

}

function generatePlayerId()
{	
	var gid = random.integer(1, 100);

	for(let a = 0; a < players.length; a++) 
	{
		if( players[ a ].id === gid ) {
			gid = random.integer(1, 100);
			a = 0;
		}
	}

	return gid;
}

function disconnectPlayer(id)
{
    for(let a = 0; a < players.length; a++)
    {
        if( players[a].id === id ) players.splice(a, 1);
    }
}