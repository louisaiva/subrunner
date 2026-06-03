using UnityEngine;

public class UI_Message : MonoBehaviour
{

    // static variables
    public const int WAITING_DURATION = 5;
    public const int FADING_DURATION = 5;


    // instance variables
    public Message message;
    [SerializeField] private UI_Writer writer;
    [SerializeField] private Transitioner transitioner;
    private RectTransform _rectTransform;
    private RectTransform rectTransform
    {
        get
        {
            if (_rectTransform != null) { return _rectTransform; }
            _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }
    public bool IsWriting => writer.IsWriting;
    public bool IsFading { get; private set; } = false;


    private TalkCapacity talker;
    public async void Init(Message message)
    {
        this.message = message;
        rectTransform.sizeDelta = new Vector2(message.width, rectTransform.sizeDelta.y);
        talker = GetComponentInParent<TalkCapacity>();
        if (talker == null)
        {
            Debug.LogWarning($"(UI_Message) No TalkCapacity found in parent hierarchy of {this.gameObject.name}.");
        }

        // we start writing before any showing stuff
        writer.Write(message.text);

        await transitioner.Show(); // first we show the canvas group
        while (writer.IsWriting) { await System.Threading.Tasks.Task.Yield(); } // then we wait for the writing to be done
        talker.OnMessageDoneTalking(this);

        await System.Threading.Tasks.Task.Delay(WAITING_DURATION * 1000); // and finally we wait before fading the message

        // ? we can also wait for the potential just above sibling ui_message to be fading before fading ?


        // then we slowly hide the message
        IsFading = true;
        talker.OnMessageFading(this);
        await transitioner.HideAndDestroy(duration: FADING_DURATION);
    }
    private void OnDestroy()
    {
        if (talker == null) { return; }
        talker.OnMessageDestroyed(this);
    }
}