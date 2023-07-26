// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Menu
{
    public partial class StarFountain : SkinReloadableDrawable
    {
        private StarFountainSpewer spewer = null!;

        [Resolved]
        private TextureStore textures { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = spewer = new StarFountainSpewer();
        }

        public void Shoot() => spewer.Shoot();

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);
            spewer.Texture = skin.GetTexture("Menu/fountain-star") ?? textures.Get("Menu/fountain-star");
        }

        public partial class StarFountainSpewer : ParticleSpewer
        {
            private const int particle_duration_min = 300;
            private const int particle_duration_max = 1000;

            private double? lastShootTime;
            private int lastShootDirection;

            protected override float ParticleGravity => 800;

            private const double shoot_duration = 800;

            protected override bool CanSpawnParticles => lastShootTime != null && Time.Current - lastShootTime < shoot_duration;

            [Resolved]
            private ISkinSource skin { get; set; } = null!;

            public StarFountainSpewer()
                : base(null, 240, particle_duration_max)
            {
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = skin.GetTexture("Menu/fountain-star") ?? textures.Get("Menu/fountain-star");
                Active.Value = true;
            }

            protected override FallingParticle CreateParticle()
            {
                return new FallingParticle
                {
                    StartPosition = new Vector2(0, 50),
                    Duration = RNG.NextSingle(particle_duration_min, particle_duration_max),
                    StartAngle = getRandomVariance(4),
                    EndAngle = getRandomVariance(2),
                    EndScale = 2.2f + getRandomVariance(0.4f),
                    Velocity = new Vector2(getCurrentAngle(), -1400 + getRandomVariance(100)),
                };
            }

            private float getCurrentAngle()
            {
                const float x_velocity_from_direction = 500;
                const float x_velocity_random_variance = 60;

                return lastShootDirection * x_velocity_from_direction * (float)(1 - 2 * (Clock.CurrentTime - lastShootTime!.Value) / shoot_duration) + getRandomVariance(x_velocity_random_variance);
            }

            public void Shoot()
            {
                lastShootTime = Clock.CurrentTime;
                lastShootDirection = RNG.Next(-1, 2);
            }

            private static float getRandomVariance(float variance) => RNG.NextSingle(-variance, variance);
        }
    }
}
