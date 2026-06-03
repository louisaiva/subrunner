using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PROTOTYPE :
/// For now it is a very simple class that just loads messages written in the inspector,
/// but we should implement a new version of it that loads from external storage
/// (json ? xml ? google doc ? tbd)
/// </summary>
public class MessageLoader : MonoBehaviour
{
    public List<Message> messages = new List<Message>();
    public Dictionary<string, Message> LoadMessages()
    {
        return messages.ToDictionary(message => message.id, message => message);
    }

}