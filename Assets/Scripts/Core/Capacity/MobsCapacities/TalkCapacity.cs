using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// TalkCapacity is a capacity that allows a being to talk
/// it shows subtitle on the right-top corner of the character
/// </summary>

public class TalkCapacity : Capacity
{

    [Header("Talking parameters")]
    [SerializeField] private string talk_anim = "talk";


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
    [SerializeField] private Transitioner main_transitioner;

    private List<UI_Message> ui_messages = new List<UI_Message>();


    // MAIN ENTRY POINT + TALKING
    public override void Use(Capable capable)
    {
        /* // todo talking has animations ?
        randomTalk();
        CancelInvoke("randomTalk"); */

        // monologue
        SaySomething();
    }
    public void SaySomething()
    {
        // get a random message from bank
        UI_Message msg = MessageBank.Instance.CreateUIMessage(MessageBank.Instance.GetRandomMessage(), ui_messages_parent, this);
        ui_messages.Add(msg);

        // here we need to make sure that the canvas transitionner is shown
        // and then we will receive msg status to hide it when it's done
        _ = main_transitioner.Show();

        // todo we start talking anim here
        Capable.AnimPlayer.Play(talk_anim);
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

        // todo then we stop the anim player "talk" anim
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


    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data)
    {
        base.LoadData(data);

        _ = main_transitioner.Hide(0f);

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

    // CONSTRUCTOR
    public TalkData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new TalkData(base.Duplicate() as CapacityData)
        {
            talk_anim_name = this.talk_anim_name,
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - talk anim name : {talk_anim_name}\n";
        return base.GetDetails() + details;
    }
}