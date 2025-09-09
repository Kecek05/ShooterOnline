# FPS Movement Controller Documentation

## Overview
This FPS Movement Controller implementation follows SOLID principles and uses multiple design patterns to create a robust, extensible, and maintainable first-person movement system for Unity.

## Features
- **Physics-based Movement**: Smooth acceleration/deceleration using animation curves
- **Mouse Look Camera**: Cinemachine 3-based first-person camera with configurable sensitivity
- **Movement States**: Walk, Sprint, Crouch with different speed multipliers
- **Ground Detection**: Sphere-cast based ground detection with configurable layers
- **Jumping System**: Jump with cooldown and air control
- **Input Events**: Event-driven input system following Observer pattern
- **Smooth Crouching**: Height transition with configurable speed

## Architecture

### SOLID Principles Applied

1. **Single Responsibility Principle (SRP)**
   - `FPSController`: Orchestrates components
   - `FirstPersonCameraController`: Handles camera rotation
   - `GroundDetector`: Manages ground detection
   - `InputReaderSO`: Processes input events

2. **Open/Closed Principle (OCP)**
   - Movement strategies can be extended without modifying existing code
   - New camera behaviors can be added through interfaces

3. **Liskov Substitution Principle (LSP)**
   - All movement strategies inherit from `BaseMovementStrategy`
   - Components can be substituted with any implementation of their interfaces

4. **Interface Segregation Principle (ISP)**
   - `IMovementController`, `ICameraController`, `IGroundDetector` are focused interfaces
   - Each interface serves a specific purpose

5. **Dependency Inversion Principle (DIP)**
   - High-level modules depend on abstractions (interfaces)
   - Components are injected rather than created internally

### Design Patterns Used

1. **Strategy Pattern**
   - `WalkStrategy`, `SprintStrategy`, `CrouchStrategy`
   - Allows switching movement behaviors at runtime

2. **Observer Pattern**
   - Input events in `InputReaderSO`
   - Decoupled communication between components

3. **Dependency Injection**
   - ScriptableObjects for configuration
   - Interface-based component dependencies

## Setup Instructions

### Using the Editor Tool
1. Open Unity and go to `Tools > Kecek > FPS Controller Setup`
2. Click "Create FPS Player GameObject" to create the player structure
3. Click "Create Movement Settings SO" to create movement configuration
4. Click "Create Camera Settings SO" to create camera configuration
5. Assign the created ScriptableObjects to the FPSController component
6. Configure the ground layer mask in MovementSettings

### Manual Setup
1. Create a GameObject with:
   - `Rigidbody` (Freeze Rotation enabled)
   - `CapsuleCollider`
   - `FPSController` component
   - `GroundDetector` component
   - `FirstPersonCameraController` component

2. Create camera hierarchy:
   ```
   Player
   └── Camera Target (Y: 1.6)
       └── Camera Pitch Target
           └── Virtual Camera (CinemachineCamera)
   ```

3. Create ScriptableObject assets:
   - `MovementSettingsSO`
   - `CameraSettingsSO`
   - `InputReaderSO`

4. Assign all references in the FPSController component

## Configuration

### Movement Settings
- **Max Speed**: Base walking speed
- **Sprint Speed Multiplier**: Speed increase when sprinting
- **Crouch Speed Multiplier**: Speed reduction when crouching
- **Jump Force**: Vertical force applied when jumping
- **Ground Check Distance/Radius**: Ground detection parameters
- **Ground Layer Mask**: Which layers count as ground

### Camera Settings
- **Mouse Sensitivity**: Overall mouse sensitivity
- **Mouse Sensitivity X/Y**: Separate X and Y axis sensitivity
- **Min/Max Vertical Angle**: Camera pitch constraints
- **Invert Mouse Y**: Invert vertical mouse movement
- **Cursor Locked**: Lock cursor to screen center

## Usage Examples

### Basic Movement
The system automatically handles WASD movement with physics-based acceleration and deceleration.

### Sprinting
Hold Left Shift to sprint. Speed is multiplied by the sprint multiplier.

### Crouching
Hold C to crouch. Reduces speed and player height smoothly.

### Jumping
Press Space to jump. Includes cooldown and optional air control.

### Mouse Look
Move mouse to look around. Sensitivity and constraints are configurable.

## Extending the System

### Adding New Movement States
1. Create a new class inheriting from `BaseMovementStrategy`
2. Implement `MaxSpeed` and `Acceleration` properties
3. Add the strategy to the FPSController's strategy selection logic

### Custom Camera Behaviors
1. Implement the `ICameraController` interface
2. Replace the `FirstPersonCameraController` with your implementation

### Additional Input Actions
1. Add new events to `InputReaderSO`
2. Subscribe to these events in the appropriate controller

## Performance Considerations
- Ground detection uses sphere casting for accuracy
- Camera rotation is applied in LateUpdate for smooth visuals
- Physics calculations are performed in FixedUpdate
- Input events minimize coupling between systems

## Troubleshooting

### Player falls through ground
- Check ground layer mask in MovementSettings
- Ensure ground has appropriate layer assigned
- Verify ground check distance is appropriate

### Camera rotation issues
- Check camera target hierarchy is correct
- Verify virtual camera is properly configured
- Ensure Cinemachine Brain exists in the scene

### Movement feels unresponsive
- Adjust acceleration/deceleration curves in MovementSettings
- Check rigidbody mass and drag settings
- Verify input sensitivity settings