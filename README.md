# PvPer

- Author: Soofa, Yuxue
- Source: [GitHub](https://github.com/Soof4/PvPer/)
- This is a TShock server plugin primarily designed for:
- Implementing player-versus-player (PvP) combat functionality, utilizing an SQLite database to store duel data.
- Monitoring game events through event hooks, imposing restrictions and managing player behavior to ensure adherence to dueling rules.
- Additionally, the plugin supports loading and reloading of configuration files.
- Note: Players who leave the designated combat area will be immediately killed and declared as losers.

## Logs

```
1.1.1
- In the configuration file, added options for "Pulling Range" and whether to "Pull Player Back to Arena."
- When the Pulling Range is set to 0, teleport the player who has exited the arena to the exact center point of the arena.
- If the Pulling Range is negative, pull the player to a position on the center point aligned with the direction they exited the arena.
-If the Pulling Range is positive, pull the player to a position on the center point that is opposite to the direction they exited the arena.

1.1.0
- Added the [Duel Reset] command to `/duel help`.
- Incorporated death taunts and consecutive win announcements.
- Added a "leave arena bleeding" option to the configuration file (enabled by default when death penalty is disabled).

1.0.3
- Added a subcommand for quickly setting player and arena boundaries (automatically writes to and saves the Config).
- Added a toggle for "leaving the arena results in player death" in the configuration file, along with Chinese localization.
- Fixed the implementation for leaving the arena triggering a death determination.

1.0.2
- Yuxue fully translated and revised all commands and their feedback messages.
- Included a new command permission for resetting player data tables.
```

## Commands

| Syntax                | Permission  |                                     Description                                      |
| --------------------- | :---------: | :----------------------------------------------------------------------------------: |
| /duel invite <player> | pvper.duel  |                              Invite a player to a duel                               |
| /duel acccept         | pvper.duel  |                               Accept a duel invitation                               |
| /duel decline         | pvper.duel  |                              Decline a duel invitation                               |
| /duel leaderboard     | pvper.duel  |                                    View rankings                                     |
| /duel stats [player]  | pvper.duel  |                             Check individual performance                             |
| /duel reset           | pvper.admin |                                Reset player database                                 |
| /duel set <1/2/3/4>   | pvper.admin | Sync current position to coordinates: 1/2 for player locations, 3/4 for arena bounds |

## Config

```json
{
  "README": "/pvp set 3 4 Set the coordinates of the player's teleportation higher or lower than the player's coordinates by 3 blocks",
  "README2": "`README2` = \"Pull Range: Will retract players from their direction of exiting the arena back to a specified opposite position relative to the arena center (a positive value indicates a position in the same direction). This feature is enabled by default when the option to kill players is disabled.\"",
  "PermName": "pvper.use / pvper.admin",
  "PullArena": true,
  "PullRange": -20,
  "PlayerKill": false,
  "PlayerSlap": 20,
  "Player1PositionX": 0,
  "Player1PositionY": 0,
  "Player2PositionX": 0,
  "Player2PositionY": 0,
  "ArenaPosX1": 0,
  "ArenaPosY1": 0,
  "ArenaPosX2": 0,
  "ArenaPosY2": 0
}
```
