////////////////////////////////////////////////////////////////////
/////////////////// AUTOMATICALLY GENERATED FILE ///////////////////
////////////////////////////////////////////////////////////////////

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Vaflov {
	public delegate void OnDamageTakenAction(ExampleDamageable damageable, float healthPct);

	public class OnDamageTakenGameEvent : GameEvent2Base<OnDamageTakenGameEvent, ExampleDamageable, float> {
		public event OnDamageTakenAction action;

		#if ODIN_INSPECTOR
		[Button]
		[DrawButtonTypes]
		[PropertyOrder(15)]
		#endif
		public override void Raise(ExampleDamageable damageable, float healthPct) {
			action?.Invoke(damageable,  healthPct);
		}

		public override string EditorToString() {
			return "(damageable,  healthPct)";
		}

		public override void AddListener(GameEventListener<OnDamageTakenGameEvent, ExampleDamageable, float> listener) {
			#if UNITY_EDITOR
			base.AddListener(listener);
			#endif
			action += listener.CallResponse;
		}

		public override void RemoveListener(GameEventListener<OnDamageTakenGameEvent, ExampleDamageable, float> listener) {
			#if UNITY_EDITOR
			base.RemoveListener(listener);
			#endif
			action -= listener.CallResponse;
		}
	}
}
