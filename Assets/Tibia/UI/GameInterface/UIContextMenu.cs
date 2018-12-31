using Assets.Tibia.ASPR.Graphics;
using Assets.Tibia.DAO;
using Game.DAO;
using GameClient.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Tibia.UI.GameInterface
{
    [RequireComponent(typeof(UIVisibleToggle))]
    public class UIContextMenu : MonoBehaviour
    {
        public bool Visible
        {
            get => GetComponent<UIVisibleToggle>().Visible;
            set => GetComponent<UIVisibleToggle>().Visible = value;
        }

        public GameObject Separator;
        public GameObject Option;

        public void Clear()
        {
            foreach (Transform item in transform)
            {
                Destroy(item.gameObject);
            }
        }


        void Awake()
        {
            GetComponent<UIVisibleToggle>().VisibilityChange += UIContextMenu_VisibilityChange;
        }

        public void ContextMenuRequest()
        {
            Visible = false;

            var autoWalkPos = new Vector3();
            var mouseButton = MouseButton.MouseNoButton;

            if (Input.GetMouseButtonUp(0)) mouseButton = MouseButton.MouseLeftButton;
            if (Input.GetMouseButtonUp(1)) mouseButton = MouseButton.MouseRightButton;

            MouseSpriteSelect.Instance.Test();

            if (MouseSpriteSelect.Instance.Selected != null)
            {
                
                var tile = ((Tile)MouseSpriteSelect.Instance.Selected.Thing.Tile);
                var lookThing = tile.TopLookThing;
                var useThing = tile.TopUseThing;
                var topCreature = tile.TopCreature as Creature;

                UITextController.Instance.PoolText(MouseSpriteSelect.Instance.worldPos, lookThing.ThingType.Id.ToString(), Color.cyan, 2);

                ProcessMouseAction(Input.mousePosition, mouseButton, autoWalkPos, lookThing, useThing, topCreature, topCreature);
            }
        }

        internal static KeyboardModifier GetKeyboardModifiers()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                return KeyboardModifier.KeyboardCtrlModifier;
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
                return KeyboardModifier.KeyboardAltModifier;
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                return KeyboardModifier.KeyboardShiftModifier;
            return KeyboardModifier.KeyboardNoModifier;
        }

        internal static bool IsAltPressed()
        {
            return ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)));
        }

        public bool ProcessMouseAction(Vector3 menuPosition, MouseButton mouseButton, Vector3 autoWalkPos, Thing lookThing, Thing useThing, Creature creatureThing, Creature attackCreature)
        {
            var keyboardModifiers = GetKeyboardModifiers();

            var classicControl = false;
            if (!classicControl)
            {
                if (keyboardModifiers == KeyboardModifier.KeyboardNoModifier && mouseButton == MouseButton.MouseRightButton)
                {
                    CreateThingMenu(menuPosition, lookThing, useThing, creatureThing);
                    return true;
                }
                else if (lookThing != null && keyboardModifiers == KeyboardModifier.KeyboardShiftModifier && (mouseButton == MouseButton.MouseLeftButton || mouseButton == MouseButton.MouseRightButton))
                {
                    LocalPlayer.Look(lookThing);
                    return true;
                }
                else if (useThing != null && keyboardModifiers == KeyboardModifier.KeyboardCtrlModifier && (mouseButton == MouseButton.MouseLeftButton || mouseButton == MouseButton.MouseRightButton))
                {
                    if (useThing.ThingType.IsContainer)
                    {
                        if (useThing.ParentContainer != null)
                        {
                            ContainerSystem.Open((Item)useThing, useThing.ParentContainer);
                        }
                        else
                        {
                            ContainerSystem.Open((Item)useThing, null);
                        }
                        return true;
                    }
                    else if (useThing.ThingType.IsMultiUse)
                    {
                        StartUseWith(useThing);
                        return true;
                    }
                    else
                    {
                        LocalPlayer.Use(useThing);
                        return true;
                    }
                }
                else if (attackCreature != null && IsAltPressed() && (mouseButton == MouseButton.MouseLeftButton || mouseButton == MouseButton.MouseRightButton))
                {
                    LocalPlayer.Attack(attackCreature);
                    return true;
                }
                else if (creatureThing != null && creatureThing.Position.z == (int)autoWalkPos.z && IsAltPressed() && (mouseButton == MouseButton.MouseLeftButton || mouseButton == MouseButton.MouseRightButton))
                {
                    LocalPlayer.Attack(creatureThing);
                    return true;
                }
            }

            // classic control
            else
            {
                if (useThing != null && keyboardModifiers == KeyboardModifier.KeyboardNoModifier && mouseButton == MouseButton.MouseRightButton || !Input.GetMouseButtonUp(0))
                {
                    var player = LocalPlayer.Current;
                    if (attackCreature != null && attackCreature != player)
                    {
                        LocalPlayer.Attack(attackCreature);
                        return true;
                    }
                    else if (creatureThing != null && creatureThing != player && creatureThing.Position.z == autoWalkPos.z)
                    {
                        LocalPlayer.Attack(creatureThing);
                        return true;
                    }
                    else if (useThing.ThingType.IsContainer)
                    {
                        if (useThing.ParentContainer != null)
                        {
                            ContainerSystem.Open((Item)useThing, useThing.ParentContainer);
                            return true;
                        }
                        else
                        {
                            ContainerSystem.Open((Item)useThing, null);
                            return true;
                        }
                    }
                    else if (useThing.ThingType.IsMultiUse)
                    {
                        StartUseWith(useThing);
                        return true;
                    }
                    else
                    {
                        LocalPlayer.Use(useThing);
                        return true;
                    }
                }
                else if (lookThing != null && keyboardModifiers == KeyboardModifier.KeyboardShiftModifier && (mouseButton == MouseButton.MouseLeftButton || mouseButton == MouseButton.MouseRightButton))
                {
                    LocalPlayer.Look(lookThing);
                    return true;
                }
                else if (lookThing != null && ((Input.GetMouseButtonUp(0) && mouseButton == MouseButton.MouseRightButton) && (Input.GetMouseButtonUp(1) || mouseButton == MouseButton.MouseLeftButton)))
                {
                    LocalPlayer.Look(lookThing);
                    return true;
                }
                else if (useThing != null && keyboardModifiers == KeyboardModifier.KeyboardCtrlModifier && (mouseButton == MouseButton.MouseLeftButton || mouseButton == MouseButton.MouseRightButton))
                {
                    CreateThingMenu(menuPosition, lookThing, useThing, creatureThing);
                    return true;
                }
                else if (attackCreature != null && IsAltPressed() && (mouseButton == MouseButton.MouseLeftButton && mouseButton == MouseButton.MouseRightButton))
                {
                    LocalPlayer.Attack(attackCreature);
                    return true;
                }
                else if (creatureThing != null && creatureThing.Position.z == autoWalkPos.z && IsAltPressed() && (mouseButton == MouseButton.MouseLeftButton || mouseButton == MouseButton.MouseRightButton))
                {
                    LocalPlayer.Attack(creatureThing);
                    return true;
                }
            }


            var player1 = LocalPlayer.Current;

            if (autoWalkPos != null && keyboardModifiers == KeyboardModifier.KeyboardNoModifier && mouseButton == MouseButton.MouseLeftButton)
            {
                //player1.AutoWalk(new Vector3((int)autoWalkPos.x, (int)autoWalkPos.y, (int)autoWalkPos.z));
                return true;
            }

            return false;
        }

        private void StartUseWith(Thing useThing)
        {
            throw new NotImplementedException();
        }

        private void CreateThingMenu(Vector3 menuPosition, Thing lookThing, Thing useThing, Creature creatureThing)
        {
            var menu = this;

            var classic = bool.Parse(PlayerPrefs.GetString("classicControl", "false"));
            var shortcut = "";

            if (!classic) { shortcut = "(Shift)"; } else { shortcut = ""; }
            if (lookThing != null)
            {
                menu.AddOption("Look", () => LocalPlayer.Look(lookThing), shortcut);
            }

            if (!classic) { shortcut = "(Ctrl)"; } else { shortcut = ""; }
            if (useThing != null)
            {
                if (useThing.ThingType.IsContainer)
                {
                    if (useThing.ParentContainer != null)
                    {
                        menu.AddOption("Open", () => ContainerSystem.Open((Item)useThing, useThing.ParentContainer), shortcut);
                        menu.AddOption("Open in window", () => ContainerSystem.Open((Item)useThing, null), "");
                    }
                    else
                    {
                        menu.AddOption("Open", () => ContainerSystem.Open((Item)useThing, null), shortcut);
                    }
                }
                else
                {
                    if (useThing.ThingType.IsMultiUse)
                    {
                        menu.AddOption("Use with ...", () => StartUseWith(useThing), shortcut);
                    }
                    else
                    {
                        menu.AddOption("Use", () => LocalPlayer.Use(useThing), shortcut);
                    }
                }

                if (useThing.ThingType.IsRotateable)
                {
                    menu.AddOption("Rotate", () => LocalPlayer.Rotate(useThing), "");
                }

                if (FeatureManager.GetFeature(GameFeature.GameBrowseField) && useThing.Position.x != 0xffff)
                {
                    menu.AddOption("Browse Field", () => ContainerSystem.BrowseField(useThing.Position), "");
                }
            }

            if (lookThing != null && !(lookThing is Creature) && !lookThing.ThingType.IsNotMoveable && lookThing.ThingType.IsPickupable)
            {
                menu.AddSeparator();
                menu.AddOption("Trade with ...", () => StartTradeWith(lookThing), "");
            }

            if (lookThing != null)
            {
                var parentContainer = lookThing.ParentContainer;
                if (parentContainer != null && parentContainer.HasParent)
                {
                    menu.AddOption("Move up", () => ContainerSystem.MoveToParentContainer(lookThing, lookThing.Count), "");
                }
            }
            var localPosition = new Vector3();
            if (creatureThing != null)
            {
                var localPlayer = LocalPlayer.Current;
                menu.AddSeparator();

                if (creatureThing is LocalPlayer)
                {
                    menu.AddOption("Set Outfit", () => OutfitSystem.RequestOutfit());

                    if (FeatureManager.GetFeature(GameFeature.GamePlayerMounts))
                    {
                        if (!localPlayer.IsMounted)
                        {
                            menu.AddOption("Mount", () => localPlayer.Mount());
                        }
                        else
                        {
                            menu.AddOption("Dismount", () => localPlayer.Dismount());
                        }
                    }

                    if (((LocalPlayer)creatureThing).IsPartyMember)
                    {
                        if (((LocalPlayer)creatureThing).IsPartyLeader)
                        {
                            if (((LocalPlayer)creatureThing).IsPartySharedExperienceActive)
                            {
                                menu.AddOption("Disable Shared Experience", () => VipSystem.PartyShareExperience = false);
                            }
                            else
                            {
                                menu.AddOption("Enable Shared Experience", () => VipSystem.PartyShareExperience = true);
                            }
                        }
                        menu.AddOption("Leave Party", () => VipSystem.PartyLeave());
                    }

                }
                else
                {
                    localPosition = localPlayer.Position;
                    if (!classic) { shortcut = "(Alt)"; } else { shortcut = ""; }
                    if (creatureThing.Position.z == localPosition.z)
                    {
                        if (LocalPlayer.AttackingCreature != creatureThing)
                        {
                            menu.AddOption("Attack", () => LocalPlayer.Attack(creatureThing), shortcut);
                        }
                        else
                        {
                            menu.AddOption("Stop Attack", () => LocalPlayer.CancelAttack(), shortcut);
                        }

                        if (LocalPlayer.FollowingCreature != creatureThing)
                        {
                            menu.AddOption("Follow", () => LocalPlayer.Follow(creatureThing));
                        }
                        else
                        {
                            menu.AddOption("Stop Follow", () => LocalPlayer.CancelFollow());
                        }
                    }

                    if (creatureThing is Player)
                    {
                        menu.AddSeparator();
                        var creatureName = creatureThing.Name;

                        menu.AddOption("Message to " + creatureName, () => ConsoleSystem.OpenPrivateChannel(creatureName));
                        if (ConsoleSystem.GetOwnPrivateTab())
                        {
                            menu.AddOption("Invite to private chat", () => ConsoleSystem.InviteToOwnChannel(creatureName));
                            menu.AddOption("Exclude from private chat", () => ConsoleSystem.ExcludeFromOwnChannel(creatureName));
                        }
                        if (!localPlayer.HasVip(creatureName))
                        {
                            menu.AddOption("Add to VIP list", () => VipSystem.AddVip(creatureName));
                        }

                        if (ConsoleSystem.IsIgnored(creatureName))
                        {
                            menu.AddOption("Unignore " + creatureName, () => ConsoleSystem.RemoveIgnoredPlayer(creatureName));
                        }
                        else
                        {
                            menu.AddOption("Ignore " + creatureName, () => ConsoleSystem.AddIgnoredPlayer(creatureName));
                        }

                        var localPlayerShield = (PlayerShields)localPlayer.Shield;
                        var creatureShield = (PlayerShields)creatureThing.Shield;

                        if (localPlayerShield == PlayerShields.ShieldNone && localPlayerShield == PlayerShields.ShieldWhiteBlue)
                        {
                            if (creatureShield == PlayerShields.ShieldWhiteYellow)
                            {
                                menu.AddOption(string.Format("Join {0} Party", creatureThing.Name), () => VipSystem.PartyJoin(creatureThing.Id));
                            }
                            else
                            {
                                menu.AddOption("Invite to Part", () => VipSystem.PartyInvite(creatureThing.Id));
                            }
                        }
                        else if (localPlayerShield == PlayerShields.ShieldWhiteYellow)
                        {
                            if (creatureShield == PlayerShields.ShieldWhiteBlue)
                            {
                                menu.AddOption(string.Format("Revoke {0} Invitation", creatureThing.Name), () => VipSystem.PartyRevokeInvitation(creatureThing.Id));
                            }
                        }
                        else if (localPlayerShield == PlayerShields.ShieldYellow && localPlayerShield == PlayerShields.ShieldYellowSharedExp && localPlayerShield == PlayerShields.ShieldYellowNoSharedExpBlink && localPlayerShield == PlayerShields.ShieldYellowNoSharedExp)
                        {
                            if (creatureShield == PlayerShields.ShieldWhiteBlue)
                            {
                                menu.AddOption(string.Format("Revoke {0} Invitation", creatureThing.Name), () => VipSystem.PartyRevokeInvitation(creatureThing.Id));
                            }
                            else if (creatureShield == PlayerShields.ShieldBlue && creatureShield == PlayerShields.ShieldBlueSharedExp && creatureShield == PlayerShields.ShieldBlueNoSharedExpBlink && creatureShield == PlayerShields.ShieldBlueNoSharedExp)
                            {
                                menu.AddOption(string.Format("Pass Leadership to {0}", creatureThing.Name), () => VipSystem.PartyPassLeadership(creatureThing.Id));
                            }
                            else
                            {
                                menu.AddOption("Invite to Party", () => VipSystem.PartyInvite(creatureThing.Id));
                            }
                        }
                    }
                }
            }
            // if(modules.game_ruleviolation.hasWindowAccess() && creatureThing.IsPlayer()) {
            //   menu.AddSeparator()
            //   menu.AddOption("Rule Violation", ()=> modules.game_ruleviolation.show(creatureThing.Name));
            // }

            menu.AddSeparator();
            menu.AddOption("Copy Name", ()=> GUIUtility.systemCopyBuffer = (creatureThing.Name));
            menu.Visible = true;
            menu.transform.position = menuPosition;
        }

        private void StartTradeWith(Thing lookThing)
        {
            throw new NotImplementedException();
        }

        private void UIContextMenu_VisibilityChange(bool isVisible)
        {
            if (isVisible) OnShow();
            else OnHide();
        }

        public void AddOption(string name, Action callback, string shortcut = "")
        {
            var option = Instantiate(Option, transform);
            option.transform.Find("Label").GetComponent<TMP_Text>().text = name;
            option.transform.Find("Shortcut").GetComponent<TMP_Text>().text = shortcut;
            option.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                callback();
                Visible = false;
                OnHide();
            }));

        }


        public void AddSeparator()
        {
            var option = Instantiate(Separator, transform);
        }

        void OnShow()
        {
            foreach (Transform item in transform)
            {
                item.GetComponent<UIVisibleToggle>().Visible = true;
            }

            transform.SetAsLastSibling();
        }

        void OnHide()
        {
            Clear();
        }
    }
}
