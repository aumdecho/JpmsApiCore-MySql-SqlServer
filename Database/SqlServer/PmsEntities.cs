using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JpmsApiCore_MySql.Database.SqlServer
{
    [Table("TB_GATE")]
    public class TbGate
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME")]
        public string? Name { get; set; }

        [Column("DESCRIPTION")]
        public string Description { get; set; } = "";

        [Column("STATE")]
        public int? State { get; set; }

        [Column("GATETYPE_ID")]
        public int GatetypeId { get; set; }

        [Column("GATE_CODE")]
        public string? GateCode { get; set; }

        [Column("GATEGROUP_ID")]
        public int GategroupId { get; set; }

        [Column("GATE_REF")]
        public string? GateRef { get; set; }

        [Column("BUILDING_ID")]
        public int BuildingId { get; set; }
    }

    [Table("TB_ZONE")]
    public class TbZone
    {
        [Key]
        [Column("ZONE_ID")]
        public int ZoneId { get; set; }

        [Column("ZONE_NAME")]
        public string ZoneName { get; set; } = "";

        [Column("ZONE_DESC")]
        public string ZoneDesc { get; set; } = "";

        [Column("GATE_LIST")]
        public string GateList { get; set; } = "";
    }

    [Table("TB_BUILDING")]
    public class TbBuilding
    {
        [Key]
        [Column("BUILDING_ID")]
        public int BuildingId { get; set; }

        [Column("BUILDING_NAME")]
        public string BuildingName { get; set; } = "";

        [Column("CAR_SPACE")]
        public int CarSpace { get; set; }

        [Column("MOTORCYCLE_SPACE")]
        public int MotorcycleSpace { get; set; }
    }

    [Table("TB_SYSTEMINFO")]
    public class TbSystemInfo
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NAME_TH")]
        public string NameTh { get; set; } = "";

        [Column("NAME_EN")]
        public string NameEn { get; set; } = "";

        [Column("ADDRESS")]
        public string Address { get; set; } = "";

        [Column("TELEPHONE")]
        public string Telephone { get; set; } = "";

        [Column("FAX")]
        public string Fax { get; set; } = "";

        [Column("TAX_ID")]
        public string TaxId { get; set; } = "";

        [Column("VAT_RATE")]
        public float VatRate { get; set; }

        [Column("OVERNIGHT")]
        public TimeOnly? Overnight { get; set; }

        [Column("SYSTEM_CODE")]
        public string? SystemCode { get; set; }
    }

    [Table("TB_COMPANYINFO")]
    public class TbCompanyInfo
    {
        [Key]
        [Column("COMPANY_ID")]
        public int CompanyId { get; set; }

        [Column("COMPANY_CODE")]
        public string? CompanyCode { get; set; }

        [Column("COMPANY_NAME")]
        public string? CompanyName { get; set; }

        [Column("COMPANY_ADDRESS")]
        public string? CompanyAddress { get; set; }

        [Column("COMPANY_TELEPHONE")]
        public string? CompanyTelephone { get; set; }

        [Column("BUILDING_ID")]
        public int? BuildingId { get; set; }

        [Column("ACTIVE")]
        public string? Active { get; set; }

        [Column("RECORDER_ID")]
        public int RecorderId { get; set; }

        [Column("RECORD_DATE")]
        public DateTime RecordDate { get; set; }
    }

    [Table("TB_PARKINGCARD")]
    public class TbParkingCard
    {
        [Key]
        [Column("PARKINGCARD_ID")]
        public int ParkingcardId { get; set; }

        [Column("PARKINGCARD_CODE")]
        public string ParkingcardCode { get; set; } = "";

        [Column("CARDTYPE_ID")]
        public int? CardtypeId { get; set; }

        [Column("MEMBER_ID")]
        public int? MemberId { get; set; }

        [Column("COMPANY_ID")]
        public int? CompanyId { get; set; }

        [Column("BUILDING_ID")]
        public int? BuildingId { get; set; }

        [Column("ZONE_ID")]
        public int? ZoneId { get; set; }

        [Column("ISSUE_DATE")]
        public DateOnly IssueDate { get; set; }

        [Column("ISSUE_TIME")]
        public TimeOnly IssueTime { get; set; }

        [Column("EXPIRE_DATE")]
        public DateOnly ExpireDate { get; set; }

        [Column("EXPIRE_TIME")]
        public TimeOnly ExpireTime { get; set; }

        [Column("PARKINGCARD_STATE")]
        public int? ParkingcardState { get; set; }

        [Column("ACTIVE")]
        public int? Active { get; set; }

        [Column("ENTER_STATE")]
        public int? EnterState { get; set; }

        [Column("UNIQUE_ID")]
        public string UniqueId { get; set; } = "";

        [Column("RECORDER_ID")]
        public int RecorderId { get; set; }

        [Column("RECORD_DATE")]
        public DateTime RecordDate { get; set; }
    }

    [Table("TB_PARKINGCARDMAP")]
    public class TbParkingCardMap
    {
        [Column("PARKINGCARD_ID")]
        public int ParkingcardId { get; set; }

        [Column("VEHICLE_ID")]
        public int VehicleId { get; set; }
    }

    [Table("TB_CARDTYPE")]
    public class TbCardType
    {
        [Key]
        [Column("CARDTYPE_ID")]
        public int CardtypeId { get; set; }

        [Column("CARDTYPE_NAME")]
        public string CardtypeName { get; set; } = "";

        [Column("CARDGROUP_ID")]
        public int? CardgroupId { get; set; }

        [Column("RECORDER_ID")]
        public int RecorderId { get; set; }

        [Column("RECORD_DATE")]
        public DateTime RecordDate { get; set; }
    }

    [Table("TB_MEMBER")]
    public class TbMember
    {
        [Key]
        [Column("MEMBER_ID")]
        public int MemberId { get; set; }

        [Column("MEMBER_CODE")]
        public string? MemberCode { get; set; }

        [Column("MEMBER_NAME")]
        public string? MemberName { get; set; }

        [Column("MEMBER_TELEPHONE")]
        public string? MemberTelephone { get; set; }

        [Column("ACTIVE")]
        public int? Active { get; set; }

        [Column("COMPANY_ID")]
        public int? CompanyId { get; set; }

        [Column("RECORD_DATE")]
        public DateTime RecordDate { get; set; }
    }

    [Table("TB_VEHICLE")]
    public class TbVehicle
    {
        [Key]
        [Column("VEHICLE_ID")]
        public int VehicleId { get; set; }

        [Column("VEHICLE_LICENSE")]
        public string VehicleLicense { get; set; } = "";

        [Column("ACTIVE")]
        public int? Active { get; set; }
    }

    [Table("TB_CURRENTPARKING")]
    public class TbCurrentParking
    {
        [Key]
        [Column("PARKING_ID")]
        public long ParkingId { get; set; }

        [Column("PARKINGCARD_ID")]
        public string ParkingcardId { get; set; } = "";

        [Column("ARRIVE_DATE")]
        public DateOnly ArriveDate { get; set; }

        [Column("ARRIVE_TIME")]
        public TimeOnly ArriveTime { get; set; }

        [Column("DEPART_DATE")]
        public DateOnly? DepartDate { get; set; }

        [Column("DEPART_TIME")]
        public TimeOnly? DepartTime { get; set; }

        [Column("ENTER_GATE")]
        public byte? EnterGate { get; set; }

        [Column("EXIT_GATE")]
        public byte? ExitGate { get; set; }

        [Column("PARKING_MINUTE")]
        public int? ParkingMinute { get; set; }

        [Column("STATE")]
        public byte? State { get; set; }

        [Column("LICENSE")]
        public string? License { get; set; }

        [Column("PROVINCE_ID")]
        public byte? ProvinceId { get; set; }

        [Column("STAMP_ID")]
        public int? StampId { get; set; }
    }

    [Table("TB_PARKING")]
    public class TbParking
    {
        [Key]
        [Column("PARKING_ID")]
        public long ParkingId { get; set; }

        [Column("PARKINGCARD_ID")]
        public int? ParkingcardId { get; set; }

        [Column("ARRIVE_DATE")]
        public DateOnly? ArriveDate { get; set; }

        [Column("ARRIVE_TIME")]
        public TimeOnly? ArriveTime { get; set; }

        [Column("DEPART_DATE")]
        public DateOnly? DepartDate { get; set; }

        [Column("DEPART_TIME")]
        public TimeOnly? DepartTime { get; set; }

        [Column("ENTER_GATE")]
        public int? EnterGate { get; set; }

        [Column("EXIT_GATE")]
        public int? ExitGate { get; set; }

        [Column("PARKING_MINUTE")]
        public int? ParkingMinute { get; set; }

        [Column("STATE")]
        public int? State { get; set; }

        [Column("LICENSE")]
        public string? License { get; set; }

        [Column("PARKING_FEE")]
        public double? ParkingFee { get; set; }

        [Column("SHIFT_IN")]
        public int? ShiftIn { get; set; }

        [Column("SHIFT_OUT")]
        public int? ShiftOut { get; set; }
    }
}
