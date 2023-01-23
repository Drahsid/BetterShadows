using BetterShadows;
using Dalamud.Utility.Signatures;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Runtime.InteropServices;
using Dalamud.Game;

// referenced from WAIA

[StructLayout(LayoutKind.Explicit, Size = 76)]
public readonly struct TerritoryInfoStruct {
    [FieldOffset(8)] private readonly int InSanctuary;
    [FieldOffset(16)] public readonly uint RegionID;
    [FieldOffset(20)] public readonly uint SubAreaID;

    public bool IsInSanctuary => InSanctuary == 1;
    public PlaceName? Region => Service.DataManager.GetExcelSheet<PlaceName>()!.GetRow(RegionID);
    public PlaceName? SubArea => Service.DataManager.GetExcelSheet<PlaceName>()!.GetRow(SubAreaID);
}

namespace BetterShadows
{
    public unsafe class DtrDisplay : IDisposable
    {
        public PlaceName? currentContinent;
        public PlaceName? currentTerritory;
        public PlaceName? currentRegion;
        public PlaceName? currentSubArea;

        private uint lastTerritory;
        private uint lastRegion;
        private uint lastSubArea;

        public bool locationChanged;

        [Signature("8B 2D ?? ?? ?? ?? 41 BF", ScanType = ScanType.StaticAddress)]
        private readonly TerritoryInfoStruct* territoryInfo = null!;

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
        }

        private void OnZoneChange(object? sender, ushort e) => locationChanged = true;

        private void UpdateTerritory()
        {
            if (lastTerritory != Service.ClientState.TerritoryType)
            {
                lastTerritory = Service.ClientState.TerritoryType;
                var territory = Service.DataManager.GetExcelSheet<TerritoryType>()!
                    .GetRow(Service.ClientState.TerritoryType);

                currentTerritory = territory?.PlaceName.Value;
                currentContinent = territory?.PlaceNameRegion.Value;
                locationChanged = true;
            }
        }

        private void UpdateSubArea()
        {
            if (lastSubArea != territoryInfo->SubAreaID)
            {
                lastSubArea = territoryInfo->SubAreaID;
                currentSubArea = territoryInfo->SubArea;
                locationChanged = true;
            }
        }

        private void UpdateRegion()
        {
            if (lastRegion != territoryInfo->RegionID)
            {
                lastRegion = territoryInfo->RegionID;
                currentRegion = territoryInfo->Region;
                locationChanged = true;
            }
        }
    }
}
