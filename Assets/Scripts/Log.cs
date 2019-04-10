using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static State;

namespace Log
{
    // Logs for UI to display a turn.
    // These logs will be transmitted over network. Should be kept lightweight

    [Serializable]
    public class TurnLog
    {
        public PhaseLog[][] phases;
    }

    // Log for a piece's phase
    [Serializable]
    public class PhaseLog
    {
        public Piece piece; // piece's health will be updated in the log
        public MoveLog moveLog;
        public AttackLog[] attackLogs;

        public PhaseLog(Piece piece, MoveLog moveLog, AttackLog[] attackLogs)
        {
            this.piece = piece;
            this.moveLog = moveLog;
            this.attackLogs = attackLogs;
        }
    }

    [Serializable]
    public class MoveLog
    {
        public Square combatSquare;      // where piece will be during combat
        public Square finalSquare;       // where piece will be at the end of combat

        public MoveLog(Square combatSquare, Square finalSquare)
        {
            this.combatSquare = combatSquare;
            this.finalSquare = finalSquare;
        }
    }

    [Serializable]
    public class AttackLog
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
