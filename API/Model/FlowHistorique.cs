// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Data;

namespace API.Model
{
    public class ValidationsHistory
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }
        [ForeignKey(nameof(FromUserId))]
        public virtual User User1 { get; set; }
        public Guid? ToDocumentStepId { get; set; }
        public Guid DocumentId { get; set; }
        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        public string? Comment { get; set; }
        public DateTime CreationDate { get; set; }
        public DocumentActionType ActionType { get; set; }
    }
}
