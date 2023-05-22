using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class EnemyStateMachine : StateMachineBase
{
    public const string Tag = "Enemy";
    public const float LookAroundStopMargin = 0.5f;

    public float LookAroundSpeed = 180f;
    public Transform[] PatrolPoints;
    public Transform EnemyTransform;
    public NavMeshAgent EnemyNavigation;
    public EnemyVisionController EnemyVisionController;

    private int PatrolPointIndex { get; set; }

    protected override void Awake()
    {
        for (int i = 0; i < PatrolPoints.Length; i++)
        {
            var point = PatrolPoints[i];
            Assert.IsNotNull(point, $"index: {i}");
        }
        Assert.IsNotNull(EnemyTransform);
        Assert.IsNotNull(EnemyNavigation);
        Assert.IsNotNull(EnemyVisionController);

        CurrentState = new GoToPatrolPointState(this);
        CurrentState.OnStateStart();
        base.Awake();
    }

    #region States
    private abstract class EnemyStateBase : IState
    {
        protected EnemyStateMachine StateMachine { get; }

        public EnemyStateBase(EnemyStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public virtual void OnStateStart() { }

        public virtual void OnStateFixedUpdate() { }

        public abstract IState Transition();
    }

    private class GoToPatrolPointState : EnemyStateBase
    {
        public GoToPatrolPointState(EnemyStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnStateStart()
        {
            var currentDestination = StateMachine.PatrolPoints[StateMachine.PatrolPointIndex].transform.position;
            StateMachine.EnemyNavigation.destination = currentDestination;
        }

        public override IState Transition()
        {
            if (StateMachine.EnemyVisionController.CanSeePlayer())
                return new ChasePlayerState(StateMachine);

            if (StateMachine.EnemyNavigation.remainingDistance == 0)
                return new ChangePatrolPointState(StateMachine);

            return this;
        }
    }

    private class ChangePatrolPointState : EnemyStateBase
    {
        public ChangePatrolPointState(EnemyStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnStateStart()
        {
            StateMachine.PatrolPointIndex++;
            if (StateMachine.PatrolPointIndex >= StateMachine.PatrolPoints.Length)
                StateMachine.PatrolPointIndex = 0;
        }

        public override IState Transition()
        {
            return new GoToPatrolPointState(StateMachine);
        }
    }

    private class ChasePlayerState : EnemyStateBase
    {
        public ChasePlayerState(EnemyStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnStateFixedUpdate()
        {
            if (StateMachine.EnemyVisionController.CanSeePlayer())
                StateMachine.EnemyNavigation.destination = StateMachine.EnemyVisionController.Player.transform.position;
        }

        public override IState Transition()
        {
            if (StateMachine.EnemyNavigation.remainingDistance == 0)
                return new LookAroundState(StateMachine);

            return this;
        }
    }

    private class LookAroundState : EnemyStateBase
    {
        private float _initAngle;

        public LookAroundState(EnemyStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnStateStart()
        {
            _initAngle = StateMachine.EnemyTransform.eulerAngles.y;
        }

        public override void OnStateFixedUpdate()
        {
            var angleMovement = StateMachine.LookAroundSpeed * Time.deltaTime;
            StateMachine.EnemyTransform.Rotate(0f, angleMovement, 0f);
        }

        public override IState Transition()
        {
            if (Mathf.Abs(_initAngle - StateMachine.EnemyTransform.eulerAngles.y) <= EnemyStateMachine.LookAroundStopMargin)
                return new GoToPatrolPointState(StateMachine);

            if (StateMachine.EnemyVisionController.CanSeePlayer())
                return new ChasePlayerState(StateMachine);

            return this;
        }
    }

    #endregion
}
