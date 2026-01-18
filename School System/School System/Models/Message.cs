using System;
using System.Collections.Generic;

namespace School_System.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int? SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public string? MessageBody { get; set; }

    public string? Attachment { get; set; }

    public DateTime? Timestamp { get; set; }

    public bool? ReadStatus { get; set; }

    public virtual User? Receiver { get; set; }

    public virtual User? Sender { get; set; }
}
