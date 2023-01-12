using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Driver : MonoBehaviour
{
    int min = 0;
    int max = 255;
    float moveSpeed = 0.05f;

    public Transform mainCamera;
    public GameObject driftEffect;
    public Transform driftMount;

    int steerLeft;
    int steerRight;
    int acceleration;
    int deceleration;

    bool pressedLeft = false;
    bool pressedRight = false;
    bool pressedUp = false;
    bool pressedDown = false;

    bool onDrift = false;

    bool collided = false;

    int health = 100;
    public Text healthText;
    int counter = 0;
    public Text counterText;

    public Text mainText;
    public Image left;
    public Image right;
    public Image up;
    public Image down;

    // Start is called before the first frame update
    void Start()
    {

    }
    void Update()
    {
        mainCamera.position = new Vector3(transform.position.x, mainCamera.position.y, transform.position.z);
        //Input holders
        //Left steer Q
        if (Input.GetKeyDown(KeyCode.Q) && pressedLeft == false)
        {
            pressedLeft = true;
            //Only one can be pressed
            pressedRight = false;
        }
        else if (Input.GetKeyDown(KeyCode.Q) && pressedLeft == true) pressedLeft = false;


        //Right steer E
        if (Input.GetKeyDown(KeyCode.E) && pressedRight == false)
        {
            pressedRight = true;
            //Only one can be pressed
            pressedLeft = false;
        }
        else if (Input.GetKeyDown(KeyCode.E) && pressedRight == true) pressedRight = false;

        if (Input.GetKeyDown(KeyCode.W) && pressedUp == false)
        {
            pressedUp = true;
            //Only one can be pressed
            pressedDown = false;
        }
        else if (Input.GetKeyDown(KeyCode.W) && pressedUp == true) pressedUp = false;

        if (Input.GetKeyDown(KeyCode.D) && pressedDown == false)
        {
            //If pressed D with some acceleration threshold apply drift
            //Also must be left or right steers
            if (acceleration > max / 2 && !onDrift)
            {
                StartCoroutine(Drift());
                driftEffect.GetComponent<ParticleSystem>().Play();
            }

            pressedDown = true;
            //Only one can be pressed
            pressedUp = false;
        }
        else if (Input.GetKeyDown(KeyCode.D) && pressedDown == true) pressedDown = false;


        //Colored icons
        if (pressedLeft) left.color = Color.white;
        else left.color = Color.gray;
        if (pressedRight) right.color = Color.white;
        else right.color = Color.gray;
        if (pressedUp) up.color = Color.white;
        else up.color = Color.gray;
        if (pressedDown) down.color = Color.white;
        else down.color = Color.gray;

        counterText.text = counter.ToString();
        healthText.text = health.ToString();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        //https://www.youtube.com/watch?v=L1X7YRyzFwE - AE86 gameplay
        //If only one steer/acceleration can be accumulated, other will decrease
        //One tap on D key apply drift 
        //Steers is angle to move

        //Give stacks
        if (pressedLeft)
        {
            if (steerLeft < max) steerLeft++;
        }
        else if (steerLeft > min) steerLeft--;

        if (pressedRight)
        {
            if (steerRight < max) steerRight++;
        }
        else if (steerRight > min) steerRight--;

        if (pressedUp)
        {
            if (acceleration < max && deceleration == min) acceleration++;
        }
        else if (acceleration > min) acceleration--;

        if (pressedDown)
        {
            if (deceleration < max && acceleration == min) deceleration++;
        }
        else if (deceleration > min) deceleration--;





        //Move forward
        if (acceleration > 0 && !onDrift && !collided) transform.Translate(Vector3.forward * (acceleration * moveSpeed) * Time.deltaTime);
        //Move backwards
        if (deceleration > 0 && !onDrift && !collided) transform.Translate(Vector3.back * (deceleration * moveSpeed) * Time.deltaTime);
        //Rotate left
        if (steerLeft > 0 && !onDrift) transform.Rotate(Vector3.down * steerLeft * Time.deltaTime);
        //Rotate right
        if (steerRight > 0 && !onDrift) transform.Rotate(Vector3.up * steerRight * Time.deltaTime);


        //Drift feature
        if (onDrift)
        {
            //Now it only increase speed and slows rotation
            transform.Translate((Vector3.forward * (acceleration * moveSpeed * 1f)) * Time.deltaTime);
            //Make steer movement
            transform.Translate(Vector3.right * (steerLeft - steerRight) * moveSpeed * 1.5f * Time.deltaTime);
            transform.Rotate(Vector3.down * (steerLeft - steerRight) * 0.5f * Time.deltaTime);
        }



        //Show stacks
        mainText.text = steerLeft.ToString() + " " + acceleration.ToString() + " " + steerRight.ToString() + " " + deceleration.ToString();
    }
    public IEnumerator Drift()
    {
        // X Y Z, change only X and Z
        Debug.Log("Drift");
        onDrift = true;

        //GameObject go = Instantiate(driftEffect, driftMount.position, driftMount.rotation);
        //go.transform.parent = driftMount;
        //driftEffect.GetComponent<ParticleSystem>().Play();

        yield return new WaitForSeconds(2.5f);
        onDrift = false;
        driftEffect.GetComponent<ParticleSystem>().Stop();
        //Destroy(go);


        //StartCoroutine(moveObject(transform.position, transform.position + Vector3.left * (acceleration * moveSpeed), transform.rotation));


    }
    //public IEnumerator moveObject(Vector3 Origin, Vector3 Destination, Quaternion Rotation)
    //{
    //    float totalMovementTime = 3f; //the amount of time you want the movement to take
    //    float currentMovementTime = 0f;//The amount of time that has passed
    //    while (Vector3.Distance(transform.localPosition, Destination) > 0)
    //    {
    //        currentMovementTime += Time.deltaTime;
    //        transform.localPosition = Vector3.Lerp(Origin, Destination, currentMovementTime / totalMovementTime);
    //        transform.rotation = Rotation;
    //        yield return null;
    //    }
    //}
    private void OnTriggerEnter(Collider other)
    {
        collided = true;
        if (other.gameObject.tag == "Enemy" && onDrift)
            counter++;
        if (health <= 0) Application.Quit();
        if (other.gameObject.tag == "Obstacle")
            health-=5;
    }
    private void OnTriggerStay(Collider other)
    {
        transform.Translate(Vector3.left * Time.fixedDeltaTime);
    }
    private void OnTriggerExit(Collider other)
    {
        collided = false;
        if (other.gameObject.tag == "Enemy" && onDrift)
        {
            StartCoroutine(Explosion(other.gameObject));
        }
    }
    public IEnumerator Explosion(GameObject go)
    {
        go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        go.GetComponentInChildren<ParticleSystem>().Play();
        yield return new WaitForSeconds(5f);
        go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }
}
