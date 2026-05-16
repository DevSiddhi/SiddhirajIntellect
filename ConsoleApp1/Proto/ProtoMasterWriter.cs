using Google.Protobuf;

namespace ConsoleApp1.Proto
{
    public static class ProtoMasterWriter
    {
        public static void CreateProtoMasterFile<T>(
            List<T> records,
            string outputFile)
            where T : IMessage<T>
        {
            using FileStream fs = new FileStream(
                outputFile,
                FileMode.Create,
                FileAccess.Write);

            foreach (var record in records)
            {
                record.WriteDelimitedTo(fs);
            }
        }
    }
}