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
    [SerializeField] private bool allow_bad_words = true;
    [SerializeField] private Vector2 talking_delay_range = new Vector2(30f, 60f);


    [Header("Talks")]
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
    private List<string> talks_random_bad_words = new List<string>()
                        {
                            "fuck this shit/.\ni'm HUNGRY !",
                            "is all of this\nsh*t even real ?",
                            "ahh orangina I love that/l/.f*ck capitalism\nperchance",
                            "fuck Google I'm gonna hack them",
                        };

    [Header("Components")]
    private GameObject floating_dmg_provider;
    private Being being;


    // START
    private void Start()
    {
        // on récupère le provider de floating dmg
        floating_dmg_provider = GameObject.Find("/utils/dmgs_provider");

        // on lance le parlage automatique
        Invoke("randomTalk", Random.Range(talking_delay_range.x, talking_delay_range.y));
    }

    // trigger the attack
    public override void Use(Capable capable)
    {
        if (capable is not Being) {return;}
        being = (Being) capable;

        // todo talking has animations ?
        randomTalk();
        CancelInvoke("randomTalk");
    }
    
    void randomTalk()
    {
        if (being == null) {return;}

        // on fait parler le perso
        int index = Random.Range(0, talks_random.Count + (allow_bad_words ? talks_random_bad_words.Count : 0));
        if (index >= talks_random.Count)
        {
            floating_dmg_provider.GetComponent<TextManager>().talk(talks_random_bad_words[index - talks_random.Count], being);
        }
        else
        {
            floating_dmg_provider.GetComponent<TextManager>().talk(talks_random[index], being);
        }

        // on relance
        Invoke("randomTalk", Random.Range(talking_delay_range.x, talking_delay_range.y));
    }
}