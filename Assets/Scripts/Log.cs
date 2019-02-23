using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State;

namespace Log
{
    // Logs for UI to display a turn.
    // These logs will be transmitted over network. Should be kept lightweight

    // Log for a piece's phase
    class PhaseLog
    {
        public Piece piece; // piece's health will be updated in the log
        public MoveLog moveLog;
        public List<AttackLog> attackLogs;

        public PhaseLog(Piece piece, MoveLog moveLog, List<AttackLog> attackLogs)
        {
            this.piece = piece;
            this.moveLog = moveLog;
            this.attackLogs = attackLogs;
        }
    }

    class MoveLog
    {
        public Square combatSquare;      // where piece will be during combat
        public Square finalSquare;       // where piece will be at the end of combat

        public MoveLog(Square combatSquare, Square finalSquare)
        {
            this.combatSquare = combatSquare;
            this.finalSquare = finalSquare;
        }
    }

    class AttackLog
    {
        public int damage;               // damage dealt
        public Move.Direction direction; // direction of attack, world direction
        // Other metadata for visuals can be added, for example: crits, dampening

        public AttackLog(int damage, Move.Direction direction)
        {
            this.damage = damage;
            this.direction = direction;
        }
    }
}
