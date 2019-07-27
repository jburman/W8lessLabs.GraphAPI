using System;

namespace W8lessLabs.GraphAPI
{
    public class DriveItem : IEquatable<DriveItem>, IComparable<DriveItem>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public DateTimeOffset LastModifiedDateTime { get; set; }
        public string ETag { get; set; }
        public string WebUrl { get; set; }
        public File File { get; set; }
        public Folder Folder { get; set; }
        public ItemReference ParentReference { get; set; }
        public Shared Shared { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is DriveItem)) return false;
            return Equals((DriveItem)obj);
        }

        public bool Equals(DriveItem otherItem)
        {
            if (otherItem is null)
                return false;
            else
                return Id == otherItem.Id && Name == otherItem.Name && ETag == otherItem.ETag;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            if (Id != null)
                hash = hash * 31 + Id.GetHashCode();
            if (Name != null)
                hash = hash * 31 + Name.GetHashCode();
            if (ETag != null)
                hash = hash * 31 + ETag.GetHashCode();
            return hash;
        }

        public int CompareTo(DriveItem other)
        {
            if (other is null || other.Name is null)
                return 1;
            else
                return Name.CompareTo(other.Name);
        }
    }
}
