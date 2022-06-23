import uvicorn
import fastapi
import datetime

from eventgame import EventGame
from player import Player
import numpy as np
from buildChallenge import BuildChallenge

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
{"state":"1","room_id":"417560573037949984","room_name":"1"}
http://127.0.0.1:5000/join_waiting_room?rn=1&pn=player2
{"state":"1","player_code":-7336321001789273646}
http://127.0.0.1:5000/activate_game?rn=1&r_id=417560573037949984
{"game_id":"5708920961190911877"}
http://127.0.0.1:5000/ai_move?gid=5708920961190911877
{"Result":"[15  1 -1]"}
http://127.0.0.1:5000/reg_move_multi?gid=5708920961190911877&pn=Admin_player&pc=417560573037949984&piece=8&perm=1&x_coor=15&y_coor=17
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


@app.get('/')
async def home():
    return {"message": "Hello World"}


@app.get('/create_waiting_room')
async def create_new_room(rn: str, p1: str):
    room_admin = p1
    room_name = rn
    if room_name in game_rooms:
        return {"ERR_NUM": '-2', "Desc": 'Game room with that name already exists'}

    room_id = hash(datetime.datetime.now().isoformat() + str(room_admin))
    game_rooms[room_name] = GameRoom(room_name, room_id, room_admin)
    return {'state': '1', "room_id": str(room_id), 'room_name': str(room_name)}


@app.get('/query_waiting_room')
async def query_waiting_room(rn: str):
    if rn not in game_rooms:
        return {'state': '-1', 'game_id': '-1'}
    else:
        return {'state': str(game_rooms[rn].state), 'game_id': str(game_rooms[rn].game_id)}


@app.get('/query_all_rooms')
async def query_all_rooms():
    games = str(game_rooms.keys())
    return {'Game list': games}


@app.get('/join_waiting_room')
async def join_waiting_room(rn: str, pn: str):
    if rn not in game_rooms:
        return {"ERR_NUM": '-1', "Desc": 'Game room does not exist'}
    if pn in game_rooms[rn].players:
        return {"ERR_NUM": '-3', "Desc": 'Game room already has a player with that name'}
    if game_rooms[rn].state > 1:
        return {"ERR_NUM": '-4', "Desc": 'Game has already begun'}
    if len(game_rooms[rn].players) >= 4:
        return {"ERR_NUM": '-6', "Desc": 'Room has too many players'}

    game_rooms[rn].players.append(pn)
    code = hash(datetime.datetime.now().isoformat() + str(pn))
    game_rooms[rn].player_codes[pn] = code
    return {'state': str(game_rooms[rn].state), 'player_code': code}


@app.get('/remove_from_room')
async def remove_from_room(rn: str, r_id: int, pn: str):
    if rn not in game_rooms:
        return {"ERR_NUM": '-1', "Desc": 'Game room does not exist'}
    if pn not in game_rooms[rn].players:
        return {"ERR_NUM": '-3', "Desc": 'Game room  does not have a player with that name'}
    if game_rooms[rn].state > 1:
        return {"ERR_NUM": '-4', "Desc": 'Game has already begun'}
    if pn == game_rooms[rn].admin:
        return {"ERR_NUM": '-5', "Desc": 'Cannot remove admin from room'}
    if r_id != game_rooms[rn].id:
        return {"ERR_NUM": '-6', "Desc": 'Only admin can remove from room'}
    game_rooms[rn].players.remove(pn)
    game_rooms[rn].player_codes.pop(pn, None)
    return {'state': '1'}


@app.get('/leave_room')
async def leave_room(rn: str, pn: str, pc: int):
    if rn not in game_rooms:
        return {"ERR_NUM": '-1', "Desc": 'Game room does not exist'}
    if pn not in game_rooms[rn].players:
        return {"ERR_NUM": '-3', "Desc": 'Game room  does not have a player with that name'}
    if game_rooms[rn].state > 1:
        return {"ERR_NUM": '-4', "Desc": 'Game has already begun'}
    if pn == game_rooms[rn].admin:
        return {"ERR_NUM": '-5', "Desc": 'Admin cannot leave room from room'}
    if pc != game_rooms[rn].player_codes[pn]:
        return {"ERR_NUM": '-6', "Desc": 'Only the player himself can leave room'}
    game_rooms[rn].players.remove(pn)
    game_rooms[rn].player_codes.pop(pn, None)
    return {'state': '1'}


