using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Abstractions
{
    /// <summary>
    /// Defines a contract for serializing and deserializing messages.
    /// </summary>
    public interface IMessageSerializer
    {
        string Serialize<T>(T message);
        T? Deserialize<T>(string payload);
    }
}
