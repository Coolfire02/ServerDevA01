//SkillBox.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using PlayFab;
using PlayFab.ClientModels;

[System.Serializable]
public class Skill {
    public string name;
    public int level;
    public Skill(string _name, int _level){
        name=_name;
        level=_level;
    }
}
public class SkillBox : MonoBehaviour
{
    Skill skill;
    [SerializeField] string skillName;
    int level = -1; //Means unloaded

    [SerializeField]TMP_Text tx_skillName;
    [SerializeField]TMP_Text tx_price;
    [SerializeField] TMP_Text tx_level;

    public string getSkillName()
    {
        return skillName;
    }

    public void BuyLevel()
    {
        if(level >= 0)
        {

        }
    }

    public int GetLevel()
    {
        return level;
    }

    public int getSkillCost(int level)
    {
        return 10 + level * 5;
    }

    public void Start()
    {
        skill = new Skill(skillName, -1);
        UpdateSkillBoxUI();
    }
    public void SetSkillStatsFromPrefab(int level)
    {
        this.skill.level = level;
        UpdateSkillBoxUI();
    }

    public void UpdateSkillBoxUI()
    {
        print("Skillbox updated");
        if(level >= 0)
        {
            tx_price.text = "Cost: " + getSkillCost(level + 1).ToString() + " Coins";
            tx_skillName.text = skillName;
            tx_level.text = "Level " + skill.level;
        }else
        {
            tx_skillName.text = "Loading...";
            tx_level.text = "Level -1";
            tx_price.text = "Cost: 0 Coins";
        }
    }



    public Skill ReturnClass(){
        if (skill.level < 0)
            return new Skill(skill.name, 0);
        else return skill;
    }

 
}
