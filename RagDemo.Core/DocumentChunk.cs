using Azure.Search.Documents.Indexes;

public class DocumentChunk
{
    [SimpleField(IsKey = true, IsFilterable = true)]
    public string Id { get; set; }

    [SearchableField]
    public string Content { get; set; }

    [SimpleField(IsFilterable = true)]
    public string Source { get; set; }

    [SimpleField]
    public int ChunkIndex { get; set; }

    public float[] Embedding { get; set; }
}