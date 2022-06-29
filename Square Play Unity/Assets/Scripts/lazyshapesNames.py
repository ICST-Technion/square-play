import requests;

if __name__ == '__main__':
    '''names ={"Six","H","shape3","Gimel","F","E","P","A","notnot","ground","Five","chair","G","C","Seven","Lambda"}
    with open("lazy.txt","w") as f:
        for name in names:
            #f.write(name+"Shape = ("+name+"Class)AssetDatabase.LoadAssetAtPath(\"Assets/Prefabs/Shapes/" + name + ".prefab\", typeof("+name+"Class));\n")#for loading assets from library
            #f.write("{\""+name+"\",("+name+"Shape,typeof("+name+"Class))},\n")#for dictionary setup
            #f.write("shapeLibrary[\""+name+"\"] = (("+name+"Class)AssetDatabase.LoadAssetAtPath(\"Assets/Prefabs/Shapes/"+name+".prefab\", typeof("+name+"Class)), shapeLibrary[\""+name+"\"].Item2);\n")
            f.write("playerShapes.Add(Instantiate(("+name+"Class)AssetDatabase.LoadAssetAtPath(\"Assets/Prefabs/Shapes/"+name+".prefab\", typeof("+name+"Class))) as "+name+"Class);\n")
        f.close()'''
        #Test scenario: I create a game in the Unity, and all other 3 join with apitester. Than first move is sent also via apitester

    first = "http://132.69.8.19:80/join_waiting_room?rn=J&pn=ma"
    print(requests.get(first).json())
    sec ="http://132.69.8.19:80/join_waiting_room?rn=J&pn=adi"
    print(requests.get(sec).json())
    third = "http://132.69.8.19:80/join_waiting_room?rn=J&pn=tal"
    r=requests.get(third).json()
    print(r)
    gid =input()
    
    pc =r["player_code"]

    last = "http://132.69.8.19:80/first_move_multi?gid="+gid+"&pn=tal&pc="+pc+"&piece=8&perm=1"
    print(requests.get(last))

