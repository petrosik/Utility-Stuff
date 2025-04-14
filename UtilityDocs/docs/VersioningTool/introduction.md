# Versioning Tool
Tool for automaticly updating classes and structs from some state to other. In future they will also be able to update methods.

## Fields
- **IReadOnlyDictionary&lt;[Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0), [VersioningAction](./versioningaction.md)> Actions**
    Contains all actions that 

## Methods overview
- **[Add](./methods.md#add)([Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0), [VersioningAction](./versioningaction.md))**  
    Adds Action
    <br><br>

- **[Remove](./methods.md#remove)([Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0))**  
    Removes Action
    <br><br>

- **[Modify](./methods.md#modify)([Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0), [VersioningAction](./versioningaction.md))**  
    Modifies Action specified by VersioningAction
    <br><br>

- **[Insert](./methods.md#insert)([Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0), [VersioningAction](./versioningaction.md))**  
    Insesrts VersioningAction into the list. If the version is already inside it shifts it and all after that conflicts
    <br><br>

- **[Shift](./methods.md#shift)([Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0), [Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0))**  
    Moves the action on version to the version
    <br><br>

- **[Clear](./methods.md#clear)()**  
    Removes all Actions
    <br><br>

- **[CompleteUpdate](./methods.md#completeupdate)(TIn, [Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0), [Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0))**  
    Applies the updates to the object that are > than version and < than version and tries to convert it to the output type
    <br><br>

- **[SimpleUpdate](./methods.md#simpleupdate)(object, [Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0), [Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0))**  
    Applies the updates to the object that are > than version and < than version
    <br><br>

- **[AutoGenerateVersions](./methods.md#autogenerateversions)(object, object, [Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0)?, [Version](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=net-8.0)?)**  
    Tries to generate actions that will change the current object into the target. Additionally you can specify what version they will start and tries to fit into the version
    <br><br>