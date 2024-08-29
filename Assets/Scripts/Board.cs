﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour {

    public GameObject gem;
    public Sprite[] gemsSprites;
    private int[,] indexGrid;
    private int comboCount = 1;
    private bool reset = true;
    private bool combo = false;
    private Vector3 initialPos;
    private GameObject[,] gems;
    private List<Dictionary<string, int>> matchCoordinates;

    private const int GRID_WIDTH = 5;
    private const int GRID_HEIGHT = 7;
    private const float TILE_SIZE = 8;
    private const float WAIT_TIME_MOVE = 0.5f;

    void Awake() => Setup();
    void Start() 
    {
        UnityEngine.Debug.Log("Start board with shuffle false");
        SetupBoard(false);
    }

    void Setup() {
        //Initial conditions
        initialPos = gameObject.transform.position;
        indexGrid = new int[GRID_WIDTH, GRID_HEIGHT];
        gems = new GameObject[GRID_WIDTH, GRID_HEIGHT];
        matchCoordinates = new List<Dictionary<string, int>>();
    }

    /* 
    *   Creates a 2D randomize grid with possible moves and no matches
    *   It is called in the beginning of the game and when there are no possible move
    */
    void SetupBoard(bool shuffle) {
        while (reset) {
            UnityEngine.Debug.Log("Resetting board");
            for (int x = 0; x < indexGrid.GetLength(0); x++) {
                for (int y = 0; y < indexGrid.GetLength(1); y++) {
                    indexGrid[x, y] = Random.Range(0, gemsSprites.GetLength(0));
                }
            }
            UnityEngine.Debug.Log("Index grid: " + indexGrid);
            if (!CheckMatches(false) && CheckIfThereArePossibleMoves()) {
                reset = false;
                shuffle = false;
            }
        }
        UnityEngine.Debug.Log("Setting up board");
        for (int x = 0; x < indexGrid.GetLength(0); x++) {
            for (int y = 0; y < indexGrid.GetLength(1); y++) {
                UnityEngine.Debug.Log("Shuffle: " + shuffle);
                if (!shuffle) {
                    UnityEngine.Debug.Log("x: " + x + " y: " + y + " index: " + indexGrid[x, y]);
                    GameObject tempGemObj = Gem.Start(gem, initialPos, x, y, TILE_SIZE, gameObject.transform);
                    gems[x, y] = tempGemObj;
                }
                var currentGem = gems[x, y];
                var spriteRenderer = currentGem.GetComponent<SpriteRenderer>();
                var gemSprite = gemsSprites[indexGrid[x, y]];
                spriteRenderer.sprite = gemSprite;
                string type = gems[x, y].GetComponent<SpriteRenderer>().sprite.name.Split("characters_000").Last();
                gems[x, y].gameObject.name = "Gem";
                gems[x, y].GetComponent<Gem>().type = type;
                gems[x, y].GetComponent<Gem>().index = new int[] { x, y };
                Gem tempGem = gems[x, y].GetComponent<Gem>();
                tempGem.SetCoordinates(new Dictionary<string, int>() { { "x", x }, { "y", y } });
            }
        }
    }

    /* 
    *  Check for any horizontal or vertical match
    *  If it is a player move, it saves the match coordinates so that a future trigger can be applied     
    */
    bool CheckMatches(bool isPlayerMove) {
        bool ifMatch = false;
        //Horizontal Check
        for (int x = 0; x < indexGrid.GetLength(0) - 2; x++) {
            for (int y = 0; y < indexGrid.GetLength(1); y++) {
                int current = indexGrid[x, y];
                if (current == indexGrid[x + 1, y] && current == indexGrid[x + 2, y]) {
                    if (isPlayerMove) {
                        SaveMatchCoordinates(x, y, true);
                    }
                    ifMatch = true;
                }
            }
        }
        //Vertical check
        for (int x = 0; x < indexGrid.GetLength(0); x++) {
            for (int y = 0; y < indexGrid.GetLength(1) - 2; y++) {
                int current = indexGrid[x, y];
                if (current == indexGrid[x, y + 1] && current == indexGrid[x, y + 2]) {
                    if (isPlayerMove) {
                        SaveMatchCoordinates(x, y, false);
                    }
                    ifMatch = true;
                }
            }
        }
        return ifMatch;
    }

    /*
    *  After the move has been checked possible, this function swaps the grid sprites
    */
    public void MakeSpriteSwitch(int x_1, int y_1, int x_2, int y_2) {
        Sprite spriteTemp_1 = gems[x_1, y_1].GetComponent<SpriteRenderer>().sprite;
        Sprite spriteTemp_2 = gems[x_2, y_2].GetComponent<SpriteRenderer>().sprite;
        gems[x_1, y_1].GetComponent<SpriteRenderer>().sprite = spriteTemp_2;
        gems[x_2, y_2].GetComponent<SpriteRenderer>().sprite = spriteTemp_1;
        gems[x_1, y_1].GetComponent<Gem>().ResetSelection();
        StartCoroutine(TriggerMatch());
    }

    /* 
    *   Check if the move generates a match
    *   If it is a player move, it keeps the grid index change
    */
    public bool CheckIfSwitchIsPossible(int x_1, int y_1, int x_2, int y_2, bool isPlayerMove) {
        int temp_1 = indexGrid[x_1, y_1];
        int temp_2 = indexGrid[x_2, y_2];
        indexGrid[x_1, y_1] = temp_2;
        indexGrid[x_2, y_2] = temp_1;
        if (CheckMatches(isPlayerMove)) {
            if (isPlayerMove) {
                return true;
            } else {
                indexGrid[x_1, y_1] = temp_1;
                indexGrid[x_2, y_2] = temp_2;
                return true;
            }
        } else {
            indexGrid[x_1, y_1] = temp_1;
            indexGrid[x_2, y_2] = temp_2;
        }
        return false;
    }

    /* 
    *   Save all match coordinates in a dictionary
    */
    void SaveMatchCoordinates(int x, int y, bool isHorizontal) {
        matchCoordinates.Add(new Dictionary<string, int>() { { "x", x }, { "y", y } });
        if (isHorizontal) {
            for (int i = 1; i < (GRID_WIDTH - x); i++) {
                if (indexGrid[x, y] == indexGrid[x + i, y]) {
                    matchCoordinates.Add(new Dictionary<string, int>() { { "x", x + i }, { "y", y } });
                } else {
                    break;
                }
            }
        } else {
            for (int i = 1; i < (GRID_HEIGHT - y); i++) {
                if (indexGrid[x, y] == indexGrid[x, y + i]) {
                    matchCoordinates.Add(new Dictionary<string, int>() { { "x", x }, { "y", y + i } });
                } else {
                    break;
                }
            }
        }
    }

    /* 
    *   Coroutine that applies the match by removing sequences and calling a method to move gems downwards
    *   If there are still matches after the rearrangement, the coroutine is called again
    *   If there are no more matches, the functions calls for possible moves
    */
    IEnumerator TriggerMatch() {
        foreach (Dictionary<string, int> unit in matchCoordinates) {
            gems[unit["x"], unit["y"]].GetComponent<SpriteRenderer>().sprite = null;
        }
        GameManager.instance.AddScore(matchCoordinates.Count, comboCount);
        matchCoordinates.Clear();
        yield return new WaitForSeconds(WAIT_TIME_MOVE);
        AudioManager.instance.Play("Clear");
        MoveGemsDownwards();
        yield return new WaitForSeconds(WAIT_TIME_MOVE);
        combo = CheckMatches(true);
        if (combo) {
            comboCount += 1;
            //Recalls the method due to combo
            StartCoroutine(TriggerMatch());
        } else {
            CheckIfThereArePossibleMoves();
            GameManager.instance.SetIsSwitching(false);
            combo = false;
            comboCount = 1;
        }
    }


    /* 
    *   Fills empty tiles from top to bottom
    *   New gems are random 
    */
    void MoveGemsDownwards() {
        bool hasNull = true;
        while (hasNull) {
            hasNull = false;
            for (int y = 0; y < GRID_HEIGHT; y++) {
                for (int x = 0; x < GRID_WIDTH; x++) {
                    SpriteRenderer currentGemSprite = gems[x, y].GetComponent<SpriteRenderer>();
                    if (currentGemSprite.sprite == null) {
                        hasNull = true;
                        if (y != GRID_HEIGHT - 1) {
                            SpriteRenderer aboveGemSprite = gems[x, y + 1].GetComponent<SpriteRenderer>();
                            indexGrid[x, y] = indexGrid[x, y + 1];
                            indexGrid[x, y + 1] = 7;
                            currentGemSprite.sprite = aboveGemSprite.sprite;
                            aboveGemSprite.sprite = null;

                        } else {
                            indexGrid[x, y] = Random.Range(0, gemsSprites.GetLength(0));
                            currentGemSprite.sprite = gemsSprites[indexGrid[x, y]];
                        }
                    }
                }
            }
        }
    }

    /* 
    *   Check for all possible moves in the grid
    *   Enforce game rules
    *   Applies a try & catch to ignore IndexOutOfRangeException error            
    */
    bool CheckIfThereArePossibleMoves() {
        UnityEngine.Debug.Log("Checking for possible moves");
        bool ifPossibleMove = false;
        for (int x = 0; x < GRID_WIDTH; x++) {
            for (int y = 0; y < GRID_HEIGHT; y++) {
                try {
                    ifPossibleMove = CheckIfSwitchIsPossible(x, y, x, y + 1, false); //UP
                    if (ifPossibleMove) {
                        Debug.Log("Hint  x: " + x + " y: " + y);
                        break;
                    }
                } catch (System.IndexOutOfRangeException) { }
                try {
                    ifPossibleMove = CheckIfSwitchIsPossible(x, y, x, y - 1, false); //DOWN
                    if (ifPossibleMove) {
                        Debug.Log("Hint  x: " + x + " y: " + y);
                        break;
                    }
                } catch (System.IndexOutOfRangeException) { }
                try {
                    ifPossibleMove = CheckIfSwitchIsPossible(x, y, x + 1, y, false); //RIGHT
                    if (ifPossibleMove) {
                        Debug.Log("Hint  x: " + x + " y: " + y);
                        break;
                    }
                } catch (System.IndexOutOfRangeException) { }
                try {
                    ifPossibleMove = CheckIfSwitchIsPossible(x, y, x - 1, y, false); //LEFT
                    if (ifPossibleMove) {
                        Debug.Log("Hint  x: " + x + " y: " + y);
                        break;
                    }
                } catch (System.IndexOutOfRangeException) { }
            }
            if (ifPossibleMove) {
                break;
            }
        }
        if (!ifPossibleMove) {
            UnityEngine.Debug.Log("Need to shuffle");
            reset = true;
            SetupBoard(true);
            StartCoroutine(UI.instance.PopUpFadeAway(GameManager.instance.shuffleUI, 3.0f)); //Shuffle Pop-Up
        }
        return ifPossibleMove;
    }

    public void DeactivateBoard() {
        gameObject.SetActive(false);
    }
}
