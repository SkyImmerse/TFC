using Assets.Tibia.DAO;
using Game.DAO;
using System;
using System.Collections.Generic;

namespace GameClient.Network
{
    public static class FeatureManager
    {
        private static int ClientVersion => Config.ClientVersion;
        private static int ProtocolVersion => Config.ProtocolVersion;
        private static string ClientSignature;

        private static Dictionary<GameFeature, bool> Features = new Dictionary<GameFeature, bool>();


        public static void Init()
        {
            MessageModeTranslator.buildMessageModesMap(ClientVersion);
            SetClientVersion();
        }

        private static void EnableFeature(GameFeature feature)
        {
            Features[feature] = true;
        }
        private static void DisableFeature(GameFeature feature)
        {
            Features[feature] = false;
        }
        private static void SetFeature(GameFeature feature, bool enabled)
        {
            Features[feature] = enabled;
        }
        public static bool GetFeature(GameFeature feature)
        {
            if (Features.ContainsKey(feature))
            {
                return Features[feature];
            }
            return false;
        }
        public static void SetClientVersion()
        {
            var version = ClientVersion;

            if (version != 0 && (version < 740 || version > 1099))
                throw new Exception(String.Format("Client version %d not supported", version));

            Features.Clear();
            EnableFeature(GameFeature.GameFormatCreatureName);

            if (version >= 770)
            {
                EnableFeature(GameFeature.GameLooktypeU16);
                EnableFeature(GameFeature.GameMessageStatements);
                EnableFeature(GameFeature.GameLoginPacketEncryption);
            }

            if (version >= 780)
            {
                EnableFeature(GameFeature.GamePlayerAddons);
                EnableFeature(GameFeature.GamePlayerStamina);
                EnableFeature(GameFeature.GameNewFluids);
                EnableFeature(GameFeature.GameMessageLevel);
                EnableFeature(GameFeature.GamePlayerStateU16);
                EnableFeature(GameFeature.GameNewOutfitProtocol);
            }

            if (version >= 790)
            {
                EnableFeature(GameFeature.GameWritableDate);
            }

            if (version >= 840)
            {
                EnableFeature(GameFeature.GameProtocolChecksum);
                EnableFeature(GameFeature.GameAccountNames);
                EnableFeature(GameFeature.GameDoubleFreeCapacity);
            }

            if (version >= 841)
            {
                EnableFeature(GameFeature.GameChallengeOnLogin);
                EnableFeature(GameFeature.GameMessageSizeCheck);
            }

            if (version >= 854)
            {
                EnableFeature(GameFeature.GameCreatureEmblems);
            }

            if (version >= 860)
            {
                EnableFeature(GameFeature.GameAttackSeq);
            }

            if (version >= 862)
            {
                EnableFeature(GameFeature.GamePenalityOnDeath);
            }

            if (version >= 870)
            {
                EnableFeature(GameFeature.GameDoubleExperience);
                EnableFeature(GameFeature.GamePlayerMounts);
                EnableFeature(GameFeature.GameSpellList);
            }

            if (version >= 910)
            {
                EnableFeature(GameFeature.GameNameOnNpcTrade);
                EnableFeature(GameFeature.GameTotalCapacity);
                EnableFeature(GameFeature.GameSkillsBase);
                EnableFeature(GameFeature.GamePlayerRegenerationTime);
                EnableFeature(GameFeature.GameChannelPlayerList);
                EnableFeature(GameFeature.GameEnvironmentEffect);
                EnableFeature(GameFeature.GameItemAnimationPhase);
            }

            if (version >= 940)
            {
                EnableFeature(GameFeature.GamePlayerMarket);
            }

            if (version >= 953)
            {
                EnableFeature(GameFeature.GamePurseSlot);
                EnableFeature(GameFeature.GameClientPing);
            }

            if (version >= 960)
            {
                EnableFeature(GameFeature.GameSpritesU32);
                EnableFeature(GameFeature.GameOfflineTrainingTime);
            }

            if (version >= 963)
            {
                EnableFeature(GameFeature.GameAdditionalVipInfo);
            }

            if (version >= 980)
            {
                EnableFeature(GameFeature.GamePreviewState);
                EnableFeature(GameFeature.GameClientVersion);
            }

            if (version >= 981)
            {
                EnableFeature(GameFeature.GameLoginPending);
                EnableFeature(GameFeature.GameNewSpeedLaw);
            }

            if (version >= 984)
            {
                EnableFeature(GameFeature.GameContainerPagination);
                EnableFeature(GameFeature.GameBrowseField);
            }

            if (version >= 1000)
            {
                EnableFeature(GameFeature.GameThingMarks);
                EnableFeature(GameFeature.GamePVPMode);
            }

            if (version >= 1035)
            {
                EnableFeature(GameFeature.GameDoubleSkills);
                EnableFeature(GameFeature.GameBaseSkillU16);
            }

            if (version >= 1036)
            {
                EnableFeature(GameFeature.GameCreatureIcons);
                EnableFeature(GameFeature.GameHideNpcNames);
            }

            if (version >= 1038)
            {
                EnableFeature(GameFeature.GamePremiumExpiration);
            }

            if (version >= 1050)
            {
                EnableFeature(GameFeature.GameEnhancedAnimations);
            }

            if (version >= 1053)
            {
                EnableFeature(GameFeature.GameUnjustifiedPoints);
            }

            if (version >= 1054)
            {
                EnableFeature(GameFeature.GameExperienceBonus);
            }

            if (version >= 1055)
            {
                EnableFeature(GameFeature.GameDeathType);
            }

            if (version >= 1057)
            {
                EnableFeature(GameFeature.GameIdleAnimations);
            }

            if (version >= 1061)
            {
                EnableFeature(GameFeature.GameOGLInformation);
            }

            if (version >= 1071)
            {
                EnableFeature(GameFeature.GameContentRevision);
            }

            if (version >= 1072)
            {
                EnableFeature(GameFeature.GameAuthenticator);
            }

            if (version >= 1074)
            {
                EnableFeature(GameFeature.GameSessionKey);
            }

            if (version >= 1080)
            {
                EnableFeature(GameFeature.GameIngameStore);
            }

            if (version >= 1092)
            {
                EnableFeature(GameFeature.GameIngameStoreServiceType);
            }

            if (version >= 1093)
            {
                EnableFeature(GameFeature.GameIngameStoreHighlights);
            }

            if (version >= 1094)
            {
                EnableFeature(GameFeature.GameAdditionalSkills);
            }

        }

    }
}
