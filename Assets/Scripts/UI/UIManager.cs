using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    //visualized game manager
    public GameManager gm;
    //field piece
    public GameObject fieldSlot;
    //field itself
    public Transform field;

    //Ui elements
    [SerializeField] private TMP_Dropdown modeSelector;
    [SerializeField] private TMP_Dropdown sizeSelector;
    [SerializeField] private TMP_Dropdown figureSelector;
    [SerializeField] private TMP_Text figureHint;
    [SerializeField] private TMP_Text firstTurnAnnouncement;
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private TMP_Text infoText;

    //keep selected game settings until we send them to GameManager
    GameMode selectedMode = GameMode.PLAYER_AI;
    int size = 3;
    bool playerFigure = false; // false - zero, true - cross

    //called on the start of the game
    void Start()
    {
        //registering event listeners
        //onValueChanged is called whenever one of the dropdown list's options is selected
        gm.onFieldChanged.AddListener(UpdateSlot);
        gm.onVictory.AddListener(ShowVictoryText);

        modeSelector.onValueChanged.AddListener(UpdateMode);
        sizeSelector.onValueChanged.AddListener((value) => size = 3 + value);
        figureSelector.onValueChanged.AddListener((figure) => playerFigure = figure > 0);
    }

    //event function, used as a mode selector
    //also corrects figure selection info depending on which mode is selected
    void UpdateMode(int selected)
    {
        selectedMode = (GameMode)selected;
        switch (selectedMode)
        {
            case GameMode.PLAYER_AI:
                figureHint.text = "Фигура игрока:";
                figureSelector.interactable = true;
                break;
            case GameMode.PLAYER_PLAYER:
                figureHint.text = "Фигура игрока 1:";
                figureSelector.interactable = true;
                break;
            case GameMode.AI_AI:
                playerFigure = Random.value < 0.5 ? true : false;
                figureSelector.interactable = false;
                break;
        }
    }

    //fills up game field with slots
    public void CreateGameField()
    {
        //checking every child object of our field
        //and clearing them for a new game
        foreach(Transform t in field)
        {
            Destroy(t.gameObject);
        }

        //setting field grid column count to selected size to ensure that we have a correct grid (3x3, 4x4, 5x5)
        field.GetComponent<GridLayoutGroup>().constraintCount = size;

        //creating new field pieces as child objects of field
        //and initializing them
        for (int i = 0; i < size * size; i++)
        {
            GameObject fieldPiece = Instantiate(fieldSlot, field);
            fieldPiece.GetComponent<UISlot>().Init(this, i);
        }
    }

    //updates game field UI
    //on specified position
    void UpdateSlot(int slotIDX)
    {
        field.GetChild(slotIDX).GetComponent<UISlot>().UpdateIcon(GameManager.GetFieldOnIndex(slotIDX, gm.gameField));

        //disable first turn text if it's still on
        if (firstTurnAnnouncement.gameObject.activeInHierarchy) firstTurnAnnouncement.gameObject.SetActive(false);

        UpdateInfoText();
    }

    //shows who's making a turn right now
    //if it's an AI, states that he's "thinking"
    void UpdateInfoText()
    {
        if(!infoText.gameObject.activeInHierarchy) infoText.gameObject.SetActive(true);
        if(gm.CurrentPlayer.aiControlled) infoText.text = gm.players[gm.whoseTurn].name + " думает...";
        else infoText.text = "Ход игрока " + (gm.players[gm.whoseTurn].name);
    }

    //shows who won
    void ShowVictoryText()
    {
        victoryText.gameObject.SetActive(true);
        if (gm.gameStage != 4)
            victoryText.text = "Побеждает " + (gm.gameStage == 3 ? gm.players[1].name : gm.players[0].name);
        else
            victoryText.text = "Ничья";

        infoText.gameObject.SetActive(false);
    }

    //shows who's making a turn first
    void ShowFirstTurnText()
    {
        firstTurnAnnouncement.gameObject.SetActive(true);
        firstTurnAnnouncement.text = "Первым ходит..." + gm.players[gm.whoseTurn].name;

        UpdateInfoText();
    }

    //called when "Begin Game" button is pressed
    //sends request to gamemanager, regenerates game field, some UI stuff too
    public void StartGame()
    {
        CreateGameField();
        gm.CreateGame(selectedMode, size, playerFigure);
        victoryText.gameObject.SetActive(false);
        ShowFirstTurnText();
    }
}
