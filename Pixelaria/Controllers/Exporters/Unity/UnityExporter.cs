/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pixelaria.Data;
using Pixelaria.Data.Exports;
using Pixelaria.Utils;
using YamlDotNet.RepresentationModel;

namespace Pixelaria.Controllers.Exporters.Unity
{
    /// <summary>
    /// Represents a Unity file format exporter
    /// </summary>
    public class UnityExporter : IBundleExporter
    {
        public const string SerializedName = "unityv1";

        private Settings _settings;
        private readonly ISheetExporter _sheetExporter;

        /// <summary>
        /// Dictionary that maps Animation Sheet IDs to a completion progress, ranging from 0 to 1 inclusive.
        /// </summary>
        private readonly Dictionary<int, float> _sheetProgress = new Dictionary<int, float>();

        public UnityExporter(ISheetExporter sheetExporter)
        {
            _settings = new Settings();
            _sheetExporter = sheetExporter;
        }

        public void SetSettings(IBundleExporterSettings settings)
        {
            _settings = (Settings) settings;
        }

        /// <summary>
        /// Gets the currently loaded settings object.
        /// </summary>
        public Settings GetSettings()
        {
            return _settings;
        }

        public IBundleExporterSettings GenerateDefaultSettings()
        {
            return new Settings();
        }

        public async Task ExportBundleConcurrent(Bundle bundle, CancellationToken cancellationToken = new CancellationToken(), BundleExportProgressEventHandler progressHandler = null)
        {
            var settings = _settings;
            string savePath = bundle.ExportPath;

            // Start with initial values for the progress export of every sheet
            var stageProgresses = new float[bundle.AnimationSheets.Count];

            var progressAction = new Action<AnimationSheet>(sheet =>
            {
                if (progressHandler == null)
                    return;

                // Calculate total progress
                int total = (int)Math.Floor(stageProgresses.Sum() / stageProgresses.Length * 100);
                progressHandler(new SheetGenerationBundleExportProgressEventArgs(sheet, BundleExportStage.TextureAtlasGeneration, total, total, "Generating sheets"));
            });

            var generationList = new List<Task>();

            for (int i = 0; i < bundle.AnimationSheets.Count; i++)
            {
                var animationSheet = bundle.AnimationSheets[i];
                int j = i;
                generationList.Add(new Task(() =>
                {
                    progressAction(animationSheet);

                    var bundleSheetTask = _sheetExporter.ExportBundleSheet(animationSheet, cancellationToken, args =>
                    {
                        stageProgresses[j] = (float)args.StageProgress / 100;
                        _sheetProgress[animationSheet.ID] = args.TotalProgress / 100.0f;
                        progressAction(animationSheet);
                    });
                    bundleSheetTask.Wait(cancellationToken);
                    var bundleSheet = bundleSheetTask.Result;

                    var pngMeta = GeneratePngMeta(bundleSheet, animationSheet.Name);

                    using (var pngMetaFile = File.CreateText(Path.Combine(savePath, animationSheet.Name + ".png.meta")))
                    {
                        pngMetaFile.Write(pngMeta.SerializeYaml());
                        pngMetaFile.Flush();
                    }

                    bundleSheet.Sheet.Save(Path.Combine(savePath, animationSheet.Name + ".png"), ImageFormat.Png);

                    foreach (var unityAnimationFile in GenerateAnimations(pngMeta, bundleSheet))
                    {
                        // Animations with 0 FPS don't make sense in Unity; skip them, for now
                        if (unityAnimationFile.Animation.PlaybackSettings.FPS <= 0)
                            continue;

                        using (var animFile = File.CreateText(Path.Combine(savePath, unityAnimationFile.Animation.Name + ".anim")))
                        {
                            animFile.Write(unityAnimationFile.SerializeYaml());
                            animFile.Flush();
                        }
                        using (var animMetaFile = File.CreateText(Path.Combine(savePath, unityAnimationFile.Animation.Name + ".anim.meta")))
                        {
                            animMetaFile.Write(unityAnimationFile.SerializeMetaYaml());
                            animMetaFile.Flush();
                        }

                        if (settings.GenerateAnimationControllers)
                        {
                            using (var animControllerFile = File.CreateText(Path.Combine(savePath, unityAnimationFile.Animation.Name + ".controller")))
                            {
                                animControllerFile.Write(unityAnimationFile.SerializeAnimationControllerYaml());
                                animControllerFile.Flush();
                            }

                            using var animControllerMetaFile = File.CreateText(Path.Combine(savePath, unityAnimationFile.Animation.Name + ".controller.meta"));
                            animControllerMetaFile.Write(unityAnimationFile.SerializeAnimationControllerMetaYaml());
                            animControllerMetaFile.Flush();
                        }
                    }

                    bundleSheet.Dispose();

                    _sheetProgress[animationSheet.ID] = 1;
                    progressAction(animationSheet);
                }));
            }

            var concurrent = new Task(() => { AsyncHelpers.StartAndWaitAllThrottled(generationList, 7, cancellationToken); });

            concurrent.Start();

            await concurrent;
        }

