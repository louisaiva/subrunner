using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TalkCapacity is a capacity that allows a being to talk
/// it shows subtitle on the right-top corner of the character
/// </summary>

public class TalkCapacity : Capacity
{
    [SerializeField] private bool log_lerp = false;

    [Header("Talk mode")]
    [SerializeField] private TalkMode talk_mode;

    [Header("Talking parameters")]
    [SerializeField] private string talk_anim = "talk";

    [Header("Colors")]
    [SerializeField] private Color slot_color = Color.white;
    [SerializeField] private Color text_color = Color.black;


    [Header("Talks (NEED TO BE IN A FILE OR IN MESSAGE LOADER)")]
    [SerializeField]
    private List<string> talks_random = new List<string>()
                        {
                            "here we go again/.",
                            "well/.i'm not dead yet :D/lthat's a good start",
                            "give me some noodles !/l/.anyone ?",
                            "still not dead/./lbut am i alive ?",
                            "let's decrypt\nthe chaos, babyy",
                            "ARGHHH/.ZOMBOS !",
                            "those posters reminds\nme of the pixel war/./lwait/.wtf is the pixel war ?",
                            "looks like life has/./lno meaning after all",
                            "let's ctrl+alt+suppr\nall those cyberzombies",
                            "do you feel the\nbreath of the city ?",
                            "i'm talking alone/./las always..",
                            "bits & bytes are\nmy only love, honey",
                            "404: noodles not found :/",
                            "feel like the labyrinth\nkeeps moving/.is it ?",
                            "omg i almost glitched oO",
                            "what if i could pass\nthrough those walls ?/lwell/.I can't",
                            "this city is full of bugs/./llooks like a game actually",
                            "ahh, the smell of the city,/lso/.digital",
                            "i'm a pixel runner/.i'm a pixel runner",
                            "let's hack the world, baby !",
                            "running in da\nunderground,/lagain/.and again/.n agn",
                            "i'm so tired/./lwhen was the last\ntime i slept ?",
                            "why do i keep\nrunning, really ?",
                            "this wall looks suspicious/. O.o",
                            "feels good to run after/./l502420h of debugging",
                            "here i dash/.here i slash",
                            "where is my old\nfriend Marco ?",
                            "ayy, 9 years without\nseeing the sun,/lwho can beat that ?",
                            "i need to find the exit/./lis there even an exit ?",
                            "ahhh what if I\ncould ctrl-Z irl ?",
                            "alright./ltime to remember passwords/./ls7j3k9../lwell/.not this time apparently",
                            "what's my name again ?",
                            "ahh/./lI love feeling the\nbytes flowing in my veins..",
                            "where is the internet again ?",
                            "zombos, zombos\neverywhere/./lwelcome to the\nzapocalypse",
                            "noodles,/lthe ultimate cure\nfor existential hunger !",
                            "starving and debugging/./la perfect combo, really",
                            "is this reality or just\na weird illusion ?",
                            "zombos & hacking/./lno way it's real/./lit's so/.mainstream",
                            "OH MY GOD,/ljust found the bug\nI was struggling with/./lToo bad my computer\npassed away..",
                            "line 475,car. 45 :\n(WorldError)\nhere's comes the \"déjà vu\"",
                            "looks like the matrix/./lugh ?/lwhat did I just say ?",
                            "i could use a jetpack/./lwell no that's\nanother game./lwait/.a game ?",
                            "ohhh, cuty rat !",
                            "why do I keep\ntalking alone ?/l;-;",
                            "i'm so tired of\nrunning/l/.looks like\nI can't stop",
                            "how many times\nI've been here ?",
                            "mmmmh/./lhow many zombos lives is\nworth some noodles ?",
                            "pff !/lthose zombos are\nso annoying",
                            "ahh/./lloneliness is almost\nas scary as\nthe deep web",
                            "maybe I'll find\nsome friends/./l/. but I want noodles !",
                        };
    [SerializeField]
    private List<string> talks_random_bad_words = new List<string>()
                        {
                            "fuck this shit/.\ni'm HUNGRY !",
                            "is all of this\nsh*t even real ?",
                            "ahh orangina I love that/l/.f*ck capitalism\nperchance",
                            "fuck Google I'm gonna hack them",
                        };

    [Header("Components")]
    // private GameObject floating_dmg_provider;
    [SerializeField] private Transform ui_messages_parent;
    private RectTransform _msg_parent_rect;
    private RectTransform msg_parent_rect
    {
        get
        {
            if (_msg_parent_rect != null) { return _msg_parent_rect; }
            _msg_parent_rect = ui_messages_parent.GetComponent<RectTransform>();
            return _msg_parent_rect;
        }
    }
    [SerializeField] private Transitioner main_transitioner;
    [SerializeField] private Graphic notch;

