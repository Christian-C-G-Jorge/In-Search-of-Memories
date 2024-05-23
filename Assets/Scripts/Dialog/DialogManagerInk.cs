using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.EventSystems;
using JetBrains.Annotations;
using System;

public class DialogManagerInk : MonoBehaviour
{
    [Header("Dialog UI")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI dialogText;

    private bool waitingForInput;
    private Story currentStory;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;

    private TextMeshProUGUI[] choicesText;

    public bool dialogIsPlaying { get; private set; }
    public static DialogManagerInk instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one dialog manager");
        }
        instance = this;
    }

    void Start()
    {
        dialogIsPlaying = false;
        dialogPanel.SetActive(false);

        //Initialise choicesText array
        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;

        //Get TextMeshPro components for each choice UI object
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    //Method to start displaying dialog from an Ink JSON file
    public void EnterDialogMode(TextAsset inkJSON)
    {
        //Create a new Story instance from the Ink JSON text
        currentStory = new Story(inkJSON.text);
        dialogIsPlaying = true;
        dialogPanel.SetActive(true);
        waitingForInput = true;

        //Bind external Ink functions to C# methods
        currentStory.BindExternalFunction("levelCheck", (int requiredLevel) =>
        {
            bool levelMet = true;
            if (PlayerStats.instance.currentLevel < requiredLevel)
            {
                levelMet = false;
            }

            return levelMet;
        });
        currentStory.BindExternalFunction("checkCanStart", (int requiredLevel, string questId) =>
        {
            bool canStart = true;
            if (PlayerStats.instance.currentLevel < requiredLevel)
            {
                canStart = false;
            }

            // Find the QuestPoint instance with the corresponding questId
            QuestPoint questPoint = FindObjectOfType<QuestPoint>();
            if (questPoint != null && questPoint.questId == questId)
            {
                if (questPoint.GetCurrentQuestState() != QuestState.CAN_START)
                {
                    canStart = false;
                }
            }
            else
            {
                Debug.LogError("QuestPoint with questId " + questId + " not found in the scene.");
            }

            return canStart;
        });
        currentStory.BindExternalFunction("completed", (string questId) =>
        {
            bool completed = false;

            // Find the QuestPoint instance with the corresponding questId
            QuestPoint questPoint = FindObjectOfType<QuestPoint>();
            if (questPoint != null && questPoint.questId == questId)
            {
                if (questPoint.GetCurrentQuestState() == QuestState.FINISHED)
                {
                    
                        completed = true;
                }
            }
         

            return completed;
        });
        currentStory.BindExternalFunction("startQuest", (string questId) =>
    {
        Debug.Log("Inkle output: " + questId);
        GameEventsManager.instance.questEvents.StartQuest(questId);
        
    });

        //Start Displaying the story
       //ContinueStory();
    }

    //Method to exit dialog mode
    private void ExitDialogMode()
    {
        dialogIsPlaying = false;
        dialogPanel.SetActive(false);
        dialogText.text = "";
    }

    void Update()
    {
        //Check if dialog is currently playing
        if (!dialogIsPlaying)
        {
            return;
        }
        //Check if waiting for player input and the 'E' key is pressed
        if (currentStory.currentChoices.Count == 0 && waitingForInput && Input.GetKeyDown(KeyCode.E))
        {
            ContinueStory();
        }
        //Exit dialog if not waiting for input
        else if (!waitingForInput && !currentStory.canContinue)
        {
            ExitDialogMode();
        }
    }

    private void ContinueStory()
    {
        //Check if the story can continue
        if (currentStory.canContinue)
        {
            //Display the next line of dialog
            dialogText.text = currentStory.Continue();
            waitingForInput = true;
            //Display choices if available
            DisplayChoices();
        }
        else
        {
            //No more content to display, stop waiting for input
            waitingForInput = false;
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        //Check if there are more choices than available UI objects
        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given than the UI can support");
        }

        int index = 0;

        //Initialises choices to the amount of choices for this line of dialog
        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }

        //Hide leftover null choices
        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        //Select the first choice automatically
        StartCoroutine(selectFirstChoice());
    }

    private IEnumerator selectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);

    }

    //Method to handle player choice selection
    public void MakeChoice(int choiceIndex)
    {
        //Choose the selected choice index and continue the story
        currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }

}
