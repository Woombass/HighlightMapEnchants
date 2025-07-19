using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements.AtlasElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using HighlightMapEnchants.Models;
using ImGuiNET;
using InputHumanizer.Input;
using Microsoft.VisualBasic.Devices;
using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vector2 = System.Numerics.Vector2;


namespace HighlightMapEnchants;

public static class VectorExtensions
{
    public static System.Numerics.Vector2 ToNumerics(this SharpDX.Vector2 v)
    {
        return new System.Numerics.Vector2(v.X, v.Y);
    }

    public static SharpDX.Vector2 ToSharpDX(this System.Numerics.Vector2 v)
    {
        return new SharpDX.Vector2(v.X, v.Y);
    }
}



public class HighlightMapEnchants : BaseSettingsPlugin<HighlightMapEnchantsSettings>
{
    private readonly string Einhar = "contain Einhar";
    private readonly string Harvest = "contain The Sacred Grove";
    private readonly string Essence = "contain additional Essences";
    private SyncTask<bool> _currentOperation;
    private SharpDX.Vector2 WindowOffset => GameController.Window.GetWindowRectangleTimeCache.TopLeft;
    private bool RollCancellationRequested => (Control.MouseButtons & MouseButtons.Middle) != 0;

    private IInputController _inputController;

    public override bool Initialise()
    {
        Graphics.InitImage(Path.Combine(DirectoryFullName, "Assets\\reroll.png").Replace('\\', '/'), false);
        LogMsg($"Full image path: {Path.Combine(DirectoryFullName, "Assets\\reroll.png").Replace('\\', '/')}");        
        return true;
    }
    private readonly ConcurrentDictionary<RectangleF, bool?> _mouseStateForRect = [];

    private bool IsButtonPressed(RectangleF buttonRect)
    {
        var prevState = _mouseStateForRect.GetValueOrDefault(buttonRect);
        var isHovered = buttonRect.Contains(MouseSimulator.GetCurrentCursorPosition().ToVector2() - WindowOffset);
        if (!isHovered)
        {
            _mouseStateForRect[buttonRect] = null;
            return false;
        }

        var isPressed = Control.MouseButtons == MouseButtons.Left && CanClickButtons;
        _mouseStateForRect[buttonRect] = isPressed;
        return isPressed &&
               prevState == false;
    }

    private bool CanClickButtons => !ImGui.GetIO().WantCaptureMouse && GameController.IngameState.IngameUi.ZanaMissionChoice.IsVisible;
    public override void Render()
    {
        var kiracMissionPanel = GameController.IngameState.IngameUi.ZanaMissionChoice;
        var activateButtonRect = kiracMissionPanel.GetChildAtIndex(0).GetChildAtIndex(2).GetClientRect();
        var btnRect = new RectangleF(activateButtonRect.BottomRight.X + 50, activateButtonRect.Y, 48, 48);
        if (kiracMissionPanel.IsVisible && GameController.IngameState.IngameUi.InventoryPanel.IsVisible && Settings.EnableAutoRoll)
        {
            Graphics.DrawImage("reroll.png", btnRect);
        }

        if (_currentOperation != null)
        {
            DebugWindow.LogMsg("[Enchant]: Running reroll process..");
            TaskUtils.RunOrRestart(ref _currentOperation, () => null);
            return;
        }

        HighlightMap();

        if (IsButtonPressed(btnRect))
        {

            DebugWindow.LogMsg("[Enchant]: Button pressed");
            _currentOperation = RerollMaps();
        }

    }

