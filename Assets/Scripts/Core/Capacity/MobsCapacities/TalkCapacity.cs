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
    // [SerializeField] private Graphic notch;
    private Canvas _canvas;
    private Canvas canvas
    {
        get
        {
            if (_canvas != null) { return _canvas; }
            _canvas = GetComponentInChildren<Canvas>(includeInactive: true);
            return _canvas;
        }
    }

    private Dictionary<UI_Message, TalkCapacity> ui_messages = new Dictionary<UI_Message, TalkCapacity>();
    private List<TalkCapacity> talk_members = new List<TalkCapacity>();



    ///
    //
    /// MAIN ENTRY POINTS + TALKING
    //
    ///


    // TALKING MAIN METHODS
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
    public void Say(string message_id)
    {
        // get a random message from bank
        UI_Message msg = MessageBank.Instance.CreateUIMessage(message_id);
        if (msg == null) { Debug.LogError($"(TalkCapacity) No message found with id {message_id}"); return; }
        WriteMessage(msg, this);
    }
    public void WriteMessage(UI_Message msg, TalkCapacity talker)
    {
        // notch.color = slot_color;

        // we set the msg as a child of the ui_messages_parent and we set its colors
        take_over_msg(msg, talker);
        msg.SetColors(slot_color, text_color);
        msg.StartWriting();

        // here we need to make sure that the canvas transitionner is shown
        // and then we will receive msg status to hide it when it's done
        _ = main_transitioner.Show();

        talker.Capable.AnimPlayer.Play(talk_anim);

        if (Controller.Capable == null || Controller.Capable == this.Capable) { return; }
        if (talker == this) { return; }

        // here we just heared the ui_message from another talker,
        // and we are not the player. we face the talker to be more immersive
        Capable.Orientation = (talker.Capable.transform.position - Capable.transform.position).normalized;

        // we can also answer it
        Invoke(nameof(talk_randomly), UnityEngine.Random.Range(.5f, 3f));
    }
    
    
    // LOW LEVEL TALKING
    private void take_over_msg(UI_Message msg, TalkCapacity dude_who_talked)
    {
        msg.transform.SetParent(ui_messages_parent);
        msg.transform.localScale = Vector3.one; // important to reset the scale since we change parent
        ui_messages.Add(msg, dude_who_talked);
        msg.SetHolder(this);
        msg.SetTalker(dude_who_talked);

        // here we also set the facing right parameter
        if (was_facing_right == null) { set_facing(dude_who_talked.Capable.Orientation.x >= 0f); return; }
        if (dude_who_talked == this) { msg.SetFacing(was_facing_right.Value); }
        else { msg.SetFacing(!was_facing_right.Value); }
    }
    private void talk_randomly()
    {
        // we rotate over the person we talk to
        TalkCapacity leader = talk_mode.Leader;
        if (leader == null) { return; }
        if (leader != this)
        {
            Capable.Orientation = (leader.Capable.transform.position - Capable.transform.position).normalized;
        }
        else // we turn back to the first talk_member if we are the leader
        {
            if (talk_members.Count == 0) { return; }
            Capable.Orientation = (talk_members[0].Capable.transform.position - Capable.transform.position).normalized;
        }

        SaySomething();
    }


    // RAYCAST LOW LEVEL
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

        Debug.DrawRay(raycast_origin, raycast_direction * raycast_distance, Color.lightPink, duration: 2f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(raycast_origin, raycast_direction, raycast_distance, talking_entity_layers);
        if (hits.Length == 0)
        {
            Debug.Log($"(TalkCapacity) {Capable.ID} said something but no one heard it..");
            return;
        }
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



    // MESSAGE STATUS (events)
    public void OnMessageDoneTalking(UI_Message msg)
    {
        // we check if there are more messages still talking
        foreach (KeyValuePair<UI_Message, TalkCapacity> kvp in ui_messages)
        {
            if (kvp.Key == msg) { continue; }
            if (kvp.Value != this) { continue; }
            if (kvp.Key.IsWriting) { return; }
        }

        Capable.AnimPlayer.StopPlaying(talk_anim);
    }
    public void OnMessageFading(UI_Message msg)
    {
        if (ui_messages.Count == 0) { return; }

        // we check if the just fading message was the last not-fading msg
        foreach (KeyValuePair<UI_Message, TalkCapacity> kvp in ui_messages)
        {
            if (kvp.Key == msg) { continue; }
            if (!kvp.Key.IsFading) { return; }
        }
        // then we can fade the canvas group
        _ = main_transitioner.Hide(UI_Message.FADING_DURATION);
    }
    public void OnMessageDestroyed(UI_Message msg)
    {
        ui_messages.Remove(msg);
    }











    ///
    //
    /// DIALOG METHODS
    //
    ///

    // DIALOG START HANDLING
    private Dictionary<TalkCapacity, UI_Message> last_spoken_dudes = new Dictionary<TalkCapacity, UI_Message>();
    public void OnSomeoneSaidSomething(TalkCapacity someone, UI_Message msg)
    {

        // first, if we already said something, we can enter dialog if we want,
        // maybe this person wants to talk with us, that would be nice
        if (ui_messages.Count > 0)
        {
            AcceptOrRefuseDialog(someone);
            return;
        }
        else if (talk_mode.Mode == TalkType.Dialog && talk_mode.Leader != this)
        {
            // if we are in a dialog and not the leader, it means the leader is the one holding the ui_msgs, so that's why we agree
            talk_mode.Leader.AcceptOrRefuseDialog(someone);
            return;
        }


        // here we did not say anything yet, so

        // we register to a cooldown with these infos so
        // if in the next seconds we say something,
        // we can ask dialog
        last_spoken_dudes[someone] = msg;
        Debug.Log($"(TalkCapacity) {Capable.ID} heard from {someone.Capable.ID}.\nmessage was : {msg.message.text}");

        if (Controller.Capable == null || Controller.Capable == this.Capable) { return; }
        
        // orientate towards the talker who just said something
        Vector2 orientation_to_talker = (someone.Capable.transform.position - Capable.transform.position).normalized;
        Capable.Orientation = orientation_to_talker;

        // we automatically answer after 1s if we are not the controlled capable
        Invoke("SaySomething", 1f);
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

        if (Controller.Capable == null || Controller.Capable == this.Capable) { return; }

        // here we start invoking the talk method so the npc is talking randomly
        InvokeRepeating(nameof(talk_randomly), UnityEngine.Random.Range(3, 10), 3f);
    }
    public void ReceiveNewDialogMember(TalkCapacity new_member)
    {
        // we gather all the new member messages into our own list
        Debug.Log($"(TalkCapacity) {Capable.ID} is receiving new dialog member {new_member.Capable.ID}.");
        foreach (KeyValuePair<UI_Message, TalkCapacity> kvp in new_member.GetCurrentMessages())
        {
            UI_Message ui_msg = kvp.Key;
            TalkCapacity talker = kvp.Value;

            float smallest_time_diff = Mathf.Infinity;
            int sibling_index_to_take = 0;
            foreach (KeyValuePair<UI_Message, TalkCapacity> kvp2 in ui_messages)
            {
                float time_diff = ui_msg.WritingTime - kvp2.Key.WritingTime;
                if (time_diff > 0f) { continue; }
                if (Mathf.Abs(time_diff) < smallest_time_diff)
                {
                    smallest_time_diff = Mathf.Abs(time_diff);
                    sibling_index_to_take = kvp2.Key.transform.GetSiblingIndex();
                }
            }
            // we want to set the according sibling index for the message based on its writing time
            // we find the sibling index of the message that has its writing time just after the one of our message
            // smallest negative time diff !!!!
            take_over_msg(ui_msg, talker);

            // we apply the sibling index
            if (smallest_time_diff != Mathf.Infinity)
            {
                ui_msg.transform.SetSiblingIndex(sibling_index_to_take);
            }
        }

        // then we add the talk capacity to the ongoing dialog members so our local position will lerp properly in update method
        talk_members.Add(new_member);

        // we can now updat the facing since we have a new member in the dialog
        UpdateFacing(force_update: true);
    }
    
    // DIALOG END HANDLING
    public void QuitDialog()
    {
        CancelInvoke(nameof(talk_randomly)); // i put it here but it can be after the == null check i think

        if (talk_mode.Mode != TalkType.Dialog) { return; }
        if (talk_mode.Leader == null) { return; }


        talk_mode = new TalkMode(TalkType.Monologue, this);

        if (talk_mode.Leader != this)
        {
            Debug.Log($"(TalkCapacity) {Capable.ID} is quitting dialog led by {talk_mode.Leader.Capable.ID}.");
            
            // we get back the messages we send to the leader to handle them again
            foreach (KeyValuePair<UI_Message, TalkCapacity> kvp in talk_mode.Leader.GetCurrentMessages())
            {
                if (kvp.Value != this) { continue; }
                UI_Message ui_msg = kvp.Key;
                take_over_msg(ui_msg, this);
            }

            UpdateFacing(force_update: true);

            return;
        }

        Debug.Log($"(TalkCapacity) Leader {Capable.ID} is quitting dialog. We end it for everyone.");
        // reset things on the leader
        foreach (TalkCapacity talk_member in talk_members)
        {
            talk_member.QuitDialog();
        }
        talk_members.Clear();
        List<UI_Message> msgs_to_remove = new List<UI_Message>();
        foreach (KeyValuePair<UI_Message, TalkCapacity> kvp in ui_messages)
        {
            if (kvp.Value == this) { continue; }
            msgs_to_remove.Add(kvp.Key);
        }
        foreach (UI_Message ui_msg in msgs_to_remove) { ui_messages.Remove(ui_msg); } // ! important : we don't destroy here bcz the ui_msg was transfered to its real talker

        // force update facing
        UpdateFacing(force_update: true);
    }



    ///
    //
    /// UPDATE
    //
    ///

    // UPDATE
    private const float LOCAL_POS_LERP_SPEED = 5f;
    private const float MAX_DIALOG_DISTANCE = 8f;
    private void Update()
    {
        if (!Loaded) { return; }

        // ! important
        // todo : denest this in methods based on if we are the leader & talk mode


        // update ui_message orientation if we changed orientation
        UpdateFacing();


        // update the local position    
        Vector2 avg_local_pos = MouthLocalPosition;
        float max_talk_member_distance = 0f;
        if (talk_mode.Mode == TalkType.Dialog && talk_mode.Leader == this)
        {
            List<TalkCapacity> members_to_remove = new List<TalkCapacity>();
            foreach (TalkCapacity talk_member in talk_members)
            {
                Vector2 distance_vector = (Vector2)(talk_member.Capable.transform.position - Capable.transform.position);
                if (Mathf.Abs(distance_vector.x) > MAX_DIALOG_DISTANCE)
                {
                    members_to_remove.Add(talk_member);
                    continue;
                }

                // target_talk_width = Mathf.Max(target_talk_width, );
                if (Mathf.Abs(distance_vector.x) > max_talk_member_distance) { max_talk_member_distance = Mathf.Abs(distance_vector.x); }

                // convert to local position
                avg_local_pos += talk_member.MouthLocalPosition + distance_vector;
            }

            // we remove the far members from the dialog
            foreach (TalkCapacity talk_member in members_to_remove)
            {
                talk_members.Remove(talk_member);
                talk_member.QuitDialog();
            }
            if (talk_members.Count == 0) { QuitDialog(); }

            avg_local_pos /= (talk_members.Count + 1);
        }


        // we lerp the local position of the talk capacities to the average local position of the dialog members
        Vector2 new_local_pos = Vector2.Lerp(transform.localPosition, avg_local_pos, Time.deltaTime * LOCAL_POS_LERP_SPEED);
        if (Vector2.Distance(new_local_pos, avg_local_pos) < 0.01f)
        {
            transform.localPosition = avg_local_pos;
        }
        else { transform.localPosition = new_local_pos; }



        // update the msg group width
        if (talk_mode.Mode == TalkType.Monologue || talk_mode.Leader != this)
        {
            msg_parent_rect.sizeDelta = new Vector2(150f, msg_parent_rect.sizeDelta.y);
            return;
        }
        else
        {
            // we do a rapport proportionnel to calculate the target msg group width based on max distance btwn talk members :
            // max_distance == MAX_DIALOG_DISTANCE ---------> width == 75f
            // max_distance == 0f -----------------------> width == 0f
            float target_width = (max_talk_member_distance / MAX_DIALOG_DISTANCE) * 100f;
            
            if (target_width < 50f) { target_width = 50f; } // we clamp to a minimum width so the msg don't look too weirdd
            // todo if too small we need to split back the messages in 2 talk capacities, with the inverse order ?
            // todo : or we keep on this talk capa, and raise the width of msg group, and just inverse
            // todo : the facing of the current and future messages


            // Debug.Log($"(TalkCapacity) max talk member distance : {max_talk_member_distance}, so width will be {target_width}");
            msg_parent_rect.sizeDelta = new Vector2(Mathf.Abs(target_width), msg_parent_rect.sizeDelta.y);
        }
    }



    // FACING LOW LEVEL
    private const float HORIZONTAL_MOUTH_OFFSET = .75f;
    public Vector2 MouthLocalPosition { get { return data.local_position + new Vector2(HORIZONTAL_MOUTH_OFFSET * (Facing ? 1f : -1f), 0f); } }
    private bool? was_facing_right = null;
    public bool Facing { get { return was_facing_right != null ? was_facing_right.Value : false; } }
    public void UpdateFacing(bool force_update = false)
    {
        bool new_facing_right = calculate_facing();
        if (new_facing_right == was_facing_right && !force_update) { return; }

        if (talk_mode.Mode == TalkType.Monologue)
        {
            if (Capable.Orientation.x != 0f) { set_facing(new_facing_right); }
            return;
        }

        // else we need to change the the facing
        set_facing(new_facing_right);
    }
    private bool calculate_facing()
    {
        if (talk_mode.Mode == TalkType.Monologue) { return Capable.Orientation.x >= 0f; }
        else if (talk_mode.Leader != this) { return !talk_mode.Leader.Facing; }

        // else we want to look at the average position of the other talk members
        float avg_other_members_pos_x = 0f;
        foreach (TalkCapacity talk_member in talk_members)
        {
            avg_other_members_pos_x += talk_member.Capable.transform.position.x;
        }
        avg_other_members_pos_x /= talk_members.Count;

        return avg_other_members_pos_x >= Capable.transform.position.x;
    }
    private void set_facing(bool facing_right)
    {
        was_facing_right = facing_right;

        if (talk_mode.Mode == TalkType.Monologue)
        {
            msg_parent_rect.pivot = new Vector2(facing_right ? 0f : 1f, msg_parent_rect.pivot.y);
            msg_parent_rect.anchoredPosition = new Vector2(0f, msg_parent_rect.anchoredPosition.y);
        }
        else if (talk_mode.Leader == this)
        {
            msg_parent_rect.pivot = new Vector2(0.5f, msg_parent_rect.pivot.y);
            msg_parent_rect.anchoredPosition = new Vector2(0f, msg_parent_rect.anchoredPosition.y);

            // todo here we can update the width ?
        }

        foreach (KeyValuePair<UI_Message, TalkCapacity> kvp in ui_messages)
        {
            if (kvp.Value == this) { kvp.Key.SetFacing(facing_right); }
            else { kvp.Key.SetFacing(!facing_right); }
        }
    }


    ///
    //
    /// GETTERS
    //
    ///

    // GETTERS
    public Dictionary<UI_Message, TalkCapacity> GetCurrentMessages() { return ui_messages; }



    ///
    //
    /// DATA MANAGEMENT
    //
    ///

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

    // TODO : WE COULD HOLD THE TALK MEMBERS HERE, would be better

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