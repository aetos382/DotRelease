using System.ComponentModel.DataAnnotations;

using YamlDotNet.Serialization;

namespace DotRelease.Cli.Setting;

public class BranchSettings
{
    [Required]
    public required string Name { get; init; }

    [Required]
    [YamlMember(Alias = "tag-format")]
    public required string ReleaseTagFormat { get; init; }
}
