# Methods

### Add
Adds Action
```csharp
void Add(Version version, VersioningAction action)
```
---
### Remove 
Removes Action
```csharp
void Remove(Version version)
```
---
### Modify
Modifies Action specified by version
```csharp
void Modify(Version version, VersioningAction action)
```
---
### Insert
Insesrts action into the list. If the version is already inside it shifts it and all after that conflicts
```csharp
void Insert(Version version, VersioningAction action)
```
---
### Shift
Moves the action on oldVersion to the newVersion
```csharp
void Shift(Version oldVersion, Version newVersion)
```
---
### Clear
Removes all Actions
```csharp
void Clear()
```
---
### CompleteUpdate
Applies the updates to the obj that are > than current and <= than target version and tries to convert it to the output type
```csharp
Tout CompleteUpdate(TIn obj, Version current, Version target)
```
---
### SimpleUpdate
Applies the updates to the obj that are > than current and &lt;= than target
```csharp
object SimpleUpdate(object obj, Version current, Version target)
```
---
### AutoGenerateVersions
Tries to generate actions that will change the current object into the target. Additionally you can specify what version they will start and tries to fit into the maxv version  

```csharp
void AutoGenerateVersions(object current, object target, Version? currentv = null, Version? maxv = null)
```
!!! warning
    Deletes all current actions!   
!!! note  
    I give up on trying to also get difference between values. mby in the future(prob not)
---