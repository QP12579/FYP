# Simple Choice System

## Overview

The Simple Choice System is a plugin developed for the Unity game engine, designed to enhance gameplay by providing players with diverse options and strategies. This plugin supports level-based upgrades and random selection of game skills, items, traps, and weapons, giving players more choices in the game.

## Features

- **Level-Based Upgrades**: Automatically unlocks new skills, items, traps, and weapons as players progress.
- **Random Selection**: Provides a random selection mechanism to increase replayability.
- **Customizable Parameters**: Allows developers to customize the types and attributes of skills, items, traps, and weapons.
- **User-Friendly Interface**: Easy integration with Unity's interface.

## Installation

1. Download the plugin and import it into your Unity project.
2. Ensure you are using a supported version of Unity.
3. Follow the documentation instructions for configuration.

## Usage Instructions

### Core Components

- **itemData**: Stores item data; you can add or remove enum values as needed.
- **item**: Converts a list of itemData into a scriptable object.
- **Choice**: Provides a randomly generated choice (weapon, skill, item, trap).
- **Player**: Provides functionality to save the selected data.
- **ChoiceManager**: Manages the number of choices available for selection.
- **LevelManager**: Controls grading and tests whether the choices meet the requirements.

### Testing

We tested the plugin in a project and concluded the following:

- Developers can customize the options they provide, including skills, items, traps, and weapons, along with their details.
- The choices appear randomly and without repetition.

## Deployment & Maintenance

We will continue to upgrade this plugin in the future, adding new features to improve user experience, such as introducing a skill tree interface to display selected functions.

## Contact Information

For any questions or suggestions, please feel free to contact us.

## References

- [Unity Documentation](https://docs.unity3d.com/cn/2021.3/Manual/class-ScriptableObject.html)

- [GUI Parts](https://assetstore.unity.com/packages/2d/gui/icons/gui-parts-159068)
- [2D SKILLS ICON SET - HANDPAINTED](https://assetstore.unity.com/packages/2d/gui/icons/2d-skills-icon-set-handpainted-210622)
- [YouTube Tutorials]
- [How to make Skill Trees (E01 Display Skill Info)](https://www.youtube.com/watch?v=CWyJF9k4wyE)
- [SCRIPTABLE OBJECTS in Unity](https://www.youtube.com/watch?v=aPXvoWVabPY&ab_channel=Brackeys)
- [ScriptableObject Item Database (Part 01) [Unity Tutorial]](https://www.youtube.com/watch?v=iLrF_tnB__A&ab_channel=Comp-3Interactive)