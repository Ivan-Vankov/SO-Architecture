////////////////////////////////////////////////////////////////////
/////////////////// AUTOMATICALLY GENERATED FILE ///////////////////
////////////////////////////////////////////////////////////////////

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Vaflov {
	public delegate void AchievementUnlockedAction(ExampleAchievement achievement);

	public class AchievementUnlockedGameEvent : GameEvent1Base<AchievementUnlockedGameEvent, ExampleAchievement> {
		public event AchievementUnlockedAction action;

		#if ODIN_INSPECTOR
		[Button]
		[DrawButtonTypes]
		[PropertyOrder(15)]
		#endif
		public override void Raise(ExampleAchievement achievement) {
			action?.Invoke(achievement);
		}

		public override string EditorToString() {
			return "(achievement)";
		}

		public override void AddListener(GameEventListener<AchievementUnlockedGameEvent, ExampleAchievement> listener) {
			#if UNITY_EDITOR
			base.AddListener(listener);
			#endif
			action += listener.CallResponse;
		}

		public override void RemoveListener(GameEventListener<AchievementUnlockedGameEvent, ExampleAchievement> listener) {
			#if UNITY_EDITOR
			base.RemoveListener(listener);
			#endif
			action -= listener.CallResponse;
		}
	}
}
