if __name__ == '__main__':
    names ={"Six","H","shape3","Gimel","F","E","P","A","notnot","ground","Five","chair","G","C","Seven","Lambda"}
    with open("lazy.txt","w") as f:
        for name in names:
            #f.write(name+"Shape = ("+name+"Class)AssetDatabase.LoadAssetAtPath(\"Assets/Prefabs/Shapes/" + name + ".prefab\", typeof("+name+"Class));\n")#for loading assets from library
            #f.write("{\""+name+"\",("+name+"Shape,typeof("+name+"Class))},\n")#for dictionary setup
            #f.write("shapeLibrary[\""+name+"\"] = (("+name+"Class)AssetDatabase.LoadAssetAtPath(\"Assets/Prefabs/Shapes/"+name+".prefab\", typeof("+name+"Class)), shapeLibrary[\""+name+"\"].Item2);\n")
            f.write("playerShapes.Add(Instantiate(("+name+"Class)AssetDatabase.LoadAssetAtPath(\"Assets/Prefabs/Shapes/"+name+".prefab\", typeof("+name+"Class))) as "+name+"Class);\n")
        f.close()