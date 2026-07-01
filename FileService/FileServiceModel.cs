namespace JpmsApiCore_MySql.FileService
{
    public class FileServiceModel
    {
        public class ReturnModel
        {
            public bool Success { get; set; }
            public bool Repeat { get; set; }
            public DateTime TransDate { get; set; }
            public int MachineId { get;set; }
            public string ErrorMsg { get; set; }
            public string InnerException { get; set; }

            public ReturnModel() 
            { 
                this.Success = false; 
                this.Repeat = false; 
                this.TransDate = DateTime.Now;
                this.MachineId = 0;
                this.ErrorMsg = string.Empty;
                this.InnerException = string.Empty;
            }
        }

        public enum trans_status
        {
            processing = 1,
            finished = 2, // jexit ข้อมูลปกติ
            cancel = 3, // jexit Card ID เดียวกัน แต่มีข้อมูลข้าเข้า มากกว่า 1
            record_error = 4,
            pending = 5,
            cancelPay = 6, // jexit ยกเลิกการจ่ายเงิน
            confirmLicense = 7, // exit confirm license to gate
            notEqualLicense = 8, // not equal license member entran and exit, exit write transaction
        }

        public partial class PrkiModel
        {
            public int gate_id_in { get; set; }
            public string gate_name_in { get; set; } = "";
            public string license { get; set; } = "";
            public int province_id { get; set; }
            public long? unique_id { get; set; }
            public string cardcode { get; set; } = "";
            public int? cardtype_id { get; set; }
            public DateTime arrive_DateTime { get; set; }
            public long StartDateInt { get; set; }
            public string shift_id { get; set; } = "";
            public string Description { get; set; } = "";

            public int Place_Id { get; set; }
            public int Building_Id { get; set; }
            public string Building_Name { get; set; } = "";
            public int Zone_Id { get; set; }
            public string ZoneName { get; set; } = "";
            public string Place_Name { get; set; } = "";

            public int CompanyID { get; set; }
            public string CompanyName { get; set; } = "";

            public int BU_ID { get; set; }
            public string BU_Name { get; set; } = "";

            public string CompanyCode { get; set; } = "";
            public long? ShiftRefEntry { get; set; }

            public int? MemberID { get; set; }

            public int Vehicletype_ID { get; set; }
            public DateTime shiftLogin { get; set; }
            public long? InsAllowRef;
            public string transref_1;
        }

        public class PrkoModel
        {
            public DateTime startDate { get; set; }
            public DateTime? endDate { get; set; }
            public int entrance { get; set; }
            public int? exit { get; set; }
            public int? cardID { get; set; }
            public DateTime? shiftLogin { get; set; }
            public string license { get; set; } = "";
            public int stampID { get; set; }
            public int? MemberID { get; set; }
            public string MemberName { get; set; } = "";
            public long startDateInt { get; set; }
            public long endDateInt { get; set; }
            public int PlaceID { get; set; }
            public string PlaceName { get; set; } = "";
            public int BulidingID { get; set; }
            public string BulidingName { get; set; } = "";
            public int ZoneID { get; set; }
            public string ZoneName { get; set; } = "";
            public string CardCode { get; set; } = "";
            public string MemberCode { get; set; } = "";
            public string EntranceName { get; set; } = "";
            public string ExitName { get; set; } = "";
            public string StampCode { get; set; } = "";
            public string StampName { get; set; } = "";
            public int CardTypeID { get; set; }
            public string CardTypeName { get; set; } = "";
            public double ParkingFeeNormal { get; set; }
            public double CardLost { get; set; }
            public double ParkingFeeExtra { get; set; }
            public double ParkingFeeOvernight { get; set; }
            public double ParkingFeeStamp { get; set; }
            public double ParkingFeePay { get; set; }
            public double NormalStampFee { get; set; }
            public int CompanyID { get; set; }
            public string CompanyName { get; set; } = "";
            public int CardGroupID { get; set; }
            public string CardGroupName { get; set; } = "";
            public int? BU_ID { get; set; }
            public string BU_Name { get; set; } = "";

            // new
            public string EntranceLicense { get; set; } = "";
            public int tranParkingStatus { get; set; }
            public string CompanyCode { get; set; } = "";
            public int StaffID { get; set; }
            public string StaffCode { get; set; } = "";
            public string StaffName { get; set; } = "";

            //Sathon
            public int StampDiscountMinute_Used { get; set; } //จำนวน stamp ที่ใช้  //unit minute
            public int StampMinute_Remain { get; set; } //จำนวน stamp ที่คืน //unit minute
            public int AvailableMinuteDiscounts { get; set; } //จำนวน stamp ที่ใช้ได้ตามเงื่อนไข เช่น stamp 3 ชม, 2 ชม ... //unit minute

            ///////////////////////////////////////////
            public double OverStayFeePay { get; set; }//machine (ได้รับจาก calculatefeev2api)
            public double TenantDiscountValue { get; set; } //machine (ได้รับจาก calculatefeev2api)
            public double SecondDiscountValue { get; set; } //machine (ได้รับจาก calculatefeev2api)
            ///////////////////////////////////////////

            public string CancelPaymentReason { get; set; } = "";

            // all season
            public int nCouponOffline { get; set; }
            public List<coupon> CouponUsed { get; set; } = new List<coupon>();
            public List<coupon> CouponRemain { get; set; } = new List<coupon>();
        }

        public class coupon
        {
            public string stamp_code { get; set; }
            public int nCoupon { get; set; }

            public coupon()
            {
                this.stamp_code = "";
                this.nCoupon = 0;
            }

            public coupon(string stamp, int ncoupon)
            {
                this.stamp_code = stamp;
                this.nCoupon = ncoupon;
            }
        }
    }
}
