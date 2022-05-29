using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    string shopName;

    [SerializeField]
    string currencyID;

    [SerializeField]
    GameObject shopItemPrefab;

    [SerializeField]
    GameObject shopLayoutObject;

    [SerializeField]
    LobbyStats playerStats;

    bool itemsLoaded = false;

    struct PlayFabShopItem
    {
        public string catalogVer;
        public string itemID;
        public uint price;
        public string currencyID;
        public string desc;
        public string iconURL;
        public Texture texture;
        public bool purchased;
        public bool purchasing;
        public PlayFabShopItem(string catalogVer, string itemID, uint price, string currencyID, string desc, string iconURL)
        {
            this.catalogVer = catalogVer;
            this.itemID = itemID;
            this.price = price;
            this.currencyID = currencyID;
            this.desc = desc;
            this.iconURL = iconURL;
            texture = null;
            purchased = false;
            purchasing = false;
        }
        
    }

    Dictionary<string, PlayFabShopItem> shopItems;

    void Start()
    {
        shopItems = new Dictionary<string, PlayFabShopItem>();
        PlayFabClientAPI.GetCatalogItems(
            new GetCatalogItemsRequest { CatalogVersion = shopName } ,
            r => {
                List<CatalogItem> items = r.Catalog;
                foreach(CatalogItem i in items)
                {
                    print("Add shop item " + i.DisplayName);
                    shopItems.Add(i.DisplayName, new PlayFabShopItem(i.CatalogVersion, i.ItemId, i.VirtualCurrencyPrices[currencyID], currencyID, i.Description, i.ItemImageUrl));
                }

                GetPlayerInventory();

                foreach (string key in shopItems.Keys)
                {
                    StartCoroutine(DownloadImage(shopItems[key].iconURL, key));
                }
            },
            OnError);
    }

    IEnumerator DownloadImage(string MediaUrl, string itemName)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
        {
            PlayFabShopItem i = shopItems[itemName];
            i.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            shopItems[itemName] = i;
            if(!LoadedTextures.instance.textureList.ContainsKey(itemName))
                LoadedTextures.instance.textureList.Add(itemName, i.texture);
        }
    }

    public void GetPlayerInventory()
    {
        PlayFabClientAPI.GetUserInventory(
            new GetUserInventoryRequest { },
            r =>
            {
                List<ItemInstance> iiList = r.Inventory;
                foreach(ItemInstance ii in iiList)
                {
                    print("Unlocked items " + ii.DisplayName);
                    if(shopItems.ContainsKey(ii.DisplayName))
                    {
                        PlayFabShopItem sI = shopItems[ii.DisplayName];
                        sI.purchased = true;
                        shopItems[ii.DisplayName] = sI;
                    }
                }
                ReLoadShopPrefabItems();
            }, OnError);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) ReLoadShopPrefabItems();
        int loadedCount = 0;
        foreach(string key in shopItems.Keys)
        {
            if(shopItems[key].texture != null)
            {
                loadedCount++;
            }
        }
        if(loadedCount > 0 && loadedCount == shopItems.Count && !itemsLoaded)
        {
            itemsLoaded = true;
            ReLoadShopPrefabItems();
        }
    }

    void ReLoadShopPrefabItems()
    {
        foreach(Transform trans in shopLayoutObject.transform)
        {
            GameObject.Destroy(trans.gameObject);
        }
        foreach(string key in shopItems.Keys)
        {
            GameObject obj = GameObject.Instantiate(shopItemPrefab, shopLayoutObject.transform);
            if(shopItems[key].texture != null)  obj.GetComponentInChildren<RawImage>().texture = shopItems[key].texture;
            obj.transform.Find("tx_name").GetComponent<TMP_Text>().text = key;
            TMP_Text coinsText = obj.transform.Find("tx_coinsreq").GetComponent<TMP_Text>();
            coinsText.text = "Cost: " + shopItems[key].price + " Coins";
            obj.transform.Find("tx_desc").GetComponent<TMP_Text>().text = shopItems[key].desc;

            GameObject bn = obj.transform.Find("bn_action").gameObject;
            bn.GetComponent<Button>().onClick.AddListener(() => { ShopButtonClick(bn); });
            string equippedSkin = playerStats.getSkinEquipped();
            PlayerPrefs.SetString("EquippedSkin", equippedSkin);
            if (key == equippedSkin)
            {
                bn.GetComponentInChildren<TMP_Text>().text = "Selected";
                coinsText.text = "Unlocked";
            }
            else if (shopItems[key].purchased)
            {
                bn.GetComponentInChildren<TMP_Text>().text = "Equip";
                coinsText.text = "Unlocked";

            }
            else
                bn.GetComponentInChildren<TMP_Text>().text = "Buy";
        }
    }

    public void ShopButtonClick(GameObject bn)
    {
        string itemDisplayName = bn.transform.parent.Find("tx_name").gameObject.GetComponent<TMP_Text>().text;
        string buttonAction = bn.GetComponentInChildren<TMP_Text>().text;

        if(buttonAction == "Buy")
        {
            if(shopItems[itemDisplayName].purchased == false && !shopItems[itemDisplayName].purchasing)
            {
                if(playerStats.getCoins() > shopItems[itemDisplayName].price)
                {
                    PlayFabShopItem si = shopItems[itemDisplayName];
                    si.purchasing = true; //To prevent more click through while API is running
                    shopItems[itemDisplayName] = si;

                    string oldeqSkin = playerStats.getSkinEquipped();
                    print("Playerstat coins pre" + playerStats.getCoins());
                    playerStats.setCoins(playerStats.getCoins() - (int)shopItems[itemDisplayName].price);
                    print("minus coins " + ((int)shopItems[itemDisplayName].price).ToString());
                    print("Playerstat coins post" + playerStats.getCoins());
                    playerStats.setEquippedSkin(itemDisplayName);

                    PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest
                    {
                        CatalogVersion = shopItems[itemDisplayName].catalogVer,
                        ItemId = shopItems[itemDisplayName].itemID,
                        VirtualCurrency = currencyID,
                        Price = (int)shopItems[itemDisplayName].price
                    },
                    r => {
                        //On success, send changes to playfab
                        playerStats.sendLocalCacheToPlayfab();
                        PlayFabShopItem i = shopItems[itemDisplayName];
                        i.purchased = true;
                        i.purchasing = false;
                        shopItems[itemDisplayName] = i;
                        ReLoadShopPrefabItems();


                    },
                    e => {
                        OnError(e);

                        //Reset local changes
                        playerStats.setCoins(playerStats.getCoins() + (int)shopItems[itemDisplayName].price);
                        playerStats.setEquippedSkin(oldeqSkin);

                        PlayFabShopItem si = shopItems[itemDisplayName];
                        si.purchasing = false; //To prevent more click through while API is running
                        shopItems[itemDisplayName] = si;
                        ReLoadShopPrefabItems();

                    });
                }
            }
        }else if (buttonAction == "Equip")
        {
            playerStats.setEquippedSkin(itemDisplayName);
            ReLoadShopPrefabItems();
        }
    }

    void OnError(PlayFabError e)
    {
    }

    public bool allShopItemsLoaded()
    {
        return itemsLoaded;
    }
}
