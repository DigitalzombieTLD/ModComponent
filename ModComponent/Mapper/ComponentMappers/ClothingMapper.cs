﻿using Il2Cpp;
using ModComponent.API.Components;

namespace ModComponent.Mapper.ComponentMappers;

internal static class ClothingMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		ModClothingComponent modClothingItem = modComponent.TryCast<ModClothingComponent>();
		if (modClothingItem == null)
		{
			return;
		}

		ClothingItem clothingItem = ModComponent.Utils.ComponentUtils.GetOrCreateComponent<ClothingItem>(modClothingItem);

		clothingItem.m_DailyHPDecayWhenWornInside = ItemMapper.GetDecayPerStep(modClothingItem.DaysToDecayWornInside, modClothingItem.MaxHP);
		clothingItem.m_DailyHPDecayWhenWornOutside = ItemMapper.GetDecayPerStep(modClothingItem.DaysToDecayWornOutside, modClothingItem.MaxHP);
		clothingItem.m_DryBonusWhenNotWorn = 1.5f;
		clothingItem.m_DryPercentPerHour = 100f / modClothingItem.HoursToDryNearFire;
		clothingItem.m_DryPercentPerHourNoFire = 100f / modClothingItem.HoursToDryWithoutFire;
		clothingItem.m_FreezePercentPerHour = 100f / modClothingItem.HoursToFreeze;

		clothingItem.m_Region = ModComponent.Utils.EnumUtils.TranslateEnumValue<ClothingRegion, ModClothingComponent.BodyRegion>(modClothingItem.Region);
		clothingItem.m_MaxLayer = modClothingItem.MaxLayer;
		clothingItem.m_MinLayer = modClothingItem.MinLayer;
		// Made obsolete in 2.17
		//clothingItem.m_FootwearType = modClothingItem.Footwear;

		clothingItem.m_PaperDollTextureName = modClothingItem.MainTexture;
		clothingItem.m_PaperDollBlendmapName = modClothingItem.BlendTexture;

		clothingItem.m_Warmth = modClothingItem.Warmth;
		clothingItem.m_WarmthWhenWet = modClothingItem.WarmthWhenWet;
		clothingItem.m_Waterproofness = modClothingItem.Waterproofness / 100f;
		clothingItem.m_Windproof = modClothingItem.Windproof;
		clothingItem.m_SprintBarReductionPercent = modClothingItem.SprintBarReduction;
		clothingItem.m_Toughness = modClothingItem.Toughness;

		// FPS clothing
		clothingItem.m_FirstPersonPrefabFemale = (string.IsNullOrEmpty(modClothingItem.FirstPersonPrefabMale)) ? null : new AssetReferenceFirstPersonClothing(modClothingItem.FirstPersonPrefabMale);
		clothingItem.m_FirstPersonPrefabMale = (string.IsNullOrEmpty(modClothingItem.FirstPersonPrefabFemale)) ? null : new AssetReferenceFirstPersonClothing(modClothingItem.FirstPersonPrefabFemale);

		// PreventAllDamageFromSource (DamageReason)
		clothingItem.m_PreventAllDamageFromSource = modClothingItem.PreventAllDamageFromSource;



		ConfigureWolfIntimidation(modClothingItem);
	}

	private static void ConfigureWolfIntimidation(ModClothingComponent modClothingItem)
	{
		if (modClothingItem.DecreaseAttackChance != 0 || modClothingItem.IncreaseFleeChance != 0)
		{
			WolfIntimidationBuff wolfIntimidationBuff = ModComponent.Utils.ComponentUtils.GetOrCreateComponent<WolfIntimidationBuff>(modClothingItem);
			wolfIntimidationBuff.m_DecreaseAttackChancePercentagePoints = modClothingItem.DecreaseAttackChance;
			wolfIntimidationBuff.m_IncreaseFleePercentagePoints = modClothingItem.IncreaseFleeChance;
		}
	}
}