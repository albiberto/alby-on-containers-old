using System.Reflection;
using Demetra.Infrastructure;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

namespace Demetra.Extensions
{
    public class UseApplicationDbContextAttribute : ObjectFieldDescriptorAttribute
    {
        public override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
        {
            descriptor.UseDbContext<ApplicationDbContext>();
        }
    }
}