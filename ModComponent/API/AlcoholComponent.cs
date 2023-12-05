﻿using UnityEngine;

namespace ModComponent.API;

[MelonLoader.RegisterTypeInIl2Cpp(false)]
public class AlcoholComponent : MonoBehaviour
{
	public float AmountTotal;
	public float AmountRemaining;
	public float UptakeSeconds;
	public AlcoholComponent(IntPtr intPtr) : base(intPtr) { }
}
