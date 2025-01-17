using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdelManger : MonoBehaviour
{

    public static IdelManger Instance { get; private set; }

    private float sugarAutoUpgradeAmout;
    private float flourAutoUpgradeAmout;
    private float eggsAutoUpgradeAmout;
    private float butterAutoUpgradeAmout;
    private float chocolateAutoUpgradeAmout;
    private float milkAutoUpgradeAmout;

    private float sugarPowerAmout;
    private float flourPowerAmout;
    private float eggsPowerAmout;
    private float butterPowerAmout;
    private float chocolatePowerAmout;
    private float milkPowerAmout;

    private User user;


    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Debug.Log("Destroying duplicate WebAPI instance.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUserRec();
    }

    private void UpdateUserRec()
    {
        user.sugar += sugarAutoUpgradeAmout;
        user.flour += flourAutoUpgradeAmout;
        user.eggs += eggsAutoUpgradeAmout;
        user.butter += butterAutoUpgradeAmout;
        user.chocolate += chocolateAutoUpgradeAmout;
        user.milk += milkAutoUpgradeAmout;
    }
}
