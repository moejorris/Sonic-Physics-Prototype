using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class Sonic1_TitleCard_Generator : MonoBehaviour
{
    //TODO: Animate This Like the OG
    public string sceneName = "LV_Green Hill_1";
    string curSceneName = "";
    string zoneName = "";
    int actNum = 1;
    [SerializeField] Sprite[] alphabetSprites = new Sprite[26];
    [SerializeField] Sprite[] actNumSprites = new Sprite[3];

    [SerializeField] Image actNumImg;
    [SerializeField] Image actTxtImg;

    [SerializeField] Transform zoneNameParent;
    [SerializeField] RectTransform zoneLayoutTransform;


    bool isRunning;

    void Start()
    {
        UpdateTitleCard(false);
    }

    void Update()
    {
        if(isRunning) return;

        if(isEditing() && !isRunning)
        {
            isRunning = true;
            UpdateTitleCard(true);
        }
        isRunning = false;
    }

    bool isEditing()
    {
        #if UNITY_EDITOR
            if(!Application.isPlaying)
            {
                return true;
            }
        #endif
        return false;
    }

    void UpdateTitleCard(bool isEditor = false)
    {
        if(!zoneNameParent || sceneName == "" || sceneName == " " || !actNumImg || !enabled)
        {
            if(!isEditor)
            {
                gameObject.SetActive(false);
            }
            return;
        }


        if(!isEditor)
        {
            sceneName = SceneManager.GetActiveScene().name;
        }

        if(curSceneName != sceneName)
        {
            isRunning = true;

            UpdateStrings();
            DestroyOldLetters(isEditor);
            SpawnLetters();
            UpdatePosition();

            if(actNum != -1)
            {
                actNumImg.sprite = actNumSprites[actNum];
            }

            isRunning = false;
        }
    }

    void UpdatePosition()
    {
        RectTransform rect = GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }

    void UpdateStrings()
    {
        curSceneName = sceneName;
        actNum = 0;
        curSceneName = curSceneName.Replace(" Zone", "", System.StringComparison.OrdinalIgnoreCase).Trim();

        string[] parts = curSceneName.Split('_');

        foreach(string part in parts)
        {
            if (!int.TryParse(part, out actNum) && actNum == 0)
            {
                actNum = -1;
            }
        }


        if(actNum != -1)
        {
            actNum--;

            actNum = Mathf.Clamp(actNum, 0, 2); //can be a max of 3 acts
        }

        actNumImg.gameObject.SetActive(actNum != -1);
        actTxtImg.gameObject.SetActive(actNum != -1);
        
        zoneLayoutTransform.localPosition = new Vector2 (actNum != -1 ? -16 : -8, zoneLayoutTransform.localPosition.y);


        if(parts[0] == "LV")
        {
            zoneName = string.Join(" ", parts, 1, parts.Length - 2);
        }
        else
        {
            zoneName = parts[0];
        }
    }

    void DestroyOldLetters(bool isEditor = false)
    {
        for(int i = zoneNameParent.transform.childCount - 1; i >= 1; i--)
        {
            if(zoneNameParent.GetChild(i).name != "Info")
            {
                if(isEditor)
                {
                    DestroyImmediate(zoneNameParent.GetChild(i).gameObject);
                }
                else
                {
                    Destroy(zoneNameParent.GetChild(i).gameObject);                                    
                }
            }
            else continue;
        }
    }

    void SpawnLetters()
    {
        zoneName = zoneName.ToLower();

        for(int i = 0; i < zoneName.Length; i++)
        {
            int letterNum = zoneName[i] - 'a';

            if((letterNum >= 0 && letterNum < alphabetSprites.Length) || zoneName[i] == ' ')
            {
                GameObject letterGO = new GameObject("letter");
                letterGO.transform.parent = zoneNameParent;
                Image img = letterGO.AddComponent<Image>();
                letterGO.transform.localScale = Vector3.one;

                if(zoneName[i] == ' ')
                {
                    img.color = new Color(0f,0f,0f,0f);
                    letterGO.GetComponent<RectTransform>().sizeDelta = Vector3.one * 16f;

                }
                else
                {
                    img.color = Color.white;
                    img.sprite = alphabetSprites[letterNum];
                    letterGO.GetComponent<RectTransform>().sizeDelta = img.sprite.bounds.size * img.sprite.pixelsPerUnit;
                }
            }


        }
    }
}
