using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "WallsRuleTileAssetGenerator", menuName = "Tools/Tiles/Walls Rule Tile Asset Generator")]
public class WallsRuleTileAssetGenerator : ScriptableObject
{
    private enum SpriteMappingMode
    {
        Position = 0,
        Name = 1,
    }

    private struct SpriteDescriptor
    {
        public Sprite Sprite;
        public long FileId;
        public string Name;
        public string PositionKey;
    }

    private static readonly Regex DefaultSpriteGuidRegex = new Regex(
        @"m_DefaultSprite:\s*\{fileID:\s*-?\d+,\s*guid:\s*([0-9a-fA-F]{32}),\s*type:\s*3\}",
        RegexOptions.Compiled);

    private static readonly Regex SpriteReferenceRegex = new Regex(
        @"\{fileID:\s*(-?\d+),\s*guid:\s*([0-9a-fA-F]{32}),\s*type:\s*3\}",
        RegexOptions.Compiled);

    private static readonly Regex MainObjectNameRegex = new Regex(
        @"^  m_Name:\s*.*$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    [SerializeField] private WallRuleTile templateAsset;
    [SerializeField] private Texture2D targetSpriteSheet;
    [SerializeField] private string assetSuffix = "new";
    [SerializeField] private SpriteMappingMode mappingMode = SpriteMappingMode.Position;
    [SerializeField] private bool overwriteIfExists;

    public bool TryGenerate(out string outputPath, out string error)
    {
        outputPath = string.Empty;
        error = ValidateInputs();
        if (!string.IsNullOrEmpty(error))
            return false;

        var templatePath = AssetDatabase.GetAssetPath(this.templateAsset);
        var templateYaml = global::System.IO.File.ReadAllText(templatePath, Encoding.UTF8);

        if (!TryExtractTemplateSpriteSheetGuid(templateYaml, out var templateSpriteGuid))
        {
            error = "Could not read the template spritesheet GUID from the template YAML.";
            return false;
        }

        var templateSpriteSheetPath = AssetDatabase.GUIDToAssetPath(templateSpriteGuid);
        if (string.IsNullOrEmpty(templateSpriteSheetPath))
        {
            error = "The template spritesheet GUID found in YAML does not resolve to an asset path.";
            return false;
        }

        var templateSpriteSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(templateSpriteSheetPath);
        if (templateSpriteSheet == null)
        {
            error = "Could not load the template spritesheet texture from the GUID in the copied YAML.";
            return false;
        }

        if (!TryLoadSpriteDescriptorsFromTexture(templateSpriteSheet, out var templateSprites, out error))
            return false;

        if (!TryLoadSpriteDescriptorsFromTexture(this.targetSpriteSheet, out var targetSprites, out error))
            return false;

        if (!TryBuildFileIdMap(templateSprites, targetSprites, out var fileIdMap, out error))
            return false;

        LogMappingPreview(fileIdMap, templateSprites, targetSprites);

        var targetSpriteSheetPath = AssetDatabase.GetAssetPath(this.targetSpriteSheet);
        var targetSpriteGuid = AssetDatabase.AssetPathToGUID(targetSpriteSheetPath);

        var generatedYaml = RewriteSpriteReferences(
            templateYaml,
            templateSpriteGuid,
            targetSpriteGuid,
            fileIdMap,
            out var unresolvedSourceIds,
            out var replacedCount);

        if (unresolvedSourceIds.Count > 0)
        {
            var missingPreview = string.Join(", ", unresolvedSourceIds.Take(5).Select(id => id.ToString(CultureInfo.InvariantCulture)));
            error =
                $"Found sprite references in template YAML that are not present in the template spritesheet mapping. " +
                $"Missing fileIDs: {missingPreview}";
            return false;
        }

        if (replacedCount == 0)
        {
            error = "No sprite references were replaced. Ensure the template YAML contains sprite references to remap.";
            return false;
        }

        var outputFolder = GetGeneratorFolderPath();
        if (string.IsNullOrEmpty(outputFolder))
        {
            error = "Could not resolve output folder from generator asset path.";
            return false;
        }

        var outputFileName = BuildOutputFileName(this.assetSuffix);
        var outputObjectName = global::System.IO.Path.GetFileNameWithoutExtension(outputFileName);
        generatedYaml = RewriteMainObjectName(generatedYaml, outputObjectName);
        outputPath = global::System.IO.Path.Combine(outputFolder, outputFileName).Replace('\\', '/');

        if (global::System.IO.File.Exists(outputPath) && !this.overwriteIfExists)
        {
            error = $"Output file already exists: {outputPath}. Enable 'Overwrite If Exists' to replace it.";
            return false;
        }

        global::System.IO.File.WriteAllText(outputPath, generatedYaml, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();

        return true;
    }

    public string GetOutputPreviewPath()
    {
        var folder = GetGeneratorFolderPath();
        if (string.IsNullOrEmpty(folder))
            return "<assign generator asset first>";

        return global::System.IO.Path.Combine(folder, BuildOutputFileName(this.assetSuffix)).Replace('\\', '/');
    }

    private string ValidateInputs()
    {
        if (this.templateAsset == null)
            return "Assign a template WallRuleTile asset.";

        if (this.targetSpriteSheet == null)
            return "Assign a target spritesheet texture.";

        if (string.IsNullOrWhiteSpace(this.assetSuffix))
            return "Asset suffix cannot be empty.";

        var generatorPath = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(generatorPath))
            return "Save the generator asset first so the output folder can be resolved.";

        return string.Empty;
    }

    private static bool TryLoadSpriteDescriptorsFromTexture(
        Texture2D texture,
        out List<SpriteDescriptor> descriptors,
        out string error)
    {
        descriptors = new List<SpriteDescriptor>();
        error = string.Empty;

        var texturePath = AssetDatabase.GetAssetPath(texture);
        var sprites = AssetDatabase
            .LoadAllAssetsAtPath(texturePath)
            .OfType<Sprite>()
            .ToArray();

        if (sprites.Length == 0)
        {
            error = $"The spritesheet '{texture.name}' does not contain any sliced sprites.";
            return false;
        }

        foreach (var sprite in sprites)
        {
            if (!TryGetLocalFileId(sprite, out var fileId))
            {
                error = $"Could not resolve fileID for sprite '{sprite.name}' in sheet '{texture.name}'.";
                return false;
            }

            descriptors.Add(new SpriteDescriptor
            {
                Sprite = sprite,
                FileId = fileId,
                Name = sprite.name,
                PositionKey = BuildPositionKey(sprite.rect),
            });
        }

        return true;
    }

    private bool TryBuildFileIdMap(
        IReadOnlyList<SpriteDescriptor> templateSprites,
        IReadOnlyList<SpriteDescriptor> targetSprites,
        out Dictionary<long, long> fileIdMap,
        out string error)
    {
        fileIdMap = new Dictionary<long, long>();
        error = string.Empty;

        if (templateSprites.Count != targetSprites.Count)
        {
            error =
                $"Template and target spritesheet sprite counts do not match. " +
                $"Template: {templateSprites.Count}, Target: {targetSprites.Count}.";
            return false;
        }

        switch (this.mappingMode)
        {
            case SpriteMappingMode.Name:
                return TryBuildFileIdMapByName(templateSprites, targetSprites, out fileIdMap, out error);
            case SpriteMappingMode.Position:
            default:
                return TryBuildFileIdMapByPosition(templateSprites, targetSprites, out fileIdMap, out error);
        }
    }

    private static bool TryBuildFileIdMapByPosition(
        IReadOnlyList<SpriteDescriptor> templateSprites,
        IReadOnlyList<SpriteDescriptor> targetSprites,
        out Dictionary<long, long> fileIdMap,
        out string error)
    {
        fileIdMap = new Dictionary<long, long>();
        error = string.Empty;

        var targetByPosition = new Dictionary<string, SpriteDescriptor>(StringComparer.Ordinal);
        foreach (var target in targetSprites)
        {
            if (targetByPosition.ContainsKey(target.PositionKey))
            {
                error = $"Target spritesheet has duplicate slice position key '{target.PositionKey}'.";
                return false;
            }

            targetByPosition[target.PositionKey] = target;
        }

        foreach (var template in templateSprites)
        {
            if (!targetByPosition.TryGetValue(template.PositionKey, out var target))
            {
                error =
                    $"Missing target sprite for template position {template.PositionKey} " +
                    $"(template sprite: {template.Name}, fileID: {template.FileId}).";
                return false;
            }

            fileIdMap[template.FileId] = target.FileId;
        }

        return true;
    }

    private static bool TryBuildFileIdMapByName(
        IReadOnlyList<SpriteDescriptor> templateSprites,
        IReadOnlyList<SpriteDescriptor> targetSprites,
        out Dictionary<long, long> fileIdMap,
        out string error)
    {
        fileIdMap = new Dictionary<long, long>();
        error = string.Empty;

        var targetByName = new Dictionary<string, SpriteDescriptor>(StringComparer.Ordinal);
        foreach (var target in targetSprites)
        {
            if (targetByName.ContainsKey(target.Name))
            {
                error = $"Target spritesheet has duplicate sprite name '{target.Name}'.";
                return false;
            }

            targetByName[target.Name] = target;
        }

        foreach (var template in templateSprites)
        {
            if (!targetByName.TryGetValue(template.Name, out var target))
            {
                error =
                    $"Missing target sprite named '{template.Name}' " +
                    $"(template fileID: {template.FileId}).";
                return false;
            }

            fileIdMap[template.FileId] = target.FileId;
        }

        return true;
    }

    private static string BuildPositionKey(Rect rect)
    {
        var x = Mathf.RoundToInt(rect.x);
        var y = Mathf.RoundToInt(rect.y);
        var width = Mathf.RoundToInt(rect.width);
        var height = Mathf.RoundToInt(rect.height);

        return $"{x}:{y}:{width}:{height}";
    }

    private void LogMappingPreview(
        IReadOnlyDictionary<long, long> fileIdMap,
        IReadOnlyList<SpriteDescriptor> templateSprites,
        IReadOnlyList<SpriteDescriptor> targetSprites)
    {
        var templateById = templateSprites.ToDictionary(sprite => sprite.FileId);
        var targetById = targetSprites.ToDictionary(sprite => sprite.FileId);

        var sampleLines = fileIdMap
            .Take(8)
            .Select(pair =>
            {
                var template = templateById[pair.Key];
                var target = targetById[pair.Value];

                return $"{template.FileId} [{template.Name} @ {template.PositionKey}] -> {target.FileId} [{target.Name} @ {target.PositionKey}]";
            });

        Debug.Log(
            $"WallsRuleTile mapping mode '{this.mappingMode}' built {fileIdMap.Count} mappings. Sample:\n" +
            string.Join("\n", sampleLines));
    }

    private static bool TryGetLocalFileId(Sprite sprite, out long fileId)
    {
        fileId = 0;

        if (sprite == null)
            return false;

        return AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out _, out fileId);
    }