        public float ProgressForAnimationSheet(AnimationSheet sheet)
        {
            return _sheetProgress.TryGetValue(sheet.ID, out float p) ? p : 0;
        }

        private static UnityPngMeta GeneratePngMeta([NotNull] BundleSheetExport sheet, [NotNull] string sheetName)
        {
            int metaSeed = sheetName.GetHashCode();

            var meta = new UnityPngMeta
            {
                GuidSeed = metaSeed,
                Guid = GuidHelper.GenerateSeededGuid(metaSeed)
            };

            var frameRegions = sheet.GetUniqueFrameRegions();

            for (int i = 0; i < frameRegions.Count; i++)
            {
                var entry = frameRegions[i];

                var rand = new Random($"{sheetName}_{i}".GetHashCode());

                var frameEntry = new UnityPngMeta.FrameEntry
                {
                    Frames = entry.FrameRects.Select(f => f.Frame).ToArray(),
                    Name = $"{sheetName}_{i}",
                    InternalId = (long) (rand.NextDouble() * long.MaxValue),
                    Rect = entry.SheetArea,
                    SpriteId = GuidHelper.GenerateSeededGuid(metaSeed + i).ToString().Replace("-", "")
                };
                frameEntry.Rect.Y = sheet.Sheet.Height - frameEntry.Rect.Y - frameEntry.Rect.Height;

                meta.AddEntry(frameEntry);
            }

            return meta;
        }

        private static IEnumerable<UnityAnimationFile> GenerateAnimations([NotNull] UnityPngMeta meta, [NotNull] BundleSheetExport sheet)
        {
            var anims = new List<UnityAnimationFile>();

            foreach (var animation in sheet.Animations)
            {
                var animFile = GenerateAnimationFile(meta, animation);
                anims.Add(animFile);
            }

            return anims;
        }

        private static UnityAnimationFile GenerateAnimationFile([NotNull] UnityPngMeta meta, [NotNull] Animation animation)
        {
            var anim = new UnityAnimationFile(meta, animation);

            return anim;
        }

        public class Settings : IBundleExporterSettings
        {
            private const short Version = 0;

            [Browsable(false)]
            public string ExporterSerializedName => SerializedName;

            [Category("Behavior")]
            [DisplayName("Generate Animation Controllers")]
            [Description("Whether to generate .controller/.controller.meta files to accompany each .anim file generated during export. Defaults to true.")]
            public bool GenerateAnimationControllers { get; set; } = true;

            public IBundleExporterSettings Clone()
            {
                return new Settings
                {
                    GenerateAnimationControllers = GenerateAnimationControllers
                };
            }

            public void Save(Stream stream)
            {
                var writer = new BinaryWriter(stream);
                writer.Write(Version);
                writer.Write(GenerateAnimationControllers);
            }

            public void Load(Stream stream)
            {
                var reader = new BinaryReader(stream);
                reader.ReadInt16(); // Version
                GenerateAnimationControllers = reader.ReadBoolean();
            }
        }
    }

    /// <summary>
    /// Represents a Unity .meta file for a PNG file
    /// </summary>
    internal class UnityPngMeta
    {
        private readonly List<FrameEntry> _frameEntries = new List<FrameEntry>();

