using System.Runtime.CompilerServices;

namespace speedrocket;

public class RegisterRequest
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

	public string Password
	{
		[CompilerGenerated]
		get
		{
			return _003CPassword_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			_003CPassword_003Ek__BackingField = value;
		}
	}

	public RegisterRequest(string guid, string password)
	{
		Guid = guid;
		Password = password;
	}
}