    private static bool TryExtractTemplateSpriteSheetGuid(string yamlContent, out string guid)
    {
        guid = string.Empty;

        var defaultSpriteGuidMatch = DefaultSpriteGuidRegex.Match(yamlContent);
        if (defaultSpriteGuidMatch.Success)
        {
            guid = defaultSpriteGuidMatch.Groups[1].Value;
            return true;
        }

        var mostFrequentGuid = SpriteReferenceRegex
            .Matches(yamlContent)
            .Select(match => match.Groups[2].Value)
            .GroupBy(value => value)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .FirstOrDefault();

        if (string.IsNullOrEmpty(mostFrequentGuid))
            return false;

        guid = mostFrequentGuid;
        return true;
    }

    private static string RewriteSpriteReferences(
        string templateYaml,
        string templateSpriteGuid,
        string targetSpriteGuid,
        IReadOnlyDictionary<long, long> fileIdMap,
        out HashSet<long> unresolvedSourceIds,
        out int replacedCount)
    {
        var unresolved = new HashSet<long>();
        var replacements = 0;

        var rewrittenYaml = SpriteReferenceRegex.Replace(templateYaml, match =>
        {
            var sourceFileIdText = match.Groups[1].Value;
            var sourceGuid = match.Groups[2].Value;

            if (!sourceGuid.Equals(templateSpriteGuid, StringComparison.OrdinalIgnoreCase))
                return match.Value;

            if (!long.TryParse(sourceFileIdText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sourceFileId))
                return match.Value;

            if (!fileIdMap.TryGetValue(sourceFileId, out var targetFileId))
            {
                unresolved.Add(sourceFileId);
                return match.Value;
            }

            replacements++;
            return $"{{fileID: {targetFileId}, guid: {targetSpriteGuid}, type: 3}}";
        });

        unresolvedSourceIds = unresolved;
        replacedCount = replacements;
        return rewrittenYaml;
    }

