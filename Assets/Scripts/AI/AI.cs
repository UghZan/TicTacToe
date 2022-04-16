using System.Linq;
using System.Collections.Generic;
using UnityEngine;

//used to keep move information in one spot for easier access
public struct Move
{
    public int idx;
    public int score;
    public Move(int _idx, int _score)
    {
        score = _score;
        idx = _idx;
    }
}

//AI class
//considering how it's implemented, it isn't that exciting to play against - it's perfectly boring :D
//if this ever turns into a real game, should introduce random chance to miss (as a difficulty picker) and maybe some randomness in 4x4+ fields,
//as right now it straight up places figures sequencially (could be a bug?)
public static class AI
{
    public static int bestMove = -1;
    public static int calls;
    static int startDepth;
    public static void Turn(GameManager gm)
    {
        calls = 0;
        bestMove = -1;
        //picking max depth to make AI faster
        switch (gm.size)
        {
            case 3:
                startDepth = 10;
                break;
            case 4:
                startDepth = 8;
                break;
            case 5:
                startDepth = 6;
                break;
        }

        //AB pruning
        Move move = GetTurnAB(gm, gm.whoseTurn, gm.gameField, startDepth, int.MinValue, int.MaxValue);
        //default minimax
        //Move move = GetTurn(gm, gm.whoseTurn, gm.gameField, 0);

        int newMove = move.idx;

        //debug info
        //Debug.Log(calls);

        //if no move was decided, pick it at random
        //also picks a random spot for the first AI turn to make it at least somewhat interesting
        if (newMove == -1 || gm.turns == 0)
        {
            Debug.Log("No move found, placing randomly");
            newMove = GameManager.GetRandomFreeIndexOnField(gm.gameField);
        }
        //making a calculated turn
        GameManager.UpdateFigureOnIndex(gm.CurrentPlayer.figure, newMove, gm.gameField);
        gm.HandleEndTurn();
        gm.onFieldChanged.Invoke(newMove);
        //Debug.Log(calls);
    }

    //minimax implementation
    //no depth limit, it's fine for 3x3 but for 4x4+ depth limit wouldn't really help: too slow
    public static Move GetTurn(GameManager gm, int playerIdx, byte[] field, int depth)
    {
        calls++;
        //if we are at leaf, check current field state
        //depth is added/subtracted to ensure that AI always tries to play till the end
        //e.g. if turn A is to ensure in loss in 2 turns, but turn B is to ensure loss in 3 turns, AI will go for a 3-turn loss, because end score for loss will be -10 + depth = -7, so it's more
        //attractive for it
        if (GameManager.CheckForVictory(field, gm.CurrentPlayer.figure))
            return new Move(-1, 10 + depth);
        else if (GameManager.CheckForVictory(field, gm.OtherPlayer.figure))
            return new Move(-1, -10 - depth);
        else if (GameManager.CheckFieldIsFull(field)) return new Move(-1, 0);

        List<Move> moves = new List<Move>();

        //checking every empty field as a potential turn
        int[] availPlaces = GameManager.GetFreeIndicesOnField(field);
        for (int i = 0; i < availPlaces.Length; i++)
        {
            //testing a turn
            byte[] newField = GameManager.UpdateFigureOnIndex(gm.players[playerIdx].figure, availPlaces[i], field);
            Move newMove = new Move(availPlaces[i], GetTurn(gm, (playerIdx + 1) % 2, newField, depth - 1).score);
            //remembering turn and it's potential score
            moves.Add(newMove);
            //reverting a turn, because C# arrays are reference types so the original will be changed too
            GameManager.UpdateFigureOnIndex(0, availPlaces[i], field);
        }

        //if we are maximizing (AI player)
        Move bestMove = new Move(-1,-1);
        int bestScore;
        if (playerIdx == gm.whoseTurn)
        {
            bestScore = int.MinValue; //looking for the highest score move
            for (var i = 0; i < moves.Count; i++)
            {
                if (moves[i].score > bestScore)
                {
                    bestScore = moves[i].score;
                    bestMove = moves[i];
                }
            }
        }
        else //if we are minimizing (opposite player)
        {
            bestScore = int.MaxValue; //looking for the lowest score move
            for (var i = 0; i < moves.Count; i++)
            {
                if (moves[i].score < bestScore)
                {
                    bestScore = moves[i].score;
                    bestMove = moves[i];
                }
            }
        }

        return bestMove;
    }

    //minimax implementation with alpha-beta pruning
    //limits depth to a certain value, otherwise 4x4 and more becomes too slow
    public static Move GetTurnAB(GameManager gm, int playerIdx, byte[] field, int depth, int alpha, int beta)
    {
        calls++;

        //same as non-AB minimax, but now it has a depth limit
        if (GameManager.CheckForVictory(field, gm.CurrentPlayer.figure))
            return new Move(-1, 10 + depth);
        else if (GameManager.CheckForVictory(field, gm.OtherPlayer.figure))
            return new Move(-1, -10 - depth);
        else if (GameManager.CheckFieldIsFull(field)) return new Move(-1, 0);
        else if (depth < 0) return new Move(-1, -1);

        int[] availPlaces = GameManager.GetFreeIndicesOnField(field);
        int move = -1, bestScore = 0;

        for (int i = 0; i < availPlaces.Length; i++)
        {
            //checking a turn
            byte[] newField = GameManager.UpdateFigureOnIndex(gm.players[playerIdx].figure, availPlaces[i], field);
            Move newMove = GetTurnAB(gm, (playerIdx + 1) % 2, newField, depth - 1, alpha, beta);
            GameManager.UpdateFigureOnIndex(0, availPlaces[i], field);

            //if we're at depth limit, stop
            if (newMove.score == -1) continue;
            //depending on player...
            if (playerIdx == gm.whoseTurn)
            {
                //if new max limit of maximizing player is found
                if (newMove.score > alpha)
                {
                    //remember it for neighboring child
                    //and remember this move
                    alpha = bestScore = newMove.score;
                    move = availPlaces[i];
                    //if new max limit is lower than min player limit, then stop
                    //because there is no sense in checking it further - it's a guaranteed loss
                    if (beta <= alpha) break;
                }
            }
            else
            {
                //if new min limit of minimizing player is found
                if (newMove.score < beta)
                {
                    beta = bestScore = newMove.score;
                    move = availPlaces[i];
                    if (beta <= alpha) break;
                }
            }
        }
        return new Move(move, bestScore);

    }
}