        public IReadOnlyList<FrameEntry> FrameEntries => _frameEntries;

        /// <summary>
        /// The seed that was used to generate the <see cref="Guid"/> property using <see cref="GuidHelper.GenerateSeededGuid"/>.
        /// </summary>
        public int GuidSeed { get; set; }

        public Guid Guid { get; set; }

        public void AddEntry(FrameEntry frameEntry)
        {
            _frameEntries.Add(frameEntry);
        }

        public FrameEntry EntryForFrame(IFrame frame)
        {
            return _frameEntries.FirstOrDefault(entry => entry.Frames.Any(f => ReferenceEquals(f, frame)));
        }

        /// <summary>
        /// Returns a string containing a YAML representation of this <see cref="UnityPngMeta"/>
        /// </summary>
        public string SerializeYaml()
        {
            var root = new YamlMappingNode
            {
                {"fileFormatVersion", "2"},
                {"guid", Guid.ToString().Replace("-", "")},
                {"TextureImporter", CreateTextureImporter()}
            };

            var document = new YamlDocument(root);
            var stream = new YamlStream(document);

            using var writer = new StringWriter();
            stream.Save(writer, false);
            writer.Flush();
            return writer.ToString();
        }

        private YamlMappingNode CreateTextureImporter()
        {
            var node = new YamlMappingNode
            {
                {"internalIDToNameTable", CreateMappingTable()},
                {"externalObjects", new YamlMappingNode()},
                {"serializedVersion", "10"},
                {
                    "mipmaps", new YamlMappingNode
                    {
                        {"mipMapMode", "0"},
                        {"enableMipMap", "0"},
                        {"sRGBTexture", "1"},
                        {"linearTexture", "0"},
                        {"fadeOut", "0"},
                        {"borderMipMap", "0"},
                        {"mipMapsPreserveCoverage", "0"},
                        {"alphaTestReferenceValue", "0.5"},
                        {"mipMapFadeDistanceStart", "1"},
                        {"mipMapFadeDistanceEnd", "3"},
                    }
                },
                {
                    "bumpmap", new YamlMappingNode
                    {
                        {"convertToNormalMap", "0"},
                        {"externalNormalMap", "0"},
                        {"heightScale", "0.25"},
                        {"normalMapFilter", "0"},
                    }
                },
                {"isReadable", "0"},
                {"streamingMipmaps", "0"},
                {"streamingMipmapsPriority", "0"},
                {"grayScaleToAlpha", "0"},
                {"generateCubemap", "6"},
                {"cubemapConvolution", "0"},
                {"seamlessCubemap", "0"},
                {"textureFormat", "1"},
                {"maxTextureSize", "2048"},
                {
                    "textureSettings",
                    new YamlMappingNode
                    {
                        {"serializedVersion", "2"},
                        {"filterMode", "0"},
                        {"aniso", "-1"},
                        {"mipBias", "-100"},
                        {"wrapUU", "1"},
                        {"wrapV", "1"},
                        {"wrapW", "1"}
                    }
                },
                {"nPOTScale", "0"},
                {"lightmap", "0"},
                {"compressionQuality", "50"},
                {"spriteMode", "2"},
                {"spriteExtrude", "1"},
                {"spriteMeshType", "1"},
                {"alignment", "0"},
                {"spritePivot", new YamlMappingNode {{"x", "0.5"}, {"y", "0.5"}}},
                {"spritePixelsToUnits", "100"},
                {"spriteBorder", new YamlMappingNode {{"x", "0"}, {"y", "0"}, {"z", "0"}, {"w", "0"}}},
                {"spriteGenerateFallbackPhysicsShape", "1"},
                {"alphaUsage", "1"},
                {"alphaIsTransparency", "1"},
                {"spriteTessellationDetail", "-1"},
                {"textureType", "8"},
                {"textureShape", "1"},
                {"singleChannelComponent", "0"},
                {"maxTextureSizeSet", "0"},
                {"compressionQualitySet", "0"},
                {"textureFormatSet", "0"},
                {
                    "platformSettings", new YamlSequenceNode
                    {
                        new YamlMappingNode
                        {
                            { "serializedVersion", "3" },
                            { "buildTarget", "DefaultTexturePlatform" },
                            { "maxTextureSize", "2048" },
                            { "resizeAlgorithm", "0" },
                            { "textureFormat", "-1" },
                            { "textureCompression", "1" },
                            { "compressionQuality", "50" },
                            { "crunchedCompression", "0" },
                            { "allowsAlphaSplitting", "0" },
                            { "overridden", "0" },
                            { "androidETC2FallbackOverride", "0" },
                            { "forceMaximumCompressionQuality_BC6H_BC7", "0" },
                        },
                        new YamlMappingNode
                        {
                            { "serializedVersion", "3" },
                            { "buildTarget", "Standalone" },
                            { "maxTextureSize", "2048" },
                            { "resizeAlgorithm", "0" },
                            { "textureFormat", "-1" },
                            { "textureCompression", "1" },
                            { "compressionQuality", "50" },
                            { "crunchedCompression", "0" },
                            { "allowsAlphaSplitting", "0" },
                            { "overridden", "0" },
                            { "androidETC2FallbackOverride", "0" },
                            { "forceMaximumCompressionQuality_BC6H_BC7", "0" },
                        }
                    }
                },
                {"spriteSheet", CreateSpriteSheet()}
            };
            return node;
        }

