# Server Logging Configuration

This application uses Serilog for comprehensive server-side logging with file-based output.

## Log Files Location

All log files are stored in the `logs/` directory:

- **General logs**: `logs/log-YYYY-MM-DD.log` - Contains all log levels (Information, Warning, Error)
- **Error logs**: `logs/errors-YYYY-MM-DD.log` - Contains only Error and Fatal level logs
- **Rolling**: Log files are automatically rolled daily

## Usage Examples

### In Controllers
```csharp
public class CustomerController : ControllerBase
{
    private readonly IAppLoggerService _logger;

    public CustomerController(IAppLoggerService logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> GetCustomers()
    {
        try
        {
            await _logger.LogInformationAsync("Getting all customers");
            
            // Your logic here
            
            await _logger.LogInformationAsync("Successfully retrieved {Count} customers", customers.Count);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Error occurred while getting customers");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

## Key Features

1. **Global Exception Handling**: All unhandled exceptions are logged with detailed context
2. **API Request Logging**: All API requests are logged with timing and user information
3. **Structured Logging**: Consistent log formatting through AppLoggerService
4. **File-based Output**: Automatic daily log rotation
5. **Error-specific Logs**: Separate error-only log files for easy monitoring

## Log Levels

- **Debug**: Detailed debugging information
- **Information**: General information about application flow
- **Warning**: Potentially harmful situations
- **Error**: Error events that might still allow the application to continue
- **Fatal**: Very serious errors that will likely lead to application failure
