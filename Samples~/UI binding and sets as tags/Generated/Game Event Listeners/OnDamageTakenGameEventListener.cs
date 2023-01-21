////////////////////////////////////////////////////////////////////
/////////////////// AUTOMATICALLY GENERATED FILE ///////////////////
////////////////////////////////////////////////////////////////////

using ExtEvents;

namespace Vaflov {
	public class OnDamageTakenGameEventListener : GameEventListener<OnDamageTakenGameEvent, ExampleDamageable, float> {
		[EventArguments("Damageable",  "Health Pct")]
		public ExtEvent<ExampleDamageable, float> response;
		public override ExtEvent<ExampleDamageable, float> Response => response;
	}
}