        private YamlSequenceNode CreateMappingTable()
        {
            var node = new YamlSequenceNode();

            foreach (var entry in _frameEntries)
            {
                var mapping = new YamlMappingNode();
                var first = new YamlMappingNode(new YamlScalarNode("213"), new YamlScalarNode($"{entry.InternalId}"));
                mapping.Add("first", first);
                mapping.Add("second", entry.Name);

                node.Add(mapping);
            }

            return node;
        }

        private YamlMappingNode CreateSpriteSheet()
        {
            var node = new YamlMappingNode
            {
                {"serializedVersion", "2"},
                {"sprites", CreateSprites()},
                {"outline", new YamlSequenceNode()},
                {"physicsShape", new YamlSequenceNode()},
                {"bones", new YamlSequenceNode()},
                {"spriteID", "5e97eb03825dee720800000000000021"},
                {"internalID", "0"},
                {"vertices", new YamlSequenceNode()},
                {"indices", GenerateNullNode()},
                {"edges", new YamlSequenceNode()},
                {"weights", new YamlSequenceNode()},
                {"secondaryTextures", new YamlSequenceNode()},
            };

            return node;
        }

        private YamlSequenceNode CreateSprites()
        {
            var node = new YamlSequenceNode();

            foreach (var frameEntry in _frameEntries)
            {
                node.Add(frameEntry.CreateSpriteNode());
            }

            return node;
        }

        private static YamlScalarNode GenerateNullNode()
        {
            return new YamlScalarNode("");
        }

        public struct FrameEntry
        {
            public IFrame[] Frames;
            public Rectangle Rect;
            public string Name;
            public long InternalId;
            public string SpriteId;

            public YamlMappingNode CreateSpriteNode()
            {
                var node = new YamlMappingNode
                {
                    {"name", Name},
                    {
                        "rect", new YamlMappingNode
                        {
                            {"serializedVersion", "2"},
                            {"x", Rect.X.ToString()},
                            {"y", Rect.Y.ToString()},
                            {"width", Rect.Width.ToString()},
                            {"height", Rect.Height.ToString()}
                        }
                    },
                    {"alignment", "1"},
                    {"pivot", new YamlMappingNode {{"x", "0"}, {"y", "1"}}},
                    {"border", new YamlMappingNode {{"x", "0"}, {"y", "0"}, {"z", "0"}, {"w", "0"}}},
                    {"outline", new YamlSequenceNode()},
                    {"physicsShape", new YamlSequenceNode()},
                    {"tessellationDetail", "0"},
                    {"bones", new YamlSequenceNode()},
                    {"spriteID", $"{SpriteId}"},
                    {"internalID", $"{InternalId}"},
                    {"vertices", new YamlSequenceNode()},
                    {"indices", GenerateNullNode()},
                    {"edges", new YamlSequenceNode()},
                    {"weights", new YamlSequenceNode()},
                };


                return node;
            }
        }
    }

