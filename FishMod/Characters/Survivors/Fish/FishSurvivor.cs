using BepInEx.Configuration;
using EntityStates;
using EntityStates.Fish;
using EntityStates.LunarWisp;
using FishMod.Characters.Survivors.Fish.Components;
using FishMod.Modules;
using FishMod.Modules.Characters;
using FishMod.Modules.Weapons;
using FishMod.Modules.Weapons.Guns;
using FishMod.Survivors.Fish.SkillStates;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static RoR2.GenericPickupController;
using static RoR2.TeleporterInteraction;

namespace FishMod.Survivors.Fish
{
    public class FishSurvivor : SurvivorBase<FishSurvivor>
    {
        //used to load the assetbundle for this character. must be unique
        public override string assetBundleName => "fishassetbundle"; //if you do not change this, you are giving permission to deprecate the mod

        //the name of the prefab we will create. conventionally ending in "Body". must be unique
        public override string bodyName => "FishBody"; //if you do not change this, you get the point by now

        //name of the ai master for vengeance and goobo. must be unique
        public override string masterName => "FishMonsterMaster"; //if you do not

        //the names of the prefabs you set up in unity that we will use to build your character
        public override string modelPrefabName => "mdlFish";
        public override string displayPrefabName => "FishDisplay";

        public const string FISH_PREFIX = FishPlugin.DEVELOPER_PREFIX + "_FISH_";

        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => FISH_PREFIX;

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = FISH_PREFIX + "NAME",
            subtitleNameToken = FISH_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texHenryIcon"),
            bodyColor = Color.white,
            sortPosition = 100,

