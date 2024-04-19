using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;

public class QuestManager : MonoBehaviour
{
    private Dictionary<string, Quest> questMap;
    public PlayerStats playerStats;

    [Header("Config")]
    [SerializeField] private bool loadQuestState = true; // Change this to true if you want to use PlayerPrefs or false to not
    private int currentPlayerLevel;

    private void Awake()
    {
        questMap = CreateQuestMap();
        ////Debuging
        //Quest quest = GetQuestById("CollectCoinsQuest");
        //Debug.Log(quest.info.displayName);
        //Debug.Log(quest.info.levelRequirement);
        //Debug.Log(quest.state);
        //Debug.Log(quest.currentStepExists());
    }

    
    private void OnEnable()
    {
        GameEventsManager.instance.questEvents.onStartQuest += StartQuest;
        GameEventsManager.instance.questEvents.onAdvanceQuest += AdvanceQuest;
        GameEventsManager.instance.questEvents.onFinishQuest += FinishQuest;
        GameEventsManager.instance.questEvents.onQuestStepStateChange += QuestStepStateChange;

        GameEventsManager.instance.playerEvents.onPlayerLevelChange += PlayerLevelChange;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.questEvents.onStartQuest -= StartQuest;
        GameEventsManager.instance.questEvents.onAdvanceQuest -= AdvanceQuest;
        GameEventsManager.instance.questEvents.onFinishQuest -= FinishQuest;
        GameEventsManager.instance.questEvents.onQuestStepStateChange -= QuestStepStateChange;

        GameEventsManager.instance.playerEvents.onPlayerLevelChange -= PlayerLevelChange;
    }

    
    private void Start()
    {
        foreach(Quest quest in questMap.Values)
        {
            //Loads QuestData on startup
            if (quest.state == QuestState.IN_PROGRESS)
            {
                quest.InstantiateCurrentQuestStep(this.transform);
            }
            GameEventsManager.instance.questEvents.QuestStateChange(quest);
        }
    }

    private void ChangeQuestState(string id, QuestState state)
    {
        Quest quest = GetQuestById(id);
        quest.state = state;
        GameEventsManager.instance.questEvents.QuestStateChange(quest);
    }

    private void PlayerLevelChange(int level)
    {
        currentPlayerLevel = level;
    }

    private bool CheckRequirementsMet(Quest quest)
    {
        bool meetsRequirements = true;

        if(currentPlayerLevel < quest.info.levelRequirement) 
        {
            meetsRequirements = false;  
        }

        foreach(QuestInfoSO prerequisiteQuestInfo in quest.info.questPrerequisites)
        {
            if(GetQuestById(prerequisiteQuestInfo.id).state != QuestState.FINISHED)
            {
                meetsRequirements = false;
            }
        }

        return meetsRequirements;
    }

    private void Update()
    {
        foreach (Quest quest in questMap.Values)
        {
            if(quest.state == QuestState.REQUIREMENTS_NOT_MET && CheckRequirementsMet(quest))
            {
                ChangeQuestState(quest.info.id, QuestState.CAN_START);
            }
        }
    }
    private void StartQuest(string id)
    {
        Debug.Log("Start Quest: " + id);
        Quest quest = GetQuestById(id);
        quest.InstantiateCurrentQuestStep(this.transform);
        ChangeQuestState(quest.info.id, QuestState.IN_PROGRESS);
    }

    private void AdvanceQuest(string id)
    {
        Debug.Log("Advance Quest: " + id);
        Quest quest = GetQuestById(id);
        quest.MoveToNextStep();
        if(quest.CurrentStepExists())
        {
            quest.InstantiateCurrentQuestStep(this.transform);

        }
        else
        {
            ChangeQuestState(quest.info.id, QuestState.CAN_FINISH);
        }
        
    }
    private void FinishQuest(string id)
    {
        Debug.Log("Finish Quest " + id);
        Quest quest = GetQuestById(id);
        ClaimRewards(quest);
        ChangeQuestState(quest.info.id, QuestState.FINISHED);
        
    }

    private void ClaimRewards(Quest quest)
    {
        GameEventsManager.instance.goldEvents.GoldGained(quest.info.goldReward);
        GameEventsManager.instance.playerEvents.ExperienceGained(quest.info.experienceReward);
    }

    private void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState)
    {
        Quest quest = GetQuestById(id);
        quest.StoreQuestStepState(questStepState, stepIndex);
        ChangeQuestState(id,quest.state);
    }

    private Dictionary<string, Quest> CreateQuestMap()
    {
        //Loads all QuestInfoSO Scriptable objects under the assets/resources/quests folder
        QuestInfoSO[] allQuest = Resources.LoadAll <QuestInfoSO> ("Quest");

        Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();
        foreach(QuestInfoSO questInfo in allQuest)
        {
            if (idToQuestMap.ContainsKey(questInfo.id))
            {
                Debug.LogWarning("Duplicate ID found when creating quest map: " + questInfo.id);
            }
            idToQuestMap.Add(questInfo.id, LoadQuest(questInfo));
        }
        return idToQuestMap;
    }

    private Quest GetQuestById(string id)
    {
        Quest quest = questMap[id];
        if (quest == null)
        {
            Debug.LogError("ID not found in the Quest map:" + id);
        }   
        return quest;
    }

    private void OnApplicationQuit()
    {
        foreach(Quest quest in questMap.Values)
        {
            QuestData questData = quest.GetQuestData();
            Debug.Log(quest.info.id);
            Debug.Log("State = " + questData.state);
            Debug.Log("index = " + questData.questStepIndex);
            foreach(QuestStepState stepState in questData.questStepStates) 
            {
                Debug.Log("Step state = " + stepState.state);
            }
            SaveQuest(quest);
        }
    }

    private void SaveQuest(Quest quest)
    {
        try
        {
            QuestData questData = quest.GetQuestData();
            string serializedData = JsonUtility.ToJson(questData);
            PlayerPrefs.SetString(quest.info.id, serializedData);
            Debug.Log(serializedData);
        }
        catch(System.Exception e)
        {
            Debug.LogError("Failed to save quest with Id " + quest.info.id + ": " + e);
        }

        
    }

    private Quest LoadQuest(QuestInfoSO questInfo)
    {
        Quest quest = null;
        try
        {
            // load quest from saved data
            if (PlayerPrefs.HasKey(questInfo.id) && loadQuestState)
            {
                string serializedData = PlayerPrefs.GetString(questInfo.id);
                QuestData questData = JsonUtility.FromJson<QuestData>(serializedData);
                quest = new Quest(questInfo, questData.state, questData.questStepIndex, questData.questStepStates);
            }
            // otherwise, initialize a new quest
            else
            {
                quest = new Quest(questInfo);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load quest with id " + quest.info.id + ": " + e);
        }
        return quest;
    }
}
