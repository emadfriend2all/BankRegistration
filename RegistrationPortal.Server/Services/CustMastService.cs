using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.Repositories;
using RegistrationPortal.Server.DTOs;
using Mapster;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Transactions;

namespace RegistrationPortal.Server.Services
{
    public class CustMastService : ICustMastService
    {
        private readonly ICustMastRepository _custMastRepository;
        private readonly IAccountMastRepository _accountMastRepository;
        private readonly ICustomerDocumentService _customerDocumentService;
        private readonly ILogger<CustMastService> _logger;

        public CustMastService(ICustMastRepository custMastRepository, IAccountMastRepository accountMastRepository, ICustomerDocumentService customerDocumentService, ILogger<CustMastService> logger)
        {
            _custMastRepository = custMastRepository;
            _accountMastRepository = accountMastRepository;
            _customerDocumentService = customerDocumentService;
            _logger = logger;
        }

        public async Task<CustMast?> GetCustomerAsync(string branchCode, decimal custId)
        {
            return await _custMastRepository.GetByBranchAndIdAsync(branchCode, custId);
        }

        public async Task<IEnumerable<CustMast>> GetAllCustomersAsync()
        {
            return await _custMastRepository.GetAllAsync();
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

                    // Create FATCA record if FATCA data is provided
                    if (HasFatcaData(customerDto))
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

        public async Task<IEnumerable<AccountMast>> GetCustomerAccountsAsync(string branchCode, decimal custId)
        {
            return await _custMastRepository.GetCustomerAccountsAsync(branchCode, custId);
        }
    }
}
