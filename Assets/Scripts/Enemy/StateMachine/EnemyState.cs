using UnityEngine;
using UnityEngine.AI;

public class EnemyState
{
    protected Enemy enemyBase;
    protected EnemyStateMachine stateMachine;
    protected Animator anim;

    protected string animBoolName;
    protected float stateTimer;
    protected bool triggerCalled;

    public EnemyState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName)
    {
        this.enemyBase = enemyBase;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
        this.anim = enemyBase.anim;
    }

    #region State Lifecycle Methods

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
    }

    public virtual void Enter()
    {
        enemyBase.anim.SetBool(animBoolName, true);
        triggerCalled = false;
    }

    public virtual void Exit()
    {
        enemyBase.anim.SetBool(animBoolName, false);
    }
    #endregion

    #region Animation Trigger

    public void AnimationTrigger() => triggerCalled = true;
    #endregion

    #region Pathfinding Utility

    // Get the next point in the NavMesh path
    protected Vector3 GetNextPathPoint()
    {
        NavMeshAgent agent = enemyBase.agent;
        NavMeshPath path = agent.path;

        if (path.corners.Length < 2)
            return agent.destination;

        for (int i = 0; i < path.corners.Length; i++)
        {
            if (Vector3.Distance(agent.transform.position, path.corners[i]) < 1)
                return path.corners[i + 1];
        }
        return agent.destination;
    }
    #endregion

    #region Ability Trigger Logic

    public virtual void AbilityTrigger() { }
    #endregion
}
