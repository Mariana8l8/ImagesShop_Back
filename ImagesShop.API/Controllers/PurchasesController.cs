using ClosedXML.Excel;
using ImagesShop.Application.DTOs;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImagesShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseHistoryService _purchaseService;

        public PurchasesController(IPurchaseHistoryService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpGet(Name = "GetAllPurchasesHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var list = await _purchaseService.GetAllAsync(cancellationToken);
            var dto = list.Select(MapToDto);
            return Ok(dto);
        }

        [HttpGet("{id:guid}", Name = "GetPurchaseHistoryById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var purchase = await _purchaseService.GetByIdAsync(id, cancellationToken);
            return purchase is null ? NotFound() : Ok(MapToDto(purchase));
        }

        [HttpPost(Name = "AddPurchaseHistory")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] PurchaseHistoryDTO dto, CancellationToken cancellationToken)
        {
            var entity = MapToEntity(dto);
            var created = await _purchaseService.CreateAsync(entity, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, MapToDto(created));
        }

        [HttpDelete("{id:guid}", Name = "DeletePurchaseHistoryById")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _purchaseService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpGet("export", Name = "ExportPurchases")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Export(CancellationToken cancellationToken)
        {
            var purchases = await _purchaseService.GetAllAsync(cancellationToken);
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Purchases");

            ws.Cell(1, 1).Value = "UserName";
            ws.Cell(1, 2).Value = "UserEmail";
            ws.Cell(1, 3).Value = "ImageId";
            ws.Cell(1, 4).Value = "ImageTitle";
            ws.Cell(1, 5).Value = "ImagePrice";
            ws.Cell(1, 6).Value = "PurchasedAt";

            var row = 2;
            foreach (var purchase in purchases)
            {
                ws.Cell(row, 1).Value = purchase.UserName;
                ws.Cell(row, 2).Value = purchase.UserEmail;
                ws.Cell(row, 3).Value = purchase.ImageId.ToString();
                ws.Cell(row, 4).Value = purchase.ImageTitle;
                ws.Cell(row, 5).Value = purchase.ImagePrice;
                ws.Cell(row, 6).Value = purchase.PurchasedAt;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"purchases-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(stream.ToArray(), contentType, fileName);
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

        private static PurchaseHistory MapToEntity(PurchaseHistoryDTO dto) => new PurchaseHistory
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            UserName = dto.UserName,
            UserEmail = dto.UserEmail,
            ImageId = dto.ImageId,
            ImagePrice = dto.ImagePrice,
            ImageTitle = dto.ImageTitle,
            PurchasedAt = dto.PurchasedAt == default ? DateTime.UtcNow : dto.PurchasedAt
        };
    }
}
