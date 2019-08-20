using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleFollowing : MonoBehaviour
{
    public Path path;

    public float speed = 20.0f;

    public float mass = 5.0f;

    public bool isLooping = true;

    //
    private float currSpeed;

    private int currPathIndex;

    private float pathLength;

    private Vector3 targetPoint;

    private Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        pathLength = path.Length;
        currPathIndex = 0;
        //得到当前速度, 以车的正方向为基准
        velocity = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        //计算speed
        currSpeed = speed * Time.deltaTime;

        targetPoint = path.GetPoint(currPathIndex);

        //到达目标
        if (Vector3.Distance(transform.position, targetPoint) < path.Radius)
        {
            //没有到达最后一个目标点
            if (currPathIndex < pathLength - 1) currPathIndex++;
            else if (isLooping) currPathIndex = 0; //到了最后, 重置index
            else return;               
        }


        //根据加速度计算速度
        if (currPathIndex >= pathLength - 1 && !isLooping)
        {
            velocity += Steer(targetPoint, true);
        }
        else
        {
            velocity += Steer(targetPoint);
        }

        //
        transform.position += velocity;
        transform.rotation = Quaternion.LookRotation(velocity);

    }

    //把正方向旋转到目标方向
    public Vector3 Steer(Vector3 target, bool bFinalPoint = false)
    {
        //期望速度方向
        Vector3 desiredVelocity = target - transform.position;
        //计算和目标的距离
        float dis = desiredVelocity.magnitude;
        //单位化方向
        desiredVelocity.Normalize();
        //根据speed计算velocity
        if (bFinalPoint && dis < 10.0f)
            desiredVelocity *= (currSpeed * (dis / 10.0f));//距离小于10的时候开始减速
        else
            desiredVelocity *= currSpeed;
        //计算力的Vector
        Vector3 steeringForce = desiredVelocity - velocity;
        Vector3 acceleration = steeringForce / mass;
        return acceleration;
    }
}
