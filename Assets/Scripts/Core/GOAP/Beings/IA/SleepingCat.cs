using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SleepingCat : IA
{

    [Header("Nap spots")]
    public Transform nap_spot; // The current sleeping spot
    public Transform last_nap_spot;
    public List<Transform> nap_spots = new List<Transform>(); // List of possible sleeping spots
    public List<Vector2> nap_spots_ratings = new List<Vector2>(); // Ratings for each sleeping spot, where x is the time spent sleeping and y the number of nights slept here

    [Header("Daily life parameters")]
    public float timer_before_next_sleeping = 0f; // Timer for how long before the next sleeping action
    public float seconds_per_lick_stay_up = 5f; // for one lick we stay up for this many seconds -> so 5 licks = 25 seconds of staying up before getting back to sleep

    // START
    protected override void Start()
    {
        base.Start(); // Call the base class Start method

        // Initialize the nap spots and ratings
        if (nap_spots.Count == 0) { return; }
        for (int i = 0; i < nap_spots.Count; i++)
        {
            nap_spots_ratings.Add(new Vector2(0f, 0f)); // Initialize ratings with zero time and zero nights
        }

        // Start the daily loop coroutine
        StartCoroutine(DayLoop());
    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        // we reduce the timer for the next sleeping action
        if (timer_before_next_sleeping > 0f)
        {
            timer_before_next_sleeping -= Time.deltaTime;
        }
        else
        {
            timer_before_next_sleeping = 0f;
        }
    }

    // BEHAVIORS
    public IEnumerator DayLoop()
    {
        // This is the main loop for the cat's daily life
        while (true)
        {
            yield return new WaitForSeconds(1f); // Wait for 1 second before the next action

            // Check if the cat needs to sleep
            if (timer_before_next_sleeping <= 0f && !GetCapacity<SleepCapacity>().asleep)
            {
                yield return SleepSomewhere();
            }
        }
    }
    public IEnumerator SleepSomewhere()
    {
        // we pick our best nap spot
        Transform chosen_spot = PickBestNapSpot();
        if (chosen_spot == null) { yield break; }

        if (log) { Debug.Log("Cat " + name + " chose to sleep at: " + chosen_spot.name); }

        // we go there
        // yield return GoToCoroutine(chosen_spot.position);

        // we sleep at the chosen spot
        nap_spot = chosen_spot; // Set the current nap spot
        GetCapacity<SleepCapacity>().Use(this); // Use the sleep capacity to start sleeping
    }

    // NAP SPOTS
    public void RateNap(float time_spent_asleep)
    {
        // checks if we have a nap spot
        if (nap_spot == null) { return; }

        // we find the index of the nap spot in the list
        int index = nap_spots.IndexOf(nap_spot);

        // if we don't have a rating for this nap spot
        // (index will be -1 if the nap spot is not in the list)
        if (index < 0)
        {
            // we add a new rating
            nap_spots.Add(nap_spot); // add the nap spot to the list
            nap_spots_ratings.Add(new Vector2(time_spent_asleep, 1)); // add a new rating with time spent asleep and number of nights slept here
            if (log) { Debug.Log("New nap spot rated: " + nap_spot.name + " with time: " + time_spent_asleep); }
            return;
        }

        // we have a rating for this nap spot
        else if (index < nap_spots_ratings.Count)
        {
            // we update the rating
            Vector2 rating = nap_spots_ratings[index];
            rating.x += time_spent_asleep; // increase the time spent sleeping
            rating.y += 1; // increase the number of nights slept here
            nap_spots_ratings[index] = rating;
        }

        // we remove the nap spot
        last_nap_spot = nap_spot;
        nap_spot = null;
    }
    public Transform PickBestNapSpot()
    {
        // we verify if we have any nap spots
        if (nap_spots.Count == 0) { return null; }

        // we sort the nap spots based on their ratings
        List<Transform> sorted_spots = sort_nap_spots();

        // we overlap check to see if the last nap spot (where we just woke up) is still a valid nap spot
        sorted_spots = remove_current_nap_spot_if_populated(sorted_spots);

        // we have 75% chance to pick the best nap spot
        if (Random.value < 0.75f) { return sorted_spots[0]; }
        else
        {
            // we pick a random nap spot
            return sorted_spots[Random.Range(0, sorted_spots.Count)];
        }
    }
    private List<Transform> sort_nap_spots()
    {
        // Sort the nap spots based on their ratings
        return nap_spots.OrderByDescending(t => nap_spots_ratings[nap_spots.IndexOf(t)].x
            / nap_spots_ratings[nap_spots.IndexOf(t)].y + 1).ToList(); // Sort the nap spots accordingly
    }
    private List<Transform> remove_current_nap_spot_if_populated(List<Transform> sorted_spots)
    {
        if (last_nap_spot == null || sorted_spots.Count <= 0) { return sorted_spots; }

        // we get the circle collider 2D of the sleeping capacity
        Collider2D collider = GetCapacity<SleepCapacity>().GetComponent<Collider2D>();
        if (collider == null) { return sorted_spots; }

        // we check if the last nap spot is populated
        ContactFilter2D beingFilter = new ContactFilter2D();
        beingFilter.useTriggers = true; // We want to check triggers
        beingFilter.SetLayerMask(LayerMask.GetMask("Beings")); // We only want to check beings layer
        List<Collider2D> results = new List<Collider2D>();
        Physics2D.OverlapCollider(collider, beingFilter, results);

        if (log)
        {
            Debug.Log("(Cat - rcnsip) " + name + " is checking if the last nap spot is populated: " + last_nap_spot.name);
            string s = "Results: " + results.Count + " beings found\n\t";
            foreach (Collider2D c in results)
            {
                s += c.name;
                if (c.transform.parent != null)
                {
                    s += " (" + c.transform.parent.name + ") ";
                }
                s += "\n\t";
            }
            Debug.Log("(Cat - rcnsip) " + s);
        }

        // checks if results is not empty
        if (results.Count <= 0) { return sorted_spots; }

        // we remove the cat from the results
        results.RemoveAll(c => c.transform.parent == transform);

        // if there are still results, we remove the last nap spot from the sorted spots
        if (results.Count > 0)
        {
            // we find the index of the last nap spot in the sorted spots
            int index = sorted_spots.IndexOf(last_nap_spot);
            if (index >= 0)
            {
                // we remove the last nap spot from the sorted spots
                sorted_spots.RemoveAt(index);
            }
        }

        return sorted_spots;
    }
}
