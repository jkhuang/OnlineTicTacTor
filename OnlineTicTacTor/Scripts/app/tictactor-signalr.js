/// <reference path="../jquery-ui-1.10.4.js" />
/// <reference path="../jquery-2.1.0.js" />
/// <reference path="../knockout-3.1.0.debug.js" />
/// <reference path="../jquery.signalR-2.0.2.js" />

// The game viem model.
var GameViewModel = function () {
    var self = this;
    
    // The connection user information.
    self.Users = ko.observableArray();
    
    // The user connection.
    self.UserConnections = [];
    
    // Stores the game instances.
    self.Game = {};
    
    // Gets the current user.
    self.CurrentPlayer = ko.observable('Game not started');
    
    // If the game started, Challenge is disabled.
    self.ChallengeDisabled = ko.observable(false);
};

$(function () {
    
    // Create a game view model.
    var vm = new GameViewModel();
    ko.applyBindings(vm);
   
    var $canvas = document.getElementById('gameCanvas'); //$('gameCanvas')[0];
    
    if ($canvas) {
        var hSpacing = $canvas.width / 3,
            vSpacing = $canvas.height / 3;
    }

    // Declares a proxy to reference the server hub. 
    // The connection name is the same as our declared in server side.
    var hub = $.connection.gameNotificationHub;

    // Draws the game with 'X' or 'O'.
    hub.client.drawPlay = function (rowCol, game, letter) {
        vm.Game = game;
        var row = rowCol.row,
            col = rowCol.col,
            hCenter = (col - 1) * hSpacing + (hSpacing / 2),
            vCenter = (row - 1) * vSpacing + (vSpacing / 2);
        writeMessage($canvas, letter, hCenter, vCenter);
        if (game.GameStatus == 0) {
            vm.CurrentPlayer(game.NextTurn);
        } else {
            vm.CurrentPlayer(game.Message);
            alert("Game Over - " + game.Message);
            location.reload();
        }
    };

    // Adds the online user.
    hub.client.joined = function (connection) {
        
        // Remove the connection by userid.
        vm.Users.remove(function(item) {
            return item.UserId == connection.UserId;
        });
        vm.Users.push(connection);  
    };

    // Gets the challenge response.
    hub.client.getChallengeResponse = function (connectionId, userId) {
        
        vm.ChallengeDisabled(true);
        refreshConnections();
        var cnf = confirm('You have been challenged to a game of Tic-Tac-ToR by \'' + userId + '\'. Ok to Accept!');

        if (cnf) {
            hub.server.challengeAccepted(connectionId);
        } else {
            hub.server.challengeRefused(connectionId);
            vm.ChallengeDisabled(false);
            refreshConnections();
        }

    };

    // Refreshs the user connection.
    function refreshConnections() {
        var oldItems = vm.Users.removeAll();
        vm.Users(oldItems);
    }


    // Stores all connection into the user list, expect the current login user.
    hub.client.updateSelf = function (connections, connectionName) {
        for (var i = 0; i < connections.length; i++) {
            if (connections[i].UserId != connectionName) {
                vm.Users.push(connections[i]);
            }
        } 
    };

    // Handles other client refuses the chanllenge.
    hub.client.challengeRefused = function () {
        vm.ChallengeDisabled(false);
        vm.CurrentPlayer('Challenge not accepted!');
        refreshConnections();
    };

    hub.client.waitForResponse = function (userId) {
        vm.ChallengeDisabled(true);
        vm.CurrentPlayer('Waiting for ' + userId + ' to accept challenge');
        refreshConnections();
    };

    // Keeps the connection still alive.
    hub.client.rejoinGame = function (connections, connectionName, gameDetails) {
        if (gameDetails != null) {
            vm.ChallengeDisabled(true);
            refreshConnections();
            vm.Game = gameDetails;
            
            // Sets the current player.
            vm.CurrentPlayer(gameDetails.NextTurn);
            
            for (var row = 0; row < 3; row++)
                for (var col = 0; col < 3; col++) {
                    var letter = '';
                    if (gameDetails.GameMatrix[row][col] == 1) {
                        letter = 'O';
                    }
                    else if (gameDetails.GameMatrix[row][col] == 10) {
                        letter = 'X';
                    }
                    if (letter != '') {
                        var hCenter = (col) * hSpacing + (hSpacing / 2);
                        var vCenter = (row) * vSpacing + (vSpacing / 2);
                        writeMessage($canvas, letter, hCenter, vCenter);
                    }
                }

            vm.Users = ko.observableArray();
            for (var i = 0; i < connections.length; i++) {
                if (connections[i].UserId != connectionName) {
                    vm.Users.push(connections[i]);
                }
            }
            vm.Users.remove(function (item) { return item.UserId == gameDetails.User1Id.UserId; });
            vm.Users.remove(function (item) { return item.UserId == gameDetails.User2Id.UserId; });
            
        }
    };

    // The game begins.
    hub.client.beginGame = function (gameDetails) {
        vm.ChallengeDisabled(true);
        refreshConnections();
        if (gameDetails.User1Id.UserId == clientId ||
            gameDetails.User2Id.UserId == clientId) {
            clearCanvas();
            vm.Game = gameDetails;
            vm.CurrentPlayer(gameDetails.NextTurn);
        }
        var oldArray = vm.Users;
        vm.Users.remove(function (item) { return item.UserId == gameDetails.User1Id.UserId; });
        vm.Users.remove(function (item) { return item.UserId == gameDetails.User2Id.UserId; });
    };

    // Removes the leave user from the user list.
    hub.client.leave = function (connectionId) {
        vm.Users.remove(function (item) { return item.ConnectionId == connectionId; });
    };

    // When signalR hub was ready after hub.start().done()
    $.connection.hub.start().done(function () {
        var canvasContext;
        
        // The user list click event handle.
        $('#activeUsersList').delegate('.challenger', 'click', function () {
            vm.ChallengeDisabled(true);
            
            // TODO:
            var challengeTo = ko.dataFor(this);
            vm.CurrentPlayer('Waiting for ' + challengeTo.UserId + ' to accept challenge');
            hub.server.challenge(challengeTo.ConnectionId, clientId);
            refreshConnections();
        });

        if ($canvas && $canvas.getContext) {
            canvasContext = $canvas.getContext('2d');
            var rect = $canvas.getBoundingClientRect();
            $canvas.height = rect.height;
            $canvas.width = rect.width;
            hSpacing = $canvas.width / 3;
            vSpacing = $canvas.height / 3;

            // canvas click event handle.
            $canvas.addEventListener('click', function (evt) {
                if (vm.CurrentPlayer() == clientId) {
                    var rowCol = getRowCol(evt);
                    rowCol.Player = 'O';
                    hub.server.gameMove(vm.Game.GameId, rowCol);
                }
            }, false);

            drawGrid(canvasContext);
        }
        
        // Gets the user clicks on grid row and column position.
        function getRowCol(evt) {
            var hSpacing = $canvas.width / 3;
            var vSpacing = $canvas.height / 3;
            var mousePos = getMousePos($canvas, evt);
            return {
                row: Math.ceil(mousePos.y / vSpacing),
                col: Math.ceil(mousePos.x / hSpacing)
            };
        }

        // Gets the user mouse click relative poisition in the canvas. 
        function getMousePos($canvas, evt) {
            var rect = $canvas.getBoundingClientRect();
            return {
                x: evt.clientX - rect.left,
                y: evt.clientY - rect.top
            };
        }
    });
    
    // When the game end, clear the canvas.
    function clearCanvas() {
        if ($canvas && $canvas.getContext) {
            var canvasContext = $canvas.getContext('2d');
            var rect = $canvas.getBoundingClientRect();
            $canvas.height = rect.height;
            $canvas.width = rect.width;

            if (canvasContext) {
                canvasContext.clearRect(rect.left, rect.top, rect.width, rect.height);
            }
            drawGrid(canvasContext);
        }
    }

    // Draws the grid.
    function drawGrid(canvasContext) {
        var hSpacing = $canvas.width / 3;
        var vSpacing = $canvas.height / 3;
        canvasContext.lineWidth = "2.0";
        for (var i = 1; i < 3; i++) {
            canvasContext.beginPath();
            canvasContext.moveTo(0, vSpacing * i);
            canvasContext.lineTo($canvas.width, vSpacing * i);
            canvasContext.stroke();

            canvasContext.beginPath();
            canvasContext.moveTo(hSpacing * i, 0);
            canvasContext.lineTo(hSpacing * i, $canvas.height);
            canvasContext.stroke();
        }
    }

    // Update the grid with 'X' or 'O'.
    function writeMessage($canvas, message, x, y) {
        var canvasContext = $canvas.getContext('2d');
        canvasContext.font = '40pt Calibri';
        canvasContext.fillStyle = 'red';
        var textSize = canvasContext.measureText(message);
        canvasContext.fillText(message, x - (textSize.width / 2), y + 10);

    }
});