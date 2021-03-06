﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxOrderController : MonoBehaviour
{
    private float timer = 0.0f;
    private float alarm = 200.0f;
    private float seconds = 0.5f;


    public List<BoxController> orders = new List<BoxController>();
    public GameObject scoreManager;
     
    // Start is called before the first frame update

    #region Singleton
    public static BoxOrderController instance;

    void Awake ()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one order form, not good.");
            return;
        }
        instance = this;
    }
    #endregion

    public delegate void OnItemChanged();

    ScoreControllerScript scoring;

    public OnItemChanged onItemChangedCallback;
    private void Start()
    {
        scoring = ScoreControllerScript.instance;
    }
    public void CheckBox(BoxController currentBox)
    {
        
        GameObject.Find("Chute").GetComponent<Animator>().SetTrigger("eatPackage");
        BoxController correctOrder = null;
        bool completed = true;
        foreach (BoxController order in orders)
        {
            completed = true;
            foreach (string key in order.fields)
            {
                if (order.attributes[key] != currentBox.attributes[key]) 
                {
                    completed = false;
                    Debug.Log("Failed this order");
                    break;
                }
                
            }

            if (completed && (order.isFragile == currentBox.isFragile) && (order.isHeavy == currentBox.isHeavy))
            {
                if (currentBox.isSafe == true)
                {
                    correctOrder = order;
                    break;
                }
                else
                {
                    Debug.Log("You passed an unsafe box. Shame on you.");
                    scoring.subtractScore(20);
                    break;
                }
                
            }
        }

        currentBox.GetComponent<BoxController>().onDeath();



        
        if (correctOrder != null)
        {
            Remove(correctOrder);
            Debug.Log("Correct");
            GameObject.Find("Chute").GetComponent<Animator>().SetBool("correct", true);
        } else
        {

            GameObject.Find("Chute").GetComponent<Animator>().SetBool("correct", false);
            scoring.subtractScore(20);
            Debug.Log("Failed");
        }
    }

    private void Remove (BoxController box )
    {
        scoring.addScore(40);
        orders.Remove(box);
        if (onItemChangedCallback != null)
        {
            onItemChangedCallback.Invoke();
        }
    }

    public void Add (BoxController box)
    {
        if (orders.Count < 8)
        {
            orders.Add(box);
            if (onItemChangedCallback != null)
            {
                onItemChangedCallback.Invoke();
            }
        } else
        {
            Debug.Log("Too many objects");
        }
        
    }

    void GenerateOrder()
    {
        int type = Random.Range(0, 3);
        GameObject currentBox;
        switch (type)
        {
            case 1:
                currentBox = ObjectPooler.Instance.SpawnFromPool(Pool.HEAVY_BOX, transform.position, Quaternion.identity);
                currentBox.GetComponent<BoxController>().isHeavy = true;
                break;
            case 2:            
                currentBox = ObjectPooler.Instance.SpawnFromPool(Pool.FRAGILE_BOX, transform.position, Quaternion.identity);
                currentBox.GetComponent<BoxController>().isFragile = true;
                break;
            default:
                currentBox = ObjectPooler.Instance.SpawnFromPool(Pool.NORMAL_BOX, transform.position, Quaternion.identity);
                break;
        }
        BoxController order = currentBox.GetComponent<BoxController>();
        currentBox.SetActive(false);
        order.OnObjectSpawn();
        int redRNG = Random.Range(0, 2);
        int blueRNG = Random.Range(0, 2);
        int whiteRNG = Random.Range(0, 2);
        int bubbleRNG = Random.Range(0, 2);
        if (blueRNG == 0)
        {
            order.attributes[order.fields[0]] = true;
        }
        if (redRNG == 0)
        {
            order.attributes[order.fields[1]] = true;
        }
        if (whiteRNG == 0)
        {
            order.attributes[order.fields[2]] = true;
        }
        if (bubbleRNG == 0)
        {
            order.attributes[order.fields[3]] = true;
        }
        Add(order);
        // Debug.Log("Order Created: " + order);
    }

    private void Update()
    {
        if (timer >= alarm)
        {
            timer = 0.0f;
            GenerateOrder();
        }
        else {
            //Debug.Log("timer = " +timer);
            timer += seconds;
        }
    }

}
