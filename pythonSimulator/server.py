from flask import Flask
import uvicorn
import fastapi
from multiprocessing import Process
import datetime

from eventgame import EventGame
from interactivegame import InteractiveGame
from player import Player
import socket
import numpy as np
import struct
from buildChallenge import BuildChallenge

app = fastapi.FastAPI()

current_games = {}

"""
EXAMPLE GAME:
http://127.0.0.1:5000/start_new_game?p1=p_1&p2=p_2&p3=AI_Player1&p4=AI_Player2
http://127.0.0.1:5000/first_move?gid=1298297976736304479&piece=2&perm=1
http://127.0.0.1:5000/reg_move?gid=1298297976736304479&p_num=1&piece=15&perm=1&x_coor=15&y_coor=13
http://127.0.0.1:5000/pass_turn?gid=1298297976736304479&p_num=2
http://127.0.0.1:5000/ai_move?gid=1298297976736304479
http://127.0.0.1:5000/end_game?gid=1298297976736304479
"""

@app.get('/')
async def home():
    return {"message": "Hello World"}


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
    print("hi",gid not in current_games)
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


@app.get('/ai_move')
async def ai_move(gid: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    if game.players[game.curr_player_num - 1].ai_player:
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
    to_send=np.append(to_send,game.last_new_squares)
    return {'Result': str(to_send)}


@app.get('/pass_turn')
async def pass_turn(gid: int, p_num: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    game = current_games[gid]
    to_send = [game.pass_turn(p_num), game.last_new_squares]
    return {'Result': str(to_send)}


@app.get('/end_game')
async def end_game(gid: int):
    if gid not in current_games:
        return {'Result': '[-22]'}
    del current_games[gid]
    return {'Result': '[1]'}


def run_server():
    print(f"Listening on port 5000...")
    uvicorn.run("server:app", host="127.0.0.1", port=5000)


if __name__ == '__main__':
    run_server()