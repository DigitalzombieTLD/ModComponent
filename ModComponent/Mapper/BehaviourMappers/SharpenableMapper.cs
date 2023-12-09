﻿using Il2Cpp;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class SharpenableMapper
{
	internal static void Configure(ModBaseComponent modComponent) => Configure(ComponentUtils.GetGameObject(modComponent));
	public static void Configure(GameObject prefab)
	{
		ModSharpenableBehaviour modSharpenableComponent = ComponentUtils.GetComponentSafe<ModSharpenableBehaviour>(prefab);
		if (modSharpenableComponent == null)
		{
			return;
		}

		Sharpenable sharpenable = ComponentUtils.GetOrCreateComponent<Sharpenable>(modSharpenableComponent);

		sharpenable.m_ConditionIncreaseMax = modSharpenableComponent.ConditionMax;
		sharpenable.m_ConditionIncreaseMin = modSharpenableComponent.ConditionMin;
		sharpenable.m_DurationMinutesMax = modSharpenableComponent.MinutesMax;
		sharpenable.m_DurationMinutesMin = modSharpenableComponent.MinutesMin;

		sharpenable.m_SharpenToolChoices = ModUtils.GetItems<ToolsItem>(modSharpenableComponent.Tools, prefab.name + ": Tools");
		sharpenable.m_RequiresToolToSharpen = sharpenable.m_SharpenToolChoices.Count() > 0;
		sharpenable.m_SharpenAudio = modSharpenableComponent.Audio;
	}
}
