using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarshipLaunchExpansion.Modules
{
	public class ModuleSLESequentialAnimate : PartModule
	{
		// Fields
		public static string MODULENAME = "ModuleSLESequentialAnimate";

		[KSPField]
		public string RetractActionName = "RetractActionName";

		[KSPField]
		public string ExtendActionName = "ExtendActionName";

		[KSPField]
		public string Animations = "";

		private List<ModuleSLEAnimate> animations = new List<ModuleSLEAnimate>();
		private bool isMoving = false;

		// UI Stuff
		[KSPAction(activeEditor = true, guiName = "RetractActionName")]
		public void RetractAction(KSPActionParam param) => StartCoroutine(routine: Retract());

		[KSPAction(activeEditor = true, guiName = "ExtendActionName")]
		public void ExtendAction(KSPActionParam param) => StartCoroutine(routine: Extend());

		[KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "RetractActionName")]
		public void RetractEvent() => StartCoroutine(routine: Retract());

		[KSPEvent(guiActive = true, guiActiveEditor = true,  guiName = "ExtendActionName")]
		public void ExtendEvent() => StartCoroutine(routine: Extend());

		public void Start()
		{
			SetUI();
			var anims = part.Modules.GetModules<ModuleSLEAnimate>();

			foreach (var anim in Animations.Split(';'))
			{
				var matchedAnim = anims.Find(a => a.moduleID == anim);
				if (matchedAnim != null)
				{
					animations.Add(matchedAnim);
				}
				else
				{
					Debug.LogError($"[{MODULENAME}] Animation with moduleId {anim} not found!");
					this.enabled = false;
					break;
				}
			}
		}

		public void FixedUpdate()
		{
			if (Events["RetractEvent"].guiActive && !animations.Any(a => a.ExtensionLimit != 0))
			{
				Events["RetractEvent"].guiActive = false;
				Events["RetractEvent"].guiActiveEditor = false;
			}
			else if (!Events["RetractEvent"].guiActive && animations.Any(a => a.ExtensionLimit != 0) && !isMoving)
			{
				Events["RetractEvent"].guiActive = true;
				Events["RetractEvent"].guiActiveEditor = true;
			}

			if (Events["ExtendEvent"].guiActive && !animations.Any(a => a.ExtensionLimit != a.MaxExtension))
			{
				Events["ExtendEvent"].guiActive = false;
				Events["ExtendEvent"].guiActiveEditor = false;
			}
			else if (!Events["ExtendEvent"].guiActive && animations.Any(a => a.ExtensionLimit != a.MaxExtension) && !isMoving)
			{
				Events["ExtendEvent"].guiActive = true;
				Events["ExtendEvent"].guiActiveEditor = true;
			}
		}

		private void SetUI()
		{
			Events["RetractEvent"].guiName = RetractActionName;
			Events["ExtendEvent"].guiName = ExtendActionName;
			Actions["RetractAction"].guiName = RetractActionName;
			Actions["ExtendAction"].guiName = ExtendActionName;
		}

		public System.Collections.IEnumerator Extend()
		{
			isMoving = true;
			Events["ExtendEvent"].guiActive = false;
			Events["ExtendEvent"].guiActiveEditor = false;
			Events["RetractEvent"].guiActive = true;
			Events["RetractEvent"].guiActiveEditor = true;
			for (int i = 0; i < animations.Count; i++)
			{
				animations[i].Events["Button3"].Invoke();

				// Wait two frames,
				// because if the animation was going in the other direction it will be stopped first before reversing.
				yield return new WaitForFixedUpdate();
				yield return new WaitForFixedUpdate();

				yield return new WaitUntil(() => animations[i].AnimationState == "Stopped");

				if (animations[i].ExtensionLimit != animations[i].MaxExtension)
				{
					Debug.Log($"[{MODULENAME}] Animation {animations[i].moduleID} stopped before reaching the target ({animations[i].ExtensionLimit}) expected {animations[i].MaxExtension}, cancelling.");
					isMoving = false;
					break;
				}
			}
			isMoving = false;
		}

		public System.Collections.IEnumerator Retract()
		{
			isMoving = true;
			Events["ExtendEvent"].guiActive = true;
			Events["ExtendEvent"].guiActiveEditor = true;
			Events["RetractEvent"].guiActive = false;
			Events["RetractEvent"].guiActiveEditor = false;
			for (int i = animations.Count - 1; i >= 0; i--)
			{
				animations[i].Events["Button2"].Invoke();

				// Wait two frames,
				// because if the animation was going in the other direction it will be stopped first before reversing.
				yield return new WaitForFixedUpdate();
				yield return new WaitForFixedUpdate();

				yield return new WaitUntil(() => animations[i].AnimationState == "Stopped");

				if (animations[i].ExtensionLimit != 0)
				{
					Debug.Log($"[{MODULENAME}] Animation {animations[i].moduleID} stopped before reaching the target ({animations[i].ExtensionLimit}) expected 0, cancelling.");
					isMoving = false;
					break;
				}
			}
			isMoving = false;
		}
	}
}
