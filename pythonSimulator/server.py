import uvicorn
import fastapi
import datetime
import json
from eventgame import EventGame
from player import Player
import numpy as np
from buildChallenge import BuildChallenge

from typing import Any
import socketio
import websockets

sio: Any = socketio.AsyncServer(async_mode="asgi")
socket_app = socketio.ASGIApp(sio)

app = fastapi.FastAPI()

current_games = {}
game_rooms = {}
game_to_room_map = {}

"""
SAMPLE SINGLE PLAYER GAME:
http://127.0.0.1:5000/start_new_game?p1=p_1&p2=p_2&p3=AI_Player1&p4=AI_Player2
http://127.0.0.1:5000/first_move?gid=1298297976736304479&piece=2&perm=1
http://127.0.0.1:5000/reg_move?gid=1298297976736304479&p_num=1&piece=15&perm=1&x_coor=15&y_coor=13
http://127.0.0.1:5000/pass_turn?gid=1298297976736304479&p_num=2
http://127.0.0.1:5000/ai_move?gid=1298297976736304479
http://127.0.0.1:5000/end_game?gid=1298297976736304479
"""

"""
SAMPLE MULTIPLAYER GAME:
http://127.0.0.1:5000/create_waiting_room?rn=1&p1=Admin_player
{"Result":"[0]","Desc":"OK","state":"1","room_id":"-5263700288565586101","room_name":"1"}
http://127.0.0.1:5000/join_waiting_room?rn=1&pn=player2
{"Result":"[0]","Desc":"OK","state":"1","player_code":-6035365310716894887}
http://127.0.0.1:5000/activate_game?rn=1&r_id=-5263700288565586101
{"Result":"[0]","Desc":"OK","game_id":"-1781493617116865433"}
http://127.0.0.1:5000/ai_move?gid=-1781493617116865433
{"Result":"[0]","Desc":"OK","shape_num":"15","permutation":"1","number_of_squares_closed":"-1"}
http://127.0.0.1:5000/reg_move_multi?gid=-7970304273228674816&pn=Admin_player&pc=8253353491383645553&piece=8&perm=1&x_coor=15&y_coor=17
{"Result":"[2, 1]"}

"""


class GameRoom:
    def __init__(self, name, r_id, admin):
        self.name = name
        self.id = r_id  # acts as a password for the admin
        self.admin = admin
        self.players = [admin]
        self.state = 1
        self.game_id = -1
        self.player_codes = {admin: self.id}
        self.broadcast = {}

    async def broadcast_move(self, info_dict):
        for k, v in self.broadcast.items():
            await sio.emit("move_update", str(info_dict), room=v)


@app.get('/')
async def home():
    return {"message": "Hello World"}


@app.get('/create_waiting_room')
async def create_new_room(rn: str, p1: str):
    room_admin = p1
    room_name = rn
    if room_name in game_rooms:
        return {"Result": '-2', "Desc": 'Game room with that name already exists'}

    room_id = hash(datetime.datetime.now().isoformat() + str(room_admin))
    game_rooms[room_name] = GameRoom(room_name, room_id, room_admin)
    return {'Result': '[0]', 'Desc': 'OK', 'state': '1', "room_id": str(room_id), 'room_name': str(room_name)}


@app.get('/query_waiting_room')
async def query_waiting_room(rn: str):
    # fix like the rest later
    to_send = ""
    if rn not in game_rooms:
        to_send = {'state': '-1', 'game_id': '-1'}
    else:
        to_send = {'state': str(game_rooms[rn].state), 'game_id': str(game_rooms[rn].game_id)}
    to_send = str(to_send)
    return {'Result': to_send, 'Desc': 'OK'}


@app.get('/query_all_rooms')
async def query_all_rooms():
    games = str(game_rooms.keys())
    return {'Result': '[0]', 'Desc': 'OK', 'Game list': games}


