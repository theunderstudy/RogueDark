using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{

    public int Range = 2;
    public int Damage = 1;
    public Unit.ActionCallback CB;
    public void UseAbility(Unit target, Unit.ActionCallback abilityComplete)
    {
        target.Health -= Damage;
        CB = abilityComplete;
        Wait(1);
    }






    bool bwaiting = false;
    float waitTime;
    float currentTime = 0;
    private void Update()
    {
        if (bwaiting)
        {
            currentTime += Time.deltaTime;
            if (currentTime > waitTime)
            {
                if (CB != null)
                {
                    bwaiting = false;
                    CB();

                }
            }
        }
    }
    private void Wait(float time)
    {
        bwaiting = true;
        waitTime = time;
        currentTime = 0;
    }

}
