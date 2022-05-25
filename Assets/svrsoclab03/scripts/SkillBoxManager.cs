using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class SkillBoxManager:MonoBehaviour
{
    List<SkillBox> SkillBoxes;

    [SerializeField]
    LobbyStats lobbyStats;

    private void Start()
    {
        SkillBoxes = new List<SkillBox>();
        foreach (Transform trans in gameObject.transform)
        {
            if (trans.gameObject.GetComponent<SkillBox>()) SkillBoxes.Add(trans.gameObject.GetComponent<SkillBox>());
        }
    }

    public void SendJSON(){
        List<Skill> skillList=new List<Skill>();
        foreach(var item in SkillBoxes) skillList.Add(item.ReturnClass());
        string stringListAsJson = JsonUtility.ToJson(new JSListWrapper<Skill>(skillList));
        Debug.Log("JSON data prepared:"+stringListAsJson);
        var req=new UpdateUserDataRequest{
            Data=new Dictionary<string, string>{ //package as dictionary item
                {"Skills",stringListAsJson}
            }
        };
        PlayFabClientAPI.UpdateUserData(req,result =>Debug.Log("Data sent success!"),OnError);
    }
    public void LoadJSON(){
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),OnJSONDataReceived,OnError); 
    }
    void OnJSONDataReceived(GetUserDataResult r){
        Debug.Log("received JSON data");
        if(r.Data!=null&&r.Data.ContainsKey("Skills")){
            Debug.Log(r.Data["Skills"].Value);
            JSListWrapper<Skill> jlw=JsonUtility.FromJson<JSListWrapper<Skill>>(r.Data["Skills"].Value);
            for(int i = 0; i < jlw.list.Count; ++i)
            {
                
                foreach(SkillBox sb in SkillBoxes)
                {
                    if(sb.getSkillName().Equals(jlw.list[i].name))
                    {
                        sb.SetSkillStatsFromPrefab(jlw.list[i].level);
                    }
                }
                foreach(SkillBox sb in SkillBoxes)
                {
                    if(sb.GetLevel() == -1) // Not loaded
                    {
                        sb.SetSkillStatsFromPrefab(0);
                    }
                }
            }
        }
    }
    void OnError(PlayFabError e) //report any errors here!
    {
        Debug.Log("Error" + e.GenerateErrorReport());
    }
    public void BacktoMainScene(){
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}

[System.Serializable]
public class JSListWrapper<T> //this is just class to have a list inside
{
    public List<T> list;
    public JSListWrapper(List<T> list) => this.list = list;
}