    private List<UI_Message> ui_messages = new List<UI_Message>();
    private List<TalkCapacity> talk_members = new List<TalkCapacity>();


    // MAIN ENTRY POINT + TALKING
    public override void Use(Capable capable) { SaySomething(); }
    public void SaySomething()
    {
        // get a random message from bank
        UI_Message msg = MessageBank.Instance.CreateUIMessage(MessageBank.Instance.GetRandomMessage());

        // then we need to handle properly the ui_message we just created
        // either we are talking alone (monologue), in this case we handle the ui_msg ourselves
        // or we are into a dialog, which means we need to know which is the leading talk capacity for this dialog

        if (talk_mode.Mode == TalkType.Monologue)
        {
            WriteMessage(msg, this);

            // we send an event (raycast) in front of us and if we find a potential talker,
            // we trigger their OnSomeoneSaidSomething method, so they can ask us back if they want to start a dialog
            raycast_in_front_of_me(msg);
            return;
        }

        // here we are in a dialog
        TalkCapacity talk_leader = talk_mode.Leader;
        if (talk_leader == null)
        {
            Debug.LogWarning($"(TalkCapacity) No talk leader assigned for dialog mode in {this.gameObject.name}.");
            WriteMessage(msg, this);
            return;
        }

        talk_leader.WriteMessage(msg, this);
    }
    public void WriteMessage(UI_Message msg, TalkCapacity talker)
    {
        notch.color = slot_color;

        // we set the msg as a child of the ui_messages_parent and we set its colors
        take_over_msg(msg);
        msg.SetColors(slot_color, text_color);
        msg.StartWriting();

        // here we need to make sure that the canvas transitionner is shown
        // and then we will receive msg status to hide it when it's done
        _ = main_transitioner.Show();

        Capable.AnimPlayer.Play(talk_anim);
    }
    private void take_over_msg(UI_Message msg)
    {
        msg.transform.SetParent(ui_messages_parent);
        msg.transform.localScale = Vector3.one; // important to reset the scale since we change parent
        ui_messages.Add(msg);
        msg.SetTalker(this);

        // here we also set the facing right parameter
        // todo : if we are in dialog we need to check which talker said what so that leader messages are properly oriented,
        // but opposite to the other talkers, but for now we only have one talker in dialog so it doesn't matter
        msg.SetFacing(was_facing_right);
    }


    [SerializeField] private LayerMask talking_entity_layers;
    private const float RAYCAST_DISTANCE = 3f;
    private void raycast_in_front_of_me(UI_Message msg)
    {
        // first we delete all the old talkers in cooldown
        List<TalkCapacity> talkers_to_remove = new List<TalkCapacity>();
        foreach (KeyValuePair<TalkCapacity, UI_Message> kvp in last_spoken_dudes)
        {
            if (kvp.Value == null) { talkers_to_remove.Add(kvp.Key); } // the msg was destroyed, we can remove the talker from the cooldown
        }
        foreach (TalkCapacity talker in talkers_to_remove)
        {
            last_spoken_dudes.Remove(talker);
        }

        // then we raycast in front of us to find potential talkers
        Vector2 raycast_origin = Capable.transform.position;
        Vector2 raycast_direction = Capable.Orientation;
        float raycast_distance = RAYCAST_DISTANCE;

        RaycastHit2D[] hits = Physics2D.RaycastAll(raycast_origin, raycast_direction, raycast_distance, talking_entity_layers);
        if (hits.Length == 0)
        {
            Debug.Log($"(TalkCapacity) {Capable.ID} said something but no one heard it..");
            return;
        }
        Debug.DrawRay(raycast_origin, raycast_direction * raycast_distance, Color.lightPink, duration: 1f);
        List<Capable> hearing_capables = new List<Capable>();
        foreach (RaycastHit2D hit in hits)
        {
            // check if the hit thing has a capable on it
            Capable capable = hit.transform.GetComponent<Capable>();
            if (capable == null)
            {
                if (hit.transform.parent == null) { continue; }
                if (hit.transform.parent.parent == null) { continue; }
                capable = hit.transform.parent.parent.GetComponent<Capable>();
            }
            if (capable == null) { continue; }
            if (capable == this.Capable) { continue; } // we don't want to talk to ourself, that would be sad
            if (hearing_capables.Contains(capable)) { continue; }
            hearing_capables.Add(capable);
            if (!capable.TryGetCapacity(out TalkCapacity potential_talker)) { continue; }

            // we check if the potential talker is in cooldown with us
            if (last_spoken_dudes.ContainsKey(potential_talker))
            {
                // yes we dooooo omg !! we now ask directly for dialog with this dude
                AskDialog(potential_talker);
                return;
            }

            // we trigger the potential talker's OnSomeoneSaidSomething method, so they can ask us back if they want to start a dialog
            potential_talker.OnSomeoneSaidSomething(this, msg);
        }
        Debug.Log($"(TalkCapacity) {Capable.ID} said something and {hearing_capables.Count} colliders heard it.\n they are : \n  -{string.Join("\n  -", hearing_capables.Select(c => c.ID))}");
    }


