using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches;

public class MissionNetworkComponentPatch
{
    public static bool Prefix(MissionNetworkComponent __instance, NetworkCommunicator? networkPeer)
    {
        if (networkPeer == null)
        {
            return false;
        }

        foreach (var missionObject in __instance.Mission.MissionObjects)
        {
            if (missionObject is SpawnedItemEntity spawnedItemEntity)
            {
                GameEntity gameEntity = spawnedItemEntity.GameEntity;
                if (gameEntity.Parent != null && gameEntity.Parent.HasScriptOfType<SpawnedItemEntity>())
                {
                    continue;
                }

                MissionObject? missionObject2 = null;
                if (gameEntity.Parent != null)
                {
                    missionObject2 = gameEntity.Parent.GetFirstScriptOfType<MissionObject>();
                }

                MatrixFrame matrixFrame = gameEntity.GetGlobalFrame();
                if (missionObject2 != null)
                {
                    matrixFrame = missionObject2.GameEntity.GetGlobalFrame().TransformToLocalNonOrthogonal(ref matrixFrame);
                }

                matrixFrame.origin.z = MathF.Max(matrixFrame.origin.z,
                    CompressionBasic.PositionCompressionInfo.GetMinimumValue() + 1f);
                Mission.WeaponSpawnFlags weaponSpawnFlags = spawnedItemEntity.SpawnFlags;
                if (weaponSpawnFlags.HasAnyFlag(Mission.WeaponSpawnFlags.WithPhysics) && !gameEntity.GetPhysicsState())
                {
                    weaponSpawnFlags = (weaponSpawnFlags & ~Mission.WeaponSpawnFlags.WithPhysics) | Mission.WeaponSpawnFlags.WithStaticPhysics;
                }

                bool hasLifeTime = true;
                bool isVisible = gameEntity.Parent == null || missionObject2 != null;
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new SpawnWeaponWithNewEntity(spawnedItemEntity.WeaponCopy,
                    weaponSpawnFlags, spawnedItemEntity.Id.Id, matrixFrame, missionObject2, isVisible,
                    hasLifeTime));
                GameNetwork.EndModuleEventAsServer();
                for (int i = 0; i < spawnedItemEntity.WeaponCopy.GetAttachedWeaponsCount(); i++)
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new AttachWeaponToSpawnedWeapon(
                        spawnedItemEntity.WeaponCopy.GetAttachedWeapon(i), spawnedItemEntity,
                        spawnedItemEntity.WeaponCopy.GetAttachedWeaponFrame(i)));
                    GameNetwork.EndModuleEventAsServer();

                    // Whole load of null checks to see what the issue is
                    if (!spawnedItemEntity.WeaponCopy.GetAttachedWeapon(i).Item.ItemFlags.HasAnyFlag(ItemFlags.CanBePickedUpFromCorpse))
                    {
                        continue;
                    }

                    if (gameEntity.GetChild(i) == null)
                    {
                        continue;
                    }

                    if (gameEntity.GetChild(i).GetFirstScriptOfType<SpawnedItemEntity>() == null)
                    {
                        continue;
                    }

                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new SpawnAttachedWeaponOnSpawnedWeapon(spawnedItemEntity, i,
                            gameEntity.GetChild(i).GetFirstScriptOfType<SpawnedItemEntity>().Id.Id));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
            else if (missionObject.CreatedAtRuntime)
            {
                Mission.DynamicallyCreatedEntity? dynamicallyCreatedEntity = __instance.Mission.AddedEntitiesInfo.SingleOrDefault(x => x.ObjectId == missionObject.Id);
                if (dynamicallyCreatedEntity != null)
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new CreateMissionObject(dynamicallyCreatedEntity.ObjectId,
                        dynamicallyCreatedEntity.Prefab, dynamicallyCreatedEntity.Frame,
                        dynamicallyCreatedEntity.ChildObjectIds));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
        }

        return false;
    }
}
