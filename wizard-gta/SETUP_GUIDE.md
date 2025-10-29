# Complete Setup Guide - Stealth System

All scripts are now in `Assets/Scripts/` folder.

---

## 📋 GameObject Setup Guide

### 🎮 **1. Player GameObject**

**Required Components:**
- `PlayerMovement.cs`
- `SoundEmitter.cs`
- `SurfaceDetector.cs` (optional, for automatic surface detection)
- Rigidbody2D
- Collider2D

**Setup:**
1. Add all three scripts to your Player GameObject
2. Configure `PlayerMovement.cs`:
   - Set Move Speed (default: 5)
   - Set Footstep Interval (default: 0.4s)
   - Set Dash Sound Intensity (default: 5)
   - Set Dash Sound Range (default: 3)
3. Configure `SoundEmitter.cs`:
   - Set Base Noise Intensity (default: 3)
   - Set Max Sound Range (default: 2)
   - Enable Automatic Surface Detection
4. Configure `SurfaceDetector.cs` (if using):
   - Set Detection Distance (default: 1)
   - Set Surface Layer Mask
   - Configure tags for different surfaces

---

### 🚔 **2. Guard/Enemy GameObject**

**Required Components:**
- `EnemyMovement.cs`
- `SoundListener.cs`
- Rigidbody2D
- Collider2D

**Setup:**
1. Add both scripts to your Guard GameObject
2. Configure `EnemyMovement.cs`:
   - **Patrol Settings:**
     - Square Size (default: 10)
     - Patrol Speed (default: 1)
   - **Detection Settings:**
     - Chase Distance (default: 7)
     - Notice Distance (default: 15)
   - **Movement Speeds:**
     - Investigate Speed (default: 3)
     - Chase Speed (default: 5)
   - **Search Settings:**
     - Search Duration (default: 3)
     - Investigation Tolerance (default: 0.5)
   - **Boundaries:**
     - Set Min/Max X and Y positions
   - **References:**
     - Assign Player Transform
3. Configure `SoundListener.cs`:
   - Set Hearing Sensitivity Threshold (default: 1.0)
   - Set Max Hearing Range (default: 8)
   - Set Wall Layer Mask (select "Wall" layer)
   - Enable Show Hearing Range for debugging

---

### 🌍 **3. Scene Manager (Empty GameObject)**

**Required Components:**
- `NoiseManager.cs`
- `AlertManager.cs`

**Setup:**
1. Create an empty GameObject named "GameManager" or "SystemManager"
2. Add both scripts to it
3. Configure `NoiseManager.cs`:
   - **Wall Settings:**
     - Set Wall Layer Mask (select "Wall" layer)
     - Wall Dampening Factor (default: 0.5) - NOT USED, walls fully block
   - **Debug Settings:**
     - Enable Is Debug Mode
     - Enable Show Listener Ranges
     - Sound Event Display Time (default: 2s)
4. Configure `AlertManager.cs`:
   - **Alert Settings:**
     - Max Alert Level (default: 1.0)
     - Alert Decay Rate (default: 0.5)
     - Min Alert Level (default: 0.0)
   - **Heat Settings:**
     - Heat Increase Rate (default: 2.0)
     - Heat Decay Rate (default: 1.0)
     - Max Heat Level (default: 100)
   - **Memory Settings:**
     - Memory Duration (default: 10s)
     - Sound Memory Duration (default: 5s)
   - **Debug:**
     - Enable Show Debug UI to see alert/heat levels

---

### 🧱 **4. Walls/Obstacles**

**Setup:**
1. Create a Layer named "Wall" in Unity (Edit → Project Settings → Tags and Layers)
2. Set all wall GameObjects to the "Wall" layer
3. Ensure walls have Collider2D components
4. Walls will now block:
   - Sound propagation (completely)
   - Vision (line of sight)

---

### 🎨 **5. Surface Types (Optional)**

**For Different Sound Levels:**
1. Tag your ground tiles/surfaces:
   - Tag: "Grass" (default, normal sound)
   - Tag: "Carpet" (quiet, 0.5x multiplier)
   - Tag: "Wood" (normal, 1.0x multiplier)
   - Tag: "Metal" (loud, 1.5x multiplier)
   - Tag: "Glass" (very loud, 2.0x multiplier)
