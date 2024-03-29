# Machine Learning Roguelike

![MLRoguelike](https://i.imgur.com/Cxf4PaK.png)

## Description

A small Roguelike game that uses Machine Learning to power its entities. Both the player and its enemies are ML Agents, and the demo is a good playground to test Machine Learning in a real game environment. A scene specifically for training is included, to demonstrate how to train the agents in a different environment than the one where the game is going to happen. This demo also features the use of Cinemachine for 2D and Tilemap.<br><br>
Originally used in Codemotion (Milan) and DevGAMM (Minsk) talks by Ciro Continisio &amp; Alessia Nigretti.<br>

## Documentation

**Objective**

This project is intended to demonstrate a practical application of the Machine Learning Agents in a real game.

**Usage instructions**

Please note that this project is using v0.2.1d of Unity ML-Agents.

To try out the project, you need to add the [Tensorflow Sharp plugin](https://s3.amazonaws.com/unity-ml-agents/0.5/TFSharpPlugin.unitypackage) to your Assets folder. More information on how to set up Tensorflow Sharp Support is provided [here](https://github.com/Unity-Technologies/ml-agents/blob/0.2.1d/docs/Getting-Started-with-Balance-Ball.md).<br> 
To be able to train the agents, make sure that the Python API is installed in your system. [This](https://github.com/Unity-Technologies/ml-agents/blob/0.2.1d/docs/installation.md) is a guide on how to do it. Then, add the [python folder](https://github.com/Unity-Technologies/ml-agents/blob/0.2.1d/python) from the Machine Learning Agents repository to the project (outside the Assets folder).<br><br>
Refer to the [Machine Learning Agents wiki](https://github.com/Unity-Technologies/ml-agents/tree/0.2.1d) for further instructions on how to set up the project for external training.<br>

**Extra Materials**

Information on how this project was created is available on the [blog post](https://blogs.unity3d.com/2017/12/11/using-machine-learning-agents-in-a-real-game-a-beginners-guide/).<br>
Slides: [Link](https://docs.google.com/presentation/d/1Cs2r8eRLkcjqyKXUT5O96VAZ7NsvlNmSI1eFbGFhx3w/edit).<br>
Talk video: [Link](https://www.youtube.com/watch?v=ZIHJ28oz3hk).

**Software Requirements**

Required: Unity 2017.2, or later version

**Hardware Requirements**

Required: Any computer (Win or Mac)

**Owner and Responsible Devs**

Owners: Alessia Nigretti (alessian@unity3d.com), Ciro Continisio (ciro@unity3d.com)
Original graphics: Michele "Buch" Bucelli on [OpenGameArt](https://opengameart.org/content/a-blocky-dungeon) under CC0 License

**Major Change Log**
- 24 Oct: Created repository
- 14 Nov: First real working copy
- 27 Nov: Updated for public use, added license, Readme
- 11 Dec: Repository is public
