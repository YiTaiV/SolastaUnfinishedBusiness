﻿using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using static SolastaUnfinishedBusiness.Builders.Features.AutoPreparedSpellsGroupBuilder;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAdditionalDamages;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionMagicAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class RangerArcanist : AbstractSubclass
{
    private const string ArcanistMarkTag = "ArcanistMark";

    internal RangerArcanist()
    {
        var conditionMarkedByArcanist = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionMarkedByBrandingSmite, "ConditionMarkedByArcanist")
            .SetOrUpdateGuiPresentation(Category.Condition)
            .SetAllowMultipleInstances(false)
            .SetDuration(DurationType.Permanent)
            .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
            .SetPossessive()
            .SetSpecialDuration(true)
            .AddToDB();

        //
        // LEVEL 03
        //

        var autoPreparedSpellsArcanist = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create("AutoPreparedSpellsArcanist")
            .SetGuiPresentation(Category.Feature)
            .SetSpellcastingClass(CharacterClassDefinitions.Ranger)
            .SetPreparedSpellGroups(
                BuildSpellGroup(2, Shield),
                BuildSpellGroup(5, MistyStep),
                BuildSpellGroup(9, Haste),
                BuildSpellGroup(13, DimensionDoor),
                BuildSpellGroup(17, HoldMonster))
            .AddToDB();

        var magicAffinityRangerArcanist = FeatureDefinitionMagicAffinityBuilder
            .Create(MagicAffinityBattleMagic, "MagicAffinityRangerArcanist")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        var additionalDamageArcanistMark = FeatureDefinitionAdditionalDamageBuilder
            .Create(AdditionalDamageHuntersMark, "AdditionalDamageArcanistMark")
            .SetGuiPresentation(Category.Feature)
            .SetSpecificDamageType(DamageTypeForce)
            .SetDamageDice(DieType.D6, 0)
            .SetNotificationTag(ArcanistMarkTag)
            .SetTargetCondition(conditionMarkedByArcanist, AdditionalDamageTriggerCondition.TargetDoesNotHaveCondition)
            .SetConditionOperations(
                new ConditionOperationDescription
                {
                    ConditionDefinition = conditionMarkedByArcanist,
                    Operation = ConditionOperationDescription.ConditionOperation.Add
                }
            )
            .AddToDB();

        //
        // LEVEL 07
        //

        var additionalDamageArcanistArcaneDetonation = FeatureDefinitionAdditionalDamageBuilder
            .Create(AdditionalDamageHuntersMark, "AdditionalDamageArcanistArcaneDetonation")
            .SetGuiPresentation(Category.Feature)
            .SetSpecificDamageType(DamageTypeForce)
            .SetDamageDice(DieType.D6, 1)
            .SetNotificationTag(ArcanistMarkTag)
            .SetTargetCondition(
                conditionMarkedByArcanist,
                AdditionalDamageTriggerCondition.TargetHasConditionCreatedByMe)
            .SetConditionOperations(
                new ConditionOperationDescription
                {
                    ConditionDefinition = conditionMarkedByArcanist,
                    Operation = ConditionOperationDescription.ConditionOperation.Remove
                })
            .SetAdvancement(
                AdditionalDamageAdvancement.ClassLevel,
                DiceByRankBuilder.BuildDiceByRankTable(1, step: 11, increment: 1))
            .SetFrequencyLimit(FeatureLimitedUsage.None)
            .SetImpactParticleReference(MagicMissile.EffectDescription.EffectParticleParameters.impactParticleReference)
            .AddToDB();

        //
        // LEVEL 07
        //

        var arcanistMarkedEffect =
            EffectFormBuilder
                .Create()
                .SetConditionForm(conditionMarkedByArcanist, ConditionForm.ConditionOperation.Add)
                .Build();

        var arcanistDamageEffect =
            EffectFormBuilder
                .Create()
                .SetDamageForm(DamageTypeForce, 4, DieType.D8)
                .Build();

        var powerArcanistArcanePulse = CreatePowerArcanistArcanePulse(
            "PowerArcanistArcanePulse",
            null,
            arcanistMarkedEffect,
            arcanistDamageEffect);

        //
        // LEVEL 11
        //

        var additionalDamageArcanistArcaneDetonationUpgrade = FeatureDefinitionBuilder
            .Create("AdditionalDamageArcanistArcaneDetonationUpgrade")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        //
        // LEVEL 15
        //

        var arcanistDamageUpgradeEffect =
            EffectFormBuilder
                .Create()
                .SetDamageForm(DamageTypeForce, 8, DieType.D8)
                .Build();

        var powerArcanistArcanePulseUpgrade = CreatePowerArcanistArcanePulse(
            "PowerArcanistArcanePulseUpgrade",
            powerArcanistArcanePulse,
            arcanistMarkedEffect,
            arcanistDamageUpgradeEffect
        );

        Subclass = CharacterSubclassDefinitionBuilder
            .Create("RangerArcanist")
            .SetGuiPresentation(Category.Subclass, RoguishShadowCaster)
            .AddFeaturesAtLevel(3,
                autoPreparedSpellsArcanist,
                magicAffinityRangerArcanist,
                additionalDamageArcanistMark,
                additionalDamageArcanistArcaneDetonation)
            .AddFeaturesAtLevel(7,
                powerArcanistArcanePulse)
            .AddFeaturesAtLevel(11,
                additionalDamageArcanistArcaneDetonationUpgrade)
            .AddFeaturesAtLevel(15,
                powerArcanistArcanePulseUpgrade)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceRangerArchetypes;

    private static FeatureDefinitionPower CreatePowerArcanistArcanePulse(
        string name,
        FeatureDefinitionPower overriddenPower = null,
        params EffectForm[] effectForms)
    {
        return FeatureDefinitionPowerBuilder
            .Create(name)
            .SetGuiPresentation("PowerArcanistArcanePulse", Category.Feature,
                PowerDomainElementalHeraldOfTheElementsThunder)
            .SetUsesAbilityBonus(ActivationTime.Action, RechargeRate.LongRest, AttributeDefinitions.Wisdom, 1, 0)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create(MagicMissile.EffectDescription)
                .SetCreatedByCharacter()
                .SetTargetingData(Side.Enemy, RangeType.Distance, 30, TargetType.Sphere)
                .SetEffectForms(effectForms)
                .Build())
            .SetShowCasting(true)
            .SetOverriddenPower(overriddenPower)
            .AddToDB();
    }
}
