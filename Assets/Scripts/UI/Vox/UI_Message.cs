using UnityEngine;
using UnityEngine.UI;

public class UI_Message : MonoBehaviour
{

    // static variables
    public const int WAITING_DURATION = 5;
    public const int FADING_DURATION = 5;


    // instance variables
    public Message message;
    
    // [Header("Colors")]
    // [SerializeField] private Color slot_color = Color.white;
    // [SerializeField] private Color text_color = Color.black;


    [Header("Components")]
    [SerializeField] private Transform slot;
    [SerializeField] private UI_Writer writer;
    [SerializeField] private Transitioner transitioner;
    private RectTransform _bulleRectTransform;
    private RectTransform bulleRect
    {
        get
        {
            if (_bulleRectTransform != null) { return _bulleRectTransform; }
            _bulleRectTransform = transform.Find("bulle").GetComponent<RectTransform>();
            return _bulleRectTransform;
        }
    }
    private RectTransform _notchRectTransform;
    private RectTransform notchRect
    {
        get
        {
            if (_notchRectTransform != null) { return _notchRectTransform; }
            _notchRectTransform = transform.Find("notch").GetComponent<RectTransform>();
            return _notchRectTransform;
        }
    }
    public bool IsWriting => writer.IsWriting;
    public bool IsFading { get; private set; } = false;
    public float WritingTime { get; private set; } = 0f;

    public void Init(Message message)
    {
        this.message = message;

        // set the bulle size
        bulleRect.sizeDelta = new Vector2(message.width, bulleRect.sizeDelta.y);
    }


    // START WRITING
    private TalkCapacity talker;
    private TalkCapacity holder;
    public async void StartWriting()
    {
        // ensure we have a talker reference to send status to
        if (talker == null || holder == null)
        {
            Debug.LogWarning($"(UI_Message) No TalkCapacity holder or talker was found at start writing for {this.gameObject.name}.");
        }

        // we start writing before any showing stuff
        WritingTime = Time.unscaledTime;
        writer.Write(message.text);

        await transitioner.Show(); // first we show the canvas group
        while (writer.IsWriting) { await System.Threading.Tasks.Task.Yield(); } // then we wait for the writing to be done
        talker.OnMessageDoneTalking(this);
        if (holder != null && holder != talker) { holder.OnMessageDoneTalking(this); }

        await System.Threading.Tasks.Task.Delay(WAITING_DURATION * 1000); // we wait before fading the message
        if (gameObject == null) { return; }

        // we also wait for the above sibling to be fading before we do
        int sibling_index = transform.GetSiblingIndex();
        if (sibling_index > 0)
        {
            Transform above_sibling = transform.parent.GetChild(sibling_index - 1);
            UI_Message above_sibling_msg = above_sibling.GetComponent<UI_Message>();
            if (above_sibling_msg != null)
            {
                while (!above_sibling_msg.IsFading) { await System.Threading.Tasks.Task.Yield(); }
            }
        }


        // then we slowly hide the message
        IsFading = true;
        holder.OnMessageFading(this);
        _ = transitioner.HideAndDestroy(duration: FADING_DURATION);
    }
    private void OnDestroy()
    {
        if (holder == null) { return; }
        holder.OnMessageDestroyed(this);
    }
    


    // GETTERS / SETTERS
    public void SetColors(Color slot_color, Color text_color)
    {
        // this.slot_color = slot_color;
        // this.text_color = text_color;

        // we apply the colors
        writer.SetColor(text_color);
        foreach (Transform child in slot)
        {
            Graphic graphic = child.GetComponent<Graphic>();
            if (graphic == null) { continue; }
            graphic.color = slot_color;
        }
        notchRect.GetComponent<Graphic>().color = slot_color;
    }
    public void SetTalker(TalkCapacity talker)
    {
        this.talker = talker;
    }
    public void SetHolder(TalkCapacity holder)
    {
        this.holder = holder;
    }
    public void SetFacing(bool facing_right)
    {
        bulleRect.pivot = new Vector2(facing_right ? 0f : 1f, bulleRect.pivot.y);
        bulleRect.anchorMin = new Vector2(facing_right ? 0f : 1f, bulleRect.anchorMin.y);
        bulleRect.anchorMax = new Vector2(facing_right ? 0f : 1f, bulleRect.anchorMax.y);
        bulleRect.anchoredPosition = new Vector2(0f, bulleRect.anchoredPosition.y);

        // todo we also flip the notch
        notchRect.anchorMin = new Vector2(facing_right ? 0f : 1f, notchRect.anchorMin.y);
        notchRect.anchorMax = new Vector2(facing_right ? 0f : 1f, notchRect.anchorMax.y);
        notchRect.localScale = new Vector3(facing_right ? 1f : -1f, 1f, 1f);
        notchRect.anchoredPosition = new Vector2(0f, notchRect.anchoredPosition.y);

        // todo : we change the text anchor as well, if facing right the text is left aligned
    }
    public void HideNotch(bool hide = true)
    {
        notchRect.gameObject.SetActive(!hide);
    }
}