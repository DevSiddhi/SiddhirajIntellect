using Google.Protobuf;

namespace JIO_QH.Proto
{
    public static class ProtoMasterReader
    {
        public static IEnumerable<T> Read<T>(
            string file,
            MessageParser<T> parser)
            where T : IMessage<T>
        {
            using FileStream fs = new FileStream(
                file,
                FileMode.Open,
                FileAccess.Read);

            while (fs.Position < fs.Length)
            {
                yield return parser.ParseDelimitedFrom(fs);
            }
        }
    }
}