    // DIALOG START HANDLING
    private Dictionary<TalkCapacity, UI_Message> last_spoken_dudes = new Dictionary<TalkCapacity, UI_Message>();
    public void OnSomeoneSaidSomething(TalkCapacity someone, UI_Message msg)
    {
        // we register to a cooldown with these infos so
        // if in the next seconds we say something,
        // we can ask dialog
        last_spoken_dudes[someone] = msg;
        Debug.Log($"(TalkCapacity) {Capable.ID} heard from {someone.Capable.ID}.\nmessage was : {msg.message.text}");

        // we automatically answer after 1s if we are not the controlled capable
        if (Controller.Capable != null && Controller.Capable != this.Capable)
        {
            Invoke("SaySomething", 1f);
        }
    }
    public void AskDialog(TalkCapacity first_spoken_dude)
    {
        // the first spoken dude said something.
        // we also just said something back, so we basically ask the first spoken dude if he wants to start a dialog with us
        Debug.Log($"(TalkCapacity) {Capable.ID} is asking dialog to {first_spoken_dude.Capable.ID}.");
        first_spoken_dude.AcceptOrRefuseDialog(this);
    }
    public void AcceptOrRefuseDialog(TalkCapacity asker)
    {
        // we first said something
        // then the asker answered by saying something,
        // and now we can decide to accept/refuse the dialog
        
        // todo [long-term] : check IA_SocialData to check if
        // we like this asker or not. if we hate them, we 
        // basically refuse the dialog.
        // but for now we always accept dialog

        // if we are already in a dialog, we transmit the talk mode to the new asker
        if (talk_mode.Mode == TalkType.Dialog)
        {
            asker.StartDialog(talk_mode);
            return;
        }

        // else we have no dialog running, we set ourself as the leader of the new dialog
        Debug.Log($"(TalkCapacity) {Capable.ID} is creating dialog with {asker.Capable.ID}.");
        talk_mode = new TalkMode(TalkType.Dialog, this);
        asker.StartDialog(talk_mode);
    }
    public void StartDialog(TalkMode mode)
    {
        Debug.Log($"(TalkCapacity) {Capable.ID} is starting dialog with {mode.Leader.Capable.ID}.");
        talk_mode = mode;
        talk_mode.Leader.ReceiveNewDialogMember(this);

        // we clear our messages since it is now handled by the talk leader
        ui_messages.Clear(); // ! important : we don't destroy the msgs bcz it was moved to the leader talk capacity. we just don't care about them anymore
    }
    public void ReceiveNewDialogMember(TalkCapacity new_member)
    {
        // we gather all the new member messages into our own list
        Debug.Log($"(TalkCapacity) {Capable.ID} is receiving new dialog member {new_member.Capable.ID}.");
        foreach (UI_Message ui_msg in new_member.GetCurrentMessages())
        {
            float smallest_time_diff = Mathf.Infinity;
            int sibling_index_to_take = 0;
            for (int i = 0; i < ui_messages.Count; i++)
            {
                float time_diff = ui_msg.WritingTime - ui_messages[i].WritingTime;
                if (time_diff > 0f) { continue; }
                if (Mathf.Abs(time_diff) < smallest_time_diff)
                {
                    smallest_time_diff = Mathf.Abs(time_diff);
                    sibling_index_to_take = i;
                }
            }
            // we want to set the according sibling index for the message based on its writing time
            // we find the sibling index of the message that has its writing time just after the one of our message
            // smallest negative time diff !!!!
            take_over_msg(ui_msg);

            // we apply the sibling index
            if (smallest_time_diff != Mathf.Infinity)
            {
                ui_msg.transform.SetSiblingIndex(sibling_index_to_take);
            }
        }

        // then we add the talk capacity to the ongoing dialog members so our local position will lerp properly in update method
        talk_members.Add(new_member);
    }

