
**状态机的图解**

![](https://upload.wikimedia.org/wikipedia/commons/thumb/c/cf/Finite_state_machine_example_with_comments.svg/420px-Finite_state_machine_example_with_comments.svg.png)

**代码数据结构分析**

![](https://github.com/zhangsanfenggithub/img/blob/master/Untitled%20Diagram.png?raw=true)


## 加入新状态

首先需要在**Traision**和**StateId**里面添加相应的通道和状态例如

	ScarePlayer 通道
	Escape 逃跑状态
	

**FSMState**是一个纯虚类,我们通过声明一个新类**EscapeState**,然后继承**FSMState**,重写四个函数:

	class EscapeState : FSMState{
		...//继承的属性和方法
		DoBeforeEntering(), 
		DoBeforeLeaving(), 
		Reason(), 
		Act(),
		...
	}
	
我们在子类的`Reason()`函数中重写状态转换的动机

	public void Reason(GameObject Player, GameObject npc){
		//例如当前npc的血量小于某个值激活这个状态
		npc.GetComponent<NPCControler>().SetTransition(Transition.ScarePlayer);
	}

在`Act()`函数中重写当前状态的行为

	public void Act(...){
		//进行原理玩家的操作
	}

重写进入状态前和离开状态前的函数

	DoBeforeEntering();
	DoBeforeLeaving();
	
最后我们需要在控制Enemy的脚本中, `MakeFSM()`构造状态机函数中,

首先需要在实例化的时候添加相应的通道,

	FollowPathState follow = new FollowPathState(path);
	follow.AddTransition(Transition.ScarePlayer, StateID.Escape);

	ChasePlayerState chase = new ChasePlayerState();
	chase.AddTransition(Transition.ScarePlayer, StateID.Escape);

	//如果逃跑状态有其他的转换也需要进行通道添加
	EscapeState escape = new EscapeState();
	...
	 fsm = new FSMSystem();
    fsm.AddState(follow);//把跟随状态加入状态机, 第一个状态就是初始状态
    fsm.AddState(chase);//把追逐状态加入状态机
	fsm.AddState(escape);//把逃跑状态加入状态机
