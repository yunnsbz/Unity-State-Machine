# Unity AI State Machine

This project is a modular **state machine** system designed for managing AI behaviors in the Unity game engine. It includes various AI states (e.g., Idle, Attack, SearchForTarget, TakeRange) and control mechanisms (e.g., Movement, Attack, Perception) to create flexible and reusable AI behavior systems for game developers.

## Features
- **Modular States:** Defines different states for AI entities (Idle, Attack, SearchForTarget, TakeRange).
- **Control Systems:** Manages AI behaviors with controllers for Movement, Attack, and Perception.
- **Easy Integration:** Easily integrable and customizable within Unity projects.
- **Flexible Architecture:** Allows adding new states and controllers with minimal effort.

## File Structure
The project's core file structure is as follows:
```
📁Scripts
 ├── Entity.cs  
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



## Installation
1. **Unity Version:** The project has been tested with Unity 2021.3.
2. **Clone the Repository:**
   ```bash
   https://github.com/yunnsbz/Unity-State-Machine.git
5. **Open in Unity:** Open the cloned project using Unity Hub.

6. Set Up the Scene: Create an AI entity (GameObject) and add the AIController component.

7. Configure States: Customize AI behaviors using the states defined in the AIStates folder.

## Usage
1. Create an AI Entity: Create a GameObject and attach the Entity script.

2. Add AIController: Attach the AIController component to the same GameObject.

3. Assign States: In the AIController component, specify the states (e.g., AIState_Idle, AIState_Attack) for the AI to use.

4. Test: Run the game and observe the AI behavior.

## Contributing
If you'd like to contribute to this project:

1. Fork the repository.

2. Create a new branch for your feature or bug fix.

3. Make your changes and submit a pull request.

## License
This project is licensed under the MIT LICENSE. See the LICENSE file for details.


