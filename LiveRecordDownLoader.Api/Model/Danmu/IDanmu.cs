using Api.Enums;

namespace Api.Model.Danmu
{
	public interface IDanmu
	{
		DanmuCommand Cmd { get; set; }
	}
}
