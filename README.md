# PvPer

- Author: Soofa, Yuxue
- Source: [GitHub](https://github.com/Soof4/PvPer/)
- This is a TShock server plugin primarily designed for:
- Implementing player-versus-player (PvP) combat functionality, utilizing an SQLite database to store duel data.
- Monitoring game events through event hooks, imposing restrictions and managing player behavior to ensure adherence to dueling rules.
- Additionally, the plugin supports loading and reloading of configuration files.
- Note: Players who leave the designated combat area will be immediately killed and declared as losers.

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
  "PermName": "pvper.duel / pvper.admin",
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
