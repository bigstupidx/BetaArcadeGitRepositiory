﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// TARGET: any empty game object
/// Script pauses the game but not the camera. Use specific camera scripts for this task
/// </summary>

public class pauseGame : MonoBehaviour {

    public Player playerScript;
    private GameObject pauseMenu;

    public UnityEngine.UI.Button initialBtn;

    public bool isPaused = false;

    //Initial menu transform
    // A reason for this peculiar way of disabling a menu is a unity bug
    // After enabling a button it is selected but not highlighted
    private Vector3 pauseMenuInitialPos;
    private Vector3 pauseMenuUnseenPos;

    private void Start()
    {
        pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu");
        pauseMenuInitialPos = pauseMenu.transform.position;
        pauseMenuUnseenPos.y = pauseMenuInitialPos.y + 400;
    }

    // Update is called once per frame
    void Update ()
    {
        //Get input for a pause
        if (Input.GetButtonDown("Pause"))
        {
            isPaused = !isPaused;
            Debug.Log("Game paused " + isPaused);
        }

        //Handle a pause
        PauseGame();
    }

    private void PauseGame()
    {
        //Show menu
        showMenu(isPaused);

        //Change time
        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
            Time.timeScale = 1f;

        //Disable player's input
        playerScript.SetIgnoreInput(isPaused);
    }

    /// <summary>
    /// A reason for this peculiar way of disabling a menu is a unity bug
    /// After enabling a button it is selected but not highlighted
    /// </summary>
    /// <param name="toShow"></param>
    public void showMenu(bool toShow)
    {
        if (toShow)
        {
            pauseMenu.transform.position = pauseMenuInitialPos;
        }
        else
        {
            pauseMenu.transform.position = pauseMenuUnseenPos;
        }

    }

    //------------BUTTON CLICKS---------------
    public void onResumeClick()
    {
        isPaused = false;
    }

    public void onMenuClick()
    {
        isPaused = false;
        PauseGame();
        EditorSceneManager.LoadScene(0);
    }



}
