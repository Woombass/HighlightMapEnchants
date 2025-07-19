using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;
using System.Drawing;
using Color = SharpDX.Color;

namespace HighlightMapEnchants;

public class HighlightMapEnchantsSettings : ISettings
{
    //Mandatory setting to allow enabling/disabling your plugin
    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    private static Color essenceColor = new Color(170, 91, 222); 
    private static Color harvestColor = new Color(51, 153, 255);
    private static Color beastColor = new Color(255, 250, 0);
    public ToggleNode ShowBeastEnchant { get; set; } = new(true);
    public ColorNode BeastHighlightColor { get; set; } = new ColorNode(beastColor);
    public ToggleNode ShowHarvestEnchant { get; set; } = new(true);
    public ColorNode HarvestHighlightColor { get; set; } = new ColorNode(harvestColor);
    public ToggleNode ShowCrystalPrisonEnchant { get; set; } = new(true);
    public ColorNode EssenceHighlightColor { get; set; } = new ColorNode(essenceColor);

    #region Auto-Roll
    public ToggleNode EnableAutoRoll { get; set; } = new(false);


    [Menu("Auto-rolling till hit Sacred Grove map enchant")]
    public ToggleNode AutoRollTillHarvest { get; set;} = new(false);
    [Menu("Auto-rolling till hit Einhar's beasts map enchant")]
    public ToggleNode AutoRollTillBeast { get; set; } = new(false);
    [Menu("Auto-rolling till hit crystal prisons (essence) map enchant")]
    public ToggleNode AutoRollTillEssence { get; set; } = new(false);
    #endregion
}