    /// <summary>
    /// Represents a Unity .anim file for an animation
    /// </summary>
    internal class UnityAnimationFile
    {
        private readonly List<AnimationCurveEntry> _animationCurve = new List<AnimationCurveEntry>();

        private int AnimationStateMachineId { get; set; }
        private int AnimationStateId { get; set; }

        public Guid Guid { get; set; }
        public Guid AnimationControllerGuid { get; set; }

        public UnityPngMeta Meta { get; }

        public Animation Animation { get; }

        public IReadOnlyList<AnimationCurveEntry> AnimationCurve => _animationCurve;

        public UnityAnimationFile([NotNull] UnityPngMeta meta, [NotNull] Animation animation)
        {
            Guid = GuidHelper.GenerateSeededGuid(meta.GuidSeed + animation.Name.GetHashCode());
            AnimationControllerGuid = GuidHelper.GenerateSeededGuid(meta.GuidSeed + animation.Name.GetHashCode() + 1);
            AnimationStateMachineId = meta.GuidSeed + animation.Name.GetHashCode() + 10;
            AnimationStateId = meta.GuidSeed + animation.Name.GetHashCode() + 9;

            Meta = meta;
            Animation = animation;
            GenerateAnimationCurve();
        }

        private void GenerateAnimationCurve()
        {
            float time = 0;

            foreach (var frame in Animation.Frames)
            {
                var entry = Meta.EntryForFrame(frame);

                var curveEntry = new AnimationCurveEntry
                {
                    FileId = entry.InternalId,
                    Guid = Meta.Guid.ToString().Replace("-", ""),
                    Time = time
                };

                _animationCurve.Add(curveEntry);

                time += 1.0f / Animation.PlaybackSettings.FPS;
            }
        }

        /// <summary>
        /// Returns a string containing a YAML representation of this <see cref="UnityAnimationFile"/>
        /// </summary>
        public string SerializeYaml()
        {
            var root = new YamlMappingNode
            {
                {"AnimationClip", GenerateAnimationClip()}
            };
            var document = new YamlDocument(root);
            var stream = new YamlStream(document);

            using var writer = new StringWriter();
            stream.Save(writer, false);
            writer.Flush();
            string body = writer.ToString();

            body = $@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
{body}";

            return body;
        }

        /// <summary>
        /// Returns a string containing a YAML .meta representation of this <see cref="UnityAnimationFile"/>
        /// </summary>
        public string SerializeMetaYaml()
        {
            var root = new YamlMappingNode
            {
                {"fileFormatVersion", "2"},
                {"guid", Guid.ToString().Replace("-", "")},
                {
                    "NativeFormatImporter", new YamlMappingNode
                    {
                        {"externalObjects", new YamlMappingNode()},
                        {"mainObjectFileID", "7400000"},
                        {"userData", GenerateNullNode()},
                        {"assetBundleName", GenerateNullNode()},
                        {"assetBundleVariant", GenerateNullNode()}
                    }
                }
            };

            var document = new YamlDocument(root);
            var stream = new YamlStream(document);

            using var writer = new StringWriter();
            stream.Save(writer, false);
            writer.Flush();
            string body = writer.ToString();
            return body;
        }

        /// <summary>
        /// Returns a string containing a YAML representation of an animation controller for this <see cref="UnityAnimationFile"/>
        /// </summary>
        public string SerializeAnimationControllerYaml()
        {
            string body = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:";

            // Animation controller
            {
                var rootController = new YamlMappingNode
                {
                    {"AnimationController", GenerateAnimationController()}
                };
                var document = new YamlDocument(rootController);
                var stream = new YamlStream(document);

                using var writer = new StringWriter();
                stream.Save(writer, false);
                writer.Flush();

                body += $"\n--- !u!91 &9100000\n{writer}";
            }
            body = body.Replace("...\r\n", "");
            // Animation state machine
            {
                var rootController = new YamlMappingNode
                {
                    {"AnimatorStateMachine", GenerateAnimationStateMachine()}
                };
                var document = new YamlDocument(rootController);
                var stream = new YamlStream(document);

                using var writer = new StringWriter();
                stream.Save(writer, false);
                writer.Flush();

                body += $"--- !u!1107 &{AnimationStateMachineId}\n{writer}";
            }
            body = body.Replace("...\r\n", "");
            // Animation state
            {
                var rootController = new YamlMappingNode
                {
                    {"AnimatorState", GenerateAnimationState()}
                };
                var document = new YamlDocument(rootController);
                var stream = new YamlStream(document);

                using var writer = new StringWriter();
                stream.Save(writer, false);
                writer.Flush();

                body += $"--- !u!1102 &{AnimationStateId}\n{writer}";
            }

            return body;
        }

