using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Transition//可以理解为通道
{
    NullTransition = 0,//过渡代表不存在的状态
    SawPlayer,
    LostPlayer
}

public enum StateID//状态ID
{
    NullStateId = 0,
    ChasingPlayer,//配合NPCControl添加两个状态
    FollowingPath
}

/// <summary>
/// 这个类维护一个字典,这个字典中的pair是<Transition,StateID> 表示通道可以到达的状态
/// </summary>
public abstract class FSMState
{
    protected Dictionary<Transition, StateID> map = new Dictionary<Transition, StateID>();
    protected StateID stateId;//当前状态的id

    public StateID ID
    {
        get { return stateId; }
    }

    public void AddTransition(Transition trans, StateID id)
    {
        //验证参数是否合法
        if (trans == Transition.NullTransition)
        {
            Debug.LogError("FSMState ERROR: NullTransition is not allowed for a real transition");
            return;
        }

        if (id == StateID.NullStateId)
        {
            Debug.LogError("FSMState ERROR: NullStateID is not allowed for a real ID");
            return; 
        }

        if (map.ContainsKey(trans))
        {
            Debug.LogError($"FSMState ERROR: State {stateId.ToString()} already has transition. {trans.ToString()} impossible to assign to another state.");
            return;
        }

        map.Add(trans, id);
    }

    public void DeleteTransition(Transition trans)
    {
        //检查空状态
        if (trans == Transition.NullTransition)
        {
            Debug.LogError("FSMState ERROR: NullTransition is not allowed");
            return;
        }

        //删除之前检查是否存在
        if (map.ContainsKey(trans))
        {
            map.Remove(trans);
            return;
        }
        //不在通道的map里面
        Debug.LogError($"FSMState ERROR Transition {trans.ToString()} passed to {stateId.ToString()} was not on the state's transition list");

    }

    public StateID GetOutputStateID(Transition trans)
    {
        if (map.ContainsKey(trans))
            return map[trans];
        return StateID.NullStateId;
    }

    /// <summary>
    /// 这个方法用来设置进入状态前的条件,在分配它到当前状态之前, 它会被状态机类自动调用
    /// </summary>
    public virtual void DoBeforeEntering()
    {

    }
    /// <summary>
    /// 这个方法让一切都有必要的,例如在一个状态转换到另外的一个状态前,需要重置变量, 在状态机切换新状态前自动调用
    /// </summary>
    public virtual void DoBeforeLeaving()
    {

    }

    //动机函数 这个方法用来决定当前状态是否需要转换,
    public abstract void Reason(GameObject player, GameObject npc);

    //行为函数 用来控制npc的行为,npc的所有动作都要放在这个函数里面
    public abstract void Act(GameObject player, GameObject npc);

}

/// <summary>
/// 有限状态机类,它持有npc的状态集合, 并且含有添加/删除状态的方法, 以及改变当前正在执行的状态的方法
/// </summary>
public class FSMSystem
{
    private List<FSMState> states;

    //改变FSMState的唯一办法是performing a transition (通过一个通道) 而不是直接修改当前的状态
    private StateID currentStateID;

    public StateID CurrentStateID//当前的状态ID
    {
        get { return currentStateID; }
    }

    private FSMState currentState;

    public FSMState CurrentState//当前的状态
    {
        get { return currentState; }
    }

    public FSMSystem()
    {
        states = new List<FSMState>();
    }

    public void AddState(FSMState s)
    {
        if (s == null)
            Debug.LogError("FSM ERROR: Null reference is not allowed");
        //count == 0 时的初始状态
        if (states.Count == 0)
        {
            states.Add(s);
            currentState = s;
            currentStateID = s.ID;
            return;
        }

        //如果状态没被添加过,加入集合
        foreach (var state in states)
        {
            if (state.ID == s.ID)
            {
                Debug.LogError($"FSM ERROR：{s.ID} has already been added.");
               return;
            }
        }
        states.Add(s);
    }

    public void DeleteState(StateID id)
    {
        //检查空状态
        if (id == StateID.NullStateId)
        {
            Debug.LogError("FSM ERROR: NullStateID is not allowed for a real state");
            return;
        }

        foreach (var state in states)
        {
            if (state.ID == id)
            {
                states.Remove(state);
                return;
            }
        }

        //输出错误信息
        Debug.LogError($"FSM ERROR impossible to delete state {id.ToString()},it was not on the list of states");
    }

    /// <summary>
    /// 该方法基于当前的状态和transition来尝试去改变FSM的状态,如果当前状态没有一个可以通过transition到达的状态,就会报错
    /// </summary>
    /// <param name="trans"></param>
    public void PerformTransition(Transition trans)
    {
        //检验空状态
        if (trans == Transition.NullTransition)
        {
            //打印错误信息
            Debug.LogError($"FSM ERROR NUllTransition is not allowed for a real transition");
            return;
        }

        //检查当前状态能不能通过transition到达其他状态
        StateID id = currentState.GetOutputStateID(trans);
        if(id == StateID.NullStateId)
        {//表示在currentState的dic中没有trans可以去
            Debug.Log($"FSM ERROR State {currentStateID.ToString()} does not have a target state for transition {trans.ToString()}");
            return;
        }

        //更新currentState和currentStateId
        currentStateID = id;
        foreach(var state in states)
        {
            if(state.ID == currentStateID)//如果在states中找到了我们要转向的状态
            {
                currentState.DoBeforeLeaving();//当前状态更新前的处理

                currentState = state;//更新到该状态

                currentState.DoBeforeEntering();//当前状态进入前的处理

                break;
            }
        }

    }

}


