using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Msg;
    void UpdateMsg(string msg)
    {
        Debug.Log(msg);
        Msg.text += msg + '\n';
    }

    void OnError(PlayFabError e)
    {
        UpdateMsg(e.GenerateErrorReport());
    }

    public void LoadScene(string scene)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    public void GetVirtualCurrencies()
    {
        PlayFabClientAPI.GetUserInventory(
            new GetUserInventoryRequest(),
            r =>
            {
                int coins = r.VirtualCurrency["GD"];
                UpdateMsg("Coins:" + coins);
            }, OnError);
    }

    public void GetCatalog()
    {
        PlayFabClientAPI.GetCatalogItems(
            new GetCatalogItemsRequest { CatalogVersion = "Bullet Skins" },
            r =>
            {
                List<CatalogItem> items = r.Catalog;
                UpdateMsg("Catalog Items");
                foreach (CatalogItem i in items)
                {
                    UpdateMsg(i.DisplayName + "," + i.VirtualCurrencyPrices["GD"]);
                }
            }, OnError);
    }

    public void GetPlayerInventory()
    {
        PlayFabClientAPI.GetUserInventory(
            new GetUserInventoryRequest{},
            r =>
            {
                List<ItemInstance> ii = r.Inventory;
                UpdateMsg("Player Inventory");
                foreach(ItemInstance i in ii)
                {
                    UpdateMsg(i.DisplayName + ", " + i.ItemId + ", " + i.ItemInstanceId);
                }
            }, OnError);
    }

    public void Buy()
    {
        PlayFabClientAPI.PurchaseItem(
            new PurchaseItemRequest
            {
                CatalogVersion = "Bullet Skins",
                ItemId = "SKIN01_RedBullet",
                VirtualCurrency = "GD",
                Price = 25
            },
            r =>
            {
                UpdateMsg("Bought " + r.Items[0].ItemId + "!");
            }, OnError);
    }
}
