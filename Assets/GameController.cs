using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public sealed class GameController
{
    private static GameController instance;
    public static GameController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameController();
                instance.Checkpoints.AddRange(GameObject.FindGameObjectsWithTag("Checkpoint"));
                instance.checkPoints = instance.checkPoints.OrderBy(x => x.name).ToList();
            }
            return instance;
        }
    }
    private List<GameObject> checkPoints = new List<GameObject>(); 
    public List<GameObject> Checkpoints { get { return checkPoints; } }
}

   
    


        // Start is called before the first frame update

    
