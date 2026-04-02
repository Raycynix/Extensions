using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessageCodecResolver(IEnumerable<IMessageCodec> codecs) : IMessageCodecResolver
{
    public IMessageCodec Resolve(Type messageType, MessageFormat format)
    {
        ArgumentNullException.ThrowIfNull(messageType);

        var codec = codecs
            .Select((candidate, index) => new
            {
                Candidate = candidate,
                Index = index,
                Matches = candidate.Format == format && candidate.CanHandle(messageType)
            })
            .Where(entry => entry.Matches)
            .OrderBy(entry => GetSpecificityRank(entry.Candidate, messageType))
            .ThenByDescending(entry => entry.Index)
            .Select(entry => entry.Candidate)
            .FirstOrDefault();

        return codec ?? throw new InvalidOperationException(
            $"No messaging codec is registered for format '{format}' and payload type '{messageType.FullName}'.");
    }

    private static int GetSpecificityRank(IMessageCodec codec, Type messageType)
    {
        var visited = new HashSet<Type>();
        var queue = new Queue<(Type Type, int Distance)>();

        queue.Enqueue((messageType, 0));
        visited.Add(messageType);

        var maxDistance = 0;

        while (queue.Count > 0)
        {
            var (candidateType, distance) = queue.Dequeue();

            if (codec.CanHandle(candidateType))
            {
                maxDistance = Math.Max(maxDistance, distance);
            }

            var baseType = candidateType.BaseType;
            if (baseType is not null && visited.Add(baseType))
            {
                queue.Enqueue((baseType, distance + 1));
            }

            foreach (var interfaceType in candidateType.GetInterfaces())
            {
                if (visited.Add(interfaceType))
                {
                    queue.Enqueue((interfaceType, distance + 1));
                }
            }
        }

        return maxDistance;
    }
}
