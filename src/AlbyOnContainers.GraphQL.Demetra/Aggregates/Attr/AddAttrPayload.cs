using Demetra.Model;

namespace Demetra.Aggregates.Attr
{
    public class AddAttrPayload: AttrPayloadBase
    {
        public AddAttrPayload(Model.Attr attr) : base(attr)
        {
        }
        
        public AddAttrPayload(UserError error) : base(new[] { error })
        {
        }
    }
}