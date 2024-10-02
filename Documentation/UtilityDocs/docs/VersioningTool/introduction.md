# Versioning Tool
Tool for automaticly updating classes and structs from some state to other. In future they will also be able to update methods.

## Fields
- IReadOnlyDictionary&lt;Version, [VersioningAction](./versioningaction.md)> Actions


## Methods overview
- **Add(Version, [VersioningAction](./versioningaction.md))**  
    Adds Action
<br><br>

- **Remove(Version)**  
    Removes Action
    <br><br>

- **Modify(Version, [VersioningAction](./versioningaction.md))**  
    Modifies Action specified by VersioningAction
    <br><br>

- **Insert(Version, [VersioningAction](./versioningaction.md))**  
    Insesrts VersioningAction into the list. If the version is already inside it shifts it and all after that conflicts
    <br><br>

- **Shift(Version, Version)**  
    Moves the action on version to the version
    <br><br>

- **Clear()**  
    Removes all Actions
    <br><br>

- **CompleteUpdate(TIn, Version, Version)**  
    Applies the updates to the object that are > than version and < than version and tries to convert it to the output type
    <br><br>

- **SimpleUpdate(object, Version, Version)**  
    Applies the updates to the object that are > than version and < than version
    <br><br>

- **AutoGenerateVersions(object, object, Version?, Version?)**  
    Tries to generate actions that will change the current object into the target. Additionally you can specify what version they will start and tries to fit into the version
    <br><br>