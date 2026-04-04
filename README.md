# Dom's Upgradeable Mix Station Mod

**Dom's Upgradeable Mix Station mod** adds a fully-featured upgrade and enhancement system to the **Mixing Station Mk2** in *Schedule I*, giving players more control and progression in their drug manufacturing empire.

---

## Requirements

- **MelonLoader** must be installed for the game
- Use the latest version of the game

---

## Compatibility

**Works with Mono mods, cannot guarantee compatability with Il2Cpp

---

## Features

### Speed Upgrades
Upgrade your Mixing Station Mk2 through **5 speed tiers**, each reducing mixing time:

| Tier | Cost | Effect | Materials Required |
|------|------|--------|--------------------|
| Quick Mix | $3,000 | 80% of original time | 2x Mixing Gear |
| Rapid Mix | $6,000 | 60% of original time | 3x Mixing Gear, 1x Precision Component |
| Swift Mix | $8,000 | 40% of original time | 4x Mixing Gear, 2x Precision Component |
| Blitz Mix | $10,000 | 20% of original time | 3x Precision Component, 1x Prototype Core |
| Instant Mix | $20,000 | Completes in 1 minute | 5x Precision Component, 3x Prototype Core |

### Quality Floor Upgrades
Upgrade your station to guarantee a **minimum product quality**, through **3 quality tiers**:

| Tier | Cost | Minimum Quality | Materials Required |
|------|------|----------------|--------------------|
| Standard Floor | $5,000 | Standard | 3x Mixing Gear, 1x Precision Component |
| Premium Floor | $12,000 | Premium | 4x Precision Component, 1x Prototype Core |
| Heavenly Floor | $20,000 | Heavenly | 5x Precision Component, 3x Prototype Core |

### Upgrade Parts
Three new craftable/purchasable upgrade materials are added to the game:

- **Mixing Gear** — Basic upgrade component, available from the parts vendor
- **Precision Component** — Mid-tier upgrade component, available from the parts vendor
- **Prototype Core** — Rare upgrade component; **spawns in dead drops** across the map (3 per day, rotated daily). Ask the parts vendor for a hint on today's location.

### Parts Vendor (Nick)
A new NPC vendor named **Nick** spawns in the world. Interact with him to:
- **Buy upgrade parts** (Mixing Gear, Precision Component) from his shop
- **Get a hint** on where today's Prototype Core dead drop is located

### Upgrade Panel
A draggable **Upgrade Panel** opens when you interact with a Mixing Station Mk2. It shows:
- Current speed and quality upgrade levels
- The next available upgrade tier, cost, and material requirements
- Buttons to purchase upgrades directly

---

## Installation

**EITHER**

1. Download the latest release from https://github.com/DomMakarov1/Upgradeable-Mix-Station-Mod/releases
2. Drag and drop **MixingStationUpgrades.dll** into `SteamLibrary\steamapps\common\Schedule I\Mods`
3. Place the `UpgradeableMixStationMod` folder (containing item icons) into the `Mods` directory alongside the DLL

**OR**

1. Build the project (`.dll`) targeting `.NET 6`
2. Drop the compiled `.dll` into `SteamLibrary\steamapps\common\Schedule I\Mods`

---

## Save Data

Upgrade progress is saved to `{AppData}/LocalLow/.../UpgradeableMixStationMod.json`, keyed by station GUID. Note: upgrades will reset if a station is picked up and replaced.

---

Made by **Dom**
If you have any questions feel free to reach out to me on Discord, I'm always available!
