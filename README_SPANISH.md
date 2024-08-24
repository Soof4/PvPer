# PvPer

- Autor: Soofa, Yuxue
- Source: [GitHub](https://github.com/Soof4/PvPer/)
- Este es un complemento del servidor TShock diseñado principalmente para:
- Implementar un modo de Duelos de jugador vs Jugador (PvP) el combate funciona utilizando una base de datos SQLite donde se guarda la data de los duelos.
- Monitoreo de eventos del juego a través de enlaces de eventos, impone restricciones y gestionar el comportamiento de los jugadores para garantizar el cumplimiento de las reglas de duelo.
- Además, el complemento admite la carga y recarga de archivos de configuración..
- Nota: Los jugadores que abandonen el área de combate designada serán asesinados inmediatamente y declarados perdedores.

(Traducido por [FrankV22](https://github.com/itsFrankV22))

## Commands

| Syntax                | Permission  |                                     Description                                      |
| --------------------- | :---------: | :----------------------------------------------------------------------------------: |
| /duel invite <player> | pvper.duel  |                            Invita a un jugador a un duelo                            |
| /duel acccept         | pvper.duel  |                             Acepta la invitacion al duelo                            |
| /duel decline         | pvper.duel  |                             Cancela la invitacion al duelo                           |
| /duel leaderboard     | pvper.duel  |                                 Ver Tabla de mejores                                 |
| /duel stats [player]  | pvper.duel  |                            Ver tu informacion de Rendimiento                         |
| /duel reset           | pvper.admin |                           Resetear Base de datos de Jugadores                        |
| /duel set <1/2/3/4>   | pvper.admin | Establecer posiciones de jugadores y Region de duelos: 1/2 Para lugar de jugadores, 3/4 para esquinas de la arena |

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
