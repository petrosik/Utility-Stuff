# DetRandom

(deterministic friendly) [Random](https://learn.microsoft.com/en-us/dotnet/api/system.random?view=net-8.0) with exposed Seed and number of actions(Pulls) taken.

!!! tip
    Works very similar to normal [Random](https://learn.microsoft.com/en-us/dotnet/api/system.random?view=net-8.0)

## Fields
- **uint Pulls**  
    The number of actions taken from the creation of the random
- **int Seed**  
    Seed of the random
## Methods overview
### Base methods ([Random](https://learn.microsoft.com/en-us/dotnet/api/system.random?view=net-8.0))    
- **Next(int, int)**
- **Next(int)**
- **Next()**
- **NextDouble()**
- **NextSingle()**
- **NextInt64()**
- **NextInt64(int, int)**
- **NextInt64(int)**          
- **NextBytes(byte[])**  

### Extra methods  
- **[Export](methods.md#export)()**  
    Exports string that can be inported