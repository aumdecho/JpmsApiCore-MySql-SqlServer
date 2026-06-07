using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using JPMSAPI.Models;

namespace JPMSAPI.Controllers.Ticketless
{
    [ApiController]
    public class TicketlessDataParkingApiController : ControllerBase
    {
        [Route("Api/TicketlessDataParkingApi")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (Request.Query["Search"].Any())
            {
                string Search = Request.Query["Search"].ToString();
                int ZoneID = !string.IsNullOrEmpty(Request.Query["ZoneID"]) ? Convert.ToInt32(Request.Query["ZoneID"]) : 0;
                int PlaceId = !string.IsNullOrEmpty(Request.Query["PlaceId"]) ? Convert.ToInt32(Request.Query["PlaceId"]) : 0;
                int companyID = 0;
                if (Request.Query["companyID"].Any() && !string.IsNullOrEmpty(Request.Query["companyID"]))
                    companyID = Convert.ToInt32(Request.Query["companyID"]);

                bool location = false;
                if (Request.Query["location"].Any() && !string.IsNullOrEmpty(Request.Query["location"]))
                    location = Convert.ToBoolean(Request.Query["location"]);

                return await GetBySearchStr(Search, ZoneID, PlaceId, companyID, location);
            }
            else if (Request.Query["startDateInt"].Any())
            {
                long startDateInt = !string.IsNullOrEmpty(Request.Query["startDateInt"]) ? Convert.ToInt64(Request.Query["startDateInt"]) : 0;
                long endDateInt = !string.IsNullOrEmpty(Request.Query["endDateInt"]) ? Convert.ToInt64(Request.Query["endDateInt"]) : 0;
                int zoneId = !string.IsNullOrEmpty(Request.Query["zoneId"]) ? Convert.ToInt32(Request.Query["zoneId"]) : 0;
                int placeId = !string.IsNullOrEmpty(Request.Query["placeId"]) ? Convert.ToInt32(Request.Query["placeId"]) : 0;
                int stampId = !string.IsNullOrEmpty(Request.Query["stampId"]) ? Convert.ToInt32(Request.Query["stampId"]) : 0;
                int gateIn = !string.IsNullOrEmpty(Request.Query["gateIn"]) ? Convert.ToInt32(Request.Query["gateIn"]) : 0;
                return await GetByStartEndDate(startDateInt, endDateInt, zoneId, placeId, stampId, gateIn);
            }
            else if (Request.Query["ZoneID"].Any())
            {
                int ZoneID = !string.IsNullOrEmpty(Request.Query["ZoneID"]) ? Convert.ToInt32(Request.Query["ZoneID"]) : 0;
                return await GetByZoneID(ZoneID);
            }
            else
                return BadRequest("Invalie parameter");
        }

