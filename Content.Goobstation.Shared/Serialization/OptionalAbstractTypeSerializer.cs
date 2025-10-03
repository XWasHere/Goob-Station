using System.Linq;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Content.Goobstation.Shared.Serialization;

/// <summary>
///     Deserializes lists of objects and skips over objects whose type does not exist.
/// </summary>
/// <typeparam name="TContainedType">Abstract base type</typeparam>
/// <seealso cref="CurrencyStoreItemPrototype.Conditions"/>
public sealed class OptionalAbstractTypeSerializer<TContainedType> : ITypeSerializer<TContainedType[], SequenceDataNode>
    where TContainedType : class
{
    public ValidationNode Validate(ISerializationManager serializationManager,
        SequenceDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        var validated = new List<ValidationNode>();

        foreach (var contained in node.Sequence)
        {
            // Check that the node specifies a type
            if (contained.Tag == null || !contained.Tag.StartsWith("!type:"))
            {
                validated.Add(new ErrorNode(contained, "Node does not specify a type"));
                continue;
            }

            // Check that the type is valid
            var typename = contained.Tag.Substring(6);
            var type = serializationManager.ReflectionManager.YamlTypeTagLookup(typeof(TContainedType), typename);

            // Skip over it if it doesn't exist
            if (type == null)
                continue;

            // Check that the prototype is valid
            validated.Add(serializationManager.ValidateNode(type, contained, context));
        }

        return new ValidatedSequenceNode(validated);
    }

    public TContainedType[] Read(ISerializationManager serializationManager,
        SequenceDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<TContainedType[]>? instanceProvider = null)
    {
        var objects = new List<TContainedType>();

        foreach (var contained in node.Sequence)
        {
            if (contained.Tag == null)
                continue;

            // Resolve type
            var type = serializationManager.ReflectionManager.YamlTypeTagLookup(
                typeof(TContainedType),
                contained.Tag.Substring(6));

            // Skip over server/client specific types if we don't have them.
            if (type == null)
                continue;

            // Deserialize the value
            if (serializationManager.Read(type, contained, context) is TContainedType value)
                objects.Add(value);
        }

        return objects.ToArray();
    }

    public DataNode Write(ISerializationManager serializationManager,
        TContainedType[] value,
        IDependencyCollection dependencies,
        bool alwaysWrite = false,
        ISerializationContext? context = null)
    {
        var nodes = new List<DataNode>();

        // Serialize values
        foreach (var obj in value)
        {
            nodes.Add(serializationManager.WriteValue(obj));
        }

        return new SequenceDataNode(nodes);
    }
}
