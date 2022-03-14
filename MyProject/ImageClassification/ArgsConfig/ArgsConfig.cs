using NeoCortexApi.Entities;
using Newtonsoft.Json;

namespace AConfig
{
    public class ArgsConfig
    {
        private List<string> listInputWithValue = new() { "-cf", "-if" };
        public HtmConfig htmConfig;
        public string inputFolder;
        public string configFile;
        public string saveFormat;
        public bool ifSaveResult;
        public string saveResultPath;

        /// <summary>
        /// Reading the inputfolder path and htmconfig1.json path
        /// </summary>
        /// <param name="args"></param>
        public ArgsConfig(string[] args)
        {
            configFile = "";
            //Parsing the input cmd
            int index = 0;
            string currentDir = Directory.GetCurrentDirectory();
            while (index < args.Length)
            {
                if (listInputWithValue.Contains(args[index]))
                {
                    switch (args[index])
                    {
                        case "-if":
                            index += 1;
                            inputFolder = Path.Combine(currentDir, args[index]);
                            break;
                        case "-cf":
                            index += 1;
                            configFile = Path.Combine(currentDir, args[index]);
                            break;
                        case "--save-format":
                            index += 1;
                            saveFormat = args[index];
                            break;
                        case "--save-result":
                            ifSaveResult = true;
                            break;
                        case "--save-result-path":
                            index += 1;
                            saveResultPath = args[index];
                            break;
                        default:
                            break;
                    }
                }
                index += 1;
            }
            htmConfig = SetupHtmConfigParameters(configFile);
        }

        /// <summary>
        /// Adding htmconfig1.json to a Dictionary and throw exception if the file is empty
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public HtmConfig SetupHtmConfigParameters(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("File Name is empty ");
            }
            using (StreamReader sw = new StreamReader(fileName))
            {
                var cfgJson = sw.ReadToEnd();
                JsonSerializerSettings settings1 = new JsonSerializerSettings { Formatting = Formatting.Indented };
                HtmConfig htmConfig = JsonConvert.DeserializeObject<HtmConfig>(cfgJson, settings1);
                return htmConfig;
            }
        }
    }
}