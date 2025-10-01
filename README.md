# **About**
Just a collection of code that i use in most of my projects.   
[Documentation](https://petrosik.github.io/Utility-Stuff/)
## Features

- Collection of utility methods
- Collection of utility methods for UnityEngine 
- Collection of utility methods for ImageSharp
- Sqlite Helper
- Chance Table
- Deterministic Random
- Versioning Tool
- Pathfinding
- Save Manager
- Bidirectional Dictionary

# **Usage**
If you want to use it, either:
- ### Build on your own
    Download the rep and build it. it will give you a `.dll` and `.xml` file you can jut use in your projects.
- ### Release
    If i donť forget and the release is up-to date you can just download that.
> [!IMPORTANT]
> Don't forget to keep the xml next to the dll, if you want descriptions.

> [!IMPORTANT]
> This package does not have references automaticly specified in the package. As it depends on what you want to use (otherwise it would download all of them), so it will throw error if it can't find those packages but you can just add them manually through nugget manager

> [!WARNING]
> If you don't disable post build events it will attempt to copy the exported nugget package to the path in the event, which might or might not work.

## Known Issues
- Unity is stupid and loads the entire package even if you are using only the unity part so you might have to just copy the unity class/methods directly into your project.