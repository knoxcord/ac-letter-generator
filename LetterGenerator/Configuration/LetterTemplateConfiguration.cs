namespace LetterGenerator.Configuration;

public class LetterTemplateConfiguration
{
    public const string SectionName = "LetterTemplates";

    /// <summary>
    /// Directory holding the letter templates. Relative paths resolve against the content root;
    /// absolute paths are used as-is, which is how a mounted volume is pointed at in a container.
    /// </summary>
    public string DirectoryPath { get; set; } = "Images";
}
