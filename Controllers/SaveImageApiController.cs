using JPMSAPI.Repo;
using JpmsApiCore_MySql.Database.SqlServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Drawing;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JPMSAPI.Controllers
{
    [Route("Api/[Controller]")]
    [ApiController]
    public class SaveImageApiController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post(DateTime TranDate, int gateID, string imageType, string CardCode)
        {
            await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("SaveImageAPI Start : TranDate = " + TranDate.ToString() + " gateId = " + gateID + " imageType = " + imageType);
            ////await Repo.GData.Loggings_JpmsApi.AddLoggingAsync($"Headers: CL={Request.ContentLength}, TE={Request.Headers["Transfer-Encoding"]}, CT={Request.ContentType}, IP={HttpContext.Connection.RemoteIpAddress}");

            await Repo.GData.Loggings_JpmsApi.AddLoggingAsync($"RID={HttpContext.TraceIdentifier} " +
          $"CL={Request.ContentLength} " +
          $"TE={Request.Headers["Transfer-Encoding"]} " +
          $"CT={Request.ContentType} " +
          $"Expect={Request.Headers["Expect"]} " +
          $"UA={Request.Headers["User-Agent"]} " +
          $"IP={HttpContext.Connection.RemoteIpAddress}");

            //// TE = chunked + CL = ว่าง → ส่งแบบ chunked
            ////CT = multipart / form - data → ส่งแบบ multipart
            ////CL = 0 → sender ส่งว่างจริง(หรือส่งผิด)

            try
            {
                DateTime dtArrive = TranDate; //DateTime.ParseExact(TranDate, "yyyyMMddHHmmss", en);
                string day = dtArrive.ToString("yyyyMMdd", Repo.GData.cultureEn);

                string folderGateId;
                using (var db = new PmsDbContext())
                {
                    var gate = await db.TbGates.FirstOrDefaultAsync(g => g.Id == gateID);
                    folderGateId = gate?.Name ?? "Gate" + gateID.ToString();
                }

                await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("SaveImageAPI : TranDate = " + TranDate.ToString("yy-MM-dd HH:mm:ss", Repo.GData.cultureEn) + " gateId = " + gateID + " imageType = " + imageType);

                string IMGPath = GData.PicturePath;
                if (!Directory.Exists(IMGPath + "\\" + folderGateId + "\\" + day + "\\" + imageType))
                {
                    Directory.CreateDirectory(IMGPath + "\\" + folderGateId + "\\" + day + "\\" + imageType);
                }

                string time = dtArrive.ToString("yyyyMMddHHmmss", Repo.GData.cultureEn);
                //string FileName = time + gateID.ToString("D7") + ".jpg";
                string FileName = time + CardCode + ".jpg";

                ////////////////////////////////////////////////////////////////////////

                string fullName = IMGPath + "\\" + folderGateId + "\\" + day + "\\" + imageType + "\\" + FileName;

                // รองรับทั้ง Content-Length และ TE=chunked
                await using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                {
                    await Request.Body.CopyToAsync(fs);
                    ////await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("SaveImageAPI Success : TranDate = " + TranDate.ToString("yy-MM-dd HH:mm:ss", Repo.GData.cultureEn) + " gateId = " + gateID + " imageType = " + imageType);
                }

                // ตรวจว่ามีข้อมูลจริง
                //////if (new FileInfo(fullName).Length == 0)
                //////{
                //////    System.IO.File.Delete(fullName);

                //////    await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("SaveImageAPI fullName = " + fullName + " : return Ok(Body empty)");
                //////    return Ok("Body empty");
                //////}

                ////////////////////////////////////////////////////////////////////////

                //////if (Request.Body != null)
                //////{
                ////Stream memoryStream = Request.BodyReader.AsStream();
                ////Image img = Image.FromStream(memoryStream);
                ////img.Save(IMGPath + "\\" + day + "\\" + folderGateId + "\\" + imageType + "\\" + "Test.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                /////////////////////////////////////////////////////////////////////////
                //////int len = (int)Request.ContentLength.GetValueOrDefault();
                //////if (len <= 0)
                //////{
                //////    await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("GateId = " + gateID + " image = null");
                //////    return Ok("Body Len = 0");
                //////}

                ////////////////////////////////////////////////////////////////////////////////

                // If Content-Length exists and is 0, it's definitely empty.
                //////if (Request.ContentLength.HasValue && Request.ContentLength.Value == 0)
                //////{
                //////    await Repo.GData.Loggings_JpmsApi.AddLoggingAsync($"RID={HttpContext.TraceIdentifier} GateId={gateID} Body Len = 0 (Content-Length=0)");
                //////    return Ok("Body Len = 0");
                //////}

                /////////////////////////////////////////////////////////////////////////

                ////// byte[] bytes = new byte[len];
                ////// await Request.Body.ReadExactlyAsync(bytes, 0, bytes.Length);

                ////// /////////////////////////////////////////////////////////////////////////

                ////// string fullName = IMGPath + "\\" + day + "\\" + folderGateId + "\\" + imageType + "\\" + FileName;
                //////await using (FileStream fs = new(fullName, FileMode.OpenOrCreate, FileAccess.Write))
                ////// {
                //////     await fs.WriteAsync(bytes.AsMemory(0, len));
                ////// }

                await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("SaveImageAPI  Ok(Success) : TranDate = " + TranDate.ToString("yy-MM-dd HH:mm:ss", Repo.GData.cultureEn) + " gateId = " + gateID + " imageType = " + imageType);
                return Ok("Success");
                //////}
                //////else
                //////{
                //////    await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("SaveImageAPI Ok(Body = null) : TranDate = " + TranDate.ToString("yy-MM-dd HH:mm:ss", Repo.GData.cultureEn) + " gateId = " + gateID + " imageType = " + imageType);
                //////    return Ok("Body = null");
                //////}
            }
            catch (Exception ex)
            {
                await Repo.GData.Loggings_JpmsApi.AddLoggingAsync("SaveImageAPI Exception : TranDate = " + TranDate.ToString("yy-MM-dd HH:mm:ss", Repo.GData.cultureEn) + " gateId = " + gateID + " imageType = " + imageType + " : " + ex.ToString());
                return BadRequest(ex.ToString());
            }
        }

        private async Task sendApi()
        {
            var options = new RestClientOptions("https://localhost:7232")
            {
                //ME- MaxTimeout = -1,
            };

            var client = new RestClient(options);
            var request = new RestRequest("/Api/GateINADriverapi?Arrive_DateTime=20240213075150&gateID=2528", Method.Post);
            request.AddHeader("accept", "*/*");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("C", "209715");

            byte[] bytes = new byte[1024];

            request.AddParameter("application/json", bytes, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
        }
    }
}