    private void HighlightMap()
    {
        var kiracMissionPanel = GameController.IngameState.IngameUi.ZanaMissionChoice;
        var kiracInv = GameController.IngameState.Data.ServerData.NPCInventories.FirstOrDefault();
        if (kiracMissionPanel != null && kiracMissionPanel.IsValid == true && kiracMissionPanel.IsVisible)
        {

            var avMaps = kiracInv.Inventory.InventorySlotItems;
            foreach (var av in avMaps)
            {
                if (av != null && av.Item.GetComponent<Mods>() != null && av.Item.GetComponent<Mods>().EnchantedMods.Count > 0)
                {
                    var enchMods = av.Item.GetComponent<Mods>().EnchantedMods;
                    foreach (var ench in enchMods)
                    {
                        if (Settings.ShowBeastEnchant == true && ench.Translation.Contains(Einhar, System.StringComparison.OrdinalIgnoreCase))
                        {
                            Color color = Settings.BeastHighlightColor;
                            var cell = kiracMissionPanel.GetChildAtIndex(0).GetChildAtIndex(3).GetChildAtIndex(av.PosX);
                            var rect = cell.GetClientRect();

                            var boxPoints = new List<Vector2>();
                            boxPoints.Add(new Vector2(rect.BottomLeft.X, rect.BottomLeft.Y));
                            boxPoints.Add(new Vector2(rect.BottomRight.X, rect.BottomRight.Y));
                            boxPoints.Add(new Vector2(rect.TopRight.X, rect.TopRight.Y));
                            boxPoints.Add(new Vector2(rect.TopLeft.X, rect.TopLeft.Y));
                            boxPoints.Add(new Vector2(rect.BottomLeft.X, rect.BottomLeft.Y));
                            Graphics.DrawPolyLine(boxPoints.ToArray(), color, 2);
                            Graphics.DrawConvexPolyFilled(boxPoints.ToArray(),
                                color with { A = Color.ToByte((int)((double)0.2f * byte.MaxValue)) });
                        }
                        else if (Settings.ShowHarvestEnchant == true && ench.Translation.Contains(Harvest, System.StringComparison.OrdinalIgnoreCase))
                        {
                            Color color = Settings.HarvestHighlightColor;
                            var cell = kiracMissionPanel.GetChildAtIndex(0).GetChildAtIndex(3).GetChildAtIndex(av.PosX);
                            var rect = cell.GetClientRect();

                            var boxPoints = new List<Vector2>();
                            boxPoints.Add(new Vector2(rect.BottomLeft.X, rect.BottomLeft.Y));
                            boxPoints.Add(new Vector2(rect.BottomRight.X, rect.BottomRight.Y));
                            boxPoints.Add(new Vector2(rect.TopRight.X, rect.TopRight.Y));
                            boxPoints.Add(new Vector2(rect.TopLeft.X, rect.TopLeft.Y));
                            boxPoints.Add(new Vector2(rect.BottomLeft.X, rect.BottomLeft.Y));
                            Graphics.DrawPolyLine(boxPoints.ToArray(), color, 2);
                            Graphics.DrawConvexPolyFilled(boxPoints.ToArray(),
                                color with { A = Color.ToByte((int)((double)0.2f * byte.MaxValue)) });
                        }
                        else if (Settings.ShowCrystalPrisonEnchant == true && ench.Translation.Contains(Essence, System.StringComparison.OrdinalIgnoreCase))
                        {
                            Color color = Settings.EssenceHighlightColor;
                            var cell = kiracMissionPanel.GetChildAtIndex(0).GetChildAtIndex(3).GetChildAtIndex(av.PosX);
                            var rect = cell.GetClientRect();

                            var boxPoints = new List<Vector2>();
                            boxPoints.Add(new Vector2(rect.BottomLeft.X, rect.BottomLeft.Y));
                            boxPoints.Add(new Vector2(rect.BottomRight.X, rect.BottomRight.Y));
                            boxPoints.Add(new Vector2(rect.TopRight.X, rect.TopRight.Y));
                            boxPoints.Add(new Vector2(rect.TopLeft.X, rect.TopLeft.Y));
                            boxPoints.Add(new Vector2(rect.BottomLeft.X, rect.BottomLeft.Y));
                            Graphics.DrawPolyLine(boxPoints.ToArray(), color, 2);
                            Graphics.DrawConvexPolyFilled(boxPoints.ToArray(),
                                color with { A = Color.ToByte((int)((double)0.2f * byte.MaxValue)) });
                        }
                    }
                }
            }
        }

    }

    private List<ReportItem> reportItems = new List<ReportItem>();
    private async SyncTask<bool> RerollMaps()
    {

        DebugWindow.LogMsg("[Enchant]: Parsing inventory");
        reportItems = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems
        .Where(itemSlot => itemSlot.Item.Path.Contains("AtlasScoutingReport", StringComparison.CurrentCultureIgnoreCase))
        .OrderBy(x => x.PosX)
        .ThenBy(x => x.PosY)
        .Select(x => new ReportItem
        {
            Rectangle = x.GetClientRect(),
            StackSize = x.Item.GetComponent<Stack>().Size
        })
        .ToList();
        DebugWindow.LogMsg($"[Enchant]: Start rolling");

        await Roll();
        return true;

    }

