import { get } from '@/services/crpg-client';
import { GameServerStats } from '@/models/game-server-stats';

export const getGameServerStats = () => get<GameServerStats>('/game-server-statistics');
