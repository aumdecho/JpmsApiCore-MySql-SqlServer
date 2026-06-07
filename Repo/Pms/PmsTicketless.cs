using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JPMSAPI.Models;
using JpmsApiCore_MySql.Database.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace JPMSAPI.Repo.Pms
{
    public static class PmsTicketless
    {
        static readonly CultureInfo en = CultureInfo.InvariantCulture;

        static List<int> ParseGateList(string gateList)
        {
            if (string.IsNullOrEmpty(gateList)) return new List<int>();
            return gateList.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                .Where(n => n > 0).ToList();
        }

        static DateTime CombineDateTime(DateOnly date, TimeOnly? time)
            => date.ToDateTime(time ?? TimeOnly.MinValue);

        static DateTime ParseDateInt(long dateInt)
        {
            string s = dateInt.ToString();
            if (s.Length >= 14 && DateTime.TryParseExact(s[..14], "yyyyMMddHHmmss", en, DateTimeStyles.None, out var dt14))
                return dt14;
            if (s.Length >= 8 && DateTime.TryParseExact(s[..8], "yyyyMMdd", en, DateTimeStyles.None, out var dt8))
                return dt8;
            return DateTime.MinValue;
        }

        static CardCalculate MapParking(
            TbCurrentParking p, TbParkingCard? card,
            TbMember? member, TbCardType? cardType, TbZone? zone,
            Dictionary<int, string> gateNames, TbBuilding? building)
        {
            var arriveDate = CombineDateTime(p.ArriveDate, p.ArriveTime);
            var departDate = p.DepartDate.HasValue
                ? (DateTime?)CombineDateTime(p.DepartDate.Value, p.DepartTime)
                : null;

            return new CardCalculate
            {
                trans = new CardCalculate.Transaction
                {
                    TranRef = 0,
                    entranceDate = arriveDate,
                    entranceGateId = p.EnterGate ?? 0,
                    exitGateId = p.ExitGate ?? 0,
                    entranceGateName = gateNames.GetValueOrDefault(p.EnterGate ?? 0, ""),
                    exitGateName = gateNames.GetValueOrDefault(p.ExitGate ?? 0, ""),
                    exitDate = departDate?.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffffzzzz")
                               ?? DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffffzzzz"),
                    license = p.License?.Trim() ?? "",
                    Zone_Id = card?.ZoneId ?? 0,
                    Place_Id = 0,
                    Building_Id = card?.BuildingId ?? 0,
                    ZoneName = zone?.ZoneName ?? "",
                    Place_Name = "",
                    Building_Name = building?.BuildingName ?? "",
                    CompanyID = card?.CompanyId ?? 0,
                    CompanyName = "",
                    BU_ID = 0,
                    BU_Name = "",
                    MemberID = card?.MemberId ?? 0,
                    MemberName = member?.MemberName ?? "",
                    MemberCode = member?.MemberCode ?? "",
                    MemberTel = member?.MemberTelephone ?? "",
                    CardID = int.TryParse(p.ParkingcardId, out var cid) ? cid : 0,
                    CardCode = card?.ParkingcardCode ?? "",
                    CardType = card?.CardtypeId ?? 0,
                    CardTypeName = cardType?.CardtypeName ?? "",
                    CardGroupID = cardType?.CardgroupId ?? 0,
                    CardGroupName = ""
                }
            };
        }

        static CardCalculate MapParkingCompleted(
            TbParking p, TbParkingCard? card,
            TbMember? member, TbCardType? cardType, TbZone? zone,
            Dictionary<int, string> gateNames, TbBuilding? building)
        {
            var arriveDate = p.ArriveDate.HasValue
                ? CombineDateTime(p.ArriveDate.Value, p.ArriveTime)
                : DateTime.MinValue;
            var departDate = p.DepartDate.HasValue
                ? (DateTime?)CombineDateTime(p.DepartDate.Value, p.DepartTime)
                : null;

            return new CardCalculate
            {
                trans = new CardCalculate.Transaction
                {
                    TranRef = p.ParkingId,
                    entranceDate = arriveDate,
                    entranceGateId = p.EnterGate ?? 0,
                    exitGateId = p.ExitGate ?? 0,
                    entranceGateName = gateNames.GetValueOrDefault(p.EnterGate ?? 0, ""),
                    exitGateName = gateNames.GetValueOrDefault(p.ExitGate ?? 0, ""),
                    exitDate = departDate?.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffffzzzz")
                               ?? DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffffzzzz"),
                    license = p.License?.Trim() ?? "",
                    Zone_Id = card?.ZoneId ?? 0,
                    Place_Id = 0,
                    Building_Id = card?.BuildingId ?? 0,
                    ZoneName = zone?.ZoneName ?? "",
                    Place_Name = "",
                    Building_Name = building?.BuildingName ?? "",
                    CompanyID = card?.CompanyId ?? 0,
                    CompanyName = "",
                    BU_ID = 0,
                    BU_Name = "",
                    MemberID = card?.MemberId ?? 0,
                    MemberName = member?.MemberName ?? "",
                    MemberCode = member?.MemberCode ?? "",
                    MemberTel = member?.MemberTelephone ?? "",
                    CardID = p.ParkingcardId,
                    CardCode = card?.ParkingcardCode ?? "",
                    CardType = card?.CardtypeId ?? 0,
                    CardTypeName = cardType?.CardtypeName ?? "",
                    CardGroupID = cardType?.CardgroupId ?? 0,
                    CardGroupName = ""
                }
            };
        }

        static async Task<(Dictionary<int, TbParkingCard>, Dictionary<int, TbMember>, Dictionary<int, TbCardType>, Dictionary<int, TbZone?>, Dictionary<int, TbBuilding?>, Dictionary<int, string>)>
            LoadLookups(PmsDbContext db, IEnumerable<TbCurrentParking> parkings, IEnumerable<int> cardIds)
        {
            var cardIdList = cardIds.ToList();
            var cards = await db.TbParkingCards
                .Where(c => cardIdList.Contains(c.ParkingcardId))
                .ToDictionaryAsync(c => c.ParkingcardId);

            var memberIds = cards.Values.Where(c => c.MemberId.HasValue).Select(c => c.MemberId!.Value).Distinct().ToList();
            var members = await db.TbMembers.Where(m => memberIds.Contains(m.MemberId)).ToDictionaryAsync(m => m.MemberId);

            var cardTypeIds = cards.Values.Where(c => c.CardtypeId.HasValue).Select(c => c.CardtypeId!.Value).Distinct().ToList();
            var cardTypes = await db.TbCardTypes.Where(ct => cardTypeIds.Contains(ct.CardtypeId)).ToDictionaryAsync(ct => ct.CardtypeId);

            var allGateIds = parkings
                .SelectMany(p => new[] { (int)(p.EnterGate ?? 0), (int)(p.ExitGate ?? 0) })
                .Where(g => g > 0).Distinct().ToList();
            var gates = await db.TbGates.Where(g => allGateIds.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g.Name ?? g.Description);

            var zoneList = await db.TbZones.ToListAsync();
            var buildingIds = cards.Values.Where(c => c.BuildingId.HasValue).Select(c => c.BuildingId!.Value).Distinct().ToList();
            var buildings = await db.TbBuildings.Where(b => buildingIds.Contains(b.BuildingId)).ToDictionaryAsync(b => b.BuildingId, b => (TbBuilding?)b);

            var zonesByGate = new Dictionary<int, TbZone?>();
            foreach (var p in parkings)
            {
                int gid = p.EnterGate ?? 0;
                if (!zonesByGate.ContainsKey(gid))
                    zonesByGate[gid] = zoneList.FirstOrDefault(z => ParseGateList(z.GateList).Contains(gid));
            }

            return (cards, members, cardTypes, zonesByGate, buildings, gates);
        }

        static async Task<(Dictionary<int, TbParkingCard>, Dictionary<int, TbMember>, Dictionary<int, TbCardType>, Dictionary<int, TbZone?>, Dictionary<int, TbBuilding?>, Dictionary<int, string>)>
            LoadLookupsForCompleted(PmsDbContext db, IEnumerable<TbParking> parkings, IEnumerable<int> cardIds)
        {
            var cardIdList = cardIds.ToList();
            var cards = await db.TbParkingCards
                .Where(c => cardIdList.Contains(c.ParkingcardId))
                .ToDictionaryAsync(c => c.ParkingcardId);

            var memberIds = cards.Values.Where(c => c.MemberId.HasValue).Select(c => c.MemberId!.Value).Distinct().ToList();
            var members = await db.TbMembers.Where(m => memberIds.Contains(m.MemberId)).ToDictionaryAsync(m => m.MemberId);

            var cardTypeIds = cards.Values.Where(c => c.CardtypeId.HasValue).Select(c => c.CardtypeId!.Value).Distinct().ToList();
            var cardTypes = await db.TbCardTypes.Where(ct => cardTypeIds.Contains(ct.CardtypeId)).ToDictionaryAsync(ct => ct.CardtypeId);

            var allGateIds = parkings
                .SelectMany(p => new[] { p.EnterGate ?? 0, p.ExitGate ?? 0 })
                .Where(g => g > 0).Distinct().ToList();
            var gates = await db.TbGates.Where(g => allGateIds.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g.Name ?? g.Description);

            var zoneList = await db.TbZones.ToListAsync();
            var buildingIds = cards.Values.Where(c => c.BuildingId.HasValue).Select(c => c.BuildingId!.Value).Distinct().ToList();
            var buildings = await db.TbBuildings.Where(b => buildingIds.Contains(b.BuildingId)).ToDictionaryAsync(b => b.BuildingId, b => (TbBuilding?)b);

            var zonesByGate = new Dictionary<int, TbZone?>();
            foreach (var p in parkings)
            {
                int gid = p.EnterGate ?? 0;
                if (!zonesByGate.ContainsKey(gid))
                    zonesByGate[gid] = zoneList.FirstOrDefault(z => ParseGateList(z.GateList).Contains(gid));
            }

            return (cards, members, cardTypes, zonesByGate, buildings, gates);
        }

        public static async Task<List<CardCalculate>> SearchByTelephone(string strTelephone, int ZoneID, int PlaceId, int companyId)
        {
            var result = new List<CardCalculate>();
            try
            {
                using var db = new PmsDbContext();

                var memberIds = await db.TbMembers
                    .Where(m => m.MemberTelephone == strTelephone)
                    .Select(m => m.MemberId)
                    .ToListAsync();

                if (!memberIds.Any()) return result;

                var cardQuery = db.TbParkingCards.Where(c => memberIds.Contains(c.MemberId ?? -1));
                if (ZoneID > 0) cardQuery = cardQuery.Where(c => c.ZoneId == ZoneID);
                if (companyId > 0) cardQuery = cardQuery.Where(c => c.CompanyId == companyId);
                var cardIds = await cardQuery.Select(c => c.ParkingcardId).ToListAsync();
                if (!cardIds.Any()) return result;

                var cardIdStrings = cardIds.Select(id => id.ToString()).ToList();
                var parkings = await db.TbCurrentParkings
                    .Where(p => cardIdStrings.Contains(p.ParkingcardId) && p.DepartDate == null)
                    .ToListAsync();

                if (!parkings.Any()) return result;

                var (cards, members, cardTypes, zonesByGate, buildings, gates) =
                    await LoadLookups(db, parkings, cardIds);

                foreach (var p in parkings)
                {
                    int cid = int.TryParse(p.ParkingcardId, out var tmp) ? tmp : 0;
                    cards.TryGetValue(cid, out var card);
                    var member = card?.MemberId.HasValue == true ? members.GetValueOrDefault(card.MemberId.Value) : null;
                    var cardType = card?.CardtypeId.HasValue == true ? cardTypes.GetValueOrDefault(card.CardtypeId.Value) : null;
                    var zone = zonesByGate.GetValueOrDefault(p.EnterGate ?? 0);
                    var building = card?.BuildingId.HasValue == true ? buildings.GetValueOrDefault(card.BuildingId.Value) : null;
                    result.Add(MapParking(p, card, member, cardType, zone, gates, building));
                }
            }
            catch (Exception ex)
            {
                await GData.Loggings_JpmsApi.AddLoggingAsync("PmsTicketless.SearchByTelephone error: " + ex.Message);
            }
            return result;
        }

        public static async Task<List<CardCalculate>> SearchByCardCode(string strCardCode, int ZoneID, int PlaceId, int companyId)
        {
            var result = new List<CardCalculate>();
            try
            {
                using var db = new PmsDbContext();

                var cardQuery = db.TbParkingCards.Where(c => c.ParkingcardCode == strCardCode);
                if (ZoneID > 0) cardQuery = cardQuery.Where(c => c.ZoneId == ZoneID);
                if (companyId > 0) cardQuery = cardQuery.Where(c => c.CompanyId == companyId);
                var cardIds = await cardQuery.Select(c => c.ParkingcardId).ToListAsync();
                if (!cardIds.Any()) return result;

                var cardIdStrings = cardIds.Select(id => id.ToString()).ToList();
                var parkings = await db.TbCurrentParkings
                    .Where(p => cardIdStrings.Contains(p.ParkingcardId) && p.DepartDate == null)
                    .ToListAsync();

                if (!parkings.Any()) return result;

                var (cards, members, cardTypes, zonesByGate, buildings, gates) =
                    await LoadLookups(db, parkings, cardIds);

                foreach (var p in parkings)
                {
                    int cid = int.TryParse(p.ParkingcardId, out var tmp) ? tmp : 0;
                    cards.TryGetValue(cid, out var card);
                    var member = card?.MemberId.HasValue == true ? members.GetValueOrDefault(card.MemberId.Value) : null;
                    var cardType = card?.CardtypeId.HasValue == true ? cardTypes.GetValueOrDefault(card.CardtypeId.Value) : null;
                    var zone = zonesByGate.GetValueOrDefault(p.EnterGate ?? 0);
                    var building = card?.BuildingId.HasValue == true ? buildings.GetValueOrDefault(card.BuildingId.Value) : null;
                    result.Add(MapParking(p, card, member, cardType, zone, gates, building));
                }
            }
            catch (Exception ex)
            {
                await GData.Loggings_JpmsApi.AddLoggingAsync("PmsTicketless.SearchByCardCode error: " + ex.Message);
            }
            return result;
        }

        // Ticket format (7 chars) uses MySQL-specific machine location data — not supported in SQL Server schema.
        public static Task<List<CardCalculate>> SearchByTicket(string TicketNo, int ZoneID, int PlaceId, int companyId)
            => Task.FromResult(new List<CardCalculate>());

        // TransRef is a MySQL-specific 19-digit decimal — no equivalent in SQL Server schema.
        public static Task<List<CardCalculate>> SearchByTransRef(string strTransRef, int ZoneID, int PlaceId, int companyId)
            => Task.FromResult(new List<CardCalculate>());

        public static async Task<List<CardCalculate>?> SearchByLicense(string license, int ZoneID, int PlaceId, int companyId)
        {
            var result = new List<CardCalculate>();
            try
            {
                using var db = new PmsDbContext();

                var allParkings = await db.TbCurrentParkings
                    .Where(p => p.DepartDate == null && p.License != null)
                    .ToListAsync();

                // Apply fuzzy Thai license matching in memory (same approach as MySQL side)
                // Apply fuzzy license match in memory: exact first, then case-insensitive contains
                var matchedParkings = allParkings
                    .Where(p => string.Equals(p.License?.Trim(), license.Trim(), StringComparison.OrdinalIgnoreCase)
                             || (p.License != null && p.License.Trim().ToUpper().Contains(license.Trim().ToUpper())))
                    .ToList();

                if (!matchedParkings.Any()) return result;

                var cardIds = matchedParkings
                    .Select(p => int.TryParse(p.ParkingcardId, out var cid) ? cid : 0)
                    .Where(id => id > 0).Distinct().ToList();

                var (cards, members, cardTypes, zonesByGate, buildings, gates) =
                    await LoadLookups(db, matchedParkings, cardIds);

                foreach (var p in matchedParkings)
                {
                    int cid = int.TryParse(p.ParkingcardId, out var tmp) ? tmp : 0;
                    cards.TryGetValue(cid, out var card);
                    if (ZoneID > 0 && card?.ZoneId != ZoneID) continue;
                    if (companyId > 0 && card?.CompanyId != companyId) continue;
                    var member = card?.MemberId.HasValue == true ? members.GetValueOrDefault(card.MemberId.Value) : null;
                    var cardType = card?.CardtypeId.HasValue == true ? cardTypes.GetValueOrDefault(card.CardtypeId.Value) : null;
                    var zone = zonesByGate.GetValueOrDefault(p.EnterGate ?? 0);
                    var building = card?.BuildingId.HasValue == true ? buildings.GetValueOrDefault(card.BuildingId.Value) : null;
                    result.Add(MapParking(p, card, member, cardType, zone, gates, building));
                }
            }
            catch (Exception ex)
            {
                await GData.Loggings_JpmsApi.AddLoggingAsync("PmsTicketless.SearchByLicense error: " + ex.Message);
            }
            return result;
        }

        public static async Task<List<CardCalculate>> GetDataCurrent(long startDateInt, long endDateInt, int zoneId, int placeId)
        {
            var result = new List<CardCalculate>();
            try
            {
                using var db = new PmsDbContext();

                var startDate = DateOnly.FromDateTime(ParseDateInt(startDateInt));
                var endDate = DateOnly.FromDateTime(ParseDateInt(endDateInt));

                // Get all zone gate IDs to filter by zone
                var allZones = await db.TbZones.ToListAsync();
                var targetZone = zoneId > 0 ? allZones.FirstOrDefault(z => z.ZoneId == zoneId) : null;
                var zoneGateIds = targetZone != null
                    ? ParseGateList(targetZone.GateList).Select(g => (byte)g).ToList()
                    : new List<byte>();

                var query = db.TbCurrentParkings
                    .Where(p => p.ArriveDate >= startDate && p.ArriveDate <= endDate
                             && p.DepartDate == null
                             && p.License != null && p.License.Trim().ToUpper() != "EMPTY");

                if (zoneGateIds.Any())
                    query = query.Where(p => p.EnterGate != null && zoneGateIds.Contains((byte)p.EnterGate));

                var parkings = await query.ToListAsync();
                if (!parkings.Any()) return result;

                var cardIds = parkings.Select(p => int.TryParse(p.ParkingcardId, out var cid) ? cid : 0)
                    .Where(id => id > 0).Distinct().ToList();

                var (cards, members, cardTypes, zonesByGate, buildings, gates) =
                    await LoadLookups(db, parkings, cardIds);

                foreach (var p in parkings)
                {
                    int cid = int.TryParse(p.ParkingcardId, out var tmp) ? tmp : 0;
                    cards.TryGetValue(cid, out var card);
                    var member = card?.MemberId.HasValue == true ? members.GetValueOrDefault(card.MemberId.Value) : null;
                    var cardType = card?.CardtypeId.HasValue == true ? cardTypes.GetValueOrDefault(card.CardtypeId.Value) : null;
                    var zone = zonesByGate.GetValueOrDefault(p.EnterGate ?? 0);
                    var building = card?.BuildingId.HasValue == true ? buildings.GetValueOrDefault(card.BuildingId.Value) : null;
                    result.Add(MapParking(p, card, member, cardType, zone, gates, building));
                }
            }
            catch (Exception ex)
            {
                await GData.Loggings_JpmsApi.AddLoggingAsync("PmsTicketless.GetDataCurrent error: " + ex.Message);
            }
            return result;
        }

        public static async Task<List<CardCalculate>> GetDataCurrentEmpty(long startDateInt, long endDateInt, int zoneId, int placeId)
        {
            var result = new List<CardCalculate>();
            try
            {
                using var db = new PmsDbContext();

                var startDate = DateOnly.FromDateTime(ParseDateInt(startDateInt));
                var endDate = DateOnly.FromDateTime(ParseDateInt(endDateInt));

                var allZones = await db.TbZones.ToListAsync();
                var targetZone = zoneId > 0 ? allZones.FirstOrDefault(z => z.ZoneId == zoneId) : null;
                var zoneGateIds = targetZone != null
                    ? ParseGateList(targetZone.GateList).Select(g => (byte)g).ToList()
                    : new List<byte>();

                // EMPTY = parking without license plate (NULL or empty license)
                var query = db.TbCurrentParkings
                    .Where(p => p.ArriveDate >= startDate && p.ArriveDate <= endDate
                             && p.DepartDate == null
                             && (p.License == null || p.License.Trim() == "" || p.License.Trim().ToUpper() == "EMPTY"));

                if (zoneGateIds.Any())
                    query = query.Where(p => p.EnterGate != null && zoneGateIds.Contains((byte)p.EnterGate));

                var parkings = await query.ToListAsync();
                if (!parkings.Any()) return result;

                var cardIds = parkings.Select(p => int.TryParse(p.ParkingcardId, out var cid) ? cid : 0)
                    .Where(id => id > 0).Distinct().ToList();

                var (cards, members, cardTypes, zonesByGate, buildings, gates) =
                    await LoadLookups(db, parkings, cardIds);

                foreach (var p in parkings)
                {
                    int cid = int.TryParse(p.ParkingcardId, out var tmp) ? tmp : 0;
                    cards.TryGetValue(cid, out var card);
                    var member = card?.MemberId.HasValue == true ? members.GetValueOrDefault(card.MemberId.Value) : null;
                    var cardType = card?.CardtypeId.HasValue == true ? cardTypes.GetValueOrDefault(card.CardtypeId.Value) : null;
                    var zone = zonesByGate.GetValueOrDefault(p.EnterGate ?? 0);
                    var building = card?.BuildingId.HasValue == true ? buildings.GetValueOrDefault(card.BuildingId.Value) : null;
                    result.Add(MapParking(p, card, member, cardType, zone, gates, building));
                }
            }
            catch (Exception ex)
            {
                await GData.Loggings_JpmsApi.AddLoggingAsync("PmsTicketless.GetDataCurrentEmpty error: " + ex.Message);
            }
            return result;
        }

        public static async Task<List<CardCalculate>> GetDataTranparking(long startDateInt, long endDateInt, int zoneId, int placeId, int stampId)
        {
            var result = new List<CardCalculate>();
            try
            {
                using var db = new PmsDbContext();

                var startDate = DateOnly.FromDateTime(ParseDateInt(startDateInt));
                var endDate = DateOnly.FromDateTime(ParseDateInt(endDateInt));

                var allZones = await db.TbZones.ToListAsync();
                var targetZone = zoneId > 0 ? allZones.FirstOrDefault(z => z.ZoneId == zoneId) : null;
                var zoneGateIds = targetZone != null
                    ? ParseGateList(targetZone.GateList).ToList()
                    : new List<int>();

                var query = db.TbParkings
                    .Where(p => p.DepartDate >= startDate && p.DepartDate <= endDate);

                if (zoneGateIds.Any())
                    query = query.Where(p => p.EnterGate != null && zoneGateIds.Contains((int)p.EnterGate));

                var parkings = await query.ToListAsync();
                if (!parkings.Any()) return result;

                var cardIds = parkings.Where(p => p.ParkingcardId.HasValue)
                    .Select(p => p.ParkingcardId!.Value).Distinct().ToList();

                var (cards, members, cardTypes, zonesByGate, buildings, gates) =
                    await LoadLookupsForCompleted(db, parkings, cardIds);

                foreach (var p in parkings)
                {
                    int cid = p.ParkingcardId ?? 0;
                    cards.TryGetValue(cid, out var card);
                    var member = card?.MemberId.HasValue == true ? members.GetValueOrDefault(card.MemberId.Value) : null;
                    var cardType = card?.CardtypeId.HasValue == true ? cardTypes.GetValueOrDefault(card.CardtypeId.Value) : null;
                    var zone = zonesByGate.GetValueOrDefault(p.EnterGate ?? 0);
                    var building = card?.BuildingId.HasValue == true ? buildings.GetValueOrDefault(card.BuildingId.Value) : null;
                    result.Add(MapParkingCompleted(p, card, member, cardType, zone, gates, building));
                }
            }
            catch (Exception ex)
            {
                await GData.Loggings_JpmsApi.AddLoggingAsync("PmsTicketless.GetDataTranparking error: " + ex.Message);
            }
            return result;
        }
    }
}
