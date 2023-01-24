////////////////////////////////////////////////////////////////////
/////////////////// AUTOMATICALLY GENERATED FILE ///////////////////
////////////////////////////////////////////////////////////////////

using ExtEvents;

namespace Vaflov {
	public class AchievementUnlockedGameEventListener : GameEventListener<AchievementUnlockedGameEvent, ExampleAchievement> {
		[EventArguments("Achievement")]
		public ExtEvent<ExampleAchievement> response;
		public override ExtEvent<ExampleAchievement> Response => response;
	}
}
