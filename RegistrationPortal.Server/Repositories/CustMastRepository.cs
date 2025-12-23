using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace RegistrationPortal.Server.Repositories
{
    public class CustMastRepository : Repository<CustMast>, ICustMastRepository
    {
        private readonly IConfiguration _configuration;

        public CustMastRepository(RegistrationPortalDbContext context, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
        }

        private OracleConnection CreateNewConnection()
        {
            var connectionString = _configuration.GetConnectionString("OracleConnection");
            return new OracleConnection(connectionString);
        }

        public async Task<CustMast?> GetByBranchAndIdAsync(string branchCode, decimal custId)
        {
            using var connection = CreateNewConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM SSDBONLINE.""cust_mast"" 
                WHERE ""branch_c_code"" = :branchCode 
                AND ""cust_i_id"" = :custId";
            
            var branchParam = new OracleParameter("branchCode", OracleDbType.Varchar2, branchCode, ParameterDirection.Input);
            var idParam = new OracleParameter("custId", OracleDbType.Decimal, custId, ParameterDirection.Input);
            
            command.Parameters.Add(branchParam);
            command.Parameters.Add(idParam);
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var customer = MapReaderToCustMast(reader);
                
                // Load accounts separately for this customer
                customer.AccountMasts = await GetCustomerAccountsForCustomer(connection, customer.BranchCCode, customer.CustIId);
                
                return customer;
            }
            
            return null;
        }

        public async Task<IEnumerable<CustMast>> GetByBranchAsync(string branchCode)
        {
            using var connection = CreateNewConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM SSDBONLINE.""cust_mast"" 
                WHERE ""branch_c_code"" = :branchCode";
            
            var branchParam = new OracleParameter("branchCode", OracleDbType.Varchar2, branchCode, ParameterDirection.Input);
            command.Parameters.Add(branchParam);
            
            var customers = new List<CustMast>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                customers.Add(MapReaderToCustMast(reader));
            }
            
            return customers;
        }

        public async Task<IEnumerable<AccountMast>> GetCustomerAccountsAsync(string branchCode, decimal custId)
        {
            using var connection = CreateNewConnection();
            await connection.OpenAsync();
            
            return await GetCustomerAccountsForCustomer(connection, branchCode, custId);
        }

        public async Task<decimal> GetNextCustomerIdAsync(string branchCode)
        {
            using var connection = CreateNewConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT MAX(""cust_i_id"") FROM SSDBONLINE.""cust_mast"" 
                WHERE ""branch_c_code"" = :branchCode";
            
            var branchParam = new OracleParameter("branchCode", OracleDbType.Varchar2, branchCode, ParameterDirection.Input);
            command.Parameters.Add(branchParam);
            
            var result = await command.ExecuteScalarAsync();
            var maxId = result != DBNull.Value ? Convert.ToDecimal(result) : 5999;
            
            return Math.Max(6000, maxId + 1); // Ensure minimum ID is 6000
        }

        public async Task<CustMast?> GetByIdNumberAndBranchAsync(string idNumber, string branchCode)
        {
            using var connection = CreateNewConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM SSDBONLINE.""cust_mast"" 
                WHERE ""branch_c_code"" = :branchCode 
                AND ""id_c_no"" = :idNumber";
            
            var branchParam = new OracleParameter("branchCode", OracleDbType.Varchar2, branchCode, ParameterDirection.Input);
            var idParam = new OracleParameter("idNumber", OracleDbType.Varchar2, idNumber, ParameterDirection.Input);
            
            command.Parameters.Add(branchParam);
            command.Parameters.Add(idParam);
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToCustMast(reader);
            }
            
            return null;
        }

        private CustMast MapReaderToCustMast(System.Data.Common.DbDataReader reader)
        {
            var customer = new CustMast
            {
                Id = reader["id"]?.ToString() ?? string.Empty,
                BranchCCode = reader["branch_c_code"]?.ToString() ?? string.Empty,
                CustIId = Convert.ToDecimal(reader["cust_i_id"]),
                CustCName = reader["cust_c_name"]?.ToString() ?? string.Empty,
                CustDEntrydt = reader["cust_d_entrydt"] != DBNull.Value ? Convert.ToDateTime(reader["cust_d_entrydt"]) : DateTime.MinValue,
                CustCFname = reader["cust_c_fname"]?.ToString(),
                CustCSname = reader["cust_c_sname"]?.ToString(),
                CustCTname = reader["cust_c_tname"]?.ToString(),
                CustCFoname = reader["cust_c_foname"]?.ToString(),
                CustCMname = reader["cust_c_mname"]?.ToString(),
                CustDBdate = reader["cust_d_bdate"] != DBNull.Value ? Convert.ToDateTime(reader["cust_d_bdate"]) : null,
                CustCSex = reader["cust_c_sex"]?.ToString(),
                CustCReligion = reader["cust_c_religion"]?.ToString(),
                CustCCaste = reader["cust_c_caste"]?.ToString(),
                CustCMaritalsts = reader["cust_c_maritalsts"]?.ToString(),
                CustCPadd1 = reader["cust_c_padd1"]?.ToString(),
                CustCPCity = reader["cust_c_pcity"]?.ToString(),
                MobileCNo = reader["mobile_c_no"]?.ToString(),
                EmailCAdd = reader["email_c_add"]?.ToString(),
                IdCType = reader["id_c_type"]?.ToString(),
                IdCNo = reader["id_c_no"]?.ToString(),
                IdDIssdate = reader["id_d_issdate"] != DBNull.Value ? Convert.ToDateTime(reader["id_d_issdate"]) : null,
                IdCIssplace = reader["id_c_issplace"]?.ToString(),
                IdDExpdate = reader["id_d_expdate"] != DBNull.Value ? Convert.ToDateTime(reader["id_d_expdate"]) : null,
                CustCAuthority = reader["cust_c_authority"]?.ToString(),
                HusbCName = reader["husb_c_name"]?.ToString(),
                CountryCCode = reader["country_c_code"]?.ToString(),
                PlaceCBirth = reader["place_c_birth"]?.ToString(),
                CustCNationality = reader["cust_c_nationality"]?.ToString(),
                CustCWife1 = reader["cust_c_wife1"]?.ToString(),
                IdCType2 = reader["id_c_type2"]?.ToString(),
                IdCNo2 = reader["id_c_no2"]?.ToString(),
                CustCOccupation = reader["cust_c_occupation"]?.ToString(),
                HomeINumber = reader["home_i_number"] != DBNull.Value ? Convert.ToDecimal(reader["home_i_number"]) : null,
                CustIIdentify = reader["cust_i_identify"]?.ToString(),
                CustCCountrybrith = reader["cust_c_countrybrith"]?.ToString(),
                CustCStatebrith = reader["cust_c_statebrith"]?.ToString(),
                CustCCitizenship = reader["cust_c_citizenship"]?.ToString(),
                CustCEmployer = reader["cust_c_employer"]?.ToString(),
                CustFIncome = reader["cust_f_income"] != DBNull.Value ? Convert.ToDecimal(reader["cust_f_income"]) : null,
                CustCHigheducation = reader["cust_c_higheducation"]?.ToString(),
                CustFAvgmonth = reader["cust_f_avgmonth"] != DBNull.Value ? Convert.ToDecimal(reader["cust_f_avgmonth"]) : null,
                TradeCNameenglish = reader["trade_c_nameenglish"]?.ToString(),
                CustCEngfname = reader["cust_c_engfname"]?.ToString(),
                CustCEngsname = reader["cust_c_engsname"]?.ToString(),
                CustCEngtname = reader["cust_c_engtname"]?.ToString(),
                CustCEngfoname = reader["cust_c_engfoname"]?.ToString()
            };
            
            return customer;
        }

        private async Task<List<AccountMast>> GetCustomerAccountsForCustomer(System.Data.Common.DbConnection connection, string branchCode, decimal custId)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM SSDBONLINE.""account_mast"" 
                WHERE ""branch_c_code"" = :branchCode 
                AND ""cust_i_no"" = :custId";
            
            var branchParam = new OracleParameter("branchCode", OracleDbType.Varchar2, branchCode, ParameterDirection.Input);
            var idParam = new OracleParameter("custId", OracleDbType.Decimal, custId, ParameterDirection.Input);
            
            command.Parameters.Add(branchParam);
            command.Parameters.Add(idParam);
            
            var accounts = new List<AccountMast>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                accounts.Add(MapReaderToAccountMast(reader));
            }
            
            return accounts;
        }

        private AccountMast MapReaderToAccountMast(System.Data.Common.DbDataReader reader)
        {
            var account = new AccountMast
            {
                Id = reader["id"]?.ToString() ?? string.Empty,
                BranchCCode = reader["branch_c_code"]?.ToString() ?? string.Empty,
                CustINo = Convert.ToDecimal(reader["cust_i_no"]),
                ActCType = reader["act_c_type"]?.ToString() ?? string.Empty,
                CurrencyCCode = reader["currency_c_code"]?.ToString(),
                ActCName = reader["act_c_name"]?.ToString() ?? string.Empty,
                ActCOcccode = reader["act_c_occcode"]?.ToString() ?? string.Empty,
                ActDOpdate = reader["act_d_opdate"] != DBNull.Value ? Convert.ToDateTime(reader["act_d_opdate"]) : default,
                ActCOrgn = reader["act_c_orgn"]?.ToString() ?? string.Empty,
                ActCActsts = reader["act_c_actsts"]?.ToString() ?? string.Empty,
                ActCOdsts = reader["act_c_odsts"]?.ToString() ?? string.Empty,
                ActCChqfac = reader["act_c_chqfac"]?.ToString() ?? string.Empty,
                ActCIntrotype = reader["act_c_introtype"]?.ToString() ?? string.Empty,
                ActCOpmode = reader["act_c_opmode"]?.ToString() ?? string.Empty,
                ActIIntroid = reader["act_i_introid"] != DBNull.Value ? Convert.ToDecimal(reader["act_i_introid"]) : null,
                ActITrbrcode = reader["act_i_trbrcode"] != DBNull.Value ? Convert.ToDecimal(reader["act_i_trbrcode"]) : null,
                ActDClosdt = reader["act_d_closdt"] != DBNull.Value ? Convert.ToDateTime(reader["act_d_closdt"]) : null,
                ActCAbbsts = reader["act_c_abbsts"]?.ToString(),
                WithdrawCFlag = reader["withdraw_c_flag"]?.ToString(),
                IntroCRem = reader["intro_c_rem"]?.ToString(),
                ActCAtm = reader["act_c_atm"]?.ToString() ?? string.Empty,
                ActCInternet = reader["act_c_internet"]?.ToString() ?? string.Empty,
                ActCTelebnk = reader["act_c_telebnk"]?.ToString() ?? string.Empty
            };
            
            return account;
        }

        public async Task<CustMast?> GetByPhoneNumberAndBranchAsync(string phoneNumber, string branchCode)
        {
            using var connection = CreateNewConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM SSDBONLINE.""cust_mast"" 
                WHERE ""branch_c_code"" = :branchCode 
                AND ""mobile_c_no"" = :phoneNumber";
            
            var branchParam = new OracleParameter("branchCode", OracleDbType.Varchar2, branchCode, ParameterDirection.Input);
            var phoneParam = new OracleParameter("phoneNumber", OracleDbType.Varchar2, phoneNumber, ParameterDirection.Input);
            
            command.Parameters.Add(branchParam);
            command.Parameters.Add(phoneParam);
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToCustMast(reader);
            }
            
            return null;
        }

        public async Task<bool> HasAccountTypeInBranchAsync(decimal customerId, string accountType, string branchCode)
        {
            using var connection = CreateNewConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) FROM SSDBONLINE.""account_mast"" 
                WHERE ""cust_i_no"" = :customerId 
                AND ""act_c_type"" = :accountType 
                AND ""branch_c_code"" = :branchCode";
            
            var customerParam = new OracleParameter("customerId", OracleDbType.Decimal, customerId, ParameterDirection.Input);
            var typeParam = new OracleParameter("accountType", OracleDbType.Varchar2, accountType, ParameterDirection.Input);
            var branchParam = new OracleParameter("branchCode", OracleDbType.Varchar2, branchCode, ParameterDirection.Input);
            
            command.Parameters.Add(customerParam);
            command.Parameters.Add(typeParam);
            command.Parameters.Add(branchParam);
            
            var result = await command.ExecuteScalarAsync();
            var count = Convert.ToInt32(result);
            
            return count > 0;
        }

        public async Task AddFatcaAsync(CustomerFatca fatca)
        {
            await _context.CustomerFATCA.AddAsync(fatca);
        }
    }
}
