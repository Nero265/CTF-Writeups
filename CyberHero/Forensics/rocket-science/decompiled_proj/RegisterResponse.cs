using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

internal record RegisterResponse(string key)
{
	[CompilerGenerated]
	protected virtual global::System.Type EqualityContract
	{
		[CompilerGenerated]
		get
		{
			return typeof(RegisterResponse);
		}
	}

	public RegisterResponse(string key)
	{
		_003Ckey_003Ek__BackingField = key;
		base._002Ector();
	}

	[CompilerGenerated]
	public override string ToString()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		StringBuilder val = new StringBuilder();
		val.Append("RegisterResponse");
		val.Append(" { ");
		if (PrintMembers(val))
		{
			val.Append(' ');
		}
		val.Append('}');
		return ((object)val).ToString();
	}

	[CompilerGenerated]
	protected virtual bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("key = ");
		builder.Append((object)key);
		return true;
	}

	[CompilerGenerated]
	public override int GetHashCode()
	{
		return EqualityComparer<global::System.Type>.get_Default().GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.get_Default().GetHashCode(_003Ckey_003Ek__BackingField);
	}

	[CompilerGenerated]
	public virtual bool Equals(RegisterResponse? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other!.EqualityContract)
			{
				return EqualityComparer<string>.get_Default().Equals(_003Ckey_003Ek__BackingField, other!._003Ckey_003Ek__BackingField);
			}
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	protected RegisterResponse(RegisterResponse original)
	{
		_003Ckey_003Ek__BackingField = original._003Ckey_003Ek__BackingField;
	}
}
