using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace LSDZombie
{
    public static class Unitily
    {
        public static string GetPlayerTeam(Entity player)
        {
            return player.GetField<string>("sessionteam");
        }

        public static Entity[] getPlayerList()
        {
            List<Entity> players = new List<Entity>();
            for (int i = 0; i < 17; i++)
            {
                Entity entity = Entity.GetEntity(i);
                if (entity != null)
                {
                    if (entity.IsPlayer)
                    {
                        players.Add(entity);
                    }
                }
            }
            return players.ToArray();
        }
    }
}
