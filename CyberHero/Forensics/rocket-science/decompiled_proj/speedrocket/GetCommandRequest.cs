using System.Runtime.CompilerServices;

namespace speedrocket;

public class GetCommandRequest
{
	public string Guid
	{
		[CompilerGenerated]
		get
		{
			return _003CGuid_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			_003CGuid_003Ek__BackingField = value;
		}
	}

	public GetCommandRequest(string guid)
	{
		Guid = guid;
	}
}
