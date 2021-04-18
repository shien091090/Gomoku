//Focus狀態時自動旋轉

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotating : MonoBehaviour
{
    public bool rotating = false; //開始旋轉
    public float speed; //旋轉速度

    void FixedUpdate()
    {
        if(rotating)
        {
            this.transform.Rotate(Vector3.forward * speed * Time.fixedDeltaTime);
        }
    }

    //設定旋轉與否
    public void SetRotating(bool s)
    {
        if(s)
        {
            this.transform.rotation = Quaternion.identity;
            rotating = true;
        }
        else
        {
            this.transform.rotation = Quaternion.identity;
            rotating = false;
        }
    }

}
