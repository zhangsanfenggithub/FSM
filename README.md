
## 状态机原理

![](https://upload.wikimedia.org/wikipedia/commons/thumb/c/cf/Finite_state_machine_example_with_comments.svg/420px-Finite_state_machine_example_with_comments.svg.png)

## 代码数据结构分析

`FSMState`类表示当前的状态，使用字典来维护通道-状态的集合，枚举`StateID`表示当前的状态

![](https://github.com/zhangsanfenggithub/img/blob/master/1.jpg?raw=true)

`currentState`表示状态机现在正在执行的状态，`List<FSMState>`用与管理图中存在的所有状态

![](https://github.com/zhangsanfenggithub/img/blob/master/2.jpg?raw=true)

>通道之间的转换及转换的行为

![](https://github.com/zhangsanfenggithub/img/blob/master/Untitled%20Diagram.png?raw=true)


## 加入新状态

首先需要在**Traision**和**StateId**里面添加相应的通道和状态例如

```
	ScarePlayer 通道
	Escape 逃跑状态
```

`FSMState`是一个纯虚类,我们通过声明一个新类如`EscapeState`,然后继承`FSMState`,同时需要重写四个方法:
```
class EscapeState : FSMState{
	...//继承的属性和方法
	DoBeforeEntering(), 
	DoBeforeLeaving(), 
	Reason(), 
	Act(),
	...
}
```
其中
1. 我们在子类的`Reason()`函数中重写状态转换的动机，当某个条件达成时，我们调用SetTransition函数进行切换
```
public void Reason(GameObject Player, GameObject npc){
	if(触发某个条件)
	    npc.GetComponent<NPCControler>().SetTransition(Transition.ScarePlayer);
}
```
2. 在`Act()`函数中重写当前状态的行为，只要脚本挂载的对象没有进行状态转换，会一直执行Act()
```
public void Act(...){
	//此处添加对象的行为
}
```
3. 重写进入状态前和离开状态前的方法，并在其中添加相应的行为，该方法可以为空。
```
DoBeforeEntering();
DoBeforeLeaving();
```	
## 构造状态机
`MakeFSM()`为构造状态机的方法，我们需要在控制`Enemy`的脚本中（状态机负责控制的脚本）对该方法进行相应的更新

1. 首先需要给某个状态到新状态添加新的通道，比如我们下面给跟随状态和追逐状态添加了`当前状态`到`Escape`状态的通道
```
FollowPathState follow = new FollowPathState(path);
follow.AddTransition(Transition.ScarePlayer, StateID.Escape);
ChasePlayerState chase = new ChasePlayerState();
chase.AddTransition(Transition.ScarePlayer, StateID.Escape);
```
2. 紧接着我们需要声明新的状态实例
```
//如果逃跑状态有其他的转换也需要进行通道添加
EscapeState escape = new EscapeState();
```
3. 声明fsm并在其中实例化
```
fsm = new FSMSystem();
```
4. 把我们需要状态机管理的的状态使用`AddState()`加入
```
fsm.AddState(follow);//把跟随状态加入状态机, 第一个状态就是初始状态
fsm.AddState(chase);//把追逐状态加入状态机
fsm.AddState(escape);//把逃跑状态加入状态机
```
5. 我们在Start()函数中调用配置好的`MakeFSM()`
```
void Start()
{
    MakeFSM();
}
```
6. 在`Update()`方法中不断执行`Act()`及`Reason()`函数
```
void Update()
{
    Debug.Log(fsm.CurrentState.ID.ToString());
    fsm.CurrentState.Reason(player, gameObject);//根据不同的状态去调用不同的行为函数和动机函数
    fsm.CurrentState.Act(player, gameObject);
}
```
