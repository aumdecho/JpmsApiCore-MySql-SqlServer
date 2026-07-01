using CHOCore.Json;
using JPMSAPI.Repo;
using JpmsApiCore_MySql.Database.SqlServer;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static JpmsApiCore_MySql.FileService.FileServiceModel;

namespace JpmsApiCore_MySql.FileService
{
    public class Prki
    {
        public async Task<ReturnModel> ProcessPrki(string Value, string FileFullName)
        {
            ReturnModel result = new ReturnModel();
            try
            {
                var tranParkingIn = Value.ToClass<FileServiceModel.PrkiModel>();
                if (tranParkingIn == null)
                {
                    await GData.Loggings_FileService.AddLoggingAsync("Prki Error convert To Class : " + Value);
                    result.ErrorMsg = "PrkiFormat_Eror";
                    return result;
                }

                tranParkingIn.arrive_DateTime = new DateTime(
                    tranParkingIn.arrive_DateTime.Year, tranParkingIn.arrive_DateTime.Month,
                    tranParkingIn.arrive_DateTime.Day, tranParkingIn.arrive_DateTime.Hour,
                    tranParkingIn.arrive_DateTime.Minute, tranParkingIn.arrive_DateTime.Second);

                if (!string.IsNullOrEmpty(tranParkingIn.license))
                    tranParkingIn.license = tranParkingIn.license.Trim();

                if (int.TryParse(tranParkingIn.license, out _))
                    tranParkingIn.license = tranParkingIn.license + " " + System.Environment.NewLine;

                string tranMachine = tranParkingIn.gate_id_in.ToString();
                string tranDateTime = tranParkingIn.arrive_DateTime.ToString("yyyyMMdd", GData.cultureEn);

                if (FileServiceFunctioncs.CheckDuplicatedFile(FileFullName, tranMachine, tranDateTime))
                {
                    await GData.Loggings_FileService.AddLoggingAsync("Duplicate File: " + FileFullName);
                    result.ErrorMsg = "FileDuplicated";
                    return result;
                }

                // PARKING_ID = yyyyMMddHHmmss + gate_id (2 digits)
                string pidStr = tranParkingIn.arrive_DateTime.ToString("yyyyMMddHHmmss", GData.cultureEn)
                                + tranParkingIn.gate_id_in.ToString("D2");
                long parkingId = long.Parse(pidStr);

                using (var db = new PmsDbContext())
                {
                    if (await db.TbCurrentParkings.AnyAsync(f => f.ParkingId == parkingId))
                    {
                        await GData.Loggings_FileService.AddLoggingAsync("Prki already found in TB_CURRENTPARKING ParkingId : " + parkingId + " Data : " + Value);
                        result.ErrorMsg = "DuplicatedTranparking";
                        return result;
                    }

                    var gate = await db.TbGates.FirstOrDefaultAsync(f => f.Id == tranParkingIn.gate_id_in);
                    if (gate == null)
                    {
                        await GData.Loggings_FileService.AddLoggingAsync("Prki ไม่พบข้อมูล Gate In Id : " + tranParkingIn.gate_id_in + " Data : " + Value);
                        result.ErrorMsg = "MachineId_NotFound";
                        return result;
                    }

                    string cardIdStr = "0";
                    if (tranParkingIn.unique_id > 0)
                    {
                        // DB stores UNIQUE_ID as 8-digit uppercase hex (e.g. "009402A1")
                        string uniqueIdStr = tranParkingIn.unique_id.Value.ToString("X8");
                        var card = await db.TbParkingCards
                            .FirstOrDefaultAsync(f => f.UniqueId == uniqueIdStr && f.Active == 1);
                        if (card != null)
                            cardIdStr = card.ParkingcardId.ToString();
                        else
                            await GData.Loggings_FileService.AddLoggingAsync("Prki ไม่พบ Card UniqueId : " + uniqueIdStr + " (decimal=" + tranParkingIn.unique_id + ") ใช้ cardId=0");
                    }

                    var entry = new TbCurrentParking
                    {
                        ParkingId   = parkingId,
                        ParkingcardId = cardIdStr,
                        ArriveDate  = DateOnly.FromDateTime(tranParkingIn.arrive_DateTime),
                        ArriveTime  = TimeOnly.FromDateTime(tranParkingIn.arrive_DateTime),
                        EnterGate   = (byte?)tranParkingIn.gate_id_in,
                        License     = tranParkingIn.license ?? "",
                        ProvinceId  = tranParkingIn.province_id > 0 ? (byte?)tranParkingIn.province_id : null,
                    };

                    await db.TbCurrentParkings.AddAsync(entry);
                    await db.SaveChangesAsync();
                    await GData.Loggings_FileService.AddLoggingAsync("Prki add into TB_CURRENTPARKING ParkingId : " + parkingId);
                }

                result.Success = true;
                result.Repeat = false;
                result.TransDate = tranParkingIn.arrive_DateTime;
                result.MachineId = tranParkingIn.gate_id_in;
            }
            catch (Exception ex)
            {
                await GData.Loggings_FileService.AddLoggingAsync("Prki Error : " + ex.ToString());
                result.Success = false;
                result.Repeat = true;
                result.ErrorMsg = ex.ToString();
                result.InnerException = ex.InnerException != null ? ex.InnerException.Message : "Unknown Exception";
            }
            return result;
        }

    }
}
