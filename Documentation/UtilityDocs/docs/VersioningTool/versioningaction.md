# Versioning Action

Class that contains actions that specify what needs to be done.

## Fields

- **[VersioningActionType](../enums.md#versioningactiontype) Type { get; set; }**   
    Action type
- **string? OriginName { get; set; }**  
    Name of the field that you want to modify
- **Type? OriginType { get; set; }**  
    Type of the field that you are modifying  
- **string? TargetName { get; set; }**  
    If you are changing or creating field this will be the final name
- **Type? TargetType { get; set; }**  
    If you are chanfing or creating field this will be the final type
- **object? Value { get; set; }**  
    Value that will be set in selected field
- **Func&lt;object, object>? ValueConverter { get; set; }**  
    This can be value converter or value modifer  
- **public bool IsField { get; set; }**  
!!! note
    Only applies for field/property changes  
    true = field, false = property  
- **FieldAttributes FAttributes { get; set; }**  
    Additional atributes that you want to set or modify for field atribute
- **PropertyAttributes PAttributes { get; set; }**  
    Additional atributes that you want to set or modify for property atribute

!!! note   
    OriginName and OriginType are pairs you need to fill out both. Same with target.  

## Methods
There are only constructors.  
!!! tip 
    Make sure to use correct constructor if you don't know what you are doing.  
    There are multiple preset constructors that you can use, and for the most part it's all you need.