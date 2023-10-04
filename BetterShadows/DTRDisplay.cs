using DrahsidLib;
using Lumina.Excel.GeneratedSheets;
using System;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace BetterShadows;

// referenced from WAIA

public unsafe class DtrDisplay : IDisposable {
    public PlaceName? currentContinent;
    public PlaceName? currentTerritory;
    public PlaceName? currentRegion;
    public PlaceName? currentSubArea;

    private uint lastTerritory;
    private uint lastRegion;
    private uint lastSubArea;

    public bool locationChanged = false;

    private TerritoryInfo* territoryInfo => TerritoryInfo.Instance();

    public DtrDisplay() {
        Service.Framework.Update += OnFrameworkUpdate;
        Service.ClientState.TerritoryChanged += OnZoneChange;
    }

    public void Dispose() {
        Service.Framework.Update -= OnFrameworkUpdate;
        Service.ClientState.TerritoryChanged -= OnZoneChange;
    }

    private void OnFrameworkUpdate(IFramework framework) {
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

    private void OnZoneChange(ushort e) => locationChanged = true;

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
