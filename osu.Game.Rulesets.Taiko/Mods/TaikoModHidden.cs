// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModHidden : ModHidden, IApplicableToDrawableRuleset<TaikoHitObject>
    {
        public override string Description => @"Beats fade out before you hit them!";
        public override double ScoreMultiplier => 1.06;

        /// <summary>
        /// How far away from the hit target should hitobjects start to fade out.
        /// Range: [0, 1]
        /// </summary>
        public float FadeOutStartTime = 1f;

        /// <summary>
        /// How long hitobjects take to fade out, in terms of the scrolling length.
        /// Range: [0, 1]
        /// </summary>
        public float FadeOutDuration = 0.375f;

        private DrawableTaikoRuleset drawableRuleset;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            this.drawableRuleset = (DrawableTaikoRuleset)drawableRuleset;
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            ApplyNormalVisibilityState(hitObject, state);
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            switch (hitObject)
            {
                case DrawableDrumRollTick _:
                case DrawableHit _:
                    double preempt = drawableRuleset.TimeRange.Value / drawableRuleset.ControlPointAt(hitObject.HitObject.StartTime).Multiplier;
                    double start = hitObject.HitObject.StartTime - preempt * FadeOutStartTime;
                    double duration = preempt * FadeOutDuration;

                    using (hitObject.BeginAbsoluteSequence(start))
                    {
                        hitObject.FadeOut(duration);

                        // DrawableHitObject sets LifetimeEnd to LatestTransformEndTime if it isn't manually changed.
                        // in order for the object to not be killed before its actual end time (as the latest transform ends earlier), set lifetime end explicitly.
                        hitObject.LifetimeEnd = state == ArmedState.Idle || !hitObject.AllJudged
                            ? hitObject.HitObject.GetEndTime() + hitObject.HitObject.HitWindows.WindowFor(HitResult.Miss)
                            : hitObject.HitStateUpdateTime;
                    }

                    break;
            }
        }
    }
}