        /// <summary>
        /// Returns a string containing a YAML .meta representation of the animation controller for this <see cref="UnityAnimationFile"/>
        /// </summary>
        public string SerializeAnimationControllerMetaYaml()
        {
            var root = new YamlMappingNode
            {
                {"fileFormatVersion", "2"},
                {"guid", AnimationControllerGuid.ToString().Replace("-", "")},
                {
                    "NativeFormatImporter", new YamlMappingNode
                    {
                        {"externalObjects", new YamlMappingNode()},
                        {"mainObjectFileID", "9100000"},
                        {"userData", GenerateNullNode()},
                        {"assetBundleName", GenerateNullNode()},
                        {"assetBundleVariant", GenerateNullNode()}
                    }
                }
            };

            var document = new YamlDocument(root);
            var stream = new YamlStream(document);

            using var writer = new StringWriter();
            stream.Save(writer, false);
            writer.Flush();
            string body = writer.ToString();
            return body;
        }

        #region Animation

        private YamlMappingNode GenerateAnimationClip()
        {
            var node = new YamlMappingNode
            {
                {"m_ObjectHideFlags", "0"},
                {"m_CorrespondingSourceObject", GenerateZeroFileId()},
                {"m_PrefabInstance", GenerateZeroFileId()},
                {"m_PrefabAsset", GenerateZeroFileId()},
                {"m_Name", Animation.Name},
                {"serializedVersion", "6"},
                {"m_Legacy", "0"},
                {"m_Compressed", "0"},
                {"m_UseHighQualityCurve", "1"},
                {"m_RotationCurves", new YamlSequenceNode()},
                {"m_CompressedRotationCurves", new YamlSequenceNode()},
                {"m_EulerCurves", new YamlSequenceNode()},
                {"m_PositionCurves", new YamlSequenceNode()},
                {"m_ScaleCurves", new YamlSequenceNode()},
                {"m_FloatCurves", new YamlSequenceNode()},
                {
                    "m_PPtrCurves", new YamlSequenceNode
                    {
                        GenerateSpriteCurve()
                    }
                },
                {"m_SampleRate", "60"},
                {"m_WrapMode", "0"},
                {
                    "m_Bounds", new YamlMappingNode
                    {
                        {"m_Center", new YamlMappingNode {{"x", "0"}, {"y", "0"}, {"z", "0"}}},
                        {"m_Extent", new YamlMappingNode {{"x", "0"}, {"y", "0"}, {"z", "0"}}}
                    }
                },
                {"m_ClipBindingConstant", GenerateClipBindingConstant()},
                {"m_AnimationClipSettings", GenerateAnimationClipSettings()},
                {"m_EditorCurves", new YamlSequenceNode()},
                {"m_EulerEditorCurves", new YamlSequenceNode()},
                {"m_HasGenericRootTransform", "0"},
                {"m_HasMotionFloatCurves", "0"},
                {"m_Events", new YamlSequenceNode()}
            };

            return node;
        }

        private YamlMappingNode GenerateSpriteCurve()
        {
            var node = new YamlMappingNode
            {
                {"curve", GenerateCurveList()},
                {"attribute", "m_Sprite"},
                {"path", GenerateNullNode()},
                {"classID", "212"},
                {
                    "script", GenerateZeroFileId()
                }
            };

            return node;
        }

        private YamlSequenceNode GenerateCurveList()
        {
            var node = new YamlSequenceNode();

            foreach (var curveEntry in _animationCurve)
            {
                node.Add(curveEntry.GenerateNode());
            }

            return node;
        }

