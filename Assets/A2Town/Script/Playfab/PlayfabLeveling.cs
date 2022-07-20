using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfabLeveling : MonoBehaviour
{
    public static int getXPRequiredForLevel(int level)
    {
        return (level - 1 * 25 + 100);
    }
}
