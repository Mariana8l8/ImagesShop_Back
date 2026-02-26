using ImagesShop.Application.DTOs;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImagesShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseHistoryService _purchaseHistoryService;

        public PurchasesController(IPurchaseHistoryService purchaseHistoryService)
        {
            _purchaseHistoryService = purchaseHistoryService;
        }

        [HttpGet(Name = "GetAllPurchasesHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var purchaseHistoryContent = await _purchaseHistoryService.GetAllAsync(cancellationToken);
            var purchaseHistoryDto = purchaseHistoryContent.Select(purchase => MapToDto(purchase));
            
            return Ok(purchaseHistoryDto);
        }

        [HttpGet("{id:guid}", Name = "GetPurchaseHistoryById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var purchaseHistoryItem = await _purchaseHistoryService.GetByIdAsync(id, cancellationToken);
            
            if (purchaseHistoryItem is null)
            {
                return NotFound();
            }

            return Ok(MapToDto(purchaseHistoryItem));
        }

        [HttpPost(Name = "AddPurchaseHistory")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] PurchaseHistoryDTO purchaseHistoryDto, CancellationToken cancellationToken)
        {
            var purchaseHistoryEntity = MapToEntity(purchaseHistoryDto);
            var createdPurchaseHistory = await _purchaseHistoryService.CreateAsync(purchaseHistoryEntity, cancellationToken);
            
            return CreatedAtAction(nameof(Get), new { id = createdPurchaseHistory.Id }, MapToDto(createdPurchaseHistory));
        }

        [HttpDelete("{id:guid}", Name = "DeletePurchaseHistoryById")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _purchaseHistoryService.DeleteAsync(id, cancellationToken);
            
            return NoContent();
        }

        [HttpGet("export", Name = "ExportPurchases")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Export(CancellationToken cancellationToken)
        {
            var excelFileBytes = await _purchaseHistoryService.ExportToExcelAsync(cancellationToken);
            var fileName = $"purchases-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            
            return File(excelFileBytes, contentType, fileName);
        }

        private static PurchaseHistoryDTO MapToDto(PurchaseHistory purchaseHistory) => new PurchaseHistoryDTO
        {
            Id = purchaseHistory.Id,
            UserName = purchaseHistory.UserName,
            UserEmail = purchaseHistory.UserEmail,
            ImageId = purchaseHistory.ImageId,
            ImagePrice = purchaseHistory.ImagePrice,
            ImageTitle = purchaseHistory.ImageTitle,
            PurchasedAt = purchaseHistory.PurchasedAt
        };

        private static PurchaseHistory MapToEntity(PurchaseHistoryDTO purchaseHistoryDto) => new PurchaseHistory
        {
            Id = purchaseHistoryDto.Id == Guid.Empty ? Guid.NewGuid() : purchaseHistoryDto.Id,
            UserName = purchaseHistoryDto.UserName,
            UserEmail = purchaseHistoryDto.UserEmail,
            ImageId = purchaseHistoryDto.ImageId,
            ImagePrice = purchaseHistoryDto.ImagePrice,
            ImageTitle = purchaseHistoryDto.ImageTitle,
            PurchasedAt = purchaseHistoryDto.PurchasedAt == default ? DateTime.UtcNow : purchaseHistoryDto.PurchasedAt
        };
    }
}
