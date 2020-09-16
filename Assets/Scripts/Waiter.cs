﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waiter : MonoBehaviour
{
    private EmployeeBehaviour employeeBehaviour;
    private Workstation workstation;
    private List<Transform> destinations;
    private int bringDirtyPlatesIndex;
    private Transform waitingZone;
    [SerializeField] private List<float> waitTimesBringFood;
    [SerializeField] private List<float> waitTimesBringDirtyPlates;
    private Order currentOrder;
    [SerializeField] private float movementSpeed = 1f;
    private int currentDestIndex;
    private bool shouldWait;
    private float timer;
    private bool hasGrabbedDirtyPlates;
    private int drawResourceIndex;
    private int task;
    private bool hasDrawnResource;
    private Table currentTable;

    void Start()
    {
        employeeBehaviour = GetComponent<EmployeeBehaviour>();
        waitingZone = employeeBehaviour.ParentZone.GetWaitingZone();
    }

    void Update()
    {
        if (employeeBehaviour.ShouldBeginTask(out Workstation station, out Order order))
        {
            BringFood(station, order);
        }
        if (employeeBehaviour.IsBusy)
        {
            if (task == 1)
            {
                MoveToNextPoint1();
            }
            if (task == 2)
            {
                MoveToNextPoint2();
            }
        }
        else
        {
            MoveToWaitPoint();
        }
    }

    private void MoveToWaitPoint()
    {
        if (!(Vector3.Distance(waitingZone.position, transform.position) < 0.1f))
        {
            transform.position = Vector3.MoveTowards(transform.position, waitingZone.position, movementSpeed * Time.deltaTime);
        }
    }

    private void MoveToNextPoint1()
    {
        if (Vector3.Distance(destinations[currentDestIndex].position, transform.position) < 0.1f)
        {
            shouldWait = true;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, destinations[currentDestIndex].position, movementSpeed * Time.deltaTime);
        }
        if (shouldWait)
        {
            timer += Time.deltaTime;
            if (timer >= waitTimesBringFood[currentDestIndex])
            {
                shouldWait = false;
                timer = 0;
                currentDestIndex++;

                if (!hasDrawnResource && currentDestIndex == drawResourceIndex)
                {
                    hasDrawnResource = true;
                    OnDrawRessource();
                }
            }
        }
        if (currentDestIndex >= destinations.Count)
        {
            currentDestIndex = 0;
            employeeBehaviour.IsBusy = false;
            OnLeavingFoodPlate();
        }
    }



    private void MoveToNextPoint2()
    {
        if (Vector3.Distance(destinations[currentDestIndex].position, transform.position) < 0.1f)
        {
            shouldWait = true;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, destinations[currentDestIndex].position, movementSpeed * Time.deltaTime);
        }
        if (shouldWait)
        {
            timer += Time.deltaTime;
            if (timer >= waitTimesBringDirtyPlates[currentDestIndex])
            {
                shouldWait = false;
                timer = 0;
                currentDestIndex++;
                if (!hasGrabbedDirtyPlates && currentDestIndex == bringDirtyPlatesIndex)
                {
                    hasGrabbedDirtyPlates = true;
                    OnDirtyPlateGrab();
                }
            }
        }
        if (currentDestIndex >= destinations.Count)
        {
            employeeBehaviour.IsBusy = false;
            employeeBehaviour.TaskAccomplished();
            currentDestIndex = 0;
        }
    }
    private void OnDrawRessource()
    {
        employeeBehaviour.DrawResource();
    }
    private void OnDirtyPlateGrab()
    {
        currentOrder.Customer.PayAndLeave();
        currentTable.InUse = false;
    }

    private void OnLeavingFoodPlate()
    {
        currentOrder.Customer.StartEating();
    }

    private void BringFood(Workstation station, Order order)
    {
        task = 1;
        hasDrawnResource = false;
        currentTable = order.Table;
        destinations = new List<Transform>
        {
            employeeBehaviour.ParentZone.GetInputPos(),
            currentTable.GetWaiterZone()
        };
        drawResourceIndex = 1;
        if (waitTimesBringFood.Count != destinations.Count)
        {
            Debug.LogError("Different size for waitTimesList and Path list for waiter. Fill the waitTimesList onthe inspector properly");
        }
        currentOrder = order;
    }

    public void BringDirtyPlates(Order order)
    {
        task = 2;
        employeeBehaviour.IsBusy = true;
        hasGrabbedDirtyPlates = false;
        currentTable = order.Table;
        destinations = new List<Transform>
        {
            currentTable.GetWaiterZone(),
            employeeBehaviour.ParentZone.GetOutputPos(),
        };
        bringDirtyPlatesIndex = 1;
        if (waitTimesBringDirtyPlates.Count != destinations.Count)
        {
            Debug.LogError("Different size for waitTimesList and Path list for waiter. Fill the waitTimesList onthe inspector properly");
        }
        currentOrder = order;
    }
}
