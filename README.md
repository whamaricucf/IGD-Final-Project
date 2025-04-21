# IGD-Final-Project: Weapons & Object Pooling

## Weapon System Implementation

Our game now includes three auto-firing weapons inspired by *Vampire Survivors*. Each weapon activates and handles itself without user input beyond movement.

### Magic Wand  
- Automatically targets the closest enemy.  
- Fires a homing projectile.  
- Uses **object pooling** for performance.  
- Each projectile deals damage, then returns to the pool on impact or timeout.

### King Bible  
- Rotates around the player in orbit.  
- Multiple bibles are spawned in a circular pattern.  
- Each bible fades in and out to create a pulsing effect.  
- Uses **object pooling** to reuse Bible GameObjects instead of destroying them.

### Garlic (Aura)  
- Constantly active forcefield surrounding the player.  
- Does **not** use object pooling, as it's a continuous effect.  
- Includes a Shader Graph gradient visual and a particle system.  
- Visual fades from transparent in the center to more opaque on the edge using a radial gradient shader.

---

## Object Pooling System

### `ObjectPooler.cs`
- Singleton-based pooling manager.
- Pools are defined using a list of `Pool` structs (tag, prefab, size).
- Reusable via:  
  ```csharp
  ObjectPooler.Instance.SpawnFromPool("Tag", position, rotation);