    private async SyncTask<bool> Roll()
    {
        if (reportItems == null || reportItems.Count <= 0)
        {
            return false;
        }
        if (await CheckForEnchants())
        {
            return true;
        }
        if (!await RollPreamble())
        {
            DebugWindow.LogMsg($"[Enchant]: Stop rolling due to condition");
            return false;
        }

        foreach (var reportItem in reportItems)
        {
            if (RollCancellationRequested)
            {
                return false;
            }
            if (await CheckForEnchants())
            {
                return true;
            }

            var tryGetInputController = GameController.PluginBridge.GetMethod<Func<string, IInputController>>("InputHumanizer.TryGetInputController");
            if (tryGetInputController == null)
            {
                LogError("InputHumanizer method not registered.");
                return false;
            }

            if ((_inputController = tryGetInputController(this.Name)) != null)
            {
                using (_inputController)
                {
                    await _inputController.MoveMouse(reportItem.Rectangle.Center.ToNumerics());
                    while (reportItem.StackSize >= 1)
                    {
                        if (await CheckForEnchants())
                        {
                            return true;
                        }
                        if (RollCancellationRequested)
                        {
                            return false;
                        }

                        if (!GameController.IngameState.IngameUi.InventoryPanel.IsVisible)
                        {
                            DebugWindow.LogMsg("[Enchant]: Inventory Panel closed, aborting loop");
                            return false;
                        }

                        if (!GameController.IngameState.IngameUi.ZanaMissionChoice.IsVisible)
                        {
                            DebugWindow.LogMsg("[Enchant]: Mission Panel closed, aborting loop");
                            return false;
                        }

                        if (RollCancellationRequested)
                        {
                            return false;
                        }
                        await _inputController.Click(MouseButtons.Right);
                        await Wait(TimeSpan.FromMilliseconds(_inputController.GenerateDelay()));
                        if (RollCancellationRequested)
                        {
                            return false;
                        }
                        if (await CheckForEnchants())
                        {
                            return true;
                        }
                        reportItem.StackSize--;


                        if (reportItem.StackSize < 1)
                        {
                            DebugWindow.LogMsg("[Enchant]: End of stack, move to next item");
                            break;
                        }
                    }

                }
            }
            await TaskUtils.NextFrame();
            DebugWindow.LogMsg("[Enchant]: Go to next item");

        }
        await TaskUtils.NextFrame();
        return false;
    }

    private async SyncTask<bool> Wait(TimeSpan period, bool canUseThreadSleep = true)
    {
        if (canUseThreadSleep)
        {
            Thread.Sleep(period);
            return true;
        }

        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < period)
        {
            await TaskUtils.NextFrame();
        }

        return true;
    }

    private async Task<bool> CheckForEnchants()
    {
        DebugWindow.LogMsg($"[Enchant]: Check for enchants");
        var kiracMissionPanel = GameController.IngameState.IngameUi.ZanaMissionChoice;
        var kiracInv = GameController.IngameState.Data.ServerData.NPCInventories.FirstOrDefault();
        var avMaps = kiracInv.Inventory.InventorySlotItems;
        await TaskUtils.NextFrame();
        foreach (var av in avMaps)
        {
            if (av != null && av.Item.GetComponent<Mods>() != null && av.Item.GetComponent<Mods>().EnchantedMods.Count > 0)
            {
                var enchMods = av.Item.GetComponent<Mods>().EnchantedMods;
                foreach (var ench in enchMods)
                {
                    DebugWindow.LogMsg($"[Enchant]: Found enchant: {ench.Translation}");
                    if (Settings.AutoRollTillBeast == true && ench.Translation.Contains(Einhar, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else if (Settings.AutoRollTillHarvest == true && ench.Translation.Contains(Harvest, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else if (Settings.AutoRollTillEssence == true && ench.Translation.Contains(Essence, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }
        await TaskUtils.NextFrame();
        return false;
    }

    private async SyncTask<bool> RollPreamble()
    {

        while (Control.MouseButtons == MouseButtons.Left || RollCancellationRequested)
        {
            if (RollCancellationRequested)
            {
                return false;
            }

            await TaskUtils.NextFrame();
        }

        return true;

    }
}