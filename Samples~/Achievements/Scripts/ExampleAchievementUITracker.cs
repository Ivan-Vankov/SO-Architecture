#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Vaflov {
    public class ExampleAchievementUITracker : MonoBehaviour {
        public Text text;
        #if ODIN_INSPECTOR
        [Required]
        #endif
        public ExampleAchievementUnlocker achievementUnlocker;

        public GameObject achievementRoot;
        public Text achievementTitle;
        public Text achievementDescription;
        public Image achievementImage;

        public void SetAchievementProgressText() {
            var clicksLeft = achievementUnlocker.minUnlockClicks - achievementUnlocker.clicks;
            if (clicksLeft <= 0) {
                text.text = $"Achievement unlocked!";
            } else {
                text.text = $"Click {clicksLeft} more times\nto unlock the achievement";
            }
        }

        public void ShowAchievement(ExampleAchievement achievement) {
            achievementTitle.text = achievement.achievementName;
            achievementDescription.text = achievement.achievementDescription;
            achievementImage.sprite = achievement.achievementImage;
            achievementRoot.SetActive(true);
        }

        private void Start() {
            SetAchievementProgressText();
        }

        private void OnEnable() {
            achievementUnlocker.OnExampleAchievementProgressed += SetAchievementProgressText;
        }

        private void OnDisable() {
            achievementUnlocker.OnExampleAchievementProgressed -= SetAchievementProgressText;
        }
    }
}