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
        private TaikoModHardRock hardRockMod;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            drawableTaikoRuleset.LockPlayfieldAspect.Value = false;

            hiddenMod = drawableTaikoRuleset.Mods.OfType<TaikoModHidden>().FirstOrDefault();
            hardRockMod = drawableTaikoRuleset.Mods.OfType<TaikoModHardRock>().FirstOrDefault();

            if (hiddenMod != null)
            {
                taikoPlayfield = (TaikoPlayfield)drawableTaikoRuleset.Playfield;

                if (hardRockMod == null)
                {
                    // Classic taiko HD without HR forces a 4:3 aspect ratio.
                    taikoPlayfield.PlayfieldContainer.Add(hiddenOverlay = new Box
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Colour = Colour4.Black,
                        Height = TaikoPlayfield.DEFAULT_HEIGHT,
                        Width = taikoPlayfield.DrawWidth * Math.Max(1 - getPlayfieldPercentage(4f / 3f), 0),
                    });
                }
                else
                {
                    // On Classic HDHR, fadeout starts before the hitobjects have entered the screen.
                    hiddenMod.FadeOutStartTime = 1.2f;
                }
            }
        }

        private float getPlayfieldPercentage(float aspectRatio)
        {
            float playfieldAspectRatio = (taikoPlayfield.DrawWidth / taikoPlayfield.DrawHeight);

            // The ratio playfield : screen is 3.84.
            const float screen_to_playfield = 3.84f;

            // playfieldPercentage here is a value which represents how many % of the
            // current playfield are supposed to remain visible when forcing 4:3.
            return (float)Math.Round((aspectRatio * screen_to_playfield) / playfieldAspectRatio, 4);
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
                if (hardRockMod == null)
                {
                    // 4:3 is the target screen aspect ratio for Classic HD.
                    float playfieldPercentage = getPlayfieldPercentage(4f / 3f);

                    // We add 0.05f because stable starts the fadeout right before the actual cutoff.
                    hiddenMod.FadeOutStartTime = Math.Min(playfieldPercentage + 0.05f, 1);
                    hiddenOverlay.Width = taikoPlayfield.DrawWidth * Math.Max(1 - playfieldPercentage, 0);

                    // The default HD fadeout duration of 0.375f is for a 16:9 screen, here we scale this too.
                    hiddenMod.FadeOutDuration = 0.375f * getPlayfieldPercentage(16f / 9f);
                }
            }
        }
    }
}
