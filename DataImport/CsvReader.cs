using System;

namespace DataImport
{
    public class TextFileReader : IDisposable
    {
        private readonly CsvReader csvReader;
        private readonly LmsTextReaderBase fileReader;
        private const string delimiterTab = "\t";
        private bool _isRecordBad = false;

        public List<LmsError> ErrorList { get; set; } = new List<LmsError>();

        public TextFileReader(string sourceFile)
        {
            fileReader = new FileTextReader(sourceFile);
            csvReader = new CsvReader(fileReader, ConfigReader());
            csvReader.Context.RegisterClassMap<ColumnClassMap>();

        }

        public TextFileReader(Stream sourceFileStream)
        {
            fileReader = new MemoryTextReader(sourceFileStream);
            csvReader = new CsvReader(fileReader, ConfigReader());
            csvReader.Context.RegisterClassMap<ColumnClassMap>();

        }

        private CsvConfiguration ConfigReader()
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                MemberTypes = MemberTypes.Properties,
                HasHeaderRecord = true,
                AllowComments = true,
                Delimiter = delimiterTab,
                Encoding = Encoding.UTF8,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
                WhiteSpaceChars = new[] {' '},
                BadDataFound = x =>
                {
                    LoggingManager.AddMessage(LogLevel.Error, x.Field);
                },
                ReadingExceptionOccurred = e =>
                {
                    _isRecordBad = true;

                    var msg ="Exception: " + e.Exception.InnerException.Message.ToString();

                    ErrorList.Add(
                        new LmsError
                        {
                            ErrorType = ErrorType.Error,
                            Message = msg
                        });

                    LoggingManager.AddMessage(LogLevel.Error, msg);

                    return false;
                }
            };

            return config;

        }

        public IList<CSRecord> ReadCSFile()
        {
            var records = new List<CSRecord>();
            try
            {
                int skipLines = fileReader.GetSkipLines();
                SkipLines(skipLines);
                while (csvReader.Read())
                {
                    var record = csvReader.GetRecord<CSRecord>();
                    if (!_isRecordBad)
                    {
                        records.Add(record);
                    }

                    _isRecordBad = false;
                }
            }
            catch (Exception ex)
            {
                LoggingManager.AddMessage(LogLevel.Error, ex.Message);
                throw new ApplicationException(nameof(ReadCSFile) + ": " + ex.Message, ex);
            }

            return records;
        }

        private string SkipLines(int noLines)
        {
            string ignoredLines = string.Empty;

            for (int i = 0; i < noLines; ++i)
            {
                ignoredLines += fileReader.ReadLine() + Environment.NewLine;
            }

            return ignoredLines;
        }

        private sealed class ColumnClassMap : ClassMap<CSRecord>
        {
            public ColumnClassMap()
            {
                Map(m => m.UserID).Name("UserID");
                Map(m => m.AgencySubelement).Name("Title");
                Map(m => m.TrainingTitle).Name("Date");
                Map(m => m.TrainingType).Name("Description");
                Map(m => m.TrainingTypeCode).Name("Category");
                Map(m => m.TrainingSubType).Name("City");
            }
        }

        #region Dispose
        private bool alreadyDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (alreadyDisposed)
                return;

            if (isDisposing)
            {
                if (fileReader != null)
                {
                    fileReader.Dispose();
                }
            }
            alreadyDisposed = true;
        }

        ~TextFileReader() // the finalizer
        {
            Dispose(false);
        }
        #endregion Dispose
    }
}
