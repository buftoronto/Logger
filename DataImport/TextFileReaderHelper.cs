using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport
{
    public abstract class LmsTextReaderBase : StreamReader
    {
        //OPM New report code change SM 
        //protected static Regex regHeader = new Regex(@"^User ID\s+Agency Sub Element Code.+");
        protected static Regex regHeader = new Regex(@"^User - User ID\s+Transcript - Agency Sub Element Code.+");
        public LmsTextReaderBase(string sourceFile)
            : base(sourceFile)
        {
        }

        public LmsTextReaderBase(Stream stream)
            : base(stream)
        {
        }

        public abstract int GetSkipLines();

        protected int GetSkipLines(TextReader stream)
        {
            int ignoredLines = 0;
            string line;
            while ((line = stream.ReadLine()) != null)
            {
                if (regHeader.IsMatch(line))
                {
                    break;
                }
                ++ignoredLines;
            }
            return ignoredLines;
        }
    }

    public class FileTextReader : LmsTextReaderBase
    {
        private string filePath;
        public FileTextReader(string sourceFile)
            : base(sourceFile)
        {
            filePath = sourceFile;
        }

        public override int GetSkipLines()
        {
            using (TextReader fileReader = File.OpenText(filePath))
            {
                return GetSkipLines(fileReader);
            }
        }
    }

    public class MemoryTextReader : LmsTextReaderBase
    {
        public MemoryTextReader(Stream stream)
            :base(stream)
        {
        }

        public override int GetSkipLines()
        {
            var skipLines = GetSkipLines(this);
            BaseStream.Position = 0;
            DiscardBufferedData();
            return skipLines;
        }
    }
}
