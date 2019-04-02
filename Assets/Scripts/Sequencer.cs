using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Log;
using State;

public class Sequencer : MonoBehaviour
{
    public UITileManager utm;

    void OnEnable()
    {
        NetworkEventManager.OnReceiveTurnLog += OnReceiveTurnLog;
    }

    void OnDisable()
    {
        NetworkEventManager.OnReceiveTurnLog -= OnReceiveTurnLog;
    }

    IEnumerator RunTurn(TurnLog turnLog)
    {
        foreach (PhaseLog[] pla in turnLog.phases)
        {
            Debug.Log("PhaseCombat");
            foreach (PhaseLog pl in pla)
            {
                Square combatSquare = pl.moveLog.combatSquare;
                utm.MoveFromTo(pl.piece.uid, combatSquare.col, combatSquare.row);
            }
            utm.PhasePostProcess();
            yield return new WaitForSeconds(3);
            Debug.Log("PhaseFinal");
            foreach (PhaseLog pl in pla)
            {
                Square finalSquare = pl.moveLog.finalSquare;
                utm.MoveFromTo(pl.piece.uid, finalSquare.col, finalSquare.row);
            }
            utm.PhasePostProcess();
            yield return new WaitForSeconds(3);
        }
    }

    void OnReceiveTurnLog(TurnLog turnLog)
    {
        StartCoroutine(RunTurn(turnLog));
    }
}