        private YamlMappingNode GenerateClipBindingConstant()
        {
            var node = new YamlMappingNode
            {
                {
                    "genericBindings", 
                    new YamlSequenceNode
                    {
                        new YamlMappingNode
                        {
                            {"serializedVersion", "2"},
                            {"path", "0"},
                            {"attribute", "0"},
                            {"script", GenerateZeroFileId()},
                            {"typeID", "212"},
                            {"customType", "23"},
                            {"isPPtrCurve", "1"}
                        }
                    }
                },
                {"pptrCurveMapping", GeneratePptrCurveMapping()}
            };

            return node;
        }

        private YamlMappingNode GenerateAnimationClipSettings()
        {
            var node = new YamlMappingNode
            {
                {"serializedVersion", "2"},
                {"m_AdditiveReferencePoseClip", GenerateZeroFileId()},
                {"m_AdditiveReferencePoseTime", "0"},
                {"m_StartTime", "0"},
                {"m_StopTime", $"{1.0f / Animation.PlaybackSettings.FPS * Animation.FrameCount}".Replace(",", ".")},
                {"m_OrientationOffsetY", "0"},
                {"m_Level", "0"},
                {"m_CycleOffset", "0"},
                {"m_HasAdditiveReferencePose", "0"},
                {"m_LoopTime", "1"},
                {"m_LoopBlend", "0"},
                {"m_LoopBlendOrientation", "0"},
                {"m_LoopBlendPositionY", "0"},
                {"m_LoopBlendPositionXZ", "0"},
                {"m_KeepOriginalOrientation", "0"},
                {"m_KeepOriginalPositionY", "1"},
                {"m_KeepOriginalPositionXZ", "0"},
                {"m_HeightFromFeet", "0"},
                {"m_Mirror", "0"}
            };

            return node;
        }

        private YamlSequenceNode GeneratePptrCurveMapping()
        {
            var node = new YamlSequenceNode();

            foreach (var curveEntry in _animationCurve)
            {
                node.Add(curveEntry.GenerateValueNode());
            }

            return node;
        }

        #endregion

        #region Animation Controller

        private YamlMappingNode GenerateAnimationController()
        {
            var node = new YamlMappingNode
            {
                {"m_ObjectHideFlags", "0"},
                {"m_CorrespondingSourceObject", GenerateZeroFileId()},
                {"m_PrefabInstance", GenerateZeroFileId()},
                {"m_PrefabAsset", GenerateZeroFileId()},
                {"m_Name", "PlayerWeapon_Fists_0"},
                {"serializedVersion", "5"},
                {"m_AnimatorParameters", new YamlSequenceNode()},
                {
                    "m_AnimatorLayers", new YamlSequenceNode
                    {
                        new YamlMappingNode
                        {
                            {"serializedVersion", "5"},
                            {"m_Name", "Base Layer"},
                            {
                                "m_StateMachine", new YamlMappingNode
                                {
                                    {"fileID", $"{AnimationStateMachineId}"}
                                }
                            },
                            {"m_Mask", GenerateZeroFileId()},
                            {"m_Motions", new YamlSequenceNode()},
                            {"m_Behaviours", new YamlSequenceNode()},
                            {"m_BlendingMode", "0"},
                            {"m_SyncedLayerIndex", "-1"},
                            {"m_DefaultWeight", "0"},
                            {"m_IKPass", "0"},
                            {"m_SyncedLayerAffectsTiming", "0"},
                            {
                                "m_Controller", new YamlMappingNode
                                {
                                    {"fileID", "9100000"}
                                }
                            }
                        }
                    }
                }
            };

            return node;
        }

