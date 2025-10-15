﻿namespace InteractiveLeads.Application.Feature.Identity.Roles
{
    public class RoleResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Permissions { get; set; } = [];
    }
}
