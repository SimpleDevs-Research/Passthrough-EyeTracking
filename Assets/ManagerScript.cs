using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerScript : MonoBehaviour
{
    private List<int> scenes = new List<int> {0};
    private bool loadComplete = true;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        //int random = Random.Range(0, 4);
        //scenes.Add(random);
        //StartCoroutine(loadScene(random));
    }

    void OnEnable()
    {
        int random = Random.Range(1, 5);
        scenes.Add(random);
        StartCoroutine(loadScene(random));
    }

    // Update is called once per frame
    void Update()
    {
        if (loadComplete && Input.GetKeyUp(KeyCode.Space))
        {
            if (scenes.Count == 5)
            {
                scenes = new List<int> {0};
            }

            int random = Random.Range(1, 5);
            while (scenes.Contains(random))
            {
                random = Random.Range(1, 5);
            }
            scenes.Add(random);
            StartCoroutine(loadScene(random));
        }
    }

    IEnumerator loadScene(int index)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index);
        loadComplete = false;
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        loadComplete = true;
    }
}
