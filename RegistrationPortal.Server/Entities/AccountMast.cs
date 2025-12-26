using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrationPortal.Server.Entities
{
    [Table("account_mast", Schema = "SSDBONLINE")]
    public class AccountMast
    {
        // New string primary key
        [Key]
        [Column("id")]
        [Required]
        [StringLength(50)]
        public string Id { get; set; } = string.Empty;

        [Column("branch_c_code")]
        [Required]
        [StringLength(10)]
        public string BranchCCode { get; set; } = string.Empty;

        [Column("act_c_type")]
        [Required]
        [StringLength(10)]
        public string ActCType { get; set; } = string.Empty;

        [Column("cust_i_no", TypeName = "DECIMAL(18,2)")]
        public decimal CustINo { get; set; }

        [Column("currency_c_code")]
        [StringLength(10)]
        public string? CurrencyCCode { get; set; }

        // Foreign key property for CustMast string ID
        [Column("cust_id")]
        [Required]
        [StringLength(50)]
        public string CustId { get; set; }

        [Column("act_c_name")]
        [StringLength(250)]
        public string? ActCName { get; set; }

        [Column("act_c_occcode")]
        [StringLength(10)]
        public string? ActCOcccode { get; set; }

        [Column("act_d_opdate")]
        public DateTime? ActDOpdate { get; set; }

        [Column("act_c_orgn")]
        [StringLength(10)]
        public string? ActCOrgn { get; set; }

        [Column("act_c_actsts")]
        [StringLength(50)]
        public string? ActCActsts { get; set; }

        [Column("act_c_odsts")]
        [StringLength(50)]
        public string? ActCOdsts { get; set; }

        [Column("act_c_chqfac")]
        [StringLength(50)]
        public string? ActCChqfac { get; set; }

        [Column("act_c_introtype")]
        [StringLength(50)]
        public string? ActCIntrotype { get; set; }

        [Column("act_c_opmode")]
        [StringLength(50)]
        public string? ActCOpmode { get; set; }

        [Column("act_i_introid", TypeName = "DECIMAL(18,2)")]
        public decimal? ActIIntroid { get; set; }

        [Column("act_i_trbrcode", TypeName = "DECIMAL(18,2)")]
        public decimal? ActITrbrcode { get; set; }

        [Column("act_d_closdt")]
        public DateTime? ActDClosdt { get; set; }

        [Column("act_c_abbsts", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? ActCAbbsts { get; set; }

        [Column("withdraw_c_flag", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? WithdrawCFlag { get; set; }

        [Column("intro_c_rem")]
        [StringLength(100)]
        public string? IntroCRem { get; set; }

        [Column("act_c_atm", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? ActCAtm { get; set; }

        [Column("act_c_internet", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? ActCInternet { get; set; }

        [Column("act_c_telebnk", TypeName = "VARCHAR2(50)")]
        [StringLength(50)]
        public string? ActCTelebnk { get; set; }

        [Column("group_c_code")]
        [StringLength(50)]
        public string? GroupCCode { get; set; }

        [Column("entered_c_by")]
        [StringLength(50)]
        public string? EnteredCBy { get; set; }

        [Column("auth_c_by")]
        [StringLength(50)]
        public string? AuthCBy { get; set; }

        [Column("close_c_reason")]
        [StringLength(50)]
        public string? CloseCReason { get; set; }

        // Navigation property for related customer - made nullable for initial creation
        public virtual CustMast CustMast { get; set; } = default!;
    }
}
