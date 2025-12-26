using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.Repositories;
using RegistrationPortal.Server.DTOs;
using RegistrationPortal.Server.DTOs.Pagination;
using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Transactions;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace RegistrationPortal.Server.Services
{
    public class CustMastService : ICustMastService
    {
        private readonly ICustMastRepository _custMastRepository;
        private readonly IAccountMastRepository _accountMastRepository;
        private readonly ICustomerDocumentService _customerDocumentService;
        private readonly ILogger<CustMastService> _logger;
        private readonly IConfiguration _configuration;

        public CustMastService(ICustMastRepository custMastRepository, IAccountMastRepository accountMastRepository, ICustomerDocumentService customerDocumentService, ILogger<CustMastService> logger, IConfiguration configuration)
        {
            _custMastRepository = custMastRepository;
            _accountMastRepository = accountMastRepository;
            _customerDocumentService = customerDocumentService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<GetCustMastByIdDto?> GetCustomerByIdAsync(string id)
        {
            var customer = await _custMastRepository.GetCustomerByIdAsync(id);
            if (customer == null)
                return null;

            // Get customer documents
            var documents = await _customerDocumentService.GetCustomerDocumentsAsync(customer.Id);
            var documentDtos = documents.Select(d => new CustomerDocumentDto
            {
                Id = d.Id,
                CustomerId = d.CustomerId,
                DocumentType = d.DocumentType,
                FileUrl = d.FileUrl,
                OriginalFileName = d.OriginalFileName,
                FileSize = d.FileSize,
                MimeType = d.MimeType,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();

            // Get customer accounts using the correct repository method
            var accounts = await _accountMastRepository.GetByCustomerAsync(customer.BranchCCode, customer.CustIId);
            var accountDtos = accounts.Select(a => new AccountMastDto
            {
                Id = a.Id,
                BranchCCode = a.BranchCCode,
                ActCType = a.ActCType,
                CustINo = a.CustINo,
                CurrencyCCode = a.CurrencyCCode,
                ActCName = a.ActCName
            }).ToList();

            // Map customer to GetCustMastByIdDto
            var customerDto = new GetCustMastByIdDto
            {
                Id = customer.Id,
                BranchCCode = customer.BranchCCode,
                CustINo = customer.CustIId,
                CustCName = customer.CustCName,
                CustCFname = customer.CustCFname,
                CustCSname = customer.CustCSname,
                CustCTname = customer.CustCTname,
                CustCFoname = customer.CustCFoname,
                CustCMname = customer.CustCMname,
                CustDBdate = customer.CustDBdate,
                CustCSex = customer.CustCSex,
                CustCReligion = customer.CustCReligion,
                CustCCaste = customer.CustCCaste,
                CustCMaritalsts = customer.CustCMaritalsts,
                CustCPadd1 = customer.CustCPadd1,
                CustCPCity = customer.CustCPCity,
                MobileCNo = customer.MobileCNo,
                EmailCAdd = customer.EmailCAdd,
                IdCType = customer.IdCType,
                IdCNo = customer.IdCNo,
                IdDIssdate = customer.IdDIssdate,
                IdCIssplace = customer.IdCIssplace,
                IdDExpdate = customer.IdDExpdate,
                CustCAuthority = customer.CustCAuthority,
                HusbCName = customer.HusbCName,
                CountryCCode = customer.CountryCCode,
                PlaceCBirth = customer.PlaceCBirth,
                CustCNationality = customer.CustCNationality,
                CustCWife1 = customer.CustCWife1,
                IdCType2 = customer.IdCType2,
                IdCNo2 = customer.IdCNo2,
                CustCOccupation = customer.CustCOccupation,
                HomeINumber = customer.HomeINumber?.ToString(),
                CustIIdentify = customer.CustIIdentify,
                CustCCountrybrith = customer.CustCCountrybrith,
                CustCStatebrith = customer.CustCStatebrith,
                CustCCitizenship = customer.CustCCitizenship,
                CustCEmployer = customer.CustCEmployer,
                CustFIncome = customer.CustFIncome,
                CustCHigheducation = customer.CustCHigheducation,
                CustFAvgmonth = customer.CustFAvgmonth,
                TradeCNameenglish = customer.TradeCNameenglish,
                CustCEngfname = customer.CustCEngfname,
                CustCEngsname = customer.CustCEngsname,
                CustCEngtname = customer.CustCEngtname,
                CustCEngfoname = customer.CustCEngfoname,
                Status = customer.Status,
                ReviewStatus = customer.ReviewStatus,
                ReviewedBy = customer.ReviewedBy,
                CustDEntrydt = customer.CustDEntrydt,
                AccountMasts = accountDtos,
                CustomerDocuments = documentDtos
            };

            return customerDto;
        }

        public async Task<CustMast?> GetCustomerAsync(string branchCode, decimal custId)
        {
            return await _custMastRepository.GetByBranchAndIdAsync(branchCode, custId);
        }

        public async Task<PaginatedResultDto<CustMastDto>> GetAllCustomersAsync(PaginationParameters parameters, string? userRole = null, string? userBranch = null)
        {
            // Process comma-separated status values
            var statusValues = ProcessCommaSeparatedValues(parameters.Status);
            var reviewStatusValues = ProcessCommaSeparatedValues(parameters.ReviewStatus);
            
            // Apply role-based filtering logic
            var (effectiveStatus, effectiveReviewStatus) = ApplyRoleBasedFiltering(userRole, statusValues, reviewStatusValues);
            
            return await _custMastRepository.GetAllCustomersPaginatedAsync(
                parameters.PageNumber, 
                parameters.PageSize, 
                parameters.SearchTerm, 
                parameters.SortBy, 
                parameters.SortDescending,
                effectiveStatus,
                effectiveReviewStatus,
                userRole,
                userBranch);
        }

        public async Task<IEnumerable<CustMast>> GetCustomersByBranchAsync(string branchCode)
        {
            return await _custMastRepository.GetByBranchAsync(branchCode);
        }

        private bool HasFatcaData(CreateCustomerDto customerDto)
        {
            return !string.IsNullOrEmpty(customerDto.IsUsPerson) ||
                   !string.IsNullOrEmpty(customerDto.CitizenshipCountries) ||
                   !string.IsNullOrEmpty(customerDto.HasImmigrantVisa) ||
                   !string.IsNullOrEmpty(customerDto.PermanentResidencyStates) ||
                   !string.IsNullOrEmpty(customerDto.HasOtherCountriesResidency) ||
                   !string.IsNullOrEmpty(customerDto.SoleSudanResidencyConfirmed) ||
                   !string.IsNullOrEmpty(customerDto.SSN) ||
                   !string.IsNullOrEmpty(customerDto.ITIN) ||
                   !string.IsNullOrEmpty(customerDto.ATIN) ||
                   !string.IsNullOrEmpty(customerDto.Country1TaxJurisdiction) ||
                   !string.IsNullOrEmpty(customerDto.Country1TIN) ||
                   !string.IsNullOrEmpty(customerDto.Country1NoTinReason) ||
                   !string.IsNullOrEmpty(customerDto.Country1NoTinExplanation) ||
                   !string.IsNullOrEmpty(customerDto.Country2TaxJurisdiction) ||
                   !string.IsNullOrEmpty(customerDto.Country2TIN) ||
                   !string.IsNullOrEmpty(customerDto.Country2NoTinReason) ||
                   !string.IsNullOrEmpty(customerDto.Country2NoTinExplanation) ||
                   !string.IsNullOrEmpty(customerDto.Country3TaxJurisdiction) ||
                   !string.IsNullOrEmpty(customerDto.Country3TIN) ||
                   !string.IsNullOrEmpty(customerDto.Country3NoTinReason) ||
                   !string.IsNullOrEmpty(customerDto.Country3NoTinExplanation);
        }

        private bool HasDocuments(CreateCustomerDto customerDto)
        {
            return customerDto.Identification != null ||
                   customerDto.NationalId != null ||
                   customerDto.PersonalImage != null ||
                   customerDto.ImageFortheRequesterHoldingTheID != null ||
                   customerDto.SignitureTemplate != null ||
                   customerDto.HandwrittenRequest != null ||
                   customerDto.EmploymentCertificate != null;
        }

        public async Task<CustMast> CreateCustomerAsync(CreateCustomerDto customerDto)
        {
            try
            {
                // Debug: Log incoming DTO values including FATCA
                _logger.LogInformation("Creating customer with FATCA data: IsUsPerson={IsUsPerson}, CitizenshipCountries={CitizenshipCountries}, SSN={SSN}", 
                    customerDto.IsUsPerson, customerDto.CitizenshipCountries, customerDto.SSN);

                // Use transaction scope to ensure all operations are atomic
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Map DTO to entity
                    var customer = customerDto.Adapt<CustMast>();

                    // Set default review status to Pending
                    customer.ReviewStatus = "Pending";

                    // Validate for duplicates before creating
                    await ValidateCustomerDuplicatesAsync(customer);

                    // Extract accounts if present (we'll create them after customer)
                    var accountsToCreate = customerDto.AccountMasts?.Select(a => a.Adapt<AccountMast>()).ToList() ?? new List<AccountMast>();

                    // Set entry date to current date if not provided
                    if (customer.CustDEntrydt == default)
                    {
                        customer.CustDEntrydt = DateTime.UtcNow;
                    }

                    // Generate customer ID and set it
                    customer.CustIId = await _custMastRepository.GetNextCustomerIdAsync(customer.BranchCCode);
                    customer.Id = $"{customer.BranchCCode}{customer.CustIId}";

                    // Create customer first
                    await _custMastRepository.AddAsync(customer);
                    await _custMastRepository.SaveChangesAsync();

                    // Create FATCA record if FATCA data is provided and status is not "Update"
                    if (customerDto.Status != "Update" && HasFatcaData(customerDto))
                    {
                        _logger.LogInformation("Creating FATCA record for customer {CustomerId}", customer.Id);
                        
                        var fatcaData = customerDto.Adapt<CustomerFatca>();
                        fatcaData.CustomerId = customer.Id;
                        fatcaData.Id = $"{customer.Id}_FATCA_{DateTime.UtcNow:yyyyMMddHHmmss}";
                        fatcaData.CreatedAt = DateTime.UtcNow;
                        fatcaData.UpdatedAt = DateTime.UtcNow;
                        
                        await _custMastRepository.AddFatcaAsync(fatcaData);
                        await _custMastRepository.SaveChangesAsync();
                        
                        _logger.LogInformation("FATCA record created successfully for customer {CustomerId}", customer.Id);
                    }
                    else if (customerDto.Status == "Update")
                    {
                        _logger.LogInformation("Skipping FATCA validation for customer {CustomerId} due to Update status", customer.Id);
                    }
                    else
                    {
                        _logger.LogInformation("No FATCA data provided for customer {CustomerId}", customer.Id);
                    }

                    // Now create accounts separately
                    if (accountsToCreate.Any())
                    {
                        foreach (var account in accountsToCreate)
                        {
                            // Set customer reference and ID for each account
                            account.CustINo = customer.CustIId;
                            account.BranchCCode = customer.BranchCCode; // Ensure branch code matches
                            account.CustId = customer.Id; // Set string foreign key to customer's ID
                            
                            // Generate account string ID
                            account.Id = $"{account.BranchCCode}{account.ActCType}{account.CustINo}{account.CurrencyCCode}";
                            
                            // Set default values for account if not provided
                            if (account.ActDOpdate == default)
                            {
                                account.ActDOpdate = DateTime.UtcNow;
                            }
                            
                            // Create account using AccountMastRepository directly
                            await _accountMastRepository.AddAsync(account);
                        }
                        
                        // Save all accounts
                        await _accountMastRepository.SaveChangesAsync();
                        
                        // Reload customer with accounts to ensure proper navigation properties
                        customer = await _custMastRepository.GetByBranchAndIdAsync(customer.BranchCCode, customer.CustIId);
                    }

                    // Process document uploads if any documents are provided
                    if (HasDocuments(customerDto))
                    {
                        _logger.LogInformation("Processing document uploads for customer {CustomerId}", customer.Id);
                        
                        var uploadedDocuments = await _customerDocumentService.ProcessCustomerDocumentsAsync(
                            customer.Id, 
                            customerDto.BranchCCode,
                            customer.IdCNo2 ?? customer.IdCNo ?? customer.Id, 
                            customerDto);
                            
                        _logger.LogInformation("Successfully uploaded {Count} documents for customer {CustomerId}", 
                            uploadedDocuments.Count, customer.Id);
                    }

                    // Complete the transaction if all operations succeeded
                    transactionScope.Complete();
                    
                    return customer;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer - transaction rolled back");
                throw;
            }
        }

        private async Task ValidateCustomerDuplicatesAsync(CustMast customer)
        {
            var validationErrors = new List<string>();

            // Check for duplicate ID number in the same branch
            if (!string.IsNullOrEmpty(customer.IdCNo))
            {
                var existingCustomerById = await _custMastRepository.GetByIdNumberAndBranchAsync(customer.IdCNo2?? customer.IdCNo, customer.BranchCCode);
                if (existingCustomerById != null)
                {
                    validationErrors.Add($"هذا العميل موجود مسبقا");
                }
            }

            // Check for duplicate phone number in the same branch
            if (!string.IsNullOrEmpty(customer.MobileCNo))
            {
                var existingCustomerByPhone = await _custMastRepository.GetByPhoneNumberAndBranchAsync(customer.MobileCNo, customer.BranchCCode);
                if (existingCustomerByPhone != null)
                {
                    validationErrors.Add($"هذا العميل موجود مسبقا");
                }
            }

            // Check for duplicate account types in the same branch
            if (customer.AccountMasts != null && customer.AccountMasts.Any())
            {
                foreach (var account in customer.AccountMasts)
                {
                    if (!string.IsNullOrEmpty(account.ActCType) && !string.IsNullOrEmpty(account.BranchCCode))
                    {
                        // For new customer, we need to check if there are existing customers with this account type
                        // This is a business rule check - you might want to modify this based on your requirements
                        // For now, we'll allow it since this is a new customer
                    }
                }
            }

            if (validationErrors.Any())
            {
                throw new InvalidOperationException(string.Join("; ", validationErrors));
            }
        }

        public async Task<CustMast> UpdateCustomerAsync(CustMast customer)
        {
            try
            {
                await _custMastRepository.UpdateAsync(customer);
                await _custMastRepository.SaveChangesAsync();
                return customer;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteCustomerAsync(string branchCode, decimal custId)
        {
            try
            {
                var customer = await _custMastRepository.GetByBranchAndIdAsync(branchCode, custId);
                if (customer == null)
                    return false;

                await _custMastRepository.DeleteAsync(customer);
                await _custMastRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateCustomerReviewAsync(UpdateCustomerReviewDto reviewDto, string reviewedBy)
        {
            try
            {
                // Use a direct database update to avoid tracking conflicts
                using var connection = CreateNewConnection();
                await connection.OpenAsync();

                var updateSql = @"
                    UPDATE SSDBONLINE.""cust_mast"" 
                    SET ""review_status"" = :reviewStatus, 
                        ""reviewed_by"" = :reviewedBy
                    WHERE ""id"" = :customerId";

                using var command = connection.CreateCommand();
                command.CommandText = updateSql;

                var reviewStatusParam = new OracleParameter("reviewStatus", OracleDbType.Varchar2);
                reviewStatusParam.Value = reviewDto.ReviewStatus;
                reviewStatusParam.Direction = ParameterDirection.Input;
                
                var reviewedByParam = new OracleParameter("reviewedBy", OracleDbType.Varchar2);
                reviewedByParam.Value = reviewedBy;
                reviewedByParam.Direction = ParameterDirection.Input;
                
                var customerIdParam = new OracleParameter("customerId", OracleDbType.Varchar2);
                customerIdParam.Value = reviewDto.CustomerId;
                customerIdParam.Direction = ParameterDirection.Input;

                command.Parameters.Add(reviewStatusParam);
                command.Parameters.Add(reviewedByParam);
                command.Parameters.Add(customerIdParam);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                
                return rowsAffected > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<AccountMast>> GetCustomerAccountsAsync(string branchCode, decimal custId)
        {
            return await _custMastRepository.GetCustomerAccountsAsync(branchCode, custId);
        }

        private OracleConnection CreateNewConnection()
        {
            var connectionString = _configuration.GetConnectionString("OracleConnection");
            return new OracleConnection(connectionString);
        }
        
        private string? ProcessCommaSeparatedValues(string? commaSeparatedValues)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedValues))
                return null;
                
            var values = commaSeparatedValues.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();
                
            return values.Any() ? string.Join(",", values) : null;
        }
        
        private (string? status, string? reviewStatus) ApplyRoleBasedFiltering(string? userRole, string? statusValues, string? reviewStatusValues)
        {
            var effectiveStatus = statusValues;
            var effectiveReviewStatus = reviewStatusValues;
            
            if (!string.IsNullOrWhiteSpace(userRole))
            {
                switch (userRole)
                {
                    case "Reviewer":
                        // Reviewer can only see Pending records, override any provided reviewStatus
                        effectiveReviewStatus = "Pending";
                        break;
                    case "Manager":
                        // Manager can see Approved or Rejected records only
                        // If manager provides specific filter, validate it only contains Approved/Rejected
                        if (!string.IsNullOrWhiteSpace(reviewStatusValues))
                        {
                            var requestedStatuses = reviewStatusValues.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(v => v.Trim())
                                .Where(v => !string.IsNullOrWhiteSpace(v))
                                .ToList();
                            
                            // Filter out any Pending requests and keep only Approved/Rejected
                            var allowedStatuses = requestedStatuses
                                .Where(s => s.Equals("Approved", StringComparison.OrdinalIgnoreCase) || 
                                           s.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                                .ToList();
                            
                            if (allowedStatuses.Any())
                            {
                                effectiveReviewStatus = string.Join(",", allowedStatuses);
                            }
                            else
                            {
                                // If no valid statuses provided, default to both Approved and Rejected
                                effectiveReviewStatus = "Approved,Rejected";
                            }
                        }
                        else
                        {
                            // If no filter provided, show both Approved and Rejected
                            effectiveReviewStatus = "Approved,Rejected";
                        }
                        break;
                    case "Admin":
                        // Admin can see all records, no filtering needed
                        break;
                }
            }
            
            return (effectiveStatus, effectiveReviewStatus);
        }
    }
}
