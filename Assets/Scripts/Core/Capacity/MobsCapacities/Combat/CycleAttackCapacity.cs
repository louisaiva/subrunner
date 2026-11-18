using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleAttackCapacity : Capacity
{
	
	[Header("GetStatic Only cycle animations"), Tooltip("This is get static only. Requires a template save in order to apply changes")]
	[SerializeField] private List<string> attack_cycle = new List<string>();


	///
	//
	/// USE CAPA
	//
	///


	// USE
	public override void Use(Capable user)
	{
		// ensure we have an attacker
		if (!Capable.TryGetCapacity(out AttackCapacity attacker) || !attacker.Able) { return; }

		// select the next anim in the cycle
		string next_anim = get_next_attack_animation();

		// make the attacker use it
		attacker.SetAttackCapacityAnimation(next_anim);
		attacker.Use(user);
	}

	private int current_cycle_index = 0;
	private string get_next_attack_animation()
	{
		current_cycle_index++;
		if (current_cycle_index >= ((CycleAttackData)data).animations_cycle.Count)
		{
			current_cycle_index = 0;
		}

		return ((CycleAttackData)data).animations_cycle[current_cycle_index];
	}





	///
	//
	/// DATA MANAGEMENT
	//
	///

	// GET STATIC DATA
	public override CapacityData GetStaticData()
	{
		CycleAttackData static_data = new CycleAttackData(base.GetStaticData())
		{
			animations_cycle = new List<string>(attack_cycle)
		};
		return static_data;
	}
}
[Serializable] public class CycleAttackData : CapacityData
{
	public List<string> animations_cycle = new List<string>();

	// CONSTRUCTOR
	public CycleAttackData(CapacityData parent) : base(parent) { }

	// DUPLICATE
	public override ICapacityData Duplicate()
	{
		return new CycleAttackData(base.Duplicate() as CapacityData)
		{
			animations_cycle = new List<string>(animations_cycle)
		};
	}

	// GET DETAILS
	public override string GetDetails()
	{
		string details = "";
		if (animations_cycle == null) { details += " - no animations cycle"; }
		else { details += $" - animations cycle : {string.Join(" -> ", animations_cycle)}\n"; }
		return base.GetDetails() + details;
	}
}