2. Player's `SurfaceDetector` will automatically detect these

---

## 🎯 Vision Cone Setup (If Using)

### **6. Vision Cone GameObject (Child of Guard)**

**If using the existing FieldOfView system:**

**Required Components:**
- `FieldOfView.cs`
- MeshFilter
- MeshRenderer
- Material (for visualization)

**Setup:**
1. Create a child GameObject under your Guard named "VisionCone"
2. Add `FieldOfView.cs` script
3. Add MeshFilter and MeshRenderer components
4. Create a material for the cone visualization
5. Configure `FieldOfView.cs`:
   - Assign Enemy Script (parent EnemyScript if using)
   - Set FOV angle (default: 90)
   - Set View Distance (default: 10)
   - Set Ray Count (default: 50)
   - Set Obstacle Mask (select "Wall" layer)
   - Assign Player Transform
   - Enable Show State for debugging
   - Set Partial Detection Time (default: 2s)

---

## 🔧 Layer Setup Summary

Create these layers in Unity:
1. **Wall** - For walls and obstacles that block sight/sound
2. **Player** - For player character
3. **Enemy** - For guards/enemies

---

## 📊 Testing & Debugging

### Debug Features Available:

**NoiseManager:**
- Sound radius visualization (orange spheres)
- Shows sound intensity rings
- Toggle with `Is Debug Mode`

**AlertManager:**
- On-screen UI showing:
  - Current alert level
  - Current heat level
  - Last known player position
  - Time since last sighting
- Toggle with `Show Debug UI`

**SoundListener:**
- Hearing range visualization (green spheres)
- Lines to investigated sounds
- Toggle with `Show Hearing Range`

**EnemyMovement:**
- Boundary box visualization (red)
- Patrol waypoints (blue spheres)
- Always visible in Scene view

---

## 🎮 How It All Works Together

1. **Player moves** → `PlayerMovement` → `SoundEmitter` emits footsteps
2. **SoundEmitter** → `NoiseManager` broadcasts sound event
3. **NoiseManager** → All `SoundListener` components check if they can hear
4. **SoundListener** checks:
   - Distance to sound
   - Wall blocking (if wall present, sound completely blocked)
   - Sound intensity
5. **If sound heard** → `SoundListener` → `EnemyMovement` investigates
6. **If player seen** → `EnemyMovement` → `AlertManager` raises alert/heat
7. **AlertManager** → Shares last known position with all guards
8. **Alert/Heat decays** over time when player is hidden

---

## ✅ Quick Checklist

- [ ] All scripts in `Assets/Scripts/` folder
- [ ] Player has: PlayerMovement, SoundEmitter, SurfaceDetector
- [ ] Guards have: EnemyMovement, SoundListener
- [ ] Scene has: NoiseManager, AlertManager (on one GameObject)
- [ ] "Wall" layer created and assigned to walls
- [ ] Wall Layer Mask set in NoiseManager and SoundListener
- [ ] Player Transform assigned to guards
- [ ] Debug modes enabled for testing
- [ ] Surface tags created (optional)

---

## 🚨 Common Issues

**Guards don't hear sounds:**
- Check Wall Layer Mask in SoundListener
- Ensure NoiseManager exists in scene
- Check sound intensity and hearing range values

**Sounds go through walls:**
- Ensure walls are on "Wall" layer
- Check Wall Layer Mask in NoiseManager
- Verify walls have Collider2D components

**Alert system not working:**
- Ensure AlertManager exists in scene
- Check that it's named correctly and script is enabled
- Enable debug logs to see events

---

## 📝 Script Locations

All in `Assets/Scripts/`:
- `PlayerMovement.cs` → Player GameObject
- `SoundEmitter.cs` → Player GameObject
- `SurfaceDetector.cs` → Player GameObject
- `EnemyMovement.cs` → Guard GameObjects
- `SoundListener.cs` → Guard GameObjects
- `NoiseManager.cs` → Scene Manager GameObject
- `AlertManager.cs` → Scene Manager GameObject
- `FieldOfView.cs` → Vision cone child (if using)
- `PlayerFOV.cs` → Player vision cone child (if using)
- `EnemyScript.cs` → Alternative guard system (if using)

---

**You're all set! 🎉**

