using System.Numerics;
using Lagrange.Core.Internal.Packets.Message.Element;
using Lagrange.Core.Internal.Packets.Message.Element.Implementation;
using Lagrange.Core.Internal.Packets.Service.Oidb.Common;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Message.Entity;

[MessageElement(typeof(VideoFile))]
public class VideoEntity : IMessageEntity
{
    public string FilePath { get; set; } = string.Empty;

    public string VideoHash { get; set; } = string.Empty;
    
    public Vector2 Size { get; }
    
    public int VideoSize { get; }
    
    public int VideoLength { get; set; }

    public string VideoUrl { get; set; } = string.Empty;

    #region Internal Properties

    internal Stream? VideoStream { get; set; }
    
    internal string? VideoUuid { get; }
    
    internal MsgInfo? MsgInfo { get; set; }

    internal VideoFile? Compat { get; set; }
    

    #endregion
    
    internal VideoEntity() { }
    
    internal VideoEntity(Vector2 size, int videoSize, string filePath, string fileMd5, string videoUuid)
    {
        Size = size;
        VideoSize = videoSize;
        FilePath = filePath;
        VideoHash = fileMd5;
        VideoUuid = videoUuid;
    }
    
    public VideoEntity(string filePath, int videoLength = 0)
    {
        FilePath = filePath;
        VideoStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        VideoSize = (int)VideoStream.Length;
        VideoLength = videoLength;
    }

    public VideoEntity(byte[] file, int videoLength = 0)
    {
        FilePath = string.Empty;
        VideoStream = new MemoryStream(file);
        VideoSize = (int)VideoStream.Length;
        VideoLength = videoLength;
    }
    
    IEnumerable<Elem> IMessageEntity.PackElement()
    {
        var common = MsgInfo.Serialize();

        var elems = new List<Elem>
        {
            new()
            {
                CommonElem = new CommonElem
                {
                    ServiceType = 48,
                    PbElem = common.ToArray(),
                    BusinessType = 21
                }
            }
        };
        
        if (Compat != null) elems.Add(new Elem { VideoFile = Compat });

        return elems;
    }

    IMessageEntity? IMessageEntity.UnpackElement(Elem elem)
    {
        if (elem.VideoFile is not { } videoFile) return null;
        
        var size = new Vector2(videoFile.ThumbWidth, videoFile.ThumbHeight);
        return new VideoEntity(size, videoFile.FileSize, videoFile.FileName, videoFile.FileMd5.Hex(), videoFile.FileUuid);
    }

    public string ToPreviewString()
    {
        return $"[Video {Size.X}x{Size.Y}]: {VideoSize} {VideoUrl}";
    }
}