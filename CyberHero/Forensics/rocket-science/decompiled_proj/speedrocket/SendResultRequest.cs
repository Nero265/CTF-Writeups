using System.Runtime.CompilerServices;

namespace speedrocket;

public class SendResultRequest
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

	public string Result
	{
		[CompilerGenerated]
		get
		{
			return _003CResult_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			_003CResult_003Ek__BackingField = value;
		}
	}

	public SendResultRequest(string guid, string result)
	{
		Guid = guid;
		Result = result;
	}
}
