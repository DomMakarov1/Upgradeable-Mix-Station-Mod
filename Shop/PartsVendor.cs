using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using MelonLoader.Utils;
using UpgradeableMixStationMod.Data;
using UpgradeableMixStationMod.Items;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Interaction;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.UI.Shop;
using ScheduleOne.Map;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace UpgradeableMixStationMod.Shop
{
    public static class PartsVendor
    {
        private static ShopInterface _shop;
        private static Canvas _shopCanvas;
        private static RectTransform _shopContainer;

        private static NPC _vendorNPC;
        private static Vector3 _spawnPos;
        private static Quaternion _spawnRot;
        private static bool _spawned;

        public static Vector3 VendorPosition = new Vector3(67.77f, 0f, 28.74f);
        public static Quaternion VendorRotation = Quaternion.Euler(0f, 90f, 0f);

        public static bool IsShopOpen => _shopContainer != null && _shopContainer.gameObject.activeInHierarchy;

        public static IEnumerator SpawnVendor()
        {
            if (_spawned) yield break;
            yield return new WaitForSeconds(3f);

            var sourceShop = ShopInterface.AllShops.Find(s => s.ShopCode == "gas_mart_central");
            if (sourceShop == null)
            {
                MelonLogger.Warning("[Shop] Could not find source shop to clone.");
                yield break;
            }

            var shopGO = UnityEngine.Object.Instantiate(sourceShop.gameObject);
            shopGO.name = "UpgradePartsShop";
            _shop = shopGO.GetComponent<ShopInterface>();
            _shop.ShopName = "Mix Tech Parts";
            _shop.ShopCode = "mix_tech_parts";
            _shopCanvas = shopGO.GetComponentInChildren<Canvas>(true);
            _shopContainer = _shop.Container;

            if (_shop.StoreNameLabel != null)
                _shop.StoreNameLabel.text = "Mix Tech Parts";

            var screenField = typeof(ShopInterface).GetField("shopScreen",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (screenField != null)
            {
                var screen = shopGO.GetComponentInChildren(screenField.FieldType, true);
                if (screen != null) screenField.SetValue(_shop, screen);
            }

            var cart = shopGO.GetComponentInChildren<Cart>(true);
            if (cart != null) cart.Shop = _shop;

            var listingContainer = _shop.ListingContainer;
            if (listingContainer != null)
                for (int i = listingContainer.childCount - 1; i >= 0; i--)
                    UnityEngine.Object.Destroy(listingContainer.GetChild(i).gameObject);

            var listingUIField = typeof(ShopInterface).GetField("listingUI",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            (listingUIField?.GetValue(_shop) as List<ListingUI>)?.Clear();
            _shop.Listings.Clear();

            yield return null;

            if (UpgradeParts.MixingGear != null)
                AddListing(_shop, UpgradeParts.MixingGear, 150f);
            if (UpgradeParts.PrecisionComponent != null)
                AddListing(_shop, UpgradeParts.PrecisionComponent, 400f);

            SpawnTable();
            
            MelonCoroutines.Start(AddVendorMapPOI());

            MelonCoroutines.Start(SpawnNPC());

            try { TryRegisterDayPass(); } catch { }

            _spawned = true;
        }

        private static void TryRegisterDayPass()
        {
            try
            {
                var timeMgr = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
                if (timeMgr != null)
                    timeMgr.onDayPass = (Action)Delegate.Combine(timeMgr.onDayPass, new Action(OnDayPass));
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Shop] Could not register TimeManager onDayPass: {ex.Message}");
            }
        }

        private static IEnumerator SpawnNPC()
        {
            _spawnPos = new Vector3(67.01f, 0.11f, 28.78f);
            _spawnRot = Quaternion.Euler(0f, 90f, 0f);

            NPC sourceNPC = null;
            foreach (var npc in NPCManager.NPCRegistry)
            {
                if (npc == null) continue;
                if (npc.DialogueHandler == null) continue;
                if (npc.GetComponentInChildren<DialogueController>() == null) continue;
                sourceNPC = npc;
                break;
            }

            if (sourceNPC == null)
            {
                MelonLogger.Error("[Shop] Checked NPCs, none suitable. Vendor NPC not spawned.");
                yield break;
            }

            var npcGO = UnityEngine.Object.Instantiate(sourceNPC.gameObject);
            npcGO.name = "Nick";

            _vendorNPC = npcGO.GetComponent<NPC>();
            _vendorNPC.FirstName = "Nick";
            _vendorNPC.hasLastName = false;
            _vendorNPC.LastName = "";
            _vendorNPC.ID = "tech_dealer_nick";
            _vendorNPC.BakedGUID = Guid.NewGuid().ToString();
            _vendorNPC.CanBeSummoned = false;

            var agent = npcGO.GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            if (_vendorNPC.Health != null)
                _vendorNPC.Health.Invincible = true;

            _vendorNPC.OverrideParent = true;
            _vendorNPC.OverriddenParent = null;

            foreach (var comp in npcGO.GetComponentsInChildren<MonoBehaviour>())
            {
                var typeName = comp.GetType().Name;
                if (typeName.Contains("Schedule") || typeName.Contains("Behaviour") || typeName.Contains("Behavior"))
                {
                    if (comp != _vendorNPC && !typeName.Contains("Dialogue"))
                        comp.enabled = false;
                }
            }

            SetupDialogue(npcGO);

            yield return null;
            yield return null;

            npcGO.transform.position = _spawnPos;
            npcGO.transform.rotation = _spawnRot;
            npcGO.SetActive(true);
            _vendorNPC.SetVisible(true);

            foreach (var rend in npcGO.GetComponentsInChildren<Renderer>(true))
            {
                if (rend is MeshRenderer && rend.GetComponent<MeshFilter>() != null)
                {
                    var meshName = rend.GetComponent<MeshFilter>().sharedMesh?.name ?? "";
                    if (meshName.Contains("Capsule") || meshName.Contains("Sphere") ||
                        meshName.Contains("Cube") || meshName.Contains("Cylinder"))
                    {
                        rend.enabled = false;
                        continue;
                    }
                }
                rend.enabled = true;
            }

            if (_vendorNPC.Movement != null)
            {
                _vendorNPC.Movement.Stop();
                _vendorNPC.Movement.SetAgentEnabled(false);
            }

            yield return null;
            npcGO.transform.position = _spawnPos;
            npcGO.transform.rotation = _spawnRot;

            MelonCoroutines.Start(KeepInPlace());
        }

        private static void SetupDialogue(GameObject npcGO)
        {
            var controller = npcGO.GetComponent<DialogueController>()
                          ?? npcGO.GetComponentInChildren<DialogueController>();

            if (controller == null)
            {
                MelonLogger.Error("[Shop] No DialogueController found on cloned NPC — dialogue will not work.");
                return;
            }

            controller.Choices.Clear();
            controller.GreetingOverrides.Clear();
            controller.UseDialogueBehaviour = false;

            var handlerComp = npcGO.GetComponent<DialogueHandler>()
                           ?? npcGO.GetComponentInChildren<DialogueHandler>();
            if (handlerComp != null)
                handlerComp.DialogueEvents = new ScheduleOne.Dialogue.DialogueEvent[0];

            var greeting = new DialogueController.GreetingOverride();
            greeting.Greeting = "What do you need?";
            greeting.ShouldShow = true;
            greeting.PlayVO = false;
            controller.AddGreetingOverride(greeting);

            var shopChoice = new DialogueController.DialogueChoice();
            shopChoice.Enabled = true;
            shopChoice.ChoiceText = "I want to buy something.";
            shopChoice.ShowWorldspaceDialogue = true;
            shopChoice.onChoosen = new UnityEngine.Events.UnityEvent();
            shopChoice.onChoosen.AddListener(() =>
            {
                SetSkipBehaviourEnd();
                MelonCoroutines.Start(EndDialogueThen(() => OpenShop()));
            });
            controller.AddDialogueChoice(shopChoice, 10);

            var hintChoice = new DialogueController.DialogueChoice();
            hintChoice.Enabled = true;
            hintChoice.ChoiceText = "Where can I find a Prototype Core?";
            hintChoice.ShowWorldspaceDialogue = true;
            hintChoice.onChoosen = new UnityEngine.Events.UnityEvent();
            hintChoice.onChoosen.AddListener(() =>
            {
                SetSkipBehaviourEnd();
                string response = GetHintResponse();
                MelonCoroutines.Start(EndDialogueThen(() => ShowHintBubble(response)));
            });
            controller.AddDialogueChoice(hintChoice, 5);

            var nothingChoice = new DialogueController.DialogueChoice();
            nothingChoice.Enabled = true;
            nothingChoice.ChoiceText = "Nothing.";
            nothingChoice.ShowWorldspaceDialogue = true;
            nothingChoice.onChoosen = new UnityEngine.Events.UnityEvent();
            nothingChoice.onChoosen.AddListener(() =>
            {
                SetSkipBehaviourEnd();
                MelonCoroutines.Start(EndDialogueThen(null));
            });
            controller.AddDialogueChoice(nothingChoice, 1);
        }

        private static IEnumerator EndDialogueThen(Action callback)
        {
            yield return null;
            yield return null;
            callback?.Invoke();
        }

        private static void SetSkipBehaviourEnd()
        {
            try
            {
                var handler = _vendorNPC?.DialogueHandler;
                if (handler == null) return;
                var field = typeof(DialogueHandler).GetField(
                    "skipNextDialogueBehaviourEnd",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    field.SetValue(handler, true);
            }
            catch { }
        }

        private static void ShowHintBubble(string text)
        {
            try { _vendorNPC?.DialogueHandler?.ShowWorldspaceDialogue(text, 6f); } catch { }
        }

        private static string GetHintResponse()
        {
            if (PartSpawner.TrackedDrops.Count == 0)
                return "No Prototype Cores out there today. Check back tomorrow.";

            if (PartSpawner.HasGivenHintToday)
                return "I already told you what I know today. Come back tomorrow.";

            var hintDrop = PartSpawner.GetHintDrop();
            if (hintDrop == null)
                return "I don't have any leads right now.";

            string dropName = !string.IsNullOrEmpty(hintDrop.DeadDropName)
                ? hintDrop.DeadDropName
                : "an unknown location";

            PartSpawner.MarkHintGiven();
            return $"I heard there's a Prototype Core at the dead drop near \"{dropName}\". That's all I know for today.";
        }

        private static IEnumerator KeepInPlace()
        {
            while (_vendorNPC != null)
            {
                if (_vendorNPC.Movement != null && _vendorNPC.Movement.IsMoving)
                {
                    _vendorNPC.Movement.Stop();
                    _vendorNPC.Movement.SetAgentEnabled(false);
                }

                if (Vector3.Distance(_vendorNPC.transform.position, _spawnPos) > 0.3f)
                {
                    _vendorNPC.transform.position = _spawnPos;
                    _vendorNPC.transform.rotation = _spawnRot;
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private static void OnDayPass()
        {
            if (_vendorNPC == null) return;

            try
            {
                if (_vendorNPC.Health != null && (_vendorNPC.Health.IsDead || _vendorNPC.Health.IsKnockedOut))
                {
                    UnityEngine.Object.Destroy(_vendorNPC.gameObject);
                    MelonCoroutines.Start(SpawnNPC());
                    return;
                }

                _vendorNPC.gameObject.SetActive(true);
                _vendorNPC.SetVisible(true);
                _vendorNPC.transform.position = _spawnPos;
                _vendorNPC.transform.rotation = _spawnRot;

                if (_vendorNPC.Movement != null)
                {
                    _vendorNPC.Movement.Stop();
                    _vendorNPC.Movement.SetAgentEnabled(false);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Shop] Respawn failed: {ex.Message}");
            }
        }

        private static void OpenShop()
        {
            if (_shop == null) return;
            _shop.SetIsOpen(true);
            if (_shopCanvas != null) _shopCanvas.enabled = true;
            if (_shopContainer != null) _shopContainer.gameObject.SetActive(true);
            MelonCoroutines.Start(WatchForShopClose());
        }

        private static void ForceCloseShop()
        {
            if (_shop == null) return;
            _shop.SetIsOpen(false);
            if (_shopCanvas != null) _shopCanvas.enabled = false;
            if (_shopContainer != null) _shopContainer.gameObject.SetActive(false);
            ClearWorldspaceDialogue();
        }

        private static void ClearWorldspaceDialogue()
        {
            try { _vendorNPC?.DialogueHandler?.ShowWorldspaceDialogue(" ", 0f); } catch { }
        }

        private static IEnumerator WatchForShopClose()
        {
            yield return new WaitForSeconds(0.3f);
            while (_shop != null && _shop.IsOpen)
            {
                var player = PlayerSingleton<PlayerMovement>.Instance;
                if (player != null && Vector3.Distance(player.transform.position, VendorPosition) > 8f)
                {
                    ForceCloseShop();
                    yield break;
                }
                yield return null;
            }
            if (_shopCanvas != null) _shopCanvas.enabled = false;
            if (_shopContainer != null) _shopContainer.gameObject.SetActive(false);
            ClearWorldspaceDialogue();
        }

        private static void SpawnTable()
        {
            var root = new GameObject("VendorTable");
            root.transform.position = VendorPosition;
            root.transform.rotation = VendorRotation;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.transform.SetParent(root.transform, false);
            table.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            table.transform.localScale = new Vector3(1.4f, 0.08f, 0.7f);
            table.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.15f, 0.2f, 0.25f) };
            table.layer = LayerMask.NameToLayer("Default");

            var legMat = new Material(shader) { color = new Color(0.1f, 0.12f, 0.15f) };
            foreach (var pos in new[] {
                new Vector3(0.6f, 0.37f, 0.28f), new Vector3(-0.6f, 0.37f, 0.28f),
                new Vector3(0.6f, 0.37f, -0.28f), new Vector3(-0.6f, 0.37f, -0.28f) })
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.transform.SetParent(root.transform, false);
                leg.transform.localPosition = pos;
                leg.transform.localScale = new Vector3(0.06f, 0.7f, 0.06f);
                leg.GetComponent<Renderer>().material = legMat;
                leg.layer = LayerMask.NameToLayer("Default");
            }

            var gear = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gear.transform.SetParent(root.transform, false);
            gear.transform.localPosition = new Vector3(-0.3f, 0.85f, 0f);
            gear.transform.localScale = new Vector3(0.2f, 0.04f, 0.2f);
            gear.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.5f, 0.7f, 0.9f) };
            UnityEngine.Object.Destroy(gear.GetComponent<Collider>());

            var comp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            comp.transform.SetParent(root.transform, false);
            comp.transform.localPosition = new Vector3(0.3f, 0.84f, 0f);
            comp.transform.localScale = new Vector3(0.15f, 0.1f, 0.15f);
            comp.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.9f, 0.6f, 0.2f) };
            UnityEngine.Object.Destroy(comp.GetComponent<Collider>());
        }

        private static void AddListing(ShopInterface shop, PropertyItemDefinition def, float price)
        {
            if (def == null || shop == null) return;

            var listing = new ShopListing
            {
                name = def.Name,
                Item = def,
                LimitedStock = false,
                DefaultStock = -1,
                CanBeDelivered = false,
            };

            typeof(ShopListing).GetField("OverridePrice",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(listing, true);
            typeof(ShopListing).GetField("OverriddenPrice",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(listing, price);

            shop.Listings.Add(listing);
            listing.Initialize(shop);

            typeof(ShopInterface)
                .GetMethod("CreateListingUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(shop, new object[] { listing });
        }

        public static Texture2D LoadCustomTexture(string fileName)
        {
            try
            {
                string path = Path.Combine(MelonEnvironment.MelonBaseDirectory, "Mods", "UpgradeableMixStationMod", "Icons", fileName);

                if (!File.Exists(path))
                {
                    MelonLogger.Warning($"[Shop] Map icon not found at: {path}");
                    return null;
                }

                byte[] data = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                #if MONO
                tex.LoadImage(data);
                #else
                UnityEngine.ImageConversion.LoadImage(tex, data);
                #endif
                return tex;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Shop] Failed to load map icon: {ex.Message}");
                return null;
            }
        }

        private static IEnumerator AddVendorMapPOI()
        {
            yield return new WaitForSeconds(1f);

            var existingPOI = UnityEngine.Object.FindObjectOfType<POI>();
            if (existingPOI == null)
            {
                MelonLogger.Warning("[Shop] Could not find an existing POI to clone for the map marker.");
                yield break;
            }
            
            var poiAnchor = new GameObject("MixTechPartsPOI_Anchor");
            poiAnchor.transform.position = VendorPosition;

            var poiGO = UnityEngine.Object.Instantiate(existingPOI.gameObject, poiAnchor.transform);
            poiGO.name = "MixTechPartsPOI";
            poiGO.transform.localPosition = new Vector3(0f, 2f, 0f);
            
            var poi = poiGO.GetComponent<POI>();

            poi.SetMainText("Mix Tech Parts");
            
            poi.onUICreated.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                MelonCoroutines.Start(SetPOIIcon(poi));
            }));

            poi.enabled = true;
            poiGO.SetActive(true);
        }

        private static IEnumerator SetPOIIcon(POI poi)
        {
            yield return null;

            if (poi.IconContainer == null)
            {
                MelonLogger.Warning("[Shop] POI IconContainer is null, cannot set custom icon.");
                yield break;
            }

            for (int i = poi.IconContainer.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(poi.IconContainer.GetChild(i).gameObject);

            yield return null;

            var tex = LoadCustomTexture("Supplier.png");
            if (tex == null) yield break;

            var iconGO = new GameObject("MixTechIcon");
            iconGO.transform.SetParent(poi.IconContainer, false);

            var rect = iconGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(25f, 25f);

            var img = iconGO.AddComponent<Image>();
            img.sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
            img.color = Color.white;
            img.preserveAspect = true;
            
        }
    }
}