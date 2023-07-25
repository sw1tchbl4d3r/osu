// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Database
{
    /// <summary>
    /// Exporter for osu!stable legacy beatmap archives.
    /// Converts all beatmaps in the set to legacy format and exports it as a legacy package.
    /// </summary>
    public class LegacyBeatmapExporter : LegacyArchiveExporter<BeatmapSetInfo>
    {
        public LegacyBeatmapExporter(Storage storage)
            : base(storage)
        {
        }

        protected override Stream? GetFileContents(BeatmapSetInfo model, INamedFileUsage file)
        {
            bool isBeatmap = model.Beatmaps.Any(o => o.Hash == file.File.Hash);

            if (!isBeatmap)
                return base.GetFileContents(model, file);

            // Read the beatmap contents and skin
            using var contentStream = base.GetFileContents(model, file);

            if (contentStream == null)
                return null;

            using var contentStreamReader = new LineBufferedReader(contentStream);
            var beatmapContent = new LegacyBeatmapDecoder().Decode(contentStreamReader);

            using var skinStream = base.GetFileContents(model, file);

            if (skinStream == null)
                return null;

            using var skinStreamReader = new LineBufferedReader(skinStream);
            var beatmapSkin = new LegacySkin(new SkinInfo(), null!)
            {
                Configuration = new LegacySkinDecoder().Decode(skinStreamReader)
            };

            // Convert beatmap elements to be compatible with legacy format
            // So we truncate time and position values to integers, and convert paths with multiple segments to bezier curves
            foreach (var controlPoint in beatmapContent.ControlPointInfo.AllControlPoints)
                controlPoint.Time = Math.Floor(controlPoint.Time);

            foreach (var hitObject in beatmapContent.HitObjects)
            {
                // Truncate end time before truncating start time because end time is dependent on start time
                if (hitObject is IHasDuration hasDuration && hitObject is not IHasPath)
                    hasDuration.Duration = Math.Floor(hasDuration.EndTime) - Math.Floor(hitObject.StartTime);

                hitObject.StartTime = Math.Floor(hitObject.StartTime);

                if (hitObject is not IHasPath hasPath || BezierConverter.CountSegments(hasPath.Path.ControlPoints) <= 1) continue;

                var newControlPoints = BezierConverter.ConvertToModernBezier(hasPath.Path.ControlPoints);

                // Truncate control points to integer positions
                foreach (var pathControlPoint in newControlPoints)
                {
                    pathControlPoint.Position = new Vector2(
                        (float)Math.Floor(pathControlPoint.Position.X),
                        (float)Math.Floor(pathControlPoint.Position.Y));
                }

                hasPath.Path.ControlPoints.Clear();
                hasPath.Path.ControlPoints.AddRange(newControlPoints);
            }

            // Encode to legacy format
            var stream = new MemoryStream();
            using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                new LegacyBeatmapEncoder(beatmapContent, beatmapSkin).Encode(sw);

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        protected override string FileExtension => @".osz";
    }
}
