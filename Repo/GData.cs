using JPMSAPI.Models;
using JpmsApiCore_MySql.Database;
using JpmsApiCore_MySql.Database.SqlServer;
using JpmsApiCore_MySql.FileService;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JPMSAPI.Repo
{


    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
                return null; // ✅ ให้ผ่านและตีเป็น null

            if (DateTime.TryParseExact(
                    value,
                    Formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var dt))
            {
                return dt;
            }

            // ✅ รองรับ ISO เช่น 2026-02-05T13:49:11.059335+07:00
            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out dt))
            {
                return dt;
            }

            throw new JsonException($"Invalid datetime format: {value}");
        }
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();   // ✅ null ออกมาแน่นอน
                return;
            }
            writer.WriteStringValue(value.Value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
        }

        private static readonly string[] Formats =
            {
                "yyyyMMddHHmmss",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyyMMdd",
                "yyyy-MM-ddTHH:mm:ss.ffffffK"

            };
    }

    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
#pragma warning disable CS8604 // Possible null reference argument.

            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
                throw new JsonException("DateTime is null or empty");

            if (DateTime.TryParseExact(value, Formats,
    CultureInfo.InvariantCulture,
    DateTimeStyles.None,
    out var dt))
            {
                return dt;
            }

            throw new JsonException($"Invalid datetime format: {value}");

            ////return DateTime.Parse(reader.GetString());
