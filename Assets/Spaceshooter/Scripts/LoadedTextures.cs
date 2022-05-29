using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadedTextures : MonoBehaviour
{
    public static LoadedTextures instance;
    private void Awake()
    {
        if (instance != null)
            GameObject.Destroy(instance);
        else
            instance = this;
        DontDestroyOnLoad(this);
    }

    public Dictionary<string, Texture> textureList;
    // Start is called before the first frame update
    void Start()
    {
        textureList = new Dictionary<string, Texture>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
