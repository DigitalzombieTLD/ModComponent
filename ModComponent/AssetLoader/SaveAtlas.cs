﻿using Il2Cpp;
using UnityEngine;

namespace ModComponent.AssetLoader;

[MelonLoader.RegisterTypeInIl2Cpp(false)]
internal class SaveAtlas : MonoBehaviour
{
	public UIAtlas? original;

	public SaveAtlas(IntPtr intPtr) : base(intPtr) { }
}
