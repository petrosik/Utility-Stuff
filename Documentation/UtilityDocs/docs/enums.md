# Enums
<p style="margin:0"><code>Serializable</code></p> 

###### Rarity
```csharp
None = 0
Common = 45
Uncommon = 21
Rare = 10
Epic = 4
Legendary = 2
Artifact = 1
```
!!! note
    More common bigger number
<p style="margin:0"><code>Serializable</code></p> 
<p style="margin:0"><code>Flags</code></p> 

###### GeneralDirections
```csharp
None = 0
Up = 1
Down = 2
Left = 4
Right = 8
Fowards = 16
Backwards = 32
```

<p style="margin:0"><code>Serializable</code></p> 

###### SQLOptions
```csharp
Save
Load
SaveAll
LoadAll
Delete
Update
Sync 
``` 
<p style="margin:0"><code>Serializable</code></p> 

###### InfoType 
```csharp
Info
Warn
Error
Important
```

###### VersioningActionType
```csharp
None = -1
ModifyValue
AddProperty
RemoveProperty
ModifyProperty
AddMethod
RemoveMethod
ModifyMethod
```
!!! note
    These are not implemented:
    
    - AddMethod  
    - RemoveMethod  
    - ModifyMethod  

<p style="margin:0"><code>Serializable</code></p> 

###### PathOccupancy
```csharp
Blocked = 0 
Clear = 1
LowP = 2
MediumP = 4
HighP = 8
Path = 128
```