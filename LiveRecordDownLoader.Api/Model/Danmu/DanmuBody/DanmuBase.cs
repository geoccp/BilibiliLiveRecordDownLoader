using Api.Enums;

namespace Api.Model.Danmu.DanmuBody
{
	public class DanmuBase : IDanmu
	{
		public DanmuCommand Cmd { get; set; }
	}
}
