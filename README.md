# Laugh-Little-Lamb

Updated Unity 2022.3.17f project for the game "Laugh Little Lamb" from the Global Game Jam 2024.

### This project is a "Polished" version of the Original project from the GameJam.

Some changes from the original version are:

- Some animations have been added/or molished.
- Scripts names and folder structure have been changed making the project cleaner.
- Bug Fixes:
  - Pathfinding crashed under some circunstances.
  - Do/Undo system sometimes executed in the wrong order when a level had multiple enemies. 

### Nonetheless the main design decisions of the project have been preserved, and no mayor refactoring was done.


## Core objectives

Besides the GGJ24 Theme "Make me Laugh", we also wanted to make a game that could be a "Test ground" for some features we needed to try before adding to our other main project "Legion" (https://twitter.com/PlayLegionGame).
Because of that we decided to make a "Turn Based" game where multiple entities could execute "Composed Actions" in order **but** play their animations in "parallel" (if said animations do not share participants). 

## Example project for beginners and intermediate programmers
I personally also wanned to share a "real example" of how to apply some common programming patters in an "Real Game" instead of the common "Whiteboard examples" that people usually use on classrooms or youtube videos. Something that could have helped me when I started making games on my own 8 years ago.
So if you are a beginner or a intermediate programmer this project could be of use for you. _Keep in mind_, this code is MY interpretation/implementation of these patterns and ideas for **this** especific game. 
You **will** find, bad code and bad design decisions, so take anything you see here with a grain of salt.


## Design Patterns and Ideas explored

- **MVC Pattern**
  -Clear separation between, Model, View, Controllers, reinforced with Assemplies, allowing for a faster and scalable script builds. This decoupling makes a lot easier to implement many of the following points.  
- **Command Pattern**
  - Every "Action" entities can perform(Walk, Rotate, Eat, TurnLightOn,TurnLightOff, etc), are encapsulated in a "Command Instance" that can "o() and Undo().
- **Command History**
  - With Do(), Undo() and Redo() functionality
- **Strategy Pattern** for the control of entities with the interface IPlayer.
  -Ai control for the Hyenas (EnemyAiController.cs),
  -Human control for Sheep (SheepControler.cs).
  -Ai control for Sheep (AutomaticSheepController.cs,). Only for editor special mode for automated level testing. Doesnt really work, still needs some new heuristic optimizations before it becomes useful, right now is just brute force.   
- **Independent Animation System** which allows for 3 modes:
  - None: No animation is played. (Useful for automated testing)
  - Sequencial: Every animations goes into a Queue, and start only after the previous animation ends.
  - Parallel: Every animation starts the moment it doesnt have any "dependent" animation running. Each animation instance has an array of Participants, which is used for determining dependencies.
- **UniTask** for async programming in some places for convenience.

