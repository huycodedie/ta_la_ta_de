using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public enum StatusRemovalCategory
    {
        None = 0,
        Buff = 1,
        Debuff = 2,
        DoT = 3,
        CrowdControl = 4,
        NegativeStatus = 5,
        PositiveStatus = 6,
        SpecificStatus = 7,
        SpecificStatusType = 8,
        Shield = 9
    }

    public enum StatusSelectionMode
    {
        Oldest = 1,
        Newest = 2,
        Random = 3,
        HighestPriority = 4,
        LowestPriority = 5,
        All = 6
    }

    public struct StatusRemovalResult
    {
        public bool Success;
        public int StatusesRemovedCount;
        public int StacksRemovedCount;
        public List<string> RemovedStatusIds;
        public string Message;

        public static StatusRemovalResult None => new StatusRemovalResult
        {
            Success = false,
            StatusesRemovedCount = 0,
            StacksRemovedCount = 0,
            RemovedStatusIds = new List<string>(),
            Message = "No statuses removed."
        };
    }
}
