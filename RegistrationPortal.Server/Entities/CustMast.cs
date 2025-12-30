using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RegistrationPortal.Server.Entities
{
    [Table("cust_mast", Schema = "SSDBONLINE")]
    public class CustMast
    {
        // New string primary key
        [Key]
        [Column("id", TypeName = "VARCHAR2(50)")]
        [Required]
        [StringLength(50)]
        public string Id { get; set; } = string.Empty;

        [Column("branch_c_code", TypeName = "VARCHAR2(10)")]
        [Required]
        [StringLength(10)]
        public string BranchCCode { get; set; } = string.Empty;

        [Column("cust_i_id", TypeName = "NUMBER(18,0)")]
        public decimal CustIId { get; set; }

        [Column("cust_c_name", TypeName = "VARCHAR2(250)")]
        [Required]
        [StringLength(250)]
        public string CustCName { get; set; } = string.Empty;

        [Column("cust_d_entrydt", TypeName = "DATE")]
        public DateTime CustDEntrydt { get; set; }

        [Column("cust_c_fname", TypeName = "VARCHAR2(250)")]
        [StringLength(250)]
        public string? CustCFname { get; set; }

        [Column("cust_c_sname", TypeName = "VARCHAR2(250)")]
        [StringLength(250)]
        public string? CustCSname { get; set; }

        [Column("cust_c_tname", TypeName = "VARCHAR2(250)")]
        [StringLength(250)]
        public string? CustCTname { get; set; }

        [Column("cust_c_foname", TypeName = "VARCHAR2(250)")]
        [StringLength(250)]
        public string? CustCFoname { get; set; }

        [Column("cust_c_mname", TypeName = "VARCHAR2(250)")]
        [StringLength(250)]
        public string? CustCMname { get; set; }

        [Column("cust_d_bdate", TypeName = "DATE")]
        public DateTime? CustDBdate { get; set; }

        [Column("cust_c_sex", TypeName = "VARCHAR2(10)")]
        [StringLength(10)]
        public string? CustCSex { get; set; }

        [Column("cust_c_religion", TypeName = "VARCHAR2(20)")]
        [StringLength(20)]
        public string? CustCReligion { get; set; }

        [Column("cust_c_caste", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? CustCCaste { get; set; }

        [Column("cust_c_maritalsts", TypeName = "VARCHAR2(20)")]
        [StringLength(20)]
        public string? CustCMaritalsts { get; set; }

        [Column("cust_c_padd1", TypeName = "VARCHAR2(100)")]
        [StringLength(100)]
        public string? CustCPadd1 { get; set; }

        [Column("cust_c_pcity", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? CustCPCity { get; set; }

        [Column("mobile_c_no", TypeName = "VARCHAR2(18)")]
        [StringLength(18)]
        public string? MobileCNo { get; set; }

        [Column("email_c_add", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? EmailCAdd { get; set; }

        [Column("id_c_type", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? IdCType { get; set; }

        [Column("id_c_no", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? IdCNo { get; set; }

        [Column("id_d_issdate", TypeName = "DATE")]
        public DateTime? IdDIssdate { get; set; }

        [Column("id_c_issplace", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? IdCIssplace { get; set; }

        [Column("id_d_expdate", TypeName = "DATE")]
        public DateTime? IdDExpdate { get; set; }

        [Column("cust_c_authority", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? CustCAuthority { get; set; }

        [Column("husb_c_name", TypeName = "VARCHAR2(100)")]
        [StringLength(100)]
        public string? HusbCName { get; set; }

        [Column("country_c_code", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? CountryCCode { get; set; }

        [Column("place_c_birth", TypeName = "VARCHAR2(100)")]
        [StringLength(100)]
        public string? PlaceCBirth { get; set; }

        [Column("cust_c_nationality", TypeName = "VARCHAR2(100)")]
        [StringLength(100)]
        public string? CustCNationality { get; set; }

        [Column("cust_c_wife1", TypeName = "VARCHAR2(250)")]
        [StringLength(250)]
        public string? CustCWife1 { get; set; }

        [Column("id_c_type2", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? IdCType2 { get; set; }

        [Column("id_c_no2", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? IdCNo2 { get; set; }

        [Column("cust_c_occupation", TypeName = "VARCHAR2(100)")]
        [StringLength(100)]
        public string? CustCOccupation { get; set; }

        [Column("home_i_number", TypeName = "NUMBER(18,0)")]
        public decimal? HomeINumber { get; set; }

        [Column("cust_i_identify", TypeName = "VARCHAR2(20)")]
        [StringLength(20)]
        public string? CustIIdentify { get; set; }

        [Column("cust_c_countrybrith", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? CustCCountrybrith { get; set; }

        [Column("cust_c_statebrith", TypeName = "VARCHAR2(10)")]
        [StringLength(10)]
        public string? CustCStatebrith { get; set; }

        [Column("cust_c_citizenship", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? CustCCitizenship { get; set; }

        [Column("cust_c_employer", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? CustCEmployer { get; set; }

        [Column("cust_f_income", TypeName = "NUMBER(18,2)")]
        public decimal? CustFIncome { get; set; }

        [Column("cust_c_higheducation", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? CustCHigheducation { get; set; }

        [Column("cust_f_avgmonth", TypeName = "NUMBER(18,2)")]
        public decimal? CustFAvgmonth { get; set; }

        [Column("trade_c_nameenglish", TypeName = "VARCHAR2(250)")]
        [StringLength(250)]
        public string? TradeCNameenglish { get; set; }

        [Column("cust_c_engfname", TypeName = "VARCHAR2(250)")]
        [StringLength(250)]
        public string? CustCEngfname { get; set; }

        [Column("cust_c_engsname", TypeName = "VARCHAR2(250)")]
        [StringLength(250)]
        public string? CustCEngsname { get; set; }

        [Column("cust_c_engtname", TypeName = "VARCHAR2(250)")]
        [StringLength(250)]
        public string? CustCEngtname { get; set; }

        [Column("cust_c_engfoname", TypeName = "VARCHAR2(250)")] 
        [StringLength(250)]
        public string? CustCEngfoname { get; set; }

        [Column("status", TypeName = "NVARCHAR2(50)")]
        [StringLength(50)]
        public string? Status { get; set; }

        [Column("review_status", TypeName = "NVARCHAR2(50)")]
        [StringLength(50)]
        public string? ReviewStatus { get; set; }

        [Column("reviewed_by", TypeName = "NVARCHAR2(100)")]
        [StringLength(100)]
        public string? ReviewedBy { get; set; }

        // Navigation property for related accounts
        public virtual ICollection<AccountMast> AccountMasts { get; set; } = [];

        // Navigation property for FATCA information
        public virtual CustomerFatca? CustomerFatca { get; set; }
    }
}
