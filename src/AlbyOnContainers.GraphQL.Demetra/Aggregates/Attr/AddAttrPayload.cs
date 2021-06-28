using Demetra.Common;
using Demetra.Model;

namespace Demetra.Aggregates.Attr
{
    public class AddAttrPayload: Payload
    {
        public AddAttrPayload(Model.Attr attr)
        {
            Attr = attr;
        }

        public AddAttrPayload(UserError error) : base(new[] { error })
        {
        }

        public Model.Attr? Attr { get; }
    }
}