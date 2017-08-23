using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wall : MonoBehaviour {
    const float wall_thickness = .5f;
    const float wall_height = 2;
    const bool showLength = false;


    [Header("Wall stats")]
    public Vector2 wall_position;
    public float wall_length;
    public enum Direction
    {
        UP = 0,
        RIGHT = 1,
        DOWN = 2,
        LEFT = 3
    }
    public Direction type = Direction.UP;

    Transform cube;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateWall()
    {
        cube = transform.Find("Cube");
        //works
        /*switch(type)
        {
            case Direction.UP:
                cube.localScale = new Vector3(wall_thickness, wall_height, wall_length);
                cube.localPosition = new Vector3(0, 0, wall_length/2);
                break;
            case Direction.RIGHT:
                cube.localScale = new Vector3(wall_length, wall_height, wall_thickness);
                cube.localPosition = new Vector3(wall_length / 2,  0,0);
                break;
            case Direction.DOWN:
                cube.localScale = new Vector3(wall_thickness, wall_height, wall_length);
                cube.localPosition = new Vector3(0, 0, -wall_length / 2);
                break;
            default:
                cube.localScale = new Vector3(wall_length, wall_height, wall_thickness);
                cube.localPosition = new Vector3(-wall_length / 2, 0, 0);
                break;
        }*/
        //testing on removing weird corner
        switch (type)
        {
            case Direction.UP:
                cube.localScale = new Vector3(wall_thickness, wall_height, wall_length + (wall_thickness));
                cube.localPosition = new Vector3(0, 0, (wall_length / 2));
                break;
            case Direction.RIGHT:
                cube.localScale = new Vector3(wall_length + (wall_thickness), wall_height, wall_thickness);
                cube.localPosition = new Vector3((wall_length / 2), 0, 0);
                break;
            case Direction.DOWN:
                cube.localScale = new Vector3(wall_thickness, wall_height, wall_length + (wall_thickness));
                cube.localPosition = new Vector3(0, 0, -(wall_length / 2));
                break;
            default:
                cube.localScale = new Vector3(wall_length + (wall_thickness), wall_height, wall_thickness);
                cube.localPosition = new Vector3(-(wall_length / 2), 0, 0);
                break;
        }
        transform.localPosition = new Vector3(wall_position.x, 0, wall_position.y);
    }

    public void UpdateScale()
    {
        //dont do anything if no parent (not instantiated in scene)
        if (transform.parent == null)
            return;
        UpdateWall();
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        
        Vector2 updatedPos = new Vector2();
        //find current object then traverse back up through list
        
        for(int i = 0; i < transform.parent.childCount; i++)
        {
            Transform child = transform.parent.GetChild(i);
            if (child.tag == "SEPARATOR")
            {
                updatedPos = new Vector2(child.localPosition.x, child.localPosition.z);
            }
            else
            {
                Wall curWall = child.GetComponent<Wall>();
                if (curWall != null)
                {
                    curWall.wall_position = updatedPos;
                    curWall.UpdateWall();
                    switch (curWall.type)
                    {
                        case Direction.UP:
                            updatedPos += Vector2.up * curWall.wall_length;
                            break;
                        case Direction.RIGHT:
                            updatedPos += Vector2.right * curWall.wall_length;
                            break;
                        case Direction.DOWN:
                            updatedPos += Vector2.down * curWall.wall_length;
                            break;
                        default:
                            updatedPos += Vector2.left * curWall.wall_length;
                            break;
                    }
                }
                else
                {
                    child.name = "ERROR, NEW SEPARATOR";
                    child.tag = "SEPARATOR";
                    i--;
                    Debug.Log("ERROR, MADE NEW SEPERATOR");
                }
            }
        }

    }
}