@app.get('/join_waiting_room')
async def join_waiting_room(rn: str, pn: str):
    if rn not in game_rooms:
        return {"Result": '-1', "Desc": 'Game room does not exist'}
    if pn in game_rooms[rn].players:
        return {"Result": '-3', "Desc": 'Game room already has a player with that name'}
    if game_rooms[rn].state > 1:
        return {"Result": '-4', "Desc": 'Game has already begun'}
    if len(game_rooms[rn].players) >= 4:
        return {"Result": '-6', "Desc": 'Room has too many players'}

    game_rooms[rn].players.append(pn)
    code = hash(datetime.datetime.now().isoformat() + str(pn))
    game_rooms[rn].player_codes[pn] = code
    return {'Result': '[0]', 'Desc': 'OK', 'state': str(game_rooms[rn].state), 'player_code': code}


@app.get('/remove_from_room')
async def remove_from_room(rn: str, r_id: int, pn: str):
    if rn not in game_rooms:
        return {"Result": '-1', "Desc": 'Game room does not exist'}
    if pn not in game_rooms[rn].players:
        return {"Result": '-3', "Desc": 'Game room  does not have a player with that name'}
    if game_rooms[rn].state > 1:
        return {"Result": '-4', "Desc": 'Game has already begun'}
    if pn == game_rooms[rn].admin:
        return {"Result": '-5', "Desc": 'Cannot remove admin from room'}
    if r_id != game_rooms[rn].id:
        return {"Result": '-6', "Desc": 'Only admin can remove from room'}
    game_rooms[rn].players.remove(pn)
    game_rooms[rn].player_codes.pop(pn, None)
    return {'Result': '[0]', 'Desc': 'OK', "state": 1}


@app.get('/leave_room')
async def leave_room(rn: str, pn: str, pc: int):
    if rn not in game_rooms:
        return {"Result": '-1', "Desc": 'Game room does not exist'}
    if pn not in game_rooms[rn].players:
        return {"Result": '-3', "Desc": 'Game room  does not have a player with that name'}
    if game_rooms[rn].state > 1:
        return {"Result": '-4', "Desc": 'Game has already begun'}
    if pn == game_rooms[rn].admin:
        return {"Result": '-5', "Desc": 'Admin cannot leave room from room'}
    if pc != game_rooms[rn].player_codes[pn]:
        return {"Result": '-6', "Desc": 'Only the player himself can leave room'}
    game_rooms[rn].players.remove(pn)
    game_rooms[rn].player_codes.pop(pn, None)
    game_rooms[rn].broadcast.pop(pn, None)
    return {'Result': '[0]', 'Desc': 'OK', "state": 1}


@app.get('close_room')
async def close_room(rn: str, r_id: int):
    if rn not in game_rooms:
        return {"Result": '-1', "Desc": 'Game room does not exist'}
    if game_rooms[rn].state > 1:
        return {"Result": '-4', "Desc": 'Game has already begun'}
    if r_id != game_rooms[rn].id:
        return {"Result": '-6', "Desc": 'Only admin can close room'}
    game_rooms.pop(rn, None)

    return {'Result': '[0]', 'Desc': 'OK', "state": 1}


@app.get('/activate_game')
async def activate_game(rn: str, r_id: int):
    if rn not in game_rooms:
        return {"Result": '-1', "Desc": 'Game room does not exist'}
    if game_rooms[rn].state > 1:
        return {"Result": '-4', "Desc": 'Game has already begun'}
    if game_rooms[rn].id != r_id:
        return {"Result": '-5', "Desc": 'Only room admin can start a game'}
    player_names = game_rooms[rn].players
    players = []
    le = len(player_names)
    for i in range(le, 4):
        player_names.append(f'AI_Player{i}')
    for name in player_names:
        if name.find("AI_Player") >= 0:
            players.append(Player(name=name, ai=True))
        else:
            players.append(Player(name=name))

    new_game_id = hash(datetime.datetime.now().isoformat() + str(player_names))
    current_games[new_game_id] = EventGame(players)
    game_rooms[rn].state = 2
    game_rooms[rn].game_id = new_game_id
    game_to_room_map[new_game_id] = rn
    await game_rooms[rn].broadcast_move({'Move': 'Game_started', 'Players': str(player_names),
                                         'Result': '[0]', 'Desc': 'OK', 'game_id': str(new_game_id)
                                         })
    return {'Result': '[0]', 'Desc': 'OK', 'game_id': str(new_game_id)}


