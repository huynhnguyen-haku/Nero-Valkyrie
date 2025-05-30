using UnityEngine;

public class EnemyStateMachine
{
    public EnemyState currentState { get; private set; } 

    #region State Management

    // Set the initial state and enter it
    public void Initialize(EnemyState startState)
    {
        currentState = startState;
        currentState.Enter();
    }

    // Switch to a new state, calling exit/enter as needed
    public void ChangeState(EnemyState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
    #endregion
}
