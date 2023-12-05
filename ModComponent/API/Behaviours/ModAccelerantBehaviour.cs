﻿using Il2CppInterop.Runtime.Attributes;
using MelonLoader.TinyJSON;

namespace ModComponent.API.Behaviours;

[MelonLoader.RegisterTypeInIl2Cpp(false)]
public class ModAccelerantBehaviour : ModFireMakingBaseBehaviour
{
	/// <summary>
	/// Is the item destroyed immediately after use?
	/// </summary>
	public bool DestroyedOnUse;

	public ModAccelerantBehaviour(System.IntPtr intPtr) : base(intPtr) { }

	[HideFromIl2Cpp]
	internal override void InitializeBehaviour(ProxyObject dict, string className = "ModAccelerantBehaviour")
	{
		base.InitializeBehaviour(dict, className);
		this.DestroyedOnUse = dict.GetVariant(className, "DestroyedOnUse");
	}
}