@app.get('/first_move_multi')
async def first_move_multi(gid: int, pn: str, pc: int, piece: int, perm: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    if game.players[game.curr_player_num - 1].ai_player:
        return {'Result': '[-9]', 'Desc': 'AI_Player Turn'}
    if gid not in game_to_room_map:
        return {'Result': '[-5]', 'Desc': 'Game not multiplayer'}
    room = game_rooms[game_to_room_map[gid]]
    if room.game_id != gid:
        return {'Result': '[-6]', 'Desc': 'Game Number mismatch'}
    if game.started:
        return {'Result': '[-33]', 'Desc': 'Game Already Started'}
    if room.players[game.curr_player_num - 1] != pn:
        return {'Result': '[-34]', 'Desc': 'Wrong player'}
    if room.player_codes[pn] != pc:
        return {'Result': '[-35]', 'Desc': 'Wrong Player Code'}
    to_send = game.start_game(piece, perm), game.last_new_squares
    await room.broadcast_move({'Player': pn, "Piece": piece, "Perm": perm, 'Result': '[0]', 'Desc': 'OK',
                               'number_that_indicates_whether_the_move_was_legal': str(to_send[0]),
                               'number_of_squares_closed': str(to_send[1])
                               })
    return {'Result': '[0]', 'Desc': 'OK', 'number_that_indicates_whether_the_move_was_legal': str(to_send[0]),
            'number_of_squares_closed': str(to_send[1])}


@app.get('/reg_move_multi')
async def reg_move_multi(gid: int, pn: str, pc: int, piece: int, perm: int, x_coor: int, y_coor: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    if game.players[game.curr_player_num - 1].ai_player:
        return {'Result': '[-9]', 'Desc': 'AI_Player Turn'}

    if gid not in game_to_room_map:
        return {'Result': '[-5]', 'Desc': 'Game not multiplayer'}
    room = game_rooms[game_to_room_map[gid]]
    if room.game_id != gid:
        return {'Result': '[-6]', 'Desc': 'Game Number mismatch'}
    if not game.started:
        return {'Result': '[-33]', 'Desc': 'Game has not started'}
    if room.players[game.curr_player_num - 1] != pn:
        return {'Result': '[-34]', 'Desc': 'Wrong player'}
    print(room.player_codes)
    if room.player_codes[pn] != pc:
        return {'Result': '[-35]', 'Desc': 'Wrong Player Code'}
    to_send = game.move(game.curr_player_num, piece, perm, x_coor, y_coor), game.last_new_squares
    await room.broadcast_move({'Player': pn, "Piece": piece, "Perm": perm, 'x_coor': x_coor, 'y_coor': y_coor,
                               'Result': '[0]', 'Desc': 'OK',
                               'number_that_indicates_whether_the_move_was_legal': str(to_send[0]),
                               'number_of_squares_closed': str(to_send[1])
                               })
    return {'Result': '[0]', 'Desc': 'OK', 'number_that_indicates_whether_the_move_was_legal': str(to_send[0]),
            'number_of_squares_closed': str(to_send[1])}


@app.get('/query_game_state')
async def query_game_state(gid: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    state = game.started
    player_num = game.curr_player_num
    return {'Result': '[0]', 'Desc': 'OK', 'Started': state, 'Player Turn': player_num, 'Board': game.get_board()}


@app.get('/query_game_board')
async def query_game_state(gid: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    state = game.started
    player_num = game.curr_player_num
    return {'Result': '[0]', 'Desc': 'OK', 'Board': game.get_board()}


@app.get('/pass_turn_multi')
async def pass_turn_multi(gid: int, pn: str, pc: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    if game.players[game.curr_player_num - 1].ai_player:
        return {'Result': '[-9]', 'Desc': 'AI_Player Turn'}
    if gid not in game_to_room_map:
        return {'Result': '[-5]', 'Desc': 'Game not multiplayer'}
    room = game_rooms[game_to_room_map[gid]]
    if room.game_id != gid:
        return {'Result': '[-6]', 'Desc': 'Game Number mismatch'}
    if not game.started:
        return {'Result': '[-33]', 'Desc': 'Game has not started'}
    if room.players[game.curr_player_num - 1] != pn:
        return {'Result': '[-34]', 'Desc': 'Wrong player'}
    if room.player_codes[pn] != pc:
        return {'Result': '[-35]', 'Desc': 'Wrong Player Code'}
    to_send = str({game.pass_turn(game.curr_player_num), game.last_new_squares})
    await room.broadcast_move({'Player': pn, "Piece": -1, 'Move': 'Pass',
                               'Result': to_send, 'Desc': 'OK',
                               })
    return {'Result': to_send, 'Desc': 'OK'}


# ----------------- single player version -----------------

@app.get('/start_new_game')
async def start_new_game(p1: str = 'NULL', p2: str = 'NULL',
                         p3: str = 'NULL', p4: str = 'NULL'):
    players = []
    player_names = []
    if p1 != 'NULL':
        player_names.append(p1)
    if p2 != 'NULL':
        player_names.append(p2)
    if p3 != 'NULL':
        player_names.append(p3)
    if p4 != 'NULL':
        player_names.append(p4)
    print(player_names)
    for name in player_names:
        if name.find("AI_Player") >= 0:
            players.append(Player(name=name, ai=True))
        else:
            players.append(Player(name=name))
    print("Names:")
    print(player_names)
    new_game_id = hash(datetime.datetime.now().isoformat() + str(player_names))
    print(new_game_id)
    current_games[new_game_id] = EventGame(players)
    return {'Result': '[0]', 'Desc': 'OK', "game_id": new_game_id}


@app.get('/first_move')
async def first_move(gid: int, piece: int, perm: int):
    print("hi", gid not in current_games)
    if gid not in current_games:
        return {'Result': '[-22]', 'Desc': 'Error - no game with given id'}
    game = current_games[gid]
    if game.started:
        return {'Result': '[-33]', 'Desc': 'Error - game already started!'}
    to_send = game.start_game(piece, perm), game.last_new_squares
    return {'Result': '[0]', 'Desc': 'OK', 'number_that_indicates_whether_the_move_was_legal': str(to_send[0]),
            'number_of_squares_closed': str(to_send[1])}


@app.get('/reg_move')
async def reg_move(gid: int, p_num: int, piece: int, perm: int, x_coor: int, y_coor: int):
    if gid not in current_games:
        return {'Result': '[-22]', 'Desc': 'Error - no game with given id'}
    game = current_games[gid]
    if not game.started:
        return {'Result': '[-33]', 'Desc': 'Error - game not started!'}
    to_send = game.move(p_num, piece, perm, x_coor, y_coor), game.last_new_squares
    return {'Result': '[0]', 'Desc': 'OK', 'number_that_indicates_whether_the_move_was_legal': str(to_send[0]),
            'number_of_squares_closed': str(to_send[1])}


@app.get('/ai_move')  # ai_move is the same for multiplayer and singleplayer.
async def ai_move(gid: int):
    if gid not in current_games:
        return {'Result': '[-22]', 'Desc': 'Error - no game with given id'}
    game = current_games[gid]
    if game.players[game.curr_player_num - 1].ai_player:  # if the current turn is of an AI player
        best_move = game.players[game.curr_player_num - 1].oracle.calc_move(
            game.game_board,
            [p.pieces for p in game.players],
            game.curr_player_num - 1
        )
        if game.started:
            to_send = [float(
                game.move(game.curr_player_num, best_move[0], best_move[1], best_move[2], best_move[3]))]
        else:
            to_send = [float(game.start_game(best_move[0], best_move[1], ))]

        if to_send != [-1]:
            send_arr = np.array([i for i in best_move], dtype=np.intc)
            to_send = send_arr
    else:
        to_send = [-1]
    to_send = np.append(to_send, game.last_new_squares)
    if gid in game_to_room_map:
        room = game_rooms[game_to_room_map[gid]]
        if len(to_send) < 5:
            await room.broadcast_move({'Move': 'Ai_Move', 'Result': '[0]', 'Desc': 'OK',
                                       'shape_num': str(to_send[0]), 'permutation': str(to_send[1]),
                                       'number_of_squares_closed': str(to_send[2])
                                       })
        else:
            await room.broadcast_move({'Move': 'Ai_Move', 'Result': '[0]', 'Desc': 'OK',
                                       'shape_num': str(to_send[0]), 'permutation': str(to_send[1]),
                                       'x_position': str(to_send[2]), 'y_position': str(to_send[3]),
                                       'number_of_squares_closed': str(to_send[4])
                                       })
    if len(to_send) < 5:
        return {'Result': '[0]', 'Desc': 'OK', 'shape_num': str(to_send[0]), 'permutation': str(to_send[1]),
                'number_of_squares_closed': str(to_send[2])}
    return {'Result': '[0]', 'Desc': 'OK', 'shape_num': str(to_send[0]), 'permutation': str(to_send[1]),
            'x_position': str(to_send[2]), 'y_position': str(to_send[3]), 'number_of_squares_closed': str(to_send[4])}


@app.get('/pass_turn')
async def pass_turn(gid: int, p_num: int):
    if gid not in current_games:
        return {'Result': '[-22]', 'Desc': 'Error - no game with given id'}
    game = current_games[gid]
    to_send = game.pass_turn(p_num), game.last_new_squares
    return {'Result': '[0]', 'Desc': 'OK', 'more:': to_send}


@app.get('/end_game')
async def end_game(gid: int, r_id: int = -1):
    to_send = 1
    if gid not in current_games:
        return {'Result': '[-22]', 'Desc': 'Error - no game with given id'}
    if r_id == -1:
        if gid in game_to_room_map:
            return {"Result": '-2', "Desc": 'Room ID also needed'}
        del current_games[gid]
    else:
        if gid not in game_to_room_map:
            return {"Result": '-1', "Desc": 'Game room does not exist'}
        else:
            if game_rooms[game_to_room_map[gid]].game_id != r_id:
                return {"Result": '-3', "Desc": 'Wrong Room ID'}
            await game_rooms[game_to_room_map[gid]].broadcast_move({'Move': 'Game End', 'Result': '[0]',
                                                                    'Desc': 'OK', 'more:': to_send})
            game_rooms[game_to_room_map[gid]].state = 1
            game_rooms[game_to_room_map[gid]] = -1
            game_to_room_map.pop(gid, None)
            del current_games[gid]

    return {'Result': '[0]', 'Desc': 'OK', 'more:': str(to_send)}


app.mount("/", socket_app)  # Here we mount socket app to main fastapi app


@sio.on("join_broadcast_group")
async def join_broadcast_group(sid, msg):
    print(msg)
    res = json.loads(msg)
    try:
        rn = res['rn']
        pn = res['pn']
        pc = int(res['pc'])
    except Exception as e:
        return
    if rn not in game_rooms:
        await sio.emit("join_request_reply", '{"Result": "-1", "Desc": "Game room does not exist"}')
        return {"Result": "-1", "Desc": "Game room does not exist"}
    if pn not in game_rooms[rn].players:
        await sio.emit("join_request_reply", '{"Result": "-7", "Desc": "Player does not exist in room"}')
        return {"Result": "-7", "Desc": "Player does not exist in room"}
    if game_rooms[rn].player_codes[pn] != pc:
        await sio.emit("join_request_reply", '{"Result": "-35", "Desc": "Wrong Player Code"}')
        return {"Result": "-35", "Desc": "Wrong Player Code"}
    game_rooms[rn].broadcast[pn] = sid
    await sio.emit("join_request_reply", '{"Result": "1", "Desc": "Success"}')
    return {"Result": "1", "Desc": "Success"}


def run_server():
    print(f"Listening on port 80...")
    uvicorn.run("server:app", host="132.69.8.19", port=80)

    #  print(f"Listening on port 5000...")
    #  uvicorn.run("server:app", host="127.0.0.1", port=5000)


if __name__ == '__main__':
    run_server()