    // UPDATE
    private const float LOCAL_POS_LERP_SPEED = 5f;
    private const float MAX_DIALOG_DISTANCE = 5f;
    private const float HORIZONTAL_MOUTH_OFFSET = .75f;
    public Vector2 MouthLocalPosition { get { return data.local_position + new Vector2(HORIZONTAL_MOUTH_OFFSET * (was_facing_right ? 1f : -1f), 0f); } }
    private bool was_facing_right = false;
    private bool FacingRight { get { return Capable.Orientation.x >= 0f; } }
    private void Update()
    {
        if (!Loaded) { return; }


        // update ui_message orientation if we changed orientation
        if (FacingRight != was_facing_right && Capable.Orientation.x != 0f) { SetFacing(FacingRight); }


        // update the local position
        Vector2 avg_local_pos = MouthLocalPosition;
        if (talk_mode.Mode == TalkType.Dialog)
        {
            List<TalkCapacity> members_to_remove = new List<TalkCapacity>();
            foreach (TalkCapacity talk_member in talk_members)
            {
                // get the world position
                Vector2 talk_member_world_pos = talk_member.transform.position;
                if (Vector2.Distance(talk_member_world_pos, transform.position) > MAX_DIALOG_DISTANCE)
                {
                    members_to_remove.Add(talk_member);
                    continue;
                }

                // convert to local position
                avg_local_pos += talk_member.MouthLocalPosition + (Vector2)(talk_member.Capable.transform.position - Capable.transform.position);
            }

            // we remove the far members from the dialog
            foreach (TalkCapacity talk_member in members_to_remove)
            {
                talk_members.Remove(talk_member);
                // todo : here we notify the member that they quit the conv,
                // and we give them back their messages
            }

            avg_local_pos /= (talk_members.Count + 1);
        }

        // we lerp the local position of the talk capacities to the average local position of the dialog members
        Vector2 new_local_pos = Vector2.Lerp(transform.localPosition, avg_local_pos, Time.deltaTime * LOCAL_POS_LERP_SPEED);
        if (Vector2.Distance(new_local_pos, avg_local_pos) < 0.01f)
        {
            transform.localPosition = avg_local_pos;
            return;
        }
        transform.localPosition = new_local_pos;
    }
    public void SetFacing(bool facing_right)
    {
        was_facing_right = facing_right;

        // // todo : change pivot of msg group + reset anchored position
        msg_parent_rect.pivot = new Vector2(facing_right ? 0f : 1f, msg_parent_rect.pivot.y);
        msg_parent_rect.anchoredPosition = new Vector2(0f, msg_parent_rect.anchoredPosition.y);

        foreach (UI_Message ui_msg in ui_messages)
        {
            ui_msg.SetFacing(facing_right);
        }
    }


    // MESSAGE STATUS
    public void OnMessageDoneTalking(UI_Message msg)
    {
        // we check if there are more messages still talking
        foreach (UI_Message ui_msg in ui_messages)
        {
            if (ui_msg == msg) { continue; }
            if (ui_msg.IsWriting) { return; }
        }

        Capable.AnimPlayer.StopPlaying(talk_anim);
    }
    public void OnMessageFading(UI_Message msg)
    {
        if (ui_messages.Count == 0) { return; }

        // we check if the just fading message was the last not-fading msg
        foreach (UI_Message ui_msg in ui_messages)
        {
            if (ui_msg == msg) { continue; }
            if (!ui_msg.IsFading) { return; }
        }
        // then we can fade the canvas group
        _ = main_transitioner.Hide(UI_Message.FADING_DURATION);
    }
    public void OnMessageDestroyed(UI_Message msg)
    {
        ui_messages.Remove(msg);
    }

    // GETTERS
    public List<UI_Message> GetCurrentMessages() { return ui_messages; }

    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data)
    {
        base.LoadData(data);

        _ = main_transitioner.Hide(0f);
        // talk_members = new List<TalkCapacity>() { this };

        // todo : load the data
        /* if (data is not TalkData talk_data) { return; }
        talk_anim = talk_data.talk_anim_name; */
        // todo : just above, but first we need to write getstaticdata & save template so for now we don't
    }
    /* public override void UnloadData()
    {
        // this saves the dynamic health data
        base.UnloadData();
    } */
}

public class TalkData : CapacityData
{

    // template parameters
    public string talk_anim_name;
    public Color slot_color;
    public Color text_color;

    // CONSTRUCTOR
    public TalkData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new TalkData(base.Duplicate() as CapacityData)
        {
            talk_anim_name = this.talk_anim_name,
            slot_color = this.slot_color,
            text_color = this.text_color,
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - talk anim name : {talk_anim_name}\n";
        details += $"  - slot color : {slot_color}\n";
        details += $"  - text color : {text_color}\n";
        return base.GetDetails() + details;
    }
}



// RTO
[Serializable] public class TalkMode
{
    public TalkType Mode;
    public TalkCapacity Leader;
    public TalkMode(TalkType Mode, TalkCapacity Leader)
    {
        this.Mode = Mode;
        this.Leader = Leader;
    }
}
public enum TalkType
{
    Monologue,
    Dialog,
}