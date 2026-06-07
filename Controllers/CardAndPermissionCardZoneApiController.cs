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
    public class CardAndPermissionCardZoneApiController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(int ZoneId)
        {
            Models.CardAndPermissionCardZone result = new CardAndPermissionCardZone
            {
                Card = new List<Card>(),
                Zone = new List<Zone>()
            };

            try
            {
                using (var db = new PmsDbContext())
                {
                    // Get zone and parse its gate list
                    var zone = await db.TbZones.FirstOrDefaultAsync(z => z.ZoneId == ZoneId);
                    if (zone != null)
                    {
                        var gateIds = (zone.GateList ?? "")
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                            .Where(n => n > 0)
                            .OrderBy(n => n)
                            .ToList();

                        result.Zone = gateIds.Select(gid => new Zone
                        {
                            ZoneId = ZoneId,
                            MachineId = gid
                        }).ToList();
                    }

                    // Get active parking cards for this zone
                    var cards = await db.TbParkingCards
                        .Where(c => c.ZoneId == ZoneId && c.Active == 1)
                        .ToListAsync();

                    if (!cards.Any())
                        return Ok(result);

                    // Load related data
                    var cardIds = cards.Select(c => c.ParkingcardId).ToList();
                    var cardTypeIds = cards.Where(c => c.CardtypeId.HasValue).Select(c => c.CardtypeId!.Value).Distinct().ToList();
                    var memberIds = cards.Where(c => c.MemberId.HasValue && c.MemberId > 0).Select(c => c.MemberId!.Value).Distinct().ToList();

                    var cardTypes = await db.TbCardTypes
                        .Where(ct => cardTypeIds.Contains(ct.CardtypeId))
                        .ToDictionaryAsync(ct => ct.CardtypeId);

                    var members = await db.TbMembers
                        .Where(m => memberIds.Contains(m.MemberId))
                        .ToDictionaryAsync(m => m.MemberId);

                    // Get vehicles via TB_PARKINGCARDMAP
                    var cardMapRows = await db.TbParkingCardMaps
                        .Where(m => cardIds.Contains(m.ParkingcardId))
                        .ToListAsync();

                    var vehicleIds = cardMapRows.Select(m => m.VehicleId).Distinct().ToList();
                    var vehicles = await db.TbVehicles
                        .Where(v => vehicleIds.Contains(v.VehicleId))
                        .ToDictionaryAsync(v => v.VehicleId);

                    // Build license list per card
                    var licensesPerCard = cardMapRows
                        .GroupBy(m => m.ParkingcardId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(m => vehicles.TryGetValue(m.VehicleId, out var v) ? v.VehicleLicense : "")
                                  .Where(lic => !string.IsNullOrEmpty(lic))
                                  .ToList());

                    foreach (var card in cards)
                    {
                        cardTypes.TryGetValue(card.CardtypeId ?? -1, out var ct);
                        members.TryGetValue(card.MemberId ?? -1, out var member);
                        licensesPerCard.TryGetValue(card.ParkingcardId, out var licList);

                        var startDate = card.IssueDate.ToDateTime(card.IssueTime);
                        var endDate = card.ExpireDate.ToDateTime(card.ExpireTime);

                        long.TryParse(card.UniqueId, out var uniqueIdLong);

                        result.Card.Add(new Card
                        {
                            ID = card.ParkingcardId,
                            Name = "",
                            Code = card.ParkingcardCode,
                            CardTypeID = card.CardtypeId ?? 0,
                            CardTypeName = ct?.CardtypeName ?? "",
                            MemberID = card.MemberId ?? 0,
                            CompanyID = card.CompanyId ?? 0,
                            CardNumber = uniqueIdLong,
                            UniqueID = uniqueIdLong,
                            ZoneID = card.ZoneId ?? ZoneId,
                            StartDate = startDate,
                            EndDate = endDate,
                            ExtensionDays = 0,
                            Status = card.Active ?? 0,
                            RecorderID = card.RecorderId,
                            RecordDate = card.RecordDate,
                            CardGroup = ct?.CardgroupId ?? 0,
                            CardGroupName = "",
                            LicenseList = licList ?? new List<string>(),
                            MemberName = member?.MemberName ?? "",
                            MemberCode = member?.MemberCode ?? ""
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound("Error : " + ex);
            }
        }
    }
}
