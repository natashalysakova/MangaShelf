using MangaShelf.DAL.MangaShelf.Models;

namespace MangaShelf.DAL.MangaShelf
{
    public static class ContextMigrationExtentions
    {
        public static Ownership.VolumeStatus ConvertToOwnershipStatus(this PurchaseStatus purchaseStatus)
        {
            return purchaseStatus switch
            {
                PurchaseStatus.Announced => Ownership.VolumeStatus.Wishlist,
                PurchaseStatus.Wishlist => Ownership.VolumeStatus.Wishlist,
                PurchaseStatus.Preordered => Ownership.VolumeStatus.Preorder,
                PurchaseStatus.Bought => Ownership.VolumeStatus.Own,
                PurchaseStatus.Pirated => Ownership.VolumeStatus.Own,
                PurchaseStatus.Gift => Ownership.VolumeStatus.Own,
                PurchaseStatus.Free => Ownership.VolumeStatus.Own,
                PurchaseStatus.GiftedAway => Ownership.VolumeStatus.Gone,
                _ => throw new NotImplementedException()
            };
        }
    }
}
