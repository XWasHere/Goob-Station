using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

[Table("polls")]
[Index(nameof(StartTime))]
[Index(nameof(EndTime))]
[Index(nameof(Active))]
public sealed class Poll
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = default!;

    [StringLength(2000)]
    public string Description { get; set; } = default!;

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [ForeignKey("CreatedBy")]
    public Guid? CreatedById { get; set; }

    public Player? CreatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public bool Active { get; set; }

    public bool AllowMultipleChoices { get; set; }

    public List<PollOption> Options { get; set; } = default!;
    public List<PollVote> Votes { get; set; } = default!;
}

[Table("poll_options")]
[Index(nameof(PollId))]
public sealed class PollOption
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PollId { get; set; }

    public Poll Poll { get; set; } = default!;

    [Required, StringLength(500)]
    public string OptionText { get; set; } = default!;

    public int DisplayOrder { get; set; }

    public List<PollVote> Votes { get; set; } = default!;
}

[Table("poll_votes")]
[Index(nameof(PollId))]
[Index(nameof(PlayerUserId))]
[Index(nameof(PollOptionId))]
public sealed class PollVote
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PollId { get; set; }

    public Poll Poll { get; set; } = default!;

    [Required]
    public int PollOptionId { get; set; }

    public PollOption PollOption { get; set; } = default!;

    [Required, ForeignKey("Player")]
    public Guid PlayerUserId { get; set; }

    public Player Player { get; set; } = default!;

    [Required]
    public DateTime VotedAt { get; set; }
}

[Index(nameof(PlayerUserId))]
public sealed class GoobCurrencyStoreInventoryItem
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Player")]
    public Guid PlayerUserId { get; set; }
    public Player Player { get; set; } = default!;

    public string Prototype { get; set; } = default!;

    public bool Immediate { get; set; }

    public int UsesLeft { get; set; }
}

[PrimaryKey(nameof(PlayerUserId), nameof(Prototype))]
public sealed class GoobCurrencyStorePermanentItem
{
    [ForeignKey("Player")]
    public Guid PlayerUserId { get; set; }
    public Player Player { get; set; } = default!;

    public string Prototype { get; set; } = default!;
}
