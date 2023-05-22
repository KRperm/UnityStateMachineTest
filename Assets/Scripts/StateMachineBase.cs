using UnityEngine;
using UnityEngine.Assertions;

public abstract class StateMachineBase : MonoBehaviour
{
    public IState CurrentState { get; protected set; }

    protected virtual void Awake()
    {
        Assert.IsNotNull(CurrentState);
    }

    private void FixedUpdate()
    {
        CurrentState.OnStateFixedUpdate();
        var newState = CurrentState.Transition();
        
        if (newState == CurrentState)
            return;

        newState.OnStateStart();
        CurrentState = newState;
    }
}

public interface IState
{
    void OnStateStart();

    void OnStateFixedUpdate();

    IState Transition();
}