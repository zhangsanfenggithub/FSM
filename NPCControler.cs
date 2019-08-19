using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FollowPathState : FSMState
{
    private int currentWayPoint;
    private Transform[] wayPoints;

    public FollowPathState(Transform[] wp)
    {
        wayPoints = wp;
        currentWayPoint = 0;
        stateId = StateID.FollowingPath;//设置自己的状态Id为寻路模式
    }

    public override void DoBeforeEntering()
    {
        Debug.Log("寻路状态进入前");
    }

    public override void DoBeforeLeaving()
    {
        Debug.Log("寻路状态准备离开");
    }


    //重写动机方法
    public override void Reason(GameObject player, GameObject npc)
    {
        RaycastHit hit;
        if (Physics.Raycast(npc.transform.position, npc.transform.forward, out hit, 15f))
        {
            Debug.DrawLine(npc.transform.position,  npc.transform.forward + new Vector3(0,0, 15), Color.red);
            //Debug.DrawLine(npc.transform.position, hit.point, Color.red);
            if (hit.transform.gameObject.tag == "Player")
                npc.GetComponent<NPCControler>().SetTransition(Transition.SawPlayer);//转换当前的状态到发现玩家
        }
    }

    public override void Act(GameObject player, GameObject npc)
    {
        Vector3 velocity = npc.GetComponent<Rigidbody>().velocity;
        Vector3 moveDirection = wayPoints[currentWayPoint].position - npc.transform.position;

        if (moveDirection.magnitude < 1)//路径走完了会进行重置
        {
            currentWayPoint++;
            if (currentWayPoint >= wayPoints.Length)
                currentWayPoint = 0;
        }
        else
        {
            velocity = moveDirection.normalized * 10;
            //朝着路径旋转
            npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(moveDirection), 5 * Time.deltaTime);//旋转平滑
            npc.transform.eulerAngles = new Vector3(0, npc.transform.eulerAngles.y, 0);//更新欧拉角
        }

        npc.GetComponent<Rigidbody>().velocity = velocity;//更新速度
    }
}

public class ChasePlayerState : FSMState
{
    public ChasePlayerState()
    {
        stateId = StateID.ChasingPlayer;
    }
    public override void DoBeforeEntering()
    {
        Debug.Log("追逐状态准备进入");
    }
    public override void DoBeforeLeaving()
    {
        Debug.Log("追逐状态准备离开");
    }

    /// <summary>
    /// 重写动机函数,如果玩家和NPC的距离超过30m转到LostPlayer
    /// </summary>
    /// <param name="player"></param>
    /// <param name="npc"></param>
    public override void Reason(GameObject player, GameObject npc)
    {
        if (Vector3.Distance(npc.transform.position, player.transform.position) >= 10)
             npc.GetComponent<NPCControler>().SetTransition(Transition.LostPlayer);
    }

    /// <summary>
    /// 重写行为函数,顺着路径找到player的方向
    /// </summary>
    /// <param name="player"></param>
    /// <param name="npc"></param>
    public override void Act(GameObject player, GameObject npc)
    {
        Debug.Log("追逐状态的行为函数");
        // 追随寻路点       
        Vector3 vel = npc.GetComponent<Rigidbody>().velocity;
        Vector3 moveDir = player.transform.position - npc.transform.position;
        Debug.Log($"追逐状态的行为函数{moveDir}");
        //缓慢朝向玩家
        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation,
                                                      Quaternion.LookRotation(moveDir),
                                                  5 * Time.deltaTime);
        npc.transform.eulerAngles = new Vector3(0, npc.transform.eulerAngles.y, 0);

        vel = moveDir.normalized * 10;

        npc.GetComponent<Rigidbody>().velocity = vel;
    }

}


[RequireComponent(typeof(Rigidbody))]
public class NPCControler : MonoBehaviour
{
    public GameObject player;
    public Transform[] path;
    private FSMSystem fsm;

    /// <summary>
    /// 该函数用来改变有限状态机的状态,通过transition来过渡到下一个状态,若果没有transition会报错
    /// </summary>
    /// <param name="trans"></param>
    public void SetTransition(Transition trans)
    {
        fsm.PerformTransition(trans);
    }

    /// <summary>
    /// 开始时构造好npc的状态机
    /// </summary>
    void Start()
    {
        MakeFSM();
    }


    void Update()
    {
        Debug.Log(fsm.CurrentState.ID.ToString());
        fsm.CurrentState.Reason(player, gameObject);//根据不同的状态去调用不同的行为函数和动机函数
        fsm.CurrentState.Act(player, gameObject);
    }

    /// <summary>
    /// 构造状态机
    /// </summary>
    private void MakeFSM()
    {
        FollowPathState follow = new FollowPathState(path);
        follow.AddTransition(Transition.SawPlayer, StateID.ChasingPlayer);

        ChasePlayerState chase = new ChasePlayerState();
        chase.AddTransition(Transition.LostPlayer, StateID.FollowingPath);

        fsm = new FSMSystem();
        fsm.AddState(follow);//把跟随状态加入状态机, 第一个状态就是初始状态
        fsm.AddState(chase);//把追逐状态加入状态机
    }
}
