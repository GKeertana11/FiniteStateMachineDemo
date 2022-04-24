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
    public float visualAngle=30f;
    public float shootingDistance=5f;
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
    public bool CanSeePlayer()
    {
        Vector3 direction = player.position - enemy.transform.position;
        float angle = Vector3.Angle(direction, enemy.transform.forward);
        if(direction.magnitude<visualDistance&& angle<visualAngle)
        {
            return true;
        }
        return false;
    }

    public bool EnemyCanAttackPlayer()
    {
        Vector3 direction = player.position - enemy.transform.position;
        if(direction.magnitude<shootingDistance)
        {
            return true;
        }
        return false;
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
        if(CanSeePlayer())
        {
            nextState = new Chase(enemy, agent, player, anim);
            eventState = EVENTS.EXIT;
        }
        if(Random.Range(0,100)<10f)
        {
            nextState = new Patrol(enemy, agent,player,anim);
            eventState = EVENTS.EXIT;
        }
       // base.Update();
    }

    public override void Exit()
    {
        anim.ResetTrigger("isIdle");
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
       // currentIndex = 0;
       base.Enter();
    }
    public override void Update()
    {

        if(CanSeePlayer())
        {
            nextState = new Chase(enemy, agent, player, anim);
            eventState = EVENTS.EXIT;
        }
        if(agent.remainingDistance<1f)
        {
            if(currentIndex>=GameController.Instance.Checkpoints.Count-1)
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
public class Chase : State
{
    
    public Chase(GameObject _enemy, NavMeshAgent _agent, Transform _player, Animator _anim) : base(_enemy, _agent, _player, _anim)
    {
        stateName = STATE.CHASE;
        agent.speed = 2;
        agent.isStopped = false;
    }
    public override void Enter()
    {

        anim.SetTrigger("isRunning");
        // currentIndex = 0;
        base.Enter();
    }
    public override void Update()
    {
        agent.SetDestination(player.position);
        if(agent.hasPath)
        {
            if(EnemyCanAttackPlayer())
            {
                nextState = new Attack(enemy, agent, player, anim);
                eventState = EVENTS.EXIT;
            }
            else if(!CanSeePlayer())
            {
                nextState= new Patrol(enemy, agent, player, anim);
            }
        }
        //  base.Update();
    }

    public override void Exit()
    {
        anim.ResetTrigger("isRunning");
        base.Exit();
    }

}

public class Attack : State
{
    float rotationSpeed = 5f;
    public Attack(GameObject _enemy, NavMeshAgent _agent, Transform _player, Animator _anim) : base(_enemy, _agent, _player, _anim)
    {
        stateName = STATE.ATTACK;

    }
    public override void Enter()
    {

        anim.SetTrigger("isShooting");
        agent.isStopped = true;
        // currentIndex = 0;
        base.Enter();
    }
    public override void Update()
    {
        Vector3 direction = player.position - enemy.transform.position;
        float angle = Vector3.Angle(direction, enemy.transform.forward);
        direction.y = 0;
        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        if(!EnemyCanAttackPlayer())
        {
            nextState = new Idle(enemy, agent, player, anim);
            eventState = EVENTS.EXIT;
        }

        //  base.Update();
    }

    public override void Exit()
    {
        anim.ResetTrigger("isShooting");
        nextState = new Death(enemy, agent, player, anim);
        eventState = EVENTS.EXIT;
        base.Exit();
    }


}
public class Death : State
{
    public Death(GameObject _enemy, NavMeshAgent _agent, Transform _player, Animator _anim) : base(_enemy, _agent, _player, _anim)
    {
        stateName = STATE.DEATH;
    }
    public override void Enter()
    {
        anim.SetTrigger("isSleeping");
        base.Enter();
    }
    public override void Update()
    {
        //Future updates
        // base.Update();
    }

    public override void Exit()
    {
       anim.ResetTrigger("isSleeping");
        base.Exit();
    }
}









