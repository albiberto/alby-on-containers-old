namespace IdentityServer.Areas.Roles.Models
{
    public record RoleViewModel
    {
        public RoleViewModel(string name, bool @checked)
        {
            Name = name;
            Checked = @checked;
        }

        public string Name { get; }
        public bool Checked { get; }
    }
}