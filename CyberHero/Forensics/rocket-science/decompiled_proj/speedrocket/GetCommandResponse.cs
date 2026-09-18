using System.Runtime.CompilerServices;

namespace speedrocket;

public class GetCommandResponse
{
	public string command
	{
		[CompilerGenerated]
		get
		{
			return _003Ccommand_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			_003Ccommand_003Ek__BackingField = value;
		}
	}

	public GetCommandResponse(string command)
	{
		this.command = command;
	}
}
