using UnityEngine;
using UnityEngine.UI;

public class UI_Message : MonoBehaviour
{

    // static variables
    public const int WAITING_DURATION = 5;
    public const int FADING_DURATION = 5;


    // instance variables
    public Message message;
    
    [Header("Colors")]
    [SerializeField] private Color slot_color = Color.white;
    [SerializeField] private Color text_color = Color.black;


    [Header("Components")]
    [SerializeField] private Transform slot;
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
    public float WritingTime { get; private set; } = 0f;




    private TalkCapacity talker;
    public void Init(Message message)
    {
        this.message = message;
        rectTransform.sizeDelta = new Vector2(message.width, rectTransform.sizeDelta.y);
    }
    public async void StartWriting()
    {
        // ensure we have a talker reference to send status to
        if (talker == null)
        {
            Debug.LogWarning($"(UI_Message) No TalkCapacity was found at start writing for {this.gameObject.name}.");
        }

        // we start writing before any showing stuff
        WritingTime = Time.unscaledTime;
        writer.Write(message.text);

        await transitioner.Show(); // first we show the canvas group
        while (writer.IsWriting) { await System.Threading.Tasks.Task.Yield(); } // then we wait for the writing to be done
        talker.OnMessageDoneTalking(this);

        await System.Threading.Tasks.Task.Delay(WAITING_DURATION * 1000); // we wait before fading the message
        if (gameObject == null) { return; }

        // we also wait for the above sibling to be fading before we do
        int sibling_index = rectTransform.GetSiblingIndex();
        if (sibling_index > 0)
        {
            Transform above_sibling = rectTransform.parent.GetChild(sibling_index - 1);
            UI_Message above_sibling_msg = above_sibling.GetComponent<UI_Message>();
            if (above_sibling_msg != null)
            {
                while (!above_sibling_msg.IsFading) { await System.Threading.Tasks.Task.Yield(); }
            }
        }


        // then we slowly hide the message
        IsFading = true;
        talker.OnMessageFading(this);
        _ = transitioner.HideAndDestroy(duration: FADING_DURATION);
    }
    private void OnDestroy()
    {
        if (talker == null) { return; }
        talker.OnMessageDestroyed(this);
    }
    
    // GETTERS / SETTERS
    public void SetColors(Color slot_color, Color text_color)
    {
        this.slot_color = slot_color;
        this.text_color = text_color;

        // we apply the colors
        writer.SetColor(text_color);
        foreach (Transform child in slot)
        {
            Graphic graphic = child.GetComponent<Graphic>();
            if (graphic == null) { continue; }
            graphic.color = slot_color;
        }
    }
    public void SetTalker(TalkCapacity talker)
    {
        this.talker = talker;
    }
}