using System;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsStateManager
{
    Dictionary<Type, PhysicsState> instantiatedStates = new Dictionary<Type, PhysicsState>();
    public PhysicsState currentState;
    PlayerObject player;

    public PhysicsStateManager(PlayerObject playerPhysics)
    {
        player = playerPhysics;

        CreateNewStateInstance<GroundedState>();
        CreateNewStateInstance<AirborneState>();

        //Below states not yet created, but will be needed eventually
        // CreateNewStateInstance<HurtState>();
        // CreateNewStateInstance<DeadState>();

        ChangeState<GroundedState>();
    }

    public void ChangeState<T>(bool forceChange = false) where T : PhysicsState, new()
    {
        PhysicsState newState;

        if(!instantiatedStates.TryGetValue(typeof(T), out newState)) //check if state already exists, if not create it and add to dict
        {
            newState = CreateNewStateInstance<T>();
        }
        else if(newState == currentState && !forceChange)
        {
            //we're already in this state, ignore state change.
            return;
        }

        if(newState != null)
        {
            currentState?.Exit();
            newState.Enter();
            currentState = newState;   
        }
    }

    //Creates new instances of states and adds it to dict
    PhysicsState CreateNewStateInstance<T>() where T : PhysicsState, new()
    {
        PhysicsState newState = new T();
        newState.AssignReferences(player, this);
        instantiatedStates.Add(typeof(T), newState);
        return newState;
    }

    public void Update()
    {
        currentState?.Update();
    }
}
