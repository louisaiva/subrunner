using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MessageBank : MonoBehaviour
{
    public static MessageBank Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    [Header("UI Message generation")]
    [SerializeField] private GameObject ui_msg_prefab;


    [Header("References")]
    [SerializeField] private MessageLoader MessageLoader;
    private Dictionary<string, Message> messages = new Dictionary<string, Message>(); // msg by id

    // START
    private void Start()
    {
        messages = MessageLoader.LoadMessages();
    }


    // UI_MSG creation
    public UI_Message CreateUIMessage(string msg_id, Transform parent, TalkCapacity talker)
    {
        Message message = GetMessage(msg_id);
        if (message == null) return null;
        return CreateUIMessage(message, parent, talker);
    }
    public UI_Message CreateUIMessage(Message message, Transform parent, TalkCapacity talker)
    {
        GameObject ui_msg_go = Instantiate(ui_msg_prefab, parent);
        UI_Message ui_msg = ui_msg_go.GetComponent<UI_Message>();
        ui_msg.Init(message);
        return ui_msg;
    }


    // GETTERS
    public Message GetMessage(string id)
    {
        if (messages.TryGetValue(id, out Message message)) { return message; }
        Debug.LogWarning($"Message with id {id} not found in MessageBank.");
        return null;
    }
    public Message GetRandomMessage()
    {
        if (messages.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, messages.Count);
            return messages.Values.ElementAt(index);
        }
        Debug.LogWarning("No messages available in MessageBank.");
        return null;
    }
}

[Serializable] public class Message
{
    public string id;
    public string text;
    public int width;
    public string localization_key = "eng";
}