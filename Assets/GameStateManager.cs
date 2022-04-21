using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameStateManager : MonoBehaviour
{
    NavMeshAgent agent;
    Animator anim;
    public Transform player;
   public  State currentState;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        currentState = new Idle(this.gameObject, agent, player, anim);
    }

    // Update is called once per frame
    void Update()
    {
        currentState = currentState.Process();
    }
}

public class State
{
    public enum STATE { IDLE, ATTACK, PATROL, CHASE, DEATH };
    public STATE stateName;
    public enum EVENTS { ENTER, UPDATE, EXIT };
    public EVENTS eventState;
    public GameObject enemy;
    public Transform player;
    public State nextState;
    public float visualDistance = 10f;
    public float visualAngle;
    public float shootingDistance;
    public Animator anim;
    public NavMeshAgent agent;
    public State(GameObject _enemy, NavMeshAgent _agent, Transform _player, Animator _anim)
    {
        this.enemy = _enemy;
        this.player = _player;
        this.anim = _anim;
        this.agent = _agent;
        eventState = EVENTS.ENTER;


    }
    public virtual void Enter()
    {
        eventState = EVENTS.UPDATE;
    }
    public virtual void Update()
    {
        eventState = EVENTS.UPDATE;
    }
    public virtual void Exit()
    {
        eventState = EVENTS.EXIT;
    }
    public  State Process()
    {
        if (eventState == EVENTS.ENTER)
        {
            Enter();
        }
        if (eventState == EVENTS.UPDATE)
        {
            Update();
        }
        if (eventState == EVENTS.EXIT)
        {
            Exit();
            return nextState;
        }
        return this;
    }

}

public class Idle : State {
    public Idle(GameObject _enemy, NavMeshAgent _agent, Transform _player, Animator _anim):base( _enemy, _agent,  _player, _anim)
    {
        stateName = STATE.IDLE;
    }
    public override void Enter()
    {
        anim.SetTrigger("isIdle");
        base.Enter();
    }
    public override void Update()
    {
        if(Random.Range(0,100)<10f)
        {
            nextState = new Patrol(enemy, agent,player,anim);
            eventState = EVENTS.EXIT;
        }
       // base.Update();
    }

    public override void Exit()
    {
        anim.ResetTrigger("isWalking");
        base.Exit();
    }



}

public class Patrol: State
{
    int currentIndex = -1;
    public Patrol(GameObject _enemy, NavMeshAgent _agent, Transform _player, Animator _anim) : base(_enemy, _agent, _player, _anim)
    {
        stateName = STATE.PATROL;
        agent.speed = 2;
        agent.isStopped = false;
    }
    public override void Enter()
    {
     
        anim.SetTrigger("isWalking");
        currentIndex = 0;
       base.Enter();
    }
    public override void Update()
    {
        if(agent.remainingDistance<1f)
        {
            if(currentIndex>=GameController.Instance.Checkpoints.Count)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex++;
            }
            agent.SetDestination(GameController.Instance.Checkpoints[currentIndex].transform.position);
        }
      //  base.Update();
    }

    public override void Exit()
    {
        anim.ResetTrigger("isWalking");
        base.Exit();
    }

}