            crosshair = Asset.LoadCrosshair("Standard"),
            podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 110f,
            healthRegen = 1.5f,
            armor = 0f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "SwordModel",
                    material = assetBundle.LoadMaterial("matHenry"),
                },
                new CustomRendererInfo
                {
                    childName = "GunModel",
                },
                new CustomRendererInfo
                {
                    childName = "Model",
                }
        };

        public override UnlockableDef characterUnlockableDef => FishUnlockables.characterUnlockableDef;

        // public override ItemDisplaysBase itemDisplays => new FishItemDisplays();

        //set in base classes
        public override AssetBundle assetBundle { get; protected set; }

        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }

        public override void Initialize()
        {
            //uncomment if you have multiple characters
            //ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Henry");

            //if (!characterEnabled.Value)
            //    return;

            base.Initialize();
        }

        public override void InitializeCharacter()
        {
            //need the character unlockable before you initialize the survivordef
            FishUnlockables.Init();

            base.InitializeCharacter();

            FishConfig.Init();
            FishStates.Init();
            FishTokens.Init();

            FishAssets.Init(assetBundle);
            FishBuffs.Init(assetBundle);

            InitializeWeapons();

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();

            AddHooks();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            bodyPrefab.AddComponent<FishWeaponController>();
            //bodyPrefab.AddComponent<HuntressTrackerComopnent>();
            //anything else here
        }

        public void AddHitboxes()
        {
            //example of how to create a HitBoxGroup. see summary for more details
            Prefabs.SetupHitBoxGroup(characterModelObject, "SwordGroup", "SwordHitbox");
        }

        public override void InitializeEntityStateMachines()
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(EntityStates.GenericCharacterMain), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your HenryStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");
        }

        #region skills
        public override void InitializeSkills()
        {
            //remove the genericskills from the commando body we cloned
            Skills.ClearGenericSkills(bodyPrefab);
            //add our own
            //AddPassiveSkill();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtiitySkills();
            AddSpecialSkills();
        }

        //skip if you don't have a passive
        //also skip if this is your first look at skills
        private void AddPassiveSkill()
        {
            //option 1. fake passive icon just to describe functionality we will implement elsewhere
            bodyPrefab.GetComponent<SkillLocator>().passiveSkill = new SkillLocator.PassiveSkill
            {
                enabled = true,
                skillNameToken = FISH_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = FISH_PREFIX + "PASSIVE_DESCRIPTION",
                keywordToken = "KEYWORD_STUNNING",
                icon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),
            };

            //option 2. a new SkillFamily for a passive, used if you want multiple selectable passives
            GenericSkill passiveGenericSkill = Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");
            SkillDef passiveSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "HenryPassive",
                skillNameToken = FISH_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = FISH_PREFIX + "PASSIVE_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),

                //unless you're somehow activating your passive like a skill, none of the following is needed.
                //but that's just me saying things. the tools are here at your disposal to do whatever you like with

                //activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot)),
                //activationStateMachineName = "Weapon1",
                //interruptPriority = EntityStates.InterruptPriority.Skill,

                //baseRechargeInterval = 1f,
                //baseMaxStock = 1,

                //rechargeStock = 1,
                //requiredStock = 1,
                //stockToConsume = 1,

                //resetCooldownTimerOnUse = false,
                //fullRestockOnAssign = true,
                //dontAllowPastMaxStocks = false,
                //mustKeyPress = false,
                //beginSkillCooldownOnSkillEnd = false,

                //isCombatSkill = true,
                //canceledFromSprinting = false,
                //cancelSprintingOnActivation = false,
                //forceSprintDuringState = false,

            });
            Skills.AddSkillsToFamily(passiveGenericSkill.skillFamily, passiveSkillDef1);
        }

        //if this is your first look at skilldef creation, take a look at Secondary first
        private void AddPrimarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);

            //the primary skill is created using a constructor for a typical primary
            //it is also a SteppedSkillDef. Custom Skilldefs are very useful for custom behaviors related to casting a skill. see ror2's different skilldefs for reference
            SteppedSkillDef primarySkillDef1 = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "HenrySlash",
                    FISH_PREFIX + "PRIMARY_SLASH_NAME",
                    FISH_PREFIX + "PRIMARY_SLASH_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),
                    new EntityStates.SerializableEntityStateType(typeof(SkillStates.SlashCombo)),
                    "Weapon",
                    true
                ));
            //custom Skilldefs can have additional fields that you can set manually
            primarySkillDef1.stepCount = 2;
            primarySkillDef1.stepGraceDuration = 0.5f;

            // Skills.AddPrimarySkills(bodyPrefab, primarySkillDef1);

            // assign revolver as primary
            if (Revolver.instance.primarySkillDef != null)
            {
                Skills.AddPrimarySkills(bodyPrefab, Revolver.instance.primarySkillDef);
            }
            else
            {
                Debug.LogError("FishSurvivor.AddPrimarySkills : Failed to find Revolver skillDef! Is it disabled?");
            }
        }

        private void AddSecondarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);

            //here is a basic skill def with all fields accounted for
            SkillDef secondarySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "HenryGun",
                skillNameToken = FISH_PREFIX + "SECONDARY_GUN_NAME",
                skillDescriptionToken = FISH_PREFIX + "SECONDARY_GUN_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 1f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            Skills.AddSecondarySkills(bodyPrefab, secondarySkillDef1);
        }

        private void AddUtiitySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Utility);

            //here's a skilldef of a typical movement skill.
            SkillDef utilitySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "HenryRoll",
                skillNameToken = FISH_PREFIX + "UTILITY_ROLL_NAME",
                skillDescriptionToken = FISH_PREFIX + "UTILITY_ROLL_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Roll)),
                activationStateMachineName = "Body",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 4f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            Skills.AddUtilitySkills(bodyPrefab, utilitySkillDef1);
        }

        private void AddSpecialSkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Special);

            SkillDef specialSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "FishSwap",
                skillNameToken = FISH_PREFIX + "SWAP_NAME",
                skillDescriptionToken = FISH_PREFIX + "SWAP_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpecialIcon"),

                activationState = new SerializableEntityStateType(typeof(SwapWeapons)),
                //setting this to the "weapon2" EntityStateMachine allows us to cast this skill at the same time primary, which is set to the "weapon" EntityStateMachine
                activationStateMachineName = "Weapon",
                interruptPriority = InterruptPriority.Skill,

                stockToConsume = 0,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,

                isCombatSkill = false,
                mustKeyPress = true,
            });

            Skills.AddSpecialSkills(bodyPrefab, specialSkillDef1);
        }
        #endregion skills

        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texMainSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
            //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
            //uncomment this when you have another skin
            //defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySword",
            //    "meshHenryGun",
            //    "meshHenry");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin

            ////creating a new skindef as we did before
            //SkinDef masterySkin = Modules.Skins.CreateSkinDef(HENRY_PREFIX + "MASTERY_SKIN_NAME",
            //    assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
            //    defaultRendererinfos,
            //    prefabCharacterModel.gameObject,
            //    HenryUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            //masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySwordAlt",
            //    null,//no gun mesh replacement. use same gun mesh
            //    "meshHenryAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            //masterySkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            //masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            //{
            //    new SkinDef.GameObjectActivation
            //    {
            //        gameObject = childLocator.FindChildGameObject("GunModel"),
            //        shouldActivate = false,
            //    }
            //};
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            //skins.Add(masterySkin);

            #endregion

            skinController.skins = skins.ToArray();
        }
        #endregion skins

        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //you must only do one of these. adding duplicate masters breaks the game.

            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            FishAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);

        }

        private void InitializeWeapons()
        {
            new Revolver().Init();
            new Machinegun().Init();
        }

        private void AddHooks()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            // manage weapon+ammo drops (and later rads)
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;

            // weapon reload -> skill icon ui
            On.RoR2.UI.SkillIcon.Update += SkillIcon_Update;

            // define + manage weapon pools
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.Run.Start += Run_Start;

            On.RoR2.GenericPickupController.AttemptGrant += GenericPickupController_AttemptGrant;
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);

            FishWeaponCatalog.scenesCleared = 0;
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);

            Debug.Log("FishSurvivor.SceneDirector_Start : Active on " + SceneManager.GetActiveScene().name);

            if (Run.instance == null)
            {
                return;
            }

            // in proper nuclear throne fashion, we even count intermissions and secrets to the counter
            FishWeaponCatalog.scenesCleared++;

            FishWeaponCatalog.RefreshAvailableWeapons();
        }

        // this big hook just catches skill icons and applies weapon reload times to the ui
        // the world if you could just override skill cooldown:    utopia.gif
        private void SkillIcon_Update(On.RoR2.UI.SkillIcon.orig_Update orig, SkillIcon self)
        {
            // unfortunately mostly the same logic as the original other than feeding our custom cooldown :/
            if (self.targetSkill.skillDef is FishWeaponSkillDef fwsd)
            {
                self.CheckAndRegisterSkillChange();

                if (self.tooltipProvider)
                {
                    Color bodyColor = self.targetSkill.characterBody.bodyColor;
                    BodyIndex bodyIndex = self.targetSkill.characterBody.bodyIndex;
                    SurvivorIndex survivorIndex = SurvivorCatalog.GetSurvivorIndexFromBodyIndex(bodyIndex);

                    {
                        Color.RGBToHSV(bodyColor, out float h, out float s, out float v);
                        v = (v > 0.7f) ? 0.7f : v;
                        bodyColor = Color.HSVToRGB(h, s, v);
                    }

                    self.tooltipProvider.titleColor = bodyColor;
                    self.tooltipProvider.titleToken = self.targetSkill.skillNameToken;
                    self.tooltipProvider.bodyToken = self.targetSkill.skillDescriptionToken;
                }

                float cooldownRemaining = fwsd.pseudoCooldownRemaining;
                float totalCooldown = fwsd.basePseudoCooldown;
                int skillStock = self.targetSkill.stock;
                bool skillReady = ((skillStock > 0) || (cooldownRemaining <= 0f));
                bool skillShouldShowAsReady = self.targetSkill.IsReady();
                bool isSkillCooldownBlocked = self.targetSkill.isCooldownBlocked;
                bool skillShouldShowStock = self.targetSkill.maxStock > 1 && self.targetSkill.skillDef.hideStockCount == false;

                if (self.previousStock < skillStock && self.skillChanged == false)
                {
                    // we might wanna disable this. potentially f&$#ing annoying.
                    Util.PlaySound("Play_UI_cooldownRefresh", RoR2Application.instance.gameObject);
                }

                if (self.animator)
                {
                    if (skillShouldShowStock)
                    {
                        self.animator.SetBool(self.animatorStackString, true);
                    }
                    else
                    {
                        self.animator.SetBool(self.animatorStackString, false);
                    }
                }

                if (self.isReadyPanelObject)
                {
                    self.isReadyPanelObject.SetActive(skillShouldShowAsReady);
                }

                if (!self.wasReady && skillReady)
                {
                    if (self.flashPanelObject)
                    {
                        self.flashPanelObject.SetActive(true);
                    }
                }

                if (self.cooldownText)
                {
                    if (skillReady || skillShouldShowAsReady || isSkillCooldownBlocked)
                    {
                        self.cooldownText.gameObject.SetActive(false);
                    }
                    else
                    {
                        int thisFrameCoolDown = Mathf.CeilToInt(cooldownRemaining);
                        if (self.previousCoolDownInt != thisFrameCoolDown)
                        {
                            // ughhhhhhhhhh
                            StringBuilder stringBuilder = new StringBuilder();
                            self.previousCoolDownInt = thisFrameCoolDown;
                            stringBuilder.Clear();
                            stringBuilder.AppendInt(thisFrameCoolDown);
                            self.cooldownText.SetText(stringBuilder);
                            self.cooldownText.gameObject.SetActive(true);
                        }
                    }
                }

                if (self.iconImage)
                {
                    self.iconImage.enabled = true;
                    self.iconImage.color = skillShouldShowAsReady ? Color.white : Color.gray;
                    self.iconImage.sprite = self.targetSkill.icon;
                }

                if (self.cooldownRemapPanel)
                {
                    float cooldownFraction = 1f;
                    if (totalCooldown >= Mathf.Epsilon)
                    {
                        cooldownFraction = (1f - (cooldownRemaining / totalCooldown));
                    }

                    float alpha = cooldownFraction;
                    self.cooldownRemapPanel.enabled = alpha < 1f;
                    self.cooldownRemapPanel.color = new Color(1f, 1f, 1f, cooldownFraction);
                }

                if (self.stockText)
                {
                    if (skillShouldShowStock)
                    {
                        if (self.previousStock != skillStock)
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            self.stockText.gameObject.SetActive(true);
                            stringBuilder.Clear();
                            stringBuilder.AppendInt(skillStock);
                            self.stockText.SetText(stringBuilder);
                        }
                    }
                    else
                    {
                        self.stockText.gameObject.SetActive(false);
                    }
                }

                self.wasReady = skillReady;
                self.previousStock = skillStock;
                self.skillChanged = false;

                return;
            }

            orig(self);
        }

        // drop manager
        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            if (!NetworkServer.active)
                return;

            // victim exists and had a master
            bool validVictim =
                report.victimBody != null &&
                report.victimMaster != null;

            // killer must be fish. needs expanded if we ever want more NT survivors
            bool killedByFish =
                report.attackerBodyIndex == BodyCatalog.FindBodyIndex(instance.bodyPrefab);

            if (validVictim && killedByFish)
            {
                Debug.Log("FishSurvivor.GlobalEventManager_onCharacterDeathGlobal : Fish player killed " + report.victimBody.GetDisplayName());

                CharacterMaster fishMaster = report.attackerMaster;
                HullClassification hullClassification = report.victimBody.hullClassification;

                // elites, bosses, and bodies larger than human can drop weapons by default
                bool eligibleWeaponDrop =
                    report.victimIsElite ||
                    report.victimIsBoss ||
                    hullClassification != HullClassification.Human;

                if (true || eligibleWeaponDrop && Util.CheckRoll(8f, fishMaster))
                {
                    bool isRobot = false;
                    int offset = -1;

                    if (isRobot) offset++;


                    PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(FishWeaponCatalog.RandomAvailableWeaponDef(offset).itemDef.itemIndex);

                    Debug.Log("FishSurvivor.GlobalEventManager_onCharacterDeathGlobal : Attempting to create weapon pickup of type " + PickupCatalog.GetPickupDef(pickupIndex).nameToken);

                    CreatePickupInfo pickupInfo = new CreatePickupInfo
                    {
                        pickupIndex = pickupIndex,
                    };

                    PickupDropletController.CreatePickupDroplet(pickupInfo, report.victimBody.corePosition, Vector3.up);
                }

                FishWeaponController fishWeaponController = report.attackerBody.GetComponent<FishWeaponController>();

                float ammoDropChance = 5f;
                int maxDrops = 1;
                if (hullClassification == HullClassification.Human)
                    ammoDropChance = 15f;

                if (hullClassification == HullClassification.Golem)
                    ammoDropChance = 30f;

                if (hullClassification == HullClassification.BeetleQueen)
                    ammoDropChance = 200f;

                if (report.victimIsElite)
                {
                    ammoDropChance += 20f; maxDrops++;
                }

                if (report.victimIsBoss)
                {
                    ammoDropChance += 50f; maxDrops++;
                }

                // hull size is *mostly* indicative of intended strength in so we're using that
                // bigger guy = bigger health bar i mean it just makes sense. but theres some exceptions
                // looking at you, stone titan :/

                // elite and boss enemies have extra opportunites to drop ammo
                // in actual nuclear throne i'm pretty sure this is limited to 2
                // but that game doesnt have elite bosses and that's the only place we'll ever see 3

                // rabbit paw maybe could just be another add to maxDrops

                // ammoDropChance *= 999f; // ffffffucking debug

                for (int i = 0; i < maxDrops; ++i)
                {
                    if (Util.CheckRoll(ammoDropChance * fishWeaponController.GetCurrentDropMultiplier(), fishMaster))
                    {
                        GameObject pickup = UnityEngine.Object.Instantiate(FishAssets.ammoPickupPrefab, report.victimBody.corePosition, UnityEngine.Random.rotation);
                        TeamFilter teamFilter = pickup.GetComponent<TeamFilter>();
                        if (teamFilter != null)
                        {
                            teamFilter.teamIndex = report.attackerTeamIndex;
                        }

                        NetworkServer.Spawn(pickup);
                    }
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {

            if (sender.HasBuff(FishBuffs.armorBuff))
            {
                args.armorAdd += 300;
            }
        }

        private static void GenericPickupController_AttemptGrant(On.RoR2.GenericPickupController.orig_AttemptGrant orig, GenericPickupController self, CharacterBody body)
        {
            if (self && body)
            {
                if (self.pickupIndex.isValid)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(self.pickupIndex.itemIndex);
                    if (itemDef != null && FishWeaponCatalog.GetWeaponDefFromItemDef(itemDef) != null)
                    {
                        if (body.TryGetComponent(out FishWeaponController fwc))
                        {
                            if (self.chestGeneratedFrom == null || self.chestGeneratedFrom != fwc.chestBehavior)
                            {
                                fwc.GiveAmmoPackOfType(FishWeaponCatalog.GetWeaponDefFromItemDef(itemDef).ammoType);
                            }
                        }
                        else
                        {
                            return; // non-fish can't pick up guns
                        }
                    }

                }
            }

            orig(self, body);
        }
    }
}