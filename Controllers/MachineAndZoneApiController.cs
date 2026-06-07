using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using JPMSAPI.Models;
using JpmsApiCore_MySql.Database.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace JPMSAPI.Controllers
{
    [Route("Api/[Controller]")]
    [ApiController]
    public class MachineAndZoneApiController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(int machineId)
        {
            Models.ZoneAndMachine rsPlaceAndMachine = new ZoneAndMachine();

            try
            {
                using (var db = new PmsDbContext())
                {
                    // Step 1: Get the requested gate
                    var gate = await db.TbGates.FirstOrDefaultAsync(g => g.Id == machineId);
                    if (gate == null)
                        return NotFound("ไม่่พบข้อมูล Machine machine ID : " + machineId);

                    // Step 2: Load all zones and find which zone contains this gate
                    var allZones = await db.TbZones.ToListAsync();
                    TbZone? dataZone = allZones.FirstOrDefault(z =>
                        (z.GateList ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                        .Contains(machineId));

                    if (dataZone == null)
                        return NotFound("Machine ยังไม่ได้ Map Zone : Machine ID : " + machineId);

                    rsPlaceAndMachine.Zone = new ZoneJudjod
                    {
                        ID = dataZone.ZoneId,
                        Code = "",
                        FloorID = 0,
                        Name = dataZone.ZoneName,
                        Status = 1,
                        ExitDelayMinute = 0,
                    };

                    // Step 3: Get building info from the gate
                    var building = await db.TbBuildings.FirstOrDefaultAsync(b => b.BuildingId == gate.BuildingId);
                    rsPlaceAndMachine.Building = new BuildingJujod
                    {
                        ID = building?.BuildingId ?? 0,
                        Name = building?.BuildingName ?? "",
                        Code = ""
                    };

                    // Step 4: Get company from building
                    var company = await db.TbCompanyInfos
                        .FirstOrDefaultAsync(c => c.BuildingId == gate.BuildingId);

                    rsPlaceAndMachine.Businessunit = new Businessunit
                    {
                        ID = company?.CompanyId ?? 0,
                        Name = company?.CompanyName ?? "",
                        CompanaID = company?.CompanyId ?? 0
                    };

                    rsPlaceAndMachine.Company = new CompanyJujod
                    {
                        ID = company?.CompanyId ?? 0,
                        Name = company?.CompanyName ?? "",
                        Code = company?.CompanyCode ?? ""
                    };

                    // Step 5: Get system info for place-level data
                    var sysInfo = await db.TbSystemInfos.FirstOrDefaultAsync();
                    rsPlaceAndMachine.place = new Place
                    {
                        ID = sysInfo?.Id ?? 0,
                        Name = sysInfo?.NameTh ?? "",
                        Code = sysInfo?.SystemCode ?? "",
                        Address = sysInfo?.Address ?? "",
                        PhoneNum = sysInfo?.Telephone ?? "",
                        TAXID = sysInfo?.TaxId ?? "",
                        VatRate = (decimal)(sysInfo?.VatRate ?? 0f),
                        OpenTime = TimeSpan.Zero,
                        CloseTime = sysInfo?.Overnight?.ToTimeSpan() ?? TimeSpan.Zero,
                        Status = 1,
                        RecorderID = 0,
                        RecordDate = DateTime.MinValue,
                        HeaderSlipReceipt = "",
                        HeaderSlipShift = ""
                    };

                    // Step 6: Get all gates in this zone from GATE_LIST
                    var gateIds = (dataZone.GateList ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                        .Where(n => n > 0)
                        .ToList();

                    var zoneGates = await db.TbGates
                        .Where(g => gateIds.Contains(g.Id))
                        .ToListAsync();

                    rsPlaceAndMachine.machine = zoneGates.Select(g => new Machine
                    {
                        ID = g.Id,
                        Code = g.GateCode ?? "",
                        Name = g.Name ?? "",
                        MachineTypeID = g.GatetypeId,
                        DirectionID = 0,
                        PlaceID = g.BuildingId,
                        Status = 1,
                        RecorderID = 0,
                        RecordDate = DateTime.MinValue,
                        LOCATION_NAME = g.Description ?? "",
                        POSID = g.GateRef ?? ""
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                return NotFound("Error เกิดข้อผิดพลาดภายใน " + ex.Message + "----" + ex.InnerException);
            }

            rsPlaceAndMachine.timeServer = DateTime.Now;
            return Ok(rsPlaceAndMachine);
        }
    }
}
