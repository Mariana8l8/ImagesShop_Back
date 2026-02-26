using ClosedXML.Excel;
using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class PurchaseHistoryService : IPurchaseHistoryService
    {
        private readonly IPurchaseHistoryRepository _purchaseHistoryRepository;

        public PurchaseHistoryService(IPurchaseHistoryRepository purchaseHistoryRepository)
        {
            _purchaseHistoryRepository = purchaseHistoryRepository;
        }

        public async Task<IEnumerable<PurchaseHistory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _purchaseHistoryRepository.GetAllAsync(cancellationToken);
        }

        public async Task<PurchaseHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) 
            {
                return null;
            }
            
            return await _purchaseHistoryRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<PurchaseHistory> CreateAsync(PurchaseHistory purchaseHistory, CancellationToken cancellationToken = default)
        {
            if (purchaseHistory is null) throw new ArgumentNullException(nameof(purchaseHistory));

            if (purchaseHistory.Id == Guid.Empty)
            {
                purchaseHistory.Id = Guid.NewGuid();
            }

            purchaseHistory.UserName ??= string.Empty;
            purchaseHistory.UserEmail ??= string.Empty;
            purchaseHistory.ImageTitle ??= string.Empty;
            purchaseHistory.ImagePrice = purchaseHistory.ImagePrice < 0m ? 0m : purchaseHistory.ImagePrice;
            purchaseHistory.PurchasedAt = purchaseHistory.PurchasedAt == default ? DateTime.UtcNow : purchaseHistory.PurchasedAt;

            await _purchaseHistoryRepository.AddAsync(purchaseHistory, cancellationToken);
            
            return purchaseHistory;
        }

        public async Task UpdateAsync(PurchaseHistory purchaseHistory, CancellationToken cancellationToken = default)
        {
            if (purchaseHistory is null) throw new ArgumentNullException(nameof(purchaseHistory));
            if (purchaseHistory.Id == Guid.Empty) throw new ArgumentException("The record instance must have a valid identifier.", nameof(purchaseHistory));

            var existingPurchaseHistory = await _purchaseHistoryRepository.GetByIdAsync(purchaseHistory.Id, cancellationToken);
            if (existingPurchaseHistory is null) 
            {
                throw new InvalidOperationException("The purchase history record was not found.");
            }

            existingPurchaseHistory.UserName = purchaseHistory.UserName ?? existingPurchaseHistory.UserName;
            existingPurchaseHistory.UserEmail = purchaseHistory.UserEmail ?? existingPurchaseHistory.UserEmail;
            existingPurchaseHistory.ImageTitle = purchaseHistory.ImageTitle ?? existingPurchaseHistory.ImageTitle;
            existingPurchaseHistory.ImagePrice = purchaseHistory.ImagePrice;

            await _purchaseHistoryRepository.UpdateAsync(existingPurchaseHistory, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("The record identifier cannot be empty.", nameof(id));

            await _purchaseHistoryRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<byte[]> ExportToExcelAsync(CancellationToken cancellationToken = default)
        {
            var purchaseHistoryEntries = await _purchaseHistoryRepository.GetAllAsync(cancellationToken);
            
            using var excelWorkbook = new XLWorkbook();
            var excelWorksheet = excelWorkbook.Worksheets.Add("Purchases");

            excelWorksheet.Cell(1, 1).Value = "UserName";
            excelWorksheet.Cell(1, 2).Value = "UserEmail";
            excelWorksheet.Cell(1, 3).Value = "ImageId";
            excelWorksheet.Cell(1, 4).Value = "ImageTitle";
            excelWorksheet.Cell(1, 5).Value = "ImagePrice";
            excelWorksheet.Cell(1, 6).Value = "PurchasedAt";

            var currentRowIndex = 2;
            foreach (var purchaseRecord in purchaseHistoryEntries)
            {
                excelWorksheet.Cell(currentRowIndex, 1).Value = purchaseRecord.UserName;
                excelWorksheet.Cell(currentRowIndex, 2).Value = purchaseRecord.UserEmail;
                excelWorksheet.Cell(currentRowIndex, 3).Value = purchaseRecord.ImageId.ToString();
                excelWorksheet.Cell(currentRowIndex, 4).Value = purchaseRecord.ImageTitle;
                excelWorksheet.Cell(currentRowIndex, 5).Value = purchaseRecord.ImagePrice;
                excelWorksheet.Cell(currentRowIndex, 6).Value = purchaseRecord.PurchasedAt;
                currentRowIndex++;
            }

            using var excelMemoryStream = new MemoryStream();
            excelWorkbook.SaveAs(excelMemoryStream);
            
            return excelMemoryStream.ToArray();
        }
    }
}