using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.DTOs.Pagination;
using RegistrationPortal.Server.DTOs;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace RegistrationPortal.Server.Repositories
{
    public class CustMastRepository : Repository<CustMast>, ICustMastRepository
    {
        private readonly IConfiguration _configuration;

        public CustMastRepository(RegistrationPortalDbContext context, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
        }

        public async Task<PaginatedResultDto<CustMastDto>> GetAllCustomersPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? sortBy = null,
            bool sortDescending = false,
            string? status = null,
            string? reviewStatus = null,
            string? userRole = null,
            string? userBranch = null)
        {
            using var connection = CreateNewConnection();
            await connection.OpenAsync();

            #region Base SQL (Oracle 11g SAFE)

            var baseSql = @"SELECT
            c.*,
            a.""id""              AS acc_id,
            a.""branch_c_code""   AS acc_branch_c_code,
            a.""act_c_type"",
            a.""cust_i_no""       AS acc_cust_i_no,
            a.""currency_c_code"",
            a.""act_c_name"",
            cd.""id""             AS doc_id,
            cd.""customer_id""    AS doc_customer_id,
            cd.""document_type"",
            cd.""file_url"",
            cd.""original_file_name"",
            cd.""file_size"",
            cd.""mime_type"",
            cd.""created_at""     AS doc_created_at,
            cd.""updated_at""     AS doc_updated_at
        FROM SSDBONLINE.""cust_mast"" c
        LEFT JOIN SSDBONLINE.""account_mast"" a
            ON a.""branch_c_code"" = c.""branch_c_code""
           AND a.""cust_i_no""     = c.""cust_i_id""
        LEFT JOIN SSDBONLINE.""customer_documents"" cd
            ON cd.""customer_id"" = c.""id""
        ";

            #endregion

            #region WHERE (Search, Status, ReviewStatus filtering)

            var whereConditions = new List<string>();
            
            // Add branch filtering for non-admin users
            if (!string.IsNullOrWhiteSpace(userBranch) && userRole != "Admin")
            {
                whereConditions.Add("c.\"branch_c_code\" = :userBranch");
            }
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereConditions.Add(@"
        UPPER(c.""cust_c_name"") LIKE UPPER(:searchTerm)
           OR UPPER(c.""cust_c_fname"") LIKE UPPER(:searchTerm)
           OR UPPER(c.""branch_c_code"") LIKE UPPER(:searchTerm)
           OR TO_CHAR(c.""cust_i_id"") LIKE :searchTerm");
            }
            
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Contains(","))
                {
                    var statusValues = status.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => v.Trim())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList();
                    
                    if (statusValues.Any())
                    {
                        var statusParams = statusValues.Select((value, index) => $":status{index}").ToList();
                        whereConditions.Add($"c.\"status\" IN ({string.Join(", ", statusParams)})");
                    }
                }
                else
                {
                    whereConditions.Add("c.\"status\" = :status");
                }
            }
            
            if (!string.IsNullOrWhiteSpace(reviewStatus))
            {
                if (reviewStatus.Contains(","))
                {
                    var reviewStatusValues = reviewStatus.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => v.Trim())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList();
                    
                    if (reviewStatusValues.Any())
                    {
                        var reviewStatusParams = reviewStatusValues.Select((value, index) => $":reviewStatus{index}").ToList();
                        whereConditions.Add($"c.\"review_status\" IN ({string.Join(", ", reviewStatusParams)})");
                    }
                }
                else
                {
                    whereConditions.Add("c.\"review_status\" = :reviewStatus");
                }
            }
            
            if (whereConditions.Any())
            {
                baseSql += " WHERE " + string.Join(" AND ", whereConditions);
            }

            #endregion

            #region ORDER BY

            var orderBy = "c.\"cust_c_name\"";

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                orderBy = sortBy.ToLower() switch
                {
                    "name" => "c.\"cust_c_name\"",
                    "branch" => "c.\"branch_c_code\"",
                    "id" => "c.\"cust_i_id\"",
                    _ => "c.\"cust_c_name\""
                };
            }

            baseSql += $" ORDER BY {orderBy} {(sortDescending ? "DESC" : "ASC")}";

            #endregion

            #region Pagination (ROWNUM – Oracle 11g)

            var offset = (pageNumber - 1) * pageSize;
            var maxRow = offset + pageSize;

            var pagedSql = $@"
        SELECT * FROM (
            SELECT x.*, ROWNUM rn FROM (
                {baseSql}
            ) x WHERE ROWNUM <= {maxRow}
        ) WHERE rn > {offset}
    ";

            #endregion

            using var command = connection.CreateCommand();
            command.CommandText = pagedSql;

            // Add userBranch parameter if branch filtering is enabled
            if (!string.IsNullOrWhiteSpace(userBranch) && userRole != "Admin")
            {
                command.Parameters.Add(new OracleParameter("userBranch", OracleDbType.Varchar2)
                {
                    Value = userBranch,
                    Direction = ParameterDirection.Input
                });
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                command.Parameters.Add(new OracleParameter("searchTerm", OracleDbType.Varchar2)
                {
                    Value = $"%{searchTerm}%",
                    Direction = ParameterDirection.Input
                });
            }
            
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Contains(","))
                {
                    var statusValues = status.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => v.Trim())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList();
                    
                    for (int i = 0; i < statusValues.Count; i++)
                    {
                        command.Parameters.Add(new OracleParameter($"status{i}", OracleDbType.Varchar2)
                        {
                            Value = statusValues[i],
                            Direction = ParameterDirection.Input
                        });
                    }
                }
                else
                {
                    command.Parameters.Add(new OracleParameter("status", OracleDbType.Varchar2)
                    {
                        Value = status,
                        Direction = ParameterDirection.Input
                    });
                }
            }
            
            // Only add reviewStatus parameter if it's actually used in the SQL query
            if (!string.IsNullOrWhiteSpace(reviewStatus))
            {
                if (reviewStatus.Contains(","))
                {
                    var reviewStatusValues = reviewStatus.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => v.Trim())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList();
                    
                    for (int i = 0; i < reviewStatusValues.Count; i++)
                    {
                        command.Parameters.Add(new OracleParameter($"reviewStatus{i}", OracleDbType.Varchar2)
                        {
                            Value = reviewStatusValues[i],
                            Direction = ParameterDirection.Input
                        });
                    }
                }
                else
                {
                    command.Parameters.Add(new OracleParameter("reviewStatus", OracleDbType.Varchar2)
                    {
                        Value = reviewStatus,
                        Direction = ParameterDirection.Input
                    });
                }
            }

            #region Execute & Map

            var customerMap = new Dictionary<string, CustMastDto>();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var customerId = reader.GetString(reader.GetOrdinal("id"));
                var key = customerId;

                if (!customerMap.TryGetValue(key, out var customer))
                {
                    customer = MapReaderToCustMastDto((OracleDataReader)reader);
                    customer.AccountMasts = new List<AccountMastDto>();
                    customer.CustomerDocuments = new List<CustomerDocumentDto>();
                    customerMap[key] = customer;
                }

                // Map AccountMast
                if (!reader.IsDBNull(reader.GetOrdinal("acc_id")))
                {
                    customer.AccountMasts.Add(new AccountMastDto
                    {
                        Id = reader.GetString(reader.GetOrdinal("acc_id")),
                        BranchCCode = reader.GetString(reader.GetOrdinal("acc_branch_c_code")),
                        ActCType = reader.GetString(reader.GetOrdinal("act_c_type")),
                        CustINo = reader.GetDecimal(reader.GetOrdinal("acc_cust_i_no")),
                        CurrencyCCode = reader.IsDBNull(reader.GetOrdinal("currency_c_code"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("currency_c_code")),
                        ActCName = reader.IsDBNull(reader.GetOrdinal("act_c_name"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("act_c_name"))
                    });
                }

                // Map CustomerDocument
                if (!reader.IsDBNull(reader.GetOrdinal("doc_id")))
                {
                    var doc = new CustomerDocumentDto();
                    doc.Id = reader.GetString(reader.GetOrdinal("doc_id"));
                    doc.CustomerId = reader.GetString(reader.GetOrdinal("doc_customer_id"));
                    doc.DocumentType = reader.GetString(reader.GetOrdinal("document_type"));
                    doc.FileUrl = reader.GetString(reader.GetOrdinal("file_url"));
                    doc.OriginalFileName = reader.IsDBNull(reader.GetOrdinal("original_file_name"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("original_file_name"));
                    doc.FileSize = reader.GetInt64(reader.GetOrdinal("file_size"));
                    doc.MimeType = reader.IsDBNull(reader.GetOrdinal("mime_type"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("mime_type"));
                    doc.CreatedAt = reader.GetDateTime(reader.GetOrdinal("doc_created_at"));
                    doc.UpdatedAt = reader.GetDateTime(reader.GetOrdinal("doc_updated_at"));
                    customer.CustomerDocuments.Add(doc);
                }
            }

            var customers = customerMap.Values.ToList();

            #endregion

            #region Total Count

            var countSql = @"
        SELECT COUNT(*) FROM (
            SELECT DISTINCT
                   c.""branch_c_code"",
                   c.""cust_i_id""
            FROM SSDBONLINE.""cust_mast"" c
    ";

            var countWhereConditions = new List<string>();
            
            // Add branch filtering for non-admin users
            if (!string.IsNullOrWhiteSpace(userBranch) && userRole != "Admin")
            {
                countWhereConditions.Add("c.\"branch_c_code\" = :userBranch");
            }
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                countWhereConditions.Add(@"
                (
                    UPPER(c.""cust_c_name"")       LIKE UPPER(:searchTerm)
                 OR UPPER(c.""cust_c_fname"")      LIKE UPPER(:searchTerm)
                 OR UPPER(c.""branch_c_code"")     LIKE UPPER(:searchTerm)
                 OR UPPER(TO_CHAR(c.""cust_i_id"")) LIKE UPPER(:searchTerm)
                )");
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Contains(","))
                {
                    var statusValues = status.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => v.Trim())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList();
                    
                    if (statusValues.Any())
                    {
                        var statusParams = statusValues.Select((value, index) => $":status{index}").ToList();
                        countWhereConditions.Add($"c.\"status\" IN ({string.Join(", ", statusParams)})");
                    }
                }
                else
                {
                    countWhereConditions.Add("c.\"status\" = :status");
                }
            }
            
            if (!string.IsNullOrWhiteSpace(reviewStatus))
            {
                if (reviewStatus.Contains(","))
                {
                    var reviewStatusValues = reviewStatus.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => v.Trim())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList();
                    
                    if (reviewStatusValues.Any())
                    {
                        var reviewStatusParams = reviewStatusValues.Select((value, index) => $":reviewStatus{index}").ToList();
                        countWhereConditions.Add($"c.\"review_status\" IN ({string.Join(", ", reviewStatusParams)})");
                    }
                }
                else
                {
                    countWhereConditions.Add("c.\"review_status\" = :reviewStatus");
                }
            }
            
            if (countWhereConditions.Any())
            {
                countSql += " WHERE " + string.Join(" AND ", countWhereConditions);
            }

            countSql += ")";

            using var countCommand = connection.CreateCommand();
            countCommand.CommandText = countSql;

            // Add userBranch parameter if branch filtering is enabled
            if (!string.IsNullOrWhiteSpace(userBranch) && userRole != "Admin")
            {
                countCommand.Parameters.Add(new OracleParameter("userBranch", OracleDbType.Varchar2)
                {
                    Value = userBranch,
                    Direction = ParameterDirection.Input
                });
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                countCommand.Parameters.Add(new OracleParameter("searchTerm", OracleDbType.Varchar2)
                {
                    Value = $"%{searchTerm}%",
                    Direction = ParameterDirection.Input
                });
            }
            
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Contains(","))
                {
                    var statusValues = status.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => v.Trim())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList();
                    
                    for (int i = 0; i < statusValues.Count; i++)
                    {
                        countCommand.Parameters.Add(new OracleParameter($"status{i}", OracleDbType.Varchar2)
                        {
                            Value = statusValues[i],
                            Direction = ParameterDirection.Input
                        });
                    }
                }
                else
                {
                    countCommand.Parameters.Add(new OracleParameter("status", OracleDbType.Varchar2)
                    {
                        Value = status,
                        Direction = ParameterDirection.Input
                    });
                }
            }
            
            // Only add reviewStatus parameter if it's actually used in the SQL query
            if (!string.IsNullOrWhiteSpace(reviewStatus))
            {
                if (reviewStatus.Contains(","))
                {
                    var reviewStatusValues = reviewStatus.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => v.Trim())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList();
                    
                    for (int i = 0; i < reviewStatusValues.Count; i++)
                    {
                        countCommand.Parameters.Add(new OracleParameter($"reviewStatus{i}", OracleDbType.Varchar2)
                        {
                            Value = reviewStatusValues[i],
                            Direction = ParameterDirection.Input
                        });
                    }
                }
                else
                {
                    countCommand.Parameters.Add(new OracleParameter("reviewStatus", OracleDbType.Varchar2)
                    {
                        Value = reviewStatus,
                        Direction = ParameterDirection.Input
                    });
                }
            }

            var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

            #endregion

            return new PaginatedResultDto<CustMastDto>
            {
                Data = customers,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }




        private OracleConnection CreateNewConnection()
        {
            var connectionString = _configuration.GetConnectionString("OracleConnection");
            return new OracleConnection(connectionString);
        }

        private CustMastDto MapReaderToCustMastDto(OracleDataReader reader)
        {
            var customer = new CustMastDto();

            // Map all CustMast fields
            customer.Id = reader.GetString(reader.GetOrdinal("id"));
            customer.BranchCCode = reader.GetString(reader.GetOrdinal("branch_c_code"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_i_id")))
                customer.CustIId = reader.GetDecimal(reader.GetOrdinal("cust_i_id"));
            
            customer.CustCName = reader.GetString(reader.GetOrdinal("cust_c_name"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_d_entrydt")))
                customer.CustDEntrydt = reader.GetDateTime(reader.GetOrdinal("cust_d_entrydt"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_fname")))
                customer.CustCFname = reader.GetString(reader.GetOrdinal("cust_c_fname"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_sname")))
                customer.CustCSname = reader.GetString(reader.GetOrdinal("cust_c_sname"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_tname")))
                customer.CustCTname = reader.GetString(reader.GetOrdinal("cust_c_tname"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_foname")))
                customer.CustCFoname = reader.GetString(reader.GetOrdinal("cust_c_foname"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_mname")))
                customer.CustCMname = reader.GetString(reader.GetOrdinal("cust_c_mname"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_d_bdate")))
                customer.CustDBdate = reader.GetDateTime(reader.GetOrdinal("cust_d_bdate"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_sex")))
                customer.CustCSex = reader.GetString(reader.GetOrdinal("cust_c_sex"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_religion")))
                customer.CustCReligion = reader.GetString(reader.GetOrdinal("cust_c_religion"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_caste")))
                customer.CustCCaste = reader.GetString(reader.GetOrdinal("cust_c_caste"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_maritalsts")))
                customer.CustCMaritalsts = reader.GetString(reader.GetOrdinal("cust_c_maritalsts"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_padd1")))
                customer.CustCPadd1 = reader.GetString(reader.GetOrdinal("cust_c_padd1"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_pcity")))
                customer.CustCPCity = reader.GetString(reader.GetOrdinal("cust_c_pcity"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("mobile_c_no")))
                customer.MobileCNo = reader.GetString(reader.GetOrdinal("mobile_c_no"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("email_c_add")))
                customer.EmailCAdd = reader.GetString(reader.GetOrdinal("email_c_add"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("id_c_type")))
                customer.IdCType = reader.GetString(reader.GetOrdinal("id_c_type"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("id_c_no")))
                customer.IdCNo = reader.GetString(reader.GetOrdinal("id_c_no"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("id_d_issdate")))
                customer.IdDIssdate = reader.GetDateTime(reader.GetOrdinal("id_d_issdate"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("id_c_issplace")))
                customer.IdCIssplace = reader.GetString(reader.GetOrdinal("id_c_issplace"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("id_d_expdate")))
                customer.IdDExpdate = reader.GetDateTime(reader.GetOrdinal("id_d_expdate"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_authority")))
                customer.CustCAuthority = reader.GetString(reader.GetOrdinal("cust_c_authority"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("husb_c_name")))
                customer.HusbCName = reader.GetString(reader.GetOrdinal("husb_c_name"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("country_c_code")))
                customer.CountryCCode = reader.GetString(reader.GetOrdinal("country_c_code"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("place_c_birth")))
                customer.PlaceCBirth = reader.GetString(reader.GetOrdinal("place_c_birth"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_nationality")))
                customer.CustCNationality = reader.GetString(reader.GetOrdinal("cust_c_nationality"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_wife1")))
                customer.CustCWife1 = reader.GetString(reader.GetOrdinal("cust_c_wife1"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("id_c_type2")))
                customer.IdCType2 = reader.GetString(reader.GetOrdinal("id_c_type2"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("id_c_no2")))
                customer.IdCNo2 = reader.GetString(reader.GetOrdinal("id_c_no2"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_occupation")))
                customer.CustCOccupation = reader.GetString(reader.GetOrdinal("cust_c_occupation"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("home_i_number")))
                customer.HomeINumber = reader.GetDecimal(reader.GetOrdinal("home_i_number"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_i_identify")))
                customer.CustIIdentify = reader.GetString(reader.GetOrdinal("cust_i_identify"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_countrybrith")))
                customer.CustCCountrybrith = reader.GetString(reader.GetOrdinal("cust_c_countrybrith"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_statebrith")))
                customer.CustCStatebrith = reader.GetString(reader.GetOrdinal("cust_c_statebrith"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_citizenship")))
                customer.CustCCitizenship = reader.GetString(reader.GetOrdinal("cust_c_citizenship"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_employer")))
                customer.CustCEmployer = reader.GetString(reader.GetOrdinal("cust_c_employer"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_f_income")))
                customer.CustFIncome = reader.GetDecimal(reader.GetOrdinal("cust_f_income"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_higheducation")))
                customer.CustCHigheducation = reader.GetString(reader.GetOrdinal("cust_c_higheducation"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_f_avgmonth")))
                customer.CustFAvgmonth = reader.GetDecimal(reader.GetOrdinal("cust_f_avgmonth"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("trade_c_nameenglish")))
                customer.TradeCNameenglish = reader.GetString(reader.GetOrdinal("trade_c_nameenglish"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_engfname")))
                customer.CustCEngfname = reader.GetString(reader.GetOrdinal("cust_c_engfname"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_engsname")))
                customer.CustCEngsname = reader.GetString(reader.GetOrdinal("cust_c_engsname"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_engtname")))
                customer.CustCEngtname = reader.GetString(reader.GetOrdinal("cust_c_engtname"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("cust_c_engfoname")))
                customer.CustCEngfoname = reader.GetString(reader.GetOrdinal("cust_c_engfoname"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("status")))
                customer.Status = reader.GetString(reader.GetOrdinal("status"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("review_status")))
                customer.ReviewStatus = reader.GetString(reader.GetOrdinal("review_status"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("reviewed_by")))
                customer.ReviewedBy = reader.GetString(reader.GetOrdinal("reviewed_by"));

            return customer;
        }

        public async Task<CustMast?> GetCustomerByIdAsync(string id)
        {
            using var connection = CreateNewConnection();
            await connection.OpenAsync();

            #region Base SQL (Oracle 11g SAFE)

            var baseSql = @"SELECT
            c.*,
            a.""id""              AS acc_id,
            a.""branch_c_code""   AS acc_branch_c_code,
            a.""act_c_type"",
            a.""cust_i_no""       AS acc_cust_i_no,
            a.""currency_c_code"",
            a.""act_c_name"",
            cd.""id""             AS doc_id,
            cd.""customer_id""    AS doc_customer_id,
            cd.""document_type"",
            cd.""file_url"",
            cd.""original_file_name"",
            cd.""file_size"",
            cd.""mime_type"",
            cd.""created_at""     AS doc_created_at,
            cd.""updated_at""     AS doc_updated_at
        FROM SSDBONLINE.""cust_mast"" c
        LEFT JOIN SSDBONLINE.""account_mast"" a
            ON a.""branch_c_code"" = c.""branch_c_code""
           AND a.""cust_i_no""     = c.""cust_i_id""
        LEFT JOIN SSDBONLINE.""customer_documents"" cd
            ON cd.""customer_id"" = c.""id""
        WHERE c.""id"" = :id
        ";

            #endregion

            using var command = connection.CreateCommand();
            command.CommandText = baseSql;

            var idParam = new OracleParameter("id", OracleDbType.Varchar2, id, ParameterDirection.Input);
            command.Parameters.Add(idParam);

            using var reader = await command.ExecuteReaderAsync();
            
            CustMast? customer = null;
            var accounts = new List<AccountMast>();
            var documents = new List<CustomerDocument>();

            while (await reader.ReadAsync())
            {
                if (customer == null)
                {
                    customer = MapReaderToCustMast(reader);
                }

                // Map account if present
                if (!reader.IsDBNull(reader.GetOrdinal("acc_id")))
                {
                    var account = new AccountMast
                    {
                        Id = reader.GetString(reader.GetOrdinal("acc_id")),
                        BranchCCode = reader.GetString(reader.GetOrdinal("acc_branch_c_code")),
                        ActCType = reader.IsDBNull(reader.GetOrdinal("act_c_type")) ? null : reader.GetString(reader.GetOrdinal("act_c_type")),
                        CustINo = reader.IsDBNull(reader.GetOrdinal("acc_cust_i_no")) ? 0 : reader.GetDecimal(reader.GetOrdinal("acc_cust_i_no")),
                        CurrencyCCode = reader.IsDBNull(reader.GetOrdinal("currency_c_code")) ? null : reader.GetString(reader.GetOrdinal("currency_c_code")),
                        ActCName = reader.IsDBNull(reader.GetOrdinal("act_c_name")) ? null : reader.GetString(reader.GetOrdinal("act_c_name"))
                    };
                    accounts.Add(account);
                }

                // Map document if present
                if (!reader.IsDBNull(reader.GetOrdinal("doc_id")))
                {
                    var document = new CustomerDocument
                    {
                        Id = reader.GetString(reader.GetOrdinal("doc_id")),
                        CustomerId = reader.GetString(reader.GetOrdinal("doc_customer_id")),
                        DocumentType = reader.GetString(reader.GetOrdinal("document_type")),
                        FileUrl = reader.GetString(reader.GetOrdinal("file_url")),
                        OriginalFileName = reader.IsDBNull(reader.GetOrdinal("original_file_name")) ? null : reader.GetString(reader.GetOrdinal("original_file_name")),
                        FileSize = reader.GetInt64(reader.GetOrdinal("file_size")),
                        MimeType = reader.IsDBNull(reader.GetOrdinal("mime_type")) ? null : reader.GetString(reader.GetOrdinal("mime_type")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("doc_created_at")),
                        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("doc_updated_at"))
                    };
                    documents.Add(document);
                }
            }

            if (customer != null)
            {
                customer.AccountMasts = accounts;
                // Note: CustomerDocuments are not part of CustMast entity, they're handled separately
                return customer;
            }

            return null;
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
                CustCEngfoname = reader["cust_c_engfoname"]?.ToString(),
                Status = reader["status"]?.ToString(),
                ReviewStatus = reader["review_status"]?.ToString(),
                ReviewedBy = reader["reviewed_by"]?.ToString()
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
