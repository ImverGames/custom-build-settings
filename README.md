## Description
The Custom Build Settings Tool is a tool designed to simplify and centralize the process of configuring and building applications in Unity. It allows developers to manage all settings in one window and integrates various external plugins, making the build and versioning process of the application easier.

## Key Features

### Centralization of Settings
- Consolidation of all necessary settings in a single user interface.
- Simplification of managing and configuring various aspects of the project.

### Integration with External Plugins
- Support for integration with a variety of plugins.
- Ability to enable/disable cheats and manage other plugins from one window.

### Simplification of the Build Process
- Setting build parameters, such as platform and resolution, in one place.
- Acceleration and simplification of the build preparation process.

### Versioning with Automatic Incrementation
- Automatic update of the build version identifier depending on the selected type.
- Adherence to Semantic Versioning principles (Major.Minor.Patch).
- Automatic incrementation of the relevant version component with each new build.

### Build Assignments
- Definition and configuration of different types of builds for various purposes (testing, demonstration, release).

# Introduction

This documentation describes the process of creating custom plugins for the build settings editor window in Unity.

## Core Components

### `IBuildPluginEditor` Interface

All custom plugins must implement the `IBuildPluginEditor` interface. This interface includes the following methods:

- `InvokeSetupPlugin`: Plugin initialization.
- `InvokeOnFocusPlugin`: Actions when the plugin is focused.
- `InvokeGUIPlugin`: Rendering of the user interface.
- `InvokeDestroyPlugin`: Cleanup of plugin resources.

### `BuildIncrementorData` Class

This class is used for storing and transferring data between the plugin and the build settings editor.

### `BuildValue<T>` Class

`BuildValue<T>` is a generic class used for storing values and tracking their changes in the editor.

## Creating Your Own Plugin

To create your own plugin:

1. **Implement `IBuildPluginEditor`**: Create a class that implements `IBuildPluginEditor`.
2. **Initialize Data**: Initialize necessary data in `InvokeSetupPlugin`.
3. **Implement UI**: Create a user interface in `InvokeGUIPlugin`.
4. **Handle Events**: Add handlers for value changes or other user actions.
5. **Clean Up Resources**: Ensure all event subscriptions and used resources are cleaned up in `InvokeDestroyPlugin`.

## Conclusion

Creating custom plugins for the build settings editor in Unity allows for extending the functionality of the editor and improving the game development process. Follow the provided example and the guidelines in this documentation to create your own plugins.