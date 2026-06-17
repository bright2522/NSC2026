using System;
using System.Collections.Generic;

namespace Pep.Planning
{
    public enum PepPriority
    {
        P1,
        P2,
        P3
    }

    public enum PepTaskStatus
    {
        NotStarted,
        InProgress,
        Blocked,
        Done
    }

    [Serializable]
    public class PepTaskEntry
    {
        public int step;
        public string id;
        public string title;
        public string folder;
        public PepPriority priority;
        public string deadline;
        public PepTaskStatus status;
        public int progressPercent;
        public string notes;
        public string[] dependsOnFriends;
    }

    public static class PepPlanningData
    {
        public const string Today = "2026-06-14";
        public const string UpdatedAt = "2026-06-14";
        public const string Role = "Programmer — coding + UI mock from code (no scene setup)";

        public static readonly List<PepTaskEntry> Tasks = new List<PepTaskEntry>
        {
            new PepTaskEntry
            {
                step = 0,
                id = "PEP-000",
                title = "Research + folder setup",
                folder = "pep/",
                priority = PepPriority.P1,
                deadline = "2026-06-13",
                status = PepTaskStatus.Done,
                progressPercent = 100,
                notes = "scan Assets/, จัด pep/Scripts/",
                dependsOnFriends = Array.Empty<string>()
            },

            new PepTaskEntry
            {
                step = 1,
                id = "PEP-001",
                title = "GameStateMachine (code only — enum/events, ไม่โหลด scene)",
                folder = "pep/Scripts/Core/",
                priority = PepPriority.P1,
                deadline = "2026-06-13",
                status = PepTaskStatus.Done,
                progressPercent = 100,
                notes = "เสร็จ: state enum + event/callback + transition API",
                dependsOnFriends = new[] { "bright/GameManager.cs", "coco/CookingGameManager.cs" }
            },

            new PepTaskEntry
            {
                step = 2,
                id = "PEP-002",
                title = "PlayerDataManager — ScriptableObject + Save/Load",
                folder = "pep/Scripts/Core/",
                priority = PepPriority.P1,
                deadline = "2026-06-14",
                status = PepTaskStatus.Done,
                progressPercent = 100,
                notes = "เสร็จ: save/load ด้วย PlayerPrefs + data snapshot",
                dependsOnFriends = Array.Empty<string>()
            },

            new PepTaskEntry
            {
                step = 3,
                id = "PEP-003",
                title = "InventoryManager — logic sync Smart Fridge",
                folder = "pep/Scripts/SmartFridge/",
                priority = PepPriority.P1,
                deadline = "2026-06-14",
                status = PepTaskStatus.Done,
                progressPercent = 100,
                notes = "เสร็จ: stock logic + consume + sync selected ingredients",
                dependsOnFriends = new[] { "bright/SmartFridgeManager.cs" }
            },

            new PepTaskEntry
            {
                step = 4,
                id = "PEP-004",
                title = "ScoringManager — รับคะแนนจาก minigame",
                folder = "pep/Scripts/Scoring/",
                priority = PepPriority.P1,
                deadline = "2026-06-15",
                status = PepTaskStatus.Done,
                progressPercent = 100,
                notes = "เสร็จ: report step score + sync totalRecipeScore + final average",
                dependsOnFriends = new[] { "coco/CookingGameManager.cs" }
            },

            new PepTaskEntry
            {
                step = 5,
                id = "PEP-005",
                title = "IngredientSO + RecipeSO + RecipeCatalogManager + Editor",
                folder = "pep/Scripts/Recipe/",
                priority = PepPriority.P1,
                deadline = "2026-06-15",
                status = PepTaskStatus.Done,
                progressPercent = 100,
                notes = "เสร็จ: data SO + catalog lookup by id + custom inspector",
                dependsOnFriends = new[] { "bright/RecipeSelectionManager.cs", "coco/RecipeDatabase.cs" }
            },

            new PepTaskEntry
            {
                step = 6,
                id = "PEP-006",
                title = "Integration adapters — bright / coco / folk",
                folder = "pep/Scripts/Integration/",
                priority = PepPriority.P1,
                deadline = "2026-06-16",
                status = PepTaskStatus.NotStarted,
                progressPercent = 0,
                notes = "wrapper เรียก API เพื่อน ไม่แก้ folder เพื่อน",
                dependsOnFriends = new[] { "bright/", "coco/", "folk/" }
            },

            new PepTaskEntry
            {
                step = 7,
                id = "PEP-007",
                title = "Smart Fridge — recommendation + ShopNearby",
                folder = "pep/Scripts/SmartFridge/",
                priority = PepPriority.P1,
                deadline = "2026-06-17",
                status = PepTaskStatus.NotStarted,
                progressPercent = 10,
                notes = "Greedy + content-based + static shop data",
                dependsOnFriends = new[] { "bright/SmartFridgeManager.cs" }
            },

            new PepTaskEntry
            {
                step = 8,
                id = "PEP-008",
                title = "CookingState — gas fire Slider + ProgressBar + Timer + Shake + UI mock",
                folder = "pep/Scripts/Minigames/Cooking/",
                priority = PepPriority.P1,
                deadline = "2026-06-13",
                status = PepTaskStatus.Done,
                progressPercent = 100,
                notes = "เสร็จ: gas slider + cook progress + timer + shake + runtime UI",
                dependsOnFriends = new[] { "coco/CookingMinigameBase.cs", "coco/GrillingMinigame.cs" }
            },

            new PepTaskEntry
            {
                step = 9,
                id = "PEP-009",
                title = "PreparationState — drag logic + UI mock",
                folder = "pep/Scripts/Minigames/Preparation/",
                priority = PepPriority.P1,
                deadline = "2026-06-18",
                status = PepTaskStatus.NotStarted,
                progressPercent = 0,
                notes = "drag & drop วัตถุดิบ — code only",
                dependsOnFriends = new[] { "coco/CookingGameManager.cs" }
            },

            new PepTaskEntry
            {
                step = 10,
                id = "PEP-010",
                title = "PresentationState — plating logic + UI mock",
                folder = "pep/Scripts/Minigames/Presentation/",
                priority = PepPriority.P1,
                deadline = "2026-06-20",
                status = PepTaskStatus.NotStarted,
                progressPercent = 0,
                notes = "จัดจาน — UI mock จาก code",
                dependsOnFriends = new[] { "coco/CookingGameManager.cs" }
            },

            new PepTaskEntry
            {
                step = 11,
                id = "PEP-011",
                title = "Chopping — adapter / UI mock",
                folder = "pep/Scripts/Minigames/Chopping/",
                priority = PepPriority.P1,
                deadline = "2026-06-13",
                status = PepTaskStatus.Blocked,
                progressPercent = 0,
                notes = "รอทีมเลือก folk 3D vs coco slider",
                dependsOnFriends = new[] { "coco/ChoppingMinigame.cs", "folk/SliceableFood.cs" }
            },

            new PepTaskEntry
            {
                step = 12,
                id = "PEP-012",
                title = "NutritionDisplayUI + Post-Cook Evaluation",
                folder = "pep/Scripts/Nutrition/",
                priority = PepPriority.P2,
                deadline = "2026-06-21",
                status = PepTaskStatus.NotStarted,
                progressPercent = 0,
                notes = "kcal, benefits, stars — UI mock",
                dependsOnFriends = new[] { "coco/CookingGameUI.cs" }
            },

            new PepTaskEntry
            {
                step = 13,
                id = "PEP-013",
                title = "RewardManager + Result screen UI mock",
                folder = "pep/Scripts/Reward/",
                priority = PepPriority.P2,
                deadline = "2026-06-22",
                status = PepTaskStatus.NotStarted,
                progressPercent = 0,
                notes = "สรุปผล + reward logic",
                dependsOnFriends = new[] { "pep/Scripts/Scoring/" }
            },

            new PepTaskEntry
            {
                step = 14,
                id = "PEP-014",
                title = "Shop + Equip (code + UI mock)",
                folder = "pep/Scripts/Reward/",
                priority = PepPriority.P3,
                deadline = "2026-06-25",
                status = PepTaskStatus.NotStarted,
                progressPercent = 0,
                notes = "P3",
                dependsOnFriends = Array.Empty<string>()
            },

            new PepTaskEntry
            {
                step = 15,
                id = "PEP-015",
                title = "Community — Feed, Review, Like/Comment mock",
                folder = "pep/Scripts/Community/",
                priority = PepPriority.P3,
                deadline = "2026-06-28",
                status = PepTaskStatus.NotStarted,
                progressPercent = 0,
                notes = "P3 — local mock",
                dependsOnFriends = Array.Empty<string>()
            },

            new PepTaskEntry
            {
                step = 16,
                id = "PEP-016",
                title = "Competition 1v1",
                folder = "pep/Scripts/Competition/",
                priority = PepPriority.P3,
                deadline = "2026-06-30",
                status = PepTaskStatus.NotStarted,
                progressPercent = 0,
                notes = "P3",
                dependsOnFriends = Array.Empty<string>()
            }
        };

        public static List<PepTaskEntry> GetTodayTasks()
        {
            var result = new List<PepTaskEntry>();
            foreach (PepTaskEntry task in Tasks)
            {
                if (task.deadline == Today && task.status != PepTaskStatus.Done)
                    result.Add(task);
            }
            return result;
        }

        public static List<PepTaskEntry> GetByPriority(PepPriority priority)
        {
            var result = new List<PepTaskEntry>();
            foreach (PepTaskEntry task in Tasks)
            {
                if (task.priority == priority)
                    result.Add(task);
            }
            return result;
        }

        public static int GetOverallProgressPercent()
        {
            if (Tasks.Count == 0) return 0;
            int sum = 0;
            foreach (PepTaskEntry task in Tasks)
                sum += task.progressPercent;
            return sum / Tasks.Count;
        }
    }
}
