﻿using ModComponentAPI;
using ModComponentUtils;
using UnityEngine;

namespace ModComponentMapper.ComponentMapper
{
	internal static class AccelerantMapper
	{
		internal static void Configure(ModComponent modComponent) => Configure(ComponentUtils.GetGameObject(modComponent));
		public static void Configure(GameObject prefab)
		{
			ModAccelerantComponent modAccelerantComponent = ComponentUtils.GetComponent<ModAccelerantComponent>(prefab);
			if (modAccelerantComponent == null) return;

			FireStarterItem fireStarterItem = ComponentUtils.GetOrCreateComponent<FireStarterItem>(modAccelerantComponent);

			fireStarterItem.m_IsAccelerant = true;
			fireStarterItem.m_FireStartDurationModifier = modAccelerantComponent.DurationOffset;
			fireStarterItem.m_FireStartSkillModifier = modAccelerantComponent.SuccessModifier;
			fireStarterItem.m_ConsumeOnUse = modAccelerantComponent.DestroyedOnUse;
		}
	}
}