        private YamlMappingNode GenerateAnimationStateMachine()
        {
            var node = new YamlMappingNode
            {
                {"serializedVersion", "5"},
                {"m_ObjectHideFlags", "1"},
                {"m_CorrespondingSourceObject", GenerateZeroFileId()},
                {"m_PrefabInstance", GenerateZeroFileId()},
                {"m_PrefabAsset", GenerateZeroFileId()},
                {"m_Name", "Base Layer"},
                {
                    "m_ChildStates", new YamlSequenceNode
                    {
                        new YamlMappingNode
                        {
                            {"serializedVersion", "1"},
                            {
                                "m_State", new YamlMappingNode
                                {
                                    {"fileID", $"{AnimationStateId}"}
                                }
                            }
                        }
                    }
                },
                {"m_ChildStateMachines", new YamlSequenceNode()},
                {"m_AnyStateTransitions", new YamlSequenceNode()},
                {"m_EntryTransitions", new YamlSequenceNode()},
                {"m_StateMachineTransitions", new YamlMappingNode()},
                {"m_StateMachineBehaviours", new YamlSequenceNode()},
                {"m_AnyStatePosition", new YamlMappingNode {{"x", "50"}, {"y", "20"}, {"z", "0"}}},
                {"m_EntryPosition", new YamlMappingNode {{"x", "50"}, {"y", "120"}, {"z", "0"}}},
                {"m_ExitPosition", new YamlMappingNode {{"x", "800"}, {"y", "120"}, {"z", "0"}}},
                {"m_ParentStateMachinePosition", new YamlMappingNode {{"x", "800"}, {"y", "20"}, {"z", "0"}}},
                {
                    "m_DefaultState", new YamlMappingNode
                    {
                        {"fileID", $"{AnimationStateId}"}
                    }
                }
            };

            return node;
        }

        private YamlMappingNode GenerateAnimationState()
        {
            var node = new YamlMappingNode
            {
                {"serializedVersion", "5"},
                {"m_ObjectHideFlags", "1"},
                {"m_CorrespondingSourceObject", GenerateZeroFileId()},
                {"m_PrefabInstance", GenerateZeroFileId()},
                {"m_PrefabAsset", GenerateZeroFileId()},
                {"m_Name", Animation.Name},
                {"m_Speed", "1"},
                {"m_CycleOffset", "0"},
                {"m_Transitions", new YamlSequenceNode()},
                {"m_StateMachineBehaviours", new YamlSequenceNode()},
                {"m_Position", new YamlMappingNode {{"x", "50"}, {"y", "50"}, {"z", "0"}}},
                {"m_IKOnFeet", "0"},
                {"m_WriteDefaultValues", "1"},
                {"m_Mirror", "0"},
                {"m_SpeedParameterActive", "0"},
                {"m_MirrorParameterActive", "0"},
                {"m_CycleOffsetParameterActive", "0"},
                {"m_TimeParameterActive", "0"},
                {
                    "m_Motion", new YamlMappingNode
                    {
                        {"fileID", "7400000"},
                        {"guid", Guid.ToString().Replace("-", "")},
                        {"type", "2"}
                    }
                },
                {"m_Tag", GenerateNullNode()},
                {"m_SpeedParameter", GenerateNullNode()},
                {"m_MirrorParameter", GenerateNullNode()},
                {"m_CycleOffsetParameter", GenerateNullNode()},
                {"m_TimeParameter", GenerateNullNode()},
            };

            return node;
        }

        #endregion

        private static YamlMappingNode GenerateZeroFileId()
        {
            return new YamlMappingNode {{"fileID", "0"}};
        }

        private static YamlScalarNode GenerateNullNode()
        {
            return new YamlScalarNode("");
        }

        public struct AnimationCurveEntry
        {
            public long FileId;
            public string Guid;
            public float Time;

            public YamlMappingNode GenerateNode()
            {
                var node = new YamlMappingNode
                {
                    {"time", $"{Time}".Replace(",", ".")},
                    {"value", GenerateValueNode()}
                };

                return node;
            }

            public YamlMappingNode GenerateValueNode()
            {
                var node = new YamlMappingNode
                {
                    {"fileID", $"{FileId}"},
                    {"guid", Guid},
                    {"type", "3"}
                };

                return node;
            }
        }
    }

    /// <summary>
    /// Helper Guid extensions
    /// </summary>
    internal static class GuidHelper
    {
        /// <summary>
        /// Generates a seeded <see cref="Guid"/> from a given random number seed.
        /// </summary>
        public static Guid GenerateSeededGuid(int seed)
        {
            var r = new Random(seed);
            var guid = new byte[16];
            r.NextBytes(guid);

            return new Guid(guid);
        }
    }
}
