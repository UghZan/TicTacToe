using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GameMode
{
    PLAYER_AI,
    PLAYER_PLAYER,
    AI_AI
}

public class GameManager : MonoBehaviour
{

    //called whenever the field has changed
    //used to inform UI that we need to redraw
    public UnityEvent<int> onFieldChanged = new UnityEvent<int>();
    public UnityEvent onVictory = new UnityEvent();

    public Sprite CROSS_ICON;
    public Sprite ZERO_ICON;

    public Player CurrentPlayer { get { return players[whoseTurn]; } }
    public Player OtherPlayer { get { return players[(whoseTurn + 1) % 2]; } }

    //game settings, in case we need them
    GameMode gameMode;
    public int size;

    float AI_timer; //to emulate AI think time and to ensure everything is loaded properly if AI makes a turn instantaneously

    public Player[] players;
    public byte[] gameField;
    public int gameStage; //0 - pregame, 1 - game in progress, 2 - player 1 victory, 3 - player 2 victory, 4 - draw
    public int whoseTurn;
    public int turns = 0;

    //checks if a field is filled
    public static bool CheckFieldIsFull(byte[] field)
    {
        for(int i = 0; i < field.Length; i++)
        {
            if (field[i] == 0) return false;
        }
        return true;
    }

    private void Update()
    {
        //if the game is in progress and it's AI's turn, wait until timer is zero and then make a turn
        if(gameStage == 1 && CurrentPlayer.aiControlled)
            if(AI_timer > 0)
            {
                AI_timer -= Time.deltaTime;
            }
            else
            {
                AI.Turn(this);
            }
    }

    //creates a new game, resets every parameter of the game, starts up AI if needed
    public void CreateGame(GameMode _mode, int _size, bool firstFig)
    {
        gameMode = _mode;
        size = _size;

        players = new Player[2];
        switch(_mode)
        {
            case GameMode.PLAYER_AI:
                players[0] = new Player("Èãðîê", false, firstFig);
                players[1] = new Player("ÈÈ", true, !firstFig);
                break;
            case GameMode.PLAYER_PLAYER:
                players[0] = new Player("Èãðîê 1", false, firstFig);
                players[1] = new Player("Èãðîê 2", false, !firstFig);
                break;
            case GameMode.AI_AI:
                players[0] = new Player("ÈÈ 1", true, firstFig);
                players[1] = new Player("ÈÈ 2", true, !firstFig);
                break;
        }

        gameField = new byte[size * size];
        gameStage = 1;
        turns = 0;
        whoseTurn = players[0].figure == 2 ? 0 : 1;

        if (CurrentPlayer.aiControlled)
        {
            AI_timer = Random.Range(0.1f, 0.2f);
        }
    }

    //checks if current field configuration is a winning one
    public static bool CheckForVictory(byte[] field, byte fig)
    {
        int maxZeroLine = 0, maxCrossLine = 0, val = 0;
        int size = (int)Mathf.Sqrt(field.Length);

        //check horizontals
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                val = field[i * size + j];
                if (val == 1)
                {
                    maxZeroLine++;
                    maxCrossLine = 0;
                }
                else if (val == 2)
                {
                    maxCrossLine++;
                    maxZeroLine = 0;
                }
                else maxCrossLine = maxZeroLine = 0;
            }
            if (maxZeroLine == size) return fig == 1;
            else if (maxCrossLine == size) return fig == 2;
            maxZeroLine = maxCrossLine = 0;
        }

        //check verticals
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                val = field[j * size + i];
                if (val == 1)
                {
                    maxZeroLine++;
                    maxCrossLine = 0;
                }
                else if (val == 2)
                {
                    maxCrossLine++;
                    maxZeroLine = 0;
                }
                else maxCrossLine = maxZeroLine = 0;
            }
            if (maxZeroLine == size) return fig == 1;
            else if (maxCrossLine == size) return fig == 2;
            maxZeroLine = maxCrossLine = 0;
        }

        //check diagonal left-up to down-right
        for (int i = 0; i < size; i++)
        {
            val = field[i * size + i];
            if (val == 1)
            {
                maxZeroLine++;
                maxCrossLine = 0;
            }
            else if (val == 2)
            {
                maxCrossLine++;
                maxZeroLine = 0;
            }
            else maxCrossLine = maxZeroLine = 0;
        }
        if (maxZeroLine == size) return fig == 1;
        else if (maxCrossLine == size) return fig == 2;
        maxZeroLine = maxCrossLine = 0;

        //check diagonal down-left to up-right
        for (int i = 0; i < size; i++)
        {
            val = field[(size - 1) * (i+1)];
            if (val == 1)
            {
                maxZeroLine++;
                maxCrossLine = 0;
            }
            else if (val == 2)
            {
                maxCrossLine++;
                maxZeroLine = 0;
            }
            else maxCrossLine = maxZeroLine = 0;
        }
        if (maxZeroLine == size) return fig == 1;
        else if (maxCrossLine == size) return fig == 2;

        return false;
    }

    //handles changing turns
    void NextTurn()
    {
        whoseTurn = (whoseTurn + 1)%2;
        if (CurrentPlayer.aiControlled)
        {
            AI_timer = Random.Range(0.1f, 0.2f);
        }
        turns++;
    }

    //handles end of the turn, checks if anyone wins, fires onVictory event for UI
    public void HandleEndTurn()
    {
        bool victoryState = CheckForVictory(gameField, CurrentPlayer.figure);

        //switch turn
        if (!victoryState)
        {
            if (CheckFieldIsFull(gameField))
            {
                gameStage = 4;
                onVictory.Invoke();
            }
            NextTurn();
        }
        else
        {
            gameStage = 2 + whoseTurn;
            onVictory.Invoke();
        }
    }

    //finds all empty places on field
    public static int[] GetFreeIndicesOnField(byte[] field)
    {
        List<int> freeIndices = new List<int>();
        for(int i = 0; i < field.Length; i++)
        {
            if (field[i] == 0) freeIndices.Add(i);
        }
        return freeIndices.ToArray();
    }

    //finds a random empty place on field
    public static int GetRandomFreeIndexOnField(byte[] field)
    {
        int[] free = GetFreeIndicesOnField(field);
        return free[Random.Range(0,free.Length)];
    }

    //gets field's value
    public static byte GetFieldOnIndex(int idx, byte[] field)
    {
        return field[idx];
    }

    //gives a string representation of a field
    //used in debug purposes
    public static string FieldString(byte[] field)
    {
        string f = "";
        for (int i = 0; i < field.Length; i++)
        {
            f += field[i] + " ";
        }
        return f;
    }

    //handles player input
    public void PlayerTurn(int idx)
    {
        //if game hasn't started yet/already finished, then return
        if (gameStage != 1) return;
        //if it's AI's turn, return
        if (CurrentPlayer.aiControlled) return;
        //if the spot is occupied, return
        if (GetFieldOnIndex(idx, gameField) != 0) return;

        gameField = UpdateFigureOnIndex(CurrentPlayer.figure, idx, gameField);

        HandleEndTurn();
        onFieldChanged.Invoke(idx);
    }

    //changes value on specified position
    public static byte[] UpdateFigureOnIndex(byte f, int idx, byte[] field)
    {
        field[idx] = f;
        return field;
    }
}
