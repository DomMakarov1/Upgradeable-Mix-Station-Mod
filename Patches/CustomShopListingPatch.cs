using HarmonyLib;
using ScheduleOne.UI.Shop;
using MelonLoader;
using UnityEngine;

namespace UpgradeableMixStationMod.Patches
{
    [HarmonyPatch(typeof(ListingUI), nameof(ListingUI.Initialize))]
    public static class CustomShop_ListingUI_Patch
    {
        public static void Postfix(ListingUI __instance)
        {
            __instance.onDropdownClicked += () =>
            {
                Canvas shopCanvas = __instance.GetComponentInParent<Canvas>();
                if (shopCanvas == null)
                {
                    MelonLogger.Error("[Shop] Could not find parent Canvas!");
                    return;
                }

                var amountSelector = shopCanvas.GetComponentInChildren<ShopAmountSelector>(true);
                
                if (amountSelector == null) 
                {
                    var globalSelector = UnityEngine.Object.FindObjectOfType<ShopAmountSelector>(true);
                    if (globalSelector != null)
                    {
                        amountSelector = UnityEngine.Object.Instantiate(globalSelector, shopCanvas.transform);
                        
                        amountSelector.gameObject.SetActive(true);
                        
                        var rect = amountSelector.GetComponent<RectTransform>();
                        if (rect != null) rect.localScale = Vector3.one;
                    }
                }
                
                if (amountSelector != null)
                {
                    if (__instance.DropdownAnchor != null)
                    {
                        amountSelector.transform.position = __instance.DropdownAnchor.position;
                        
                        var rect = amountSelector.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.anchoredPosition += new Vector2(0f, -rect.rect.height);
                        }
                    }
                    
                    amountSelector.transform.SetAsLastSibling();
                    amountSelector.Open();
                    
                    amountSelector.onSubmitted.RemoveAllListeners();
                    
                    amountSelector.onSubmitted.AddListener((int amount) =>
                    {   
                        try 
                        {
                            Cart shopCart = shopCanvas.GetComponentInChildren<Cart>(true);
                            
                            if (shopCart != null)
                            {
                                shopCart.AddItem(__instance.Listing, amount);
                            }
                            else
                            {
                                MelonLogger.Error("[Shop] FAILED: Could not find the Cart component in this shop's UI!");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            MelonLogger.Error($"[Shop] Error adding to cart: {ex.Message}");
                        }
                    });
                }
                else
                {
                    MelonLogger.Error("[Shop] FAILED: Could not find the base game's ShopAmountSelector to clone!");
                }
            };
        }
    }
}