#pragma warning restore CS8604 // Possible null reference argument.
        }



        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
        }

        private static readonly string[] Formats =
        {
            "yyyyMMddHHmmss",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyyMMdd",
            "yyyy-MM-ddTHH:mm:ss.ffffffK",
            "yyyy-MM-dd'T'HH:mm:ss.fffK",
            "yyyy-MM-ddTHH:mm:ssK",
            "O"   // ⭐ MUST HAVE

        };
    }

    public static class GData
    {
        public static string LogPath = AppDomain.CurrentDomain.BaseDirectory + "\\Logging\\";

        public static CHOCore.Logging.CHOLogging Log = new CHOCore.Logging.CHOLogging("Loggings", AppDomain.CurrentDomain.BaseDirectory + @"\Logging\");

        public static int Partition = 100;

        public static int DiscountPerCoupon = 30;
        public static System.Globalization.CultureInfo cultureEn = new System.Globalization.CultureInfo("en-US");

        public static string ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["JudjodDbEntities"] ?? "";

        public static string TransactionFilePath = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["TransSactionFilePath"] ?? "C:\\Infinite";
        public static string TransFilePath = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["TransFilePath"] ?? "C:\\Infinite";
        public static int DelayTimeFile = Convert.ToInt32(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["DelayTimeFile"] ?? "200");
        public static string PlaceIDList = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["PlaceIDList"] ?? "";
        public static int TimeStartGenReport = Convert.ToInt32(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["TimeStartGenReport"] ?? "200");
        public static int TiemEndGenReport = Convert.ToInt32(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["TiemEndGenReport"] ?? "200");
        public static int TimeExportReport = Convert.ToInt32(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["TimeExportReport"] ?? "200");
        public static int DelayTimeExportFile = Convert.ToInt32(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["DelayTimeExportFile"] ?? "200");
        public static int timeSleepSuccess = Convert.ToInt32(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["TimeSleepSuccess"] ?? "10");
        public static int timeSleepError = Convert.ToInt32(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["TimeSleepError"] ?? "100");
        public static int TotalThread = Convert.ToInt32(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["DelayTimeFile"] ?? "0");
        public static int AgeCreation = Convert.ToInt32(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["DelayTimeFile"] ?? "0");

        public static string LineToken = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["LineToken"] ?? "";


        public static int MaxRetry = Convert.ToInt32(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["MaxRetry"] ?? "5");

        public static string PicturePath = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Settings")["PicturePath"] ?? GData.TransactionFilePath + "\\Picture";
        public static string TransBackupPath = GData.TransFilePath + "\\TransBackupPath\\";
        public static string TransSuccessPath = GData.TransFilePath + "\\TransSuccessPath\\";
        public static string TransTempErrorPath = GData.TransFilePath + "\\TransTempErrorPath\\";
        public static string TransErrorPath = GData.TransFilePath + "\\TransErrorPath\\";
        public static string RateExtensionPath = AppDomain.CurrentDomain.BaseDirectory + @"\RateExtension\";

        public static CHOCore.Logging.CHOLogging Loggings_FileService = new CHOCore.Logging.CHOLogging("FileService", LogPath);

        public static CHOCore.Logging.CHOLogging Loggings_JpmsApi = new CHOCore.Logging.CHOLogging("JpmsApi", LogPath);
        public static CHOCore.Logging.CHOLogging Loggings_Calfee = new CHOCore.Logging.CHOLogging("CalFee", LogPath);

        public static void InitialGData()
        {
            if (!Directory.Exists(PicturePath)) Directory.CreateDirectory(PicturePath);
            if (!Directory.Exists(RateExtensionPath)) Directory.CreateDirectory(RateExtensionPath);
            if (!Directory.Exists(TransSuccessPath)) Directory.CreateDirectory(TransSuccessPath);
            if (!Directory.Exists(TransTempErrorPath)) Directory.CreateDirectory(TransTempErrorPath);
            if (!Directory.Exists(TransErrorPath)) Directory.CreateDirectory(TransErrorPath);

            Loggings_FileService.CreateNewFileEveryday = true;
            Loggings_FileService.Enable = true;

            Loggings_JpmsApi.CreateNewFileEveryday = true;
            Loggings_JpmsApi.Enable = true;

            Loggings_Calfee.CreateNewFileEveryday = true;
            Loggings_Calfee.Enable = true;
        }
    }

    class Checkmember
    {
        static List<int> ParseGateList(string gateList)
        {
            if (string.IsNullOrEmpty(gateList)) return new List<int>();
            return gateList.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                .Where(n => n > 0).ToList();
        }

        public async Task<Models.checkMember> CheckMember(int GateId, string cardNo, string LicenseNumber)
        {
            Models.checkMember result = new Models.checkMember();
            if (string.IsNullOrEmpty(LicenseNumber) || LicenseNumber.Contains("0000"))
                LicenseNumber = "";
            result.License = LicenseNumber;

            try
            {
                await Repo.GData.Log.AddLoggingAsync("Start check CheckMemberCardAndLicense GateId = " + GateId + " LicenseNumber = " + LicenseNumber);
                using (var db = new JpmsApiCore_MySql.Database.SqlServer.PmsDbContext())
                {
                    DateTime timenow = DateTime.Now;

                    var gate = await db.TbGates.Where(g => g.Id == GateId).FirstOrDefaultAsync();
                    if (gate == null)
                    {
                        await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense error Not found GateId " + GateId);
                        return result;
                    }

                    int GateTypeId = gate.GatetypeId;

                    // Find zone for this gate by parsing GATE_LIST
                    var allZones = await db.TbZones.ToListAsync();
                    var gateZone = allZones.FirstOrDefault(z => ParseGateList(z.GateList).Contains(GateId));
                    int zoneId = gateZone?.ZoneId ?? 0;

                    int MemberId = 0;
                    int CardId = 0;
                    var today = DateOnly.FromDateTime(timenow);

                    if (!string.IsNullOrEmpty(cardNo))
                    {
                        TbParkingCard? card = null;

                        // CardNo can be: decimal UID (e.g. "13921433"), hex UID (e.g. "00D46C99"), or card code (e.g. "CM01033")
                        string? resolvedUniqueIdHex = null;
                        if (long.TryParse(cardNo, out long cardUid))
                        {
                            // Decimal UID → convert to 8-digit uppercase hex
                            resolvedUniqueIdHex = cardUid.ToString("X8");
                        }
                        else if (cardNo.Length <= 16 && long.TryParse(cardNo, System.Globalization.NumberStyles.HexNumber, null, out long hexVal))
                        {
                            // Hex string UID (e.g. "00D46C99") → uppercase and pad to 8
                            resolvedUniqueIdHex = hexVal.ToString("X8");
                        }

                        if (resolvedUniqueIdHex != null)
                        {
                            card = await db.TbParkingCards
                                .Where(c => c.UniqueId == resolvedUniqueIdHex && c.ZoneId == zoneId
                                         && c.IssueDate <= today && c.ExpireDate >= today)
                                .FirstOrDefaultAsync();
                            await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense lookup by UniqueId hex=" + resolvedUniqueIdHex + " (CardNo=" + cardNo + ")");
                        }
                        else
                        {
                            // Alpha card code (e.g. "CM01033") — search by PARKINGCARD_CODE
                            card = await db.TbParkingCards
                                .Where(c => c.ParkingcardCode == cardNo && c.ZoneId == zoneId
                                         && c.IssueDate <= today && c.ExpireDate >= today)
                                .FirstOrDefaultAsync();
                        }

                        if (card != null && card.Active == 1 && card.MemberId > 0)
                        {
                            MemberId = card.MemberId.GetValueOrDefault();
                            CardId = card.ParkingcardId;
                        }
                        else
                        {
                            await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense CardCode = " + cardNo + " is not member.");
                            return result;
                        }
                    }
                    else if (!string.IsNullOrEmpty(LicenseNumber))
                    {
                        // Find vehicle by license, then find card via PARKINGCARDMAP
                        var vehicle = await db.TbVehicles
                            .Where(v => v.VehicleLicense == LicenseNumber && v.Active == 1)
                            .FirstOrDefaultAsync();

                        if (vehicle != null)
                        {
                            var cardMap = await db.TbParkingCardMaps
                                .Where(m => m.VehicleId == vehicle.VehicleId)
                                .FirstOrDefaultAsync();

                            if (cardMap != null)
                            {
                                var card = await db.TbParkingCards
                                    .Where(c => c.ParkingcardId == cardMap.ParkingcardId && c.ZoneId == zoneId)
                                    .FirstOrDefaultAsync();

                                if (card != null && card.MemberId > 0)
                                {
                                    if (card.Active == 1 && card.IssueDate <= today && card.ExpireDate >= today)
                                    {
                                        MemberId = card.MemberId.GetValueOrDefault();
                                        CardId = card.ParkingcardId;
                                    }
                                    else
                                    {
                                        await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense License " + LicenseNumber + " card expired.");
                                        return result;
                                    }
                                }
                                else
                                {
                                    await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense License " + LicenseNumber + " is not member.");
                                    return result;
                                }
                            }
                            else
                            {
                                await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense License " + LicenseNumber + " no card mapping.");
                                return result;
                            }
                        }
                        else
                        {
                            await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense License " + LicenseNumber + " not found in TB_VEHICLE.");
                            return result;
                        }
                    }
                    else
                    {
                        return result;
                    }

                    result.IsMember = true;
                    result.MemberId = MemberId;
                    await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense CardNo = " + cardNo + " License " + LicenseNumber + " is member, checking first entry.");

                    string cardIdStr = CardId.ToString();

                    if (GateTypeId == (int)Models.MachineType.GateIn)
                    {
                        // Check if card is already parked (no DEPART_DATE = still inside)
                        var activeParking = await db.TbCurrentParkings
                            .Where(p => p.ParkingcardId == cardIdStr && p.DepartDate == null)
                            .ToListAsync();

                        if (activeParking.Count == 0)
                        {
                            result.IsFirstEnter = true;
                            result.LicenseFirstEnter = LicenseNumber;
                            await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense License " + LicenseNumber + " is first entry.");
                            return result;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(LicenseNumber) && activeParking.Any(p => p.License == LicenseNumber))
                            {
                                result.IsFirstEnter = true;
                                result.LicenseFirstEnter = LicenseNumber;
                                await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense License " + LicenseNumber + " same license re-entry.");
                                return result;
                            }
                            else
                            {
                                string firstLicense = activeParking.First().License ?? "";
                                result.LicenseFirstEnter = !string.IsNullOrEmpty(firstLicense) && !firstLicense.Contains("000")
                                    ? firstLicense
                                    : "Card" + cardNo;
                                result.IsFirstEnter = false;
                                await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense License " + LicenseNumber + " not first entry, existing: " + result.LicenseFirstEnter);
                                return result;
                            }
                        }
                    }
                    else if (GateTypeId == (int)Models.MachineType.GateOut)
                    {
                        var activeParking = await db.TbCurrentParkings
                            .Where(p => p.ParkingcardId == cardIdStr && p.DepartDate == null)
                            .ToListAsync();

                        if (activeParking.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(LicenseNumber) && activeParking.Any(p => p.License == LicenseNumber))
                            {
                                result.IsFirstEnter = true;
                                result.LicenseFirstEnter = LicenseNumber;
                                await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense CardNo " + cardNo + " License " + LicenseNumber + " gate out first entry.");
                                return result;
                            }

                            if (!string.IsNullOrEmpty(cardNo))
                            {
                                result.IsFirstEnter = true;
                                result.LicenseFirstEnter = "Card" + cardNo;
                                await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense CardNo " + cardNo + " gate out by card.");
                                return result;
                            }

                            await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense CardNo " + cardNo + " License " + LicenseNumber + " not first entry.");
                        }
                        else
                        {
                            result.IsFirstEnter = false;
                            result.LicenseFirstEnter = LicenseNumber;
                            await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense CardNo " + cardNo + " no active parking found.");
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Repo.GData.Log.AddLoggingAsync("Exception In CheckMemberCardAndLicense CardNo = " + cardNo + " License = " + LicenseNumber + " : " + ex.ToString());
            }
            await Repo.GData.Log.AddLoggingAsync("CheckMemberCardAndLicense CardNo = " + cardNo + " License = " + LicenseNumber + " not a first entry Exit.");
            return result;
        }
    }
}