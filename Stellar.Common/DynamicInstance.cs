using System.Dynamic;

namespace Stellar.Common;

/// <summary>
/// A System.Dynamic object override whose members are implemented using a Stellar.DAL default value dictionary.
/// </summary>
/// <example>
/// Whereas: 
///   dynamic person = new { Age = 42 };
///   Assert.Throws<RuntimeBinderException>(var name = person.Name); // person does not contain a definition for 'Name'
//
///   var person = new DynamicInstance();
///   Assert.Null(person.FirstName);
///   person.Alterego = "Superman";
///   Assert.Equals("Superman", person.ALTEREGO);
/// </example>
public class DynamicInstance(IDictionary<string, object>? dictionary = null, IEqualityComparer<string>? comparer = null) : DynamicObject
{
    protected readonly IDictionary<string, object> Dictionary = new DefaultValueDictionary<string, object>(dictionary, comparer ?? StringComparer.InvariantCultureIgnoreCase);

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        result = Dictionary[binder.Name];

        return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        Dictionary[binder.Name] = value!;

        return true;
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
    {
        if (!Dictionary.TryGetValue(binder.Name, out object? value) || value is not Delegate)
        {
            return base.TryInvokeMember(binder, args, out result);
        }

        var delegateValue = value as Delegate;

        result = delegateValue?.DynamicInvoke(args);

        return true;
    }
}
