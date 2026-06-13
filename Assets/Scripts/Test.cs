using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public int k;

    [ContextMenu("sav")]
    public int Sav()
    {
        string str = k.ToString();
        while (str.Length > 1)
        {
            int sum = 0;
            foreach (var c in str)
            {
                sum += c - '0';
            }

            str = sum.ToString();
        }

        Debug.Log(str);
        return int.Parse(str);
    }

    [ContextMenu("sav1")]
    public void Sav1()
    {

        Debug.Log(k % 10);
        Debug.Log(k / 10);
        int l = k;
        while (l / 10 > 0)
        {
            int sum = 0;
            int j = l;
            while (j / 10 > 0)
            {
                sum += j % 10;
                j /= 10;
            }

            l = sum;
        }

        Debug.Log(l);
    }
}