        [Route("Api/TicketlessDataParkingApi/GetBySearchStr")]
        [HttpGet]
        public async Task<IActionResult> GetBySearchStr(string Search, int ZoneID, int PlaceId, int companyID = 0, bool location = false, long CardNumber = 0)
        {
            List<JPMSAPI.Models.CardCalculate>? rs = null;

            if (CardNumber > 0)
            {
                string CardCode = CardNumber.ToString();
                await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("Start search by card code : " + CardCode + " : ZoneID : " + ZoneID + " : PlaceID : " + PlaceId + " : CompanyID : " + companyID);
                rs = await Repo.Pms.PmsTicketless.SearchByCardCode(CardCode, ZoneID, PlaceId, companyID);
                await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("End search by card code : " + CardCode);

                if (rs != null && rs.Count > 0)
                    return Ok(rs);
                else
                    return NotFound("Error : ไม่พบหมายเลขบัตร : " + CardCode);
            }
            else
            {
                if (location == true)
                {
                    if (Search.Length == 7)
                    {
                        await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("Start search by ticketno : " + Search);
                        rs = await Repo.Pms.PmsTicketless.SearchByTicket(Search, ZoneID, PlaceId, companyID);
                        await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("End search by ticketno : " + Search);

                        if (rs != null && rs.Count > 0)
                            return Ok(rs);
                        else
                            return NotFound("Error : ticket no : " + Search);
                    }
                    else
                    {
                        return NotFound("Error : TicketNo ไม่ถูกต้อง");
                    }
                }
                else
                {
                    if (Search.Length == 10)
                    {
                        await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("Start search by Tel : " + Search);
                        rs = await Repo.Pms.PmsTicketless.SearchByTelephone(Search, ZoneID, PlaceId, companyID);
                        await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("End search by Tel : " + Search);

                        if (rs != null && rs.Count > 0)
                            return Ok(rs);
                        else
                            return NotFound("Error : ไม่พบเบอร์โทร : " + Search);
                    }
                    else if (Search.Length == 8)
                    {
                        await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("Start search by card code : " + Search);
                        rs = await Repo.Pms.PmsTicketless.SearchByCardCode(Search, ZoneID, PlaceId, companyID);
                        await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("End search by card code : " + Search);

                        if (rs != null && rs.Count > 0)
                            return Ok(rs);
                        else
                            return NotFound("Error : ไม่พบหมายเลขบัตร : " + Search);
                    }
                    else if (Search.Length == 19)
                    {
                        await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("Start search by TranRef : " + Search);
                        rs = await Repo.Pms.PmsTicketless.SearchByTransRef(Search, ZoneID, PlaceId, companyID);
                        await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("End search by TranRef : " + Search);

                        if (rs != null && rs.Count > 0)
                            return Ok(rs);
                        else
                            return NotFound("Error : ไม่พบ TransRef : " + Search);
                    }
                    else
                    {
                        await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("Start search by License : " + Search);
                        rs = await Repo.Pms.PmsTicketless.SearchByLicense(Search, ZoneID, PlaceId, companyID);
                        await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("End search by License : " + Search);

                        if (rs != null && rs.Count > 0)
                            return Ok(rs);
                        else
                            return NotFound("Error : ไม่พบทะเบียน : " + Search);
                    }
                }
            }
        }

        [Route("Api/TicketlessDataParkingApi/GetByStartEndDate")]
        [HttpGet]
        public async Task<IActionResult> GetByStartEndDate(long startDateInt, long endDateInt, int zoneId, int placeId, int stampId, int gateIn)
        {
            TicketlessCheckTran rs = new TicketlessCheckTran();
            try
            {
                if (gateIn == 0)
                {
                    List<JPMSAPI.Models.CardCalculate> dataCurrent = await Repo.Pms.PmsTicketless.GetDataCurrent(startDateInt, endDateInt, zoneId, placeId);
                    if (dataCurrent == null || dataCurrent.Count == 0)
                        return NotFound("Error : no data found");
                    rs.DataCurrent.AddRange(dataCurrent);
                }
                else if (gateIn == 1)
                {
                    List<JPMSAPI.Models.CardCalculate> dataCurrent = await Repo.Pms.PmsTicketless.GetDataCurrentEmpty(startDateInt, endDateInt, zoneId, placeId);
                    if (dataCurrent == null || dataCurrent.Count == 0)
                        return NotFound("Error : no data found");
                    rs.DataCurrent.AddRange(dataCurrent);
                }

                List<JPMSAPI.Models.CardCalculate> dataTansaction = await Repo.Pms.PmsTicketless.GetDataTranparking(startDateInt, endDateInt, zoneId, placeId, stampId);
                if (dataTansaction == null || dataTansaction.Count == 0)
                    return NotFound("Error : no transaction data found");
                rs.DataTran.AddRange(dataTansaction);
            }
            catch (Exception ex)
            {
                return NotFound("Error : " + ex.ToString());
            }
            return Ok(rs);
        }

        [Route("Api/TicketlessDataParkingApi/GetByZoneID")]
        [HttpGet]
        public async Task<IActionResult> GetByZoneID(int ZoneID)
        {
            // tranemptylicense (CardTypeId=-2) has no equivalent in SQL Server schema.
            // Return empty list to maintain backward-compatible response structure.
            return Ok(new List<JPMSAPI.Models.CardCalculate>());
        }
    }
}
