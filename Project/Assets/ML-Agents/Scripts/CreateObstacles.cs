using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateObstacles : MonoBehaviour
{
    public GameObject jumpPrefab;
    public GameObject crawlPrefab;
    public GameObject trapPrefab;
    public int[] row6;
    public int[] row7;
    public int[] row8;
    public int[] row9;

    // Start is called before the first frame update
    void Start()
    {
        row6 = new int[6];
        row7 = new int[6];
        row8 = new int[6];
        row9 = new int[6];

        for (int i = 0; i < 6; i++)
        {
            row6[i] = Random.Range(0, 2);
            row7[i] = Random.Range(0, 2);
            if(row7[i] == 1)
            {
                row7[i] = 2;
            }
            row8[i] = Random.Range(1, 3);
            row9[i] = Random.Range(0, 3);
        }

        // cazul 6 traps
        if(row7[0] == row7[1] &&  row7[0] == row7[2] && row7[0] == row7[3] && row7[0] == row7[4] && row7[0] == row7[5] && row7[0] == 2)
        {
            row7[1] = 0;
            row7[4] = 0;
        }


        if(row8[0] == row8[1] &&  row8[0] == row8[2] && row8[0] == row8[3] && row8[0] == row8[4] && row8[0] == row8[5] && row8[0] == 2)
        {
            row8[1] = 1;
            row8[4] = 1;
        }

        if(row9[0] == row9[1] &&  row9[0] == row9[2] && row9[0] == row9[3] && row9[0] == row9[4] && row9[0] == row9[5] && row9[0] == 2)
        {
            row9[1] = Random.Range(0, 2);
            row9[4] = Random.Range(0, 2);
        }

        for (int i = 0; i < 6; i++)
        {
            // row 1
            Instantiate(jumpPrefab, new Vector3(-10 + i * 5, 0, 15), Quaternion.identity);

            // row 2
            Instantiate(crawlPrefab, new Vector3(-10 + i * 5, 0, 30), Quaternion.identity);

            // row 3 & 5
            if(i == 1 || i == 2 || i == 3 || i == 4)
            {
                Instantiate(trapPrefab, new Vector3(-10 + i * 5, 0.8f, 45), Quaternion.identity);
                Instantiate(trapPrefab, new Vector3(-10 + i * 5, 0.8f, 75), Quaternion.identity);
            }

            // row 4
            if(i == 0 || i == 2 || i == 3 || i == 5)
            {
                Instantiate(trapPrefab, new Vector3(-10 + i * 5, 0.8f, 60), Quaternion.identity);
            }

            //row 6 & 7 & 8 & 9
            InstatntiateWall(row6[i], 90, i);
            InstatntiateWall(row7[i], 105, i);
            InstatntiateWall(row8[i], 120, i);
            InstatntiateWall(row9[i], 135, i);
            
        }
    }

    void InstatntiateWall(int type, int z, int i)
    {
        if(type == 0)
        {
            Instantiate(jumpPrefab, new Vector3(-10 + i * 5, 0, z), Quaternion.identity);
        }
        else if(type == 1)
        {
            Instantiate(crawlPrefab, new Vector3(-10 + i * 5, 0, z), Quaternion.identity);
        }
        else
        {
            Instantiate(trapPrefab, new Vector3(-10 + i * 5, 0.8f, z), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
