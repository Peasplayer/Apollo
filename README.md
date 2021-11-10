# Apollo - Custom Map API
Easily make custom maps with Unity prefabs!

**Currently in development**

## Guide
You have to create a unity project - Version 2020.3.7. Then you make a Game-object and call it "Map". It will be the parent object for you entire map. Then you create a child object for a room. You can name it anything you want. This room object needs to have to children. One needs to be called "Ground" and the other one "Room". Add an sprite render to the ground object for the texture of your room. Also add an edge collider to mark the walls of your room
Add an edge collider to the object named "Room". That will be your shadows of the room
The "Room" object also gets a child named "AreaCollider". Add an polygon collider to it and mark the area of the room. If the player is in that area it will get shown the name of the room on the bottom of the screen. Add a child to the map object and name it "[SPAWN]". It marks the spawnpoint of the players. You can also add an object named "EmergencyButton", "SecurityPanel" and "CustomizeLaptop". Those are pretty self-explaining. You can add cams, vents and ladders to a room. Just add a child to the "Ground" object and name it anyway you want !!!

## Features 
Seperated from Polus, so you can still play Polus with the map installed\
Easy to use (if experienced with Unity)\
Customizable
