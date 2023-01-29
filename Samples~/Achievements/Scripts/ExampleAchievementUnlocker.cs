using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Example Achievement Unlocker",
        menuName = "SO Architecture/Example/Example Achievement Unlocker")]
    public class ExampleAchievementUnlocker : ScriptableObject, IResetOnExitPlayMode {
        public event Action OnExampleAchievementProgressed;

        public static ExampleAchievementUnlocker Instance;

        public AchievementUnlockedGameEvent achievementUnlockedGameEvent;

        public int clicks = 0;
        public int minUnlockClicks = 5;
        [HideLabel]
        [ReadOnly]
        public ObjSet<ExampleAchievement> unlockedAchievements = new ObjSet<ExampleAchievement>() { name = "Unlocked Achievements" };
        public void OnClicked(ExampleAchievement achievement) {
            ++clicks;
            OnExampleAchievementProgressed?.Invoke();
            if (clicks >= minUnlockClicks) {
                unlockedAchievements.Add(achievement);
                if (achievementUnlockedGameEvent) {
                    achievementUnlockedGameEvent.Raise(achievement);
                }
            }
        }

        private void OnEnable() {
            Instance = this;
        }
    }
}
