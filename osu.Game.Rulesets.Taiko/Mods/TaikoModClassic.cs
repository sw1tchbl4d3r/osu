// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModClassic : ModClassic, IApplicableToDrawableRuleset<TaikoHitObject>, IUpdatableByPlayfield
    {
        private DrawableTaikoRuleset drawableTaikoRuleset;
        private TaikoPlayfield taikoPlayfield;

        private Box hiddenOverlay;
        private TaikoModHidden hiddenMod;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            drawableTaikoRuleset.LockPlayfieldAspect.Value = false;

            hiddenMod = drawableTaikoRuleset.Mods.OfType<TaikoModHidden>().FirstOrDefault();

            if (hiddenMod != null)
            {
                taikoPlayfield = (TaikoPlayfield)drawableTaikoRuleset.Playfield;
                taikoPlayfield.PlayfieldContainer.Add(hiddenOverlay = new Box
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Colour = Colour4.Black,
                    Height = TaikoPlayfield.DEFAULT_HEIGHT,
                    Width = 0
                });
            }
        }

        public void Update(Playfield playfield)
        {
            // Classic taiko scrolls at a constant 100px per 1000ms. More notes become visible as the playfield is lengthened.
            const float scroll_rate = 10;

            // Since the time range will depend on a positional value, it is referenced to the x480 pixel space.
            float ratio = drawableTaikoRuleset.DrawHeight / 480;

            drawableTaikoRuleset.TimeRange.Value = (playfield.HitObjectContainer.DrawWidth / ratio) * scroll_rate;

            if (hiddenMod != null)
            {
                float playfieldAspectRatio = (taikoPlayfield.DrawWidth / taikoPlayfield.DrawHeight);

                // The ratio playfield : screen is 3.84.
                const float screen_to_playfield = 3.84f;

                // 4:3 is the target screen aspect ratio for HD.
                // playfieldPercentage here is a value which represents how many % of the
                // current playfield are supposed to remain visible when forcing 4:3.
                float playfieldPercentage = ((4f / 3f) * screen_to_playfield) / playfieldAspectRatio;

                // We add 0.05f because stable starts the fadeout right before the actual cutoff.
                hiddenMod.FadeOutStartTime = Math.Min(playfieldPercentage + 0.05f, 1);
                hiddenOverlay.Width = taikoPlayfield.DrawWidth * Math.Max(1 - playfieldPercentage, 0);
            }
        }
    }
}
