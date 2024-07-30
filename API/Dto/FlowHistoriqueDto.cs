// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using API.Data;

namespace API.Dto
{
    public class FlowHistoriqueDto
    {
        public Guid? Id { get; set; }
        public UserDto User1 { get; set; }
        public UserDto User2 { get; set; }
        public DocumentDto document { get; set; }
        public string? commentaire { get; set; }
        public DateTime? dateaction { get; set; }
        public DocumentActionType? action { get; set; }
    }
}