@app.get('close_room')
async def close_room(rn: str, r_id: int):
    if rn not in game_rooms:
        return {"ERR_NUM": '-1', "Desc": 'Game room does not exist'}
    if game_rooms[rn].state > 1:
        return {"ERR_NUM": '-4', "Desc": 'Game has already begun'}
    if r_id != game_rooms[rn].id:
        return {"ERR_NUM": '-6', "Desc": 'Only admin can close room'}
    game_rooms.pop(rn, None)
    return {'state': '1'}


@app.get('/activate_game')
async def activate_game(rn: str, r_id: int):
    if rn not in game_rooms:
        return {"ERR_NUM": '-1', "Desc": 'Game room does not exist'}
    if game_rooms[rn].state > 1:
        return {"ERR_NUM": '-4', "Desc": 'Game has already begun'}
    if game_rooms[rn].id != r_id:
        return {"ERR_NUM": '-5', "Desc": 'Only room admin can start a game'}
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
    return {"game_id": str(new_game_id)}


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
    to_send = [game.start_game(piece, perm), game.last_new_squares]
    return {'Result': str(to_send)}


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
    to_send = [game.move(game.curr_player_num, piece, perm, x_coor, y_coor), game.last_new_squares]
    return {'Result': str(to_send)}


@app.get('/query_game_state')
async def query_game_state(gid: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    state = game.started
    player_num = game.curr_player_num
    return {'Started': state, 'Player Turn': player_num, 'Board': game.get_board()}


@app.get('/query_game_board')
async def query_game_state(gid: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    state = game.started
    player_num = game.curr_player_num
    return {'Board': game.get_board()}


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
    to_send = [game.pass_turn(game.curr_player_num), game.last_new_squares]
    return {'Result': str(to_send)}


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
    return {"game_id": str(new_game_id)}


@app.get('/first_move')
async def first_move(gid: int, piece: int, perm: int):
    print("hi", gid not in current_games)
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    if game.started:
        return {'Result': '[-33]'}

    to_send = [game.start_game(piece, perm), game.last_new_squares]
    return {'Result': str(to_send)}


@app.get('/reg_move')
async def reg_move(gid: int, p_num: int, piece: int, perm: int, x_coor: int, y_coor: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    if not game.started:
        return {'Result': '[-33]'}

    to_send = [game.move(p_num, piece, perm, x_coor, y_coor), game.last_new_squares]
    return {'Result': str(to_send)}


@app.get('/ai_move')  # ai_move is the same for multiplayer and singleplayer.
async def ai_move(gid: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
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
    return {'Result': str(to_send)}


@app.get('/pass_turn')
async def pass_turn(gid: int, p_num: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    to_send = [game.pass_turn(p_num), game.last_new_squares]
    return {'Result': str(to_send)}


@app.get('/end_game')
async def end_game(gid: int, r_id: int = -1):
    if gid not in current_games:
        return {'Result': '[-22]'}
    if r_id == -1:
        if gid in game_to_room_map:
            return {"ERR_NUM": '-2', "Desc": 'Room ID also needed'}
        del current_games[gid]
    else:
        if gid not in game_to_room_map:
            return {"ERR_NUM": '-1', "Desc": 'Game room does not exist'}
        else:
            if game_rooms[game_to_room_map[gid]].game_id != r_id:
                return {"ERR_NUM": '-3', "Desc": 'Wrong Room ID'}
            game_rooms[game_to_room_map[gid]].state = 1
            game_rooms[game_to_room_map[gid]] = -1
            game_to_room_map.pop(gid, None)
            del current_games[gid]
    return {'Result': '[1]'}


def run_server():
    print(f"Listening on port 80...")
    uvicorn.run("server:app", host="132.69.8.19", port=80)


if __name__ == '__main__':
    run_server()
