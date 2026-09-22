using UnityEngine;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public interface ITargetResolver
    {
        Entity ResolveTarget(Entity originEntity);
    }

    public class NearestEnemyTargetResolver : ITargetResolver
    {
        public Entity ResolveTarget(Entity originEntity)
        {
            if (originEntity == null || !originEntity.IsAlive) return null;

            Entity[] allEntities = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude);
            Entity closest = null;
            float minDistance = float.MaxValue;

            foreach (var entity in allEntities)
            {
                if (entity == originEntity || !entity.IsAlive) continue;

                // Opposing team check (Hero targets Monster, Monster targets Hero)
                bool isOpposing = (originEntity is Hero && entity is Monster) ||
                                  (originEntity is Monster && entity is Hero) ||
                                  (originEntity.EntityType != entity.EntityType);
                if (isOpposing)
                {
                    float dist = Vector3.Distance(originEntity.transform.position, entity.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closest = entity;
                    }
                }
            }

            return closest;
        }
    }
}
