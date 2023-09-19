using BetterShadows;
using Dalamud.Utility.Signatures;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using Dalamud.Logging;
using DrahsidLib;

namespace BetterShadows;

// referenced from WAIA

[StructLayout(LayoutKind.Explicit, Size = 0x60)]
public struct TerritoryInfo {
    [FieldOffset(0x1C)] public int InSanctuary;
    [FieldOffset(0x24)] public uint AreaPlaceNameID;
    [FieldOffset(0x28)] public uint SubAreaPlaceNameID;

    public bool IsInSanctuary() => InSanctuary != 0;
}

public unsafe class DtrDisplay : IDisposable
{
    public PlaceName? currentContinent;
    public PlaceName? currentTerritory;
    public PlaceName? currentRegion;
    public PlaceName? currentSubArea;

    private uint lastTerritory;
    private uint lastRegion;
    private uint lastSubArea;

    public bool locationChanged = false;

    [Signature("48 8D 0D ?? ?? ?? ?? BA ?? ?? ?? ?? F3 0F 5C 05", ScanType = ScanType.StaticAddress)]
    private readonly TerritoryInfo* territoryInfo = null!;

    public DtrDisplay()
    {
        SignatureHelper.Initialise(this);

        Service.Framework.Update += OnFrameworkUpdate;
        Service.ClientState.TerritoryChanged += OnZoneChange;
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnFrameworkUpdate;
        Service.ClientState.TerritoryChanged -= OnZoneChange;
    }

    private void OnFrameworkUpdate(Framework framework)
    {
        if (Service.ClientState.LocalPlayer is null) return;

        UpdateRegion();
        UpdateSubArea();
        UpdateTerritory();

        // attempt to eliminate bogus entries
        var house = HousingManager.Instance();

        if (house->GetCurrentHouseId() != -1 || house->GetCurrentPlot() != -1 || house->GetCurrentWard() != -1) {
            currentRegion = null;
            currentSubArea = null;
        }
    }

    private void OnZoneChange(object? sender, ushort e) => locationChanged = true;

    private void UpdateTerritory() {
        if (lastTerritory != Service.ClientState.TerritoryType) {
            lastTerritory = Service.ClientState.TerritoryType;
            var territory = Service.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(Service.ClientState.TerritoryType);

            currentTerritory = territory?.PlaceName.Value;
            currentContinent = territory?.PlaceNameRegion.Value;
            locationChanged = true;
        }
    }

    private void UpdateSubArea() {
        if (lastSubArea != territoryInfo->SubAreaPlaceNameID) {
            lastSubArea = territoryInfo->SubAreaPlaceNameID;
            currentSubArea = GetPlaceName(territoryInfo->SubAreaPlaceNameID);
            locationChanged = true;
        }
    }

    private void UpdateRegion() {
        if (lastRegion != territoryInfo->AreaPlaceNameID) {
            lastRegion = territoryInfo->AreaPlaceNameID;
            currentRegion = GetPlaceName(territoryInfo->AreaPlaceNameID);
            locationChanged = true;
        }
    }

    private static PlaceName? GetPlaceName(uint row) => Service.DataManager.GetExcelSheet<PlaceName>()!.GetRow(row);
}
