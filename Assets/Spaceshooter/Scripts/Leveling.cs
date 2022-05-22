using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leveling : MonoBehaviour
{
    public static int getXPRequiredForLevel(int level)
    {
        return (level - 1 * 25 + 100);
    }
}
