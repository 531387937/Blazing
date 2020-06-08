using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    //爆炸相关
    public float exploreRange;
    public float power;
    [HideInInspector]
    private Vector3 target;
    private float distanceToTarget;
    public float g = -10;//重力加速度
    // Use this for initialization
    private Vector3 speed;//初速度向量
    private Vector3 Gravity;//重力向量
    private float dTime = 0;
    private bool fire = false;
    private bool destoryGrid;
    GameObject fx;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (fire)
        {
            Gravity.y = g * (dTime += Time.fixedDeltaTime);//v=at
                                                           //模拟位移
            transform.Translate(speed * Time.fixedDeltaTime);
            transform.Translate(Gravity * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        fx = Resources.Load<GameObject>("FX/Explosion");
        fx = Instantiate(fx, transform.position,fx.transform.rotation);
        if (collision.gameObject.GetComponent<Grid>())
        {
            int index = collision.gameObject.GetComponent<Grid>().index;
            int sqr =(int)Mathf.Sqrt( GameManager.Instance.mapManager.mapGrid.Length);
            if (GameManager.Instance.mapManager.DestoryGrid(index, 0))
                destoryGrid = true;
            if(GameManager.Instance.mapManager.DestoryGrid(index-1, 0))
            destoryGrid = true;
            if(GameManager.Instance.mapManager.DestoryGrid(index+1, 0))
            destoryGrid = true;
            if(GameManager.Instance.mapManager.DestoryGrid(index-sqr, 0))
            destoryGrid = true;
            if(GameManager.Instance.mapManager.DestoryGrid(index+sqr, 0))
            destoryGrid = true;
            if (GameManager.Instance.mapManager.DestoryGrid(index-sqr-1, 0))
            destoryGrid = true;
            if (GameManager.Instance.mapManager.DestoryGrid(index-sqr+1, 0))
            destoryGrid = true;
            if (GameManager.Instance.mapManager.DestoryGrid(index+sqr-1, 0))
            destoryGrid = true;
            if (GameManager.Instance.mapManager.DestoryGrid(index+sqr+1, 0))
            destoryGrid = true;
            //if (GameManager.Instance.mapManager.DestoryGrid(index + sqr + sqr, 0))
            //destoryGrid = true;
            //if (GameManager.Instance.mapManager.DestoryGrid(index - sqr - sqr, 0))
            //destoryGrid = true;
            //if (GameManager.Instance.mapManager.DestoryGrid(index + 2, 0))
            //    destoryGrid = true;
            //if (GameManager.Instance.mapManager.DestoryGrid(index - 2, 0))
            //    destoryGrid = true;
            GameManager.Instance.audioManager.PlaySound("炮击冰块碎裂");
            ContactPoint point = collision.contacts[0];
            Collider[] others = Physics.OverlapSphere(point.point, exploreRange);//获取所有碰撞体
            Rigidbody other;//刚体，通过添加力实现爆炸的视觉效果
            for (int i = 0; i < others.Length; i++)
            {
                //others[i].
                if ((other = others[i].GetComponent<Rigidbody>()) && other.gameObject.name == "APR_Body")
                {//检测刚体
                    other.transform.root.GetComponent<APRController>().ActivateRagdoll(0.5f);
                    other.AddExplosionForce(power, point.point+new Vector3(0,2,0), exploreRange, 10);//这个函数会自动根据距离给刚体衰减的力
                }
            }
                Destroy(gameObject);
        }
        else if(collision.gameObject.tag=="Player")
        {
            var root = collision.transform.root.GetComponent<APRController>().Root;
            ContactPoint point = collision.contacts[0];
            collision.transform.root.GetComponent<APRController>().ActivateRagdoll(0.5f);
            root.GetComponent<Rigidbody>().AddExplosionForce(power, point.point, exploreRange, 10);//这个函数会自动根据距离给刚体衰减的力
        }
    }

    public void Fire(Vector3 target,float time)
    {
        this.target = target-new Vector3(0,0.5f,0);
        speed = new Vector3((target.x - transform.position.x) / time,
                    (target.y - transform.position.y) / time - 0.5f * g * time, (target.z - transform.position.z) / time);
        Gravity = Vector3.zero;//重力初始速度为0
        dTime = 0;
        fire = true;
    }

}
