import { type LatLngLiteral } from 'leaflet';
import { MovementType, MovementTargetType } from '@/models/strategus';
import { type PartyVisible } from '@/models/strategus/party';
import { type SettlementPublic } from '@/models/strategus/settlement';

import { positionToLatLng } from '@/utils/geometry';

export const useMove = () => {
  const moveDialogCoordinates = ref<LatLngLiteral | null>(null);
  const moveDialogMovementTypes = ref<MovementType[]>([]);

  const moveTargetType = ref<MovementTargetType | null>(null);
  const moveTarget = ref<PartyVisible | SettlementPublic | null>(null);

  const onMoveDialogShown = ({
    target,
    targetType,
    movementTypes,
  }: {
    target: PartyVisible | SettlementPublic;
    targetType: MovementTargetType;
    movementTypes: MovementType[];
  }) => {
    moveTarget.value = target;
    moveTargetType.value = targetType;

    moveDialogCoordinates.value = positionToLatLng(target.position.coordinates);
    moveDialogMovementTypes.value = movementTypes;
  };

  const onMoveDialogCancel = () => {
    moveDialogCoordinates.value = null;
    moveDialogMovementTypes.value = [];
    moveTarget.value = null;
    moveTargetType.value = null;
  };

  return {
    moveTarget,
    moveTargetType,
    //
    moveDialogCoordinates,
    moveDialogMovementTypes,
    //
    onMoveDialogShown,
    onMoveDialogCancel,
  };
};