    private static string RewriteMainObjectName(string yamlContent, string objectName)
    {
        if (string.IsNullOrWhiteSpace(yamlContent) || string.IsNullOrWhiteSpace(objectName))
            return yamlContent;

        var match = MainObjectNameRegex.Match(yamlContent);
        if (!match.Success)
            return yamlContent;

        var escapedObjectName = objectName.Replace("'", "''");
        var replacement = $"  m_Name: '{escapedObjectName}'";

        return yamlContent.Substring(0, match.Index)
               + replacement
               + yamlContent.Substring(match.Index + match.Length);
    }

    private string GetGeneratorFolderPath()
    {
        var generatorAssetPath = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(generatorAssetPath))
            return string.Empty;

        var folder = global::System.IO.Path.GetDirectoryName(generatorAssetPath);
        if (string.IsNullOrEmpty(folder))
            return string.Empty;

        return folder.Replace('\\', '/');
    }

    private static string BuildOutputFileName(string suffix)
    {
        var trimmed = suffix.Trim();
        var sanitizedSuffix = new string(trimmed.Where(ch => !global::System.IO.Path.GetInvalidFileNameChars().Contains(ch)).ToArray());

        if (string.IsNullOrWhiteSpace(sanitizedSuffix))
            sanitizedSuffix = "generated";

        return $"walls_{sanitizedSuffix}.asset";
    }
}
#endif
