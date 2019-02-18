﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class playerController : MonoBehaviour {

    float hitPoints = 100f;
    [SerializeField] HealthBar playerHealth;

    [SerializeField] float aimSpeed = 4f;
    Rigidbody2D rBody;
    bool isBoostCharging = false;
    float boostTimer = 0;
    float MAX_BOOST_TIME = 2.0f;

    enum PlayerNum
    {
        P1,
        P2,
        P3,
        P4
    }

    [SerializeField] PlayerNum playerNum;
    private float horizontalDeadZone = 0.3f;
    private float verticalDeadZone = 0.3f;

    [SerializeField] float moveSpeed = 40f;
    [SerializeField] float evadeSpeed = 400f;
    [SerializeField] bool hasBoosted = false;
    [SerializeField] GameObject r, l, u, d, ru, lu, rd, ld;
    [SerializeField] GameObject colliderObj2Listen;
    GameObject objPicked;
    [SerializeField] private float throwForce = 0f;

    [SerializeField] Animator myAnim;

    bool isCarrying = false;
    [SerializeField] GameObject cranePoint;

    [SerializeField] Transform up, down, right, left;
    [SerializeField] GameObject throwRange;
    [SerializeField] GameObject targetReticle;
    [SerializeField] Transform upLarger, rightLarger;


    float collisionDamageMultiplier = 1f;

    bool isJumping = false;
    float jumpTimer = 0;
    [SerializeField] float jumpTime = 3f;

    Vector2 movementVector = Vector2.zero;

    float drag;
    public bool isOiled = false;
    bool isCoroutineRunning = false;
    int oilCount = 1;
    [SerializeField] float oilForce = 1.0f;
    [SerializeField] float oilForceTime = 0.5f;
    float oilTimer = 0.0f;

    [SerializeField] float oilSpeed = 1.7f;
    private int oilDirectionModifier = 1;



    [SerializeField] GameObject craneActual;
    [SerializeField] Sprite craneL, craneLU, craneU, craneRU, craneR, craneRD, craneD, craneLD;

    public GameObject gameManager;

    private Rigidbody2D m_rb;

    [SerializeField] float maxThrowForce = 5000f;
    [SerializeField] float maxThrowTime = 3.0f;
    [SerializeField] float minChargeTime = 1.0f;
    [SerializeField] float minThrowForce = 1650f;
    [SerializeField]float timeCharging;

    [SerializeField] private float evadeCooldown = 1f;
    private bool hasEvaded = false;
    private float evadeCooldownTimer = 0;
    [SerializeField] private float jumpCooldown = 1f;
    private bool hasJumped = false;
    private float jumpCooldownTimer = 0;
    [SerializeField] private float throwCooldown = 0.5f;
    private float throwCooldownTimer = 0;
    private bool hasThrown = false;
    private bool isCharging = false;
    [SerializeField] private float isDestroyedFreq = 1.0f; //How often the game checks if the object the crane was holding is destroyed
    private float isDestroyedTimer = 0.0f;

    public GameObject m_arrow;
    private Vector3 m_arrowOriginalScale;

    private moveBack MoveBackScript;
    private float m_stunTime;


    Vector2 smellForceUp = new Vector2(0.0f, 0.08f);
    Vector2 smellForceDown = new Vector2(0.0f, -0.08f);
    float smellRadius = 3.5f;
    public bool isSmelly;

    //[SerializeField] ContactFilter2D tempFilter = new ContactFilter2D();
    // Use this for initialization
    void Start() {
        rBody = GetComponent<Rigidbody2D>();

        switch (playerNum)
        {
            case PlayerNum.P1:
                this.gameObject.layer = 12;
                break;
            case PlayerNum.P2:
                this.gameObject.layer = 13;
                break;
            case PlayerNum.P3:
                this.gameObject.layer = 14;
                break;
            case PlayerNum.P4:
                this.gameObject.layer = 15;
                break;
            default:
                break;
        }


        m_rb = GetComponent<Rigidbody2D>();
        targetReticle.GetComponent<SpriteRenderer>().enabled = false;
        throwRange.GetComponent<SpriteRenderer>().enabled = false;
        m_arrowOriginalScale = m_arrow.transform.localScale;
        // MoveBackScript = GetComponent<moveBack>();
        //isSmelly = true;


    }

    // Update is called once per frame
    void Update() {
        //REGENERATION
        //if (regenCounter > 0)
        //{
        //    regenCounter -= Time.deltaTime;
        //}
        //else
        //{
        //    getHealth(Time.deltaTime * regenAmount);
        //}


        //Debug.Log(getOwnAxis("Horizontal"));


        cooldownTimers();
        updateMovementVec();

        //if (true)
        //{
        //    carAI[] cars = GameObject.FindObjectsOfType<carAI>();

        //    foreach (carAI car in cars)
        //    {
        //        if (true) //range
        //        {
        //            // add force
        //        }
        //    }
        //}
        if (!isBoostCharging)
        {
            if (!hasEvaded && (getOwnAxis("Trigger") < -0.2 || getOwnButtonDown("B")))
            {
                isBoostCharging = true;

            }
        }
        else // Boost is charging
        {
            boostTimer += Time.deltaTime;

            //if (boostTimer >= MAX_BOOST_TIME || (getOwnAxis("Trigger") > -0.2 && getOwnButtonUp("B")))
            if (getOwnAxis("Trigger") > -0.2 && !getOwnButton("B"))
            {
                boost();
            }
        }
        
        if (isCarrying)
        {
            cranePoint.GetComponent<SpriteRenderer>().enabled = true;
           // targetReticle.GetComponent<SpriteRenderer>().enabled = true;
           // throwRange.GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            cranePoint.GetComponent<SpriteRenderer>().enabled = false;
            targetReticle.GetComponent<SpriteRenderer>().enabled = false;
            throwRange.GetComponent<SpriteRenderer>().enabled = false;
        }


        if(isOiled == false)
        {
            //movement();
        }
        else
        {
            Collider2D[] temp = new Collider2D[30];
            ContactFilter2D tempFilter = new ContactFilter2D();
            tempFilter.useTriggers = true;
            //int i = GetComponent<BoxCollider2D>().OverlapCollider(tempFilter, temp);
            //temp = Physics2D.OverlapBoxAll(this.transform.position, GetComponent<BoxCollider2D>().size, 0f);
            int numColliders = GetComponent<PolygonCollider2D>().OverlapCollider(tempFilter, temp);
            bool isStillOiled = false;
            for (int i = 0; i < numColliders; i++)
            {
                if (temp[i].gameObject.GetComponent<oil>())
                {
                    isStillOiled = true;
                }
            }
            if (isStillOiled == false)
            {
                getUnOiled();
            }

        }

        //Debug.Log(Input.GetAxis("P1Horizontal2"));
        //Debug.Log(Input.GetAxis("P1Vertical2"));

        // Assign which collider we'll use for picking up w crane
        if (!isCarrying)
        {
            checkColliders();
        }
        else
        {
            //float x = right.transform.position.x;
            //float y = up.transform.position.y;
            //x -= this.transform.position.x;
            //y -= this.transform.position.y;

            //x *= getOwnAxis("Horizontal2");
            //y *= -getOwnAxis("Vertical2");

            //Vector2 pos = this.transform.position + new Vector3(x, y, 0);
            //cranePoint.transform.position = pos;


            aim();

        }

        //Debug.Log(getOwnAxis("Trigger"));
        if (!isJumping && isCarrying && getOwnAxis("Trigger") < 0.25f && !hasThrown)    // Charge the attack?
        {
            timeCharging += Time.deltaTime;
            isCharging = true;
            if (m_arrow.transform.localScale.y < 0.3 && timeCharging > minChargeTime)
                m_arrow.transform.localScale += new Vector3(0f, 0.01f, 0);
        }
        else if (!isJumping && isCarrying && isCharging && !hasThrown)
        {
            chargingForce();
            throwObj();
        }
        else if (isCarrying == false && !hasJumped && !isJumping && getOwnButtonDown("A"))
            //getOwnAxis("Trigger") > 0.25f)
        {
            isJumping = true;
            myAnim.SetBool("IsJumping", true);
            GetComponent<BoxCollider2D>().enabled = false;
            //GetComponent<SpriteRenderer>().enabled = false;
            jumpTimer = 0;
        }
        else if (isJumping)
        {
            jumpTimer += Time.deltaTime;

            if (jumpTimer >= jumpTime)
            {
                isJumping = false;
                hasJumped = true;
                myAnim.SetBool("IsJumping", false);
                GetComponent<BoxCollider2D>().enabled = true;
                //GetComponent<SpriteRenderer>().enabled = true;
                //takeDamage(jumpDamage);
            }
        }


    }
    private void FixedUpdate()
    {

        movement();


        //if (m_stunTime <= 0.0f)
        //{
        //    //MoveBackScript.enabled = false;
        //    //movement();
        //}
        //else
        //{
        //    m_stunTime -= Time.deltaTime;
        //}
        //if (isOiled == false)
        //{
        //    movement();
        //}
        //else
        //{
        //    oilTimer += Time.deltaTime;
        //    if (oilTimer >= oilForceTime)
        //    {
        //        oiledMovement();
        //        oilTimer = 0.0f;
        //    }
        //}


        if (isSmelly)
        {
            carAI[] cars = GameObject.FindObjectsOfType<carAI>();

            foreach (carAI car in cars)
            {
                //if (GetDistanceFromClosest(GameObject.FindGameObjectsWithTag("AICar")) <= smellRadius)
                if(Vector2.Distance(car.transform.position, this.transform.position) <= smellRadius)
                {
                    //Checks if car AI is in front of player
                    if (car.transform.position.x > this.transform.position.x)
                    {
                        //Transforms if AICar position.y is above player
                        if (car.transform.position.y > this.transform.position.y)
                        {
                            car.GetComponent<Transform>().Translate(smellForceUp);
                            Debug.Log("CAR NAME: " + car.name + smellForceUp + "moving up: " + car.transform.position);
                        }

                        //Transforms if AICar position.y is below player
                        else if (car.transform.position.y < this.transform.position.y)
                        {
                            car.GetComponent<Transform>().Translate(smellForceDown);
                            Debug.Log("CAR NAME: " + car.name + smellForceDown + "moving down: " + car.transform.position);
                        }
                    }

                    //Checks if AICar position.x is behind the player
                    else if (car.transform.position.x < this.transform.position.x)
                    {
                        //Transforms if AICar position.y is above player
                        if (car.transform.position.y > this.transform.position.y)
                        {
                            car.GetComponent<Transform>().Translate(smellForceUp);
                            Debug.Log("CAR NAME: " + car.name + smellForceUp + "moving up: " + car.transform.position);
                        }

                        //Transforms if AICar position.y is below player
                        else if (car.transform.position.y < this.transform.position.y)
                        {
                            car.GetComponent<Transform>().Translate(smellForceDown);
                            Debug.Log("CAR NAME: " + car.name + smellForceDown + "moving down: " + car.transform.position);
                        }

                    }
                }
            }
        }
    }

    float GetDistanceFromClosest(GameObject[] gameObjects)
    {
        float shortestDistance = Mathf.Infinity;
        foreach (GameObject go in gameObjects)
        {
            shortestDistance = Mathf.Min(shortestDistance, Vector2.Distance(transform.position, go.transform.position));
        }
        return shortestDistance;
    }

    void becomeSmelly()
    {
        isSmelly = true;
    }

    void updateMovementVec()
    {
        movementVector = new Vector2(getOwnAxis("Horizontal"), getOwnAxis("Vertical"));
    }

    void boost()
    {
        float force = (evadeSpeed / 3.0f) + ((evadeSpeed * 2.0f * (boostTimer / MAX_BOOST_TIME)) / 3.0f);
        GetComponent<Rigidbody2D>().AddForce(movementVector.normalized * force, ForceMode2D.Impulse);
        hasEvaded = true;
        isBoostCharging = false;
        boostTimer = 0;

    }

    Vector2 getRightStickDir()    // returns a normalized vector from right stick.
    {
        Vector3 dir = new Vector3(0, 0, 0);

        if (getOwnAxis("Horizontal2") > horizontalDeadZone)
        {
            //GetComponent<Rigidbody2D>().AddForce(new Vector2(moveSpeed, 0));
            dir.x = getOwnAxis("Horizontal2");
        }
        else if (getOwnAxis("Horizontal2") < -horizontalDeadZone)
        {
            dir.x = getOwnAxis("Horizontal2");
        }

        if (getOwnAxis("Vertical2") > verticalDeadZone)
        {
            dir.y = -getOwnAxis("Vertical2");
        }
        else if (getOwnAxis("Vertical2") < verticalDeadZone)
        {
            dir.y = -getOwnAxis("Vertical2");
        }

        if (Vector3.Magnitude(dir) > 1)
        {
            dir.Normalize();
        }

        return dir;
    }


    void aim()
    {

        m_arrow.SetActive(true);

        Vector2 dir = getRightStickDir();


        //float localX = right.transform.position.x - this.transform.position.x;
        //float localY = up.transform.position.y - this.transform.position.y;

        //Vector2 temp = new Vector2(localX * dir.x, dir.y * localY);
        //Vector3 pos = new Vector3(-temp.x, -temp.y, 0) + this.transform.position;
        //cranePoint.transform.position = pos;

        targetReticle.transform.Translate(dir * Time.deltaTime * aimSpeed);

        Vector3 temp = targetReticle.transform.position - this.transform.position;
        temp.Normalize();

        cranePoint.transform.position = this.transform.position - temp;

        Vector2 craneAngle = cranePoint.transform.position - this.transform.position;
        //MOVE CRANE ACCORDING TO CRANE POINT //
        float angle = findDegree(craneAngle.x, craneAngle.y);
        //Debug.Log(angle);

        m_arrow.transform.localEulerAngles = new Vector3(0,0,-angle - 180);

        if (angle < 22.5f)
        {
            craneActual.GetComponent<SpriteRenderer>().sprite = craneU;
        }
        else if (angle < 67.5f)
        {
            craneActual.GetComponent<SpriteRenderer>().sprite = craneRU;
        }
        else if (angle < 112.5f)
        {
            craneActual.GetComponent<SpriteRenderer>().sprite = craneR;
        }
        else if (angle < 157.5f)
        {
            craneActual.GetComponent<SpriteRenderer>().sprite = craneRD;
        }
        else if (angle < 202.5f)
        {
            craneActual.GetComponent<SpriteRenderer>().sprite = craneD;
        }
        else if (angle < 247.5f)
        {
            craneActual.GetComponent<SpriteRenderer>().sprite = craneLD;
        }
        else if (angle < 292.5f)
        {
            craneActual.GetComponent<SpriteRenderer>().sprite = craneL;
        }
        else if (angle < 337.5f)
        {
            craneActual.GetComponent<SpriteRenderer>().sprite = craneLU;
        }
        else
        {
            craneActual.GetComponent<SpriteRenderer>().sprite = craneU;
        }





        ///////////////////////////////////////////////
        while (isInsideElipse(this.transform.position.x, this.transform.position.y, targetReticle.transform.position.x, targetReticle.transform.position.y,
            rightLarger.position.x - this.transform.position.x, upLarger.position.y - this.transform.position.y) == false)
        {
            targetReticle.transform.position = (0.99f * (targetReticle.transform.position - this.transform.position)) + this.transform.position;
        }



        //Debug.Log("Is inside eclipse? " + tem);
    }

    float findDegree(float x, float y)
    {
        float value = (float)((Mathf.Atan2(x, y) * 180f) / Mathf.PI);
        if (value < 0)
        {
            value += 360f;
        }
        return value;
    }

    public int getPlayerNum()
    {
        switch (playerNum)
        {
            case PlayerNum.P1:
                return 1;
            case PlayerNum.P2:
                return 2;
            case PlayerNum.P3:
                return 3;
            case PlayerNum.P4:
                return 4;
            default:
                break;
        }
        return 0;
    }

    //void oldAim()
    //{
    //    Vector2 circularPos = new Vector2(getOwnAxis("Horizontal2"), -getOwnAxis("Vertical2"));
    //    if (circularPos.magnitude > 1)
    //        circularPos.Normalize();

    //    float localX = right.transform.position.x - this.transform.position.x;
    //    float localY = up.transform.position.y - this.transform.position.y;

    //    Vector2 temp = new Vector2(localX * circularPos.x, circularPos.y * localY);
    //    Vector3 pos = new Vector3(-temp.x, -temp.y, 0) + this.transform.position;
    //    cranePoint.transform.position = pos;




    //   // float localX2 = right2.transform.position.x - this.transform.position.x;
    //   // float localY2 = up2.transform.position.y - this.transform.position.y;

    //   // Vector2 temp2 = new Vector2(localX2 * circularPos.x, circularPos.y * localY2);
    //   // Vector3 pos2 = new Vector3(temp2.x, temp2.y, 0) + this.transform.position;
    //  //  targetReticle.transform.position = pos2;
    //}

    void checkColliders()
    {
        if (getOwnAxis("Horizontal2") < -0.34f)
        {
            if (getOwnAxis("Vertical2") < -0.34f)
            {
                colliderObj2Listen = lu;
                craneActual.GetComponent<SpriteRenderer>().sprite = craneLU;
            }
            else if (getOwnAxis("Vertical2") > 0.34f)
            {
                colliderObj2Listen = ld;
                craneActual.GetComponent<SpriteRenderer>().sprite = craneLD;
            }
            else
            {
                colliderObj2Listen = l;
                craneActual.GetComponent<SpriteRenderer>().sprite = craneL;
            }
            if(!hasThrown)
                pickUp();
        }
        else if (getOwnAxis("Horizontal2") > 0.34f)
        {
            if (getOwnAxis("Vertical2") < -0.34f)
            {
                colliderObj2Listen = ru;
                craneActual.GetComponent<SpriteRenderer>().sprite = craneRU;
            }
            else if (getOwnAxis("Vertical2") > 0.34f)
            {
                colliderObj2Listen = rd;
                craneActual.GetComponent<SpriteRenderer>().sprite = craneRD;
            }
            else
            {
                colliderObj2Listen = r;
                craneActual.GetComponent<SpriteRenderer>().sprite = craneR;
            }
            if (!hasThrown)
                pickUp();
        }
        else
        {
            if (getOwnAxis("Vertical2") < -0.34f)
            {
                colliderObj2Listen = u;
                craneActual.GetComponent<SpriteRenderer>().sprite = craneU;
                if (!hasThrown)
                    pickUp();
            }
            else if (getOwnAxis("Vertical2") > 0.34f)
            {
                colliderObj2Listen = d;
                craneActual.GetComponent<SpriteRenderer>().sprite = craneD;
                if (!hasThrown)
                    pickUp();
            }
            else
            {
                //colliderObj2Listen = null;
                colliderObj2Listen = d;
                craneActual.GetComponent<SpriteRenderer>().sprite = craneD;
            }
        }
        
    }

    void movement()
    {
        //float modifiedSpeed = moveSpeed;
        //if (isOiled)
        //{
        //    modifiedSpeed *= oilSpeed * oilDirectionModifier;
        //        //* (hitPoints /100f);
        //}
        //else
        //{
        //    modifiedSpeed *= (hitPoints / 100f);
        //}

        // MOVEMENT
        //if (getOwnAxis("Horizontal") > horizontalDeadZone)
        //{
        //    GetComponent<Rigidbody2D>().AddForce(new Vector2(moveSpeed, 0));
        //   // Debug.Log(GetComponent<Rigidbody2D>().velocity);
        //}
        //else if (getOwnAxis("Horizontal") < -horizontalDeadZone)
        //{
        //    GetComponent<Rigidbody2D>().AddForce(new Vector2(-moveSpeed, 0));
        //}

        //if (getOwnAxis("Vertical") > horizontalDeadZone)
        //{
        //    GetComponent<Rigidbody2D>().AddForce(new Vector2(0, -moveSpeed));
        //}
        //else if (getOwnAxis("Vertical") < -horizontalDeadZone)
        //{
        //    GetComponent<Rigidbody2D>().AddForce(new Vector2(0, moveSpeed));
        //}

        rBody.AddForce(movementVector * moveSpeed);
        Debug.Log("move");

        //if (getOwnAxis("RBumper") > 0 && !hasEvaded)
        //{
        //    GetComponent<Rigidbody2D>().AddForce(new Vector2(0, -evadeSpeed), ForceMode2D.Impulse);
        //    hasEvaded = true;
            
        //}
        //else if (getOwnAxis("LBumper") > 0 && !hasEvaded)
        //{
        //    GetComponent<Rigidbody2D>().AddForce(new Vector2(0, evadeSpeed), ForceMode2D.Impulse);
        //    hasEvaded = true;
        //}
        //else if (getOwnAxis("Trigger") < 0 && !hasEvaded)
        //{
            
        //}

        /* added diagonal up and down controls (in progress)
           if (getOwnAxis("RBumper") > 0 && !hasEvaded )
        {
            if (getOwnAxis("Horizontal") < 0.8)
            {
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0, -evadeSpeed), ForceMode2D.Impulse); //d
            }
            if (getOwnAxis("Horizontal") > 0 && getOwnAxis("Vertical") > 0)
            {
                GetComponent<Rigidbody2D>().AddForce(new Vector2(evadeSpeed / 2, -evadeSpeed / 2), ForceMode2D.Impulse); //dr
            }
            else if (getOwnAxis("Horizontal") > 0 && getOwnAxis("Vertical") < 0)
            {
                GetComponent<Rigidbody2D>().AddForce(new Vector2(evadeSpeed / 2, evadeSpeed / 2), ForceMode2D.Impulse); //ur
            }

            hasEvaded = true;
        }
     
         */

        //if (hitPoints < maximumHitPoints)
        //{
        //    this.transform.Translate(new Vector3(-1, 0, 0) * 0.04f * ((100f-hitPoints) / 100f));
        //}
    }

    //Coroutine deals with the movement of a player when they are oiled up


    //void oiledMovement()
    //{
    //    if (oilCount % 2 == 0)
    //    {
    //        GetComponent<Rigidbody2D>().AddForce(new Vector2(0, moveSpeed * oilForce *(hitPoints / 100f)));
    //    }
    //    else
    //    {
    //        GetComponent<Rigidbody2D>().AddForce(new Vector2(0, -moveSpeed *oilForce* (hitPoints / 100f)));
    //    }
    //    oilCount++;
    //    oilForce += 0.1f;
    //}

    float getOwnAxis(string axis)
    {
        return Input.GetAxis(playerNum.ToString() + axis);
    }

    bool getOwnButtonDown(string i)
    {
        return Input.GetButtonDown(playerNum.ToString() + i);
    }

    bool getOwnButtonUp(string i)
    {
        return Input.GetButtonUp(playerNum.ToString() + i);
    }

    bool getOwnButton(string i)
    {
        return Input.GetButton(playerNum.ToString() + i);
    }

    void pickUp()   // Gimme your best pick up lines, programmer intern. > "I hope we can merge without any conflicts ( ͡° ͜ʖ ͡°) " > Not bad playa -- E
    {
        if (colliderObj2Listen != null && colliderObj2Listen.GetComponent<CraneZone>().isTherePickable())
        {
            Debug.Log("attempting to pick up");

            //Checks if the current player can pick up the object
            //if (colliderObj2Listen.GetComponent<CraneZone>().getObj2PickUp().GetComponent<throwable>().canIPickup((int)playerNum))
           // {
                objPicked = colliderObj2Listen.GetComponent<CraneZone>().getObj2PickUp();


                objPicked.GetComponent<Rigidbody2D>().velocity.Set(0, 0);
                isCarrying = true;
                cranePoint.transform.position = objPicked.transform.position;
                objPicked.GetComponent<Rigidbody2D>().isKinematic = true;

                if (objPicked.GetComponent<CircleCollider2D>())
                {
                    objPicked.GetComponent<CircleCollider2D>().enabled = false;
                }

                if (objPicked.GetComponent<BoxCollider2D>())
                {
                    objPicked.GetComponent<BoxCollider2D>().enabled = false;
                }

                if (objPicked.GetComponent<moveBack>())
                {
                    objPicked.GetComponent<moveBack>().enabled = false;
                }


                objPicked.transform.parent = cranePoint.transform;
                // ADD MORE LATER
           // }



        }

    }

    void chargingForce()
    {
        isCharging = false;
        if (timeCharging > maxThrowTime)
            throwForce = maxThrowForce;
        else if (timeCharging < minChargeTime)
            throwForce = minThrowForce;
        else
            throwForce = (timeCharging / maxThrowTime) * maxThrowForce;
        timeCharging = 0f;
        m_arrow.transform.localScale = m_arrowOriginalScale;
    }

    void throwObj()
    {
        if (objPicked != null)
        {
            float distance = Vector3.Distance(cranePoint.transform.position, targetReticle.transform.position);

            objPicked.GetComponent<Rigidbody2D>().isKinematic = false;
            objPicked.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            objPicked.transform.parent = null;

            Vector2 dir = targetReticle.transform.position - cranePoint.transform.position;
            dir.Normalize();

            switch (playerNum)
            {
                case PlayerNum.P1:
                    objPicked.GetComponent<throwable>().setLayer(1);
                    break;
                case PlayerNum.P2:
                    objPicked.GetComponent<throwable>().setLayer(2);
                    break;
                case PlayerNum.P3:
                    objPicked.GetComponent<throwable>().setLayer(3);
                    break;
                case PlayerNum.P4:
                    objPicked.GetComponent<throwable>().setLayer(4);
                    break;
                default:
                    break;
            }

            if (objPicked.GetComponent<CircleCollider2D>())
            {
                objPicked.GetComponent<CircleCollider2D>().enabled = true;
            }
            if (objPicked.GetComponent<BoxCollider2D>())
            {
                objPicked.GetComponent<BoxCollider2D>().enabled = true;
            }

            if (objPicked.GetComponent<moveBack>())
            {
                objPicked.GetComponent<moveBack>().enabled = true;
            }

            //objPicked.GetComponent<throwable>().setDistance(distance);
            objPicked.GetComponent<Rigidbody2D>().AddForce(dir * throwForce);


            objPicked = null;
            isCarrying = false;
            hasThrown = true;
            m_arrow.SetActive(false);
        }
    }

    //Used if object gets destoryed while in possession of crane, resets crane to default state
    void resetCrane()
    {
        if (objPicked == null)
        {
            isCarrying = false;
            hasThrown = true;
            m_arrow.SetActive(false);
        }
    }


    bool isInsideElipse(float centreX, float centreY, float posX, float posY, float horDis, float verDis)
    {
        float p = (Mathf.Pow((posX - centreX), 2) / Mathf.Pow(horDis, 2)) + (Mathf.Pow(posY - centreY, 2) / Mathf.Pow(verDis, 2));

        //Debug.Log(p);
        if (p > 0.98f)
        {
            return false;
        }

        return true;
    }
    //void oldthrowObj(){
    //    if(objPicked != null)
    //    {
    //        float distance = Vector3.Distance(cranePoint.transform.position, targetReticle.transform.position);


    //        objPicked.GetComponent<Rigidbody2D>().isKinematic = false;
    //        objPicked.transform.parent = null;
            

    //        Vector2 dir = new Vector2(getOwnAxis("Horizontal2"), -getOwnAxis("Vertical2"));
    //        dir.Normalize();
    //        //objPicked.GetComponent<throwable>().setPos(targetReticle.transform.position);

    //        switch (playerNum)
    //        {
    //            case PlayerNum.P1:
    //                objPicked.GetComponent<throwable>().setLayer(1);
    //                break;
    //            case PlayerNum.P2:
    //                objPicked.GetComponent<throwable>().setLayer(1);
    //                break;
    //            default:
    //                break;
    //        }

    //        if (objPicked.GetComponent<CircleCollider2D>())
    //        {
    //            objPicked.GetComponent<CircleCollider2D>().enabled = true;
    //        }

    //        objPicked.GetComponent<throwable>().setDistance(distance);
    //        objPicked.GetComponent<Rigidbody2D>().AddForce(dir * throwForce);
            

    //        objPicked = null;
    //        isCarrying = false;
    //    }
    //}

    public void takeDamage(float amount)
    {
        //stun(amount * 0.1f);
        hitPoints -= amount;

        //if(amount >0)   //just in case?
        //regenCounter = regenDelay;

        //if (hitPoints < 0)
        //{
        //    hitPoints = 0;
        //}

        //playerHealth.updateHealthBar(hitPoints);

        if (hitPoints <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void getHealth(float amount)
    {
        //hitPoints += amount;

        //if (hitPoints > maximumHitPoints)
        //{
        //    hitPoints = maximumHitPoints;
        //}

        //playerHealth.updateHealthBar(hitPoints);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            //takeDamage(collision.relativeVelocity.magnitude * collisionDamageMultiplier);

        }
        else if (collision.transform.GetComponent<destructible>())
        {
            Debug.Log("destructible collided");
            collision.transform.GetComponent<destructible>().getDestroyed();
            takeDamage(collision.transform.GetComponent<destructible>().getDmgAmount());
        }

    }

    //Called when the player hits an oil spill. Sets drag to 0 while storing original drag value.
    public void getOiled()
    {
        if(!isOiled)
        //if (GetComponent<Rigidbody2D>().drag != 0)
        {
            drag = GetComponent<Rigidbody2D>().drag;

        }
        GetComponent<Rigidbody2D>().drag = 0;

        // DOESNT WORK THIS WAY> background speed fucks things up
        //Vector2 v = rBody.velocity;
        //v *= 10;
        //rBody.velocity.Set(v.x, v.y);

        isOiled = true;
        oilDirectionModifier = -1;
        Debug.Log("got oiled!");
    }
    //Called when the player leaves an oil spill. Sets drag to original value.
    public void getUnOiled()
    {
        GetComponent<Rigidbody2D>().drag = drag;
        isOiled = false;

        
        //if (oilCount % 2 == 0)
        //{
        //    GetComponent<Rigidbody2D>().AddForce(new Vector2(0, moveSpeed * oilForce / 2));
        //        //* (hitPoints / 100f)));
        //}
        //else
        //{
        //    GetComponent<Rigidbody2D>().AddForce(new Vector2(0, -moveSpeed * oilForce / 2));
        //        //* (hitPoints / 100f)));
        //}
        //oilCount = 1;
        //oilForce = 1.0f;
        //oilDirectionModifier = 1;
        Debug.Log("got unoiled!");
    }

    public void stun(float addedStunTime)
    {
        m_stunTime += addedStunTime;
        //MoveBackScript.enabled = true;
    }


    void cooldownTimers()
    {
        if (hasJumped)
        {
            jumpCooldownTimer += Time.deltaTime;
            if (jumpCooldownTimer >= jumpCooldown)
            {
                hasJumped = false;
                jumpCooldownTimer = 0;
            }
        }

        if (hasEvaded)
        {
            evadeCooldownTimer += Time.deltaTime;
            if (evadeCooldownTimer >= evadeCooldown)
            {
                hasEvaded = false;
                evadeCooldownTimer = 0;
            }
        }

        if (hasThrown)
        {
            throwCooldownTimer += Time.deltaTime;
            if (throwCooldownTimer >= throwCooldown)
            {
                hasThrown = false;
                throwCooldownTimer = 0;
            }
        }

        if (isCarrying)
        {
            isDestroyedTimer += Time.deltaTime;
            if (isDestroyedTimer >= isDestroyedFreq)
            {
                resetCrane();
                isDestroyedTimer = 0;
            }
        }

    }

}
