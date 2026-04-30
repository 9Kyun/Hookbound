# Hookbound

Hookbound is a 2D vertical action platformer where the player climbs upward through stages using fast movement, a grappling hook, dashes, attacks, and enemy interactions.

The core gameplay focuses on using the grappling hook to move quickly, chain actions together, and survive while progressing through vertical stages.

---

## Game Overview

In Hookbound, the player must climb a vertical map by using movement skills and combat actions.  
The player can hook onto terrain, enemies, and certain objects to move through the stage.

Different enemies react differently to attacks, slams, and hooks. Some enemies can be pulled, some are heavy, and some require specific actions before they can be defeated or used as hook targets.

After clearing stages or ending a run, the player can collect currency and use it for upgrades in the shop.

---

## Main Features

- 2D side-scrolling vertical platformer gameplay
- Player movement, jumping, and aerial control
- Grappling hook system
- Dash system with limited dash count
- Just Evade system for projectile timing
- Melee attack system
- Projectile parry interaction
- Downward slam attack
- Enemy state system
- Different enemy types and weights
- Currency pickup system
- Shop and inventory system
- Stage progression
- Result screen with run rewards
- UI for health, resources, currency, and game state

---

## Controls

| Action | Input |
|---|---|
| Move | A / D |
| Jump | Space |
| Aim | Mouse |
| Grappling Hook | Hook Button |
| Attack / Parry | Attack Button |
| Dash | Dash Button |
| Slam | Jump while airborne |
| Interact | E |
| Pause | Esc |

> Some controls may depend on the current Unity Input System settings.

---

## Core Gameplay Systems

### Grappling Hook

The grappling hook is the main movement mechanic.  
The player can aim with the mouse and hook onto valid targets.

Hook targets can include:

- Terrain
- Enemies
- Airborne objects
- Deflected projectiles
- Special hookable objects

The hook movement changes depending on the target type.  
For example, static targets pull the player directly, while light enemies can be pulled toward the player.

---

### Enemy Weight System

Enemies can have different weight types:

- **Static**: Does not move when hooked
- **Light**: Gets pulled toward the player
- **Heavy**: Moves only slightly when hooked

This allows each enemy to affect movement and combat differently.

---

### Combat

The player can attack enemies using melee attacks and slam attacks.

Some enemies can be defeated normally, while others require special conditions:

- Normal enemies can be damaged directly
- Armored enemies must have their armor broken first
- Weak point enemies must expose their weak point before taking damage

---

### Dash and Just Evade

The player can dash in different directions.  
During the early timing of the dash, the player can trigger a Just Evade when avoiding projectiles.

A successful Just Evade briefly slows time and allows the player to perform a follow-up charge action.

---

### Slam

The slam is an aerial downward attack.  
It allows the player to quickly descend and attack enemies below.

If the player defeats an enemy with a slam, the player bounces upward, allowing movement and combat to continue.

---

### Shop and Currency

Enemies and pickups can reward currency.  
Currency can be used in the shop to purchase upgrades or improve the player's abilities.

The project includes:

- Shop currency
- Reroll / perk currency
- Inventory-related systems
- Upgrade and item management

---

## Project Structure

This Unity project mainly uses the following folders:

```txt
Assets/
Packages/
ProjectSettings/
