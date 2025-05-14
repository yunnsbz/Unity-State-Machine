# Unity AI State Machine

This project is a modular **state machine** system designed for managing AI behaviors in the Unity game engine. It includes various AI states (e.g., Idle, Attack, SearchForTarget, TakeRange) and control mechanisms (e.g., Movement, Attack, Perception) to create flexible and reusable AI behavior systems for game developers.

<br>

## Features and Todos:
- [x] **Built-in States:** Defines different states for AI entities (Idle, Attack, SearchForTarget, TakeRange).
- [x] **Control Systems:** Manages AI behaviors with controllers for Movement, Attack, and Perception.
- [x] **Easy Integration:** Easily integrable and customizable within Unity projects.
- [x] **Flexible Architecture:** Allows adding new states and controllers with minimal effort.
- [ ] Modular States: change the states in the inspector without writhing any code

<br>

## File Structure
The project's scripts file structure is as follows:
``` python
📁Scripts
 ├── Entity.cs
 ├── PlayerController.cs
 │  📁StateMachine
 │   ├── AIController.cs              
 │   ├── State.cs                     
 │   ├── StateMachine.cs             
 │   │  📁AIStates
 │   │   ├── AIState_Attack.cs          
 │   │   ├── AIState_Idle.cs          
 │   │   ├── AIState_SearchForTarget.cs
 │   │   ├── AIState_TakeRange.cs       
 │   │  📁Controllers
 │   │   ├── AIController_Attack.cs    
 │   │   ├── AIController_Movement.cs  
 │   │   ├── AIController_Perception.cs
```

<br>

## Installation and Setup
1. **Unity Version:** The project has been tested with Unity 2021.3.
2. **Clone the Repository:**
   ```bash
   https://github.com/yunnsbz/Unity-State-Machine.git
3. **Open in Unity:** Open the cloned project using Unity Hub.

4. Set Up the Scene:
    - Create an enemy entity (GameObject): add the Entity, AIController and a NavMeshAgent component.
    - set the layer to ```Enemy```.
    - Add controllers (Movement, Perception, Attack) and connect them to the AIController.
    - set target (```Player```) and non-target (```Default, Enemy```) layers for the perception controller.
    - Create a player entity (GameObject): add the Entity and the PlayerController scripts.
    - set the layer to ```Player```.
7. Configure States: Customize AI behaviors using the states defined in the AIStates folder.

<br>

## Usage
1. Run the game. Use ```W A S D``` keys to control player character and observe the enemy AI behavior.

<br>

## Contributing
If you'd like to contribute to this project:

1. Fork the repository.

2. Create a new branch for your feature or bug fix.

3. Make your changes and submit a pull request.

<br>

## License
This project is licensed under the MIT LICENSE. See the LICENSE file for details.
