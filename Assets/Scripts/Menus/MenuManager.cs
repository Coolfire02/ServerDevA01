using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    //Singleton template
    public static MenuManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] Menu[] menus;

    string nextMenu;
    bool locked;
    string currentMenu;

    public void LockUI()
    {
        locked = true;
    }

    public void UnlockUI()
    {
        locked = false;
        OpenMenu(nextMenu);
    }

    public string getCurrentMenuName()
    {
        return currentMenu;
    }

    public bool isLockedUI()
    {
        return locked;
    }

    public Menu getMenu(string menuName)
    {
        for (int i = 0; i < menus.Length; ++i)
        {
            if (menus[i].menuName == menuName)
                return menus[i];
        }
        return null;
    }

    public void OpenMenu(string menuName)
    {
        Debug.Log("trying to open menu " + menuName);
        Menu toOpen = getMenu(menuName);
        if (toOpen != null)
            OpenMenu(toOpen);
    }

    public void OpenMenu(Menu menu)
    {
        if (!locked)
        {
            Debug.Log("Opening menu " + menu.menuName);
            Menu[] parentMenu = menu.parents;

            for (int i = 0; i < menus.Length; ++i)
            {
                bool isParent = false;
                for (int j = 0; j < parentMenu.Length; ++j)
                {
                    if (parentMenu[j].menuName == menus[i].menuName)
                    {
                        Debug.Log("Opening parent menu " + parentMenu[j].menuName);
                        isParent = true;
                        parentMenu[j].Open();
                        break;
                    }
                }
                if (!isParent)
                    CloseMenu(menus[i]);


            }
            menu.Open();
            currentMenu = menu.menuName;
        }
        else
        {
            nextMenu = menu.menuName;
        }
    }

    public void CloseMenu(string menuName)
    {
        for (int i = 0; i < menus.Length; ++i)
        {
            if (menus[i].menuName == menuName)
                CloseMenu(menus[i]);
        }
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }


}
